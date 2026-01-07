terraform {
  required_version = ">= 1.5.0"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "> 5.0"
    }
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "> 2.29"
    }
    random = {
      source = "hashicorp/random"
      version = "3.5.1"
    }
  }
}

provider "aws" {
  region = var.aws_region
}

# Cluster EKS
module "eks" {
  source                    = "./eks"
  cluster_name              = var.cluster_name
  node_count                = 1
  docker_image              = var.docker_image
  db_credentials_secret_arn = module.rds.db_credentials_secret_arn
  jwt_secret                = random_password.jwt_secret.result
}

# RDS PostgreSQL
module "rds" {
  source       = "./rds"
  cluster_name = var.cluster_name
}

# Provedor Kubernetes (depois do cluster pronto)
provider "kubernetes" {
  host                   = module.eks.cluster_endpoint
  cluster_ca_certificate = base64decode(module.eks.cluster_certificate_authority_data)
  token                  = module.eks.cluster_token
}

# Segredo para o JWT
resource "random_password" "jwt_secret" {
  length  = 32
  special = false
}

# Lambda Authorizer
module "lambda_authorizer" {
  source     = "./lambda_authorizer"
  jwt_secret = random_password.jwt_secret.result
}

# API Gateway para expor a API
module "api_gateway" {
  source                 = "./api_gateway"
  eks_service_url        = module.eks.service_url
  lambda_authorizer_arn  = module.lambda_authorizer.lambda_function_arn
  lambda_authorizer_invoke_arn = module.lambda_authorizer.lambda_function_invoke_arn
}
