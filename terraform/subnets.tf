# ============================================
# Data Sources for Subnets
# Discovers existing subnets created by the main deployment script.
# ============================================

data "aws_subnets" "public" {
  filter {
    name   = "vpc-id"
    values = [aws_vpc.main.id]
  }
  tags = {
    "kubernetes.io/role/elb" = "1"
  }
  depends_on = [aws_vpc.main]
}

data "aws_subnets" "private" {
  filter {
    name   = "vpc-id"
    values = [aws_vpc.main.id]
  }
  tags = {
    "kubernetes.io/role/internal-elb" = "1"
  }
  depends_on = [aws_vpc.main]
}
