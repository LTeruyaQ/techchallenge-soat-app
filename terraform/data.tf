# ============================================
# Data Sources - AWS Academy
# ============================================

# Account ID atual
data "aws_caller_identity" "current" {}

# Dados do cluster EKS (após criação)
data "aws_eks_cluster" "cluster" {
  name = aws_eks_cluster.eks.name
}

data "aws_eks_cluster_auth" "auth" {
  name = aws_eks_cluster.eks.name
}

data "aws_iam_role" "lab_role" {
  name = "LabRole"
}
