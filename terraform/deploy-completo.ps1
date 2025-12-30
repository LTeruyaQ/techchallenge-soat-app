# ============================================
# Script de Deploy COMPLETO - MecanicaOS na AWS Academy
# Versão revisada: corrige parsing do PowerShell e Job Kaniko
# ============================================

param(
    [switch]$SkipBuild,
    [switch]$SkipInfra,
    [switch]$Destroy,
    [switch]$Plan,
    [string]$AWS_REGION = "us-east-1",
    [string]$ECR_REPO_NAME = "mecanicaos-ecr"
)

$ErrorActionPreference = "Stop"

function Write-Title { param($msg)
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host " $msg" -ForegroundColor Cyan
    Write-Host "============================================" -ForegroundColor Cyan
}
function Write-Step { param($msg) Write-Host "`n==> $msg" -ForegroundColor Yellow }
function Write-Success { param($msg) Write-Host "[OK] $msg" -ForegroundColor Green }
function Write-Warning { param($msg) Write-Host "[!] $msg" -ForegroundColor Yellow }
function Write-Error { param($msg) Write-Host "[X] $msg" -ForegroundColor Red }
function Write-Info { param($msg) Write-Host "    $msg" -ForegroundColor Gray }

$PROJECT_ROOT = Split-Path -Parent $PSScriptRoot

# ======================================================
# ETAPA 1: PRE-REQUISITOS
# ======================================================

Write-Title "ETAPA 1: Verificando Pre-requisitos"

if (-not (Get-Command aws -ErrorAction SilentlyContinue)) { Write-Error "AWS CLI nao encontrado!"; exit 1 }
if (-not (Get-Command terraform -ErrorAction SilentlyContinue)) { Write-Error "Terraform nao encontrado!"; exit 1 }

Write-Step "Verificando Docker..."
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) { Write-Error "Docker nao encontrado!"; exit 1 }
try { docker info | Out-Null; Write-Success "Docker OK" } catch { Write-Error "Docker nao esta rodando!"; exit 1 }

if (-not (Get-Command kubectl -ErrorAction SilentlyContinue)) { Write-Error "kubectl nao encontrado!"; exit 1 }

# ======================================================
# ETAPA 2: Credenciais AWS
# ======================================================

Write-Title "ETAPA 2: Verificando Credenciais AWS"

try {
    $identity = aws sts get-caller-identity --output json | ConvertFrom-Json
    $AWS_ACCOUNT_ID = $identity.Account
    Write-Success "Credenciais validas!"
} catch {
    Write-Error "Credenciais AWS invalidas!"
    exit 1
}

# ======================================================
# ETAPA 2.1: Detectando IAM Role do EKS
# ======================================================

Write-Title "ETAPA 2.1: Detectando IAM Role do EKS"

try {
    $ROLE_NAME = aws iam list-roles `
        --query "Roles[?contains(RoleName, 'LabEksNodeRole') || contains(RoleName, 'LabEksClusterRole')].RoleName | [0]" `
        --output text

    if ($ROLE_NAME -eq "None" -or -not $ROLE_NAME) {
        Write-Warning "Nenhuma Role contendo LabEksNodeRole ou LabEksClusterRole encontrada."
        $EKS_ROLE_ARN = $null
    } else {
        Write-Success "Role encontrada: $ROLE_NAME"
        $EKS_ROLE_ARN = "arn:aws:iam::$AWS_ACCOUNT_ID:role/$ROLE_NAME"
        Write-Info "Role ARN: $EKS_ROLE_ARN"
    }
} catch {
    Write-Warning "Falha ao buscar Role. Continuando sem role-arn..."
    $EKS_ROLE_ARN = $null
}

# ======================================================
# ETAPA 3: TFVARS
# ======================================================

Write-Title "ETAPA 3: Verificando terraform.tfvars"

if (-not (Test-Path "terraform.tfvars")) {
    Write-Error "terraform.tfvars nao encontrado!"
    exit 1
}

$tfvarsContent = Get-Content "terraform.tfvars" -Raw

if ($Destroy) {
    Write-Title "DESTRUINDO INFRAESTRUTURA"
    terraform destroy -auto-approve
    exit 0
}

if ($Plan) {
    Write-Title "MODO PLAN"
    terraform init
    terraform plan
    exit 0
}

# ======================================================
# ETAPA 4 e 5: BUILD (Kaniko)
# ======================================================

if (-not $SkipBuild) {

    Write-Title "ETAPA 4: Criando ECR"
    terraform init
    terraform apply "-target=aws_ecr_repository.app" -auto-approve

    Write-Title "ETAPA 5: Build com Kaniko no Kubernetes"

    # PRIVATE REPO CONFIG
    $GITHUB_USER = "LTeruyaQ"
    $REPO_NAME   = "techchallenge-soat-app"

    if ($env:GIT_TOKEN -and $env:GIT_TOKEN.Trim().Length -gt 0) {
        $GIT_TOKEN = $env:GIT_TOKEN
    } else {
        # substitua por variável de ambiente em produção
        $GIT_TOKEN = "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"
    }

    if (-not $GIT_TOKEN -or $GIT_TOKEN.Trim().Length -eq 0) {
        Write-Error "Defina a variavel de ambiente GIT_TOKEN para acesso ao repo privado do GitHub."
        exit 1
    }

    $IMAGE_TAG = (Get-Date -Format "yyyyMMdd-HHmmss")
    $ECR_URI   = "$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$ECR_REPO_NAME"
    $JOB_NAME  = "kaniko-build-$IMAGE_TAG"
    $NAMESPACE = "build"

    kubectl create namespace $NAMESPACE --dry-run=client -o yaml | kubectl apply -f -

    kubectl delete secret aws-creds -n $NAMESPACE --ignore-not-found | Out-Null
    kubectl create secret generic aws-creds `
        -n $NAMESPACE `
        --from-literal=AWS_ACCESS_KEY_ID=$env:AWS_ACCESS_KEY_ID `
        --from-literal=AWS_SECRET_ACCESS_KEY=$env:AWS_SECRET_ACCESS_KEY `
        --from-literal=AWS_DEFAULT_REGION=$AWS_REGION | Out-Null

    Write-Step "Criando Job Kaniko..."

    try {
        $existingJobs = kubectl get jobs -n $NAMESPACE -o name 2>$null | Where-Object { $_ -match 'kaniko-build' }
        if ($existingJobs) {
            Write-Info "Deletando jobs antigos de kaniko..."
            $existingJobs | ForEach-Object { kubectl delete $_ -n $NAMESPACE --ignore-not-found | Out-Null }
        }
    } catch {
        Write-Warning "Falha ao listar/deletar jobs antigos (continuando)..."
    }

    # ============================
    # JOB YAML (Kaniko) - single-quoted here-string para evitar expansão
    # ============================
    Write-Info "Criando job Kaniko YAML..."

    # JOB TEMPLATE: todas as chaves literais {{ }} escapadas para o -f do PowerShell
    # O bloco que cria /kaniko/.docker/config.json usa printf para evitar heredoc/EOF problemas no YAML
    $jobTemplate = @'
apiVersion: batch/v1
kind: Job
metadata:
  name: {0}
  namespace: {1}
spec:
  backoffLimit: 0
  template:
    spec:
      restartPolicy: Never
      volumes:
      - name: workspace
        emptyDir: {{}} 
      - name: kaniko-docker-config
        emptyDir: {{}} 
      initContainers:
      - name: git-clone
        image: alpine/git:latest
        command:
        - /bin/sh
        - -c
        - |
          set -e
          rm -rf /workspace/*
          git clone --depth 1 https://{2}:{3}@github.com/{2}/{4}.git /workspace
        volumeMounts:
        - name: workspace
          mountPath: /workspace
      - name: ecr-auth
        image: amazon/aws-cli:2.15.0
        command:
        - /bin/sh
        - -c
        - |
          set -e
          mkdir -p /kaniko/.docker
          PASSWORD=$(aws ecr get-login-password --region {5})
          # cria config.json usando printf (evita heredoc indent/EOF issues)
          printf '{{"auths":{{"%s":{{"auth":"%s"}}}}}}' "{6}" "$PASSWORD" | base64 -d >/dev/null 2>&1 || true
          # fallback: escrever diretamente (sem decodificação) para compatibilidade
          printf '{{"auths":{{"%s":{{"auth":"%s"}}}}}}' "{6}" "$PASSWORD" > /kaniko/.docker/config.json
          chmod 600 /kaniko/.docker/config.json
        envFrom:
        - secretRef:
            name: aws-creds
        volumeMounts:
        - name: kaniko-docker-config
          mountPath: /kaniko/.docker
      containers:
      - name: kaniko
        image: gcr.io/kaniko-project/executor:latest
        volumeMounts:
        - name: workspace
          mountPath: /workspace
        - name: kaniko-docker-config
          mountPath: /kaniko/.docker
        envFrom:
        - secretRef:
            name: aws-creds
        args:
        - "--context=dir:///workspace"
        - "--dockerfile=/workspace/Dockerfile"
        - "--destination={6}:{7}"
        - "--destination={6}:latest"
        - "--single-snapshot"
'@

    # Formata o template com -f (substitui {0}..{7})
    $jobYaml = $jobTemplate -f $JOB_NAME, $NAMESPACE, $GITHUB_USER, $GIT_TOKEN, $REPO_NAME, $AWS_REGION, $ECR_URI, $IMAGE_TAG

    # Opcional: salvar para inspeção antes de aplicar
    # $jobYaml | Out-File -FilePath .\kaniko-job.yaml -Encoding utf8

    # Validação rápida (dry-run) antes de aplicar
    try {
        $jobYaml | kubectl apply --dry-run=client -f - | Out-Null
    } catch {
        Write-Error "YAML do Job Kaniko inválido. Salvando para inspeção em .\kaniko-job.yaml"
        $jobYaml | Out-File -FilePath .\kaniko-job.yaml -Encoding utf8
        Write-Error "Revise o arquivo .\kaniko-job.yaml e corrija o template."
        throw $_
    }

    # Aplica o Job
    $jobYaml | kubectl apply -f -

    # Aguarda o Pod do Job aparecer
    $waitStart = Get-Date
    $podName = $null
    $podWaitTimeoutSec = 120
    while (-not $podName) {
        Start-Sleep -Seconds 2
        $podName = kubectl get pods -n $NAMESPACE -l job-name=$JOB_NAME -o jsonpath="{.items[0].metadata.name}" 2>$null
        if ((Get-Date) - $waitStart -gt (New-TimeSpan -Seconds $podWaitTimeoutSec)) {
            Write-Warning "Timeout aguardando criação do Pod do Job. Verifique manualmente com 'kubectl get pods -n $NAMESPACE'."
            break
        }
    }

    if ($podName) {
        Write-Info "Pod do Job: $podName"

        # Monitor simples: se entrar em ImagePullBackOff ou CrashLoopBackOff, mostra describe e logs e aborta
        $monitorStart = Get-Date
        $monitorTimeoutSec = 1800  # 30 minutos total para o job completar
        $jobCompleted = $false

        while (-not $jobCompleted) {
            Start-Sleep -Seconds 5

            # Verifica condição de complete no job
            $jobStatus = kubectl get job $JOB_NAME -n $NAMESPACE -o json 2>$null | ConvertFrom-Json
            if ($jobStatus -and $jobStatus.status -and $jobStatus.status.succeeded -and $jobStatus.status.succeeded -ge 1) {
                Write-Success "Job Kaniko completou com sucesso."
                $jobCompleted = $true
                break
            }

            # Re-obtem o podName (pode mudar)
            $podName = kubectl get pods -n $NAMESPACE -l job-name=$JOB_NAME -o jsonpath="{.items[0].metadata.name}" 2>$null
            if (-not $podName) { continue }

            # Checa estado do container
            $waitingReason = kubectl get pod $podName -n $NAMESPACE -o jsonpath="{.status.containerStatuses[0].state.waiting.reason}" 2>$null
            $terminatedReason = kubectl get pod $podName -n $NAMESPACE -o jsonpath="{.status.containerStatuses[0].state.terminated.reason}" 2>$null

            if ($waitingReason -and ($waitingReason -match "ImagePullBackOff|ErrImagePull|CreateContainerConfigError")) {
                Write-Error "Pod em estado $waitingReason. Exibindo describe e logs."
                kubectl describe pod $podName -n $NAMESPACE
                kubectl logs $podName -n $NAMESPACE --all-containers
                throw "Kaniko Pod falhou com motivo: $waitingReason"
            }

            if ($terminatedReason -and ($terminatedReason -match "Error|OOMKilled")) {
                Write-Error "Container terminado com motivo $terminatedReason. Exibindo describe e logs."
                kubectl describe pod $podName -n $NAMESPACE
                kubectl logs $podName -n $NAMESPACE --all-containers
                throw "Kaniko Pod terminado com motivo: $terminatedReason"
            }

            # Timeout geral
            if ((Get-Date) - $monitorStart -gt (New-TimeSpan -Seconds $monitorTimeoutSec)) {
                Write-Warning "Timeout aguardando conclusão do Job Kaniko ($monitorTimeoutSec s). Pegando logs parciais."
                if ($podName) { kubectl logs $podName -n $NAMESPACE --all-containers }
                break
            }
        }

        # Se jobCompleted false, ainda tentamos pegar logs do job/pod
        if (-not $jobCompleted) {
            try {
                if ($podName) { kubectl logs $podName -n $NAMESPACE --all-containers }
            } catch { Write-Warning "Nao foi possivel obter logs do pod." }
        }
    } else {
        Write-Warning "Nao foi possivel identificar o Pod do Job. Verifique 'kubectl get pods -n $NAMESPACE'."
    }

    # Tenta obter logs do job (caso tenha completado)
    try {
        kubectl logs job/$JOB_NAME -n $NAMESPACE
    } catch {
        Write-Info "kubectl logs job/$JOB_NAME falhou (talvez job nao tenha terminado)."
    }

    Write-Success "Processo de build Kaniko finalizado (verifique logs acima para detalhes)."

    Write-Step "Atualizando terraform.tfvars"
    $tfvarsPath = "terraform.tfvars"
    $tfvarsContent = Get-Content $tfvarsPath -Raw

    if ($tfvarsContent -match 'docker_image_tag\s*=') {
        $tfvarsContent = $tfvarsContent -replace 'docker_image_tag\s*=\s*"[^"]*"', "docker_image_tag  = `"$IMAGE_TAG`""
    } else {
        $tfvarsContent = $tfvarsContent + "`ndocker_image_tag = `"$IMAGE_TAG`""
    }

    Set-Content $tfvarsPath $tfvarsContent
}

# ======================================================
# ETAPA 7: Deploy Terraform (infra)
# ======================================================

if (-not $SkipInfra) {
    Write-Title "ETAPA 7: Deploy Terraform"

    Write-Host "`nDetectando roles EKS dinamicamente..."

    $clusterRole = aws iam list-roles --query "Roles[?contains(RoleName, 'LabEksClusterRole')].RoleName" --output text
    $nodeRole    = aws iam list-roles --query "Roles[?contains(RoleName, 'LabEksNodeRole')].RoleName" --output text

    if (-not $clusterRole) { Write-Host "❌ Não achei role de cluster"; exit 1 }
    if (-not $nodeRole)    { Write-Host "❌ Não achei role de node"; exit 1 }

    Write-Host "[OK] Cluster Role: $clusterRole"
    Write-Host "[OK] Node Role: $nodeRole"

    $env:TF_VAR_eks_cluster_role = $clusterRole
    $env:TF_VAR_eks_node_role   = $nodeRole

    Write-Host "TF_VAR_eks_cluster_role = $env:TF_VAR_eks_cluster_role"
    Write-Host "TF_VAR_eks_node_role   = $env:TF_VAR_eks_node_role"

    terraform init
    terraform validate
    terraform apply -auto-approve
}

# ======================================================
# ETAPA 8: Configurando kubectl
# ======================================================

Write-Title "ETAPA 8: Configurando kubectl"
$EKS_CLUSTER_NAME = terraform output -raw eks_cluster_name 2>$null
if (-not $EKS_CLUSTER_NAME) { $EKS_CLUSTER_NAME = "eks-mecanicaos" }

aws eks update-kubeconfig --region $AWS_REGION --name $EKS_CLUSTER_NAME

Write-Success "Cluster configurado"

# ======================================================
# ETAPA 9: Verificando Deploy
# ======================================================

Write-Title "ETAPA 9: Verificando Deploy"
Start-Sleep -Seconds 30
Write-Step "Pods no namespace mecanicaos"
kubectl get pods -n mecanicaos
Write-Step "Services no namespace mecanicaos"
kubectl get svc -n mecanicaos

Write-Title "ETAPA 10: Observabilidade"
kubectl get pods -n observability
kubectl get svc -n observability

Write-Success "Script finalizado."
