# ============================================
# Subnets Públicas
# ============================================

resource "aws_subnet" "public" {
  count                   = 3
  vpc_id                  = aws_vpc.main.id
  cidr_block              = cidrsubnet(aws_vpc.main.cidr_block, 8, count.index) # Ajustado para /24
  map_public_ip_on_launch = true
  availability_zone       = var.availability_zones[count.index]

  tags = {
    Name                      = "${var.project_name}-public-subnet-${count.index + 1}"
    "kubernetes.io/role/elb"  = "1"
    "kubernetes.io/cluster/${var.project_name}-eks" = "shared"
  }
}

# ============================================
# Subnets Privadas
# ============================================

resource "aws_subnet" "private" {
  count             = 3
  vpc_id            = aws_vpc.main.id
  cidr_block        = cidrsubnet(aws_vpc.main.cidr_block, 8, count.index + 3) # Ajustado para /24, continuação do range
  availability_zone = var.availability_zones[count.index]

  tags = {
    Name                              = "${var.project_name}-private-subnet-${count.index + 1}"
    "kubernetes.io/role/internal-elb" = "1"
    "kubernetes.io/cluster/${var.project_name}-eks" = "shared"
  }
}
