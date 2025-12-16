# Terraform para MecanicaOS - AWS Academy

Este diretório contém a configuração do Terraform para implantar a aplicação MecanicaOS no Amazon EKS usando **AWS Academy**.

## ⚠️ Limitações do AWS Academy

- **NÃO pode criar IAM Roles/Policies** - Usa roles pré-existentes
- **Credenciais expiram a cada 4 horas** - Atualize antes de executar
- **Usa LabRole, LabEksClusterRole e LabEksNodeRole**

## Pré-requisitos

- [Terraform](https://www.terraform.io/downloads) >= 1.5.0
- [AWS CLI](https://aws.amazon.com/cli/) configurado
- [kubectl](https://kubernetes.io/docs/tasks/tools/)

## Configuração

### 1. Copiar arquivo de variáveis

```powershell
cp terraform.tfvars.example terraform.tfvars
```

### 2. Obter nomes das roles do AWS Academy

1. Acesse o **Console AWS**
2. Vá em **IAM > Roles**
3. Procure por **"LabEksClusterRole"** e **"LabEksNodeRole"**
4. Copie os nomes completos (incluindo o prefixo)

### 3. Preencher terraform.tfvars

```hcl
# Roles do AWS Academy (OBRIGATÓRIO)
eks_cluster_role_name = "c175509a...-LabEksClusterRole-..."
eks_node_role_name    = "c175509a...-LabEksNodeRole-..."

# Banco de dados Supabase
db_host     = "aws-0-sa-east-1.pooler.supabase.com"
db_password = "SUA_SENHA"

# JWT
jwt_secret_key = "sua_chave_secreta"
```

## Deploy

### Usando script (recomendado)

```powershell
# Deploy completo
.\deploy.ps1

# Apenas plan
.\deploy.ps1 -Plan

# Destruir
.\deploy.ps1 -Destroy
```

### Manual

```powershell
terraform init
terraform plan
terraform apply
```

## Estrutura de Arquivos

```
terraform/
├── providers.tf          # Providers AWS, Kubernetes, Kubectl
├── variables.tf          # Variáveis
├── locals.tf             # Valores calculados
├── iam-roles.tf          # Data sources das roles AWS Academy
├── vpc.tf                # VPC
├── subnets.tf            # Subnets públicas
├── internet-gateway.tf   # Internet Gateway
├── route-tables.tf       # Route Tables
├── security-groups.tf    # Security Groups
├── eks-cluster.tf        # Cluster EKS
├── eks-nodegroup.tf      # Node Group
├── eks-access.tf         # Access Entry e Policy
├── k8s-namespace.tf      # Namespace Kubernetes
├── k8s-deployment.tf     # Deployment da API
├── k8s-service.tf        # Service LoadBalancer
├── outputs.tf            # Outputs
└── data.tf               # Data sources
```

## Outputs

Após o deploy:

```powershell
# Ver todos os outputs
terraform output

# Configurar kubectl
aws eks update-kubeconfig --region us-east-1 --name eks-mecanicaos
```

## Verificar Deploy

```powershell
kubectl get pods -n mecanicaos
kubectl get svc -n mecanicaos
kubectl logs -n mecanicaos -l app=mecanicaos-api
```

## Destruir

```powershell
terraform destroy