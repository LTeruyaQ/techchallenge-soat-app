# ===================================================================
# Script de Deploy COMPLETO e AUTOMATIZADO - MecanicaOS (AWS Academy)
#
# Executa o ciclo completo:
# 1. Sanity Check do ambiente
# 2. Validação de credenciais AWS e ferramentas (Docker, etc.)
# 3. Descoberta automática de Roles do AWS Academy
# 4. Build e Push da imagem Docker para o ECR
# 5. Deploy de TODA a infraestrutura com Terraform (VPC, EKS, RDS, API GW, Lambda)
# 6. Deploy da aplicação no Kubernetes
# 7. Exibição dos endpoints e status final
#
# USO:
#   .\deploy-completo.ps1
#   .\deploy-completo.ps1 -Destroy (para destruir a infraestrutura)
#   .\deploy-completo.ps1 -Plan (para ver o plano do Terraform)
#
# ===================================================================

param(
    [switch]$Destroy,
    [switch]$Plan,
    [string]$AWS_REGION = "us-east-1"
)

# ======================================================
# CONFIGURAÇÕES
# ======================================================

# Nome do repositório ECR (deve ser o mesmo que está no terraform/variables.tf)
$ECR_REPO_NAME = "mecanicaos-ecr"

# Nome do projeto (usado para nomear o arquivo zip da lambda)
$PROJECT_NAME = "mecanicaos"

# Diretório raiz do projeto (onde o script está)
$PROJECT_ROOT = $PSScriptRoot
$SOURCE_ROOT = Resolve-Path (Join-Path $PROJECT_ROOT "..")


# ======================================================
# FUNÇÕES DE LOG E CONTROLE
# ======================================================

$ErrorActionPreference = "Stop"

function Write-Title($msg) {
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host " $msg" -ForegroundColor Cyan
    Write-Host "============================================" -ForegroundColor Cyan
}
function Write-Step($msg)     { Write-Host "`n==> $msg" -ForegroundColor Yellow }
function Write-Success($msg)  { Write-Host "[OK] $msg" -ForegroundColor Green }
function Write-Warning($msg)  { Write-Host "[!] $msg" -ForegroundColor DarkYellow }
function Write-ErrorMsg($msg) { Write-Host "[X] ERRO: $msg" -ForegroundColor Red }
function Write-Info($msg)     { Write-Host "    $msg" -ForegroundColor Gray }

function Exit-Script($message, $exitCode = 1) {
    Write-ErrorMsg $message
    exit $exitCode
}

# ======================================================
# ETAPA 0: VERIFICAÇÃO DE FERRAMENTAS
# ======================================================

Write-Title "ETAPA 0: Verificando ferramentas"

$requiredCmds = @("aws", "terraform", "kubectl", "docker", "zip")
foreach ($cmd in $requiredCmds) {
    if (-not (Get-Command $cmd -ErrorAction SilentlyContinue)) {
        Exit-Script "$cmd não encontrado no PATH. Por favor, instale-o."
    }
}
Write-Success "Todas as ferramentas necessárias foram encontradas."

try {
    docker info | Out-Null
    Write-Success "Docker Daemon está rodando."
} catch {
    Exit-Script "Docker Daemon não parece estar rodando."
}

# ======================================================
# ETAPA 1: VALIDANDO CREDENCIAIS AWS
# ======================================================

Write-Title "ETAPA 1: Validando credenciais AWS"

try {
    $identity = aws sts get-caller-identity --output json | ConvertFrom-Json
    $AWS_ACCOUNT_ID = $identity.Account
    Write-Success "Credenciais válidas para a conta AWS: $AWS_ACCOUNT_ID"
} catch {
    Exit-Script "Credenciais AWS inválidas ou não configuradas. Verifique seu ambiente."
}

# ======================================================
# ETAPA 2: MODO DE EXECUÇÃO (DESTROY / PLAN)
# ======================================================

if ($Destroy) {
    Write-Title "MODO DESTROY"
    Write-Step "Inicializando o Terraform"
    terraform init -upgrade
    Write-Step "Destruindo a infraestrutura..."
    terraform destroy -auto-approve
    Write-Success "Infraestrutura destruída."
    exit 0
}

if ($Plan) {
    Write-Title "MODO PLAN"
    Write-Step "Inicializando o Terraform"
    terraform init -upgrade
    Write-Step "Gerando o plano do Terraform..."
    terraform plan
    Write-Success "Plano gerado."
    exit 0
}

# ======================================================
# ETAPA 3: DESCOBERTA AUTOMÁTICA DE ROLES (AWS ACADEMY)
# ======================================================

Write-Title "ETAPA 3: Descoberta de IAM Roles (AWS Academy)"

try {
    Write-Step "Procurando pela role do Cluster EKS..."
    $clusterRoleName = aws iam list-roles --query "Roles[?contains(RoleName, 'LabEksClusterRole')].RoleName | [0]" --output text
    if ($clusterRoleName -eq "None" -or -not $clusterRoleName) {
        Exit-Script "LabEksClusterRole não encontrada. Verifique se você está em um ambiente AWS Academy."
    }
    Write-Success "Encontrada EKS Cluster Role: $clusterRoleName"

    Write-Step "Procurando pela role dos Nodes EKS..."
    $nodeRoleName = aws iam list-roles --query "Roles[?contains(RoleName, 'LabEksNodeRole')].RoleName | [0]" --output text
    if ($nodeRoleName -eq "None" -or -not $nodeRoleName) {
        Exit-Script "LabEksNodeRole não encontrada. Verifique se você está em um ambiente AWS Academy."
    }
    Write-Success "Encontrada EKS Node Role: $nodeRoleName"

    # Exporta as roles como variáveis de ambiente para o Terraform
    $env:TF_VAR_eks_cluster_role_name = $clusterRoleName
    $env:TF_VAR_eks_node_role_name = $nodeRoleName

} catch {
    Exit-Script "Falha ao buscar as roles do IAM. Detalhes: $($_.Exception.Message)"
}


# ======================================================
# ETAPA 4: EMPACOTANDO LAMBDA AUTHORIZER
# ======================================================
Write-Title "ETAPA 4: Empacotando Lambda Authorizer"
$lambdaDir = Join-Path $PROJECT_ROOT ".." "lambda_authorizer"
$zipFile = Join-Path $PROJECT_ROOT "lambda_authorizer.zip"

if (Test-Path $zipFile) {
    Write-Step "Removendo arquivo zip antigo da Lambda..."
    Remove-Item $zipFile
}

Write-Step "Instalando dependências da Lambda..."
pip install -r (Join-Path $lambdaDir "requirements.txt") -t $lambdaDir

Write-Step "Criando o arquivo lambda_authorizer.zip..."
Compress-Archive -Path "$lambdaDir/*" -DestinationPath $zipFile -Force

Write-Step "Limpando dependências instaladas..."
Get-ChildItem -Path $lambdaDir -Exclude "authorizer.py", "requirements.txt" | Remove-Item -Recurse -Force

Write-Success "Lambda empacotada em $zipFile"


# ======================================================
# ETAPA 5: BUILD E PUSH DA IMAGEM DOCKER
# ======================================================

Write-Title "ETAPA 5: Build e Push da Imagem Docker"

$IMAGE_TAG = (Get-Date -Format "yyyyMMddHHmmss")
$ECR_URI = "{0}.dkr.ecr.{1}.amazonaws.com/{2}" -f $AWS_ACCOUNT_ID, $AWS_REGION, $ECR_REPO_NAME

Write-Step "Inicializando Terraform para garantir que o ECR existe..."
terraform init -upgrade
terraform apply -target=aws_ecr_repository.app -auto-approve

Write-Step "Autenticando Docker no ECR..."
aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $ECR_URI

Write-Step "Construindo a imagem Docker ($ECR_URI:$IMAGE_TAG)..."
docker build -t "$ECR_URI:$IMAGE_TAG" -f (Join-Path $SOURCE_ROOT "Dockerfile") $SOURCE_ROOT

Write-Step "Enviando imagem para o ECR..."
docker push "$ECR_URI:$IMAGE_TAG"
docker tag "$ECR_URI:$IMAGE_TAG" "$ECR_URI:latest"
docker push "$ECR_URI:latest"

Write-Success "Imagem enviada para o ECR com as tags '$IMAGE_TAG' e 'latest'."


# ======================================================
# ETAPA 6: DEPLOY DA INFRAESTRUTURA COM TERRAFORM
# ======================================================

Write-Title "ETAPA 6: Deploy da Infraestrutura Principal"
Write-Step "Executando terraform apply..."
# Passa a URL completa da imagem para o Terraform
$env:TF_VAR_docker_image_url = "$ECR_URI:$IMAGE_TAG"
terraform apply -auto-approve

Write-Success "Infraestrutura provisionada com sucesso."

# ======================================================
# ETAPA 7: CONFIGURANDO KUBECTL
# ======================================================

Write-Title "ETAPA 7: Configurando Kubeconfig"
try {
    $eksClusterName = terraform output -raw eks_cluster_name
    aws eks update-kubeconfig --region $AWS_REGION --name $eksClusterName
    Write-Success "Kubeconfig atualizado para o cluster '$eksClusterName'."
} catch {
    Exit-Script "Falha ao configurar o kubectl. O cluster EKS pode não ter sido criado corretamente."
}

# ======================================================
# ETAPA 8: RELATÓRIO FINAL
# ======================================================

Write-Title "ETAPA 8: Deploy Concluído!"

$apiUrl = terraform output -raw api_gateway_invoke_url
$swaggerUrl = terraform output -raw swagger_url
$rdsEndpoint = terraform output -raw rds_endpoint

Write-Host ""
Write-Success "API Gateway URL: $apiUrl"
Write-Success "Swagger UI URL:  $swaggerUrl"
Write-Info    "RDS Endpoint:    $rdsEndpoint"
Write-Host ""
Write-Warning "Pode levar alguns minutos para o Load Balancer do EKS estar totalmente funcional e responder."
Write-Host ""
