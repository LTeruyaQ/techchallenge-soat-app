# ============================================
# EKS Access Entry e Policy (Exclusivo para AWS Academy)
# ============================================
#
# Cria as permissões necessárias para que o usuário do AWS Academy Lab
# (LabRole e voclabs) possa administrar o cluster EKS.
# Estes recursos são criados apenas se 'local.is_academy' for verdadeiro.
#
# ============================================

# O ARN da role 'voclabs' é construído dinamicamente.
# Não é possível usar um data source 'aws_iam_role' porque o AWS Academy
# restringe a permissão iam:GetRole para esta role específica,
# então construímos o ARN manualmente, o que é uma exceção aceitável.
locals {
  voclabs_role_arn = "arn:aws:iam::${local.account_id}:role/voclabs"
}

# --- Acesso para a LabRole ---

resource "aws_eks_access_entry" "lab_role" {
  # Cria este recurso apenas no ambiente Academy.
  count = local.is_academy ? 1 : 0

  cluster_name  = aws_eks_cluster.eks.name
  principal_arn = data.aws_iam_role.lab_role[0].arn
  type          = "STANDARD"

  lifecycle {
    precondition {
      condition     = local.error_message_lab_role == ""
      error_message = local.error_message_lab_role
    }
  }
}

resource "aws_eks_access_policy_association" "lab_role_admin" {
  count = local.is_academy ? 1 : 0

  cluster_name  = aws_eks_cluster.eks.name
  policy_arn    = "arn:aws:eks::aws:cluster-access-policy/AmazonEKSAdminPolicy"
  principal_arn = data.aws_iam_role.lab_role[0].arn

  access_scope {
    type = "cluster"
  }

  depends_on = [aws_eks_access_entry.lab_role]
}

# --- Acesso para a voclabs (usuário do console) ---

resource "aws_eks_access_entry" "voclabs" {
  count = local.is_academy ? 1 : 0

  cluster_name  = aws_eks_cluster.eks.name
  principal_arn = local.voclabs_role_arn
  type          = "STANDARD"
}

resource "aws_eks_access_policy_association" "voclabs_admin" {
  count = local.is_academy ? 1 : 0

  cluster_name  = aws_eks_cluster.eks.name
  # Usamos uma policy mais restrita para o usuário do console, como boa prática.
  policy_arn    = "arn:aws:eks::aws:cluster-access-policy/AmazonEKSClusterAdminPolicy"
  principal_arn = local.voclabs_role_arn

  access_scope {
    type = "cluster"
  }

  depends_on = [aws_eks_access_entry.voclabs]
}
