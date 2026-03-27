Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common.ps1')

$pathsToRemove = @(
    (Get-BackendPath '.venv'),
    (Get-BackendPath '.artifacts'),
    (Get-BackendPath '.local')
)

foreach ($path in $pathsToRemove) {
    if (Test-Path $path) {
        Remove-Item -Path $path -Recurse -Force
        Write-Host "Removed $path"
    }
}

Get-ChildItem -Path (Get-BackendRoot) -Directory -Recurse -Filter '__pycache__' | ForEach-Object {
    Remove-Item -Path $_.FullName -Recurse -Force
    Write-Host "Removed $($_.FullName)"
}
