# ============================================
# RDS - Managed Relational Database
# ============================================

# Cria um grupo de sub-redes para o RDS, garantindo que ele seja
# provisionado apenas nas sub-redes privadas.
resource "aws_db_subnet_group" "default" {
  name       = "${var.project_name}-rds-subnet-group"
  subnet_ids = data.aws_subnets.private.ids

  tags = {
    Name    = "${var.project_name}-rds-subnet-group"
    Project = "MecanicaOS"
  }
}

# Cria a instância do banco de dados PostgreSQL
resource "aws_db_instance" "default" {
  identifier             = "${var.project_name}-db"
  allocated_storage      = 20
  storage_type           = "gp2"
  engine                 = "postgres"
  engine_version         = "15.3"
  instance_class         = "db.t3.micro" # Menor instância para custo mínimo
  db_name                = var.db_name
  username               = var.db_user
  password               = var.db_password
  db_subnet_group_name   = aws_db_subnet_group.default.name
  vpc_security_group_ids = [aws_security_group.rds.id]
  skip_final_snapshot    = true
  multi_az               = false # Desabilitado para custo mínimo

  tags = {
    Name    = "${var.project_name}-db-instance"
    Project = "MecanicaOS"
  }
}

# Cria um Security Group específico para o RDS
resource "aws_security_group" "rds" {
  name        = "${var.project_name}-rds-sg"
  description = "Controle de acesso para a instância RDS"
  vpc_id      = data.aws_vpc.main.id

  tags = {
    Name    = "${var.project_name}-rds-sg"
    Project = "MecanicaOS"
  }
}
