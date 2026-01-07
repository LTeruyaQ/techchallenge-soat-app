data "aws_secretsmanager_secret_version" "db_creds" {
  secret_id = var.db_credentials_secret_arn
}

resource "kubernetes_secret" "api_secret" {
  metadata {
    name = "api-secret"
  }

  data = {
    # Acessado como ConnectionStrings__DefaultConnection no appsettings.json
    "ConnectionStrings__DefaultConnection" = "Host=${jsondecode(data.aws_secretsmanager_secret_version.db_creds.secret_string)["host"]};Port=${jsondecode(data.aws_secretsmanager_secret_version.db_creds.secret_string)["port"]};Database=${jsondecode(data.aws_secretsmanager_secret_version.db_creds.secret_string)["dbname"]};Username=${jsondecode(data.aws_secretsmanager_secret_version.db_creds.secret_string)["username"]};Password=${jsondecode(data.aws_secretsmanager_secret_version.db_creds.secret_string)["password"]}"

    # Acessado como Jwt:SecretKey no appsettings.json
    "Jwt__SecretKey" = var.jwt_secret
  }
}

resource "kubernetes_deployment" "mecanicaos_api" {
  metadata {
    name = "mecanicaos-api"
    labels = {
      app = "mecanicaos-api"
    }
  }

  spec {
    replicas = 1
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
          image = var.docker_image
          port {
            container_port = 8080
          }

          env_from {
            secret_ref {
              name = kubernetes_secret.api_secret.metadata[0].name
            }
          }

          resources {
            requests = {
              cpu    = "250m"
              memory = "256Mi"
            }
            limits = {
              cpu    = "500m"
              memory = "512Mi"
            }
          }
        }
      }
    }
  }
}

resource "kubernetes_service" "mecanicaos_service" {
  metadata {
    name = "mecanicaos-service"
  }

  spec {
    selector = {
      app = "mecanicaos-api"
    }

    port {
      port        = 80
      target_port = 8080
    }

    type = "LoadBalancer"
  }
}

resource "kubernetes_horizontal_pod_autoscaler" "mecanicaos_hpa" {
  metadata {
    name = "mecanicaos-hpa"
  }

  spec {
    scale_target_ref {
      api_version = "apps/v1"
      kind        = "Deployment"
      name        = kubernetes_deployment.mecanicaos_api.metadata[0].name
    }

    min_replicas = 1
    max_replicas = 5

    target_cpu_utilization_percentage = 50
  }
}
