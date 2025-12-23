# ============================================
# Locals - Valores calculados e Lógica Condicional
# ============================================

locals {
  # --- Detecção de Ambiente ---
  # Determina se o ambiente é AWS Academy verificando a existência de roles específicas.
  is_academy = length(data.aws_iam_roles.voclabs.names) > 0 || length(data.aws_iam_roles.labrole.names) > 0

  # --- Lógica de Seleção de Roles ---
  # Seleciona dinamicamente a role do cluster EKS.
  # Se for Academy, busca automaticamente. Se não, usa a variável fornecida.
  eks_cluster_role_arn = local.is_academy ? (
    # Validação para garantir que a role do Academy foi encontrada.
    length(data.aws_iam_roles.eks_cluster_roles[0].arns) > 0 ? tolist(data.aws_iam_roles.eks_cluster_roles[0].arns)[0] :
    "ERRO: Role 'LabEksClusterRole' não encontrada no AWS Academy."
    ) : "arn:aws:iam::${data.aws_caller_identity.current.account_id}:role/${var.eks_cluster_role_name}"

  # Seleciona dinamicamente a role dos nós do EKS.
  eks_node_role_arn = local.is_academy ? (
    # Validação para garantir que a role do Academy foi encontrada.
    length(data.aws_iam_roles.eks_node_roles[0].arns) > 0 ? tolist(data.aws_iam_roles.eks_node_roles[0].arns)[0] :
    "ERRO: Role 'LabEksNodeRole' não encontrada no AWS Academy."
    ) : "arn:aws:iam::${data.aws_caller_identity.current.account_id}:role/${var.eks_node_role_name}"

  # Mensagem de erro customizada se a role do cluster não for encontrada no Academy.
  error_message_cluster_role = local.is_academy && length(data.aws_iam_roles.eks_cluster_roles[0].arns) == 0 ? file("error_messages/academy_role_not_found_cluster.txt") : ""

  # Mensagem de erro customizada se a role dos nós não for encontrada no Academy.
  error_message_node_role = local.is_academy && length(data.aws_iam_roles.eks_node_roles[0].arns) == 0 ? file("error_messages/academy_role_not_found_node.txt") : ""

  # --- Validação Robusta da LabRole ---
  # Garante que a descoberta da LabRole (usada para dar acesso ao aluno) seja segura.
  error_message_lab_role = !local.is_academy ? "" : (
    length(data.aws_iam_roles.labrole.names) == 0 ? file("error_messages/lab_role_not_found.txt") : (
      length(data.aws_iam_roles.labrole.names) > 1 ? file("error_messages/lab_role_too_many_found.txt") : ""
    )
  )

  # --- Configurações Gerais ---
  # URL base da API, usada para construir os outputs.
  api_url = try(kubernetes_service.api.status.load_balancer.ingress[0].hostname, "")

  # String de conexão do banco de dados (Supabase)
  db_connection_string = "Host=${var.db_host};Port=${var.db_port};Database=${var.db_name};Username=${var.db_username};Password=${var.db_password};SSL Mode=Require;Trust Server Certificate=true"

  # Account ID
  account_id = data.aws_caller_identity.current.account_id

  # Tag da imagem Docker - usa variável se fornecida, senão usa "latest"
  docker_image_tag = var.docker_image_tag != "" ? var.docker_image_tag : "latest"

  # Imagem Docker no ECR
  docker_image = "${local.account_id}.dkr.ecr.${var.aws_region}.amazonaws.com/${var.docker_image_repo}:${local.docker_image_tag}"
}
