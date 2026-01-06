# ============================================
# Script de Deploy COMPLETO - MecanicaOS (AWS Academy)
# Com Sanity Check obrigatório e tratamento de erro robusto
# ============================================
param(
    [switch]$SkipBuild,
    [switch]$SkipInfra,
    [switch]$Destroy,
    [switch]$Plan,
    [string]$AWS_REGION = "us-east-1",
    [string]$ECR_REPO_NAME = "mecanicaos-ecr"
)

# Encerra o script imediatamente em caso de erro de cmdlet
$ErrorActionPreference = "Stop"

# ======================================================
# FUNÇÕES DE LOG E EXECUÇÃO
# ======================================================
function Write-Title($msg) {
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host " $msg" -ForegroundColor Cyan
    Write-Host "============================================" -ForegroundColor Cyan
}

function Write-Step($msg) {
    Write-Host "`n==> $msg" -ForegroundColor Yellow
}

function Write-Success($msg) {
    Write-Host "[OK] $msg" -ForegroundColor Green
}

function Write-Warning($msg) {
    Write-Host "[!] $msg" -ForegroundColor Yellow
}

function Write-ErrorMsg($msg) {
    Write-Host "[X] $msg" -ForegroundColor Red
}

function Write-Info($msg) {
    Write-Host " $msg" -ForegroundColor Gray
}

# Função robusta para executar comandos externos e verificar o código de saída
function Invoke-CommandAndCheck {
    param(
        [string]$command,
        [string[]]$arguments
    )
    Write-Info "Executando: $command $arguments"
    & $command $arguments
    if ($LASTEXITCODE -ne 0) {
        throw "O comando '$command' falhou com o código de saída $LASTEXITCODE."
    }
}

# ======================================================
# ETAPA 0: SANITY CHECK
# ======================================================
Write-Title "ETAPA 0: Sanity Check do Ambiente"
if (-not (Test-Path ".\sanity-check.ps1")) {
    Write-ErrorMsg "sanity-check.ps1 não encontrado no diretório atual."
    exit 1
}
try {
    Write-Step "Executando sanity-check.ps1"
    .\sanity-check.ps1
    Write-Success "Sanity check passou com sucesso."
}
catch {
    Write-ErrorMsg "O Sanity Check do ambiente falhou. Verifique os pré-requisitos e credenciais."
    # O 'throw' vai parar o script aqui, que é o comportamento desejado.
    throw
}

# ======================================================
# ETAPA 1: PRÉ-REQUISITOS
# ======================================================
Write-Title "ETAPA 1: Verificando Pré-requisitos"
try {
    foreach ($cmd in @("aws", "terraform", "kubectl", "docker")) {
        if (-not (Get-Command $cmd -ErrorAction SilentlyContinue)) {
            throw "$cmd não encontrado no PATH. Por favor, instale-o."
        }
    }
    Write-Success "Todas as ferramentas (aws, terraform, kubectl, docker) foram encontradas."

    # Verifica se o Docker está rodando
    docker info | Out-Null
    Write-Success "Docker daemon está rodando."
}
catch {
    Write-ErrorMsg $_.Exception.Message
    exit 1
}


# ======================================================
# ETAPA 2: CREDENCIAIS AWS
# ======================================================
Write-Title "ETAPA 2: Validando Credenciais AWS"
try {
    $identity = aws sts get-caller-identity --output json | ConvertFrom-Json
    $AWS_ACCOUNT_ID = $identity.Account
    Write-Success "Credenciais AWS válidas. Account ID: $AWS_ACCOUNT_ID"
}
catch {
    Write-ErrorMsg "Falha ao validar credenciais AWS. Verifique sua configuração (aws configure)."
    exit 1
}

# ======================================================
# ETAPA 3: TFVARS / MODOS DE EXECUÇÃO
# ======================================================
Write-Title "ETAPA 3: Configurando Execução do Terraform"
if (-not (Test-Path "terraform.tfvars")) {
    Write-Warning "Arquivo terraform.tfvars não encontrado. Continuando com valores padrão."
}
else {
    Write-Success "Arquivo terraform.tfvars encontrado."
}

# Inicializa o Terraform para os modos Destroy e Plan
if ($Destroy -or $Plan) {
    Write-Step "Inicializando o Terraform..."
    Invoke-CommandAndCheck "terraform" "init"
}

# Modo de Destruição
if ($Destroy) {
    Write-Title "MODO DE DESTRUIÇÃO ATIVADO"
    Write-Step "Destruindo toda a infraestrutura gerenciada pelo Terraform..."
    Invoke-CommandAndCheck "terraform" @("destroy", "-auto-approve")
    Write-Success "Infraestrutura destruída com sucesso."
    exit 0
}

# Modo de Planejamento
if ($Plan) {
    Write-Title "MODO DE PLANEJAMENTO ATIVADO"
    Write-Step "Executando o planejamento do Terraform..."
    Invoke-CommandAndCheck "terraform" "plan"
    Write-Success "Planejamento concluído."
    exit 0
}

# ======================================================
# ETAPA 4: DEPLOY DA INFRAESTRUTURA (TERRAFORM)
# ======================================================
if (-not $SkipInfra) {
    Write-Title "ETAPA 4: Deploy da Infraestrutura com Terraform"
    try {
        Write-Step "Descobrindo roles do EKS para o AWS Academy..."
        $clusterRole = aws iam list-roles --query "Roles[?contains(RoleName,'LabEksClusterRole')].RoleName | [0]" --output text
        $nodeRole = aws iam list-roles --query "Roles[?contains(RoleName,'LabEksNodeRole')].RoleName | [0]" --output text

        if (-not $clusterRole -or -not $nodeRole) {
            throw "Não foi possível encontrar as roles 'LabEksClusterRole' ou 'LabEksNodeRole'. Verifique se você está em um ambiente AWS Academy."
        }
        Write-Success "Roles do EKS encontradas: ClusterRole='$clusterRole', NodeRole='$nodeRole'"

        # Define as variáveis de ambiente para o Terraform
        $env:TF_VAR_eks_cluster_role_name = $clusterRole
        $env:TF_VAR_eks_node_role_name = $nodeRole

        Write-Step "Inicializando o Terraform..."
        Invoke-CommandAndCheck "terraform" "init"

        Write-Step "Validando a configuração do Terraform..."
        Invoke-CommandAndCheck "terraform" "validate"

        Write-Step "Aplicando a configuração do Terraform para criar/atualizar a infraestrutura..."
        Invoke-CommandAndCheck "terraform" @("apply", "-auto-approve")
        Write-Success "Infraestrutura (VPC, ECR, EKS) aplicada com sucesso."
    }
    catch {
        Write-ErrorMsg "Ocorreu um erro durante o deploy da infraestrutura com o Terraform."
        Write-ErrorMsg $_.Exception.Message
        exit 1
    }
} else {
    Write-Warning "ETAPA 4: Deploy da Infraestrutura ignorado devido ao parâmetro -SkipInfra."
}


# ======================================================
# ETAPA 5: CONFIGURANDO KUBECONFIG
# ======================================================
Write-Title "ETAPA 5: Configurando Acesso ao Cluster (kubectl)"
try {
    Write-Step "Obtendo o nome do cluster EKS do output do Terraform..."
    $EKS_CLUSTER_NAME = terraform output -raw eks_cluster_name
    if (-not $EKS_CLUSTER_NAME) {
        throw "Não foi possível obter o nome do cluster do output do Terraform."
    }
    Write-Success "Nome do cluster: $EKS_CLUSTER_NAME"

    Write-Step "Atualizando o kubeconfig para apontar para o cluster '$EKS_CLUSTER_NAME'..."
    Invoke-CommandAndCheck "aws" @("eks", "update-kubeconfig", "--region", $AWS_REGION, "--name", $EKS_CLUSTER_NAME)
    Write-Success "kubectl configurado com sucesso."
}
catch {
    Write-ErrorMsg "Falha ao configurar o kubectl. Verifique se o cluster EKS foi criado corretamente."
    Write-ErrorMsg $_.Exception.Message
    exit 1
}

# ======================================================
# ETAPA 6: BUILD COM KANIKO
# ======================================================
if (-not $SkipBuild) {
    Write-Title "ETAPA 6: Build da Imagem com Kaniko"
    try {
        $GITHUB_USER = "LTeruyaQ"
        $REPO_NAME = "techchallenge-soat-app"
        # O token não deve ser hardcoded, mas mantido conforme solicitado.
        $env:GIT_TOKEN = "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"
        if (-not $env:GIT_TOKEN) {
            throw "A variável de ambiente GIT_TOKEN é necessária para o build com Kaniko."
        }

        $IMAGE_TAG = Get-Date -Format "yyyyMMdd-HHmmss"
        $ECR_URI = "$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$ECR_REPO_NAME"
        $JOB_NAME = "kaniko-build-$IMAGE_TAG"
        $NAMESPACE = "build"

        Write-Step "Garantindo que o namespace '$NAMESPACE' exista no cluster..."
        # O --dry-run e apply garante a idempotência
        kubectl create namespace $NAMESPACE --dry-run=client -o yaml | kubectl apply -f -

        Write-Step "Criando secret com credenciais da AWS no namespace '$NAMESPACE'..."
        kubectl delete secret aws-creds -n $NAMESPACE --ignore-not-found | Out-Null
        kubectl create secret generic aws-creds -n $NAMESPACE `
            --from-literal=AWS_ACCESS_KEY_ID=$env:AWS_ACCESS_KEY_ID `
            --from-literal=AWS_SECRET_ACCESS_KEY=$env:AWS_SECRET_ACCESS_KEY `
            --from-literal=AWS_SESSION_TOKEN=$env:AWS_SESSION_TOKEN `
            --from-literal=AWS_DEFAULT_REGION=$AWS_REGION | Out-Null

        Write-Step "Criando e aplicando o Job do Kaniko no cluster..."
        $jobYaml = @"
apiVersion: batch/v1
kind: Job
metadata:
  name: $JOB_NAME
  namespace: $NAMESPACE
spec:
  backoffLimit: 0
  template:
    spec:
      restartPolicy: Never
      containers:
      - name: kaniko
        image: gcr.io/kaniko-project/executor:latest
        envFrom:
        - secretRef:
            name: aws-creds
        args:
        - "--context=git://github.com/$GITHUB_USER/$REPO_NAME.git"
        - "--dockerfile=Dockerfile"
        - "--destination=${ECR_URI}:$IMAGE_TAG"
        - "--destination=${ECR_URI}:latest"
"@
        $jobYaml | kubectl apply -f -
        Write-Success "Job Kaniko '$JOB_NAME' submetido ao cluster."

        Write-Step "Aguardando a conclusão do Job Kaniko (timeout de 5 minutos)..."
        kubectl wait --for=condition=complete job/$JOB_NAME -n $NAMESPACE --timeout=300s
        Write-Success "Build do Kaniko finalizado com sucesso."

    }
    catch {
        Write-ErrorMsg "Ocorreu um erro durante o build com Kaniko."
        Write-ErrorMsg $_.Exception.Message
        # Tenta obter os logs do pod do Kaniko para facilitar o debug
        $podName = kubectl get pods -n $NAMESPACE -l job-name=$JOB_NAME -o jsonpath='{.items[0].metadata.name}'
        if ($podName) {
            Write-ErrorMsg "Logs do pod '$podName' do Kaniko:"
            kubectl logs $podName -n $NAMESPACE
        }
        exit 1
    }
} else {
    Write-Warning "ETAPA 6: Build com Kaniko ignorado devido ao parâmetro -SkipBuild."
}

# ======================================================
# ETAPA 7: VERIFICAÇÃO FINAL
# ======================================================
Write-Title "ETAPA 7: Verificação Final do Ambiente"
try {
    Write-Step "Listando Pods em todos os namespaces..."
    Invoke-CommandAndCheck "kubectl" @("get", "pods", "-A")

    Write-Step "Listando Services em todos os namespaces..."
    Invoke-CommandAndCheck "kubectl" @("get", "svc", "-A")

    Write-Success "Script de deploy finalizado com sucesso!"
}
catch {
    Write-ErrorMsg "Falha na verificação final. O ambiente pode estar instável."
    Write-ErrorMsg $_.Exception.Message
    exit 1
}
