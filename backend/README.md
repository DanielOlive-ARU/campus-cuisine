# Backend Local Testing

Run these commands from the repository root or from the `backend/` directory on Windows PowerShell.

## Preferred Tooling
The local backend workflow is Windows-first and prefers `uv` for environment setup because it avoids relying on a preinstalled `pip`.

`uv` may provision a managed CPython runtime automatically if Python 3.12 is not already available in the normal shell `PATH`.

## Local Configuration
- `backend/.env.example` is the template.
- `backend/.env` is created locally by `bootstrap.ps1` if needed and is git-ignored.

## Scripts
- `backend/scripts/bootstrap.ps1`
  Creates the local environment, installs dependencies, prepares local directories, and creates `.env` from `.env.example` if needed.
- `backend/scripts/test-stage12.ps1`
  Runs syntax validation and the current backend Stage 1/2 pytest suite, writing artifacts only to ignored local paths.
- `backend/scripts/smoke-stage12.ps1`
  Starts the API locally, checks `/health`, verifies DB creation and seed behavior, and verifies static file serving.
- `backend/scripts/run-api.ps1`
  Runs the backend API locally for manual testing and frontend integration.
- `backend/scripts/clean-local.ps1`
  Removes ignored local artifacts such as `.venv`, `.artifacts`, `.local`, and Python cache directories.

## Typical Flow
```powershell
./backend/scripts/bootstrap.ps1
./backend/scripts/test-stage12.ps1
./backend/scripts/smoke-stage12.ps1
```

## Manual CI
The GitHub Actions backend Stage 1/2 workflow is retained for manual use only. Automatic triggers are disabled to control usage.

## Current Validation Status
As of `2026-03-23`:
- `bootstrap.ps1` completed successfully on Windows PowerShell using `uv`
- `test-stage12.ps1` passed with `8` tests
- `smoke-stage12.ps1` completed successfully
- the backend Stage 1/2 GitHub Actions workflow has also passed once on `ubuntu-latest`

## Local Artifact Policy
Local build, test, cache, and smoke-test outputs are redirected into ignored paths:
- `backend/.venv/`
- `backend/.artifacts/`
- `backend/.local/`

These paths should not be committed.
