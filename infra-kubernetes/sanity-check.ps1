Write-Host "AWS Sanity Check" -ForegroundColor Cyan

aws sts get-caller-identity --output json
if ($LASTEXITCODE -ne 0) {
    Write-Host "[X] AWS NAO AUTENTICADO (token invalido ou expirado)" -ForegroundColor Red
    exit 1
}

aws eks list-clusters --region us-east-1 --output table
if ($LASTEXITCODE -ne 0) {
    Write-Host "[X] Falha ao listar clusters EKS" -ForegroundColor Red
    exit 1
}

Write-Host "[OK] AWS CLI e STS funcionando corretamente" -ForegroundColor Green
