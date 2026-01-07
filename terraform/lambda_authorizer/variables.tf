# lambda_authorizer/variables.tf

variable "jwt_secret" {
  description = "Segredo para assinar o JWT"
  type        = string
  sensitive   = true
}

variable "aws_region" {
  description = "Região da AWS"
  type        = string
}
