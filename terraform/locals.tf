# ============================================
# Locals - Valores calculados
# ============================================

locals {
  # Prefixo para todos os recursos para garantir nomes únicos
  prefix = var.project_name
  # String de conexão do banco de dados (RDS)
  db_connection_string = "Host=${aws_db_instance.default.address};Port=${aws_db_instance.default.port};Database=${aws_db_instance.default.db_name};Username=${aws_db_instance.default.username};Password=${random_password.db_password.result};SSL Mode=Require;Trust Server Certificate=true"

  # Account ID
  account_id = data.aws_caller_identity.current.account_id

  # ARNs das Roles do IAM (construídos manualmente para contornar restrições de permissão)
  eks_cluster_role_arn = "arn:aws:iam::${local.account_id}:role/${var.eks_cluster_role}"
  eks_node_role_arn    = "arn:aws:iam::${local.account_id}:role/${var.eks_node_role}"
  lab_role_arn         = "arn:aws:iam::${local.account_id}:role/${var.lab_role}"

  # A lógica de construção da imagem foi removida pois estamos
  # usando uma imagem pública diretamente.
}
