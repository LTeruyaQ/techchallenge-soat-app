# ============================================
# Data Sources - AWS Academy
# ============================================

# Obtém o Account ID da conta AWS que está executando o script.
# É usado para construir ARNs dinamicamente, como o do ECR.
data "aws_caller_identity" "current" {}

# Obtém informações sobre as roles do IAM pré-existentes no ambiente AWS Academy.
# Isso evita a necessidade de criá-las, o que não é permitido.
data "aws_iam_role" "eks_cluster_role" {
  name = var.eks_cluster_role_name
}

data "aws_iam_role" "eks_node_role" {
  name = var.eks_node_role_name
}
