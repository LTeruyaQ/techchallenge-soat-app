Write-Host "AWS Sanity Check" -ForegroundColor Cyan

aws sts get-caller-identity --output json
if ($LASTEXITCODE -ne 0) {
    Write-Host "[X] AWS NAO AUTENTICADO (token invalido ou expirado)" -ForegroundColor Red
    exit 1
}

# A checagem 'eks list-clusters' foi removida pois causa erro de Acesso Negado no ambiente AWS Academy.
# A funcionalidade principal do script (criar um novo cluster) não depende deste comando.

Write-Host "[OK] AWS CLI e STS funcionando corretamente" -ForegroundColor Green

# A checagem do psql foi removida pois a inicialização do banco de dados agora é feita por um Job no Kubernetes.
