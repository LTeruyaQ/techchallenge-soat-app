# rds/main.tf

# 1. Obter o cluster EKS para descobrir a VPC
data "aws_eks_cluster" "cluster" {
  name = var.cluster_name
}

data "aws_vpc" "eks_vpc" {
  id = data.aws_eks_cluster.cluster.resources_vpc_config[0].vpc_id
}

# 2. Obter as subnets privadas da VPC do EKS
data "aws_subnets" "private" {
  filter {
    name   = "vpc-id"
    values = [data.aws_eks_cluster.cluster.resources_vpc_config[0].vpc_id]
  }
  tags = {
    "kubernetes.io/role/internal-elb" = "1" # Tag para subnets privadas
  }
}

# 3. Security Group para o RDS
resource "aws_security_group" "rds_sg" {
  name        = "rds-sg"
  description = "Permite acesso ao RDS a partir do EKS"
  vpc_id      = data.aws_eks_cluster.cluster.resources_vpc_config[0].vpc_id

  ingress {
    description = "PostgreSQL from EKS Nodes"
    from_port   = 5432
    to_port     = 5432
    protocol    = "tcp"
    # Permite acesso de qualquer recurso dentro da VPC do EKS
    cidr_blocks = [data.aws_vpc.eks_vpc.cidr_block]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

# 3. Subnet Group para o RDS
resource "aws_db_subnet_group" "rds_subnet_group" {
  name       = "rds-subnet-group"
  subnet_ids = data.aws_subnets.private.ids
}

# 4. Gerar senha aleatória para o RDS
resource "random_password" "db_password" {
  length  = 16
  special = true
}

# 5. Criar o segredo no AWS Secrets Manager
resource "aws_secretsmanager_secret" "rds_credentials" {
  name = "mecanicaos/rds-credentials"
}

resource "aws_secretsmanager_secret_version" "rds_credentials_version" {
  secret_id = aws_secretsmanager_secret.rds_credentials.id
  secret_string = jsonencode({
    username = var.db_username,
    password = random_password.db_password.result,
    engine   = "postgres",
    host     = aws_db_instance.default.address,
    port     = aws_db_instance.default.port,
    dbname   = var.db_name
  })
}

# 6. Instância RDS PostgreSQL
resource "aws_db_instance" "default" {
  identifier           = "mecanicaos-db"
  allocated_storage    = 20
  storage_type         = "gp2"
  engine               = "postgres"
  engine_version       = "15.3"
  instance_class       = "db.t3.micro"
  db_name              = var.db_name
  username             = var.db_username
  password             = random_password.db_password.result
  db_subnet_group_name = aws_db_subnet_group.rds_subnet_group.name
  vpc_security_group_ids = [aws_security_group.rds_sg.id]
  skip_final_snapshot  = true
  publicly_accessible  = false
}
