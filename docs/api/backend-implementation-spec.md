# Backend Implementation Spec

## Purpose
Build the backend for a restaurant flyout menu mobile app using FastAPI, SQLModel, SQLite, and pytest. The backend must satisfy the coursework requirements for:
- RESTful menu retrieval
- CRUD for food items through an OpenAPI-documented interface
- order status tracking
- validation and sanitisation
- dynamic image URLs
- a simple admin protection mechanism

The implementation should prefer framework features over custom infrastructure and keep the code coursework-appropriate, readable, and testable.

## Non-goals
Do not implement:
- full authentication or user accounts
- payments
- separate admin web frontend
- analytics dashboards
- complex kitchen workflows
- websockets
- deployment hardening beyond local coursework needs

## Tech Stack
- Python 3.12+
- FastAPI
- SQLModel
- SQLite
- Uvicorn
- Pydantic v2
- pydantic-settings
- pytest
- FastAPI TestClient

## Project Layout
```text
backend/
  app/
    __init__.py
    main.py
    core/
      __init__.py
      config.py
      security.py
    db/
      __init__.py
      engine.py
      init_db.py
      seed.py
    models/
      __init__.py
      menu_item.py
      order.py
    schemas/
      __init__.py
      menu.py
      order.py
    routers/
      __init__.py
      health.py
      menu.py
      admin_menu.py
      orders.py
    services/
      __init__.py
      menu_service.py
      order_service.py
    static/
      images/
  tests/
    __init__.py
    conftest.py
    test_health.py
    test_menu_api.py
    test_admin_menu_api.py
    test_orders_api.py
    test_validation.py
  requirements.txt
  .env.example
  README.md
```

## Design Rules
1. Use APIRouter modules, not one giant file.
2. Use FastAPI dependencies for DB session, settings, and admin key validation.
3. Use SQLModel models for persistence and separate request/response schemas where needed.
4. Keep business logic in small services only when it is reused or non-trivial.
5. Let FastAPI/OpenAPI provide the admin CRUD UI at `/docs`.
6. Use lifespan startup to initialise database tables and optionally seed starter data.
7. Mount static files for image serving.
8. Keep the implementation synchronous unless a clear need for async appears.

## Configuration
Create `app/core/config.py`.

### Settings class fields
- `app_name: str = "Campus Kitchen API"`
- `app_version: str = "0.1.0"`
- `api_prefix: str = "/api"`
- `database_url: str = "sqlite:///./campus_kitchen.db"`
- `admin_api_key: str = "change-me"`
- `admin_api_key_header: str = "x-admin-key"`
- `seed_on_startup: bool = True`
- `static_mount_path: str = "/images"`
- `static_dir: str = "app/static/images"`
- `cors_origins: list[str] = ["*"]` for development only

### Requirements
- Use `BaseSettings` from `pydantic-settings`
- Load from `.env`
- Provide `get_settings()` with `@lru_cache`

## Database
Create `app/db/engine.py`.

### Engine
- Build engine with `create_engine(settings.database_url, connect_args={"check_same_thread": False} if sqlite else {})`
- Export `engine`

### Session dependency
Create `get_session()` as a FastAPI dependency using `yield`:
- open `Session(engine)`
- `yield session`
- cleanup handled by the context manager

## DB Initialisation
Create `app/db/init_db.py` with:
- `create_db_and_tables()` calling `SQLModel.metadata.create_all(engine)`

Create `app/db/seed.py` with:
- `seed_menu_items(session: Session)`
- only seed if no menu items exist
- insert about 12 items total
- each item uses an image path under `/images/...`

Use FastAPI lifespan in `main.py` to:
- create tables on startup
- optionally seed starter data

## Domain Model
Use a small number of persistence models.

### `models/menu_item.py`
Define `MenuCategory` values:
- `main`
- `dessert`
- `appetizer`

Define `MenuItem` table model:
- `id: int | None = Field(default=None, primary_key=True)`
- `name: str = Field(index=True, max_length=100)`
- `description: str = Field(max_length=500)`
- `category: str = Field(index=True, max_length=20)`
- `price: Decimal | float`
- `image_url: str = Field(max_length=255)`
- `is_available: bool = Field(default=True, index=True)`
- `created_at: datetime`
- `updated_at: datetime`

Implementation note:
- use `Decimal` if it can be implemented cleanly with SQLModel and SQLite
- otherwise use `float` and keep rounding behaviour consistent in responses and tests

### `models/order.py`
Define MVP `OrderStatus` values:
- `confirmed`

Future extension only:
- `cancelled`

Define `Order` table model:
- `id: int | None = Field(default=None, primary_key=True)`
- `status: str = Field(default="confirmed", index=True)`
- `total_items: int`
- `grand_total: float`
- `created_at: datetime`

Define `OrderLine` table model:
- `id: int | None = Field(default=None, primary_key=True)`
- `order_id: int = Field(foreign_key="order.id", index=True)`
- `menu_item_id: int = Field(index=True)`
- `name_snapshot: str`
- `unit_price_snapshot: float`
- `quantity: int`
- `line_total: float`

### Relationship handling
Keep relationships simple:
- `Order` has many `OrderLine`
- response assembly may query lines separately if that is simpler

## Schemas
Use dedicated schemas for request and response payloads.

### `schemas/menu.py`
Create:
- `MenuItemRead`
- `MenuItemCreate`
- `MenuItemUpdate`

`MenuItemRead` fields:
- id, name, description, category, price, image_url, is_available

`MenuItemCreate` and `MenuItemUpdate` validation:
- trim whitespace for strings
- `name` required, length 1-100
- `description` required, length 1-500
- `category` must be one of allowed values
- `price` must be `> 0`
- `image_url` required and length <= 255
- `is_available` defaults true
- extra fields forbidden on request models

### `schemas/order.py`
Create:
- `OrderItemCreate`
- `CreateOrderRequest`
- `OrderLineRead`
- `OrderRead`
- `OrderConfirmation`

`OrderItemCreate`:
- `menu_item_id: int`
- `quantity: int`
- validator: quantity > 0

`CreateOrderRequest`:
- `items: list[OrderItemCreate]`
- validator: list must not be empty

`OrderLineRead`:
- `menu_item_id`
- `name`
- `unit_price`
- `quantity`
- `line_total`

`OrderRead`:
- `id`
- `status`
- `total_items`
- `grand_total`
- `created_at`
- `items`

`OrderConfirmation`:
- `id`
- `status`
- `total_items`
- `grand_total`
- `message`

## Sanitisation and Validation Rules
Implement sanitisation centrally in schema validators and service functions.

### Menu sanitisation
- strip leading and trailing whitespace from `name`, `description`, and `image_url`
- collapse repeated spaces in `name` if needed
- reject blank strings after trimming
- reject category outside the allowed set
- reject non-positive price
- reject unreasonably long strings

### Order validation
- reject empty orders
- reject quantity <= 0
- reject unknown menu item ids
- reject unavailable menu items
- calculate totals on the server from current DB values
- never trust client totals

### Error policy
Return:
- `400` for invalid business requests where appropriate
- `401` for missing or invalid admin API key
- `404` when menu item or order is not found
- `422` for schema validation errors handled by FastAPI/Pydantic

## Security
Create `app/core/security.py`.

Use FastAPI `APIKeyHeader` for admin endpoints only.
- Header name comes from settings and defaults to `x-admin-key`
- compare provided key with the configured value
- raise `HTTPException(status_code=401)` on failure

## Services
Keep services small.

### `services/menu_service.py`
Functions:
- `list_menu_items(session, category: str | None = None, available_only: bool = True) -> list[MenuItem]`
- `get_menu_item_or_404(session, item_id: int) -> MenuItem`
- `create_menu_item(session, payload: MenuItemCreate) -> MenuItem`
- `update_menu_item(session, item_id: int, payload: MenuItemUpdate) -> MenuItem`
- `delete_menu_item(session, item_id: int) -> None`

Rules:
- for update, patch only supplied fields
- update `updated_at`
- raise 404 when not found

### `services/order_service.py`
Functions:
- `create_order(session, payload: CreateOrderRequest) -> Order`
- `get_order_with_lines(session, order_id: int) -> tuple[Order, list[OrderLine]]`

Rules for `create_order`:
1. fetch all referenced menu items
2. ensure all exist and are available
3. compute `line_total = unit_price * quantity`
4. compute `total_items = sum(quantity)`
5. compute `grand_total = sum(line_total)`
6. create the `Order`
7. create `OrderLine` snapshots using the current item name and price
8. commit once
9. refresh and return

## Routers
All routers use `prefix=settings.api_prefix` at app level or at router level where appropriate.

### `routers/health.py`
Endpoints:
- `GET /health`

Response:
```json
{ "status": "ok" }
```

### `routers/menu.py`
Public endpoints.

Endpoints:
- `GET /api/menu`
  - query: `category: str | None = None`
  - query: `available_only: bool = True`
  - returns list of `MenuItemRead`
- `GET /api/menu/{item_id}`
  - returns one `MenuItemRead`

Requirements:
- allow only valid category filters if provided
- default to only available items

### `routers/admin_menu.py`
Protected with admin API key dependency.

Endpoints:
- `GET /api/admin/menu-items`
- `GET /api/admin/menu-items/{item_id}`
- `POST /api/admin/menu-items`
- `PUT /api/admin/menu-items/{item_id}`
- `DELETE /api/admin/menu-items/{item_id}`

Requirements:
- include clear summaries and descriptions for OpenAPI docs
- CRUD should be usable directly from `/docs`

### `routers/orders.py`
Public endpoints.

Endpoints:
- `POST /api/orders`
  - body: `CreateOrderRequest`
  - returns `OrderConfirmation`
- `GET /api/orders/{order_id}`
  - returns `OrderRead`

## Main Application
Create `app/main.py`.

Responsibilities:
- instantiate FastAPI with title and version
- add CORS middleware for development
- mount static image directory at `/images`
- register lifespan handler
- include routers
- ensure docs are enabled

Suggested tags:
- `health`
- `menu`
- `admin-menu`
- `orders`

## Static Images Strategy
Store sample images under:
- `app/static/images/`

Seeded `image_url` values should point to relative paths such as:
- `/images/grilled-chicken-burger.jpg`
- `/images/chocolate-brownie.jpg`

The frontend can prepend the backend base URL.

## Sample Seed Menu
Seed about 12 items.

### Main
- Grilled Chicken Burger
- Beef Lasagne
- Margherita Pizza
- BBQ Chicken Wrap
- Veggie Pasta Bake
- Fish and Chips

### Dessert/Appetizer
- Chocolate Brownie
- Classic Cheesecake
- Garlic Bread
- Onion Rings
- Ice Cream Sundae
- Apple Pie

Each item should have a realistic description, positive price, relative image path, and `is_available=True`.

## Response Examples
### `GET /api/menu?category=main`
```json
[
  {
    "id": 1,
    "name": "Grilled Chicken Burger",
    "description": "Chargrilled chicken fillet with lettuce and house sauce.",
    "category": "main",
    "price": 8.99,
    "image_url": "/images/grilled-chicken-burger.jpg",
    "is_available": true
  }
]
```

### `POST /api/orders`
Request:
```json
{
  "items": [
    {"menu_item_id": 1, "quantity": 2},
    {"menu_item_id": 7, "quantity": 1}
  ]
}
```

Response:
```json
{
  "id": 101,
  "status": "confirmed",
  "total_items": 3,
  "grand_total": 21.97,
  "message": "Order placed successfully"
}
```

### `GET /api/orders/101`
```json
{
  "id": 101,
  "status": "confirmed",
  "total_items": 3,
  "grand_total": 21.97,
  "created_at": "2026-03-23T12:00:00Z",
  "items": [
    {
      "menu_item_id": 1,
      "name": "Grilled Chicken Burger",
      "unit_price": 8.99,
      "quantity": 2,
      "line_total": 17.98
    },
    {
      "menu_item_id": 7,
      "name": "Chocolate Brownie",
      "unit_price": 3.99,
      "quantity": 1,
      "line_total": 3.99
    }
  ]
}
```

## OpenAPI Requirements
The `/docs` page must clearly expose:
- public menu retrieval
- public order creation and retrieval
- admin CRUD for menu items
- admin API key authentication scheme

## Error Model
Use a consistent error body where practical:
```json
{ "detail": "Menu item not found" }
```

Accept FastAPI's default validation error shape for 422 responses.

## Testing Strategy
Use pytest with FastAPI `TestClient`.

Current baseline:
- Phase 1 and Phase 2 already include health, startup, DB creation, seed, and static mount tests.
- Later phases expand coverage to menu, admin, validation, and orders.

### `tests/conftest.py`
Create:
- temporary SQLite test DB
- test engine
- dependency override for the app session dependency
- seeded test data fixture

### Minimum tests
- health endpoint success
- get all menu items
- filter menu by category
- get single menu item
- 404 for unknown item
- admin CRUD requires API key
- create admin item success
- reject invalid admin payloads
- create order success
- create order calculates `total_items` and `grand_total`
- get order by id success
- reject empty order
- reject quantity 0
- reject unknown menu item id
- reject unavailable item

## Implementation Sequence
1. Phase 1: scaffold backend package and app entrypoint
2. Phase 2: config, DB engine, startup, static mount, and seed
3. Phase 3: models and schemas
4. Phase 4: public menu routes
5. Phase 5: admin CRUD routes
6. Phase 6: orders
7. Phase 7: expand tests and finalise backend README
