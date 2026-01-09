# ============================================
# Security Groups
# ============================================

# SG para o control plane do EKS
resource "aws_security_group" "eks_cluster" {
  name        = "mecanicaos-eks-cluster-sg"
  description = "EKS control plane security group"
  vpc_id      = aws_vpc.main.id
  tags        = { Name = "mecanicaos-eks-cluster-sg" }
}

# SG para os worker nodes do EKS
resource "aws_security_group" "eks_nodes" {
  name        = "mecanicaos-eks-nodes-sg"
  description = "EKS worker nodes security group"
  vpc_id      = aws_vpc.main.id
  tags        = { Name = "mecanicaos-eks-nodes-sg" }
}

# SG para a instancia RDS
resource "aws_security_group" "rds" {
  name        = "mecanicaos-rds-sg"
  description = "RDS instance security group"
  vpc_id      = aws_vpc.main.id
  tags        = { Name = "mecanicaos-rds-sg" }
}

# ============================================
# Regras de Seguranca
# ============================================

# --- Regras Gerais ---

# Permite que os nos do EKS acessem a internet
resource "aws_security_group_rule" "nodes_egress_internet" {
  description       = "Nodes egress to internet"
  type              = "egress"
  from_port         = 0
  to_port           = 0
  protocol          = "-1"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = aws_security_group.eks_nodes.id
}

# --- EKS Cluster <-> Nodes ---

# Permite que o control plane se comunique com os nos
resource "aws_security_group_rule" "cluster_egress_to_nodes_ephemeral" {
  description                   = "Cluster to nodes for kubelet"
  type                          = "egress"
  from_port                     = 1025
  to_port                       = 65535
  protocol                      = "tcp"
  security_group_id             = aws_security_group.eks_cluster.id
  destination_security_group_id = aws_security_group.eks_nodes.id
}

# Permite que os nos recebam do control plane
resource "aws_security_group_rule" "nodes_ingress_from_cluster_ephemeral" {
  description              = "Nodes from cluster for kubelet"
  type                     = "ingress"
  from_port                = 1025
  to_port                  = 65535
  protocol                 = "tcp"
  security_group_id        = aws_security_group.eks_nodes.id
  source_security_group_id = aws_security_group.eks_cluster.id
}

# Permite que os nos se comuniquem com o control plane (API)
resource "aws_security_group_rule" "nodes_egress_to_cluster_api" {
  description                   = "Nodes to cluster API"
  type                          = "egress"
  from_port                     = 443
  to_port                       = 443
  protocol                      = "tcp"
  security_group_id             = aws_security_group.eks_nodes.id
  destination_security_group_id = aws_security_group.eks_cluster.id
}

# Permite que o control plane receba dos nos (API)
resource "aws_security_group_rule" "cluster_ingress_from_nodes_api" {
  description              = "Cluster from nodes API"
  type                     = "ingress"
  from_port                = 443
  to_port                  = 443
  protocol                 = "tcp"
  security_group_id        = aws_security_group.eks_cluster.id
  source_security_group_id = aws_security_group.eks_nodes.id
}

# --- Acesso ao RDS ---

# Permite que os nos do EKS (e a Lambda) acessem o RDS
resource "aws_security_group_rule" "rds_ingress_from_nodes_lambda" {
  description              = "RDS from EKS nodes and Lambda"
  type                     = "ingress"
  from_port                = 5432
  to_port                  = 5432
  protocol                 = "tcp"
  security_group_id        = aws_security_group.rds.id
  source_security_group_id = aws_security_group.eks_nodes.id # Lambda will also use this SG
}
