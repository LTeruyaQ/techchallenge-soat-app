variable "project_name" {
  description = "The name of the project"
  default     = "mecanicaos"
}

variable "eks_cluster_role_arn" {
  description = "The ARN of the IAM role for the EKS cluster"
  type        = string
}

variable "eks_node_role_arn" {
  description = "The ARN of the IAM role for the EKS node group"
  type        = string
}

variable "alb_dns_name" {
  description = "The DNS name of the Application Load Balancer"
  type        = string
  default     = ""
}
