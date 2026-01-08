# ============================================
# Data Sources - AWS Academy
# ============================================

# Dados do cluster EKS (usando o local para consistência)
data "aws_eks_cluster" "cluster" {
  name = local.eks_cluster_name
}

data "aws_eks_cluster_auth" "auth" {
  name = local.eks_cluster_name
}

# Account ID atual
data "aws_caller_identity" "current" {}

data "aws_iam_role" "lab_role" {
  name = "LabRole"
}

# As fontes de dados de VPC, Subnet e Security Group foram removidas
# para evitar erros de permissão no AWS Academy.
# A configuração agora cria e referencia seus próprios recursos de rede.

# Bloco removido para evitar dependência circular.
# O hostname do ALB agora é passado na segunda fase do 'terraform apply'.
