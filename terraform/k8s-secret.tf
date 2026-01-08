# ============================================
# Kubernetes - Secrets
# ============================================

# Busca o segredo do banco de dados no AWS Secrets Manager
data "aws_secretsmanager_secret_version" "db_credentials" {
  secret_id = aws_secretsmanager_secret.db_credentials.id
}

# Cria um Secret no Kubernetes com as credenciais do banco de dados
resource "kubernetes_secret" "db_credentials" {
  metadata {
    name      = "db-credentials"
    namespace = "default"
  }

  data = {
    username = jsondecode(data.aws_secretsmanager_secret_version.db_credentials.secret_string)["username"]
    password = jsondecode(data.aws_secretsmanager_secret_version.db_credentials.secret_string)["password"]
  }

  type = "Opaque"
}
