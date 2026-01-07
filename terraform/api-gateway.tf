# ============================================
# API Gateway & Lambda Authorizer
# ============================================

# This data source waits for the k8s service to be deployed and gets its hostname
data "kubernetes_service" "api_service" {
  metadata {
    name      = "mecanicaos-api-service"
    namespace = "mecanicaos"
  }
  depends_on = [kubectl_manifest.app]
}

resource "aws_apigatewayv2_api" "http_api" {
  name          = "${var.project_name}-http-api"
  protocol_type = "HTTP"
}

# IAM Role for Lambda
resource "aws_iam_role" "lambda_exec" {
  name = "${var.project_name}-lambda-exec-role"

  assume_role_policy = jsonencode({
    Version   = "2012-10-17"
    Statement = [{
      Action    = "sts:AssumeRole"
      Effect    = "Allow"
      Principal = {
        Service = "lambda.amazonaws.com"
      }
    }]
  })
}

# Base execution role policy
resource "aws_iam_role_policy_attachment" "lambda_basic_execution" {
  role       = aws_iam_role.lambda_exec.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

# Policy to access VPC for RDS connection
resource "aws_iam_role_policy_attachment" "lambda_vpc_access" {
  role       = aws_iam_role.lambda_exec.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaVPCAccessExecutionRole"
}


# Policy to allow reading secrets
resource "aws_iam_role_policy" "lambda_secrets_access" {
  name = "${var.project_name}-lambda-secrets-policy"
  role = aws_iam_role.lambda_exec.id
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action   = "secretsmanager:GetSecretValue"
        Effect   = "Allow"
        Resource = [
          aws_secretsmanager_secret.rds_credentials.arn,
          aws_secretsmanager_secret.jwt_key.arn
        ]
      },
    ]
  })
}

# Security group for the Lambda to access RDS
resource "aws_security_group" "lambda" {
  name        = "${var.project_name}-lambda-sg"
  description = "Security group for authorizer lambda"
  vpc_id      = aws_vpc.main.id

  # Allow all outbound traffic
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

# Allow Lambda to connect to RDS on PostgreSQL port
resource "aws_security_group_rule" "lambda_to_rds" {
  type                     = "ingress"
  from_port                = 5432
  to_port                  = 5432
  protocol                 = "tcp"
  source_security_group_id = aws_security_group.lambda.id
  security_group_id        = aws_security_group.eks_nodes.id # RDS uses the same SG as nodes
}


# Authorizer Lambda function
resource "aws_lambda_function" "authorizer" {
  filename      = "lambda_authorizer.zip" # Placeholder, will be created by script
  function_name = "${var.project_name}-authorizer"
  role          = aws_iam_role.lambda_exec.arn
  handler       = "authorizer.handler"
  runtime       = "python3.9"
  source_code_hash = filebase64sha256("lambda_authorizer.zip")

  # Required for RDS access
  vpc_config {
    subnet_ids         = aws_subnet.private[*].id
    security_group_ids = [aws_security_group.lambda.id]
  }

  environment {
    variables = {
      RDS_SECRET_ARN = aws_secretsmanager_secret.rds_credentials.arn
      JWT_SECRET_ARN = aws_secretsmanager_secret.jwt_key.arn
    }
  }
}

# API Gateway Authorizer
resource "aws_apigatewayv2_authorizer" "jwt_authorizer" {
  api_id           = aws_apigatewayv2_api.http_api.id
  authorizer_type  = "REQUEST"
  authorizer_uri   = aws_lambda_function.authorizer.invoke_arn
  name             = "jwt-authorizer"
  identity_sources = ["$request.header.Authorization"]
  enable_simple_responses = true
}

# API Gateway Integration with EKS Load Balancer
resource "aws_apigatewayv2_integration" "api_integration" {
  api_id             = aws_apigatewayv2_api.http_api.id
  integration_type   = "HTTP_PROXY"
  integration_uri    = "http://${data.kubernetes_service.api_service.status[0].load_balancer.ingress[0].hostname}"
  integration_method = "ANY"
}

# Route for all requests to the EKS service, protected by the authorizer
resource "aws_apigatewayv2_route" "api_proxy_route" {
  api_id    = aws_apigatewayv2_api.http_api.id
  route_key = "ANY /{proxy+}"
  target    = "integrations/${aws_apigatewayv2_integration.api_integration.id}"

  # Protect the route
  authorization_type = "CUSTOM"
  authorizer_id      = aws_apigatewayv2_authorizer.jwt_authorizer.id
}

# ============================================
# Public Authentication Route
# ============================================

# Integration for the public /auth route, pointing directly to the Lambda function
resource "aws_apigatewayv2_integration" "auth_lambda_integration" {
  api_id           = aws_apigatewayv2_api.http_api.id
  integration_type = "AWS_PROXY"
  integration_uri  = aws_lambda_function.authorizer.invoke_arn
}

# Public route for POST /auth to generate a token
resource "aws_apigatewayv2_route" "auth_route" {
  api_id    = aws_apigatewayv2_api.http_api.id
  route_key = "POST /auth"
  target    = "integrations/${aws_apigatewayv2_integration.auth_lambda_integration.id}"
}

# ============================================
# Permissions and Stage
# ============================================

# Permission for API Gateway to invoke the Lambda function for authorization
resource "aws_lambda_permission" "api_gateway_authorizer" {
  statement_id  = "AllowAPIGatewayInvoke"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.authorizer.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_apigatewayv2_api.http_api.execution_arn}/authorizers/${aws_apigatewayv2_authorizer.jwt_authorizer.id}"
}

# Permission for API Gateway to invoke the Lambda function for the public /auth route
resource "aws_lambda_permission" "api_gateway_auth_route" {
  statement_id  = "AllowAPIGatewayInvokeAuthRoute"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.authorizer.function_name
  principal     = "apigateway.amazonaws.com"
  source_arn    = "${aws_apigatewayv2_api.http_api.execution_arn}/routes/${aws_apigatewayv2_route.auth_route.id}"
}


# Default stage for auto-deployment
resource "aws_apigatewayv2_stage" "default" {
  api_id      = aws_apigatewayv2_api.http_api.id
  name        = "$default"
  auto_deploy = true
}
