# ============================================
# EKS Node Group
# ============================================

resource "aws_eks_node_group" "nodes" {
  cluster_name    = aws_eks_cluster.eks.name
  node_group_name = "nodegroup-${var.project_name}"

  # Usando role do AWS Academy
  node_role_arn = local.eks_node_role_arn

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
