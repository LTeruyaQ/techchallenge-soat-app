Param(
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

function Write-Step($msg) {
    Write-Host ""
    Write-Host "============================================"
    Write-Host " $msg"
    Write-Host "============================================"
}

function Write-Info($msg) {
    Write-Host "[OK] $msg"
}

function Write-Err($msg) {
    Write-Host "[ERRO] $msg" -ForegroundColor Red
}

# ============================
# VARIÁVEIS
# ============================
$AWS_REGION="us-east-1"
$ECR_NAME="mecanicaos-api"
$IMAGE_TAG="latest"
$NAMESPACE="build"

# ============================
# AWS
# ============================
Write-Step "ETAPA 1: Validando AWS"
aws sts get-caller-identity | Out-Null
Write-Info "AWS autenticado"

# ============================
# TERRAFORM
# ============================
Write-Step "ETAPA 2: Terraform"
terraform init
terraform apply -auto-approve
Write-Info "Terraform concluído"

# ============================
# KUBECTL CONFIG
# ============================
Write-Step "ETAPA 3: Configurando kubectl"
aws eks update-kubeconfig --region $AWS_REGION --name eks-mecanicaos
Write-Info "Cluster configurado"

# ============================
# ECR
# ============================
Write-Step "ETAPA 4: Preparando ECR"
$ACCOUNT_ID = (aws sts get-caller-identity --query Account --output text)
$ECR_URI = "$ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$ECR_NAME"

try {
    aws ecr describe-repositories --repository-names $ECR_NAME --region $AWS_REGION | Out-Null
    Write-Info "Repositório ECR já existe"
}
catch {
    Write-Info "Criando ECR..."
    aws ecr create-repository --repository-name $ECR_NAME --region $AWS_REGION | Out-Null
}

Write-Info "ECR pronto: $ECR_URI"

# ============================
# BUILD KANIKO
# ============================
if (-not $SkipBuild) {

Write-Step "ETAPA 5: Build com Kaniko"

# Namespace
Write-Info "Verificando namespace..."
$nsExists = kubectl get namespace $NAMESPACE --ignore-not-found
if (-not $nsExists) {
    Write-Info "Criando namespace build..."
    kubectl create namespace $NAMESPACE | Out-Null
} else {
    Write-Info "Namespace já existe"
}

# SECRET AWS
Write-Info "Criando Secret AWS..."
kubectl delete secret aws-creds -n $NAMESPACE 2>$null | Out-Null

$AWS_ACCESS_KEY_ID = aws configure get aws_access_key_id
$AWS_SECRET_ACCESS_KEY = aws configure get aws_secret_access_key
$AWS_SESSION_TOKEN = aws configure get aws_session_token

kubectl create secret generic aws-creds `
  --from-literal=aws_access_key_id=$AWS_ACCESS_KEY_ID `
  --from-literal=aws_secret_access_key=$AWS_SECRET_ACCESS_KEY `
  --from-literal=aws_session_token=$AWS_SESSION_TOKEN `
  -n $NAMESPACE

Write-Info "Secret criado"

# ============================
# JOB YAML
# ============================
Write-Info "Criando job Kaniko..."

$jobYaml = @"
apiVersion: batch/v1
kind: Job
metadata:
  name: kaniko-build
  namespace: $NAMESPACE
spec:
  backoffLimit: 0
  template:
    spec:
      restartPolicy: Never
      containers:
      - name: kaniko
        image: gcr.io/kaniko-project/executor:latest
        args:
          - "--dockerfile=/workspace/Dockerfile"
          - "--context=git://github.com/mecanicaos/techchallenge-soat-api.git"
          - "--destination=${ECR_URI}:$IMAGE_TAG"
          - "--single-snapshot"
          - "--snapshotMode=time"
        env:
        - name: AWS_ACCESS_KEY_ID
          valueFrom:
            secretKeyRef:
              name: aws-creds
              key: aws_access_key_id
        - name: AWS_SECRET_ACCESS_KEY
          valueFrom:
            secretKeyRef:
              name: aws-creds
              key: aws_secret_access_key
        - name: AWS_SESSION_TOKEN
          valueFrom:
            secretKeyRef:
              name: aws-creds
              key: aws_session_token
"@

$jobYaml | kubectl apply -f -
Write-Info "Job enviado"

# ============================
# ESPERAR BUILD
# ============================
Write-Info "Aguardando conclusão..."

$deadline = (Get-Date).AddMinutes(10)

while ($true) {

    $job = kubectl get job kaniko-build -n $NAMESPACE -o json | ConvertFrom-Json

    if ($job.status.succeeded -ge 1) {
        Write-Info "Build finalizado"
        break
    }

    if ($job.status.failed -ge 1) {
        Write-Err "Kaniko falhou"
        break
    }

    if ((Get-Date) -ge $deadline) {
        Write-Err "Timeout Kaniko"
        break
    }

    Write-Host "." -NoNewline
    Start-Sleep -Seconds 5
}

# LOGS
Write-Info "Logs do Kaniko:"
$pod = kubectl get pods -n $NAMESPACE -l job-name=kaniko-build -o jsonpath="{.items[0].metadata.name}"
kubectl logs $pod -n $NAMESPACE --tail=50
}

# ============================
# FINAL
# ============================
Write-Step "FINALIZADO!"
Write-Info "Se der erro, manda o trecho que eu resolvo 👍"
