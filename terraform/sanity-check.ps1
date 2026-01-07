Write-Host "AWS Sanity Check" -ForegroundColor Cyan

aws sts get-caller-identity --output json
if ($LASTEXITCODE -ne 0) {
    Write-Host "[X] AWS NAO AUTENTICADO (token invalido ou expirado)" -ForegroundColor Red
    exit 1
}

# A checagem 'eks list-clusters' foi removida pois causa erro de Acesso Negado no ambiente AWS Academy.
# A funcionalidade principal do script (criar um novo cluster) não depende deste comando.

Write-Host "[OK] AWS CLI e STS funcionando corretamente" -ForegroundColor Green

# Checagem do psql
try {
    psql --version | Out-Null
    Write-Host "[OK] psql (PostgreSQL client) encontrado." -ForegroundColor Green
} catch {
    Write-Host "[X] psql não encontrado no PATH. Ele é necessário para a inicialização do banco de dados." -ForegroundColor Red
    exit 1
}
