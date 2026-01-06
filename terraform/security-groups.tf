# ===================================================================
# Recurso: Security Group para o Cluster EKS
# Descrição: Controla o tráfego para o control plane do EKS.
# ===================================================================

resource "aws_security_group" "eks_cluster" {
  name        = "${var.project_name}-eks-cluster-sg"
  description = "Security group para o control plane do EKS."
  vpc_id      = aws_vpc.main.id

  tags = {
    Name = "${var.project_name}-eks-cluster-sg"
  }
}

# ===================================================================
# Recurso: Security Group para os Worker Nodes do EKS
# Descrição: Controla o tráfego para os nós de trabalho (worker nodes).
# ===================================================================

resource "aws_security_group" "eks_nodes" {
  name        = "${var.project_name}-eks-nodes-sg"
  description = "Security group para os worker nodes do EKS."
  vpc_id      = aws_vpc.main.id

  # Permite todo o tráfego de saída
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "${var.project_name}-eks-nodes-sg"
  }
}

# Permite que o control plane se comunique com os nós na porta HTTPS
resource "aws_security_group_rule" "cluster_to_nodes_https" {
  type                     = "ingress"
  from_port                = 443
  to_port                  = 443
  protocol                 = "tcp"
  security_group_id        = aws_security_group.eks_nodes.id
  source_security_group_id = aws_security_group.eks_cluster.id
}

# Permite que os nós se comuniquem com o control plane na porta HTTPS
resource "aws_security_group_rule" "nodes_to_cluster_https" {
  type                     = "ingress"
  from_port                = 443
  to_port                  = 443
  protocol                 = "tcp"
  security_group_id        = aws_security_group.eks_cluster.id
  source_security_group_id = aws_security_group.eks_nodes.id
}
