# ============================================
# Kubernetes - Database Initialization Job
# ============================================

# Cria um ConfigMap com o script SQL de inicialização
resource "kubernetes_config_map" "db_init_sql" {
  metadata {
    name      = "db-init-sql-config"
    namespace = "default"
  }

  data = {
    "rds-init.sql" = file("${path.module}/rds-init.sql")
  }
}

# Cria um Job no Kubernetes para executar o script SQL
resource "kubernetes_job" "db_init" {
  metadata {
    name      = "db-init-job"
    namespace = "default"
  }

  spec {
    template {
      metadata {}
      spec {
        container {
          name    = "db-init-container"
          image   = "postgres:13"
          command = ["/bin/sh", "-c"]
          args = [
            <<-EOT
              PGPASSWORD=$(DB_PASSWORD) psql \
                -h $(DB_HOST) \
                -U $(DB_USER) \
                -d $(DB_NAME) \
                -f /sql/rds-init.sql
            EOT
          ]

          env {
            name  = "DB_HOST"
            value = aws_db_instance.default.address
          }
          env {
            name  = "DB_NAME"
            value = aws_db_instance.default.db_name
          }
          env {
            name = "DB_USER"
            value_from {
              secret_key_ref {
                name = kubernetes_secret.db_credentials.metadata[0].name
                key  = "username"
              }
            }
          }
          env {
            name = "DB_PASSWORD"
            value_from {
              secret_key_ref {
                name = kubernetes_secret.db_credentials.metadata[0].name
                key  = "password"
              }
            }
          }

          volume_mount {
            name       = "sql-volume"
            mount_path = "/sql"
          }
        }

        volume {
          name = "sql-volume"
          config_map {
            name = kubernetes_config_map.db_init_sql.metadata[0].name
          }
        }

        restart_policy = "Never"
      }
    }
    backoff_limit = 4
  }

  depends_on = [
    aws_db_instance.default,
    kubernetes_secret.db_credentials
  ]
}
