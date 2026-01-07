# ============================================
# EKS Cluster
# ============================================

resource "aws_eks_cluster" "eks" {
  name = "eks-${var.project_name}"

  # Modo de autenticação API (necessário para AWS Academy)
  access_config {
    authentication_mode = "API_AND_CONFIG_MAP"
  }

  # Usando role do AWS Academy
  role_arn = data.aws_iam_role.eks_cluster_role.arn
  version  = "1.28"

  vpc_config {
    subnet_ids         = concat(aws_subnet.public[*].id, aws_subnet.private[*].id)
    security_group_ids = [aws_security_group.eks.id]
  }

  tags = {
    Name    = "eks-${var.project_name}"
    Project = "MecanicaOS"
  }

  depends_on = [
    aws_vpc.main,
    aws_subnet.public,
    aws_subnet.private,
    aws_internet_gateway.igw,
    aws_route_table_association.public,
    aws_route_table_association.private
  ]
}
