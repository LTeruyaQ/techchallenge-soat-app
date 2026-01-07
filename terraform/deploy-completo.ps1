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
    throw
}

# Obtém informações da conta AWS
try {
    $callerIdentity = aws sts get-caller-identity --output json | ConvertFrom-Json
    $awsAccountId = $callerIdentity.Account
    Write-Success "AWS Account ID: $awsAccountId"
} catch {
    Write-ErrorMsg "Credenciais AWS inválidas. Configure suas credenciais e tente novamente."
    throw
}

$ecrRepoName = "mecanicaos-ecr"
$ecrRepoUrl = "${awsAccountId}.dkr.ecr.${AwsRegion}.amazonaws.com/${ecrRepoName}"
$imageTag = (git rev-parse --short HEAD)

# Login no ECR
Write-Step "Autenticando Docker no ECR..."
try {
    aws ecr get-login-password --region $AwsRegion | docker login --username AWS --password-stdin $ecrRepoUrl | Out-Null
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
    try {
        Write-Info "Repositório não encontrado, criando..."
        aws ecr create-repository --repository-name $ecrRepoName --region $AwsRegion --output text > $null
        Write-Success "Repositório ECR criado."
    } catch {
        Write-ErrorMsg "Falha ao criar o repositório ECR."
        throw
    }
}

# Build e Push da Imagem
Write-Step "Construindo e enviando a imagem Docker (Tag: $imageTag)..."
try {
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

try {
    Write-Step "Executando terraform init..."
    terraform init -reconfigure

    Write-Step "Executando terraform apply..."
    $tfVars = @{
        "docker_image_repo" = $ecrRepoUrl
        "docker_image_tag" = $imageTag
    }

    # Converte o hashtable para uma string de argumentos -var
    $varString = ($tfVars.GetEnumerator() | ForEach-Object { "-var=`"$($_.Key)=$($_.Value)`"" }) -join " "

    terraform apply -auto-approve $varString
    Write-Success "Infraestrutura implantada com sucesso."
} catch {
    Write-ErrorMsg "Falha durante a execução do Terraform. A infraestrutura pode estar em um estado parcial."
    throw
}

# ======================================================
# ETAPA 4: CONFIGURAÇÃO DO KUBECTL E DEPLOY K8S
# ======================================================
Write-Title "ETAPA 4: Deploy no Kubernetes"

try {
    Write-Step "Configurando Kubeconfig..."
    $eksClusterName = terraform output -raw eks_cluster_name
    aws eks update-kubeconfig --region $AwsRegion --name $eksClusterName
    Write-Success "Kubeconfig atualizado para o cluster '$eksClusterName'."

    Write-Step "Obtendo connection string do RDS..."
    $dbPassword = terraform output -raw rds_master_password
    $dbHost = terraform output -raw rds_hostname
    $dbPort = terraform output -raw rds_port
    $dbUsername = terraform output -raw rds_username
    $dbName = "postgres"
    $connectionString = "Host=${dbHost};Port=${dbPort};Database=${dbName};Username=${dbUsername};Password=${dbPassword}"

    Write-Step "Aplicando manifestos no Kubernetes..."
    kubectl apply -f ../k8s/namespace.yaml

    kubectl create secret generic api-secret `
        --from-literal=ConnectionStrings__DefaultConnection=$connectionString `
        --namespace=mecanicaos `
        --dry-run=client -o yaml | kubectl apply -f -

    $k8sDir = "..\k8s"
    $deploymentTemplate = Get-Content -Path "$k8sDir\api-deployment.yaml" -Raw
    $deploymentContent = $deploymentTemplate -replace '\${docker_image}', "${ecrRepoUrl}:${imageTag}"

    Get-ChildItem -Path $k8sDir -Filter "*.yaml" | ForEach-Object {
        if ($_.Name -ne "api-deployment.yaml" -and $_.Name -ne "namespace.yaml") {
            kubectl apply -f $_.FullName
        }
    }

    $deploymentContent | kubectl apply -f -
    Write-Success "Aplicação implantada no Kubernetes."

} catch {
    Write-ErrorMsg "Falha durante o deploy no Kubernetes. Verifique a conexão com o cluster e os logs do kubectl."
    throw
}

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
