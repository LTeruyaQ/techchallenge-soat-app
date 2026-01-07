variable "aws_region" {
  description = "Região da AWS para provisionar os recursos."
  type        = string
  default     = "us-east-1"
}

variable "project_name" {
  description = "Nome do projeto, usado para taguear recursos."
  type        = string
  default     = "mecanicaos"
}

variable "db_name" {
  description = "Nome do banco de dados."
  type        = string
}

variable "db_user" {
  description = "Usuário do banco de dados."
  type        = string
}

variable "db_password" {
  description = "Senha do banco de dados."
  type        = string
  sensitive   = true
}

# --- Data Sources para desacoplamento ---

data "aws_vpc" "main" {
  tags = {
    Project = "MecanicaOS"
  }
}

data "aws_subnets" "private" {
  filter {
    name   = "vpc-id"
    values = [data.aws_vpc.main.id]
  }
  tags = {
    Tier = "Private"
  }
}
