# ============================================
# EKS Cluster
# ============================================

resource "aws_eks_cluster" "eks" {
  name = "eks-${var.project_name}"

  # Modo de autenticação da API. "API" é usado no AWS Academy,
  # enquanto "CONFIG_MAP" seria para contas normais com maior controle.
  access_config {
    authentication_mode = "API"
  }

  # Utiliza a role ARN determinada pela lógica em locals.tf.
  # Isso seleciona dinamicamente entre a role do Academy e a fornecida para contas normais.
  role_arn = local.eks_cluster_role_arn
  version  = "1.31"

  # Validação: Garante que o apply falhe se a role do Academy não for encontrada,
  # usando uma mensagem de erro clara e declarativa.
  lifecycle {
    precondition {
      condition     = local.error_message_cluster_role == ""
      error_message = local.error_message_cluster_role
    }
  }

  vpc_config {
    subnet_ids         = aws_subnet.public[*].id
    security_group_ids = [aws_security_group.eks.id]
  }

  tags = {
    Name    = "eks-${var.project_name}"
    Project = "MecanicaOS"
  }

  depends_on = [
    aws_vpc.main,
    aws_subnet.public,
    aws_internet_gateway.igw,
    aws_route_table_association.public
  ]
}
