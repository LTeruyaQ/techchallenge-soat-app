# ============================================
# Grupo de Subnets para o RDS
# ============================================

resource "aws_db_subnet_group" "rds" {
  name       = "${var.project_name}-rds-subnet-group"
  subnet_ids = local.private_subnet_ids

  tags = {
    Name = "${var.project_name}-rds-subnet-group"
  }
}

# ============================================
# Security Group para o RDS
# ============================================

resource "aws_security_group" "rds" {
  name        = "${var.project_name}-rds-sg"
  description = "Permite acesso ao RDS a partir do EKS e da Lambda"
  vpc_id      = local.vpc_id

  # Libera todo o tráfego de saída (necessário para updates, etc.)
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "${var.project_name}-rds-sg"
  }
}

# Regra para permitir acesso dos Nodes do EKS
resource "aws_security_group_rule" "rds_from_eks" {
  type                     = "ingress"
  from_port                = 5432
  to_port                  = 5432
  protocol                 = "tcp"
  security_group_id        = aws_security_group.rds.id
  source_security_group_id = aws_security_group.eks_nodes.id # SG dos nodes do EKS
}

# Regra para permitir acesso da função Lambda
resource "aws_security_group_rule" "rds_from_lambda" {
  type                     = "ingress"
  from_port                = 5432
  to_port                  = 5432
  protocol                 = "tcp"
  security_group_id        = aws_security_group.rds.id
  source_security_group_id = aws_security_group.lambda.id # SG da Lambda
}

# ============================================
# Instância do RDS PostgreSQL
# ============================================

resource "aws_db_instance" "default" {
  identifier             = "${var.project_name}-db"
  allocated_storage      = 20
  engine                 = "postgres"
  engine_version         = "13"
  instance_class         = "db.t3.micro" # Menor instância disponível
  db_name                = var.db_name
  username               = var.db_username
  password               = local.db_password
  db_subnet_group_name   = aws_db_subnet_group.rds.name
  vpc_security_group_ids = [aws_security_group.rds.id]
  skip_final_snapshot    = true
  publicly_accessible    = false

  tags = {
    Name = "${var.project_name}-db-instance"
  }
}
