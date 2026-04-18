# Campus Cuisine

Campus Cuisine is a full-stack restaurant ordering application built for a university assessment. The frontend is a .NET MAUI app with flyout navigation, shared order state, and editable order summary flow. The backend is a FastAPI service with OpenAPI-documented menu management, order creation, and basic order status tracking.

The project is designed to satisfy the core MUST requirements from the brief first, then add selected SHOULD and MAY enhancements in a controlled way.

## Team
- Dan: backend lead
- Adam: frontend lead

## Implemented Features
- Flyout navigation with primary customer pages:
  - Home
  - Starters
  - Mains
  - Desserts
  - Order Summary
  - Help
- Shared order state across navigation using a single MAUI-registered order state service
- Backend-driven menu retrieval with dynamic image paths
- Reusable category-page order summary bar
- Editable order summary with:
  - quantity buttons
  - direct quantity entry
  - remove confirmation
  - clear-order confirmation
- Order placement through the backend API
- Order confirmation with server-calculated estimated preparation time
- Protected admin menu CRUD through FastAPI OpenAPI docs
- Protected admin order status update endpoint for `confirmed -> cancelled`
- Automated backend and frontend unit/service tests stored in dedicated test locations

## Technology Stack
- Frontend: .NET MAUI, XAML, C#
- Frontend architecture: pragmatic MVVM with shared DI services
- Backend: FastAPI, SQLModel, SQLite, pydantic-settings, pytest
- API style: REST with OpenAPI docs
- Test stack:
  - frontend: xUnit
  - backend: pytest

## Repository Structure
```text
FrontEnd/                  MAUI application
CampusCuisine.Core/        shared frontend logic and DTOs
CampusCuisine.Tests/       frontend automated tests
backend/                   FastAPI backend
docs/                      requirements, API docs, testing notes, PM evidence, reflection
```

## Prerequisites

### Frontend
- Windows 10/11
- Visual Studio 2026 or later with .NET MAUI support
- .NET 10 SDK
- MAUI workload

Install the MAUI workload if needed:

```powershell
dotnet workload install maui
```

### Backend
- Windows PowerShell
- Python 3.12 or `uv`

The backend local workflow prefers `uv`, but the provided bootstrap script handles local setup for day-to-day work.

## Configuration

### Backend environment
The backend uses:

- `backend/.env.example` as the template
- `backend/.env` as the local configuration file

Key values:

```text
API_PREFIX=/api
ADMIN_API_KEY=change-me
ADMIN_API_KEY_HEADER=x-admin-key
SEED_ON_STARTUP=true
```

Admin routes require the configured `x-admin-key` header value.

### Frontend API base address
The frontend currently expects the backend API at:

- Windows: `http://localhost:8000/`
- Android emulator: `http://10.0.2.2:8000/`

This is configured in `FrontEnd/MauiProgram.cs`.

## Quick Start

Use this order on a Windows development machine:

1. Bootstrap the backend environment:

```powershell
./backend/scripts/bootstrap.ps1
```

2. Start the backend API:

```powershell
./backend/scripts/run-api.ps1 -Port 8000 -SeedOnStartup
```

3. Check the backend is running:

- `http://127.0.0.1:8000/health`
- `http://127.0.0.1:8000/docs`

4. Open `FrontEnd/Campus Cuisine.slnx` in Visual Studio.
5. Run the MAUI app on the Windows target.
6. Place a sample order to confirm the frontend can reach the backend.

## Running The Backend

Run from the repository root in Windows PowerShell:

```powershell
./backend/scripts/bootstrap.ps1
./backend/scripts/run-api.ps1 -Port 8000 -SeedOnStartup
```

What this gives you:
- API base URL: `http://127.0.0.1:8000`
- OpenAPI docs: `http://127.0.0.1:8000/docs`
- Health endpoint: `http://127.0.0.1:8000/health`
- Static images: served from `/images/...`

For local backend validation:

```powershell
./backend/scripts/test-stage12.ps1
./backend/scripts/smoke-stage12.ps1
```

## Running The Frontend

### Preferred approach
Open the solution in Visual Studio:

```text
FrontEnd/Campus Cuisine.slnx
```

Select the Windows target and run the app while the backend API is running locally.

If you test on the Android emulator, the current build uses `10.0.2.2` to reach the backend. Local image loading on Android is still a known limitation.

### CLI build check
From the repository root:

```powershell
dotnet build ".\FrontEnd\Campus Cuisine.csproj" -f net10.0-windows10.0.19041.0 -c Debug
```

The frontend project targets:
- Windows
- Android
- iOS
- MacCatalyst

The primary validated target for this submission is Windows.

## Running Tests

### Backend
From the repository root:

```powershell
./backend/scripts/bootstrap.ps1
./backend/.venv/Scripts/python.exe -m pytest backend/tests -q
```

Current backend suite status:
- `51` tests passing

### Frontend
From the repository root:

```powershell
dotnet test ".\CampusCuisine.Tests\CampusCuisine.Tests.csproj" -c Debug
```

Current frontend suite status:
- `39` tests passing

Optional targeted frontend test run:

```powershell
dotnet test ".\CampusCuisine.Tests\CampusCuisine.Tests.csproj" -c Debug --filter OrderState
```

## API Notes

### Public customer flow
- `GET /api/menu`
- `GET /api/menu/{item_id}`
- `POST /api/orders`
- `GET /api/orders/{order_id}`

### Admin flow
- `GET /api/admin/menu-items`
- `GET /api/admin/menu-items/{item_id}`
- `POST /api/admin/menu-items`
- `PUT /api/admin/menu-items/{item_id}`
- `DELETE /api/admin/menu-items/{item_id}`
- `PATCH /api/admin/orders/{order_id}/status`

Admin routes can be exercised directly through FastAPI Swagger at `/docs` by supplying the configured `x-admin-key` header.

Order status tracking is intentionally minimal in this slice:
- orders are created as `confirmed`
- admins may update them to `cancelled`
- same-status updates are idempotent
- reverse transitions are rejected

## Key Documentation
- Requirements mapping: [docs/requirements/requirements-mapping.md](docs/requirements/requirements-mapping.md)
- API contract: [docs/api/api-contract.md](docs/api/api-contract.md)
- Backend architecture: [docs/api/backend-architecture-sheet.md](docs/api/backend-architecture-sheet.md)
- Backend implementation spec: [docs/api/backend-implementation-spec.md](docs/api/backend-implementation-spec.md)
- Backend status: [docs/api/backend-status.md](docs/api/backend-status.md)
- Local testing platform: [docs/testing/local-testing-platform.md](docs/testing/local-testing-platform.md)
- Test plan: [docs/testing/test-plan.md](docs/testing/test-plan.md)
- Ethics and future development: [docs/ethics-and-future-development.md](docs/ethics-and-future-development.md)
- Project management: [docs/project-management/](docs/project-management/)
- Reflection: [docs/reflection/development-reflection.md](docs/reflection/development-reflection.md)
- Wireframes/design notes: [docs/wireframes/](docs/wireframes/)

## Known Limitations
- Windows is the primary validated frontend target for this submission.
- Android currently has a local image-loading limitation related to local HTTP access; the Windows build is the reference implementation.
- Automatic CI is currently disabled to control usage costs; local validation is the primary development workflow and a manual GitHub Actions backend workflow is retained.
- Offline menu caching is not implemented.
- Real-time order status updates, user accounts, payment integration, analytics, and order history are intentionally out of scope for the submitted MVP.

## Assessment-Oriented Notes
- Core MUST requirements have been prioritised first.
- SHOULD and selected MAY features were added only after the end-to-end flow was stable.
- The repository includes backend code, frontend code, tests, and supporting documentation in one place so the submission can be reviewed directly from GitHub.
