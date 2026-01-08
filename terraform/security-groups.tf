# ============================================
# Security Groups
# ============================================

resource "aws_security_group" "eks_cluster" {
  name        = "${local.prefix}-eks-cluster-sg"
  description = "Security group para o control plane do EKS"
  vpc_id      = data.aws_vpc.existing.id

  tags = {
    Name = "${local.prefix}-eks-cluster-sg"
  }
}

resource "aws_security_group" "eks_nodes" {
  name        = "${local.prefix}-eks-node-sg"
  description = "Security group for EKS nodes"
  vpc_id      = data.aws_vpc.existing.id

  # Permite todo o tráfego de saída
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    "Name"                                      = "${local.prefix}-eks-node-sg"
    "kubernetes.io/cluster/${local.prefix}-eks" = "owned"
  }
}
