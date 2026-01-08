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
# e exporta via TF_VAR_eks_cluster_role e TF_VAR_eks_node_role
#

variable "eks_cluster_role" {
  description = "IAM Role do Cluster EKS detectada automaticamente pelo script"
  type        = string
}

variable "eks_node_role" {
  description = "IAM Role do NodeGroup do EKS detectada automaticamente pelo script"
  type        = string
}

variable "lab_role" {
  description = "IAM Role do Laboratório AWS Academy"
  type        = string
  default     = "LabRole"
}

# ============================================
# Variáveis do EKS
# ============================================

variable "instance_types" {
  description = "Tipos de instância EC2 para os nodes"
  type        = list(string)
  default     = ["t3.medium"]
}

variable "lambda_execution_role_name" {
  description = "Nome do IAM Role para a execução da Lambda"
  type        = string
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
# Variáveis do ECR
# ============================================

# A variável ecr_repo_name foi removida. O nome agora é fixo
# O build da imagem foi desativado para contornar restrições do ECR.
# A imagem pública será usada diretamente.
# ============================================

variable "alb_hostname" {
  description = "Hostname do Application Load Balancer (ALB) criado pelo Ingress do EKS. Usado na segunda fase do apply."
  type        = string
  default     = ""
}

# ============================================
# Variáveis do Kubernetes
# ============================================

variable "replicas" {
  description = "Número de réplicas do deployment"
  type        = number
  default     = 2
}

variable "docker_image" {
  description = "Imagem Docker do MecanicaOS API"
  type        = string
  default     = "fthalita91/techchallenge-api:latest"
}

variable "docker_image_repo" {
  description = "Nome do repositório ECR para a imagem Docker"
  type        = string
  default     = "mecanicaos-ecr"
}

variable "docker_image_tag" {
  description = "Tag da imagem Docker no ECR. Se vazio, usa timestamp automático."
  type        = string
  default     = ""
}

# ============================================
# Variáveis do Banco de Dados (Supabase)
# ============================================

variable "db_host" {
  description = "Host do banco de dados PostgreSQL (Supabase)"
  type        = string
}

variable "db_port" {
  description = "Porta do banco de dados"
  type        = string
  default     = "5432"
}

variable "db_name" {
  description = "Nome do banco de dados"
  type        = string
  default     = "postgres"
}

variable "db_username" {
  description = "Usuário do banco de dados"
  type        = string
  default     = "postgres"
}

variable "db_password" {
  description = "Senha do banco de dados"
  type        = string
  sensitive   = true
}

# ============================================
# Variáveis do JWT
# ============================================

variable "jwt_secret_key" {
  description = "Chave secreta para geração de tokens JWT"
  type        = string
  sensitive   = true
}

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
# Variáveis do OpenTelemetry
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

variable "datadog_api_key" {
  description = "API Key do Datadog"
  type        = string
  sensitive   = true
}

variable "newrelic_license_key" {
  description = "License Key do New Relic"
  type        = string
  sensitive   = true
}
