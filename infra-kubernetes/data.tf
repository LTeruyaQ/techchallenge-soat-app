# ============================================
# Data Sources - AWS Academy
# ============================================

# Dados do cluster EKS (após criação)
data "aws_eks_cluster" "cluster" {
  name = aws_eks_cluster.eks.name
}

data "aws_eks_cluster_auth" "auth" {
  name = aws_eks_cluster.eks.name
}

# Account ID atual
data "aws_caller_identity" "current" {}
