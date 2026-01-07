# lambda_authorizer/main.tf

# 1. Compactar o código da Lambda
data "archive_file" "lambda_zip" {
  type        = "zip"
  source_dir  = "${path.module}/package"
  output_path = "${path.module}/lambda.zip"
  # O script de deploy irá popular o diretório 'package'
}

# 2. IAM Role para a Lambda
resource "aws_iam_role" "lambda_exec_role" {
  name = "lambda-authorizer-exec-role"

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

# 3. Política básica de execução da Lambda (para logs)
resource "aws_iam_role_policy_attachment" "basic_execution" {
  role       = aws_iam_role.lambda_exec_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

# 4. Função Lambda
resource "aws_lambda_function" "authorizer" {
  function_name = "api-gateway-authorizer"
  handler       = "lambda_handler.lambda_handler"
  runtime       = "python3.9"
  role          = aws_iam_role.lambda_exec_role.arn

  filename         = data.archive_file.lambda_zip.output_path
  source_code_hash = data.archive_file.lambda_zip.output_base64sha256

  environment {
    variables = {
      JWT_SECRET = var.jwt_secret
    }
  }
}
