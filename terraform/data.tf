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

# ============================================
# Data Sources - Descoberta de Rede e Segurança
# ============================================

# VPC Padrão (AWS Academy)
data "aws_vpc" "default" {
  default = true
}

# Subnets privadas na VPC
data "aws_subnets" "private" {
  filter {
    name   = "vpc-id"
    values = [data.aws_vpc.default.id]
  }
  tags = {
    "aws:cloudformation:logical-id" = "PrivateSubnet1"
  }
}

# Security Group dos Nodes do EKS
data "aws_security_group" "eks_nodes" {
  filter {
    name   = "tag:Name"
    values = ["${local.prefix}-eks-node-sg"]
  }
  depends_on = [aws_eks_node_group.nodes]
}

# Bloco removido para evitar dependência circular.
# O hostname do ALB agora é passado na segunda fase do 'terraform apply'.
