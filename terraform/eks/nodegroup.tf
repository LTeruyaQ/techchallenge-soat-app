data "external" "eks_node_role" {
  program = ["bash", "-c", "ROLE_ARN=$(aws iam list-roles --query 'Roles[?contains(RoleName, `LabEksNodeRole`)].Arn' --output text | head -n 1) && printf '{\"arn\": \"%s\"}' \"$ROLE_ARN\""]
}

resource "aws_eks_node_group" "demo_nodes" {
  cluster_name    = aws_eks_cluster.demo.name
  node_group_name = "demo-node-group"
  node_role_arn   = data.external.eks_node_role.result.arn
  subnet_ids      = data.aws_subnets.default.ids

  instance_types = ["t3.small"]
  scaling_config {
    desired_size = var.node_count
    max_size     = var.node_count + 1
    min_size     = 1
  }
}
