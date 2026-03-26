# CI/CD Notes

## Current Position
The project still intends to support CI/CD later in development, but automatic GitHub Actions runs are disabled for now to control usage.

## Current Rules
- Keep the backend validation workflow in the repo at `.github/workflows/backend-validation.yml`.
- Allow manual execution through `workflow_dispatch` only.
- Use the local backend testing platform for day-to-day validation.

## Why This Decision Was Taken
- Stage 1/2 backend validation has already passed once in GitHub Actions.
- Ongoing automatic workflow runs are not necessary for every change at this stage.
- The project still needs a clear path back to CI/CD later.
- `workflow_dispatch` cannot be used until the workflow exists on the default branch, so a temporary branch-scoped push trigger was used once to capture the first post-Phase-7 clean-environment run without touching `main`.

## Evidence Already Captured
Backend Stage 1/2 has already been validated in GitHub Actions with:
- dependency installation
- syntax validation
- automated backend tests

This gives the team one clean-environment validation point before switching to local-first development.

The current backend implementation has now also been validated in GitHub Actions after Phase 7 with:
- commit `397beaa`
- workflow `backend-validation`
- `37` passing backend tests
- uploaded artifact `backend-test-results-run-1-attempt-1`

The backend has also been validated locally on Windows PowerShell with:
- `./backend/scripts/bootstrap.ps1`
- `./backend/scripts/test-stage12.ps1`
- `./backend/scripts/smoke-stage12.ps1`

Observed local result:
- `uv` provisioned `CPython 3.12.13`
- dependencies installed successfully
- current backend suite passes locally with menu, admin, and order coverage
- smoke test completed successfully

## Re-enable Plan
Re-enable CI/CD in this order later in the project:
1. backend CI on pull requests only
2. frontend build workflow
3. packaging/release workflow for the two-track submission

## Planned Future Pipelines
### Backend CI
Purpose:
- run backend tests in a clean environment
- validate syntax, dependencies, and milestone-specific tests

### Frontend Build
Purpose:
- confirm the MAUI project still restores and builds
- catch integration regressions early

### Packaging / Release Workflow
Purpose:
- create the runnable submission artifact for assessment
- support the two-track submission strategy

## Two-Track Submission Reminder
The final delivery should still aim for:
- source repository for assessment and code review
- packaged runnable artifact for demonstration on the course leader's machine

## Notes For Later
When CI/CD is expanded again:
- prefer trigger scoping over running on every push
- use manual or tagged release workflows for expensive packaging steps
- keep build artifacts out of the source tree and out of git
