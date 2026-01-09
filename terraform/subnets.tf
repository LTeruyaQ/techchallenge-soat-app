# ============================================
# Variáveis de Subnets
# ============================================

variable "existing_public_subnet_ids" {
  description = "Lista de IDs de subnets públicas existentes. Se vazia, novas subnets serão criadas."
  type        = list(string)
  default     = []
}

variable "existing_private_subnet_ids" {
  description = "Lista de IDs de subnets privadas existentes. Se vazia, novas subnets serão criadas."
  type        = list(string)
  default     = []
}

# ============================================
# Subnets Públicas (Criação Condicional)
# ============================================

resource "aws_subnet" "public" {
  count                   = length(var.existing_public_subnet_ids) == 0 ? 3 : 0
  vpc_id                  = local.vpc_id
  cidr_block              = cidrsubnet(local.vpc_cidr, 4, count.index)
  map_public_ip_on_launch = true
  availability_zone       = var.availability_zones[count.index]

  tags = {
    Name    = "${var.project_name}-subnet-${count.index + 1}"
    Project = "MecanicaOS"
  }
}

# ============================================
# Subnets Privadas (Criação Condicional)
# ============================================

resource "aws_subnet" "private" {
  count             = length(var.existing_private_subnet_ids) == 0 ? 3 : 0
  vpc_id            = local.vpc_id
  cidr_block        = cidrsubnet(local.vpc_cidr, 8, count.index + 3)
  availability_zone = var.availability_zones[count.index]

  tags = {
    Name    = "${var.project_name}-private-subnet-${count.index + 1}"
    Project = "MecanicaOS"
  }
}

# ============================================
# Locals para Unificar IDs de Subnet
# ============================================

locals {
  public_subnet_ids  = length(var.existing_public_subnet_ids) > 0 ? var.existing_public_subnet_ids : aws_subnet.public.*.id
  private_subnet_ids = length(var.existing_private_subnet_ids) > 0 ? var.existing_private_subnet_ids : aws_subnet.private.*.id
}
