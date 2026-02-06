# ============================================
# Route Tables
# ============================================

resource "aws_route_table" "public" {
  vpc_id = local.vpc_id

  route {
    // Rota padrão para tráfego local dentro da VPC
    cidr_block = local.vpc_cidr
    gateway_id = "local"
  }

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.igw.id
  }

  tags = {
    Name    = "${var.project_name}-rt-public"
    Project = "MecanicaOS"
  }
}

# Associação das subnets com a route table
resource "aws_route_table_association" "public" {
  count          = length(local.public_subnet_ids)
  subnet_id      = local.public_subnet_ids[count.index]
  route_table_id = aws_route_table.public.id
}
