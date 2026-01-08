# ============================================
# EKS Cluster
# ============================================

resource "aws_eks_cluster" "eks" {
  name = "eks-${var.project_name}"

  # Modo de autenticação API (necessário para AWS Academy)
  access_config {
    authentication_mode = "API"
  }

  # Usando role do AWS Academy
  role_arn = local.eks_cluster_role_arn
  version  = "1.31"

  vpc_config {
    subnet_ids         = data.aws_subnets.private.ids
    security_group_ids = [aws_security_group.eks_cluster.id]
  }

  tags = {
    Name    = "eks-${var.project_name}"
    Project = "MecanicaOS"
  }
}
