# ============================================
# Locals - Valores calculados
# ============================================

locals {
  # String de conexão do banco de dados
  db_connection_string = "Host=${var.db_host};Port=${var.db_port};Database=${var.db_name};Username=${var.db_username};Password=${var.db_password};SSL Mode=Require"

  # Account ID
  account_id = data.aws_caller_identity.current.account_id
}
