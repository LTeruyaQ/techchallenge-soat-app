# ============================================
# Script de Deploy COMPLETO - MecanicaOS (AWS Academy)
# Com Sanity Check obrigatório
# ============================================

param(
    [switch]$SkipBuild,
    [switch]$SkipInfra,
    [switch]$Destroy,
    [switch]$Plan,
    [string]$AWS_REGION = "us-east-1",
    [string]$ECR_REPO_NAME = "mecanicaos-ecr"
)

$ErrorActionPreference = "Stop"

# ======================================================
# FUNÇÕES DE LOG
# ======================================================

function Write-Title($msg) {
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host " $msg" -ForegroundColor Cyan
    Write-Host "============================================" -ForegroundColor Cyan
}
function Write-Step($msg)     { Write-Host "`n==> $msg" -ForegroundColor Yellow }
function Write-Success($msg)  { Write-Host "[OK] $msg" -ForegroundColor Green }
function Write-Warning($msg)  { Write-Host "[!] $msg" -ForegroundColor Yellow }
function Write-ErrorMsg($msg) { Write-Host "[X] $msg" -ForegroundColor Red }
function Write-Info($msg)     { Write-Host "    $msg" -ForegroundColor Gray }

# ======================================================
# ETAPA 0: SANITY CHECK
# ======================================================

Write-Title "ETAPA 0: Sanity Check do Ambiente"

if (-not (Test-Path ".\sanity-check.ps1")) {
    Write-ErrorMsg "sanity-check.ps1 não encontrado"
    exit 1
}

try {
    Write-Step "Executando sanity-check.ps1"
    .\sanity-check.ps1
    Write-Success "Sanity check passou"
} catch {
    Write-ErrorMsg "Sanity check falhou"
    throw
}

# ======================================================
# ETAPA 1: PRÉ-REQUISITOS
# ======================================================

Write-Title "ETAPA 1: Verificando Pré-requisitos"

foreach ($cmd in @("aws", "terraform", "kubectl", "docker")) {
    if (-not (Get-Command $cmd -ErrorAction SilentlyContinue)) {
        Write-ErrorMsg "$cmd não encontrado no PATH"
        exit 1
    }
}

try {
    docker info | Out-Null
    Write-Success "Docker rodando"
} catch {
    Write-ErrorMsg "Docker não está rodando"
    exit 1
}

# ======================================================
# ETAPA 2: CREDENCIAIS AWS
# ======================================================

Write-Title "ETAPA 2: Validando Credenciais AWS"

try {
    $identity = aws sts get-caller-identity --output json | ConvertFrom-Json
    $AWS_ACCOUNT_ID = $identity.Account
    Write-Success "AWS Account ID: $AWS_ACCOUNT_ID"
} catch {
    Write-ErrorMsg "Credenciais AWS inválidas"
    exit 1
}

# ======================================================
# ETAPA 3: TFVARS / MODOS
# ======================================================

Write-Title "ETAPA 3: Validando terraform.tfvars"

if (-not (Test-Path "terraform.tfvars")) {
    Write-ErrorMsg "terraform.tfvars não encontrado"
    exit 1
}

if ($Destroy) {
    Write-Title "MODO DESTROY"
    terraform init
    terraform destroy -auto-approve
    exit 0
}

if ($Plan) {
    Write-Title "MODO PLAN"
    terraform init
    terraform plan
    exit 0
}

# ======================================================
# ETAPA 4: BUILD COM KANIKO
# ======================================================

if (-not $SkipBuild) {

    Write-Title "ETAPA 4: Garantindo ECR"
    terraform init
    terraform apply -target=aws_ecr_repository.app -auto-approve

    Write-Title "ETAPA 5: Build com Kaniko"

    $GITHUB_USER = "LTeruyaQ"
    $REPO_NAME   = "techchallenge-soat-app"
    $env:GIT_TOKEN = "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"

    if (-not $env:GIT_TOKEN) {
        Write-ErrorMsg "Defina a variável de ambiente GIT_TOKEN"
        exit 1
    }

    $IMAGE_TAG = Get-Date -Format "yyyyMMdd-HHmmss"
    $ECR_URI   = "$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$ECR_REPO_NAME"
    $JOB_NAME  = "kaniko-build-$IMAGE_TAG"
    $NAMESPACE = "build"

    kubectl create namespace $NAMESPACE --dry-run=client -o yaml | kubectl apply -f -

    kubectl delete secret aws-creds -n $NAMESPACE --ignore-not-found | Out-Null
    kubectl create secret generic aws-creds `
        -n $NAMESPACE `
        --from-literal=AWS_ACCESS_KEY_ID=$env:AWS_ACCESS_KEY_ID `
        --from-literal=AWS_SECRET_ACCESS_KEY=$env:AWS_SECRET_ACCESS_KEY `
        --from-literal=AWS_DEFAULT_REGION=$AWS_REGION | Out-Null

    Write-Step "Criando Job Kaniko"

$jobYaml = @"
apiVersion: batch/v1
kind: Job
metadata:
  name: $JOB_NAME
  namespace: $NAMESPACE
spec:
  backoffLimit: 0
  template:
    spec:
      restartPolicy: Never
      containers:
      - name: kaniko
        image: gcr.io/kaniko-project/executor:latest
        envFrom:
        - secretRef:
            name: aws-creds
        args:
        - "--context=git://github.com/$GITHUB_USER/$REPO_NAME.git"
        - "--dockerfile=Dockerfile"
        - "--destination=${ECR_URI}:$IMAGE_TAG"
        - "--destination=${ECR_URI}:latest"
"@

    $jobYaml | kubectl apply -f -

    Write-Success "Job Kaniko submetido"
}

# ======================================================
# ETAPA 6: INFRA (EKS)
# ======================================================

if (-not $SkipInfra) {

    Write-Title "ETAPA 6: Deploy Terraform"

    $clusterRole = aws iam list-roles --query "Roles[?contains(RoleName,'LabEksClusterRole')].RoleName | [0]" --output text
    $nodeRole    = aws iam list-roles --query "Roles[?contains(RoleName,'LabEksNodeRole')].RoleName | [0]" --output text

    if (-not $clusterRole -or -not $nodeRole) {
        Write-ErrorMsg "Roles do EKS não encontradas (AWS Academy)"
        exit 1
    }

    $env:TF_VAR_eks_cluster_role = $clusterRole
    $env:TF_VAR_eks_node_role    = $nodeRole

    terraform init
    terraform validate
    terraform apply -auto-approve
}

# ======================================================
# ETAPA 7: KUBECONFIG
# ======================================================

Write-Title "ETAPA 7: Configurando kubectl"

$EKS_CLUSTER_NAME = terraform output -raw eks_cluster_name
aws eks update-kubeconfig --region $AWS_REGION --name $EKS_CLUSTER_NAME

Write-Success "kubectl configurado"

# ======================================================
# ETAPA 8: VERIFICAÇÃO FINAL
# ======================================================

Write-Title "ETAPA 8: Verificação Final"

kubectl get pods -A
kubectl get svc  -A

Write-Success "Deploy finalizado com sucesso"
