# ===================================================================
# Script de Deploy COMPLETO - MecanicaOS (AWS Academy)
#
# Orquestra o deploy de ponta a ponta:
# 1. Build e Push da imagem Docker para o ECR.
# 2. Deploy da infraestrutura com Terraform (VPC, EKS, RDS, Lambda, API GW).
# 3. Deploy da aplicação no Kubernetes.
# ===================================================================

param(
    [switch]$Destroy,
    [string]$AwsRegion = "us-east-1"
)

# Encerra o script em caso de erro
$ErrorActionPreference = "Stop"

# ======================================================
# FUNÇÕES DE LOG
# ======================================================
function Write-Title($msg) { Write-Host "`n============================================" -ForegroundColor Cyan; Write-Host " $msg" -ForegroundColor Cyan; Write-Host "============================================" -ForegroundColor Cyan }
function Write-Step($msg)  { Write-Host "`n==> $msg" -ForegroundColor Yellow }
function Write-Success($msg) { Write-Host "[OK] $msg" -ForegroundColor Green }
function Write-ErrorMsg($msg) { Write-Host "[X] $msg" -ForegroundColor Red }
function Write-Info($msg)  { Write-Host "    $msg" }

# ======================================================
# ETAPA 0: VERIFICAÇÃO DE PRÉ-REQUISITOS
# ======================================================
Write-Title "ETAPA 0: Verificando Pré-requisitos"

$requiredCommands = @("aws", "terraform", "kubectl", "docker")
foreach ($cmd in $requiredCommands) {
    if (-not (Get-Command $cmd -ErrorAction SilentlyContinue)) {
        Write-ErrorMsg "$cmd não encontrado no PATH. Por favor, instale-o."
        exit 1
    }
}
Write-Success "Todas as ferramentas necessárias estão instaladas."

# ======================================================
# ETAPA 1: MODO DESTROY
# ======================================================
if ($Destroy) {
    Write-Title "MODO DESTROY"
    Write-Step "Executando terraform destroy..."

    # Limpa o cache local do Terraform
    Remove-Item -Path ".terraform", ".terraform.lock.hcl" -Recurse -Force -ErrorAction SilentlyContinue

    terraform init -reconfigure
    terraform destroy -auto-approve

    Write-Success "Infraestrutura destruída."
    exit 0
}

# ======================================================
# ETAPA 2: BUILD & PUSH DA IMAGEM DOCKER
# ======================================================
Write-Title "ETAPA 2: Build e Push da Imagem Docker"

# Valida se o Docker está rodando
Write-Step "Verificando se o Docker Desktop está rodando..."
try {
    docker info > $null
    Write-Success "Docker daemon está ativo."
} catch {
    Write-ErrorMsg "Docker não está rodando. Por favor, inicie o Docker Desktop e tente novamente."
    exit 1
}

# Obtém informações da conta AWS
try {
    $callerIdentity = aws sts get-caller-identity --output json | ConvertFrom-Json
    $awsAccountId = $callerIdentity.Account
    Write-Success "AWS Account ID: $awsAccountId"
} catch {
    Write-ErrorMsg "Credenciais AWS inválidas. Configure suas credenciais e tente novamente."
    exit 1
}

$ecrRepoName = "mecanicaos-ecr"
$ecrRepoUrl = "${awsAccountId}.dkr.ecr.${AwsRegion}.amazonaws.com/${ecrRepoName}"
$imageTag = (git rev-parse --short HEAD)

# Login no ECR
try {
    Write-Step "Autenticando Docker no ECR..."
    aws ecr get-login-password --region $AwsRegion | docker login --username AWS --password-stdin $ecrRepoUrl
    Write-Success "Login no ECR bem-sucedido."
} catch {
    Write-ErrorMsg "Falha ao autenticar no ECR. Verifique suas permissões do IAM."
    throw
}

# Cria o repositório ECR se não existir
Write-Step "Verificando/Criando repositório ECR '$ecrRepoName'..."
try {
    aws ecr describe-repositories --repository-names $ecrRepoName --region $AwsRegion --output text > $null
    Write-Success "Repositório ECR já existe."
} catch {
    Write-Info "Repositório não encontrado, criando..."
    aws ecr create-repository --repository-name $ecrRepoName --region $AwsRegion --output text > $null
    Write-Success "Repositório ECR criado."
}

# Build e Push da Imagem
try {
    Write-Step "Construindo e enviando a imagem Docker (Tag: $imageTag)..."
    cd .. # Sobe para a raiz do projeto
    docker build -t "${ecrRepoUrl}:${imageTag}" .
    docker tag "${ecrRepoUrl}:${imageTag}" "${ecrRepoUrl}:latest"
    docker push "${ecrRepoUrl}:${imageTag}"
    docker push "${ecrRepoUrl}:latest"
    cd terraform # Volta para o diretório
    Write-Success "Build e Push concluídos."
} catch {
    Write-ErrorMsg "Falha durante o build ou push da imagem Docker."
    cd terraform # Garante que estamos no diretório certo em caso de falha
    throw
}

# ======================================================
# ETAPA 3: DEPLOY DA INFRAESTRUTURA (TERRAFORM)
# ======================================================
Write-Title "ETAPA 3: Deploy da Infraestrutura com Terraform"

# Limpa o cache local do Terraform para garantir um estado limpo
Remove-Item -Path ".terraform", ".terraform.lock.hcl" -Recurse -Force -ErrorAction SilentlyContinue

Write-Step "Executando terraform init..."
terraform init -reconfigure

Write-Step "Executando terraform apply..."
$tfVars = @{
    "docker_image_repo" = $ecrRepoUrl
    "docker_image_tag" = $imageTag
}

# Converte o hashtable para uma string de argumentos -var
$varString = ($tfVars.GetEnumerator() | ForEach-Object { "-var='$($_.Key)=$($_.Value)'" }) -join " "

terraform apply -auto-approve $varString
Write-Success "Infraestrutura implantada com sucesso."

# ======================================================
# ETAPA 4: CONFIGURAÇÃO DO KUBECTL
# ======================================================
Write-Title "ETAPA 4: Configurando Kubeconfig"

$eksClusterName = terraform output -raw eks_cluster_name
aws eks update-kubeconfig --region $AwsRegion --name $eksClusterName
Write-Success "Kubeconfig atualizado para o cluster '$eksClusterName'."

# ======================================================
# ETAPA 5: DEPLOY DA APLICAÇÃO NO KUBERNETES
# ======================================================
Write-Title "ETAPA 5: Deploy da Aplicação no Kubernetes"

# Obtém a connection string do RDS a partir do output do Terraform
$dbPassword = terraform output -raw rds_master_password
$dbHost = terraform output -raw rds_hostname
$dbPort = terraform output -raw rds_port
$dbUsername = terraform output -raw rds_username
$dbName = "postgres" # Nome padrão do banco de dados no módulo RDS

$connectionString = "Host=${dbHost};Port=${dbPort};Database=${dbName};Username=${dbUsername};Password=${dbPassword}"

# Cria o namespace 'mecanicaos' se não existir
Write-Step "Garantindo que o namespace 'mecanicaos' existe..."
kubectl apply -f ../k8s/namespace.yaml

# Cria ou atualiza o secret com a connection string
Write-Step "Criando/Atualizando o secret 'api-secret'..."
kubectl create secret generic api-secret `
    --from-literal=ConnectionStrings__DefaultConnection=$connectionString `
    --namespace=mecanicaos `
    --dry-run=client -o yaml | kubectl apply -f -

# Aplica os manifestos do Kubernetes
Write-Step "Aplicando manifestos Kubernetes..."
$k8sDir = "..\k8s"

# Substitui o placeholder da imagem no deployment
$deploymentTemplate = Get-Content -Path "$k8sDir\api-deployment.yaml" -Raw
$deploymentContent = $deploymentTemplate -replace '\${docker_image}', "${ecrRepoUrl}:${imageTag}"

# Aplica os manifestos restantes
Get-ChildItem -Path $k8sDir -Filter "*.yaml" | ForEach-Object {
    if ($_.Name -ne "api-deployment.yaml" -and $_.Name -ne "namespace.yaml") {
        kubectl apply -f $_.FullName
    }
}

# Aplica o deployment modificado
$deploymentContent | kubectl apply -f -

Write-Success "Aplicação implantada no Kubernetes."

# ======================================================
# ETAPA 6: INFORMAÇÕES FINAIS
# ======================================================
Write-Title "ETAPA 6: Deploy Concluído!"

$apiUrl = terraform output -raw api_gateway_invoke_url
$swaggerUrl = "${apiUrl}/docs/v1/swagger.json" # Corrigir a URL do swagger

Write-Host "URL da API Gateway:" -ForegroundColor Green
Write-Host $apiUrl

Write-Host "`nSwagger UI:" -ForegroundColor Green
Write-Host "${apiUrl}/docs" # URL correta da UI

Write-Host "`n--------------------------------------------------"
Write-Host "Aguarde alguns minutos para que o Load Balancer e os pods sejam inicializados."
Write-Host "Use 'kubectl get pods -n mecanicaos -w' para monitorar."
Write-Host "--------------------------------------------------"
