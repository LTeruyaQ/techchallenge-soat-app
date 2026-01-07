# ============================================
# Script de Deploy COMPLETO - MecanicaOS (AWS Academy)
# Versão Robusta com Inicialização de DB via K8s Job
# ============================================

param(
    [switch]$SkipBuild,
    [switch]$SkipInfra,
    [switch]$Destroy,
    [switch]$Plan,
    [string]$AWS_REGION = "us-east-1",
    [string]$ECR_REPO_NAME = "mecanicaos-ecr"
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
# ETAPA 2: MODOS DE EXECUÇÃO (DESTROY / PLAN)
# ======================================================
if ($Destroy) {
    Write-Title "MODO DESTROY"
    Write-Step "Inicializando o Terraform..."
    terraform init; Check-Last-Exit-Code
    Write-Step "Destruindo a infraestrutura..."
    terraform destroy -auto-approve; Check-Last-Exit-Code
    Write-Success "Infraestrutura destruída."
    exit 0
}

if ($Plan) {
    Write-Title "MODO PLAN"
    Write-Step "Inicializando o Terraform..."
    terraform init; Check-Last-Exit-Code
    Write-Step "Planejando as alterações..."
    terraform plan; Check-Last-Exit-Code
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
    $env:TF_VAR_eks_cluster_role = "LabEksClusterRole"
    $env:TF_VAR_eks_node_role    = "LabEksNodeRole"
    Write-Success "Roles do EKS definidas: 'LabEksClusterRole' e 'LabEksNodeRole'."

    # Execução do Terraform
    Write-Step "Inicializando o Terraform..."
    terraform init; Check-Last-Exit-Code
    Write-Step "Validando a configuração..."
    terraform validate; Check-Last-Exit-Code
    Write-Step "Aplicando a infraestrutura (EKS, RDS, Lambda, API GW)... Isso pode levar vários minutos."
    terraform apply -auto-approve; Check-Last-Exit-Code
    Write-Success "Infraestrutura provisionada com sucesso."
}

# ======================================================
# ETAPA 4: CONFIGURAÇÃO PÓS-PROVISIONAMENTO
# ======================================================
Write-Title "ETAPA 4: Configuração Pós-Provisionamento"

# Configurar kubectl
Write-Step "Configurando kubectl para o novo cluster EKS..."
$EKS_CLUSTER_NAME = terraform output -raw eks_cluster_name; Check-Last-Exit-Code
aws eks update-kubeconfig --region $AWS_REGION --name $EKS_CLUSTER_NAME; Check-Last-Exit-Code
Write-Success "kubectl configurado para o cluster '$EKS_CLUSTER_NAME'."

# Inicialização do Banco de Dados RDS via Job do Kubernetes
Write-Step "Inicializando o esquema do banco de dados RDS via Job do Kubernetes..."
$NAMESPACE = "build"
try {
    # Obter detalhes do RDS do Terraform
    $DB_HOST = (terraform output -raw rds_endpoint).Split(':')[0]; Check-Last-Exit-Code
    $DB_NAME = terraform output -raw rds_dbname; Check-Last-Exit-Code
    $DB_SECRET_ARN = terraform output -raw db_secret_arn; Check-Last-Exit-Code

    # Obter senha do Secrets Manager
    Write-Info "Obtendo senha do RDS do Secrets Manager..."
    $secretValueJson = aws secretsmanager get-secret-value --secret-id $DB_SECRET_ARN --query SecretString --output text; Check-Last-Exit-Code
    $secretValue = $secretValueJson | ConvertFrom-Json
    $DB_USER = $secretValue.username
    $DB_PASSWORD = $secretValue.password

    # Criar segredo no K8s para o Job
    Write-Info "Criando segredo temporário no Kubernetes para as credenciais do RDS..."
    kubectl create namespace $NAMESPACE --dry-run=client -o yaml | kubectl apply -f -
    kubectl delete secret generic rds-credentials -n $NAMESPACE --ignore-not-found
    kubectl create secret generic rds-credentials -n $NAMESPACE `
        --from-literal=host=$DB_HOST `
        --from-literal=dbname=$DB_NAME `
        --from-literal=user=$DB_USER `
        --from-literal=password=$DB_PASSWORD; Check-Last-Exit-Code

    # Aplicar o ConfigMap e o Job
    Write-Info "Aplicando ConfigMap com script SQL e o Job de inicialização..."
    kubectl apply -f "..\k8s\rds-init-configmap.yaml"; Check-Last-Exit-Code
    kubectl delete job db-init-job -n $NAMESPACE --ignore-not-found
    kubectl apply -f "..\k8s\rds-init-job.yaml"; Check-Last-Exit-Code

    # Aguardar a conclusão do Job
    Write-Info "Aguardando a conclusão do Job de inicialização do banco de dados..."
    $timeout = 300 # 5 minutos
    $startTime = Get-Date
    while ((Get-Date) -lt $startTime.AddSeconds($timeout)) {
        $status = kubectl get job db-init-job -n $NAMESPACE -o jsonpath='{.status.conditions[?(@.type=="Complete")].status}'
        if ($status -eq 'True') {
            Write-Success "Job de inicialização do banco de dados concluído com sucesso."
            break
        }
        $failedStatus = kubectl get job db-init-job -n $NAMESPACE -o jsonpath='{.status.conditions[?(@.type=="Failed")].status}'
        if ($failedStatus -eq 'True') {
            $logs = kubectl logs job/db-init-job -n $NAMESPACE
            Write-ErrorMsg "Job de inicialização do banco de dados falhou."
            Write-Info "Logs do Pod:"
            Write-Info $logs
            exit 1
        }
        Start-Sleep -Seconds 10
    }

    Write-Success "Esquema do banco de dados inicializado com sucesso."

} finally {
    # Limpeza
    Write-Info "Limpando recursos de inicialização (Job e segredo)..."
    kubectl delete job db-init-job -n $NAMESPACE --ignore-not-found
    kubectl delete secret generic rds-credentials -n $NAMESPACE --ignore-not-found
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

    $IMAGE_TAG = (git rev-parse --short HEAD); Check-Last-Exit-Code
    $ECR_URI   = terraform output -raw ecr_repository_url; Check-Last-Exit-Code
    $JOB_NAME  = "kaniko-build-$IMAGE_TAG"

    Write-Step "Aplicando segredos para o build..."
    kubectl delete secret generic aws-creds -n $NAMESPACE --ignore-not-found
    kubectl create secret generic aws-creds `
        -n $NAMESPACE `
        --from-literal=AWS_ACCESS_KEY_ID=$env:AWS_ACCESS_KEY_ID `
        --from-literal=AWS_SECRET_ACCESS_KEY=$env:AWS_SECRET_ACCESS_KEY `
        --from-literal=AWS_DEFAULT_REGION=$AWS_REGION; Check-Last-Exit-Code

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
    $jobYaml | kubectl apply -f -; Check-Last-Exit-Code
    Write-Success "Job Kaniko '$JOB_NAME' submetido. Aguardando conclusão..."

    # Aguardar a conclusão do Job do Kaniko
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
    kubectl delete job $JOB_NAME -n $NAMESPACE --ignore-not-found
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
