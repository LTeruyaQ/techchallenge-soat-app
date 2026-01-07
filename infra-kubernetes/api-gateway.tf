# ============================================
# API Gateway
# ============================================

# --- Data Sources para Descoberta Robusta do NLB ---

# Descobre o Network Load Balancer criado pelo AWS Load Balancer Controller.
# A descoberta é feita por tags que são adicionadas automaticamente pelo controller.
data "aws_lb" "eks" {
  tags = {
    "elbv2.k8s.aws/cluster"       = var.eks_cluster_name
    "service.k8s.aws/stack"       = "mecanicaos/mecanicaos-service"
    "service.k8s.aws/resource"    = "LoadBalancer"
  }
}

# Descobre o Listener associado ao NLB.
data "aws_lb_listener" "eks" {
  load_balancer_arn = data.aws_lb.eks.arn
  port              = 80
}

# --- Recursos do API Gateway ---

# Cria a API Gateway (HTTP API para menor custo)
resource "aws_api_gateway_v2_api" "main" {
  name          = "${var.project_name}-api"
  protocol_type = "HTTP"
  description   = "API Gateway para a solução MecanicaOS"
  tags          = { Name = "${var.project_name}-api-gateway", Project = "MecanicaOS" }
}

# --- Integração e Rota da Lambda de Autenticação ---
resource "aws_api_gateway_v2_integration" "auth_lambda" {
  api_id                 = aws_api_gateway_v2_api.main.id
  integration_type       = "AWS_PROXY"
  integration_uri        = aws_lambda_function.auth.invoke_arn
  payload_format_version = "2.0"
}

resource "aws_api_gateway_v2_route" "auth" {
  api_id    = aws_api_gateway_v2_api.main.id
  route_key = "POST /auth"
  target    = "integrations/${aws_api_gateway_v2_integration.auth_lambda.id}"
}

# --- Integração e Rota da API Principal no EKS ---
resource "aws_api_gateway_v2_vpc_link" "eks" {
  name               = "${var.project_name}-eks-vpc-link"
  security_group_ids = [data.aws_security_group.eks_nodes.id]
  subnet_ids         = data.aws_subnets.private.ids
  tags               = { Name = "${var.project_name}-eks-vpc-link", Project = "MecanicaOS" }
}

resource "aws_api_gateway_v2_integration" "eks_service" {
  api_id           = aws_api_gateway_v2_api.main.id
  integration_type = "HTTP_PROXY"
  integration_uri  = data.aws_lb_listener.eks.arn # Usa o ARN do listener descoberto
  connection_type  = "VPC_LINK"
  connection_id    = aws_api_gateway_v2_vpc_link.eks.id
}

resource "aws_api_gateway_v2_route" "eks_proxy" {
  api_id    = aws_api_gateway_v2_api.main.id
  route_key = "ANY /{proxy+}"
  target    = "integrations/${aws_api_gateway_v2_integration.eks_service.id}"
}

# --- Configurações Gerais da API ---
resource "aws_api_gateway_v2_stage" "default" {
  api_id      = aws_api_gateway_v2_api.main.id
  name        = "$default"
  auto_deploy = true
  access_log_settings {
    destination_arn = aws_cloudwatch_log_group.api_gateway.arn
    format          = jsonencode({ requestId = "$context.requestId", sourceIp = "$context.identity.sourceIp", requestTime = "$context.requestTime", httpMethod = "$context.httpMethod", resourcePath = "$context.resourcePath", status = "$context.status", responseLength = "$context.responseLength", integrationErrorMessage = "$context.integrationErrorMessage" })
  }
  tags = { Name = "${var.project_name}-api-stage", Project = "MecanicaOS" }
}

resource "aws_cloudwatch_log_group" "api_gateway" {
  name              = "/aws/api-gateway/${var.project_name}-api"
  retention_in_days = 7
  tags              = { Name = "${var.project_name}-api-log-group", Project = "MecanicaOS" }
}
