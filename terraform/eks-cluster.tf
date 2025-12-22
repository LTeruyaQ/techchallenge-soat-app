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
  version  = var.eks_cluster_version

  # Validações de Ciclo de Vida:
  # Garante que as premissas para a criação do cluster sejam atendidas.
  lifecycle {
    # 1. Valida se a role do AWS Academy foi encontrada.
    precondition {
      condition     = local.error_message_cluster_role == ""
      error_message = local.error_message_cluster_role
    }

    # 2. Valida se a policy 'AmazonEKSClusterPolicy' está anexada à role.
    precondition {
      # O data source 'cluster_policy_check' só encontrará algo se a policy estiver anexada.
      condition     = data.aws_iam_role_policy_attachment.cluster_policy_check.policy_arn != null
      error_message = "ERRO: A role do cluster (${local.eks_cluster_role_arn}) nao tem a policy 'AmazonEKSClusterPolicy' anexada. Esta policy e essencial para que o EKS possa gerenciar recursos em seu nome."
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
