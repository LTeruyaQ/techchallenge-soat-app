# ============================================
# Script de Deploy COMPLETO - MecanicaOS (AWS Academy)
# ============================================

param(
    [switch]$Destroy,
    [switch]$Plan,
    [string]$AWS_REGION = "us-east-1"
)

# Termina o script imediatamente se qualquer comando falhar
$ErrorActionPreference = "Stop"

# ======================================================
# FUNÇÕES DE LOG
# ======================================================
function Write-Title($msg) { Write-Host "`n============================================" -ForegroundColor Cyan; Write-Host " $msg" -ForegroundColor Cyan; Write-Host "============================================" -ForegroundColor Cyan }
function Write-Step($msg)     { Write-Host "`n==> $msg" -ForegroundColor Yellow }
function Write-Success($msg)  { Write-Host "[OK] $msg" -ForegroundColor Green }
function Write-ErrorMsg($msg) { Write-Host "[X] $msg" -ForegroundColor Red }
function Write-Info($msg)     { Write-Host "    $msg" -ForegroundColor Gray }

# Função para checar o resultado do último comando
function Check-Last-Exit-Code {
    if ($LASTEXITCODE -ne 0) {
        Write-ErrorMsg "Comando anterior falhou com código de saída $LASTEXITCODE. Abortando."
        exit 1
    }
}

# ======================================================
# ETAPA 0: SANITY CHECK
# ======================================================
Write-Title "ETAPA 0: Sanity Check do Ambiente"
if (-not (Test-Path ".\sanity-check.ps1")) {
    Write-ErrorMsg "sanity-check.ps1 não encontrado."
    exit 1
}
try {
    Write-Step "Executando sanity-check.ps1"
    .\sanity-check.ps1
    Check-Last-Exit-Code
    Write-Success "Sanity check passou."
} catch {
    Write-ErrorMsg "Sanity check falhou."
    throw
}

# ======================================================
# ETAPA 1: CREDENCIAIS AWS
# ======================================================
Write-Title "ETAPA 1: Verificando Credenciais AWS"
try {
    $identity = aws sts get-caller-identity --output json | ConvertFrom-Json
    $AWS_ACCOUNT_ID = $identity.Account
    Write-Success "AWS Account ID: $AWS_ACCOUNT_ID"
} catch {
    Write-ErrorMsg "Credenciais AWS inválidas."
    exit 1
}

# ======================================================
# ETAPA 2: DETECÇÃO DE RECURSOS EXISTENTES
# ======================================================
Write-Title "ETAPA 2: Detecção de Recursos Existentes"
$ProjectName = "mecanicaos" # Usado para filtrar tags

Write-Step "Procurando por VPC existente com a tag 'Project=MecanicaOS'..."
$VpcId = aws ec2 describe-vpcs --filters "Name=tag:Project,Values=$ProjectName" --query "Vpcs[0].VpcId" --output text
if ($VpcId -ne "None" -and $VpcId) {
    Write-Success "VPC encontrada: $VpcId"
    $env:TF_VAR_existing_vpc_id = $VpcId

    Write-Step "Procurando e classificando todas as subnets na VPC..."
    $AllSubnetsJson = aws ec2 describe-subnets --filters "Name=vpc-id,Values=$VpcId" --output json
    $AllSubnets = $AllSubnetsJson | ConvertFrom-Json

    # Garante que $PublicSubnetIds e $PrivateSubnetIds sejam sempre arrays
    $PublicSubnetIds = @()
    $PrivateSubnetIds = @()

    if ($null -ne $AllSubnets -and $null -ne $AllSubnets.Subnets) {
        Write-Info "Total de subnets encontradas na VPC: $($AllSubnets.Subnets.Count)"
        foreach ($subnet in $AllSubnets.Subnets) {
            $subnetId = $subnet.SubnetId
            $isPublic = $subnet.MapPublicIpOnLaunch
            $cidr = $subnet.CidrBlock

            Write-Info "Analisando Subnet: $subnetId (CIDR: $cidr, MapPublicIpOnLaunch: $isPublic)"

            # A classificação primária é pelo atributo MapPublicIpOnLaunch.
            if ($isPublic) {
                $PublicSubnetIds += $subnetId
            } else {
                $PrivateSubnetIds += $subnetId
            }
        }
    }

    # Passa as listas (mesmo que vazias) para o Terraform de forma explícita para garantir consistência
    $env:TF_VAR_existing_public_subnet_ids = ($PublicSubnetIds | ConvertTo-Json -Compress)
    $env:TF_VAR_existing_private_subnet_ids = ($PrivateSubnetIds | ConvertTo-Json -Compress)

    Write-Success "Subnets públicas detectadas: $($PublicSubnetIds.Count)"
    Write-Info "TF_VAR_existing_public_subnet_ids = $($env:TF_VAR_existing_public_subnet_ids)"

    Write-Success "Subnets privadas detectadas: $($PrivateSubnetIds.Count)"
    Write-Info "TF_VAR_existing_private_subnet_ids = $($env:TF_VAR_existing_private_subnet_ids)"

} else {
    Write-Info "Nenhuma VPC existente encontrada. Uma nova será criada."
    # Garante que as variáveis de subnet estejam explicitamente vazias se nenhuma VPC for encontrada
    $env:TF_VAR_existing_vpc_id = ""
    $env:TF_VAR_existing_public_subnet_ids = "[]"
    $env:TF_VAR_existing_private_subnet_ids = "[]"
}


# ======================================================
# ETAPA 2.5: MODOS DE EXECUÇÃO (DESTROY / PLAN)
# ======================================================
if ($Destroy) {
    Write-Title "MODO DESTROY"
    terraform init; Check-Last-Exit-Code
    terraform destroy -auto-approve; Check-Last-Exit-Code
    Write-Success "Infraestrutura destruída."
    exit 0
}

if ($Plan) {
    Write-Title "MODO PLAN"
    terraform init; Check-Last-Exit-Code
    terraform plan; Check-Last-Exit-Code
    Write-Success "Plano gerado."
    exit 0
}

# ======================================================
# ETAPA 3: PREPARAÇÃO DO PACOTE DA LAMBDA
# ======================================================
Write-Title "ETAPA 3: Preparação do Pacote da Lambda"
$LambdaPackageDir = ".\lambda\package"

Write-Step "Limpando diretório de pacote antigo..."
if (Test-Path $LambdaPackageDir) {
    Remove-Item -Recurse -Force $LambdaPackageDir
}
New-Item -ItemType Directory -Path $LambdaPackageDir | Out-Null

Write-Step "Instalando dependências Python..."
if (Test-Path ".\lambda\requirements.txt") {
    pip install --target $LambdaPackageDir -r ".\lambda\requirements.txt"; Check-Last-Exit-Code
    Write-Success "Dependências da Lambda instaladas."
} else {
    Write-Warning "Arquivo requirements.txt não encontrado."
}

Write-Step "Copiando o código da função Lambda..."
Copy-Item -Path ".\lambda\main.py" -Destination $LambdaPackageDir; Check-Last-Exit-Code
Write-Success "Código da Lambda copiado para o diretório do pacote."

# ======================================================
# ETAPA 4: DEPLOY DA INFRAESTRUTURA COMPLETA
# ======================================================
Write-Title "ETAPA 4: Deploy da Infraestrutura (VPC, EKS, RDS, Lambda, API GW)"

# Injeta variáveis vazias para suprimir prompts de observability
$env:TF_VAR_datadog_api_key = ""
$env:TF_VAR_newrelic_license_key = ""

terraform init; Check-Last-Exit-Code
terraform validate; Check-Last-Exit-Code
Write-Step "Aplicando a configuração do Terraform... Isso pode levar vários minutos."
terraform apply -auto-approve; Check-Last-Exit-Code
Write-Success "Infraestrutura provisionada com sucesso."

# ======================================================
# ETAPA 4: CONFIGURANDO KUBECTL
# ======================================================
Write-Title "ETAPA 4: Configurando kubectl"
$EKS_CLUSTER_NAME = terraform output -raw eks_cluster_name; Check-Last-Exit-Code
aws eks update-kubeconfig --region $AWS_REGION --name $EKS_CLUSTER_NAME; Check-Last-Exit-Code
Write-Success "kubectl configurado para o cluster '$EKS_CLUSTER_NAME'."

# ======================================================
# ETAPA 5: DEPLOY DA APLICAÇÃO NO EKS
# ======================================================
Write-Title "ETAPA 5: Deploy da Aplicação no EKS"
kubectl apply -f "..\k8s\"; Check-Last-Exit-Code
Write-Step "Aguardando alguns segundos para os pods da aplicação iniciarem..."
Start-Sleep -Seconds 30
Write-Success "Deploy da aplicação enviado ao EKS."

# ======================================================
# ETAPA 6: INICIALIZAÇÃO DO BANCO DE DADOS
# ======================================================
Write-Title "ETAPA 6: Inicialização do Banco de Dados"
Write-Step "Obtendo detalhes de conexão do RDS..."
$RDSEndpoint = terraform output -raw rds_endpoint; Check-Last-Exit-Code
$RDSUsername = terraform output -raw rds_username; Check-Last-Exit-Code
$RDSPassword = terraform output -raw --sensitive rds_password; Check-Last-Exit-Code
$DBName      = terraform output -raw rds_dbname; Check-Last-Exit-Code

Write-Step "Executando script SQL (rds-init.sql)..."
try {
    $env:PGPASSWORD = $RDSPassword
    psql --host=$RDSEndpoint --port=5432 --username=$RDSUsername --dbname=$DBName -f ".\rds-init.sql"
    Check-Last-Exit-Code
    Write-Success "Banco de dados inicializado com sucesso."
} catch {
    Write-ErrorMsg "Falha ao executar o script SQL. Verifique se 'psql' está instalado e no PATH."
    throw
} finally {
    if (Test-Path Env:\PGPASSWORD) { Remove-Item Env:\PGPASSWORD }
}

# ======================================================
# ETAPA 7: RESUMO FINAL DO DEPLOY
# ======================================================
Write-Title "ETAPA 7: Resumo do Deploy"
$ApiGatewayUrl = terraform output -raw api_gateway_endpoint; Check-Last-Exit-Code
$SwaggerUrl = "$ApiGatewayUrl/swagger" # Assumindo que o Swagger está em /swagger

Write-Success "✔️ EKS Cluster Criado: $EKS_CLUSTER_NAME"
Write-Success "✔️ RDS Endpoint Criado: $RDSEndpoint"
Write-Success "✔️ Lambda de Autenticação Criada"
Write-Success "✔️ API Gateway Criado"
Write-Info  "-------------------------------------------"
Write-Info  "URL Pública da API: $ApiGatewayUrl"
Write-Info  "URL do Swagger UI:  $SwaggerUrl"
Write-Info  "-------------------------------------------"

Write-Title "Deploy finalizado com sucesso!"
