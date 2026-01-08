# ============================================
# Data Sources - AWS Academy
# ============================================

# Account ID atual
data "aws_caller_identity" "current" {}

# Procura a VPC do Lab
data "aws_vpc" "lab_vpc" {
  filter {
    name   = "tag:Name"
    values = ["*Lab*"]
  }
}

# Procura as Subnets Privadas na VPC do Lab
data "aws_subnets" "private" {
  filter {
    name   = "vpc-id"
    values = [data.aws_vpc.lab_vpc.id]
  }

  tags = {
    "kubernetes.io/role/internal-elb" = "1"
  }
}

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
