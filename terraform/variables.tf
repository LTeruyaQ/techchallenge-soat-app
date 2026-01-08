# ============================================
# Variáveis Globais e de Projeto
# ============================================
variable "aws_region" {
  description = "Região da AWS para o deploy."
  type        = string
  default     = "us-east-1"
}

variable "project_name" {
  description = "Nome do projeto, usado como prefixo para recursos."
  type        = string
  default     = "mecanicaos"
}

variable "environment" {
  description = "Ambiente do deploy (ex: production, staging)."
  type        = string
  default     = "production"
}

# ============================================
# Variáveis de Rede (VPC e Subnets)
# ============================================
variable "vpc_cidr" {
  description = "Bloco CIDR para a nova VPC."
  type        = string
  default     = "10.0.0.0/16"
}

variable "availability_zones" {
  description = "Zonas de disponibilidade a serem usadas."
  type        = list(string)
  default     = ["us-east-1a", "us-east-1b"]
}

variable "vpc_id" {
  description = "ID de uma VPC existente para reutilizar (opcional)."
  type        = string
  default     = ""
}

# ============================================
# Variáveis de IAM (Específicas do AWS Academy)
# ============================================
variable "eks_cluster_role" {
  description = "Nome da IAM Role para o Control Plane do EKS."
  type        = string
  default     = "LabEksClusterRole"
}

variable "eks_node_role" {
  description = "Nome da IAM Role para os Worker Nodes do EKS."
  type        = string
  default     = "LabEksNodeRole"
}

variable "use_existing_lambda_role" {
  description = "Flag para controlar a reutilização de uma IAM Role para a Lambda."
  type        = string
  default     = "false"
}

variable "lambda_role_name" {
  description = "Nome da IAM Role da Lambda a ser reutilizada."
  type        = string
  default     = ""
}

variable "lab_role" {
  description = "Nome da IAM Role do Laboratório para acesso geral."
  type        = string
  default     = "LabRole"
}

# ============================================
# Variáveis do EKS
# ============================================
variable "skip_create_eks" {
  description = "Flag para pular a criação do cluster EKS se ele já existir."
  type        = bool
  default     = false
}

variable "instance_types" {
  description = "Tipos de instância para os worker nodes."
  type        = list(string)
  default     = ["t3.medium"]
}

variable "node_desired_size" {
  description = "Número desejado de worker nodes."
  type        = number
  default     = 2
}

variable "node_max_size" {
  description = "Número máximo de worker nodes."
  type        = number
  default     = 3
}

variable "node_min_size" {
  description = "Número mínimo de worker nodes."
  type        = number
  default     = 1
}

# ============================================
# Variáveis da Aplicação e Kubernetes
# ============================================
variable "docker_image" {
  description = "Imagem Docker da aplicação a ser implantada."
  type        = string
  default     = "fthalita91/techchallenge-api:latest"
}

variable "replicas" {
  description = "Número de réplicas para o deployment no Kubernetes."
  type        = number
  default     = 2
}

variable "alb_hostname" {
  description = "Hostname do ALB (gerado pelo Ingress), usado na segunda fase do apply."
  type        = string
  default     = ""
}

# ============================================
# Variáveis do Banco de Dados (RDS)
# ============================================
variable "db_host" {
  description = "Host do banco de dados (gerado pelo RDS)."
  type        = string
  default     = ""
}

variable "db_port" {
  description = "Porta do banco de dados."
  type        = string
  default     = "5432"
}

variable "db_name" {
  description = "Nome do banco de dados."
  type        = string
  default     = "postgres"
}

variable "db_username" {
  description = "Usuário do banco de dados."
  type        = string
  default     = "postgres"
}

variable "db_password" {
  description = "Senha para o banco de dados RDS."
  type        = string
  sensitive   = true
}

# ============================================
# Variáveis de JWT e Observabilidade
# ============================================
variable "jwt_secret_key" {
  description = "Chave secreta para a assinatura de tokens JWT."
  type        = string
  sensitive   = true
}

variable "jwt_issuer" {
  description = "Emissor (issuer) para os tokens JWT."
  type        = string
  default     = "MecanicaOS"
}

variable "jwt_audience" {
  description = "Audiência (audience) para os tokens JWT."
  type        = string
  default     = "MecanicaOS-API"
}

variable "jwt_expiry_minutes" {
  description = "Tempo de expiração dos tokens JWT em minutos."
  type        = number
  default     = 120
}

variable "otel_service_name" {
  description = "Nome do serviço para o OpenTelemetry."
  type        = string
  default     = "mecanicaos-api"
}

variable "otel_exporter_otlp_endpoint" {
  description = "Endpoint do collector OpenTelemetry."
  type        = string
  default     = "http://otel-collector.observability:4317"
}

variable "datadog_api_key" {
  description = "API Key do Datadog para o collector OTEL."
  type        = string
  sensitive   = true
  default     = ""
}

variable "newrelic_license_key" {
  description = "License Key do New Relic para o collector OTEL."
  type        = string
  sensitive   = true
  default     = ""
}
