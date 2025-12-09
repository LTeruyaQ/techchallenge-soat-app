# ============================================
# Locals - Valores calculados
# ============================================

locals {
  # String de conexão do banco de dados (Supabase)
  db_connection_string = "Host=${var.db_host};Port=${var.db_port};Database=${var.db_name};Username=${var.db_username};Password=${var.db_password};SSL Mode=Require;Trust Server Certificate=true"

  # Account ID
  account_id = data.aws_caller_identity.current.account_id

  # Imagem Docker no ECR
  docker_image = "${local.account_id}.dkr.ecr.${var.aws_region}.amazonaws.com/${var.docker_image_repo}:${var.docker_image_tag}"
}
