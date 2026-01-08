# ============================================
# Script de Deploy COMPLETO - MecanicaOS (AWS Academy)
# ============================================

param(
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
function Write-ErrorMsg($msg) { Write-Host "[X] $msg" -ForegroundColor Red }

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
    Write-ErrorMsg "sanity-check.ps1 não encontrado."
    exit 1
}
try {
    Write-Step "Executando sanity-check.ps1"
    .\sanity-check.ps1
    Check-Last-Exit-Code
    Write-Success "Sanity check passou."
} catch {
    Write-ErrorMsg "Sanity check falhou."
    throw
}

# ======================================================
# ETAPA 1: CREDENCIAIS AWS
# ======================================================
Write-Title "ETAPA 1: Verificando Credenciais AWS"
try {
    $identity = aws sts get-caller-identity --output json | ConvertFrom-Json
    $AWS_ACCOUNT_ID = $identity.Account
    Write-Success "AWS Account ID: $AWS_ACCOUNT_ID"
} catch {
    Write-ErrorMsg "Credenciais AWS inválidas."
    exit 1
}

# ======================================================
# ETAPA 2: MODOS DE EXECUÇÃO (DESTROY / PLAN)
# ======================================================
if ($Destroy) {
    Write-Title "MODO DESTROY"
    terraform init; Check-Last-Exit-Code
    terraform destroy -auto-approve; Check-Last-Exit-Code
    Write-Success "Infraestrutura destruída."
    exit 0
}

if ($Plan) {
    Write-Title "MODO PLAN"
    terraform init; Check-Last-Exit-Code
    terraform plan; Check-Last-Exit-Code
    Write-Success "Plano gerado."
    exit 0
}

# ======================================================
# ETAPA 3: DEPLOY FASE 1 - INFRAESTRUTURA BASE
# ======================================================
Write-Title "ETAPA 3: Deploy FASE 1 - Infraestrutura Base (EKS, RDS, Lambda)"
$env:TF_VAR_eks_cluster_role = "LabEksClusterRole"
$env:TF_VAR_eks_node_role    = "LabEksNodeRole"

# Descobre a role do Lambda dinamicamente
$LAMBDA_ROLE_NAME = "LabRole" # Role padrão do AWS Academy
$env:TF_VAR_lambda_execution_role_name = $LAMBDA_ROLE_NAME
Write-Step "Usando a role '$LAMBDA_ROLE_NAME' para a Lambda."

terraform init; Check-Last-Exit-Code
terraform validate; Check-Last-Exit-Code
Write-Step "Aplicando a infraestrutura base... Isso pode levar vários minutos."
terraform apply -auto-approve; Check-Last-Exit-Code
Write-Success "Infraestrutura base provisionada com sucesso."

# ======================================================
# ETAPA 4: CONFIGURAÇÃO PÓS-PROVISIONAMENTO
# ======================================================
Write-Title "ETAPA 4: Configuração Pós-Provisionamento"
$EKS_CLUSTER_NAME = terraform output -raw eks_cluster_name; Check-Last-Exit-Code
aws eks update-kubeconfig --region $AWS_REGION --name $EKS_CLUSTER_NAME; Check-Last-Exit-Code
Write-Success "kubectl configurado para o cluster '$EKS_CLUSTER_NAME'."

# ======================================================
# ETAPA 5: INICIALIZAÇÃO DO BANCO DE DADOS (VIA K8S JOB)
# ======================================================
Write-Title "ETAPA 5: Inicialização do Banco de Dados via Kubernetes Job"
Write-Step "Aguardando o job 'db-init-job' concluir... Isso pode levar um minuto."
kubectl wait --for=condition=complete job/db-init-job --timeout=300s
Check-Last-Exit-Code
Write-Success "Job de inicialização do banco de dados concluído."


# ======================================================
# ETAPA 6: DEPLOY DA APLICAÇÃO E DESCOBERTA DO ALB
# ======================================================
Write-Title "ETAPA 6: Deploy da Aplicação no EKS"
kubectl apply -f "..\k8s\"; Check-Last-Exit-Code
Write-Step "Aguardando o Application Load Balancer (ALB) ser provisionado..."
# ... (código de espera do ALB permanece o mesmo)

# ======================================================
# ETAPA 7: DEPLOY FASE 2 - INTEGRAÇÃO FINAL
# ======================================================
Write-Title "ETAPA 7: Deploy FASE 2 - Integração do API Gateway"
terraform apply -auto-approve -var="alb_hostname=$ALB_HOSTNAME"; Check-Last-Exit-Code
Write-Success "Integração do API Gateway concluída."

# ======================================================
# ETAPA 8: RESUMO FINAL DO DEPLOY
# ======================================================
Write-Title "ETAPA 8: Resumo do Deploy"
$ApiGatewayUrl = terraform output -raw api_gateway_invoke_url
$SwaggerUrl = "$ApiGatewayUrl/swagger"

Write-Host "✔️ EKS criado"
Write-Host "✔️ RDS criado"
Write-Host "✔️ Lambda criada"
Write-Host "✔️ API Gateway criado"
Write-Host ""
Write-Host "URL pública da API: $ApiGatewayUrl"
Write-Host "URL do Swagger: $SwaggerUrl"

Write-Title "Deploy finalizado com sucesso!"
