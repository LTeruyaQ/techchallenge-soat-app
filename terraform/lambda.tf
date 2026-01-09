
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
  role             = local.lab_role_arn
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
