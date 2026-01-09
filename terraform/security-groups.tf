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

  # Regras de Saída (Egress) - Permite acesso a internet
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

# SG para a instancia RDS
resource "aws_security_group" "rds" {
  name        = "mecanicaos-rds-sg"
  description = "RDS instance security group"
  vpc_id      = aws_vpc.main.id
  tags        = { Name = "mecanicaos-rds-sg" }
}

# ============================================
# Regras de Seguranca - EKS Cluster <-> Nodes
# ============================================

# Permite que o control plane envie tráfego para os nós (para o kubelet)
resource "aws_security_group_rule" "cluster_egress_to_nodes" {
  description                   = "Cluster to nodes for kubelet"
  type                          = "egress"
  from_port                     = 1025
  to_port                       = 65535
  protocol                      = "tcp"
  security_group_id             = aws_security_group.eks_cluster.id
  destination_security_group_id = aws_security_group.eks_nodes.id
}

# Permite que os nós recebam tráfego do control plane (para o kubelet)
resource "aws_security_group_rule" "nodes_ingress_from_cluster" {
  description              = "Nodes from cluster for kubelet"
  type                     = "ingress"
  from_port                = 1025
  to_port                  = 65535
  protocol                 = "tcp"
  security_group_id        = aws_security_group.eks_nodes.id
  source_security_group_id = aws_security_group.eks_cluster.id
}

# Permite que os nós enviem tráfego para o control plane (API server)
resource "aws_security_group_rule" "nodes_egress_to_cluster" {
  description                   = "Nodes to cluster API"
  type                          = "egress"
  from_port                     = 443
  to_port                       = 443
  protocol                      = "tcp"
  security_group_id             = aws_security_group.eks_nodes.id
  destination_security_group_id = aws_security_group.eks_cluster.id
}

# Permite que o control plane receba tráfego dos nós (API server)
resource "aws_security_group_rule" "cluster_ingress_from_nodes" {
  description              = "Cluster from nodes API"
  type                     = "ingress"
  from_port                = 443
  to_port                  = 443
  protocol                 = "tcp"
  security_group_id        = aws_security_group.eks_cluster.id
  source_security_group_id = aws_security_group.eks_nodes.id
}

# ============================================
# Regras de Seguranca - Acesso ao RDS
# ============================================

# Permite que os nós do EKS (e a Lambda, que usará o mesmo SG) acessem o RDS na porta do Postgres
resource "aws_security_group_rule" "rds_ingress_from_nodes" {
  description              = "RDS from EKS nodes and Lambda"
  type                     = "ingress"
  from_port                = 5432
  to_port                  = 5432
  protocol                 = "tcp"
  security_group_id        = aws_security_group.rds.id
  source_security_group_id = aws_security_group.eks_nodes.id
}
