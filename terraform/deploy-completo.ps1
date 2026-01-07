# ============================================
# Script de Deploy COMPLETO - MecanicaOS (AWS Academy)
# ============================================

param(
    [switch]$Destroy,
    [string]$AWS_REGION = "us-east-1"
)

$ErrorActionPreference = "Stop"

# ======================================================
# FUNÇÕES DE LOG
# ======================================================

function Write-Title($msg) {
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host " $msg" -ForegroundColor Cyan
    Write-Host "============================================" -ForegroundColor Cyan
}
function Write-Step($msg)     { Write-Host "`n==> $msg" -ForegroundColor Yellow }
function Write-Success($msg)  { Write-Host "[OK] $msg" -ForegroundColor Green }
function Write-ErrorMsg($msg) { Write-Host "[X] $msg" -ForegroundColor Red }
function Write-Info($msg)     { Write-Host "    $msg" -ForegroundColor Gray }

# ======================================================
# ETAPA 0: VERIFICAÇÃO DE PRÉ-REQUISITOS
# ======================================================

Write-Title "ETAPA 0: Verificando Pré-requisitos"

foreach ($cmd in @("aws", "terraform", "kubectl", "git")) {
    if (-not (Get-Command $cmd -ErrorAction SilentlyContinue)) {
        Write-ErrorMsg "$cmd não encontrado no PATH"
        exit 1
    }
}

if (-not $env:GIT_TOKEN) {
    Write-ErrorMsg "A variável de ambiente GIT_TOKEN é obrigatória para o build com Kaniko."
    exit 1
}

# ======================================================
# ETAPA 1: CREDENCIAIS AWS E VARIÁVEIS
# ======================================================

Write-Title "ETAPA 1: Validando Credenciais AWS e Configurando Variáveis"

try {
    $identity = aws sts get-caller-identity --output json | ConvertFrom-Json
    $AWS_ACCOUNT_ID = $identity.Account
    Write-Success "AWS Account ID: $AWS_ACCOUNT_ID"
} catch {
    Write-ErrorMsg "Credenciais AWS inválidas"
    exit 1
}

$ECR_REPO_NAME = "mecanicaos-ecr"
$IMAGE_TAG = (git rev-parse --short HEAD)
$GIT_REMOTE_URL = git remote get-url origin
$GITHUB_USER = ($GIT_REMOTE_URL -split '/')[-2]
$REPO_NAME = ($GIT_REMOTE_URL -split '/')[-1].Replace(".git", "")

# ======================================================
# ETAPA 2: MODO DESTROY
# ======================================================

if ($Destroy) {
    Write-Title "MODO DESTROY"
    terraform init
    terraform destroy -auto-approve
    exit 0
}

# ======================================================
# ETAPA 3: PROVISIONAMENTO DA INFRAESTRUTURA (FASE 1)
# ======================================================

Write-Title "ETAPA 3: Provisionando a Infraestrutura com Terraform"

Write-Step "Instalando dependências da Lambda"
pip install -r ./lambda_authorizer/requirements.txt -t ./lambda_authorizer/package
Copy-Item -Path ./lambda_authorizer/main.py -Destination ./lambda_authorizer/package/

terraform init
terraform validate
terraform apply -auto-approve -var="alb_dns_name=dummy" # Usamos um valor dummy por enquanto

$EKS_CLUSTER_NAME = terraform output -raw eks_cluster_name
$ECR_REPOSITORY_URL = terraform output -raw ecr_repository_url

# ======================================================
# ETAPA 4: BUILD E PUSH DA IMAGEM COM KANIKO
# ======================================================

Write-Title "ETAPA 4: Build e Push da Imagem com Kaniko (In-Cluster)"

Write-Step "Configurando o kubectl para o cluster EKS"
aws eks update-kubeconfig --region $AWS_REGION --name $EKS_CLUSTER_NAME

$NAMESPACE = "kaniko"
kubectl create namespace $NAMESPACE --dry-run=client -o yaml | kubectl apply -f -

$secretContent = @"
{
    "credHelpers": {
        "${ECR_REPOSITORY_URL}": "ecr-login"
    }
}
"@
$encodedSecret = [System.Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes($secretContent))
kubectl create secret generic docker-config -n $NAMESPACE --from-literal=config.json=$secretContent --dry-run=client -o yaml | kubectl apply -f -

$JOB_NAME = "kaniko-build-$IMAGE_TAG"
$jobYaml = @"
apiVersion: batch/v1
kind: Job
metadata:
  name: $JOB_NAME
  namespace: $NAMESPACE
spec:
  template:
    spec:
      containers:
      - name: kaniko
        image: gcr.io/kaniko-project/executor:latest
        args:
        - "--context=git://${GITHUB_USER}:${env:GIT_TOKEN}@github.com/${GITHUB_USER}/${REPO_NAME}.git"
        - "--dockerfile=Dockerfile"
        - "--destination=${ECR_REPOSITORY_URL}:$IMAGE_TAG"
        - "--destination=${ECR_REPOSITORY_URL}:latest"
        volumeMounts:
        - name: docker-config
          mountPath: /kaniko/.docker/
      restartPolicy: Never
      volumes:
      - name: docker-config
        secret:
          secretName: docker-config
  backoffLimit: 4
"@

Write-Step "Iniciando o Job do Kaniko"
$jobYaml | kubectl apply -f -

Write-Step "Aguardando a conclusão do Job do Kaniko..."
kubectl wait --for=condition=complete job/$JOB_NAME -n $NAMESPACE --timeout=5m

# ======================================================
# ETAPA 5: DEPLOY NO KUBERNETES
# ======================================================

Write-Title "ETAPA 5: Deploy da Aplicação no Kubernetes"

Write-Step "Buscando o segredo do RDS no Secrets Manager"
$RDS_SECRET_ARN = aws secretsmanager list-secrets --query "SecretList[?Name=='mecanicaos-rds-credentials'].ARN" --output text
$RDS_SECRET_VALUE = aws secretsmanager get-secret-value --secret-id $RDS_SECRET_ARN --query SecretString --output text
$CONNECTION_STRING = "Host=$(($RDS_SECRET_VALUE | ConvertFrom-Json).host);Port=$(($RDS_SECRET_VALUE | ConvertFrom-Json).port);Database=$(($RDS_SECRET_VALUE | ConvertFrom-Json).dbname);Username=$(($RDS_SECRET_VALUE | ConvertFrom-Json).username);Password=$(($RDS_SECRET_VALUE | ConvertFrom-Json).password);"

Write-Step "Criando o namespace e o segredo no Kubernetes"
kubectl apply -f ../k8s/namespace.yaml
kubectl delete secret api-secret -n mecanica-os --ignore-not-found
kubectl create secret generic api-secret -n mecanica-os --from-literal=ConnectionStrings__DefaultConnection=$CONNECTION_STRING

Write-Step "Atualizando e aplicando os manifestos do Kubernetes"
(Get-Content ../k8s/api-deployment.yaml).replace('<ECR_REPOSITORY_URL>', $ECR_REPOSITORY_URL).replace('<IMAGE_TAG>', $IMAGE_TAG) | Set-Content ../k8s/api-deployment.yaml
kubectl apply -f ../k8s/

# ======================================================
# ETAPA 6: ATUALIZAÇÃO FINAL DA INFRAESTRUTURA
# ======================================================

Write-Title "ETAPA 6: Configurando a Integração Final (API Gateway)"

Write-Step "Aguardando o provisionamento do ALB pelo Ingress..."
$ALB_DNS_NAME = ""
while (-not $ALB_DNS_NAME) {
    Write-Host "..."
    Start-Sleep -Seconds 10
    $ALB_DNS_NAME = kubectl get ingress api-ingress -n mecanica-os -o jsonpath='{.status.loadBalancer.ingress[0].hostname}'
}
Write-Success "ALB DNS: $ALB_DNS_NAME"

Write-Step "Executando o terraform apply final com o DNS do ALB"
terraform apply -auto-approve -var="alb_dns_name=$ALB_DNS_NAME"

# ======================================================
# ETAPA 7: VERIFICAÇÃO FINAL
# ======================================================

Write-Title "ETAPA 7: Verificação Final e Informações"

$API_GATEWAY_URL = terraform output -raw api_gateway_url
$RDS_ENDPOINT = terraform output -raw rds_endpoint

Write-Success "Deploy finalizado com sucesso!"
Write-Info "URL da API Gateway: $API_GATEWAY_URL"
Write-Info "Endpoint do RDS: $RDS_ENDPOINT"
Write-Info "Repositório ECR: $ECR_REPOSITORY_URL"
Write-Info "Swagger UI: $API_GATEWAY_URL/swagger"
