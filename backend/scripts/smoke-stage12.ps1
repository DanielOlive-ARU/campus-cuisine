Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common.ps1')

$backendRoot = Get-BackendRoot
$python = Assert-VenvPythonPath
$artifactsDir = Get-BackendPath '.artifacts'
$localDir = Get-BackendPath '.local'
$pycacheDir = Join-Path $artifactsDir 'pycache'
$staticDir = Join-Path $localDir 'stage12-static'
$dbPath = Join-Path $localDir 'stage12-smoke.db'
$stdoutPath = Join-Path $artifactsDir 'stage12-smoke.stdout.log'
$stderrPath = Join-Path $artifactsDir 'stage12-smoke.stderr.log'
$port = 8001
$baseUrl = "http://127.0.0.1:$port"

Initialize-LocalDirectories
Ensure-Directory -Path $pycacheDir
Ensure-Directory -Path $staticDir

if (Test-Path $dbPath) {
    Remove-Item -Path $dbPath -Force
}

'local-static-ok' | Set-Content -Path (Join-Path $staticDir 'probe.txt') -Encoding utf8

$dbUrlPath = ([System.IO.Path]::GetFullPath($dbPath)).Replace('\', '/')
$dbUrl = "sqlite:///$dbUrlPath"

$previousDatabaseUrl = $env:DATABASE_URL
$previousSeedOnStartup = $env:SEED_ON_STARTUP
$previousStaticDir = $env:STATIC_DIR
$previousPycachePrefix = $env:PYTHONPYCACHEPREFIX
$process = $null

try {
    $env:DATABASE_URL = $dbUrl
    $env:SEED_ON_STARTUP = 'true'
    $env:STATIC_DIR = $staticDir
    $env:PYTHONPYCACHEPREFIX = $pycacheDir

    Write-Host 'Starting local backend smoke-test server.'
    $process = Start-Process -FilePath $python -ArgumentList @('-m', 'uvicorn', 'app.main:app', '--host', '127.0.0.1', '--port', $port.ToString()) -WorkingDirectory $backendRoot -RedirectStandardOutput $stdoutPath -RedirectStandardError $stderrPath -PassThru

    $started = $false
    for ($attempt = 0; $attempt -lt 20; $attempt++) {
        Start-Sleep -Milliseconds 500

        if ($process.HasExited) {
            break
        }

        try {
            $health = Invoke-RestMethod -Uri "$baseUrl/health" -Method Get
            if ($health.status -eq 'ok') {
                $started = $true
                break
            }
        }
        catch {
        }
    }

    if (-not $started) {
        throw "Backend smoke-test server did not become ready. See $stdoutPath and $stderrPath"
    }

    if (-not (Test-Path $dbPath)) {
        throw "Smoke-test database was not created: $dbPath"
    }

    $dbAssertScript = @"
import sqlite3
from pathlib import Path
from app.models import MenuItem

with sqlite3.connect(Path(r'$dbPath')) as connection:
    count = connection.execute(f'SELECT COUNT(*) FROM {MenuItem.__table__.name}').fetchone()[0]

if count != 12:
    raise SystemExit(f'Expected 12 seeded menu items, got {count}')
"@

    Invoke-Checked -FilePath $python -Arguments @('-c', $dbAssertScript) -WorkingDirectory $backendRoot

    $staticResponse = Invoke-WebRequest -Uri "$baseUrl/images/probe.txt" -Method Get
    if ($staticResponse.Content.Trim() -ne 'local-static-ok') {
        throw 'Static file mount did not return the expected content.'
    }

    Write-Host 'Stage 1/2 smoke test completed successfully.'
}
finally {
    if ($null -ne $process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
        Wait-Process -Id $process.Id -ErrorAction SilentlyContinue
    }

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

    if ($null -eq $previousStaticDir) {
        Remove-Item Env:STATIC_DIR -ErrorAction SilentlyContinue
    }
    else {
        $env:STATIC_DIR = $previousStaticDir
    }

    if ($null -eq $previousPycachePrefix) {
        Remove-Item Env:PYTHONPYCACHEPREFIX -ErrorAction SilentlyContinue
    }
    else {
        $env:PYTHONPYCACHEPREFIX = $previousPycachePrefix
    }
}
