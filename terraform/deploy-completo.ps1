# ============================================
# Script de Deploy COMPLETO - MecanicaOS (AWS Academy)
# Versão com Autodetecção e Lógica Condicional
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
function Write-Warning($msg)  { Write-Host "[!] $msg" -ForegroundColor Yellow }
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

# Inicializa o Terraform uma única vez, antes de qualquer operação
Write-Step "Inicializando o Terraform..."
terraform init; Check-Last-Exit-Code

# --- Bloco de autodetecção/import para evitar recriar recursos existentes ---
Write-Title "AUTODETECÇÃO: Verificando recursos AWS existentes (EKS, IAM Role)"

# util
function Exec-AwsSafe([string]$cmd){
    try {
        # Redireciona o erro para o null stream para evitar poluir o console com erros esperados (ex: recurso não encontrado)
        $out = Invoke-Expression "$cmd 2>`$null"
        return $out
    } catch {
        return $null
    }
}

# 1) EKS: existe? -> se existir, tentamos importar para o state se necessário
$eksNameCandidate = "eks-mecanicaos" # Nome fixo conforme o código Terraform
$eksDescribe = Exec-AwsSafe "aws eks describe-cluster --name $eksNameCandidate --region $AWS_REGION --output json"
if ($eksDescribe) {
    Write-Warning "Cluster EKS '$eksNameCandidate' encontrado. O Terraform tentará reutilizá-lo."
    $env:TF_VAR_skip_create_eks = "true"

    $hasState = Exec-AwsSafe "terraform state list | Select-String 'aws_eks_cluster.eks' -Quiet"
    if (-not $hasState) {
        Write-Step "Tentando importar o cluster EKS existente para o estado do Terraform..."
        try {
            terraform import aws_eks_cluster.eks $eksNameCandidate
            Check-Last-Exit-Code
            Write-Success "Importação do cluster EKS para o state do Terraform concluída."
        } catch {
            Write-ErrorMsg "Falha ao importar o cluster EKS. Pode ser necessário importar manualmente: terraform import aws_eks_cluster.eks $eksNameCandidate"
            exit 1
        }
    } else {
        Write-Step "O cluster EKS já está presente no state do Terraform."
    }
} else {
    Write-Step "Cluster EKS '$eksNameCandidate' não encontrado. O Terraform criará um novo."
    $env:TF_VAR_skip_create_eks = "false"
}

# 2) IAM Role para Lambda: existe?
$lambdaRoleName = "mecanicaos-lambda-exec-role" # Nome fixo conforme o código Terraform
$roleInfo = Exec-AwsSafe "aws iam get-role --role-name $lambdaRoleName --output json"
if ($roleInfo) {
    Write-Success "IAM Role '$lambdaRoleName' encontrada. O Terraform irá reutilizá-la."
    $env:TF_VAR_use_existing_lambda_role = "true"
    $env:TF_VAR_lambda_role_name = $lambdaRoleName
} else {
    Write-Warning "IAM Role '$lambdaRoleName' não encontrada. O Terraform tentará criá-la."
    # A verificação de permissão real ocorrerá durante o 'apply'. Se falhar, o Terraform fornecerá o erro 'AccessDenied'.
    $env:TF_VAR_use_existing_lambda_role = "false"
}

Write-Success "Autodetecção concluída."
# --- Fim do bloco de autodetecção ---


# ======================================================
# ETAPA 3: DEPLOY TERRAFORM
# ======================================================
Write-Title "ETAPA 3: Deploy da Infraestrutura com Terraform"

# Define as roles do AWS Academy que são fixas
$env:TF_VAR_eks_cluster_role = "LabEksClusterRole"
$env:TF_VAR_eks_node_role    = "LabEksNodeRole"

terraform validate; Check-Last-Exit-Code

Write-Step "Aplicando a configuração da infraestrutura... Isso pode levar vários minutos."
terraform apply -auto-approve; Check-Last-Exit-Code
Write-Success "Infraestrutura provisionada com sucesso."

# ======================================================
# ETAPA 4: CONFIGURAÇÃO PÓS-PROVISIONAMENTO
# ======================================================
Write-Title "ETAPA 4: Configuração Pós-Provisionamento"
$EKS_CLUSTER_NAME = terraform output -raw eks_cluster_name; Check-Last-Exit-Code
aws eks update-kubeconfig --region $AWS_REGION --name $EKS_CLUSTER_NAME; Check-Last-Exit-Code
Write-Success "kubectl configurado para o cluster '$EKS_CLUSTER_NAME'."

# ... (código de inicialização do DB via Job permanece o mesmo)

# ======================================================
# ETAPA 5: DEPLOY DA APLICAÇÃO E INTEGRAÇÃO
# ======================================================
Write-Title "ETAPA 5: Deploy da Aplicação no EKS e Integração do API Gateway"
kubectl apply -f "..\k8s\"; Check-Last-Exit-Code

Write-Step "Aguardando o Application Load Balancer (ALB) ser provisionado pelo Ingress..."
$ALB_HOSTNAME = ""
for ($i=0; $i -lt 30; $i++) {
    $ALB_HOSTNAME = kubectl get ingress api-ingress -n default -o jsonpath='{.status.loadBalancer.ingress[0].hostname}'
    if ($ALB_HOSTNAME) {
        Write-Success "ALB provisionado com o hostname: $ALB_HOSTNAME"
        break
    }
    Write-Host "." -NoNewline
    Start-Sleep -Seconds 10
}
if (-not $ALB_HOSTNAME) {
    Write-ErrorMsg "Timeout esperando pelo ALB. Verifique os logs do Ingress Controller."
    exit 1
}

Write-Step "Executando a segunda fase do 'apply' para integrar o API Gateway com o ALB."
terraform apply -auto-approve -var="alb_hostname=$ALB_HOSTNAME"; Check-Last-Exit-Code
Write-Success "Integração do API Gateway concluída."

# ======================================================
# ETAPA 6: RESUMO FINAL DO DEPLOY
# ======================================================
Write-Title "ETAPA 6: Resumo do Deploy"
$ApiGatewayUrl = terraform output -raw api_gateway_endpoint
$RdsEndpoint = terraform output -raw rds_endpoint
$SwaggerUrl = "$ApiGatewayUrl/swagger"

Write-Host "✔️ EKS Cluster Name: $EKS_CLUSTER_NAME"
Write-Host "✔️ RDS Endpoint: $RdsEndpoint"
Write-Host "✔️ API Gateway (URL Pública): $ApiGatewayUrl"
Write-Host "✔️ Swagger UI: $SwaggerUrl"

Write-Title "Deploy finalizado com sucesso!"
