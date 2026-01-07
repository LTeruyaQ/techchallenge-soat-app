# ============================================
# Script de Deploy COMPLETO - MecanicaOS (AWS Academy)
# Adaptado para automação total com RDS, Lambda e API Gateway
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
function Write-Title($msg) { Write-Host "`n============================================" -ForegroundColor Cyan; Write-Host " $msg" -ForegroundColor Cyan; Write-Host "============================================" -ForegroundColor Cyan }
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
    Write-ErrorMsg "sanity-check.ps1 não encontrado. Certifique-se de que ele está no diretório 'terraform/'."
    exit 1
}
try {
    Write-Step "Executando sanity-check.ps1"
    .\sanity-check.ps1
    Write-Success "Sanity check passou."
} catch {
    Write-ErrorMsg "Sanity check falhou. Verifique se todas as dependências (AWS CLI, Terraform, kubectl, psql) estão instaladas e no PATH."
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
# ETAPA 2: MODOS DE EXECUÇÃO (DESTROY / PLAN)
# ======================================================
if ($Destroy) {
    Write-Title "MODO DESTROY"
    Write-Step "Inicializando o Terraform..."
    terraform init
    Write-Step "Destruindo a infraestrutura..."
    terraform destroy -auto-approve
    Write-Success "Infraestrutura destruída."
    exit 0
}

if ($Plan) {
    Write-Title "MODO PLAN"
    Write-Step "Inicializando o Terraform..."
    terraform init
    Write-Step "Planejando as alterações..."
    terraform plan
    Write-Success "Plano gerado."
    exit 0
}

# ======================================================
# ETAPA 3: PROVISIONAMENTO COMPLETO COM TERRAFORM
# ======================================================
if (-not $SkipInfra) {
    Write-Title "ETAPA 3: Deploy de Infraestrutura Completa com Terraform"

    # Atribuição de Roles do AWS Academy
    Write-Step "Atribuindo Roles padrão do EKS (AWS Academy)..."
    $clusterRole = "LabEksClusterRole"
    $nodeRole    = "LabEksNodeRole"
    Write-Success "Roles do EKS definidas: '$clusterRole' e '$nodeRole'."
    $env:TF_VAR_eks_cluster_role = $clusterRole
    $env:TF_VAR_eks_node_role    = $nodeRole

    # Execução do Terraform
    Write-Step "Inicializando o Terraform..."
    terraform init
    Write-Step "Validando a configuração..."
    terraform validate
    Write-Step "Aplicando a infraestrutura (EKS, RDS, Lambda, API GW)... Isso pode levar vários minutos."
    terraform apply -auto-approve
    Write-Success "Infraestrutura provisionada com sucesso."
}

# ======================================================
# ETAPA 4: CONFIGURAÇÃO PÓS-PROVISIONAMENTO
# ======================================================
Write-Title "ETAPA 4: Configuração Pós-Provisionamento"

# Configurar kubectl
Write-Step "Configurando kubectl para o novo cluster EKS..."
$EKS_CLUSTER_NAME = terraform output -raw eks_cluster_name
aws eks update-kubeconfig --region $AWS_REGION --name $EKS_CLUSTER_NAME
Write-Success "kubectl configurado para o cluster '$EKS_CLUSTER_NAME'."

# Inicialização do Banco de Dados RDS
Write-Step "Inicializando o esquema do banco de dados RDS..."
try {
    $DB_ENDPOINT = terraform output -raw rds_endpoint
    $DB_NAME = terraform output -raw rds_dbname
    $DB_SECRET_ARN = terraform output -raw db_secret_arn

    Write-Info "Obtendo senha do RDS do Secrets Manager..."
    $secretValue = aws secretsmanager get-secret-value --secret-id $DB_SECRET_ARN --query SecretString --output text | ConvertFrom-Json
    $DB_USER = $secretValue.username
    $DB_PASSWORD = $secretValue.password

    Write-Info "Executando rds-init.sql no RDS..."
    $env:PGPASSWORD = $DB_PASSWORD
    psql "host=$($DB_ENDPOINT.Split(':')[0]) port=$($DB_ENDPOINT.Split(':')[1]) dbname=$DB_NAME user=$DB_USER sslmode=require" -f ".\rds-init.sql"
    $env:PGPASSWORD = $null # Limpar a variável de ambiente
    Write-Success "Esquema do banco de dados inicializado com sucesso."
} catch {
    Write-ErrorMsg "Falha ao inicializar o banco de dados RDS. Verifique a conectividade e as saídas do Terraform."
    throw
}


# ======================================================
# ETAPA 5: BUILD E DEPLOY DA APLICAÇÃO COM KANIKO
# ======================================================
if (-not $SkipBuild) {
    Write-Title "ETAPA 5: Build da Imagem com Kaniko"

    # Token do Git (mantido conforme solicitado)
    $env:GIT_TOKEN = "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"
    if (-not $env:GIT_TOKEN) {
        Write-ErrorMsg "A variável de ambiente GIT_TOKEN não está definida."
        exit 1
    }

    $IMAGE_TAG = (git rev-parse --short HEAD)
    $ECR_URI   = terraform output -raw ecr_repository_url
    $JOB_NAME  = "kaniko-build-$IMAGE_TAG"
    $NAMESPACE = "build"

    Write-Step "Aplicando namespace e segredos para o build..."
    kubectl apply -f "..\k8s\namespace.yaml"
    kubectl delete secret generic aws-creds -n $NAMESPACE --ignore-not-found | Out-Null
    kubectl create secret generic aws-creds `
        -n $NAMESPACE `
        --from-literal=AWS_ACCESS_KEY_ID=$env:AWS_ACCESS_KEY_ID `
        --from-literal=AWS_SECRET_ACCESS_KEY=$env:AWS_SECRET_ACCESS_KEY `
        --from-literal=AWS_DEFAULT_REGION=$AWS_REGION | Out-Null

    Write-Step "Criando e submetendo Job do Kaniko..."
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
    Write-Success "Job Kaniko '$JOB_NAME' submetido. Aguardando conclusão..."

    # Aguardar a conclusão do Job
    $timeout = 600 # 10 minutos
    $startTime = Get-Date
    while ((Get-Date) -lt $startTime.AddSeconds($timeout)) {
        $status = kubectl get job $JOB_NAME -n $NAMESPACE -o jsonpath='{.status.conditions[?(@.type=="Complete")].status}'
        if ($status -eq 'True') {
            Write-Success "Build do Kaniko concluído com sucesso."
            break
        }
        $failedStatus = kubectl get job $JOB_NAME -n $NAMESPACE -o jsonpath='{.status.conditions[?(@.type=="Failed")].status}'
        if ($failedStatus -eq 'True') {
            $logs = kubectl logs job/$JOB_NAME -n $NAMESPACE
            Write-ErrorMsg "Build do Kaniko falhou."
            Write-Info "Logs do Pod do Kaniko:"
            Write-Info $logs
            exit 1
        }
        Start-Sleep -Seconds 10
    }
    kubectl delete job $JOB_NAME -n $NAMESPACE --ignore-not-found | Out-Null
}

# ======================================================
# ETAPA 6: RESUMO FINAL DO DEPLOY
# ======================================================
Write-Title "ETAPA 6: Resumo do Deploy"

$APIGW_URL = terraform output -raw api_gateway_endpoint
$SWAGGER_URL = $APIGW_URL + "/swagger" # Ajuste o path se for diferente

Write-Host "
✔️ EKS criado: $($EKS_CLUSTER_NAME)
✔️ RDS criado: $(terraform output -raw rds_endpoint)
✔️ Lambda criada: $(terraform output -raw lambda_auth_function_name)
✔️ API Gateway criado
" -ForegroundColor Green

Write-Host "--------------------------------------------"
Write-Host "  URL pública da API: $APIGW_URL" -ForegroundColor White
Write-Host "  URL do Swagger: $SWAGGER_URL/index.html" -ForegroundColor White
Write-Host "--------------------------------------------"

Write-Title "Deploy finalizado com sucesso!"
