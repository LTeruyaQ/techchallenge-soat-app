# ===================================================================
# Recurso: IAM Role para a Lambda
# Descrição: Define a role e as permissões que a Lambda de autenticação usará.
# ===================================================================

resource "aws_iam_role" "lambda_auth_role" {
  name = "${var.project_name}-lambda-auth-role"

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

# Anexa a política gerenciada pela AWS para acesso à VPC
resource "aws_iam_role_policy_attachment" "lambda_vpc_access" {
  role       = aws_iam_role.lambda_auth_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaVPCAccessExecutionRole"
}

# ===================================================================
# Recurso: Pacote da Lambda
# Descrição: Empacota o código-fonte da Lambda e suas dependências.
# ===================================================================

data "archive_file" "lambda_auth_zip" {
  type        = "zip"
  source_dir  = "../lambda/authenticator"
  output_path = "${path.module}/lambda_auth.zip"
}

# ===================================================================
# Recurso: Função Lambda
# Descrição: Cria a função Lambda de autenticação.
# ===================================================================

resource "aws_lambda_function" "authenticator" {
  filename      = data.archive_file.lambda_auth_zip.output_path
  function_name = "${var.project_name}-authenticator"
  role          = aws_iam_role.lambda_auth_role.arn
  handler       = "main.lambda_handler"
  runtime       = "python3.9"
  source_code_hash = data.archive_file.lambda_auth_zip.output_base64sha256

  environment {
    variables = {
      DB_HOST           = aws_db_instance.postgres_db.address
      DB_PORT           = aws_db_instance.postgres_db.port
      DB_NAME           = aws_db_instance.postgres_db.db_name
      DB_USER           = aws_db_instance.postgres_db.username
      DB_PASSWORD       = random_password.db_master_password.result
      JWT_SECRET        = var.jwt_secret_key
      JWT_EXPIRY_MINUTES = var.jwt_expiry_minutes
    }
  }

  vpc_config {
    subnet_ids         = aws_subnet.private[*].id
    security_group_ids = [aws_security_group.rds.id]
  }

  depends_on = [
    aws_db_instance.postgres_db
  ]
}
