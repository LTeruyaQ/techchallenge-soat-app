# ============================================
# IAM Roles - AWS Academy
# ============================================
#
# AWS Academy NÃO permite criar IAM Roles/Policies
# Devemos usar as roles pré-existentes fornecidas pelo lab
#
# ============================================

# LabRole padrão do AWS Academy
data "aws_iam_role" "lab_role" {
  name = "LabRole"
}

# Descoberta automática da role do EKS Cluster
# Procura por uma role que contenha "LabEksClusterRole" no nome
data "aws_iam_roles" "eks_cluster_roles" {
  name_regex = ".*LabEksClusterRole.*"
}

# Descoberta automática da role do EKS Node
# Procura por uma role que contenha "LabEksNodeRole" no nome
data "aws_iam_roles" "eks_node_roles" {
  name_regex = ".*LabEksNodeRole.*"
}

# ============================================
# Locals para extrair ARNs
# ============================================
#
# Pega o ARN da primeira role encontrada em cada busca.
# O `try` com `[]` garante que o Terraform não falhe se a busca
# não retornar resultados (embora no Academy sempre deva retornar).
#
locals {
  eks_cluster_role_arn = try(tolist(data.aws_iam_roles.eks_cluster_roles.arns)[0], null)
  eks_node_role_arn    = try(tolist(data.aws_iam_roles.eks_node_roles.arns)[0], null)
}
