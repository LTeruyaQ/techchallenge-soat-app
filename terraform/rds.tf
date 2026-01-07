# ============================================
# AWS RDS PostgreSQL
# ============================================

resource "aws_db_subnet_group" "rds" {
  name       = "${var.project_name}-rds-subnet-group"
  subnet_ids = aws_subnet.private[*].id

  tags = {
    Name = "${var.project_name}-rds-subnet-group"
  }
}

resource "aws_db_instance" "postgres" {
  allocated_storage    = var.rds_allocated_storage
  engine               = "postgres"
  engine_version       = var.rds_engine_version
  instance_class       = var.rds_instance_class
  db_name              = var.rds_database_name
  username             = jsondecode(aws_secretsmanager_secret_version.rds_credentials.secret_string).username
  password             = jsondecode(aws_secretsmanager_secret_version.rds_credentials.secret_string).password
  db_subnet_group_name = aws_db_subnet_group.rds.name
  vpc_security_group_ids = [aws_security_group.eks_nodes.id]
  skip_final_snapshot  = true
  publicly_accessible  = false

  tags = {
    Name = "${var.project_name}-rds"
  }
}
