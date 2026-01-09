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
# Data Sources para Subnets Existentes
# ============================================

# Data source para buscar TODAS as subnets do projeto na VPC
data "aws_subnets" "all_project_subnets" {
  filter {
    name   = "vpc-id"
    values = [local.vpc_id]
  }
  filter {
    name   = "tag:Project"
    values = ["MecanicaOS"]
  }
}

# Data source para buscar especificamente as subnets PRIVADAS
data "aws_subnets" "private_existing" {
  filter {
    name   = "vpc-id"
    values = [local.vpc_id]
  }
  filter {
    name   = "tag:Name"
    values = ["${var.project_name}-private-subnet-*"]
  }
}

# ============================================
# Locals para Unificar IDs de Subnet
# ============================================

locals {
  # Determina os IDs das subnets privadas: usa as existentes se encontradas, senão usa as que serão criadas
  private_subnet_ids = length(var.existing_private_subnet_ids) > 0 ? var.existing_private_subnet_ids : (
    length(data.aws_subnets.private_existing.ids) > 0 ? tolist(data.aws_subnets.private_existing.ids) : aws_subnet.private.*.id
  )

  # Determina os IDs das subnets públicas: calcula a diferença entre todas e as privadas
  discovered_public_subnet_ids = tolist(setsubtract(toset(data.aws_subnets.all_project_subnets.ids), toset(data.aws_subnets.private_existing.ids)))

  public_subnet_ids = length(var.existing_public_subnet_ids) > 0 ? var.existing_public_subnet_ids : (
    length(local.discovered_public_subnet_ids) > 0 ? local.discovered_public_subnet_ids : aws_subnet.public.*.id
  )
}

# ============================================
# Subnets Públicas (Criação Condicional)
# ============================================

resource "aws_subnet" "public" {
  count = length(var.existing_public_subnet_ids) == 0 && length(local.discovered_public_subnet_ids) == 0 ? 3 : 0

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
  count = length(var.existing_private_subnet_ids) == 0 && length(data.aws_subnets.private_existing.ids) == 0 ? 3 : 0

  vpc_id            = local.vpc_id
  cidr_block        = cidrsubnet(local.vpc_cidr, 8, count.index + 3)
  availability_zone = var.availability_zones[count.index]

  tags = {
    Name    = "${var.project_name}-private-subnet-${count.index + 1}"
    Project = "MecanicaOS"
  }
}
