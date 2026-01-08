# Verifica a disponibilidade dos comandos essenciais
function Check-Command($command) {
    if (Get-Command $command -ErrorAction SilentlyContinue) {
        Write-Host "[OK] '$command' encontrado." -ForegroundColor Green
        return $true
    } else {
        Write-Host "[X] '$command' não encontrado no PATH." -ForegroundColor Red
        return $false
    }
}

Write-Host "Iniciando Sanity Check do Ambiente..." -ForegroundColor Cyan

$allChecksPassed = $true

# Lista de comandos a serem verificados
$commandsToCheck = @("aws", "terraform", "kubectl")

foreach ($cmd in $commandsToCheck) {
    if (-not (Check-Command $cmd)) {
        $allChecksPassed = $false
    }
}

# Verificação específica de autenticação AWS
Write-Host "`nVerificando autenticação AWS..." -ForegroundColor Cyan
aws sts get-caller-identity --output json > $null
if ($LASTEXITCODE -ne 0) {
    Write-Host "[X] AWS NÃO AUTENTICADO (token inválido ou expirado)" -ForegroundColor Red
    $allChecksPassed = $false
} else {
    Write-Host "[OK] AWS CLI e autenticação STS funcionando." -ForegroundColor Green
}


if (-not $allChecksPassed) {
    Write-Host "`nSanity check FALHOU. Corrija os erros acima e tente novamente." -ForegroundColor Red
    exit 1 # Termina o script com um código de erro
}

Write-Host "`nSanity check concluído com sucesso!" -ForegroundColor Green
