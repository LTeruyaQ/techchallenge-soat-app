# ============================================
# EKS Access Entry e Policy
# ============================================
#
# Necessário para AWS Academy permitir acesso ao cluster
#
# ============================================

# ARN da role voclabs (usuário do AWS Academy)
# Não podemos usar data source porque o AWS Academy bloqueia iam:GetRole para esta role
locals {
  voclabs_role_arn = "arn:aws:iam::${local.account_id}:role/voclabs"
}

# Access Entry para LabRole
resource "aws_eks_access_entry" "lab_role" {
  cluster_name      = aws_eks_cluster.eks.name
  principal_arn     = data.aws_iam_role.lab_role.arn
  type              = "STANDARD"
}

resource "aws_eks_access_policy_association" "lab_role_admin" {
  cluster_name  = aws_eks_cluster.eks.name
  policy_arn    = "arn:aws:eks::aws:cluster-access-policy/AmazonEKSAdminPolicy"
  principal_arn = data.aws_iam_role.lab_role.arn

  access_scope {
    type = "cluster"
  }

  depends_on = [aws_eks_access_entry.lab_role]
}

# Access Entry para voclabs (usuário do console AWS Academy)
resource "aws_eks_access_entry" "voclabs" {
  cluster_name      = aws_eks_cluster.eks.name
  principal_arn     = local.voclabs_role_arn
  type              = "STANDARD"
}

resource "aws_eks_access_policy_association" "voclabs_admin" {
  cluster_name  = aws_eks_cluster.eks.name
  policy_arn    = "arn:aws:eks::aws:cluster-access-policy/AmazonEKSClusterAdminPolicy"
  principal_arn = local.voclabs_role_arn

  access_scope {
    type = "cluster"
  }

  depends_on = [aws_eks_access_entry.voclabs]
}
