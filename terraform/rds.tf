# ===================================================================
# Recurso: RDS (PostgreSQL)
# Descrição: Cria a instância do banco de dados PostgreSQL no RDS.
# ===================================================================

# Gera uma senha aleatória para o banco de dados
resource "random_password" "db_master_password" {
  length  = 16
  special = true
}

# Armazena a senha gerada no AWS Secrets Manager
resource "aws_secretsmanager_secret" "db_password" {
  name = "${var.project_name}-db-master-password"
}

resource "aws_secretsmanager_secret_version" "db_password_version" {
  secret_id     = aws_secretsmanager_secret.db_password.id
  secret_string = random_password.db_master_password.result
}

# Grupo de subnets para o RDS, garantindo que ele seja implantado nas subnets privadas
resource "aws_db_subnet_group" "db_subnet_group" {
  name       = "${var.project_name}-db-subnet-group"
  subnet_ids = aws_subnet.private[*].id

  tags = {
    Name = "${var.project_name}-db-subnet-group"
  }
}

# Instância RDS PostgreSQL
resource "aws_db_instance" "postgres_db" {
  identifier           = "${var.project_name}-db"
  allocated_storage    = 20
  engine               = "postgres"
  engine_version       = "15.3"
  instance_class       = "db.t3.micro"
  username             = "admin"
  password             = random_password.db_master_password.result
  db_subnet_group_name = aws_db_subnet_group.db_subnet_group.name
  vpc_security_group_ids = [aws_security_group.rds.id]
  skip_final_snapshot  = true
  publicly_accessible  = false

  tags = {
    Name = "${var.project_name}-db"
  }
}

# ===================================================================
# Recurso: Security Group para o RDS
# Descrição: Define as regras de segurança para o RDS.
# ===================================================================

resource "aws_security_group" "rds" {
  name        = "${var.project_name}-rds-sg"
  description = "Permite acesso ao RDS a partir do EKS"
  vpc_id      = aws_vpc.main.id

  tags = {
    Name = "${var.project_name}-rds-sg"
  }
}

# Permite que os nós do EKS acessem o RDS na porta 5432
resource "aws_security_group_rule" "eks_to_rds" {
  type                     = "ingress"
  from_port                = 5432
  to_port                  = 5432
  protocol                 = "tcp"
  security_group_id        = aws_security_group.rds.id
  source_security_group_id = aws_security_group.eks_nodes.id
}
