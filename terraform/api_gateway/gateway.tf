# 1. API Gateway
resource "aws_apigatewayv2_api" "http_api" {
  name          = "mecanicaos-api"
  protocol_type = "HTTP"
}

# 2. Lambda Authorizer
resource "aws_apigatewayv2_authorizer" "lambda_authorizer" {
  api_id           = aws_apigatewayv2_api.http_api.id
  authorizer_type  = "REQUEST"
  authorizer_uri   = var.lambda_authorizer_invoke_arn
  name             = "jwt-authorizer"
  identity_sources = ["$request.header.Authorization"]
}

# 3. Integração com o EKS
resource "aws_apigatewayv2_integration" "eks_backend" {
  api_id             = aws_apigatewayv2_api.http_api.id
  integration_type   = "HTTP_PROXY"
  integration_uri    = "http://${var.eks_service_url}"
  integration_method = "ANY"
}

# 4. Rota pública para Login
resource "aws_apigatewayv2_route" "login_route" {
  api_id    = aws_apigatewayv2_api.http_api.id
  route_key = "POST /api/clienteautenticacao/login-cliente"
  target    = "integrations/${aws_apigatewayv2_integration.eks_backend.id}"
}

# 5. Rota protegida (Proxy para o resto da API)
resource "aws_apigatewayv2_route" "proxy_route" {
  api_id    = aws_apigatewayv2_api.http_api.id
  route_key = "ANY /{proxy+}"
  target    = "integrations/${aws_apigatewayv2_integration.eks_backend.id}"
  authorization_type = "CUSTOM"
  authorizer_id      = aws_apigatewayv2_authorizer.lambda_authorizer.id
}

# 6. Permissão para a API Gateway invocar a Lambda
resource "aws_lambda_permission" "api_gw_invoke_lambda" {
  statement_id  = "AllowAPIGatewayInvoke"
  action        = "lambda:InvokeFunction"
  function_name = var.lambda_authorizer_arn
  principal     = "apigateway.amazonaws.com"

  source_arn = "${aws_apigatewayv2_api.http_api.execution_arn}/*"
}

# 7. Estágio de Deploy
resource "aws_apigatewayv2_stage" "default_stage" {
  api_id      = aws_apigatewayv2_api.http_api.id
  name        = "$default"
  auto_deploy = true
}
