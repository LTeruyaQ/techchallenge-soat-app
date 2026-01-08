# ============================================
# Subnets Públicas
# ============================================

resource "aws_subnet" "public" {
  count                   = 3
  vpc_id                  = aws_vpc.main.id
  cidr_block              = cidrsubnet(aws_vpc.main.cidr_block, 4, count.index + 10) # Offset to avoid conflict
  map_public_ip_on_launch = true
  availability_zone       = var.availability_zones[count.index]

  tags = {
    Name    = "${var.project_name}-subnet-${count.index + 1}"
    Project = "MecanicaOS"
  }
}

# ============================================
# Subnets Privadas
# ============================================

resource "aws_subnet" "private" {
  count             = 3
  vpc_id            = aws_vpc.main.id
  cidr_block        = cidrsubnet(aws_vpc.main.cidr_block, 8, count.index + 20) # Offset to avoid conflict
  availability_zone = var.availability_zones[count.index]

  tags = {
    Name    = "${var.project_name}-private-subnet-${count.index + 1}"
    Project = "MecanicaOS"
  }
}
