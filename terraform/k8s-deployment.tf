# ============================================
# Kubernetes Deployment - Aplicação Principal
# ============================================

resource "kubernetes_deployment" "api" {
  metadata {
    name      = "mecanicaos-api"
    namespace = kubernetes_namespace.app.metadata[0].name
    labels = {
      app = "mecanicaos-api"
    }
  }

  spec {
    replicas = var.replicas

    selector {
      match_labels = {
        app = "mecanicaos-api"
      }
    }

    template {
      metadata {
        labels = {
          app = "mecanicaos-api"
        }
      }

      spec {
        container {
          name  = "mecanicaos-api"
          image = local.docker_image

          ports {
            container_port = 80
          }

          # Integração das Health Probes definidas em k8s-probes.tf
          liveness_probe {
            http_get {
              path = local.probes.liveness_probe.http_get.path
              port = local.probes.liveness_probe.http_get.port
            }
            initial_delay_seconds = local.probes.liveness_probe.initial_delay_seconds
            period_seconds        = local.probes.liveness_probe.period_seconds
            failure_threshold     = local.probes.liveness_probe.failure_threshold
          }

          readiness_probe {
            http_get {
              path = local.probes.readiness_probe.http_get.path
              port = local.probes.readiness_probe.http_get.port
            }
            initial_delay_seconds = local.probes.readiness_probe.initial_delay_seconds
            period_seconds        = local.probes.readiness_probe.period_seconds
            failure_threshold     = local.probes.readiness_probe.failure_threshold
          }

          env {
            name  = "DB_CONNECTION_STRING"
            value = local.db_connection_string
          }
          env {
            name = "JWT_SECRET_KEY"
            value_from {
              secret_key_ref {
                name = kubernetes_secret.jwt.metadata[0].name
                key  = "jwt_secret_key"
              }
            }
          }
          # Outras variáveis de ambiente para JWT...
        }
      }
    }
  }
}
