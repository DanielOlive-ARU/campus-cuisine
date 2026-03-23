param(
    [int]$Port = 8000,
    [switch]$SeedOnStartup
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common.ps1')

$backendRoot = Get-BackendRoot
$python = Assert-VenvPythonPath
$artifactsDir = Get-BackendPath '.artifacts'
$localDir = Get-BackendPath '.local'
$pycacheDir = Join-Path $artifactsDir 'pycache'
$dbPath = Join-Path $localDir 'local-dev.db'

Initialize-LocalDirectories
Ensure-EnvFile
Ensure-Directory -Path $pycacheDir

$dbUrlPath = ([System.IO.Path]::GetFullPath($dbPath)).Replace('\', '/')
$dbUrl = "sqlite:///$dbUrlPath"

$previousDatabaseUrl = $env:DATABASE_URL
$previousSeedOnStartup = $env:SEED_ON_STARTUP
$previousPycachePrefix = $env:PYTHONPYCACHEPREFIX

try {
    $env:DATABASE_URL = $dbUrl
    $env:SEED_ON_STARTUP = $(if ($SeedOnStartup.IsPresent) { 'true' } else { 'false' })
    $env:PYTHONPYCACHEPREFIX = $pycacheDir

    Write-Host "Starting backend API on http://127.0.0.1:$Port"
    Invoke-Checked -FilePath $python -Arguments @('-m', 'uvicorn', 'app.main:app', '--host', '127.0.0.1', '--port', $Port.ToString(), '--reload') -WorkingDirectory $backendRoot
}
finally {
    if ($null -eq $previousDatabaseUrl) {
        Remove-Item Env:DATABASE_URL -ErrorAction SilentlyContinue
    }
    else {
        $env:DATABASE_URL = $previousDatabaseUrl
    }

    if ($null -eq $previousSeedOnStartup) {
        Remove-Item Env:SEED_ON_STARTUP -ErrorAction SilentlyContinue
    }
    else {
        $env:SEED_ON_STARTUP = $previousSeedOnStartup
    }

    if ($null -eq $previousPycachePrefix) {
        Remove-Item Env:PYTHONPYCACHEPREFIX -ErrorAction SilentlyContinue
    }
    else {
        $env:PYTHONPYCACHEPREFIX = $previousPycachePrefix
    }
}
