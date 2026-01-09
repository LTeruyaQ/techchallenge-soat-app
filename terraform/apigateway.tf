# ============================================
# API Gateway - HTTP API
# ============================================

resource "aws_apigatewayv2_api" "http_api" {
  name          = "${var.project_name}-http-api"
  protocol_type = "HTTP"
  description   = "API Gateway for MecanicaOS"

  # Define CORS para permitir acesso do Swagger UI e front-ends
  cors_configuration {
    allow_origins = ["*"]
    allow_methods = ["*"]
    allow_headers = ["*"]
  }

  tags = {
    Name = "${var.project_name}-http-api"
  }
}

# ============================================
# VPC Link para conectar ao EKS
# ============================================

resource "aws_apigatewayv2_vpc_link" "eks" {
  name        = "${var.project_name}-eks-vpc-link"
  subnet_ids  = local.public_subnet_ids # O ALB do EKS estará nas subnets públicas
  security_group_ids = [
    aws_security_group.eks_nodes.id # Permite que o GW se comunique com os nodes/ALB
  ]

  tags = {
    Name = "${var.project_name}-eks-vpc-link"
  }
}

# ============================================
# Integrações
# ============================================

# Integração com a Lambda de autenticação
resource "aws_apigatewayv2_integration" "lambda_auth" {
  api_id                 = aws_apigatewayv2_api.http_api.id
  integration_type       = "AWS_PROXY"
  integration_uri        = aws_lambda_function.auth_lambda.invoke_arn
  payload_format_version = "2.0"
}

# Integração com o Load Balancer do EKS
resource "aws_apigatewayv2_integration" "eks_proxy" {
  api_id                 = aws_apigatewayv2_api.http_api.id
  integration_type       = "HTTP_PROXY"
  integration_method     = "ANY"
  connection_type        = "VPC_LINK"
  connection_id          = aws_apigatewayv2_vpc_link.eks.id
  integration_uri        = module.eks.cluster_endpoint # O endpoint do cluster EKS
  payload_format_version = "1.0"
}

# ============================================
# Rotas
# ============================================

# Rota para a autenticação via Lambda
resource "aws_apigatewayv2_route" "auth" {
  api_id    = aws_apigatewayv2_api.http_api.id
  route_key = "POST /auth"
  target    = "integrations/${aws_apigatewayv2_integration.lambda_auth.id}"
}

# Rota padrão que envia todo o resto para o EKS
resource "aws_apigatewayv2_route" "default" {
  api_id    = aws_apigatewayv2_api.http_api.id
  route_key = "$default"
  target    = "integrations/${aws_apigatewayv2_integration.eks_proxy.id}"
}

# ============================================
# Estágio de Deploy
# ============================================

resource "aws_apigatewayv2_stage" "default" {
  api_id      = aws_apigatewayv2_api.http_api.id
  name        = "$default"
  auto_deploy = true

  # Habilita logging (opcional, mas recomendado)
  default_route_settings {
    throttling_burst_limit = 50
    throttling_rate_limit  = 100
  }
}

# ============================================
# Permissões
# ============================================

# Permite que o API Gateway invoque a função Lambda
resource "aws_lambda_permission" "api_gateway_invoke_lambda" {
  statement_id  = "AllowAPIGatewayInvoke"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.auth_lambda.function_name
  principal     = "apigateway.amazonaws.com"

  source_arn = "${aws_apigatewayv2_api.http_api.execution_arn}/*/*"
}
