# ============================================
# Data Sources - Fontes de Dados
# ============================================

# --- Detecção de Ambiente ---

# Procura por roles que indicam o ambiente AWS Academy.
# Usamos 'aws_iam_roles' para listar roles sem causar erro se não existirem.
data "aws_iam_roles" "voclabs" {
  name_regex = "^voclabs.*"
}

data "aws_iam_roles" "labrole" {
  name_regex = "^LabRole.*"
}

data "aws_iam_role" "lab_role" {
  count = local.is_academy ? 1 : 0
  name  = length(data.aws_iam_roles.labrole.names) > 0 ? tolist(data.aws_iam_roles.labrole.names)[0] : ""
}

# --- Descoberta Dinâmica de Roles para o EKS (AWS Academy) ---

# Busca pela role do Cluster EKS no ambiente Academy.
data "aws_iam_roles" "eks_cluster_roles" {
  # O Terraform continuará se count=0, permitindo a lógica condicional.
  count      = local.is_academy ? 1 : 0
  name_regex = ".*-LabEksClusterRole-.*"
}

# Busca pela role dos Nós do EKS no ambiente Academy.
data "aws_iam_roles" "eks_node_roles" {
  count      = local.is_academy ? 1 : 0
  name_regex = ".*-LabEksNodeRole-.*"
}

# --- Dados do Cluster e Autenticação ---

# Obtém informações do cluster EKS após sua criação.
data "aws_eks_cluster" "cluster" {
  name = aws_eks_cluster.eks.name
}

# Obtém o token de autenticação para o cluster.
data "aws_eks_cluster_auth" "auth" {
  name = aws_eks_cluster.eks.name
}

# Obtém o Account ID da conta AWS atual.
data "aws_caller_identity" "current" {}

# --- Validação de Roles (Conta Normal) ---

# Valida se a role do cluster fornecida manualmente existe.
data "aws_iam_role" "eks_cluster_role_validation" {
  # Executa apenas se não for Academy e um nome de role for fornecido.
  count = !local.is_academy && var.eks_cluster_role_name != "" ? 1 : 0
  name  = var.eks_cluster_role_name

}

# Valida se a role dos nós fornecida manualmente existe.
data "aws_iam_role" "eks_node_role_validation" {
  count = !local.is_academy && var.eks_node_role_name != "" ? 1 : 0
  name  = var.eks_node_role_name
}
