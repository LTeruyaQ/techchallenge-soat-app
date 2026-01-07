# ============================================
# EKS Node Group
# ============================================

resource "aws_eks_node_group" "nodes" {
  cluster_name    = aws_eks_cluster.eks.name
  node_group_name = "${var.project_name}-nodes"
  node_role_arn   = data.aws_iam_role.eks_node_role.arn
  subnet_ids      = aws_subnet.private[*].id
  instance_types  = ["t3.small"]

  scaling_config {
    desired_size = 2
    max_size     = 3
    min_size     = 1
  }

  vpc_security_group_ids = [aws_security_group.eks_nodes.id]

  depends_on = [
    aws_eks_cluster.eks
  ]
}
