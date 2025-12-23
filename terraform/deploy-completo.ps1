# ============================================
# Script de Deploy COMPLETO - MecanicaOS na AWS Academy
# ============================================
#
# Este script faz TODO o processo de deploy:
# 1. Verifica pré-requisitos
# 2. Valida credenciais AWS
# 3. Cria repositório ECR
# 4. Build da imagem Docker
# 5. Push para ECR
# 6. Deploy da infraestrutura com Terraform
# 7. Verifica o deploy no Kubernetes
# 8. Verifica OpenTelemetry Collector (Datadog + New Relic)
#
# Uso:
#   .\deploy-completo.ps1                    # Deploy completo
#   .\deploy-completo.ps1 -SkipBuild         # Pula build Docker (usa imagem existente)
#   .\deploy-completo.ps1 -SkipInfra         # Pula Terraform (só build e push)
#   .\deploy-completo.ps1 -Destroy           # Destroi tudo
#   .\deploy-completo.ps1 -Plan              # Apenas mostra o que será feito
#
# ============================================

param(
    [switch]$SkipBuild,
    [switch]$SkipInfra,
    [switch]$Destroy,
    [switch]$Plan,
    [switch]$Debug,
    [switch]$Quiet
)

if ($Debug) {
    $global:DebugPreference = "Continue"
}
if ($Quiet) {
    $global:InformationPreference = "SilentlyContinue"
}

$ErrorActionPreference = "Stop"

# ============================================
# Funções de Output
# ============================================

function Write-Title { 
    param($msg) 
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host " $msg" -ForegroundColor Cyan
    Write-Host "============================================" -ForegroundColor Cyan
}

function Write-Step { 
    param($msg) 
    Write-Host "`n==> $msg" -ForegroundColor Yellow 
}

function Write-Success { 
    param($msg) 
    Write-Host "[OK] $msg" -ForegroundColor Green 
}

function Write-Warning { 
    param($msg) 
    Write-Host "[!] $msg" -ForegroundColor Yellow 
}

function Write-Error { 
    param($msg) 
    Write-Host "[X] $msg" -ForegroundColor Red 
}

function Write-Info {
    param($msg)
    Write-Host "    $msg" -ForegroundColor Gray
}

# ============================================
# Variáveis
# ============================================

$AWS_REGION = "us-east-1"
$ECR_REPO_NAME = "mecanicaos-ecr"
$PROJECT_ROOT = Split-Path -Parent $PSScriptRoot

# ============================================
# ETAPA 1: Verificar Pré-requisitos
# ============================================

Write-Title "ETAPA 1: Verificando Pre-requisitos"

# AWS CLI
Write-Step "Verificando AWS CLI..."
if (-not (Get-Command aws -ErrorAction SilentlyContinue)) {
    Write-Error "AWS CLI nao encontrado!"
    Write-Info "Instale em: https://aws.amazon.com/cli/"
    exit 1
}
Write-Success "AWS CLI encontrado"

# Terraform
Write-Step "Verificando Terraform..."
if (-not (Get-Command terraform -ErrorAction SilentlyContinue)) {
    Write-Error "Terraform nao encontrado!"
    Write-Info "Instale em: https://www.terraform.io/downloads"
    exit 1
}
Write-Success "Terraform encontrado"

# Docker
Write-Step "Verificando Docker..."
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Error "Docker nao encontrado!"
    Write-Info "Instale em: https://www.docker.com/products/docker-desktop"
    exit 1
}

# Verificar se Docker está rodando
try {
    docker info | Out-Null
    Write-Success "Docker encontrado e rodando"
} catch {
    Write-Error "Docker nao esta rodando! Inicie o Docker Desktop."
    exit 1
}

# kubectl
Write-Step "Verificando kubectl..."
if (-not (Get-Command kubectl -ErrorAction SilentlyContinue)) {
    Write-Error "kubectl nao encontrado!"
    Write-Info "Instale em: https://kubernetes.io/docs/tasks/tools/"
    exit 1
}
Write-Success "kubectl encontrado"

# ============================================
# ETAPA 2: Verificar Credenciais AWS
# ============================================

Write-Title "ETAPA 2: Verificando Credenciais AWS"

Write-Step "Testando credenciais AWS..."
try {
    $identity = aws sts get-caller-identity --output json 2>&1 | ConvertFrom-Json
    Write-Success "Credenciais validas!"
    Write-Info "Account: $($identity.Account)"
    Write-Info "ARN: $($identity.Arn)"
    $AWS_ACCOUNT_ID = $identity.Account
} catch {
    Write-Error "Credenciais AWS invalidas ou expiradas!"
    Write-Host ""
    Write-Host "COMO RESOLVER:" -ForegroundColor Yellow
    Write-Host "1. Acesse seu AWS Academy Lab" -ForegroundColor White
    Write-Host "2. Clique em 'AWS Details'" -ForegroundColor White
    Write-Host "3. Clique em 'Show' ao lado de 'AWS CLI'" -ForegroundColor White
    Write-Host "4. Copie o conteudo para o arquivo:" -ForegroundColor White
    Write-Host "   $HOME\.aws\credentials" -ForegroundColor Cyan
    Write-Host ""
    Write-Warning "Lembre-se: Credenciais do AWS Academy expiram a cada 4 horas!"
    exit 1
}

# ============================================
# ETAPA 3: Verificar terraform.tfvars
# ============================================

Write-Title "ETAPA 3: Verificando Configuracao"

Write-Step "Verificando terraform.tfvars..."
if (-not (Test-Path "terraform.tfvars")) {
    Write-Error "Arquivo terraform.tfvars nao encontrado!"
    Write-Host ""
    Write-Host "COMO RESOLVER:" -ForegroundColor Yellow
    Write-Host "1. Copie o arquivo de exemplo:" -ForegroundColor White
    Write-Host "   Copy-Item terraform.tfvars.example terraform.tfvars" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "2. Edite o arquivo e preencha:" -ForegroundColor White
    Write-Host "   - eks_cluster_role_name (busque 'LabEksClusterRole' no IAM)" -ForegroundColor Cyan
    Write-Host "   - eks_node_role_name (busque 'LabEksNodeRole' no IAM)" -ForegroundColor Cyan
    Write-Host "   - db_host, db_password (dados do Supabase)" -ForegroundColor Cyan
    Write-Host "   - jwt_secret_key (chave secreta)" -ForegroundColor Cyan
    exit 1
}
Write-Success "terraform.tfvars encontrado"

# Verificar se as roles foram preenchidas
$tfvarsContent = Get-Content "terraform.tfvars" -Raw
if ($tfvarsContent -match "LabRole" -and $tfvarsContent -notmatch "LabEks") {
    Write-Warning "Parece que voce esta usando 'LabRole' como role do EKS!"
    Write-Host ""
    Write-Host "ATENCAO:" -ForegroundColor Yellow
    Write-Host "O AWS Academy tem roles especificas para EKS:" -ForegroundColor White
    Write-Host "- LabEksClusterRole (para o cluster)" -ForegroundColor Cyan
    Write-Host "- LabEksNodeRole (para os nodes)" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Voce encontra em: AWS Console > IAM > Roles > Buscar 'LabEks'" -ForegroundColor White
    Write-Host ""
    $continue = Read-Host "Deseja continuar mesmo assim? (s/n)"
    if ($continue -ne "s") {
        exit 1
    }
}

# Verificar configuração do OpenTelemetry (Datadog e New Relic)
Write-Step "Verificando configuracao do OpenTelemetry..."
$hasDatadog = $tfvarsContent -match 'datadog_api_key\s*=\s*"[^"]+"'
$hasNewRelic = $tfvarsContent -match 'newrelic_license_key\s*=\s*"[^"]+"'

if (-not $hasDatadog -or -not $hasNewRelic) {
    Write-Warning "Configuracao do OpenTelemetry incompleta!"
    Write-Host ""
    Write-Host "Para habilitar observabilidade, adicione ao terraform.tfvars:" -ForegroundColor Yellow
    if (-not $hasDatadog) {
        Write-Host "  datadog_api_key = \"sua-api-key-do-datadog\"" -ForegroundColor Cyan
    }
    if (-not $hasNewRelic) {
        Write-Host "  newrelic_license_key = \"sua-license-key-do-newrelic\"" -ForegroundColor Cyan
    }
    Write-Host ""
    Write-Host "O deploy continuara, mas o OTEL Collector pode nao funcionar corretamente." -ForegroundColor Yellow
} else {
    Write-Success "OpenTelemetry configurado (Datadog + New Relic)"
}

# ============================================
# MODO DESTROY
# ============================================

if ($Destroy) {
    Write-Title "DESTRUINDO INFRAESTRUTURA"
    
    Write-Warning "Isso vai DESTRUIR toda a infraestrutura na AWS!"
    $confirm = Read-Host "Tem certeza? Digite 'sim' para confirmar"
    
    if ($confirm -eq "sim") {
        Write-Step "Executando terraform destroy..."
        terraform destroy -auto-approve
        Write-Success "Infraestrutura destruida!"
    } else {
        Write-Info "Operacao cancelada."
    }
    exit 0
}

# ============================================
# MODO PLAN
# ============================================

if ($Plan) {
    Write-Title "MODO PLAN - Apenas visualizacao"
    
    Write-Step "Inicializando Terraform..."
    terraform init
    
    Write-Step "Executando terraform plan..."
    terraform plan
    
    Write-Success "Plan concluido! Nenhuma alteracao foi feita."
    exit 0
}

# ============================================
# ETAPA 4: Criar ECR via Terraform
# ============================================

if (-not $SkipBuild) {
    Write-Title "ETAPA 4: Criando Repositorio ECR"
    
    Write-Step "Inicializando Terraform..."
    terraform init
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Falha ao inicializar Terraform!"
        exit 1
    }
    
    Write-Step "Criando repositorio ECR..."
    terraform apply "-target=aws_ecr_repository.app" -auto-approve
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Falha ao criar repositorio ECR!"
        exit 1
    }
    Write-Success "Repositorio ECR criado!"
    
    # ============================================
    # ETAPA 5: Build Docker
    # ============================================
    
    Write-Title "ETAPA 5: Build da Imagem Docker"
    
    Write-Step "Fazendo login no ECR..."
    aws ecr get-login-password --region $AWS_REGION | docker login --username AWS --password-stdin "$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com"
    Write-Success "Login no ECR realizado"
    
    Write-Step "Construindo imagem Docker..."
    $IMAGE_TAG = (Get-Date -Format "yyyyMMdd-HHmmss")
    $ECR_URI = "$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$ECR_REPO_NAME"
    
    Push-Location $PROJECT_ROOT
    docker build -t "${ECR_URI}:${IMAGE_TAG}" -t "${ECR_URI}:latest" .
    Pop-Location
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Falha no build da imagem Docker!"
        exit 1
    }
    Write-Success "Imagem construida: ${ECR_URI}:${IMAGE_TAG}"
    
    # ============================================
    # ETAPA 6: Push para ECR
    # ============================================
    
    Write-Title "ETAPA 6: Push para ECR"
    
    Write-Step "Enviando imagem para ECR..."
    docker push "${ECR_URI}:${IMAGE_TAG}"
    docker push "${ECR_URI}:latest"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Falha no push da imagem!"
        exit 1
    }
    Write-Success "Imagem enviada para ECR!"
    Write-Info "URI: ${ECR_URI}:${IMAGE_TAG}"
    
    # Atualizar tag no tfvars
    Write-Step "Atualizando tag no terraform.tfvars..."
    $tfvarsPath = "terraform.tfvars"
    $tfvarsContent = Get-Content $tfvarsPath -Raw
    
    if ($tfvarsContent -match 'docker_image_tag\s*=') {
        # Substituir tag existente
        $tfvarsContent = $tfvarsContent -replace 'docker_image_tag\s*=\s*"[^"]*"', "docker_image_tag  = `"$IMAGE_TAG`""
    } else {
        # Adicionar tag após docker_image_repo
        $tfvarsContent = $tfvarsContent -replace '(docker_image_repo\s*=\s*"[^"]*")', "`$1`ndocker_image_tag  = `"$IMAGE_TAG`""
    }
    
    Set-Content $tfvarsPath $tfvarsContent
    Write-Success "Tag atualizada para: $IMAGE_TAG"
}

# ============================================
# ETAPA 7: Deploy Terraform (Infraestrutura completa)
# ============================================

if (-not $SkipInfra) {
    Write-Title "ETAPA 7: Deploy da Infraestrutura"
    
    Write-Step "Inicializando Terraform..."
    terraform init
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Falha ao inicializar Terraform!"
        exit 1
    }
    
    Write-Step "Validando configuracao..."
    terraform validate
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Configuracao Terraform invalida!"
        exit 1
    }
    
    Write-Step "Aplicando configuracao..."
    Write-Warning "Isso pode levar 15-20 minutos para criar o cluster EKS!"
    
    terraform apply -auto-approve
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Falha ao aplicar Terraform!"
        exit 1
    }
    Write-Success "Infraestrutura criada com sucesso!"
}

# ============================================
# ETAPA 8: Configurar kubectl e Namespace
# ============================================

Write-Title "ETAPA 8: Configurando Acesso ao Cluster"

Write-Step "Configurando kubectl..."
$EKS_CLUSTER_NAME = terraform output -raw eks_cluster_name 2>$null
if (-not $EKS_CLUSTER_NAME) {
    $EKS_CLUSTER_NAME = "eks-mecanicaos"
}

aws eks update-kubeconfig --region $AWS_REGION --name $EKS_CLUSTER_NAME
Write-Success "kubectl configurado!"

Write-Step "Verificando namespace 'mecanicaos'..."
$namespaceCheck = kubectl get namespace mecanicaos --ignore-not-found -o name
if ($namespaceCheck) {
    Write-Success "Namespace 'mecanicaos' ja existe."
} else {
    Write-Info "Namespace 'mecanicaos' nao encontrado. Criando..."
    kubectl create namespace mecanicaos
    Write-Success "Namespace 'mecanicaos' criado."
}

# ============================================
# ETAPA 9: Verificar Deploy e Aguardar Load Balancer
# ============================================

Write-Title "ETAPA 9: Verificando Deploy"

Write-Step "Aguardando Load Balancer ficar pronto..."
$LB_URL = $null
$maxRetries = 60 # 60 tentativas * 10s = 10 minutos
$retryCount = 0

while ($retryCount -lt $maxRetries) {
    Write-Debug "Tentativa $retryCount..."
    # Diagnostico: Namespace existe?
    $nsCheck = kubectl get ns mecanicaos -o name --ignore-not-found
    if (-not $nsCheck) {
        Write-Warning "Diagnostico: Namespace 'mecanicaos' nao encontrado. Aguardando criacao..."
    } else {
        Write-Debug "Diagnostico: Namespace 'mecanicaos' OK."
    }

    # Diagnostico: Service existe?
    $svcCheck = kubectl get svc mecanicaos-service -n mecanicaos -o name --ignore-not-found
    if (-not $svcCheck) {
        Write-Warning "Diagnostico: Service 'mecanicaos-service' nao encontrado. Aguardando criacao..."
    } else {
        Write-Debug "Diagnostico: Service 'mecanicaos-service' OK."
    }

    # Tenta obter a URL
    $LB_URL = kubectl get svc mecanicaos-service -n mecanicaos -o jsonpath='{.status.loadBalancer.ingress[0].hostname}' 2>$null
    if ($LB_URL) {
        Write-Success "Load Balancer esta pronto!"
        Write-Info "URL: $LB_URL"
        break
    }

    # Diagnostico: Pods estao prontos?
    $readyPods = (kubectl get pods -n mecanicaos -l app=mecanicaos-api -o jsonpath='{.items[*].status.conditions[?(@.type=="Ready")].status}' 2>$null) -split ' ' | Where-Object { $_ -eq 'True' }
    $totalPods = (kubectl get pods -n mecanicaos -l app=mecanicaos-api -o jsonpath='{.items[*].metadata.name}' 2>$null) -split ' '
    if ($totalPods.Count -gt 0) {
        Write-Info "Diagnostico: $($readyPods.Count) de $($totalPods.Count) pods estao prontos."
        if ($readyPods.Count -eq 0) {
            Write-Warning "Nenhum pod esta 'Ready'. Verificando eventos..."
            kubectl get events -n mecanicaos --sort-by='.metadata.creationTimestamp' --field-selector involvedObject.kind=Pod | Select-Object -Last 5
        }
    } else {
        Write-Warning "Diagnostico: Nenhum pod encontrado para a aplicacao ainda."
    }

    $retryCount++
    $timeLeft = ($maxRetries - $retryCount) * 10
    Write-Host "Tentativa $retryCount de $maxRetries... Aguardando 10s. (Tempo restante estimado: $($timeLeft)s)" -ForegroundColor Gray
    Start-Sleep -Seconds 10
}

if (-not $LB_URL) {
    Write-Error "Timeout! O Load Balancer nao ficou pronto em 10 minutos."
    Write-Info "Verifique o status do servico com: kubectl get svc mecanicaos-service -n mecanicaos -o yaml"
    # O script continua para mostrar outras informações úteis.
}

Write-Step "Status final dos Pods (mecanicaos):"
kubectl get pods -n mecanicaos

# ============================================
# ETAPA 10: Logs e Diagnóstico Automático
# ============================================

Write-Title "ETAPA 10: Logs e Diagnostico"

Write-Step "Buscando logs da aplicacao..."
try {
    # Tenta buscar os logs dos pods com o label app=mecanicaos-api
    $appLogs = kubectl logs -n mecanicaos -l app=mecanicaos-api --tail=50
    Write-Host "--- Ultimos 50 logs da API ---" -ForegroundColor Gray
    Write-Host $appLogs
    Write-Host "-----------------------------" -ForegroundColor Gray
    Write-Success "Logs da aplicacao obtidos."
} catch {
    Write-Warning "Nao foi possivel obter os logs da aplicacao. Os pods podem estar iniciando."
}

# --- Diagnóstico do OpenTelemetry Collector ---

Write-Step "Diagnosticando OpenTelemetry Collector..."
$otelPodName = kubectl get pods -n observability -l app=otel-collector -o jsonpath='{.items[0].metadata.name}' 2>$null

if ($otelPodName) {
    $otelPodStatus = kubectl get pod $otelPodName -n observability -o jsonpath='{.status.phase}' 2>$null
    if ($otelPodStatus -eq "Running") {
        Write-Success "Pod do OpenTelemetry Collector esta RUNNING."

        # Verificar logs por falta de API keys
        $otelLogs = kubectl logs $otelPodName -n observability 2>&1
        if ($otelLogs -match "api_key not available" -or $otelLogs -match "license_key is required") {
            Write-Info "Observabilidade (OTEL): Coletor rodando, mas API Key ausente (opcional)."
            Write-Info "Para habilitar, configure 'datadog_api_key' ou 'newrelic_license_key' no terraform.tfvars."
        } else {
            Write-Success "Observabilidade (OTEL): Coletor rodando e configurado."
        }
    } else {
        Write-Info "Observabilidade (OTEL): Pod do coletor no estado '$otelPodStatus'."
    }
} else {
    Write-Info "Observabilidade (OTEL): Nao configurada (opcional)."
}

# ============================================
# RESUMO FINAL
# ============================================

Write-Title "RELATORIO FINAL DO DEPLOY"

# Obter outputs do Terraform para o relatorio
$outputs = terraform output -json | ConvertFrom-Json
$envType = $outputs.environment_type.value
$otelStatus = $outputs.otel_status.value
$namespace = $outputs.namespace.value

# Status do Load Balancer
if ($LB_URL) {
    $lbStatus = "[OK] Load Balancer online"
} else {
    $lbStatus = "[AVISO] Load Balancer ainda provisionando"
}

# Status do OTEL
if ($otelStatus -eq "Configurado") {
    $otelReport = "[OK] Observabilidade configurada (OTEL)"
} else {
    $otelReport = "[INFO] Observabilidade nao configurada (opcional)"
}

# Imprimir Relatorio
Write-Host "✔️  Namespace '$namespace' verificado/criado"
Write-Host "✔️  Aplicacao 'mecanicaos-api' deployada"
Write-Host "$lbStatus"
Write-Host "$otelReport"
Write-Host "ℹ️  Ambiente detectado: $envType"
Write-Host ""

# Imprimir Outputs Amigaveis
if ($LB_URL) {
    Write-Host "Sua API esta no ar! 🎉" -ForegroundColor Green
    Write-Host "-------------------------"
    Write-Host "Base URL: " -NoNewline; Write-Host "http://$LB_URL" -ForegroundColor Cyan
    Write-Host "Swagger:  " -NoNewline; Write-Host "http://$LB_URL/swagger/index.html" -ForegroundColor Cyan
    Write-Host "Health:"
    Write-Host "  - Live: " -NoNewline; Write-Host "http://$LB_URL/health/live" -ForegroundColor Cyan
    Write-Host "  - Ready:" -NoNewline; Write-Host "http://$LB_URL/health/ready" -ForegroundColor Cyan
    Write-Host ""
} else {
    Write-Warning "Aguarde o Load Balancer ficar online para acessar a API."
    Write-Info "Use 'terraform output api_url' para obter a URL em alguns minutos."
    Write-Host ""
}
