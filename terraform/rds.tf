resource "random_password" "db_password" {
  length  = 16
  special = true
}

resource "aws_secretsmanager_secret" "db_credentials" {
  name = "${local.prefix}-db-credentials"
}

resource "aws_secretsmanager_secret_version" "db_credentials" {
  secret_id = aws_secretsmanager_secret.db_credentials.id
  secret_string = jsonencode({
    username = "mecanicaosadmin"
    password = random_password.db_password.result
  })
}

resource "aws_security_group" "rds" {
  name        = "${local.prefix}-rds-sg"
  description = "Allow access to RDS from Lambda and EKS"
  vpc_id      = data.aws_vpc.lab_vpc.id

  ingress {
    from_port       = 5432
    to_port         = 5432
    protocol        = "tcp"
    security_groups = [aws_security_group.eks_nodes.id]
  }

  ingress {
    from_port = 5432
    to_port   = 5432
    protocol  = "tcp"
    self      = true # Allows traffic from resources in this same SG
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

resource "aws_db_subnet_group" "rds" {
  name       = "${local.prefix}-rds-subnet-group"
  subnet_ids = data.aws_subnets.private.ids
}

resource "aws_db_instance" "default" {
  identifier             = "${local.prefix}-db"
  allocated_storage      = 20
  engine                 = "postgres"
  engine_version         = "13.7"
  instance_class         = "db.t3.micro"
  username               = jsondecode(aws_secretsmanager_secret_version.db_credentials.secret_string).username
  password               = jsondecode(aws_secretsmanager_secret_version.db_credentials.secret_string).password
  db_subnet_group_name   = aws_db_subnet_group.rds.name
  vpc_security_group_ids = [aws_security_group.rds.id]
  skip_final_snapshot    = true
  publicly_accessible    = false
}
