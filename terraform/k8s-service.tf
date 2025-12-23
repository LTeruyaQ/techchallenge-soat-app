# ============================================
# Kubernetes Service (Load Balancer)
# ============================================
#
# Define o Service do tipo LoadBalancer que expoe a aplicacao
# para a internet.
#
# ============================================

resource "kubernetes_service" "api" {
  metadata {
    name      = "mecanicaos-service"
    namespace = kubernetes_namespace.app.metadata[0].name
    labels = {
      app = "mecanicaos-api"
    }
    annotations = {
      # Anotacoes especificas da AWS para o Load Balancer Controller
      # O Health Check do Load Balancer deve sempre apontar para o endpoint de READINESS.
      "service.beta.kubernetes.io/aws-load-balancer-healthcheck-path"           = "/health/ready"
      "service.beta.kubernetes.io/aws-load-balancer-healthcheck-port"            = "80"
      "service.beta.kubernetes.io/aws-load-balancer-healthcheck-protocol"        = "HTTP"
      "service.beta.kubernetes.io/aws-load-balancer-healthcheck-interval"        = "30"
      "service.beta.kubernetes.io/aws-load-balancer-healthcheck-timeout"         = "5"
      "service.beta.kubernetes.io/aws-load-balancer-healthcheck-healthy-threshold"   = "2"
      "service.beta.kubernetes.io/aws-load-balancer-healthcheck-unhealthy-threshold" = "2"
    }
  }

  spec {
    selector = {
      app = "mecanicaos-api"
    }
    type = "LoadBalancer"
    port {
      protocol    = "TCP"
      port        = 80
      target_port = 80
    }
  }

  depends_on = [kubernetes_deployment.api]
}
