data "archive_file" "lambda_zip" {
  type        = "zip"
  source_dir  = "${path.module}/lambda"
  output_path = "${path.module}/lambda.zip"
}

resource "random_string" "jwt_secret" {
  length  = 32
  special = false
}

# ============================================
# IAM Role para a Lambda
# Gerencia a criação ou a reutilização de uma IAM Role
# ============================================

# Data source para ler a role que já existe (se a flag for verdadeira)
data "aws_iam_role" "existing_lambda_role" {
  count = var.use_existing_lambda_role == "true" ? 1 : 0
  name  = var.lambda_role_name
}

# Recurso para criar a role somente se a flag for falsa
resource "aws_iam_role" "lambda_exec" {
  count = var.use_existing_lambda_role == "true" ? 0 : 1

  name = "mecanicaos-lambda-exec-role"
  assume_role_policy = jsonencode({
    Version   = "2012-10-17",
    Statement = [{
      Action    = "sts:AssumeRole",
      Effect    = "Allow",
      Principal = {
        Service = "lambda.amazonaws.com"
      }
    }]
  })

}

resource "aws_iam_role_policy_attachment" "lambda_basic_execution" {
  count = var.use_existing_lambda_role == "true" ? 0 : 1

  role       = aws_iam_role.lambda_exec[0].name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

# Local para unificar a referência ao ARN da role
locals {
  lambda_role_arn = var.use_existing_lambda_role == "true" ? data.aws_iam_role.existing_lambda_role[0].arn : aws_iam_role.lambda_exec[0].arn
}


# ============================================
# AWS Lambda Function
# ============================================

resource "aws_lambda_function" "auth_lambda" {
  function_name = "mecanicaos-auth-lambda"
  role          = local.lambda_role_arn

  # ... (resto da configuração da Lambda permanece o mesmo, usando random_password, etc.)

  handler       = "main.lambda_handler"
  runtime       = "python3.9"
  timeout       = 30
  filename      = data.archive_file.lambda_zip.output_path
  source_code_hash = data.archive_file.lambda_zip.output_base64sha256

  environment {
    variables = {
      DB_HOST        = aws_db_instance.default.address
      DB_NAME        = aws_db_instance.default.db_name
      DB_USER        = "mecanicaosadmin"
      DB_PASSWORD    = random_password.db_password.result
      JWT_SECRET_KEY = random_string.jwt_secret.result
    }
  }

  vpc_config {
    subnet_ids         = local.private_subnet_ids
    security_group_ids = [aws_security_group.rds.id]
  }
}
