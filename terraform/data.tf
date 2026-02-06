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

# As fontes de dados de VPC, Subnet e Security Group foram removidas
# para evitar erros de permissão no AWS Academy.
# A configuração agora cria e referencia seus próprios recursos de rede.

# Bloco removido para evitar dependência circular.
# O hostname do ALB agora é passado na segunda fase do 'terraform apply'.
