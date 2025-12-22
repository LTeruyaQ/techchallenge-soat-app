# ============================================
# EKS Node Group
# ============================================

resource "aws_eks_node_group" "nodes" {
  cluster_name    = aws_eks_cluster.eks.name
  node_group_name = "nodegroup-${var.project_name}"

  # Utiliza a role ARN determinada pela lógica em locals.tf.
  node_role_arn = local.eks_node_role_arn

  # Validações de Ciclo de Vida:
  # Garante que as premissas para a criação do Node Group sejam atendidas.
  lifecycle {
    # 1. Valida se a role do AWS Academy foi encontrada.
    precondition {
      condition     = local.error_message_node_role == ""
      error_message = local.error_message_node_role
    }

    # 2. Valida se a policy 'AmazonEKSWorkerNodePolicy' está anexada.
    precondition {
      condition     = data.aws_iam_role_policy_attachment.node_policy_check.policy_arn != null
      error_message = "ERRO: A role dos nos (${local.eks_node_role_arn}) nao tem a policy 'AmazonEKSWorkerNodePolicy' anexada. Essencial para o no se registrar no cluster."
    }

    # 3. Valida se a policy 'AmazonEC2ContainerRegistryReadOnly' está anexada.
    precondition {
      condition     = data.aws_iam_role_policy_attachment.ecr_policy_check.policy_arn != null
      error_message = "ERRO: A role dos nos (${local.eks_node_role_arn}) nao tem a policy 'AmazonEC2ContainerRegistryReadOnly' anexada. Essencial para baixar a imagem Docker do ECR."
    }
  }

  subnet_ids     = aws_subnet.public[*].id
  disk_size      = 50
  instance_types = var.instance_types

  scaling_config {
    desired_size = var.node_desired_size
    max_size     = var.node_max_size
    min_size     = var.node_min_size
  }

  update_config {
    max_unavailable = 1
  }

  tags = {
    Name    = "nodegroup-${var.project_name}"
    Project = "MecanicaOS"
  }

  # AWS Academy: LabRole já vem com as policies necessárias
  depends_on = [aws_eks_cluster.eks]
}
