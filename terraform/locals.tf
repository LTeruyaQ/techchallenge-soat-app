# ============================================
# Locals - Valores calculados
# ============================================

locals {
  # Prefixo para todos os recursos para garantir nomes únicos
  prefix = var.project_name
  # String de conexão do banco de dados (Supabase)
  db_connection_string = "Host=${var.db_host};Port=${var.db_port};Database=${var.db_name};Username=${var.db_username};Password=${var.db_password};SSL Mode=Require;Trust Server Certificate=true"

  # Account ID
  account_id = data.aws_caller_identity.current.account_id

  # ARNs das Roles do IAM (construídos manualmente para contornar restrições de permissão)
  eks_cluster_role_arn = "arn:aws:iam::${local.account_id}:role/${var.eks_cluster_role}"
  eks_node_role_arn    = "arn:aws:iam::${local.account_id}:role/${var.eks_node_role}"
  lab_role_arn         = "arn:aws:iam::${local.account_id}:role/${var.lab_role}"

  # Tag da imagem Docker - usa variável se fornecida, senão usa "latest"
  docker_image_tag = var.docker_image_tag != "" ? var.docker_image_tag : "latest"

  # Imagem Docker no ECR
  docker_image = "${var.ecr_repository_url}:${local.docker_image_tag}"
}
