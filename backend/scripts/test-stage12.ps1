Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common.ps1')

$backendRoot = Get-BackendRoot
$python = Assert-VenvPythonPath
$artifactsDir = Get-BackendPath '.artifacts'
$pycacheDir = Join-Path $artifactsDir 'pycache'
$pytestCacheDir = Join-Path $artifactsDir 'pytest-cache'
$testResultsPath = Join-Path $artifactsDir 'backend-test-results.xml'
$compileScriptPath = Join-Path $artifactsDir 'compile-check.py'

Initialize-LocalDirectories
Ensure-Directory -Path $pycacheDir
Ensure-Directory -Path $pytestCacheDir

$previousPycachePrefix = $env:PYTHONPYCACHEPREFIX
$env:PYTHONPYCACHEPREFIX = $pycacheDir

try {
    $compileScript = @"
from pathlib import Path
import py_compile

for path in sorted(Path('.').rglob('*.py')):
    parts = set(path.parts)
    if '.venv' in parts or '.artifacts' in parts or '.local' in parts:
        continue
    py_compile.compile(str(path), doraise=True)
"@
    Set-Content -Path $compileScriptPath -Value $compileScript -Encoding utf8

    Write-Host 'Running backend syntax validation.'
    Invoke-Checked -FilePath $python -Arguments @($compileScriptPath) -WorkingDirectory $backendRoot

    Write-Host 'Running backend pytest suite.'
    Invoke-Checked -FilePath $python -Arguments @('-m', 'pytest', 'tests', '-q', '-o', "cache_dir=$pytestCacheDir", "--junitxml=$testResultsPath") -WorkingDirectory $backendRoot

    Write-Host "Backend tests completed successfully. Results: $testResultsPath"
}
finally {
    Remove-Item -Path $compileScriptPath -ErrorAction SilentlyContinue

    if ($null -eq $previousPycachePrefix) {
        Remove-Item Env:PYTHONPYCACHEPREFIX -ErrorAction SilentlyContinue
    }
    else {
        $env:PYTHONPYCACHEPREFIX = $previousPycachePrefix
    }
}
