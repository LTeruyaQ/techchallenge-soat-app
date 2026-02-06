# ============================================
# VPC - Virtual Private Cloud
# ============================================

# Variável para receber o ID da VPC existente (se houver)
variable "existing_vpc_id" {
  description = "ID de uma VPC existente para reutilizar. Se vazio, uma nova VPC será criada."
  type        = string
  default     = ""
}

# Data source para obter dados de uma VPC existente
data "aws_vpc" "existing" {
  count = var.existing_vpc_id != "" ? 1 : 0
  id    = var.existing_vpc_id
}

# Recurso para criar uma nova VPC apenas se 'existing_vpc_id' estiver vazio
resource "aws_vpc" "main" {
  count                = var.existing_vpc_id == "" ? 1 : 0
  cidr_block           = var.vpc_cidr
  enable_dns_support   = true
  enable_dns_hostnames = true

  tags = {
    Name    = "${var.project_name}-vpc"
    Project = "MecanicaOS"
  }
}

# Local para unificar a referência à VPC, seja ela nova ou existente
locals {
  vpc_id   = var.existing_vpc_id != "" ? data.aws_vpc.existing[0].id : aws_vpc.main[0].id
  vpc_cidr = var.existing_vpc_id != "" ? data.aws_vpc.existing[0].cidr_block : aws_vpc.main[0].cidr_block
}
