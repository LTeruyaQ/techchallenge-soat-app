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
    docker_image                = local.docker_image
    environment                 = var.environment
    db_connection_string        = local.db_connection_string
    jwt_secret_key              = var.jwt_secret_key
    jwt_issuer                  = var.jwt_issuer
    jwt_audience                = var.jwt_audience
    jwt_expiry_minutes          = var.jwt_expiry_minutes
    otel_service_name           = var.otel_service_name
    otel_exporter_otlp_endpoint = var.otel_exporter_otlp_endpoint
  }

  otel_template_vars = {
    environment          = var.environment
    datadog_api_key      = var.datadog_api_key
    newrelic_license_key = var.newrelic_license_key
  }
}

# ConfigMap
resource "kubectl_manifest" "k8s_configmap" {
  depends_on = [kubernetes_namespace.app]

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
    kubernetes_namespace.app,
    kubectl_manifest.k8s_configmap,
    kubectl_manifest.k8s_secret
  ]

  yaml_body = templatefile("${path.module}/../k8s/api-deployment.yaml", local.k8s_template_vars)
}

# HPA
resource "kubectl_manifest" "k8s_hpa" {
  depends_on = [kubectl_manifest.k8s_deployment]

  yaml_body = file("${path.module}/../k8s/api-hpa.yaml")
}

# ============================================
# OpenTelemetry Collector Resources
# ============================================

# OTEL Collector Secret
resource "kubectl_manifest" "otel_collector_secret" {
  depends_on = [kubernetes_namespace.observability]

  yaml_body = templatefile("${path.module}/../k8s/otel-collector-secret.yaml", local.otel_template_vars)
}

# OTEL Collector ConfigMap
resource "kubectl_manifest" "otel_collector_configmap" {
  depends_on = [kubectl_manifest.otel_namespace]

  yaml_body = templatefile("${path.module}/../k8s/otel-collector-configmap.yaml", local.otel_template_vars)
}

# OTEL Collector Deployment
resource "kubectl_manifest" "otel_collector_deployment" {
  depends_on = [
    kubectl_manifest.otel_namespace,
    kubectl_manifest.otel_collector_secret,
    kubectl_manifest.otel_collector_configmap
  ]

  yaml_body = file("${path.module}/../k8s/otel-collector-deployment.yaml")
}

# OTEL Collector Service
resource "kubectl_manifest" "otel_collector_service" {
  depends_on = [kubectl_manifest.otel_collector_deployment]

  yaml_body = file("${path.module}/../k8s/otel-collector-service.yaml")
}
