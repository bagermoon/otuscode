param(
    [Parameter(ValueFromRemainingArguments = $true)]
    [string[]]$Args
)

$solutionRoot = $PSScriptRoot
$targetDir = Join-Path $solutionRoot "artifacts/bin/RestoRate.E2ETests/debug"
$scriptPath = Join-Path $targetDir "playwright.ps1"

if (-Not (Test-Path $scriptPath)) {
    Write-Host "playwright.ps1 not found in $targetDir"
    exit 1
}

Push-Location $targetDir
try {
    Write-Host "Running $scriptPath with arguments: $Args"
    & $scriptPath @Args
} finally {
    Pop-Location
}
