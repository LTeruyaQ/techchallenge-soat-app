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
    Write-Step "Limpando estado anterior do Terraform..."
    Remove-Item -Recurse -Force -Path ".terraform" -ErrorAction SilentlyContinue
    Remove-Item -Force -Path "terraform.tfstate*", "terraform.tfstate.d", "terraform.tfvars" -ErrorAction SilentlyContinue
    Write-Success "Estado limpo."
    terraform init; Check-Last-Exit-Code
    terraform destroy -auto-approve; Check-Last-Exit-Code
    Write-Success "Infraestrutura destruída."
    exit 0
}

if ($Plan) {
    Write-Title "MODO PLAN"
    Write-Step "Limpando estado anterior do Terraform..."
    Remove-Item -Recurse -Force -Path ".terraform" -ErrorAction SilentlyContinue
    Remove-Item -Force -Path "terraform.tfstate*", "terraform.tfstate.d", "terraform.tfvars" -ErrorAction SilentlyContinue
    Write-Success "Estado limpo."
    terraform init; Check-Last-Exit-Code
    terraform plan; Check-Last-Exit-Code
    Write-Success "Plano gerado."
    exit 0
}

# ======================================================
# ETAPA 3: DEPLOY DA INFRAESTRUTURA COMPLETA
# ======================================================
Write-Title "ETAPA 3: Deploy da Infraestrutura (VPC, EKS, RDS, Lambda, API GW)"
Write-Step "Limpando estado anterior do Terraform..."
Remove-Item -Recurse -Force -Path ".terraform" -ErrorAction SilentlyContinue
Remove-Item -Force -Path "terraform.tfstate*", "terraform.tfstate.d", "terraform.tfvars" -ErrorAction SilentlyContinue
Write-Success "Estado limpo."
terraform init; Check-Last-Exit-Code
terraform validate; Check-Last-Exit-Code
Write-Step "Aplicando a configuração do Terraform... Isso pode levar vários minutos."
terraform apply -auto-approve; Check-Last-Exit-Code
Write-Success "Infraestrutura provisionada com sucesso."

# ======================================================
# ETAPA 4: CONFIGURANDO KUBECTL
# ======================================================
Write-Title "ETAPA 4: Configurando kubectl"
$EKS_CLUSTER_NAME = terraform output -raw eks_cluster_name; Check-Last-Exit-Code
aws eks update-kubeconfig --region $AWS_REGION --name $EKS_CLUSTER_NAME; Check-Last-Exit-Code
Write-Success "kubectl configurado para o cluster '$EKS_CLUSTER_NAME'."

# ======================================================
# ETAPA 5: DEPLOY DA APLICAÇÃO NO EKS
# ======================================================
Write-Title "ETAPA 5: Deploy da Aplicação no EKS"
kubectl apply -f "..\k8s\"; Check-Last-Exit-Code
Write-Step "Aguardando alguns segundos para os pods da aplicação iniciarem..."
Start-Sleep -Seconds 30
Write-Success "Deploy da aplicação enviado ao EKS."

# ======================================================
# ETAPA 6: INICIALIZAÇÃO DO BANCO DE DADOS
# ======================================================
Write-Title "ETAPA 6: Inicialização do Banco de Dados"
Write-Step "Obtendo detalhes de conexão do RDS..."
$RDSEndpoint = terraform output -raw rds_endpoint; Check-Last-Exit-Code
$RDSUsername = terraform output -raw rds_username; Check-Last-Exit-Code
$RDSPassword = terraform output -raw rds_password; Check-Last-Exit-Code
$DBName      = terraform output -raw rds_dbname; Check-Last-Exit-Code

Write-Step "Executando script SQL (rds-init.sql)..."
try {
    $env:PGPASSWORD = $RDSPassword
    psql --host=$RDSEndpoint --port=5432 --username=$RDSUsername --dbname=$DBName -f ".\rds-init.sql"
    Check-Last-Exit-Code
    Write-Success "Banco de dados inicializado com sucesso."
} catch {
    Write-ErrorMsg "Falha ao executar o script SQL. Verifique se 'psql' está instalado e no PATH."
    throw
} finally {
    Remove-Item Env:\PGPASSWORD
}

# ======================================================
# ETAPA 7: RESUMO FINAL DO DEPLOY
# ======================================================
Write-Title "ETAPA 7: Resumo do Deploy"
$ApiGatewayUrl = terraform output -raw api_gateway_endpoint; Check-Last-Exit-Code
$SwaggerUrl = "$ApiGatewayUrl/swagger"

Write-Success "✔️ EKS criado: $EKS_CLUSTER_NAME"
Write-Success "✔️ RDS criado: $RDSEndpoint"
Write-Success "✔️ Lambda criada"
Write-Success "✔️ API Gateway criado"
Write-Info "URL pública da API: $ApiGatewayUrl"
Write-Info "URL do Swagger: $SwaggerUrl"

Write-Title "Deploy finalizado com sucesso!"
