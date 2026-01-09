# ============================================
# Security Groups - EKS
# ============================================

resource "aws_security_group" "eks_cluster" {
  name        = "${var.project_name}-eks-cluster-sg"
  description = "Security group for EKS control plane"
  vpc_id      = local.vpc_id
  tags        = { Name = "${var.project_name}-eks-cluster-sg" }
}

resource "aws_security_group" "eks_nodes" {
  name        = "${var.project_name}-eks-nodes-sg"
  description = "Security group for EKS nodes"
  vpc_id      = local.vpc_id

  # Libera todo o tráfego de saída
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags        = { Name = "${var.project_name}-eks-nodes-sg" }
}

# ============================================
# Security Group - Lambda
# ============================================

resource "aws_security_group" "lambda" {
  name        = "${var.project_name}-lambda-sg"
  description = "Security group for the Authentication Lambda"
  vpc_id      = local.vpc_id

  # Libera todo o tráfego de saída para a Lambda acessar o RDS e outros serviços AWS
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = { Name = "${var.project_name}-lambda-sg" }
}


# ============================================
# Regras de Comunicação EKS
# ============================================

# Permite que o control plane do EKS se comunique com os nodes na porta 443
resource "aws_security_group_rule" "cluster_egress_to_nodes_https" {
  type                     = "egress"
  from_port                = 443
  to_port                  = 443
  protocol                 = "tcp"
  security_group_id        = aws_security_group.eks_cluster.id
  source_security_group_id = aws_security_group.eks_nodes.id
}

# Permite que os nodes recebam comunicação do control plane na porta 443
resource "aws_security_group_rule" "nodes_ingress_from_cluster_https" {
  type                     = "ingress"
  from_port                = 443
  to_port                  = 443
  protocol                 = "tcp"
  security_group_id        = aws_security_group.eks_nodes.id
  source_security_group_id = aws_security_group.eks_cluster.id
}
