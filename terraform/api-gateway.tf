# ============================================
# API Gateway (HTTP API)
# ============================================

variable "alb_dns_name" {
  description = "The DNS name of the Application Load Balancer"
  type        = string
}

resource "aws_apigatewayv2_api" "http_api" {
  name          = "${var.project_name}-http-api"
  protocol_type = "HTTP"
}

resource "aws_apigatewayv2_stage" "default" {
  api_id      = aws_apigatewayv2_api.http_api.id
  name        = "$default"
  auto_deploy = true
}

resource "aws_apigatewayv2_integration" "service_integration" {
  api_id           = aws_apigatewayv2_api.http_api.id
  integration_type = "HTTP_PROXY"
  integration_uri  = "http://${var.alb_dns_name}"
  payload_format_version = "1.0"
  integration_method = "ANY"
}


resource "aws_apigatewayv2_route" "proxy_route" {
  api_id    = aws_apigatewayv2_api.http_api.id
  route_key = "ANY /{proxy+}"
  target    = "integrations/${aws_apigatewayv2_integration.service_integration.id}"
}

# ============================================
# API Gateway Authorizer (Lambda)
# ============================================

resource "aws_apigatewayv2_authorizer" "lambda_authorizer" {
  api_id           = aws_apigatewayv2_api.http_api.id
  authorizer_type  = "REQUEST"
  authorizer_uri   = aws_lambda_function.authorizer.invoke_arn
  name             = "${var.project_name}-lambda-authorizer"
  identity_sources = ["$request.header.Authorization"]
}
