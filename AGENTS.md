# AGENTS.md

## Project
Campus Cuisine
Frontend: .NET MAUI (.NET 10)
Backend: Python FastAPI

## Ownership
- Dan owns backend
- Adam owns frontend

## Scope Rules
- Work on the backend only unless a shared contract or documentation file must change.
- Do not refactor or rewrite frontend code.
- Do not add payments, full authentication, analytics, or a separate admin UI.
- Do not introduce unnecessary abstraction layers.

## Source Of Truth
- `docs/api/backend-architecture-sheet.md`
- `docs/api/backend-implementation-spec.md`
- `docs/api/api-contract.md`
- `docs/api/backend-plan.md`
- `docs/api/backend-status.md`
- `docs/testing/local-testing-platform.md`
- `docs/project-management/ci-cd-notes.md`
- `docs/requirements/requirements-mapping.md`

## Backend Standards
- Use FastAPI, SQLModel, SQLite, pydantic-settings, and pytest.
- Use framework features over custom infrastructure.
- Keep routers thin.
- Keep services small.
- Return relative image paths.
- Calculate totals on the server.
- Keep the implementation easy for a student team to read and defend.

## Validation
- Run backend validation after each milestone.
- Use the local backend testing platform for day-to-day validation.
- Use the manual GitHub Actions workflow only when a clean-environment check is needed.
- Keep docs aligned with implementation.
- Fix failing tests before moving to the next milestone.

## Boundaries
- Do not touch `FrontEnd/` unless explicitly instructed.
- Do not change public API shapes without updating the docs listed above.
