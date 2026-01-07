# ============================================
# Data Sources
# ============================================

data "aws_vpc" "main" {
  tags = {
    Name = "viva-real-vpc"
  }
}

data "aws_subnets" "private" {
  filter {
    name   = "vpc-id"
    values = [data.aws_vpc.main.id]
  }

  tags = {
    Tier = "Private"
  }
}

data "aws_subnets" "public" {
  filter {
    name   = "vpc-id"
    values = [data.aws_vpc.main.id]
  }

  tags = {
    Tier = "Public"
  }
}

data "aws_iam_role" "eks_cluster_role" {
  name = "LabRole"
}

data "aws_iam_role" "eks_node_role" {
  name = "LabRole"
}

data "aws_caller_identity" "current" {}
