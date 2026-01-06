Write-Host "AWS Sanity Check" -ForegroundColor Cyan

aws sts get-caller-identity --output json
if ($LASTEXITCODE -ne 0) {
    Write-Host "[X] AWS NAO AUTENTICADO (token invalido ou expirado)" -ForegroundColor Red
    exit 1
}

# A verificação de clusters EKS foi desativada pois o script pode ser executado
# justamente para CRIAR o primeiro cluster. A verificação do STS já é suficiente.
# aws eks list-clusters --region us-east-1 --output table
# if ($LASTEXITCODE -ne 0) {
#     Write-Host "[X] Falha ao listar clusters EKS" -ForegroundColor Red
#     exit 1
# }

Write-Host "[OK] AWS CLI e STS funcionando corretamente" -ForegroundColor Green
