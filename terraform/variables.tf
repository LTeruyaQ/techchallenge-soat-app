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
# IMPORTANTE: Copie os nomes exatos das roles do seu AWS Academy Lab
# Você encontra em: IAM > Roles > Procure por "LabEks"
#

variable "eks_cluster_role_name" {
  description = "Nome da LabEksClusterRole do AWS Academy (copie do console AWS)"
  type        = string
  # Exemplo: "c175509a4540172l11442646t1w891377-LabEksClusterRole-QQH0SV203Gtw"
}

variable "eks_node_role_name" {
  description = "Nome da LabEksNodeRole do AWS Academy (copie do console AWS)"
  type        = string
  # Exemplo: "c175509a4540172l11442646t1w891377135-LabEksNodeRole-r3HYcSAYWMXX"
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
  description = "Tag da imagem Docker no ECR"
  type        = string
  default     = "latest"
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
