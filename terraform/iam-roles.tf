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

# Role do Cluster EKS (detectada automaticamente pelo script)
data "aws_iam_role" "eks_cluster_role" {
  name = var.eks_cluster_role_name
}

# Role do NodeGroup do EKS (detectada automaticamente)
data "aws_iam_role" "eks_node_role" {
  name = var.eks_node_role_name
}
