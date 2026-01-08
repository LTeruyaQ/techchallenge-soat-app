data "archive_file" "lambda_zip" {
  type        = "zip"
  source_dir  = "${path.module}/lambda"
  output_path = "${path.module}/lambda.zip"
}

resource "random_string" "jwt_secret" {
  length  = 32
  special = false
}

data "aws_iam_role" "lab_role" {
  name = "LabRole"
}

resource "aws_lambda_function" "auth_lambda" {
  function_name = "mecanicaos-auth-lambda"
  role          = data.aws_iam_role.lab_role.arn

  handler          = "main.lambda_handler"
  runtime          = "python3.9"
  timeout          = 30
  filename         = data.archive_file.lambda_zip.output_path
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
    subnet_ids         = aws_subnet.private[*].id
    security_group_ids = [aws_security_group.rds.id]
  }
}
