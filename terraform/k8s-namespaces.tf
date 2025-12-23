# ============================================
# Kubernetes Namespaces
# ============================================
#
# Define os namespaces usados pela aplicacao e seus componentes.
# Usar recursos nativos do Terraform em vez de manifestos YAML
# permite melhor gerenciamento de estado e dependencias.
#
# ============================================

# Namespace para a aplicacao principal 'mecanicaos'.
resource "kubernetes_namespace" "app" {
  metadata {
    name = "mecanicaos"
  }
}

# Namespace para componentes de observabilidade, como o OpenTelemetry Collector.
resource "kubernetes_namespace" "observability" {
  metadata {
    name = "observability"
  }
}
