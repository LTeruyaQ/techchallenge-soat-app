# ============================================
# Detecção de Subnets Existentes
# ============================================

# Procura por subnets públicas existentes com as tags esperadas
data "aws_subnets" "existing_public" {
  filter {
    name   = "vpc-id"
    values = [aws_vpc.main.id]
  }
  filter {
    name   = "tag:Name"
    values = ["${var.project_name}-subnet-1", "${var.project_name}-subnet-2", "${var.project_name}-subnet-3"]
  }
}

# Procura por subnets privadas existentes com as tags esperadas
data "aws_subnets" "existing_private" {
  filter {
    name   = "vpc-id"
    values = [aws_vpc.main.id]
  }
  filter {
    name   = "tag:Name"
    values = ["${var.project_name}-private-subnet-1", "${var.project_name}-private-subnet-2", "${var.project_name}-private-subnet-3"]
  }
}

# ============================================
# Criação Condicional de Subnets
# ============================================

# Cria as subnets públicas somente se não forem encontradas
resource "aws_subnet" "public" {
  count                   = length(data.aws_subnets.existing_public.ids) == 0 ? 3 : 0
  vpc_id                  = aws_vpc.main.id
  cidr_block              = cidrsubnet(aws_vpc.main.cidr_block, 4, count.index)
  map_public_ip_on_launch = true
  availability_zone       = var.availability_zones[count.index]

  tags = {
    Name                      = "${var.project_name}-subnet-${count.index + 1}"
    Project                   = "MecanicaOS"
    "kubernetes.io/role/elb"  = "1"
  }
}

# Cria as subnets privadas somente se não forem encontradas
resource "aws_subnet" "private" {
  count             = length(data.aws_subnets.existing_private.ids) == 0 ? 3 : 0
  vpc_id            = aws_vpc.main.id
  cidr_block        = cidrsubnet(aws_vpc.main.cidr_block, 8, count.index + 3)
  availability_zone = var.availability_zones[count.index]

  tags = {
    Name                              = "${var.project_name}-private-subnet-${count.index + 1}"
    Project                           = "MecanicaOS"
    "kubernetes.io/role/internal-elb" = "1"
  }
}

# ============================================
# Locals para Unificar Referências
# ============================================

locals {
  # Usa as subnets encontradas se existirem, caso contrário, usa as que foram criadas
  public_subnet_ids  = length(data.aws_subnets.existing_public.ids) > 0 ? data.aws_subnets.existing_public.ids : aws_subnet.public[*].id
  private_subnet_ids = length(data.aws_subnets.existing_private.ids) > 0 ? data.aws_subnets.existing_private.ids : aws_subnet.private[*].id
}
