data "aws_subnets" "default" {
  filter {
    name   = "default-for-az"
    values = ["true"]
  }
}

data "external" "eks_cluster_role" {
  program = ["bash", "-c", "ROLE_ARN=$(aws iam list-roles --query 'Roles[?contains(RoleName, `LabEksClusterRole`)].Arn' --output text | head -n 1) && printf '{\"arn\": \"%s\"}' \"$ROLE_ARN\""]
}

resource "aws_eks_cluster" "demo" {
  name     = var.cluster_name
  role_arn = data.external.eks_cluster_role.result.arn

  vpc_config {
    subnet_ids = data.aws_subnets.default.ids
  }
}
