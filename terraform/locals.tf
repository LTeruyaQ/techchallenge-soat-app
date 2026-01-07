# ============================================
# Locals - Valores calculados
# ============================================

locals {
  # Account ID
  account_id = data.aws_caller_identity.current.account_id

  # Extrai as credenciais do RDS do segredo no Secrets Manager
  rds_credentials = jsondecode(aws_secretsmanager_secret_version.rds_credentials.secret_string)

  # Extrai a chave secreta JWT do segredo no Secrets Manager
  jwt_secret = jsondecode(aws_secretsmanager_secret_version.jwt_key.secret_string)

  # String de conexão do banco de dados (RDS)
  db_connection_string = "Host=${aws_db_instance.postgres.address};Port=${aws_db_instance.postgres.port};Database=${aws_db_instance.postgres.db_name};Username=${local.rds_credentials.username};Password=${local.rds_credentials.password};"
}
