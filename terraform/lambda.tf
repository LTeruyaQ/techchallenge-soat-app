# ============================================
# IAM Role para a Lambda
# ============================================

resource "aws_iam_role" "lambda_auth" {
  name = "${var.project_name}-lambda-auth-role"

  assume_role_policy = jsonencode({
    Version   = "2012-10-17",
    Statement = [
      {
        Action    = "sts:AssumeRole",
        Effect    = "Allow",
        Principal = {
          Service = "lambda.amazonaws.com"
        }
      }
    ]
  })

  tags = {
    Name = "${var.project_name}-lambda-auth-role"
  }
}

# Adiciona a política básica de execução da Lambda
resource "aws_iam_role_policy_attachment" "lambda_basic_execution" {
  role       = aws_iam_role.lambda_auth.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

# Adiciona a política para acesso à VPC
resource "aws_iam_role_policy_attachment" "lambda_vpc_access" {
  role       = aws_iam_role.lambda_auth.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaVPCAccessExecutionRole"
}

# ============================================
# Empacotamento do Código da Lambda
# ============================================

data "archive_file" "lambda_zip" {
  type        = "zip"
  source_dir  = "${path.module}/lambda/package"
  output_path = "${path.module}/lambda.zip"
}

# ============================================
# Função Lambda de Autenticação
# ============================================

resource "aws_lambda_function" "auth_lambda" {
  function_name    = "${var.project_name}-auth-lambda"
  role             = aws_iam_role.lambda_auth.arn
  handler          = "main.lambda_handler"
  runtime          = "python3.9"
  timeout          = 30
  filename         = data.archive_file.lambda_zip.output_path
  source_code_hash = data.archive_file.lambda_zip.output_base64sha256

  environment {
    variables = {
      DB_HOST        = aws_db_instance.default.address
      DB_PORT        = aws_db_instance.default.port
      DB_NAME        = var.db_name
      DB_USER        = var.db_username
      DB_PASSWORD    = local.db_password
      JWT_SECRET_KEY = local.jwt_secret_key
      JWT_ISSUER     = var.jwt_issuer
      JWT_AUDIENCE   = var.jwt_audience
    }
  }

  vpc_config {
    subnet_ids         = local.private_subnet_ids
    security_group_ids = [aws_security_group.lambda.id]
  }

  tags = {
    Name = "${var.project_name}-auth-lambda"
  }
}
