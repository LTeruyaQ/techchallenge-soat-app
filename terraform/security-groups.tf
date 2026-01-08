# ============================================
# Security Groups
# Definições básicas para EKS e RDS.
# As regras de comunicação são gerenciadas separadamente
# para evitar ciclos de dependência.
# ============================================

resource "aws_security_group" "eks_cluster" {
  name        = "${local.prefix}-eks-cluster-sg"
  description = "Security group for the EKS control plane. Rules are managed separately."
  vpc_id      = local.vpc_id

  tags = {
    Name = "${local.prefix}-eks-cluster-sg"
  }
}

resource "aws_security_group" "eks_nodes" {
  name        = "${local.prefix}-eks-node-sg"
  description = "Security group for the EKS worker nodes. Rules are managed separately."
  vpc_id      = local.vpc_id

  tags = {
    Name                                      = "${local.prefix}-eks-node-sg"
    "kubernetes.io/cluster/${local.prefix}-eks" = "owned"
  }
}

# ============================================
# Regras Explícitas de Security Group para EKS
# ============================================

# Regra: Worker nodes podem iniciar comunicação com o Control Plane na porta HTTPS
resource "aws_security_group_rule" "node_to_cluster_https" {
  type                     = "egress"
  from_port                = 443
  to_port                  = 443
  protocol                 = "tcp"
  source_security_group_id = aws_security_group.eks_cluster.id
  security_group_id        = aws_security_group.eks_nodes.id
  description              = "Allow nodes to connect to the EKS control plane API"
}

# Regra: Control Plane pode se comunicar com os kubelets dos nós
resource "aws_security_group_rule" "cluster_to_node_kubelet" {
  type                     = "egress"
  from_port                = 1025 # Kubelet e portas de workload
  to_port                  = 65535
  protocol                 = "tcp"
  source_security_group_id = aws_security_group.eks_nodes.id
  security_group_id        = aws_security_group.eks_cluster.id
  description              = "Allow EKS control plane to connect to node kubelets and workloads"
}

# Regra: Nós podem receber tráfego do Control Plane
resource "aws_security_group_rule" "node_ingress_from_cluster" {
  type                     = "ingress"
  from_port                = 1025
  to_port                  = 65535
  protocol                 = "tcp"
  source_security_group_id = aws_security_group.eks_cluster.id
  security_group_id        = aws_security_group.eks_nodes.id
  description              = "Allow nodes to receive traffic from the EKS control plane"
}

# Regra: Saída de tráfego geral para os nós (para puxar imagens, etc.)
resource "aws_security_group_rule" "node_egress_internet" {
  type              = "egress"
  from_port         = 0
  to_port           = 0
  protocol          = "-1"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = aws_security_group.eks_nodes.id
  description       = "Allow nodes to connect to the internet"
}
