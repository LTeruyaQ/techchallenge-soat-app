# ============================================
# Kubernetes Resources from k8s/ directory
# ============================================
#
# Este arquivo aplica os manifestos do diretório k8s/
# usando templatefile() para substituir variáveis
#
# ============================================

# Variáveis locais para os templates
locals {
  k8s_template_vars = {
    replicas                    = var.replicas
    docker_image                = var.docker_image_url
    environment                 = var.environment
    db_connection_string        = local.db_connection_string
    jwt_secret_key              = local.jwt_secret.secret_key
    jwt_issuer                  = var.jwt_issuer
    jwt_audience                = var.jwt_audience
    jwt_expiry_minutes          = var.jwt_expiry_minutes
    otel_service_name           = var.otel_service_name
    otel_exporter_otlp_endpoint = var.otel_exporter_otlp_endpoint
  }
}

# Manifest unificado para a aplicação
resource "kubectl_manifest" "app" {
  depends_on = [
    aws_eks_cluster.eks,
    aws_eks_node_group.nodes,
    aws_eks_access_entry.lab_role,
    aws_eks_access_policy_association.lab_role_admin,
    aws_db_instance.postgres # Garante que o BD esteja pronto
  ]

  yaml_body = <<-YAML
    ---
    ${templatefile("${path.module}/../k8s/namespace.yaml", {})}
    ---
    ${templatefile("${path.module}/../k8s/api-configmap.yaml", local.k8s_template_vars)}
    ---
    ${templatefile("${path.module}/../k8s/api-secret.yaml", local.k8s_template_vars)}
    ---
    ${templatefile("${path.module}/../k8s/api-deployment.yaml", local.k8s_template_vars)}
    ---
    ${templatefile("${path.module}/../k8s/api-service.yaml", {})}
    ---
    ${templatefile("${path.module}/../k8s/api-hpa.yaml", {})}
  YAML
}

# ============================================
# OpenTelemetry Collector Resources (Opcional)
# ============================================
#
# A criação destes recursos continua, mas o deploy principal
# não vai falhar se a observabilidade não estiver configurada.
# O status será informado no final pelo script de deploy.
#
# ============================================

# Manifest unificado para observabilidade
resource "kubectl_manifest" "observability" {
  depends_on = [
    aws_eks_cluster.eks,
    aws_eks_node_group.nodes,
    aws_eks_access_entry.lab_role,
    aws_eks_access_policy_association.lab_role_admin
  ]

  yaml_body = <<-YAML
    ---
    ${templatefile("${path.module}/../k8s/observability-namespace.yaml", {})}
    ---
    ${templatefile("${path.module}/../k8s/otel-collector-configmap.yaml", { environment = var.environment })}
    ---
    ${templatefile("${path.module}/../k8s/otel-collector-secret.yaml", {})}
    ---
    ${templatefile("${path.module}/../k8s/otel-collector-deployment.yaml", {})}
    ---
    ${templatefile("${path.module}/../k8s/otel-collector-service.yaml", {})}
  YAML
}
