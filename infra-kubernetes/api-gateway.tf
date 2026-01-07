# ============================================
# API Gateway
# ============================================

# Descobre o serviço do Kubernetes para obter o hostname do Load Balancer
data "kubernetes_service" "api" {
  metadata {
    name      = "mecanicaos-service"
    namespace = "mecanicaos"
  }
}

# Cria a API Gateway (HTTP API para menor custo)
resource "aws_api_gateway_v2_api" "main" {
  name          = "${var.project_name}-api"
  protocol_type = "HTTP"
  description   = "API Gateway para a solução MecanicaOS"
  tags = { Name = "${var.project_name}-api-gateway", Project = "MecanicaOS" }
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
  security_group_ids = [data.aws_security_group.eks_nodes.id] # Reutiliza o SG dos nós
  subnet_ids         = data.aws_subnets.private.ids
  tags = { Name = "${var.project_name}-eks-vpc-link", Project = "MecanicaOS" }
}

resource "aws_api_gateway_v2_integration" "eks_service" {
  api_id           = aws_api_gateway_v2_api.main.id
  integration_type = "HTTP_PROXY"
  # O URI aponta para o listener do NLB criado pelo serviço Kubernetes
  integration_uri = aws_lb_listener.eks.arn
  connection_type  = "VPC_LINK"
  connection_id    = aws_api_gateway_v2_vpc_link.eks.id
}

# Rota "catch-all" para a API no EKS
resource "aws_api_gateway_v2_route" "eks_proxy" {
  api_id    = aws_api_gateway_v2_api.main.id
  route_key = "ANY /{proxy+}" # Encaminha qualquer método e caminho
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

# Data source para encontrar o Load Balancer criado pelo Kubernetes
data "aws_lb" "eks" {
  name = split("-", data.kubernetes_service.api.status[0].load_balancer[0].ingress[0].hostname)[0]
}

# Recurso para o listener do Load Balancer
resource "aws_lb_listener" "eks" {
  load_balancer_arn = data.aws_lb.eks.arn
  port              = "80"
  protocol          = "HTTP"
  default_action {
    type             = "forward"
    target_group_arn = data.aws_lb_target_group.eks.arn
  }
}

# Data source para encontrar o Target Group
data "aws_lb_target_group" "eks" {
  name = "${data.aws_lb.eks.name}-targetgroup"
}
