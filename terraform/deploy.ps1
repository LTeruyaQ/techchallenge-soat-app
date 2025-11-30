# ============================================
# Script de Deploy - MecanicaOS na AWS Academy
# ============================================
# 
# Este script automatiza o processo de deploy do MecanicaOS
# na AWS Academy usando Terraform e EKS.
#
# IMPORTANTE: AWS Academy tem limitações:
# - Usa LabRole, LabEksClusterRole e LabEksNodeRole
# - NÃO pode criar IAM Roles/Policies
# - Credenciais expiram a cada 4 horas
#
# Uso: .\deploy.ps1 [-Destroy] [-Plan]
#
# ============================================

param(
    [switch]$Destroy,
    [switch]$Plan
)

$ErrorActionPreference = "Stop"

# Cores para output
function Write-Step { param($msg) Write-Host "`n==> $msg" -ForegroundColor Cyan }
function Write-Success { param($msg) Write-Host "[OK] $msg" -ForegroundColor Green }
function Write-Warning { param($msg) Write-Host "[!] $msg" -ForegroundColor Yellow }
function Write-Error { param($msg) Write-Host "[X] $msg" -ForegroundColor Red }

# ============================================
# Verificações Iniciais
# ============================================

Write-Step "Verificando pré-requisitos..."

# Verificar AWS CLI
if (-not (Get-Command aws -ErrorAction SilentlyContinue)) {
    Write-Error "AWS CLI não encontrado. Instale em: https://aws.amazon.com/cli/"
    exit 1
}
Write-Success "AWS CLI encontrado"

# Verificar Terraform
if (-not (Get-Command terraform -ErrorAction SilentlyContinue)) {
    Write-Error "Terraform não encontrado. Instale em: https://www.terraform.io/downloads"
    exit 1
}
Write-Success "Terraform encontrado"

# Verificar credenciais AWS
try {
    $identity = aws sts get-caller-identity --output json | ConvertFrom-Json
    Write-Success "Credenciais AWS válidas (Account: $($identity.Account))"
} catch {
    Write-Error "Credenciais AWS inválidas. Execute 'aws configure' primeiro"
    Write-Warning "Lembre-se: As credenciais do AWS Academy expiram a cada 4 horas!"
    exit 1
}

# Verificar arquivo terraform.tfvars
if (-not (Test-Path "terraform.tfvars")) {
    Write-Error "Arquivo terraform.tfvars não encontrado!"
    Write-Warning "Copie terraform.tfvars.example para terraform.tfvars e preencha os valores"
    Write-Warning "IMPORTANTE: Copie os nomes das roles LabEksClusterRole e LabEksNodeRole do console AWS"
    exit 1
}
Write-Success "Arquivo terraform.tfvars encontrado"

# ============================================
# Modo Destroy
# ============================================

if ($Destroy) {
    Write-Step "Destruindo infraestrutura..."
    terraform destroy -auto-approve
    Write-Success "Infraestrutura destruída com sucesso!"
    exit 0
}

# ============================================
# Etapa 1: Inicializar Terraform
# ============================================

Write-Step "Inicializando Terraform..."
terraform init

if ($LASTEXITCODE -ne 0) {
    Write-Error "Falha ao inicializar Terraform"
    exit 1
}
Write-Success "Terraform inicializado"

# ============================================
# Etapa 2: Plan (se solicitado)
# ============================================

if ($Plan) {
    Write-Step "Executando terraform plan..."
    terraform plan
    exit 0
}

# ============================================
# Etapa 3: Aplicar Terraform
# ============================================

Write-Step "Aplicando configuração Terraform..."
Write-Warning "Isso pode levar 15-20 minutos para criar o cluster EKS"

terraform apply -auto-approve

if ($LASTEXITCODE -ne 0) {
    Write-Error "Falha ao aplicar Terraform"
    exit 1
}
Write-Success "Infraestrutura criada com sucesso!"

# ============================================
# Etapa 4: Exibir Informações de Acesso
# ============================================

Write-Step "Informações de Acesso"
Write-Host ""
Write-Host "EKS Cluster:" -ForegroundColor Yellow
terraform output eks_cluster_name
Write-Host ""
Write-Host "EKS Endpoint:" -ForegroundColor Yellow
terraform output eks_cluster_endpoint
Write-Host ""
Write-Host "Para configurar kubectl:" -ForegroundColor Yellow
terraform output kubectl_config_command
Write-Host ""

Write-Success "Deploy concluído com sucesso!"
Write-Host ""
Write-Host "Próximos passos:" -ForegroundColor Cyan
Write-Host "1. Execute o comando kubectl para configurar o acesso ao cluster"
Write-Host "2. Verifique os pods: kubectl get pods -n mecanicaos"
Write-Host "3. Verifique o service: kubectl get svc -n mecanicaos"
Write-Host "4. Para destruir: .\deploy.ps1 -Destroy"
