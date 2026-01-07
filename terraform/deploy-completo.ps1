# terraform/deploy-completo.ps1

# Etapa 0: Definição de Variáveis
$AWS_REGION = "us-east-1"
$CLUSTER_NAME = "mecanicaos-demo"
$ECR_REPOSITORY_NAME = "mecanicaos-api"
$DOCKER_IMAGE_NAME = "mecanicaos-api"

# Obter o Account ID da AWS
$AWS_ACCOUNT_ID = $(aws sts get-caller-identity --query Account --output text)
if (-not $?) {
    Write-Error "Erro ao obter o Account ID da AWS. Verifique suas credenciais."
    exit 1
}

$ECR_REGISTRY = "${AWS_ACCOUNT_ID}.dkr.ecr.${AWS_REGION}.amazonaws.com"
$DOCKER_IMAGE_TAG = "$(git rev-parse --short HEAD)"
$DOCKER_IMAGE_FULL_NAME = "${ECR_REGISTRY}/${ECR_REPOSITORY_NAME}:${DOCKER_IMAGE_TAG}"

# Etapa 1: Build e Push da Imagem Docker para o ECR
Write-Host "Iniciando build da imagem Docker..."
docker build -t $DOCKER_IMAGE_FULL_NAME -f ../Dockerfile ../
if (-not $?) {
    Write-Error "Erro no build da imagem Docker."
    exit 1
}
Write-Host "Build da imagem Docker concluído."

Write-Host "Autenticando no ECR..."
aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin $ECR_REGISTRY
if (-not $?) {
    Write-Error "Erro na autenticação com o ECR."
    exit 1
}
Write-Host "Autenticação no ECR concluída."

Write-Host "Criando repositório ECR (se não existir)..."
aws ecr describe-repositories --repository-names $ECR_REPOSITORY_NAME --region $AWS_REGION 2>$null
if (-not $?) {
    aws ecr create-repository --repository-name $ECR_REPOSITORY_NAME --region $AWS_REGION
}
Write-Host "Repositório ECR verificado."

Write-Host "Enviando imagem para o ECR..."
docker push $DOCKER_IMAGE_FULL_NAME
if (-not $?) {
    Write-Error "Erro ao enviar a imagem para o ECR."
    exit 1
}
Write-Host "Imagem enviada para o ECR com sucesso."


# Etapa 2: Instalar dependências da Lambda
Write-Host "Instalando dependências da função Lambda..."
Push-Location -Path "./lambda_authorizer"
pip install -r requirements.txt -t ./package
if (-not $?) {
    Write-Error "Erro ao instalar as dependências da Lambda."
    Pop-Location
    exit 1
}
# Copia o handler para o diretório de pacotes
Copy-Item -Path "lambda_handler.py" -Destination "./package/"
Pop-Location
Write-Host "Dependências da Lambda instaladas."


# Etapa 3: Deploy da Infraestrutura com Terraform
Write-Host "Iniciando deploy da infraestrutura com Terraform..."
terraform init
if (-not $?) {
    Write-Error "Erro no terraform init."
    exit 1
}

terraform apply -auto-approve -var="docker_image=${DOCKER_IMAGE_FULL_NAME}" -var="cluster_name=${CLUSTER_NAME}"
if (-not $?) {
    Write-Error "Erro no terraform apply."
    exit 1
}
Write-Host "Deploy da infraestrutura concluído."

# Etapa 4: Obter a URL da API e exibir
$API_URL = $(terraform output -raw api_gateway_url)

Write-Host "----------------------------------------------------"
Write-Host "               DEPLOYMENT CONCLUÍDO                 "
Write-Host "----------------------------------------------------"
Write-Host "URL da API Gateway: ${API_URL}"
Write-Host "Swagger UI: ${API_URL}/swagger"
Write-Host "----------------------------------------------------"
