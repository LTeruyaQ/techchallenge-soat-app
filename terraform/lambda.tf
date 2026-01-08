data "archive_file" "lambda_zip" {
  type        = "zip"
  source_dir  = "${path.module}/lambda"
  output_path = "${path.module}/lambda.zip"
}

data "aws_iam_role" "lab_role" {
  name = "LabRole"
}

resource "random_string" "jwt_secret" {
  length  = 32
  special = false
}

resource "aws_secretsmanager_secret" "jwt_secret" {
  name = "mecanicaos-jwt-secret"
}

resource "aws_secretsmanager_secret_version" "jwt_secret" {
  secret_id     = aws_secretsmanager_secret.jwt_secret.id
  secret_string = jsonencode({
    secret = random_string.jwt_secret.result
  })
}

resource "aws_lambda_function" "auth_lambda" {
  function_name = "${local.prefix}-auth-lambda"
  role          = data.aws_iam_role.lab_role.arn

  filename         = data.archive_file.lambda_zip.output_path
  source_code_hash = data.archive_file.lambda_zip.output_base64sha256
  handler          = "main.lambda_handler"
  runtime          = "python3.9"
  timeout          = 30

  environment {
    variables = {
      DB_SECRET_ARN   = aws_secretsmanager_secret.db_credentials.arn
      JWT_SECRET_NAME = aws_secretsmanager_secret.jwt_secret.name
    }
  }

  vpc_config {
    subnet_ids         = data.aws_subnets.private.ids
    security_group_ids = [aws_security_group.rds.id]
  }

  depends_on = [
    aws_db_instance.default
  ]
}
