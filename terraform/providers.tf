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

# ===================================================================
# KUBERNETES & KUBECTL PROVIDERS (COM DEPENDÊNCIA EXPLÍCITA DO EKS)
# ===================================================================
# Esta é a configuração CORRETA.
# Os provedores obtêm os dados de conexão diretamente do RECURSO 'aws_eks_cluster.eks'.
# Isso garante que o Terraform só tentará se conectar ao cluster DEPOIS que ele for criado.
# A autenticação é feita de forma dinâmica usando o AWS CLI.

provider "kubernetes" {
  host                   = aws_eks_cluster.eks.endpoint
  cluster_ca_certificate = base64decode(aws_eks_cluster.eks.certificate_authority[0].data)

  exec {
    api_version = "client.authentication.k8s.io/v1beta1"
    command     = "aws"
    # O 'args' constrói o comando: aws eks get-token --cluster-name <nome-do-cluster>
    args = ["eks", "get-token", "--cluster-name", aws_eks_cluster.eks.name]
  }
}

provider "kubectl" {
  # A configuração é idêntica para o provider kubectl.
  host                   = aws_eks_cluster.eks.endpoint
  cluster_ca_certificate = base64decode(aws_eks_cluster.eks.certificate_authority[0].data)

  exec {
    api_version = "client.authentication.k8s.io/v1beta1"
    command     = "aws"
    args        = ["eks", "get-token", "--cluster-name", aws_eks_cluster.eks.name]
  }
}
