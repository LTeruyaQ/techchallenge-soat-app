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

# Role específica para EKS Cluster (fornecida pelo AWS Academy)
# IMPORTANTE: Copie o nome exato da sua LabEksClusterRole do console AWS
data "aws_iam_role" "eks_cluster_role" {
  name = var.eks_cluster_role_name
}

# Role específica para EKS Nodes (fornecida pelo AWS Academy)
# IMPORTANTE: Copie o nome exato da sua LabEksNodeRole do console AWS
data "aws_iam_role" "eks_node_role" {
  name = var.eks_node_role_name
}
