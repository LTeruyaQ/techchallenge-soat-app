# ============================================
# RDS (PostgreSQL)
# ============================================

resource "aws_db_subnet_group" "rds_subnet_group" {
  name       = "${var.project_name}-rds-subnet-group"
  subnet_ids = aws_subnet.private[*].id

  tags = {
    Name = "${var.project_name}-rds-subnet-group"
  }
}

resource "aws_db_instance" "main" {
  identifier           = "${var.project_name}-rds"
  allocated_storage    = 20
  storage_type         = "gp2"
  engine               = "postgres"
  engine_version       = "13.7"
  instance_class       = "db.t3.micro"
  db_name              = var.project_name
  username             = jsondecode(aws_secretsmanager_secret_version.rds_credentials_version.secret_string)["username"]
  password             = jsondecode(aws_secretsmanager_secret_version.rds_credentials_version.secret_string)["password"]
  db_subnet_group_name = aws_db_subnet_group.rds_subnet_group.name
  vpc_security_group_ids = [aws_security_group.rds.id]
  skip_final_snapshot  = true

  tags = {
    Name = "${var.project_name}-rds"
  }
}
