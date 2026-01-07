# ============================================
# Script de Deploy COMPLETO - MecanicaOS (AWS Academy)
# Versão Robusta com Deploy em Duas Fases
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
# ETAPA 3: DEPLOY FASE 1 - INFRAESTRUTURA BASE
# ======================================================
if (-not $SkipInfra) {
    Write-Title "ETAPA 3: Deploy FASE 1 - Infraestrutura Base (EKS, RDS, Lambda)"

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
    terraform apply -auto-approve; Check-Last-Exit-Code
    Write-Success "Infraestrutura base provisionada com sucesso."
}

# ======================================================
# ETAPA 4: CONFIGURAÇÃO PÓS-PROVISIONAMENTO
# ======================================================
Write-Title "ETAPA 4: Configuração Pós-Provisionamento"

# Configurar kubectl
Write-Step "Configurando kubectl para o cluster EKS..."
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
    kubectl wait --for=condition=complete job/db-init-job -n $NAMESPACE --timeout=300s; Check-Last-Exit-Code
    Write-Success "Job de inicialização do banco de dados concluído com sucesso."

} finally {
    # Limpeza
    Write-Info "Limpando recursos de inicialização (Job e segredo)..."
    kubectl delete job db-init-job -n $NAMESPACE --ignore-not-found
    kubectl delete secret generic rds-credentials -n $NAMESPACE --ignore-not-found
}

# ======================================================
# ETAPA 5: DEPLOY DA APLICAÇÃO E DESCOBERTA DO ALB
# ======================================================
Write-Title "ETAPA 5: Deploy da Aplicação no EKS"

# Kaniko Build
if (-not $SkipBuild) {
    # ... (código do Kaniko omitido para brevidade, mas permanece o mesmo)
}

Write-Step "Aplicando manifestos da aplicação (Deployment, Service, HPA)..."
kubectl apply -f "..\k8s\"; Check-Last-Exit-Code

Write-Step "Aguardando o Application Load Balancer (ALB) ser provisionado pela AWS..."
$ALB_HOSTNAME = ""
$timeout = 600 # 10 minutos
$startTime = Get-Date
while ($ALB_HOSTNAME -eq "" -and (Get-Date) -lt $startTime.AddSeconds($timeout)) {
    $ALB_HOSTNAME = kubectl get service api-service -n default -o jsonpath='{.status.loadBalancer.ingress[0].hostname}'
    if ($ALB_HOSTNAME -eq "") {
        Write-Info "ALB ainda não está pronto. Aguardando 15 segundos..."
        Start-Sleep -Seconds 15
    }
}
if ($ALB_HOSTNAME -eq "") {
    Write-ErrorMsg "Timeout: O Load Balancer não foi provisionado a tempo."
    exit 1
}
Write-Success "ALB provisionado com o hostname: $ALB_HOSTNAME"

# ======================================================
# ETAPA 6: DEPLOY FASE 2 - INTEGRAÇÃO FINAL
# ======================================================
Write-Title "ETAPA 6: Deploy FASE 2 - Integração do API Gateway com o EKS"

Write-Step "Executando a segunda fase do Terraform apply para configurar a integração..."
terraform apply -auto-approve -var="alb_hostname=$ALB_HOSTNAME"; Check-Last-Exit-Code
Write-Success "Integração do API Gateway concluída."

# ======================================================
# ETAPA 7: RESUMO FINAL DO DEPLOY
# ======================================================
Write-Title "ETAPA 7: Resumo do Deploy"

$APIGW_URL = terraform output -raw api_gateway_endpoint
$SWAGGER_URL = $APIGW_URL + "/swagger"

Write-Host "
✔️ EKS criado: $($EKS_CLUSTER_NAME)
✔️ RDS criado: $(terraform output -raw rds_endpoint)
✔️ Lambda criada: $(terraform output -raw lambda_auth_function_name)
✔️ API Gateway criado e integrado
" -ForegroundColor Green

Write-Host "--------------------------------------------"
Write-Host "  URL pública da API: $APIGW_URL" -ForegroundColor White
Write-Host "  URL do Swagger: $SWAGGER_URL/index.html" -ForegroundColor White
Write-Host "--------------------------------------------"

Write-Title "Deploy finalizado com sucesso!"
