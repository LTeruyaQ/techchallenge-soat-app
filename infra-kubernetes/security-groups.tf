# ============================================
# Security Groups
# ============================================

# Security Group para o Load Balancer ou Ingress do EKS
resource "aws_security_group" "eks_lb" {
  name        = "${var.project_name}-lb-sg"
  description = "Security group para o Load Balancer do EKS"
  vpc_id      = data.aws_vpc.main.id

  # HTTP
  ingress {
    description = "HTTP"
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # HTTPS
  ingress {
    description = "HTTPS"
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # Egress - permitir todo tráfego de saída
  egress {
    description = "All outbound"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name    = "${var.project_name}-lb-sg"
    Project = "MecanicaOS"
  }
}

# Security Group para os Worker Nodes do EKS
resource "aws_security_group" "eks_nodes" {
  name        = "${var.project_name}-nodes-sg"
  description = "Security group para os worker nodes do EKS"
  vpc_id      = data.aws_vpc.main.id

  # Egress - permitir todo tráfego de saída
  egress {
    description = "All outbound"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name    = "${var.project_name}-nodes-sg"
    Project = "MecanicaOS"
  }
}

# Adiciona uma regra ao Security Group do RDS para permitir a conexão dos nós do EKS
resource "aws_security_group_rule" "eks_to_rds" {
  type                     = "ingress"
  from_port                = 5432 # Porta do PostgreSQL
  to_port                  = 5432
  protocol                 = "tcp"
  security_group_id        = aws_security_group.rds.id
  source_security_group_id = aws_security_group.eks_nodes.id
  description              = "Permite a conexão dos worker nodes do EKS ao RDS"
}
