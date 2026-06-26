$ScriptDir = $PSScriptRoot
$TemplatesDir = Join-Path $ScriptDir "templates"

# Шаблоны пересобираются заново. Первый — solution-шаблон Blazor BFF + Aspire
# с модулями-микросервисами (см. план). Добавляй сюда по мере готовности.
$templates = @(
    # "cheetah-bff"
)

Write-Host "Installing Cheetah templates..." -ForegroundColor Cyan

foreach ($template in $templates) {
    $path = Join-Path $TemplatesDir $template

    Write-Host "`n  [$template]" -ForegroundColor Yellow

    dotnet new uninstall $path 2>&1 | Out-Null

    $result = dotnet new install $path 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  OK" -ForegroundColor Green
    } else {
        Write-Host "  FAILED" -ForegroundColor Red
        Write-Host $result
    }
}

Write-Host "`nDone. Installed templates:" -ForegroundColor Cyan
dotnet new list --tag Cheetah
