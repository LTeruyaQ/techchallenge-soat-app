# ============================================
# Lambda - Função de Autenticação
# ============================================

# Desacoplamento: Busca recursos do RDS (que estariam em outro repositório)
# usando data sources, em vez de referências diretas.
data "aws_db_instance" "main" {
  db_instance_identifier = "${var.project_name}-db"
}

data "aws_security_group" "rds" {
  name = "${var.project_name}-rds-sg"
}

# Desacoplamento: Em um cenário real, o CI/CD da Lambda publicaria o zip
# em um S3. O Terraform então o leria de lá. Para simular isso,
# esperamos que o zip exista em um local pré-definido.
data "archive_file" "lambda_auth_zip" {
  type        = "zip"
  source_dir  = "/tmp/lambda_auth_source" # Diretório temporário que o CI/CD criaria
  output_path = "/tmp/lambda_auth.zip"
}

# Role IAM para a função Lambda
resource "aws_iam_role" "lambda_auth" {
  name = "${var.project_name}-lambda-auth-role"
  assume_role_policy = jsonencode({
    Version = "2012-10-17",
    Statement = [{
      Action    = "sts:AssumeRole",
      Effect    = "Allow",
      Principal = { Service = "lambda.amazonaws.com" }
    }]
  })
  managed_policy_arns = ["arn:aws:iam::aws:policy/service-role/AWSLambdaVPCAccessExecutionRole"]
  tags = { Name = "${var.project_name}-lambda-auth-role", Project = "MecanicaOS" }
}

# Função Lambda de autenticação
resource "aws_lambda_function" "auth" {
  function_name    = "${var.project_name}-auth"
  role             = aws_iam_role.lambda_auth.arn
  handler          = "index.handler"
  runtime          = "nodejs18.x"
  memory_size      = 128
  timeout          = 5
  filename         = data.archive_file.lambda_auth_zip.output_path
  source_code_hash = data.archive_file.lambda_auth_zip.output_base64sha256

  vpc_config {
    subnet_ids         = data.aws_subnets.private.ids
    security_group_ids = [aws_security_group.lambda.id]
  }

  environment {
    variables = {
      DB_HOST     = data.aws_db_instance.main.address
      DB_USER     = var.db_user
      DB_PASSWORD = var.db_password
      DB_NAME     = var.db_name
      JWT_SECRET  = var.jwt_secret
    }
  }
  tags = { Name = "${var.project_name}-auth-lambda", Project = "MecanicaOS" }
}

# Security Group para a função Lambda
resource "aws_security_group" "lambda" {
  name        = "${var.project_name}-lambda-sg"
  description = "Controle de acesso para a função Lambda de autenticação"
  vpc_id      = data.aws_vpc.main.id
  tags        = { Name = "${var.project_name}-lambda-sg", Project = "MecanicaOS" }
}

# Permite que a Lambda se conecte ao RDS
resource "aws_security_group_rule" "lambda_to_rds" {
  type                     = "ingress"
  from_port                = 5432
  to_port                  = 5432
  protocol                 = "tcp"
  security_group_id        = data.aws_security_group.rds.id # Usa o data source
  source_security_group_id = aws_security_group.lambda.id
  description              = "Permite a conexão da Lambda de autenticação ao RDS"
}

# Permissão para o API Gateway invocar a Lambda
resource "aws_lambda_permission" "api_gateway_invoke" {
  statement_id  = "AllowAPIGatewayInvoke"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.auth.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_api_gateway_v2_api.main.execution_arn}/*/*"
}
