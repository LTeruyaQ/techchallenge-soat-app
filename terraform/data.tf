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

# Descobre a VPC padrão da AWS Academy
data "aws_vpc" "existing" {
  default = true
}

# Descobre as subnets privadas existentes na VPC
data "aws_subnets" "private" {
  filter {
    name   = "vpc-id"
    values = [data.aws_vpc.existing.id]
  }
  tags = {
    "aws:cloudformation:logical-id" = "PrivateSubnet1"
  }
}

# Bloco removido para evitar dependência circular.
# O hostname do ALB agora é passado na segunda fase do 'terraform apply'.
