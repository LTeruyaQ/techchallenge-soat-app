# ============================================
# Subnets
# ============================================

# Descobre as sub-redes privadas existentes na VPC.
# Assumimos que elas estão tagueadas com Tier = "Private".
data "aws_subnets" "private" {
  filter {
    name   = "vpc-id"
    values = [data.aws_vpc.main.id]
  }

  tags = {
    Tier = "Private"
  }
}

# Descobre as sub-redes públicas existentes na VPC.
# Assumimos que elas estão tagueadas com Tier = "Public".
data "aws_subnets" "public" {
  filter {
    name   = "vpc-id"
    values = [data.aws_vpc.main.id]
  }

  tags = {
    Tier = "Public"
  }
}
