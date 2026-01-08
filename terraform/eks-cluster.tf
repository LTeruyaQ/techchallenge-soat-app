# ============================================
# EKS Cluster
# Gerencia a criação ou a importação de um cluster EKS
# ============================================

# Tenta criar o cluster somente se a flag skip_create_eks for falsa
resource "aws_eks_cluster" "eks" {
  count = var.skip_create_eks ? 0 : 1

  name     = "eks-mecanicaos"
  role_arn = "arn:aws:iam::${data.aws_caller_identity.current.account_id}:role/${var.eks_cluster_role}"
  version  = "1.28" # Usando uma versão LTS estável

  vpc_config {
    subnet_ids         = aws_subnet.public[*].id
    security_group_ids = [aws_security_group.eks_cluster.id]
  }

  tags = {
    Name    = "eks-mecanicaos"
    Project = "MecanicaOS"
  }
}

# Data source para ler o cluster que já existe (se skip_create_eks for verdadeiro)
data "aws_eks_cluster" "existing" {
  count = var.skip_create_eks ? 1 : 0
  name  = "eks-mecanicaos"
}

data "aws_eks_cluster_auth" "existing_auth" {
  count = var.skip_create_eks ? 1 : 0
  name  = "eks-mecanicaos"
}

# Local para unificar a referência ao cluster, seja ele criado ou existente
locals {
  eks_cluster_endpoint = var.skip_create_eks ? data.aws_eks_cluster.existing[0].endpoint : aws_eks_cluster.eks[0].endpoint
  eks_cluster_ca_certificate = var.skip_create_eks ? data.aws_eks_cluster.existing[0].certificate_authority[0].data : aws_eks_cluster.eks[0].certificate_authority[0].data
  eks_cluster_name = "eks-mecanicaos" # Nome é fixo
}
