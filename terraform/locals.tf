# ============================================
# Locals - Valores calculados
# ============================================

locals {
  # Prefixo para todos os recursos para garantir nomes únicos
  prefix = var.project_name
  # Senha do banco de dados e chave JWT geradas dinamicamente
  db_password          = random_password.db_password.result
  jwt_secret_key       = random_password.jwt_secret_key.result

  # String de conexão será construída no módulo RDS
  db_connection_string = ""

  # Account ID
  account_id = data.aws_caller_identity.current.account_id

  # ARNs das Roles do IAM (construídos manualmente para contornar restrições de permissão)
  eks_cluster_role_arn = "arn:aws:iam::${local.account_id}:role/${var.eks_cluster_role}"
  eks_node_role_arn    = "arn:aws:iam::${local.account_id}:role/${var.eks_node_role}"
  lab_role_arn         = "arn:aws:iam::${local.account_id}:role/${var.lab_role}"

  # A lógica de construção da imagem foi removida pois estamos
  # usando uma imagem pública diretamente.
}
