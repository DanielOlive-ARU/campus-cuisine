Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-BackendRoot {
    return (Split-Path -Parent $PSScriptRoot)
}

function Get-RepoRoot {
    return (Split-Path -Parent (Get-BackendRoot))
}

function Get-BackendPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ChildPath
    )

    return (Join-Path (Get-BackendRoot) $ChildPath)
}

function Ensure-Directory {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not (Test-Path $Path)) {
        New-Item -ItemType Directory -Path $Path | Out-Null
    }
}

function Initialize-LocalDirectories {
    Ensure-Directory -Path (Get-BackendPath '.artifacts')
    Ensure-Directory -Path (Get-BackendPath '.local')
}

function Ensure-EnvFile {
    $envPath = Get-BackendPath '.env'
    $examplePath = Get-BackendPath '.env.example'

    if (-not (Test-Path $envPath) -and (Test-Path $examplePath)) {
        Copy-Item -Path $examplePath -Destination $envPath
        Write-Host "Created $envPath from .env.example"
    }
}

function Resolve-CommandPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    $command = Get-Command $Name -ErrorAction SilentlyContinue
    if ($null -eq $command) {
        return $null
    }

    return $command.Source
}

function Get-VenvPythonPath {
    return (Get-BackendPath '.venv\Scripts\python.exe')
}

function Assert-VenvPythonPath {
    $pythonPath = Get-VenvPythonPath

    if (-not (Test-Path $pythonPath)) {
        throw "Backend virtual environment not found. Run ./backend/scripts/bootstrap.ps1 first."
    }

    return $pythonPath
}

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,

        [Parameter(Mandatory = $false)]
        [string[]]$Arguments = @(),

        [Parameter(Mandatory = $false)]
        [string]$WorkingDirectory = (Get-BackendRoot)
    )

    Push-Location $WorkingDirectory
    try {
        & $FilePath @Arguments
        if ($LASTEXITCODE -ne 0) {
            throw "Command failed: $FilePath $($Arguments -join ' ')"
        }
    }
    finally {
        Pop-Location
    }
}
