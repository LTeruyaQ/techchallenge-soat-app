# ============================================
# EKS Access Entry e Policy
# ============================================
#
# Necessário para AWS Academy permitir acesso ao cluster
#
# ============================================

resource "aws_eks_access_entry" "lab_role" {
  cluster_name      = aws_eks_cluster.eks.name
  principal_arn     = data.aws_iam_role.lab_role.arn
  kubernetes_groups = ["system:masters"]
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
