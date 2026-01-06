# ===================================================================
# Recurso: API Gateway (HTTP API)
# Descrição: Ponto de entrada para todas as requisições, com autorizador.
# ===================================================================

resource "aws_apigatewayv2_api" "http_api" {
  name          = "${var.project_name}-http-api"
  protocol_type = "HTTP"
}

# ===================================================================
# Recurso: Autorizador Lambda
# Descrição: Anexa a função Lambda como autorizador ao API Gateway.
# ===================================================================

resource "aws_apigatewayv2_authorizer" "lambda_authorizer" {
  api_id           = aws_apigatewayv2_api.http_api.id
  authorizer_type  = "REQUEST"
  authorizer_uri   = aws_lambda_function.authenticator.invoke_arn
  identity_sources = ["$request.header.x-cpf"]
  name             = "lambda-authorizer"
}

# Permissão para o API Gateway invocar a função Lambda
resource "aws_lambda_permission" "api_gw_invoke_lambda" {
  statement_id  = "AllowAPIGatewayInvoke"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.authenticator.function_name
  principal     = "apigateway.amazonaws.com"

  source_arn = "${aws_apigatewayv2_api.http_api.execution_arn}/authorizers/${aws_apigatewayv2_authorizer.lambda_authorizer.id}"
}

# ===================================================================
# Recurso: Integração do API Gateway com o Load Balancer do EKS
# Descrição: Encaminha as requisições para o serviço rodando no EKS.
# ===================================================================

# Data source para obter o Load Balancer do serviço Kubernetes
data "kubernetes_service" "api_service" {
  metadata {
    name      = "mecanicaos-api"
    namespace = "mecanicaos"
  }
}

resource "aws_apigatewayv2_integration" "eks_integration" {
  api_id             = aws_apigatewayv2_api.http_api.id
  integration_type   = "HTTP_PROXY"
  integration_method = "ANY"
  integration_uri    = "http://${data.kubernetes_service.api_service.status.0.load_balancer.0.ingress.0.hostname}"
  payload_format_version = "1.0"
}

# ===================================================================
# Recurso: Rotas do API Gateway
# Descrição: Define as rotas públicas e protegidas.
# ===================================================================

# Rota protegida para o proxy principal
resource "aws_apigatewayv2_route" "proxy_protected" {
  api_id    = aws_apigatewayv2_api.http_api.id
  route_key = "ANY /{proxy+}"
  target    = "integrations/${aws_apigatewayv2_integration.eks_integration.id}"
  authorizer_id = aws_apigatewayv2_authorizer.lambda_authorizer.id
  authorization_type = "CUSTOM"
}

# Rota pública para o Swagger
resource "aws_apigatewayv2_route" "swagger_public" {
  api_id    = aws_apigatewayv2_api.http_api.id
  route_key = "GET /docs/{proxy+}"
  target    = "integrations/${aws_apigatewayv2_integration.eks_integration.id}"
}

# ===================================================================
# Recurso: Stage do API Gateway
# Descrição: Implanta a configuração da API em um stage.
# ===================================================================

resource "aws_apigatewayv2_stage" "default" {
  api_id      = aws_apigatewayv2_api.http_api.id
  name        = "$default"
  auto_deploy = true
}
