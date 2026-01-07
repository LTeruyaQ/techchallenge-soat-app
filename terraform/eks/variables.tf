variable "cluster_name" {
  description = "Nome do cluster EKS"
  type        = string
}

variable "node_count" {
  description = "Número de nós no node group"
  type        = number
  default     = 1
}

variable "docker_image" {
  description = "Imagem Docker do MecanicaOS API"
  type        = string
}

variable "db_credentials_secret_arn" {
  description = "ARN do segredo do RDS no Secrets Manager"
  type        = string
}

variable "jwt_secret" {
  description = "Segredo para assinar o JWT"
  type        = string
  sensitive   = true
}
