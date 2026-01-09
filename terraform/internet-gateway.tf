# ============================================
# Internet Gateway
# ============================================

resource "aws_internet_gateway" "igw" {
  vpc_id = local.vpc_id

  tags = {
    Name    = "${var.project_name}-igw"
    Project = "MecanicaOS"
  }
}
