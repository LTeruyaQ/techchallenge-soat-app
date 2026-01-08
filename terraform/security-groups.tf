resource "aws_security_group" "eks_cluster" {
  name        = "mecanicaos-eks-cluster-sg"
  description = "Security group for EKS control plane"
  vpc_id      = aws_vpc.main.id
  tags        = { Name = "mecanicaos-eks-cluster-sg" }
}

resource "aws_security_group" "eks_nodes" {
  name        = "mecanicaos-eks-nodes-sg"
  description = "Security group for EKS nodes"
  vpc_id      = aws_vpc.main.id
  tags        = { Name = "mecanicaos-eks-nodes-sg" }
}

resource "aws_security_group" "rds" {
  name        = "mecanicaos-rds-sg"
  description = "Security group for RDS"
  vpc_id      = aws_vpc.main.id
  tags        = { Name = "mecanicaos-rds-sg" }
}

# Regras de comunicação
resource "aws_security_group_rule" "nodes_egress_internet" {
  type              = "egress"
  from_port         = 0
  to_port           = 0
  protocol          = "-1"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = aws_security_group.eks_nodes.id
}

resource "aws_security_group_rule" "cluster_to_nodes_https" {
  type                     = "egress"
  from_port                = 443
  to_port                  = 443
  protocol                 = "tcp"
  source_security_group_id = aws_security_group.eks_nodes.id
  security_group_id        = aws_security_group.eks_cluster.id
}

resource "aws_security_group_rule" "nodes_from_cluster_https" {
  type                     = "ingress"
  from_port                = 443
  to_port                  = 443
  protocol                 = "tcp"
  source_security_group_id = aws_security_group.eks_cluster.id
  security_group_id        = aws_security_group.eks_nodes.id
}

resource "aws_security_group_rule" "rds_from_nodes" {
  type                     = "ingress"
  from_port                = 5432
  to_port                  = 5432
  protocol                 = "tcp"
  source_security_group_id = aws_security_group.eks_nodes.id
  security_group_id        = aws_security_group.rds.id
}
