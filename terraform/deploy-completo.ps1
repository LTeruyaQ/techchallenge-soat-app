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

# ======================================================
# FUNÇÕES DE LOG E CONTROLE
# ======================================================
function Write-Title($msg) { Write-Host "`n============================================" -ForegroundColor Cyan; Write-Host " $msg" -ForegroundColor Cyan; Write-Host "============================================" -ForegroundColor Cyan }
function Write-Step($msg)  { Write-Host "`n==> $msg" -ForegroundColor Yellow }
function Write-Success($msg) { Write-Host "[OK] $msg" -ForegroundColor Green }
function Write-ErrorAndExit($msg) { Write-Host "[X] ERRO: $msg" -ForegroundColor Red; exit 1 }
function Write-Info($msg)  { Write-Host "    $msg" }

# Função para executar comandos e verificar o código de saída
function Exec([scriptblock]$cmd, $errorMessage) {
    & $cmd
    if ($LASTEXITCODE -ne 0) {
        Write-ErrorAndExit "$errorMessage (Código de saída: $LASTEXITCODE)"
    }
}

# ======================================================
# ETAPA 0: VERIFICAÇÃO DE PRÉ-REQUISITOS
# ======================================================
Write-Title "ETAPA 0: Verificando Pré-requisitos"

$requiredCommands = @("aws", "terraform", "kubectl", "docker")
foreach ($cmd in $requiredCommands) {
    Get-Command $cmd -ErrorAction SilentlyContinue > $null
    if ($LASTEXITCODE -ne 0) {
        Write-ErrorAndExit "$cmd não encontrado no PATH. Por favor, instale-o."
    }
}
Write-Success "Todas as ferramentas necessárias estão instaladas."

# ======================================================
# ETAPA 1: MODO DESTROY
# ======================================================
if ($Destroy) {
    Write-Title "MODO DESTROY"
    Write-Step "Executando terraform destroy..."

    Remove-Item -Path ".terraform", ".terraform.lock.hcl" -Recurse -Force -ErrorAction SilentlyContinue

    Exec { terraform init -reconfigure } "Falha ao inicializar o Terraform"
    Exec { terraform destroy -auto-approve } "Falha ao destruir a infraestrutura"

    Write-Success "Infraestrutura destruída."
    exit 0
}

# ======================================================
# ETAPA 2: BUILD & PUSH DA IMAGEM DOCKER
# ======================================================
Write-Title "ETAPA 2: Build e Push da Imagem Docker"

Write-Step "Verificando se o Docker Desktop está rodando..."
Exec { docker info } "O Docker não parece estar rodando. Por favor, inicie o Docker Desktop."

Write-Step "Validando credenciais AWS..."
$callerIdentityJson = Exec { aws sts get-caller-identity --output json } "Falha ao obter identidade do AWS CLI. Verifique suas credenciais."
$callerIdentity = $callerIdentityJson | ConvertFrom-Json
$awsAccountId = $callerIdentity.Account
Write-Success "AWS Account ID: $awsAccountId"

$ecrRepoName = "mecanicaos-ecr"
$ecrRepoUrl = "${awsAccountId}.dkr.ecr.${AwsRegion}.amazonaws.com/${ecrRepoName}"
$imageTag = Exec { git rev-parse --short HEAD } "Falha ao obter o hash do commit git."

Write-Step "Autenticando Docker no ECR..."
Exec { aws ecr get-login-password --region $AwsRegion | docker login --username AWS --password-stdin $ecrRepoUrl } "Falha ao autenticar no ECR."

Write-Step "Verificando/Criando repositório ECR '$ecrRepoName'..."
aws ecr describe-repositories --repository-names $ecrRepoName --region $AwsRegion --output text > $null
if ($LASTEXITCODE -ne 0) {
    Write-Info "Repositório não encontrado, criando..."
    Exec { aws ecr create-repository --repository-name $ecrRepoName --region $AwsRegion --output text } "Falha ao criar o repositório ECR."
} else {
    Write-Success "Repositório ECR já existe."
}

Write-Step "Construindo e enviando a imagem Docker (Tag: $imageTag)..."
cd ..
Exec { docker build -t "${ecrRepoUrl}:${imageTag}" . } "Falha ao construir a imagem Docker."
Exec { docker tag "${ecrRepoUrl}:${imageTag}" "${ecrRepoUrl}:latest" } "Falha ao criar a tag 'latest'."
Exec { docker push "${ecrRepoUrl}:${imageTag}" } "Falha ao enviar a imagem para o ECR (tag: $imageTag)."
Exec { docker push "${ecrRepoUrl}:latest" } "Falha ao enviar a imagem para o ECR (tag: latest)."
cd terraform
Write-Success "Build e Push concluídos."

# ======================================================
# ETAPA 3: DEPLOY DA INFRAESTRUTURA (TERRAFORM)
# ======================================================
Write-Title "ETAPA 3: Deploy da Infraestrutura com Terraform"

Remove-Item -Path ".terraform", ".terraform.lock.hcl" -Recurse -Force -ErrorAction SilentlyContinue

Write-Step "Executando terraform init..."
Exec { terraform init -reconfigure } "Falha ao inicializar o Terraform."

Write-Step "Executando terraform apply..."
$tfVars = @{
    "docker_image_repo" = $ecrRepoUrl
    "docker_image_tag" = $imageTag
}
$varString = ($tfVars.GetEnumerator() | ForEach-Object { "-var=`"$($_.Key)=$($_.Value)`"" }) -join " "
Exec { terraform apply -auto-approve $varString } "Falha ao aplicar a configuração do Terraform."
Write-Success "Infraestrutura implantada com sucesso."

# ======================================================
# ETAPA 4: DEPLOY NO KUBERNETES
# ======================================================
Write-Title "ETAPA 4: Deploy no Kubernetes"

Write-Step "Configurando Kubeconfig..."
$eksClusterName = Exec { terraform output -raw eks_cluster_name } "Falha ao obter o nome do cluster EKS do Terraform."
Exec { aws eks update-kubeconfig --region $AwsRegion --name $eksClusterName } "Falha ao configurar o kubeconfig."

Write-Step "Obtendo connection string do RDS..."
$dbPassword = Exec { terraform output -raw rds_master_password } "Falha ao obter a senha do RDS."
$dbHost = Exec { terraform output -raw rds_hostname } "Falha ao obter o hostname do RDS."
$dbPort = Exec { terraform output -raw rds_port } "Falha ao obter a porta do RDS."
$dbUsername = Exec { terraform output -raw rds_username } "Falha ao obter o usuário do RDS."
$dbName = "postgres"
$connectionString = "Host=${dbHost};Port=${dbPort};Database=${dbName};Username=${dbUsername};Password=${dbPassword}"

Write-Step "Aplicando manifestos no Kubernetes..."
Exec { kubectl apply -f ../k8s/namespace.yaml } "Falha ao aplicar o namespace."

Exec { kubectl create secret generic api-secret --from-literal=ConnectionStrings__DefaultConnection=$connectionString --namespace=mecanicaos --dry-run=client -o yaml | kubectl apply -f - } "Falha ao criar o secret da API."

$k8sDir = "..\k8s"
$deploymentTemplate = Get-Content -Path "$k8sDir\api-deployment.yaml" -Raw
$deploymentContent = $deploymentTemplate -replace '\${docker_image}', "${ecrRepoUrl}:${imageTag}"

Get-ChildItem -Path $k8sDir -Filter "*.yaml" | ForEach-Object {
    if ($_.Name -ne "api-deployment.yaml" -and $_.Name -ne "namespace.yaml") {
        Exec { kubectl apply -f $_.FullName } "Falha ao aplicar o manifesto $($_.Name)."
    }
}

Exec { $deploymentContent | kubectl apply -f - } "Falha ao aplicar o deployment da API."
Write-Success "Aplicação implantada no Kubernetes."

# ======================================================
# ETAPA 5: RESUMO DO DEPLOY
# ======================================================
Write-Title "ETAPA 5: Resumo do Deploy"

Write-Step "Obtendo informações dos recursos criados..."

$apiUrl = Exec { terraform output -raw api_gateway_invoke_url } "Falha ao obter a URL do API Gateway."
$eksClusterName = Exec { terraform output -raw eks_cluster_name } "Falha ao obter o nome do cluster EKS."
$rdsHostname = Exec { terraform output -raw rds_hostname } "Falha ao obter o hostname do RDS."

Write-Success "Deploy finalizado com sucesso!"
Write-Host "--------------------------------------------------"
Write-Host "  Recursos Criados:"
Write-Host "  - EKS Cluster Name: " -NoNewline; Write-Host $eksClusterName -ForegroundColor Yellow
Write-Host "  - RDS Endpoint:     " -NoNewline; Write-Host $rdsHostname -ForegroundColor Yellow
Write-Host "  - API Gateway URL:  " -NoNewline; Write-Host $apiUrl -ForegroundColor Yellow
Write-Host "  - Swagger UI:       " -NoNewline; Write-Host "${apiUrl}/docs" -ForegroundColor Yellow
Write-Host "--------------------------------------------------"
Write-Host "`nAguarde alguns minutos para que o Load Balancer e os pods sejam inicializados."
Write-Host "Use 'kubectl get pods -n mecanicaos -w' para monitorar."
Write-Host "--------------------------------------------------"
