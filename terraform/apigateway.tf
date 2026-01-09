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
# Descoberta do Load Balancer do EKS
# ============================================

# Adiciona um tempo de espera para garantir que o Ingress Controller
# tenha tempo de provisionar o ALB e registrar as tags.
resource "time_sleep" "wait_for_alb" {
  depends_on      = [aws_eks_cluster.eks] # Depende do cluster EKS estar pronto
  create_duration = "2m"                  # Espera 2 minutos
}

# Data source para descobrir o Application Load Balancer (ALB) criado pelo Ingress do EKS
data "aws_lb" "eks_alb" {
  depends_on = [time_sleep.wait_for_alb]

  tags = {
    # Tags aplicadas pelo AWS Load Balancer Controller
    "elbv2.k8s.aws/cluster" = var.project_name
  }
}

# ============================================
# Integrações para cada Controller da API
# ============================================

resource "aws_apigatewayv2_integration" "autenticacao" {
  api_id             = aws_apigatewayv2_api.http_api.id
  integration_type   = "HTTP_PROXY"
  integration_method = "ANY"
  integration_uri    = "https://${data.aws_lb.eks_alb.dns_name}/Autenticacao"
}

resource "aws_apigatewayv2_integration" "cliente" {
  api_id             = aws_apigatewayv2_api.http_api.id
  integration_type   = "HTTP_PROXY"
  integration_method = "ANY"
  integration_uri    = "https://${data.aws_lb.eks_alb.dns_name}/Cliente"
}

resource "aws_apigatewayv2_integration" "estoque" {
  api_id             = aws_apigatewayv2_api.http_api.id
  integration_type   = "HTTP_PROXY"
  integration_method = "ANY"
  integration_uri    = "https://${data.aws_lb.eks_alb.dns_name}/Estoque"
}

resource "aws_apigatewayv2_integration" "ordem_servico" {
  api_id             = aws_apigatewayv2_api.http_api.id
  integration_type   = "HTTP_PROXY"
  integration_method = "ANY"
  integration_uri    = "https://${data.aws_lb.eks_alb.dns_name}/OrdemServico"
}

resource "aws_apigatewayv2_integration" "servico" {
  api_id             = aws_apigatewayv2_api.http_api.id
  integration_type   = "HTTP_PROXY"
  integration_method = "ANY"
  integration_uri    = "https://${data.aws_lb.eks_alb.dns_name}/Servico"
}

resource "aws_apigatewayv2_integration" "usuario" {
  api_id             = aws_apigatewayv2_api.http_api.id
  integration_type   = "HTTP_PROXY"
  integration_method = "ANY"
  integration_uri    = "https://${data.aws_lb.eks_alb.dns_name}/Usuario"
}

resource "aws_apigatewayv2_integration" "veiculo" {
  api_id             = aws_apigatewayv2_api.http_api.id
  integration_type   = "HTTP_PROXY"
  integration_method = "ANY"
  integration_uri    = "https://${data.aws_lb.eks_alb.dns_name}/Veiculo"
}

# ============================================
# Definição Centralizada das Rotas
# ============================================

locals {
  routes = {
    "POST /Autenticacao/Login"                    = { integration = aws_apigatewayv2_integration.autenticacao, protected = false }
    "POST /Autenticacao/Registrar"                = { integration = aws_apigatewayv2_integration.autenticacao, protected = false }
    "GET /Autenticacao/Validar-Token"             = { integration = aws_apigatewayv2_integration.autenticacao, protected = true }
    "GET /Cliente"                                = { integration = aws_apigatewayv2_integration.cliente, protected = true }
    "GET /Cliente/{id}"                           = { integration = aws_apigatewayv2_integration.cliente, protected = true }
    "GET /Cliente/documento/{documento}"          = { integration = aws_apigatewayv2_integration.cliente, protected = true }
    "GET /Cliente/nome/{nome}"                    = { integration = aws_apigatewayv2_integration.cliente, protected = true }
    "POST /Cliente"                               = { integration = aws_apigatewayv2_integration.cliente, protected = true }
    "PUT /Cliente/{id}"                           = { integration = aws_apigatewayv2_integration.cliente, protected = true }
    "DELETE /Cliente/{id}"                        = { integration = aws_apigatewayv2_integration.cliente, protected = true }
    "GET /Estoque"                                = { integration = aws_apigatewayv2_integration.estoque, protected = true }
    "GET /Estoque/{id}"                           = { integration = aws_apigatewayv2_integration.estoque, protected = true }
    "POST /Estoque"                               = { integration = aws_apigatewayv2_integration.estoque, protected = true }
    "PUT /Estoque/{id}"                           = { integration = aws_apigatewayv2_integration.estoque, protected = true }
    "DELETE /Estoque/{id}"                        = { integration = aws_apigatewayv2_integration.estoque, protected = true }
    "GET /OrdemServico"                           = { integration = aws_apigatewayv2_integration.ordem_servico, protected = true }
    "GET /OrdemServico/{id}"                      = { integration = aws_apigatewayv2_integration.ordem_servico, protected = true }
    "GET /OrdemServico/status/{status}"           = { integration = aws_apigatewayv2_integration.ordem_servico, protected = true }
    "POST /OrdemServico"                          = { integration = aws_apigatewayv2_integration.ordem_servico, protected = true }
    "PUT /OrdemServico/{id}"                      = { integration = aws_apigatewayv2_integration.ordem_servico, protected = true }
    "POST /OrdemServico/{ordemServicoId}/insumos"  = { integration = aws_apigatewayv2_integration.ordem_servico, protected = true }
    "PATCH /OrdemServico/{id}/aceitar-orcamento"  = { integration = aws_apigatewayv2_integration.ordem_servico, protected = true }
    "PATCH /OrdemServico/{id}/recusar-orcamento"  = { integration = aws_apigatewayv2_integration.ordem_servico, protected = true }
    "GET /OrdemServico/ativas"                    = { integration = aws_apigatewayv2_integration.ordem_servico, protected = true }
    "GET /Servico"                                = { integration = aws_apigatewayv2_integration.servico, protected = true }
    "GET /Servico/disponiveis"                    = { integration = aws_apigatewayv2_integration.servico, protected = true }
    "GET /Servico/{id}"                           = { integration = aws_apigatewayv2_integration.servico, protected = true }
    "POST /Servico"                               = { integration = aws_apigatewayv2_integration.servico, protected = true }
    "PUT /Servico/{id}"                           = { integration = aws_apigatewayv2_integration.servico, protected = true }
    "DELETE /Servico/{id}"                        = { integration = aws_apigatewayv2_integration.servico, protected = true }
    "GET /Usuario"                                = { integration = aws_apigatewayv2_integration.usuario, protected = true }
    "GET /Usuario/{id}"                           = { integration = aws_apigatewayv2_integration.usuario, protected = true }
    "GET /Usuario/email/{email}"                  = { integration = aws_apigatewayv2_integration.usuario, protected = true }
    "POST /Usuario"                               = { integration = aws_apigatewayv2_integration.usuario, protected = true }
    "PUT /Usuario/{id}"                           = { integration = aws_apigatewayv2_integration.usuario, protected = true }
    "DELETE /Usuario/{id}"                        = { integration = aws_apigatewayv2_integration.usuario, protected = true }
    "POST /Veiculo"                               = { integration = aws_apigatewayv2_integration.veiculo, protected = true }
    "DELETE /Veiculo/{id}"                        = { integration = aws_apigatewayv2_integration.veiculo, protected = true }
    "PUT /Veiculo/{id}"                           = { integration = aws_apigatewayv2_integration.veiculo, protected = true }
    "GET /Veiculo/cliente/{clienteId}"            = { integration = aws_apigatewayv2_integration.veiculo, protected = true }
    "GET /Veiculo/{id}"                           = { integration = aws_apigatewayv2_integration.veiculo, protected = true }
    "GET /Veiculo/placa/{placa}"                  = { integration = aws_apigatewayv2_integration.veiculo, protected = true }
    "GET /Veiculo"                                = { integration = aws_apigatewayv2_integration.veiculo, protected = true }
  }
}

# ============================================
# Criação das Rotas
# ============================================

resource "aws_apigatewayv2_authorizer" "lambda_authorizer" {
  api_id           = aws_apigatewayv2_api.http_api.id
  authorizer_type  = "REQUEST"
  authorizer_uri   = aws_lambda_function.auth_lambda.invoke_arn
  identity_sources = ["$request.header.Authorization"]
  name             = "lambda-authorizer"
  authorizer_payload_format_version = "2.0"
}

resource "aws_apigatewayv2_route" "this" {
  for_each = local.routes

  api_id    = aws_apigatewayv2_api.http_api.id
  route_key = each.key
  target    = "integrations/${each.value.integration.id}"

  authorizer_id      = each.value.protected ? aws_apigatewayv2_authorizer.lambda_authorizer.id : null
  authorization_type = each.value.protected ? "CUSTOM" : "NONE"
}

# ============================================
# Estágio de Deploy
# ============================================

resource "aws_apigatewayv2_stage" "default" {
  api_id      = aws_apigatewayv2_api.http_api.id
  name        = "$default"
  auto_deploy = true
}

# ============================================
# Permissões
# ============================================

# Permite que o API Gateway invoque a função Lambda (para o autorizador)
resource "aws_lambda_permission" "api_gateway_invoke_lambda" {
  statement_id  = "AllowAPIGatewayInvoke"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.auth_lambda.function_name
  principal     = "apigateway.amazonaws.com"

  # A permissão deve cobrir todas as rotas que usarão o autorizador
  source_arn = "${aws_apigatewayv2_api.http_api.execution_arn}/*/*"
}
