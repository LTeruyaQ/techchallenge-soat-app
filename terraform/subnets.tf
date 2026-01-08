# ============================================
# Subnets Públicas
# ============================================

resource "aws_subnet" "public" {
  count                   = 2
  vpc_id                  = aws_vpc.main.id
  cidr_block              = cidrsubnet(aws_vpc.main.cidr_block, 8, count.index)
  map_public_ip_on_launch = true
  availability_zone       = var.availability_zones[count.index]

  tags = {
    Name                                      = "${var.project_name}-public-subnet-${count.index + 1}"
    "kubernetes.io/cluster/${local.prefix}-eks" = "shared"
    "kubernetes.io/role/elb"                  = "1"
  }
}

# ============================================
# Subnets Privadas
# ============================================

resource "aws_subnet" "private" {
  count             = 2
  vpc_id            = aws_vpc.main.id
  cidr_block        = cidrsubnet(aws_vpc.main.cidr_block, 8, count.index + 2) # Offset to avoid overlap
  availability_zone = var.availability_zones[count.index]

  tags = {
    Name                                      = "${var.project_name}-private-subnet-${count.index + 1}"
    "kubernetes.io/cluster/${local.prefix}-eks" = "shared"
    "kubernetes.io/role/internal-elb"         = "1"
  }
}
