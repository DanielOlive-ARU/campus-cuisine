# Backend Viva Notes

## What did you build on the backend?
I built a FastAPI backend for Campus Cuisine that provides:
- public menu retrieval
- admin CRUD for menu items through OpenAPI
- order creation
- order retrieval with snapshot line items
- minimal order-status tracking
- input validation and protected admin actions

## Why FastAPI, SQLModel, and SQLite?
I chose:
- **FastAPI** for clean REST routing, dependency injection, and automatic OpenAPI docs
- **SQLModel** because it is simple to read and works well for a student project using typed models
- **SQLite** because the brief required menu items to be manageable without code changes, and SQLite was the simplest defendable persistent store

## How does the backend meet the CRUD requirement?
The backend exposes protected admin routes for menu items:
- list
- get by id
- create
- update
- delete

These are available through FastAPI Swagger `/docs`, so food items can be managed without changing code.

## How are orders handled?
The frontend sends a `POST /api/orders` request with item ids and quantities.
The backend:
- validates the payload
- checks menu items exist and are available
- calculates totals on the server
- stores the order and snapshot order lines
- returns a confirmation payload

## Why calculate totals on the server?
Server-side totals are more reliable because:
- the backend is the source of truth
- it avoids trusting client-side calculations
- it protects consistency if prices or items change
- it is easier to test and defend

## How did you implement order status tracking?
Orders are created as `confirmed`.
I added a protected admin endpoint:
- `PATCH /api/admin/orders/{order_id}/status`

For this submission the allowed transition is:
- `confirmed -> cancelled`

Same-status updates are idempotent.
Reverse transitions are rejected.

I kept it minimal because the brief requires status tracking, not a full kitchen workflow system.

## Why are admin routes protected this way?
Admin routes use an API key header:
- `x-admin-key`

I chose that because:
- it is simple
- easy to defend
- appropriate for a student project
- enough to separate public customer actions from protected admin actions without adding full authentication complexity

## How is validation handled?
Validation is handled with Pydantic/SQLModel schemas and service checks.
Examples:
- order item quantity must be greater than zero
- menu item price must be positive
- unknown menu item ids are rejected
- unavailable items are rejected
- extra fields are forbidden on request models

## What testing did you do?
I wrote automated backend tests with `pytest`.

They cover:
- health endpoint
- menu retrieval
- admin menu authentication and CRUD
- order creation and retrieval
- validation failures
- prep-time calculation
- order status update rules

Current backend result:
- `51` tests passing

## What SHOULD/MAY backend enhancements did you add?
I added:
- server-calculated estimated preparation time in order confirmation
- minimal admin order-status tracking
- dynamic relative image paths with static file serving

## Why is prep time calculated on the backend?
Because it belongs to the confirmation contract, like totals.
That keeps:
- one source of truth
- consistent behavior across clients
- easier testing

It is confirmation-only by design and not yet persisted in the database.

## What did you deliberately not build?
I deliberately deferred:
- payments
- user authentication
- analytics
- full order history
- complex kitchen workflow
- real-time status updates

Those would increase scope and risk without improving the core submission as much as finishing MUST requirements cleanly.

## What is the main backend limitation?
The backend status workflow is intentionally minimal.
It supports:
- `confirmed`
- `cancelled`

That is enough to satisfy the requirement clearly, but it is not a full production workflow.

## If you had more time, what would you add next?
I would add, in this order:
1. stronger project evidence and CI polish
2. richer order history
3. more complete order-status workflow
4. offline/frontend caching support
5. stronger admin/auth model if the project needed it

## One-line summary
I built a small, testable FastAPI backend that satisfies the assessment’s core API requirements, keeps the server as the source of truth for order logic, and uses simple, defendable design choices rather than overengineering.
