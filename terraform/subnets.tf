# ============================================
# Subnets
# Cria subnets públicas e privadas para a VPC
# ============================================

# Data source para buscar a VPC, caso um ID seja passado pelo script
data "aws_vpc" "existing_vpc" {
  count = var.vpc_id != "" ? 1 : 0
  id    = var.vpc_id
}

# Data source para buscar subnets privadas na VPC existente
data "aws_subnets" "existing_private_subnets" {
  count = var.vpc_id != "" ? 1 : 0
  filter {
    name   = "vpc-id"
    values = [data.aws_vpc.existing_vpc[0].id]
  }
  # Adicione tags se suas subnets existentes tiverem um padrão de nomenclatura
  tags = {
    "Tier" = "Private"
  }
}


# Unifica a referência ao ID da VPC
locals {
  vpc_id = var.vpc_id == "" ? aws_vpc.main.id : data.aws_vpc.existing_vpc[0].id
}

# Cria as subnets somente se uma vpc_id existente NÃO foi passada
resource "aws_subnet" "public" {
  count = var.vpc_id == "" ? 2 : 0

  vpc_id                  = local.vpc_id
  cidr_block              = cidrsubnet(var.vpc_cidr, 8, count.index)
  map_public_ip_on_launch = true
  availability_zone       = var.availability_zones[count.index]

  tags = {
    Name                                      = "${var.project_name}-public-subnet-${count.index + 1}"
    "kubernetes.io/cluster/${local.prefix}-eks" = "shared"
    "kubernetes.io/role/elb"                  = "1"
  }
}

resource "aws_subnet" "private" {
  count = var.vpc_id == "" ? 2 : 0

  vpc_id            = local.vpc_id
  cidr_block        = cidrsubnet(var.vpc_cidr, 8, count.index + 2) # Offset
  availability_zone = var.availability_zones[count.index]

  tags = {
    Name                                      = "${var.project_name}-private-subnet-${count.index + 1}"
    "kubernetes.io/cluster/${local.prefix}-eks" = "shared"
    "kubernetes.io/role/internal-elb"         = "1"
    "Tier"                                    = "Private"
  }
}
