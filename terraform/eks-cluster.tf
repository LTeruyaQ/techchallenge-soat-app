# ============================================
# EKS Cluster
# ============================================

resource "aws_eks_cluster" "eks" {
  name = "eks-${var.project_name}"

  # Modo de autenticação API (necessário para AWS Academy)
  access_config {
    authentication_mode = "API"
  }

  # Usando role do AWS Academy
  role_arn = local.eks_cluster_role_arn
  version  = "1.31"

  vpc_config {
    subnet_ids         = aws_subnet.public[*].id
    security_group_ids = [aws_security_group.eks_cluster.id]
  }

  tags = {
    Name    = "eks-${var.project_name}"
    Project = "MecanicaOS"
  }

  depends_on = [
    aws_vpc.main,
    aws_subnet.public,
    aws_internet_gateway.igw,
    aws_route_table_association.public
  ]
}
