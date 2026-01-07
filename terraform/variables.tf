# ============================================
# Variáveis - AWS Academy
# ============================================

# ============================================
# Variáveis Gerais
# ============================================

variable "aws_region" {
  description = "Região da AWS (AWS Academy usa us-east-1)"
  type        = string
  default     = "us-east-1"
}

variable "project_name" {
  description = "Nome do projeto (usado em tags e nomes de recursos)"
  type        = string
  default     = "mecanicaos"
}

variable "environment" {
  description = "Ambiente de deploy"
  type        = string
  default     = "production"
}

# ============================================
# Variáveis de Rede
# ============================================

variable "vpc_cidr" {
  description = "CIDR block da VPC"
  type        = string
  default     = "10.0.0.0/16"
}

variable "availability_zones" {
  description = "Zonas de disponibilidade"
  type        = list(string)
  default     = ["us-east-1a", "us-east-1b", "us-east-1c"]
}

# ============================================
# Variáveis IAM - AWS Academy
# ============================================
#
# IMPORTANTE:
# Essas variáveis NÃO possuem default.
# O script de deploy detecta automaticamente
# e exporta via TF_VAR_eks_cluster_role_name e TF_VAR_eks_node_role_name
#

variable "eks_cluster_role_name" {
  description = "Nome da IAM Role do Cluster EKS (detectada automaticamente)"
  type        = string
}

variable "eks_node_role_name" {
  description = "Nome da IAM Role do NodeGroup do EKS (detectada automaticamente)"
  type        = string
}

# ============================================
# Variáveis do EKS
# ============================================

variable "instance_types" {
  description = "Tipos de instância EC2 para os nodes"
  type        = list(string)
  default     = ["t3.medium"]
}

variable "node_desired_size" {
  description = "Número desejado de nodes"
  type        = number
  default     = 2
}

variable "node_max_size" {
  description = "Número máximo de nodes"
  type        = number
  default     = 3
}

variable "node_min_size" {
  description = "Número mínimo de nodes"
  type        = number
  default     = 1
}

# ============================================
# Variáveis do Kubernetes
# ============================================

variable "replicas" {
  description = "Número de réplicas do deployment"
  type        = number
  default     = 2
}

variable "docker_image_repo" {
  description = "Nome do repositório ECR para a imagem Docker"
  type        = string
  default     = "mecanicaos-ecr"
}

variable "docker_image_url" {
  description = "URL completa da imagem Docker no ECR (passada pelo script de deploy)"
  type        = string
}

# ============================================
# Variáveis do Banco de Dados (RDS)
# ============================================

variable "rds_instance_class" {
  description = "Classe da instância RDS PostgreSQL (deve ser a menor possível para AWS Academy)"
  type        = string
  default     = "db.t3.micro"
}

variable "rds_allocated_storage" {
  description = "Espaço alocado para o RDS em GB"
  type        = number
  default     = 20
}

variable "rds_engine_version" {
  description = "Versão do motor PostgreSQL"
  type        = string
  default     = "15.3"
}

variable "rds_database_name" {
  description = "Nome do banco de dados inicial a ser criado no RDS"
  type        = string
  default     = "mecanicaosdb"
}

# ============================================
# Variáveis do JWT (gerenciado via Secrets Manager)
# ============================================

variable "jwt_issuer" {
  description = "Emissor do token JWT"
  type        = string
  default     = "MecanicaOS"
}

variable "jwt_audience" {
  description = "Audiência do token JWT"
  type        = string
  default     = "MecanicaOS-API"
}

variable "jwt_expiry_minutes" {
  description = "Tempo de expiração do token JWT em minutos"
  type        = number
  default     = 120
}

# ============================================
# Variáveis do OpenTelemetry (NÃO ALTERAR)
# ============================================

variable "otel_service_name" {
  description = "Nome do serviço para OpenTelemetry"
  type        = string
  default     = "mecanicaos-api"
}

variable "otel_exporter_otlp_endpoint" {
  description = "Endpoint do OpenTelemetry Collector"
  type        = string
  default     = "http://otel-collector.observability:4317"
}
