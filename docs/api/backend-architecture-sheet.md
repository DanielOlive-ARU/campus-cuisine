# Backend Architecture Sheet

## Project Context
This backend supports a mobile restaurant ordering app built in **.NET MAUI**. The backend is responsible for:
- serving menu data to the app
- allowing food items to be managed without code changes
- accepting customer orders
- tracking order status
- serving dish images
- validating and sanitising input

The design aims to minimise custom code by using framework features wherever possible.

---

## Recommended Backend Stack

### Core Stack
- **Python 3.12+**
- **FastAPI** for REST API endpoints and OpenAPI/Swagger docs
- **SQLModel** for models + ORM-style database access
- **SQLite** for lightweight persistent storage
- **Uvicorn** as the ASGI server
- **Pydantic Settings** for configuration
- **pytest** + **FastAPI TestClient** for testing

### Why this stack
This combination reduces coding effort because:
- FastAPI automatically generates API documentation
- request/response validation is built into FastAPI + Pydantic
- SQLModel reduces duplication between database and API models
- SQLite avoids external database setup
- Swagger UI can act as the admin CRUD interface required by the brief

---

## Architectural Goals
1. Meet every backend **MUST** requirement from the brief.
2. Keep the codebase small and testable.
3. Use framework features instead of writing custom infrastructure.
4. Support easy integration with Adam’s MAUI frontend.
5. Keep the API stable and simple for the viva.

---

## Backend Responsibilities Mapped to the Brief

| Requirement Area | Backend Response |
|---|---|
| RESTful menu retrieval | `GET /api/menu` and `GET /api/menu/{id}` |
| CRUD for food items | Admin endpoints under `/api/admin/menu-items` |
| OpenAPI documented interface/webpage | FastAPI Swagger UI and ReDoc |
| Order status tracking | `POST /api/orders` and `GET /api/orders/{id}` |
| Input validation and sanitisation | Pydantic/SQLModel validation + custom validators |
| Add food items without code changes | Menu items stored in SQLite and edited via CRUD API |
| Dynamic dish images | Static image serving with image URLs returned in API |
| Testing | pytest suite for business logic and endpoints |

---

## High-Level Architecture

```text
MAUI App
   |
   | HTTP/JSON
   v
FastAPI Application
   |
   +-- Routers
   |     +-- menu.py
   |     +-- admin.py
   |     +-- orders.py
   |
   +-- Services
   |     +-- order_service.py
   |
   +-- DB Layer
   |     +-- SQLModel models
   |     +-- session dependency
   |     +-- SQLite database
   |
   +-- Core
   |     +-- config.py
   |     +-- security.py
   |
   +-- Static Files
         +-- /static/images
```

---

## Suggested Folder Structure

```text
backend/
  app/
    main.py
    core/
      config.py
      security.py
    db/
      models.py
      session.py
      seed.py
    routers/
      menu.py
      admin.py
      orders.py
    services/
      order_service.py
    static/
      images/
    tests/
      test_menu.py
      test_admin.py
      test_orders.py
      test_order_service.py
```

### Folder Roles
- **main.py**: app startup, router registration, static files, lifespan hooks
- **core/config.py**: environment variables and settings
- **core/security.py**: simple admin API key dependency
- **db/models.py**: SQLModel entities and shared enums
- **db/session.py**: SQLite engine and session dependency
- **db/seed.py**: starter menu data
- **routers/**: route definitions only
- **services/**: non-trivial business logic, especially order total calculation
- **static/images/**: dish image files served to the frontend
- **tests/**: backend automated tests

---

## Data Model Design

### MenuItem
Represents a food item available to customers.

| Field | Type | Notes |
|---|---|---|
| id | int | primary key |
| name | str | required, trimmed |
| description | str | required |
| category | str/enum | `main`, `dessert`, `appetizer` |
| price | decimal/float | must be greater than 0 |
| image_url | str | backend-served path or full URL |
| is_available | bool | supports hiding items without deleting |

### Order
Represents a placed customer order.

| Field | Type | Notes |
|---|---|---|
| id | int | primary key |
| status | str/enum | `pending`, `confirmed`, `cancelled` |
| created_at | datetime | generated server-side |
| total_items | int | calculated server-side |
| grand_total | decimal/float | calculated server-side |

### OrderLine
Represents one menu item within an order.

| Field | Type | Notes |
|---|---|---|
| id | int | primary key |
| order_id | int | foreign key |
| menu_item_id | int | original menu item id |
| name_snapshot | str | copy of item name at order time |
| unit_price_snapshot | decimal/float | copy of item price at order time |
| quantity | int | must be > 0 |
| line_total | decimal/float | calculated server-side |

### Why snapshot fields matter
Order lines should keep a copy of the item name and price at the time the order was placed. That prevents old orders changing when a menu item is edited later.

---

## API Design

## Public Endpoints

### `GET /api/menu`
Returns all available menu items.

Optional query parameters:
- `category`
- `available_only`

### `GET /api/menu/{item_id}`
Returns one menu item by id.

### `POST /api/orders`
Accepts an order payload from the frontend and creates a new order.

Request shape:
```json
{
  "items": [
    { "menu_item_id": 1, "quantity": 2 },
    { "menu_item_id": 7, "quantity": 1 }
  ]
}
```

Response shape:
```json
{
  "id": 101,
  "status": "confirmed",
  "total_items": 3,
  "grand_total": 21.97,
  "message": "Order placed successfully"
}
```

### `GET /api/orders/{order_id}`
Returns order details and current status.

## Admin Endpoints

### `GET /api/admin/menu-items`
Returns all menu items including unavailable items.

### `POST /api/admin/menu-items`
Creates a menu item.

### `PUT /api/admin/menu-items/{item_id}`
Updates a menu item.

### `DELETE /api/admin/menu-items/{item_id}`
Deletes a menu item.

---

## Validation and Sanitisation Strategy

Use framework validation first, then add minimal custom rules.

### Validation rules
- `name` must not be blank
- `description` must not be blank
- `category` must be one of the allowed values
- `price` must be greater than 0
- `quantity` must be greater than 0
- order must contain at least one line item
- referenced menu item ids must exist

### Sanitisation rules
- trim whitespace on text fields
- set max lengths for names/descriptions
- reject obviously invalid or malformed data
- optionally validate image URL/path format

### Principle
The backend should always calculate the authoritative total, not trust totals sent by the frontend.

---

## Authentication / Access Control

The brief says role-based access control for admin actions is a **SHOULD**, not a MUST.

### Minimal recommended approach
Use a simple API key dependency for admin routes only.

Example approach:
- public routes are open
- admin routes require `X-API-Key` header
- API key is stored in `.env`

This keeps the code small while still giving a strong professional explanation in the viva.

---

## Static Image Strategy

Use backend-served static files.

### Approach
- store images in `app/static/images/`
- mount static files in FastAPI
- each menu item stores an `image_url`
- frontend uses that URL directly in MAUI image controls

### Why this is good
- easy to implement
- satisfies dynamic image requirement
- avoids building a separate media service

---

## Dependency Injection Plan

FastAPI has built-in dependency injection. Use it rather than creating services manually.

### Planned dependencies
- **database session dependency**
- **settings dependency**
- **admin API key dependency**

### Benefits
- less boilerplate
- easy testing with dependency overrides
- aligns with the course requirement for DI

---

## Business Logic Placement

Keep routers thin.

### Put in routers
- request parsing
- dependency injection
- returning HTTP responses

### Put in services
- order total calculation
- order line snapshot creation
- status assignment rules
- shared logic reused by multiple endpoints

### Put in models/schemas
- data structure definitions
- field constraints
- validation rules

This gives a clean separation without adding unnecessary abstraction.

---

## Database Choice

### Recommended database: SQLite
Use SQLite for this coursework because:
- zero external server setup
- easy local development
- persistent storage for menu and orders
- enough for CRUD and order tracking
- easy to explain in the viva

### Why not a larger DBMS
PostgreSQL would be more scalable, but it adds setup and deployment complexity without helping much for this assignment.

---

## Testing Strategy

### Core backend tests
1. menu retrieval returns expected items
2. category filtering works
3. admin create/update/delete works
4. invalid menu item input is rejected
5. valid order creates correct totals
6. invalid order payload is rejected
7. order status can be retrieved
8. removing/changing menu data does not corrupt existing orders

### Recommended tools
- `pytest`
- `httpx` / `TestClient`
- temporary SQLite database for tests

### Minimum evidence for the brief
At minimum, show tests for:
- adding items to an order
- removing items / invalid removals
- accurate total calculations
- validation failures

---

## Build Order

### Phase 1 — Foundations
1. create FastAPI project
2. create settings/config
3. create SQLite engine and SQLModel base
4. create models
5. seed starter menu data

### Phase 2 — Menu API
6. implement `GET /api/menu`
7. implement `GET /api/menu/{id}`
8. implement admin CRUD endpoints
9. mount static image serving

### Phase 3 — Orders
10. implement order service logic
11. implement `POST /api/orders`
12. implement `GET /api/orders/{id}`
13. add status values and confirmations

### Phase 4 — Hardening
14. add API key protection for admin routes
15. improve validation/sanitisation
16. add tests
17. finalise README setup/run instructions

---

## Integration Contract with Frontend

### Agreed rules
- frontend requests menu data from backend
- frontend holds current order in shared MAUI state until checkout
- backend recalculates final totals when order is placed
- backend returns the authoritative order id, totals, and status
- image URLs must be valid for Android and Windows access

### Important integration note
The backend should never depend on frontend-specific UI behaviour. It should expose stable JSON contracts that Adam can consume regardless of platform differences.

---

## Error Handling Approach

Keep error responses simple and consistent.

### Examples
- 400 Bad Request for invalid input
- 404 Not Found for missing items/orders
- 401 Unauthorized for admin requests without valid API key
- 422 Unprocessable Entity for schema validation failures handled by FastAPI

### Response style
Use clear error messages so the frontend can display understandable feedback.

---

## What Not to Build

To keep the backend efficient, avoid these unless the full core flow is complete:
- full user authentication system
- payment processing
- separate admin frontend
- analytics dashboard
- microservices
- complex repository pattern
- cloud deployment complexity unless specifically required

---

## Viva Talking Points for Dan

### Why FastAPI?
Because it provides REST support, automatic OpenAPI documentation, validation, and dependency injection with very little code.

### Why SQLite?
Because it satisfies the persistence requirement and supports editable menu items without adding infrastructure complexity.

### How does this satisfy “add food items without updating code”?
Food items are stored in SQLite and managed through admin CRUD endpoints shown in Swagger UI.

### How is data integrity protected?
Pydantic/SQLModel validation checks fields such as category, quantity, and price, and the backend recalculates all totals.

### Why are order totals calculated on the server?
To keep placed orders authoritative and prevent mismatches or tampering.

---

## Recommended Next Task for Dan

Implement this exact sequence:
1. `main.py`
2. `config.py`
3. `session.py`
4. `models.py`
5. `seed.py`
6. `menu.py`
7. `admin.py`
8. `order_service.py`
9. `orders.py`
10. backend tests

That sequence gets a working API online quickly and allows Adam to integrate early.
