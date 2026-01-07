# ============================================
# Script de Deploy COMPLETO - MecanicaOS (AWS Academy)
# Versão Final com Criação de ECR via AWS CLI
# ============================================

param(
    [switch]$SkipBuild,
    [switch]$SkipInfra,
    [switch]$Destroy,
    [switch]$Plan,
    [string]$AWS_REGION = "us-east-1"
)

# Termina o script imediatamente se qualquer comando falhar
$ErrorActionPreference = "Stop"

# ======================================================
# FUNÇÕES DE LOG
# ======================================================
function Write-Title($msg) { Write-Host "`n============================================" -ForegroundColor Cyan; Write-Host " $msg" -ForegroundColor Cyan; Write-Host "============================================" -ForegroundColor Cyan }
function Write-Step($msg)     { Write-Host "`n==> $msg" -ForegroundColor Yellow }
function Write-Success($msg)  { Write-Host "[OK] $msg" -ForegroundColor Green }
function Write-Warning($msg)  { Write-Host "[!] $msg" -ForegroundColor Yellow }
function Write-ErrorMsg($msg) { Write-Host "[X] $msg" -ForegroundColor Red }
function Write-Info($msg)     { Write-Host "    $msg" -ForegroundColor Gray }

# Função para checar o resultado do último comando
function Check-Last-Exit-Code {
    if ($LASTEXITCODE -ne 0) {
        Write-ErrorMsg "Comando anterior falhou com código de saída $LASTEXITCODE. Abortando."
        exit 1
    }
}

# ======================================================
# ETAPA 0: SANITY CHECK
# ======================================================
Write-Title "ETAPA 0: Sanity Check do Ambiente"
if (-not (Test-Path ".\sanity-check.ps1")) {
    Write-ErrorMsg "sanity-check.ps1 não encontrado. Certifique-se de que ele está no diretório 'terraform/'."
    exit 1
}
try {
    Write-Step "Executando sanity-check.ps1"
    .\sanity-check.ps1
    Check-Last-Exit-Code
    Write-Success "Sanity check passou."
} catch {
    Write-ErrorMsg "Sanity check falhou. Verifique se as dependências (AWS CLI, Terraform, kubectl) estão instaladas e no PATH."
    throw
}

# ======================================================
# ETAPA 1: PRÉ-REQUISITOS E DESCOBERTA DINÂMICA
# ======================================================
Write-Title "ETAPA 1: Verificando Pré-requisitos e Descoberta Dinâmica"

# Validar credenciais AWS
try {
    $identity = aws sts get-caller-identity --output json | ConvertFrom-Json
    $AWS_ACCOUNT_ID = $identity.Account
    Write-Success "AWS Account ID: $AWS_ACCOUNT_ID"
} catch {
    Write-ErrorMsg "Credenciais AWS inválidas. Configure suas credenciais e tente novamente."
    exit 1
}

# Descoberta dinâmica do repositório Git
try {
    $gitUrl = git remote get-url origin
    if ($gitUrl -match "github.com/(.+)/(.+).git") {
        $GITHUB_USER = $Matches[1]
        $REPO_NAME = $Matches[2]
        Write-Success "Repositório detectado: $GITHUB_USER/$REPO_NAME"
    } else {
        throw "URL de remote do Git não está no formato esperado."
    }
} catch {
    Write-ErrorMsg "Não foi possível detectar o repositório Git. Certifique-se de que você está em um repositório clonado com um remote 'origin' para o GitHub."
    exit 1
}

# ======================================================
# ETAPA 2: CRIAÇÃO DO REPOSITÓRIO ECR
# ======================================================
Write-Title "ETAPA 2: Garantindo a existência do Repositório ECR"
$ECR_REPO_NAME = "mecanicaos-ecr" # Nome fixo
try {
    Write-Step "Tentando criar o repositório ECR '$ECR_REPO_NAME'..."
    aws ecr create-repository --repository-name $ECR_REPO_NAME --region $AWS_REGION | Out-Null
    Write-Success "Repositório ECR '$ECR_REPO_NAME' criado com sucesso."
} catch {
    if ($_.Exception.Message -like "*RepositoryAlreadyExistsException*") {
        Write-Warning "Repositório ECR '$ECR_REPO_NAME' já existe. Continuando."
    } else {
        Write-ErrorMsg "Falha ao criar o repositório ECR."
        throw
    }
}
$ECR_REPO_URL = "$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$ECR_REPO_NAME"
Write-Success "URL do ECR: $ECR_REPO_URL"

# ======================================================
# ETAPA 3: MODOS DE EXECUÇÃO (DESTROY / PLAN)
# ======================================================
if ($Destroy) {
    Write-Title "MODO DESTROY"
    Write-Step "Inicializando o Terraform..."
    terraform init; Check-Last-Exit-Code
    Write-Step "Destruindo a infraestrutura..."
    terraform destroy -auto-approve -var="ecr_repository_url=$ECR_REPO_URL"; Check-Last-Exit-Code
    Write-Success "Infraestrutura destruída."
    exit 0
}

if ($Plan) {
    Write-Title "MODO PLAN"
    Write-Step "Inicializando o Terraform..."
    terraform init; Check-Last-Exit-Code
    Write-Step "Planejando as alterações..."
    terraform plan -var="ecr_repository_url=$ECR_REPO_URL"; Check-Last-Exit-Code
    Write-Success "Plano gerado."
    exit 0
}

# ======================================================
# ETAPA 4: DEPLOY FASE 1 - INFRAESTRUTURA BASE
# ======================================================
if (-not $SkipInfra) {
    Write-Title "ETAPA 4: Deploy FASE 1 - Infraestrutura Base (EKS, RDS, Lambda)"

    # Atribuição de Roles do AWS Academy
    Write-Step "Atribuindo Roles padrão do EKS (AWS Academy)..."
    $env:TF_VAR_eks_cluster_role = "LabEksClusterRole"
    $env:TF_VAR_eks_node_role    = "LabEksNodeRole"
    Write-Success "Roles do EKS definidas: 'LabEksClusterRole' e 'LabEksNodeRole'."

    # Execução do Terraform
    Write-Step "Inicializando o Terraform..."
    terraform init; Check-Last-Exit-Code
    Write-Step "Validando a configuração..."
    terraform validate; Check-Last-Exit-Code
    Write-Step "Aplicando a infraestrutura base... Isso pode levar vários minutos."
    terraform apply -auto-approve -var="ecr_repository_url=$ECR_REPO_URL"; Check-Last-Exit-Code
    Write-Success "Infraestrutura base provisionada com sucesso."
}

# ======================================================
# ETAPA 5: CONFIGURAÇÃO PÓS-PROVISIONAMENTO
# ======================================================
Write-Title "ETAPA 5: Configuração Pós-Provisionamento"

# Configurar kubectl
Write-Step "Configurando kubectl para o cluster EKS..."
$EKS_CLUSTER_NAME = terraform output -raw eks_cluster_name; Check-Last-Exit-Code
aws eks update-kubeconfig --region $AWS_REGION --name $EKS_CLUSTER_NAME; Check-Last-Exit-Code
Write-Success "kubectl configurado para o cluster '$EKS_CLUSTER_NAME'."

# Inicialização do Banco de Dados RDS via Job do Kubernetes
# ... (código do Job omitido para brevidade, permanece o mesmo)

# ======================================================
# ETAPA 6: DEPLOY DA APLICAÇÃO E DESCOBERTA DO ALB
# ======================================================
Write-Title "ETAPA 6: Deploy da Aplicação no EKS"

# Kaniko Build
if (-not $SkipBuild) {
    # ... (código do Kaniko omitido para brevidade, permanece o mesmo)
}

Write-Step "Aplicando manifestos da aplicação (Deployment, Service, HPA)..."
kubectl apply -f "..\k8s\"; Check-Last-Exit-Code

Write-Step "Aguardando o Application Load Balancer (ALB) ser provisionado pela AWS..."
# ... (código de espera do ALB omitido para brevidade, permanece o mesmo)

# ======================================================
# ETAPA 7: DEPLOY FASE 2 - INTEGRAÇÃO FINAL
# ======================================================
Write-Title "ETAPA 7: Deploy FASE 2 - Integração do API Gateway com o EKS"

Write-Step "Executando a segunda fase do Terraform apply para configurar a integração..."
terraform apply -auto-approve -var="ecr_repository_url=$ECR_REPO_URL" -var="alb_hostname=$ALB_HOSTNAME"; Check-Last-Exit-Code
Write-Success "Integração do API Gateway concluída."

# ======================================================
# ETAPA 8: RESUMO FINAL DO DEPLOY
# ======================================================
Write-Title "ETAPA 8: Resumo do Deploy"

# ... (código de resumo omitido para brevidade, permanece o mesmo)

Write-Title "Deploy finalizado com sucesso!"
