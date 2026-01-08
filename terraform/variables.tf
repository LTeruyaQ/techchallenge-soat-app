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

variable "vpc_id" {
  description = "ID da VPC a ser usada. Se fornecido, o Terraform buscará subnets dentro dela."
  type        = string
  default     = ""
}

# ============================================
# Variáveis IAM - AWS Academy
# ============================================

variable "eks_cluster_role" {
  description = "IAM Role do Cluster EKS (fixo no AWS Academy)"
  type        = string
  default     = "LabEksClusterRole"
}

variable "eks_node_role" {
  description = "IAM Role do NodeGroup do EKS (fixo no AWS Academy)"
  type        = string
  default     = "LabEksNodeRole"
}

variable "skip_create_eks" {
  description = "Se verdadeiro, pula a criação do cluster EKS e usa o existente."
  type        = bool
  default     = false
}

variable "use_existing_lambda_role" {
  description = "Controla o uso de uma IAM Role existente para a Lambda. Valores: 'true', 'false', 'unknown'."
  type        = string
  default     = "false"
}

variable "lambda_role_name" {
  description = "Nome da IAM Role existente para a Lambda, se aplicável."
  type        = string
  default     = ""
}

# ... (outras variáveis permanecem como estão)

variable "datadog_api_key" {
  description = "API Key do Datadog"
  type        = string
  sensitive   = true
  default     = ""
}

variable "newrelic_license_key" {
  description = "License Key do New Relic"
  type        = string
  sensitive   = true
  default     = ""
}

variable "db_host" {
  description = "Host do banco de dados"
  type        = string
  default     = ""
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
  default     = ""
}

variable "docker_image" {
  description = "Imagem Docker da aplicação"
  type        = string
  default     = "fthalita91/techchallenge-api:latest"
}

variable "lab_role" {
  description = "IAM Role do Laboratório AWS Academy"
  type        = string
  default     = "LabRole"
}

variable "alb_hostname" {
  description = "Hostname do Application Load Balancer (ALB)"
  type        = string
  default     = ""
}

variable "replicas" {
  description = "Número de réplicas do deployment"
  type        = number
  default     = 2
}

variable "jwt_secret_key" {
  description = "Chave secreta para JWT"
  type        = string
  sensitive   = true
  default     = ""
}

variable "jwt_issuer" {
  description = "Emissor do JWT"
  type        = string
  default     = "MecanicaOS"
}

variable "jwt_audience" {
  description = "Audiência do JWT"
  type        = string
  default     = "MecanicaOS-API"
}

variable "jwt_expiry_minutes" {
  description = "Tempo de expiração do JWT em minutos"
  type        = number
  default     = 120
}

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

variable "instance_types" {
  description = "Tipos de instância para os nós do EKS"
  type        = list(string)
  default     = ["t3.medium"]
}

variable "node_desired_size" {
  description = "Número desejado de nós"
  type        = number
  default     = 2
}

variable "node_max_size" {
  description = "Número máximo de nós"
  type        = number
  default     = 3
}

variable "node_min_size" {
  description = "Número mínimo de nós"
  type        = number
  default     = 1
}
