resource "aws_db_subnet_group" "rds" {
  name       = "mecanicaos-rds-subnet-group"
  subnet_ids = local.private_subnet_ids
}

resource "aws_db_instance" "default" {
  identifier             = "mecanicaos-db"
  allocated_storage      = 20
  engine                 = "postgres"
  engine_version         = "13"
  instance_class         = "db.t3.micro"
  username               = "mecanicaosadmin"
  password               = random_password.db_password.result
  db_subnet_group_name   = aws_db_subnet_group.rds.name
  vpc_security_group_ids = [aws_security_group.rds.id]
  skip_final_snapshot    = true
  publicly_accessible    = false
}
