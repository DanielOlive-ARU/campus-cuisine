# Backend Plan

## Active Validation Mode
- Use the local backend testing platform for day-to-day backend validation.
- Keep `.github/workflows/backend-validation.yml` manual-only for on-demand clean-environment checks.

## Phase 0
Documentation lock and Codex handoff pack.

### Acceptance
- Backend docs agree on structure, endpoints, statuses, and payloads.
- Root `docs/` is the only authoritative documentation tree.
- Codex control files are present.

## Phase 1
Scaffold backend package and app entrypoint.

### Acceptance
- `backend/` exists.
- app entrypoint exists.
- `GET /health` works.

## Phase 2
Config, DB engine, startup, static mount, and seed support.

### Acceptance
- settings load from `.env`
- SQLite DB can be created
- tables are created on startup
- static image mount exists
- seed support runs only when the menu table is empty

## Phase 3
Models and schemas.

### Acceptance
- `MenuItem`, `Order`, and `OrderLine` are implemented
- request and response schemas match the API contract
- validation rules are implemented

## Phase 4
Public menu API.

### Acceptance
- `GET /api/menu` works
- `GET /api/menu/{item_id}` works
- category filtering works

## Phase 5
Admin menu CRUD.

### Acceptance
- admin CRUD works through Swagger UI
- `x-admin-key` protects admin routes
- unavailable items are included in admin list endpoints

## Phase 6
Orders.

### Acceptance
- `POST /api/orders` works
- `GET /api/orders/{order_id}` works
- backend calculates totals and snapshots order lines

## Phase 7
Tests and backend docs.

### Acceptance
- pytest suite passes
- backend README includes setup, run, and test steps
- implementation stays aligned with docs
