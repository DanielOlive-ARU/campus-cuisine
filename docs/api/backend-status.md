# Backend Status

## Status
Phase 7 implemented; Phase 1 to Phase 7 validated locally, with Phase 1 and Phase 2 also validated in clean CI; automatic CI is disabled in favor of local testing during active development

## Current Decisions
- Canonical backend folder structure agreed.
- Root `docs/` is the source of truth.
- `FrontEnd/docs/` has been removed as a duplicate documentation tree.
- MVP order status is `confirmed`.
- `image_url` values are relative paths.
- Admin routes use the `x-admin-key` header.
- GitHub Actions validation is retained as a manual workflow, with a temporary branch-scoped `push` trigger on `backend-initial-implementation` until the next clean-environment run is captured.
- Local backend testing uses ignored artifact directories to avoid contaminating the repo.
- `uv` is the preferred local toolchain and can provision Python 3.12 when a normal local Python installation is unavailable.

## Next Task
Push the temporary workflow-trigger update, let the branch-scoped backend validation workflow run, and review the clean-environment result before any further backend changes.

## Progress Log
| Date | Phase | Summary | Validation | Notes |
|---|---|---|---|---|
| 2026-03-23 | 0 | Documentation aligned and Codex handoff files added | Manual review | Ready for implementation review |
| 2026-03-23 | 1-2 | Backend scaffold, health route, config, DB startup, static mount, and seed support added | `python3 -m py_compile $(find backend -type f -name '*.py' | sort)` | Runtime validation blocked in this shell because `pip` and `ensurepip` are unavailable |
| 2026-03-23 | 1-2 validation | Added startup, DB, seed, and static mount tests plus a backend-only GitHub Actions workflow | Local syntax validation only | Full runtime validation now depends on the first GitHub Actions run |
| 2026-03-23 | 1-2 CI | GitHub Actions installed dependencies and ran backend Stage 1-2 tests successfully | `pytest backend/tests -q --junitxml=backend-test-results.xml` | 8 tests passed on `ubuntu-latest` |
| 2026-03-23 | local platform | Automatic CI disabled to control usage; local testing platform and CI/CD notes added | Documentation and scripts | Manual GitHub Actions workflow retained for on-demand validation |
| 2026-03-23 | 1-2 local validation | Windows PowerShell local validation completed successfully using the new backend scripts | `./backend/scripts/bootstrap.ps1`, `./backend/scripts/test-stage12.ps1`, `./backend/scripts/smoke-stage12.ps1` | `uv` provisioned CPython 3.12.13, pytest passed with 8 tests, smoke test passed |
| 2026-03-26 | validation fix | Repaired the local PowerShell validation wrapper under strict mode and removed the smoke-test parsing prompt | `powershell.exe -NoProfile -ExecutionPolicy Bypass -File backend/scripts/test-stage12.ps1`, `powershell.exe -NoProfile -ExecutionPolicy Bypass -File backend/scripts/smoke-stage12.ps1` | Local script-based validation is working again and Phase 3 can proceed |
| 2026-03-26 | 3 | Implemented Phase 3 schemas and validation rules for menu and orders | `powershell.exe -NoProfile -ExecutionPolicy Bypass -File backend/scripts/test-stage12.ps1` | Request/response schemas now match the API contract and schema validation tests pass locally |
| 2026-03-26 | 4-6 | Implemented public menu routes, protected admin menu CRUD, and order endpoints | `backend/.venv/Scripts/python.exe -m pytest backend/tests -q`, `powershell.exe -NoProfile -ExecutionPolicy Bypass -File backend/scripts/test-stage12.ps1`, `powershell.exe -NoProfile -ExecutionPolicy Bypass -File backend/scripts/smoke-stage12.ps1` | Local backend suite now passes with 37 tests; scripted validation and smoke testing both succeed |
| 2026-03-26 | 7 | Hardened workflow/script naming and aligned backend docs with the implemented API state | `backend/.venv/Scripts/python.exe -m pytest backend/tests -q`, `powershell.exe -NoProfile -ExecutionPolicy Bypass -File backend/scripts/test-stage12.ps1`, `powershell.exe -NoProfile -ExecutionPolicy Bypass -File backend/scripts/smoke-stage12.ps1` | Backend validation naming is now generic, local artifacts use backend-wide names, and docs point to the manual validation workflow |
