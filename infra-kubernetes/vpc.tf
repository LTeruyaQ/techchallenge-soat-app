# ============================================
# VPC - Virtual Private Cloud
# ============================================

# Em vez de criar uma VPC, usamos um data source para descobrir a VPC existente
# onde o cluster EKS já está provisionado.
# A descoberta será feita pela tag "Project" com o valor "MecanicaOS".
data "aws_vpc" "main" {
  tags = {
    Project = "MecanicaOS"
  }
}
