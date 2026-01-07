# rds/variables.tf

variable "db_name" {
  description = "Nome do banco de dados"
  type        = string
  default     = "mecanicaosdb"
}

variable "db_username" {
  description = "Usuário master do banco de dados"
  type        = string
  default     = "mecanicaos"
}

variable "cluster_name" {
  description = "Nome do cluster EKS para descobrir a VPC"
  type        = string
}
