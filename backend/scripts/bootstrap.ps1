Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'common.ps1')

$backendRoot = Get-BackendRoot
$venvPath = Get-BackendPath '.venv'
$venvPython = Get-VenvPythonPath
$requirementsPath = Get-BackendPath 'requirements.txt'

Initialize-LocalDirectories
Ensure-EnvFile

$uv = Resolve-CommandPath 'uv'
if ($null -ne $uv) {
    Write-Host 'Using uv for backend environment setup.'
    Invoke-Checked -FilePath $uv -Arguments @('venv', $venvPath, '--python', '3.12')
    Invoke-Checked -FilePath $uv -Arguments @('pip', 'install', '--python', $venvPython, '-r', $requirementsPath)
    Write-Host 'Backend bootstrap complete.'
    exit 0
}

$py = Resolve-CommandPath 'py'
if ($null -ne $py) {
    Write-Host 'Using py launcher for backend environment setup.'
    if (-not (Test-Path $venvPython)) {
        Invoke-Checked -FilePath $py -Arguments @('-3.12', '-m', 'venv', $venvPath) -WorkingDirectory (Get-RepoRoot)
    }
    Invoke-Checked -FilePath $venvPython -Arguments @('-m', 'pip', 'install', '-r', $requirementsPath)
    Write-Host 'Backend bootstrap complete.'
    exit 0
}

$python = Resolve-CommandPath 'python'
if ($null -ne $python) {
    Write-Host 'Using python for backend environment setup.'
    if (-not (Test-Path $venvPython)) {
        Invoke-Checked -FilePath $python -Arguments @('-m', 'venv', $venvPath)
    }
    Invoke-Checked -FilePath $venvPython -Arguments @('-m', 'pip', 'install', '-r', $requirementsPath)
    Write-Host 'Backend bootstrap complete.'
    exit 0
}

throw @'
No supported local backend toolchain was found.

Recommended option:
- Install uv, then rerun ./backend/scripts/bootstrap.ps1

Fallback option:
- Install Python 3.12 with venv and pip support, then rerun ./backend/scripts/bootstrap.ps1
'@
