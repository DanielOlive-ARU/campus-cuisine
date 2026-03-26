# Local Testing Platform

## Purpose
This document defines the local backend testing workflow used while automatic GitHub Actions runs are disabled to control usage.

## Scope
This local platform is for backend development during active implementation.
It does not replace the final two-track submission strategy:
- source repository for assessment and code review
- packaged runnable artifact later in the project

## Current Decision
- GitHub Actions validation is retained, but manual-only.
- Day-to-day validation is performed locally.
- Local build, cache, test, and smoke-test outputs must stay out of git.

## Preferred Local Tooling
Preferred option:
- `uv`

Fallback option:
- Python 3.12 with `venv` and `pip`

Rationale:
- `uv` reduces dependence on a preinstalled `pip`
- this machine has already shown that shell-side Python may exist without `pip`
- `uv` can provision a managed CPython runtime when needed

## Local Paths
All local artifacts should be written only to ignored paths:
- `backend/.venv/`
- `backend/.env`
- `backend/.artifacts/`
- `backend/.local/`

These must not be committed.

## Scripts
### `backend/scripts/bootstrap.ps1`
Creates the local environment, installs dependencies, prepares ignored local directories, and creates `.env` from `.env.example` if needed.

### `backend/scripts/test-stage12.ps1`
Runs:
- syntax validation for backend Python files
- `pytest` for the current backend test suite

Artifacts written to:
- `backend/.artifacts/pycache/`
- `backend/.artifacts/pytest-cache/`
- `backend/.artifacts/test-results-stage12.xml`

### `backend/scripts/smoke-stage12.ps1`
Starts the backend locally and validates:
- `GET /health`
- database creation
- startup seeding
- static file serving

Artifacts written to:
- `backend/.local/stage12-smoke.db`
- `backend/.local/stage12-static/`
- `backend/.artifacts/stage12-smoke.stdout.log`
- `backend/.artifacts/stage12-smoke.stderr.log`

### `backend/scripts/run-api.ps1`
Runs the backend locally for manual testing and future frontend integration.

### `backend/scripts/clean-local.ps1`
Removes local ignored artifacts and Python cache directories.

## Recommended Local Flow
```powershell
./backend/scripts/bootstrap.ps1
./backend/scripts/test-stage12.ps1
./backend/scripts/smoke-stage12.ps1
```

For manual API work:
```powershell
./backend/scripts/run-api.ps1 -Port 8000 -SeedOnStartup
```

## Current Validation Checklist
A local validation pass should confirm:
- environment can be created
- dependencies install successfully
- backend syntax validation passes
- `pytest backend/tests` passes
- `GET /health` returns success
- SQLite database is created on startup
- tables are created on startup
- seed runs only when the menu table is empty
- static files are served from `/images`

## Current Validation Evidence
Successful local Windows PowerShell run on `2026-03-23`:
- `./backend/scripts/bootstrap.ps1`
- `./backend/scripts/test-stage12.ps1`
- `./backend/scripts/smoke-stage12.ps1`

Observed result:
- `uv` provisioned `CPython 3.12.13`
- backend dependencies installed successfully
- `8` backend Stage 1/2 tests passed
- smoke test completed successfully

Successful extended local validation on `2026-03-26`:
- `./backend/scripts/test-stage12.ps1`
- `./backend/scripts/smoke-stage12.ps1`
- `./backend/.venv/Scripts/python.exe -m pytest backend/tests -q`

Observed result:
- backend API, admin CRUD, and order tests all passed
- `37` backend tests passed
- scripted validation and smoke testing both completed successfully

## Future Use
As the backend grows, this platform should be extended phase by phase before automatic CI is re-enabled more broadly.
