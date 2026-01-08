data "archive_file" "lambda_zip" {
  type        = "zip"
  source_dir  = "${path.module}/lambda"
  output_path = "${path.module}/lambda.zip"
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

resource "aws_iam_policy" "lambda_secrets_access" {
  name        = "${local.prefix}-lambda-secrets-access-policy"
  description = "Policy to allow Lambda to access the RDS and JWT secrets"

  policy = jsonencode({
    Version   = "2012-10-17"
    Statement = [
      {
        Action   = "secretsmanager:GetSecretValue"
        Effect   = "Allow"
        Resource = [
          aws_secretsmanager_secret.db_credentials.arn,
          aws_secretsmanager_secret.jwt_secret.arn
        ]
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "lambda_secrets_access" {
  role       = data.aws_iam_role.lab_role.name
  policy_arn = aws_iam_policy.lambda_secrets_access.arn
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
    subnet_ids         = aws_subnet.private[*].id
    security_group_ids = [aws_security_group.rds.id]
  }

  depends_on = [
    aws_db_instance.default
  ]
}
