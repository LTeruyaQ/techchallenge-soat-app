terraform {
  required_version = ">= 1.5.0"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = ">= 5.0"
    }
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.38"
    }
    kubectl = {
      source  = "gavinbunney/kubectl"
      version = "~> 1.19"
    }
  }
}

# =========================
# AWS PROVIDER (ROOT)
# =========================
provider "aws" {
  region = var.aws_region

  # HARD FAIL se token inválido
  skip_credentials_validation = false
  skip_requesting_account_id  = false
}

# =========================
# KUBERNETES PROVIDERS
# =========================
# Observação: os data sources aws_eks_cluster e aws_eks_cluster_auth
# devem existir apenas em data.tf (não duplicar aqui).
provider "kubernetes" {
  host                   = local.eks_cluster_endpoint
  cluster_ca_certificate = base64decode(local.eks_cluster_ca_certificate)
  token                  = var.skip_create_eks ? data.aws_eks_cluster_auth.existing_auth[0].token : data.aws_eks_cluster_auth.eks[0].token
}

provider "kubectl" {
  host                   = local.eks_cluster_endpoint
  cluster_ca_certificate = base64decode(local.eks_cluster_ca_certificate)
  token                  = var.skip_create_eks ? data.aws_eks_cluster_auth.existing_auth[0].token : data.aws_eks_cluster_auth.eks[0].token
  load_config_file       = false
}
