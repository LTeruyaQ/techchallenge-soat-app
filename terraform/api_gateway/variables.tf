variable "eks_service_url" {
  description = "URL do serviço LoadBalancer do EKS"
  type        = string
}

variable "lambda_authorizer_arn" {
  description = "ARN da função Lambda do Authorizer"
  type        = string
}

variable "lambda_authorizer_invoke_arn" {
  description = "ARN de invocação da função Lambda do Authorizer"
  type        = string
}
