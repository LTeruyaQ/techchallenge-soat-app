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
    replicas             = var.replicas
    docker_image         = local.docker_image
    environment          = var.environment
    db_connection_string = local.db_connection_string
    jwt_secret_key       = var.jwt_secret_key
    jwt_issuer           = var.jwt_issuer
    jwt_audience         = var.jwt_audience
    jwt_expiry_minutes   = var.jwt_expiry_minutes
  }
}

# Namespace
resource "kubectl_manifest" "k8s_namespace" {
  depends_on = [
    aws_eks_cluster.eks,
    aws_eks_node_group.nodes,
    aws_eks_access_entry.lab_role,
    aws_eks_access_policy_association.lab_role_admin
  ]

  yaml_body = file("${path.module}/../k8s/namespace.yaml")
}

# ConfigMap
resource "kubectl_manifest" "k8s_configmap" {
  depends_on = [kubectl_manifest.k8s_namespace]

  yaml_body = templatefile("${path.module}/../k8s/api-configmap.yaml", local.k8s_template_vars)
}

# Secret
resource "kubectl_manifest" "k8s_secret" {
  depends_on = [kubectl_manifest.k8s_namespace]

  yaml_body = templatefile("${path.module}/../k8s/api-secret.yaml", local.k8s_template_vars)
}

# Deployment
resource "kubectl_manifest" "k8s_deployment" {
  depends_on = [
    kubectl_manifest.k8s_namespace,
    kubectl_manifest.k8s_configmap,
    kubectl_manifest.k8s_secret
  ]

  yaml_body = templatefile("${path.module}/../k8s/api-deployment.yaml", local.k8s_template_vars)
}

# Service
resource "kubectl_manifest" "k8s_service" {
  depends_on = [kubectl_manifest.k8s_deployment]

  yaml_body = file("${path.module}/../k8s/api-service.yaml")
}

# HPA
resource "kubectl_manifest" "k8s_hpa" {
  depends_on = [kubectl_manifest.k8s_deployment]

  yaml_body = file("${path.module}/../k8s/api-hpa.yaml")
}
