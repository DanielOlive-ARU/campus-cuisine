# Frontend Backend Integration Notes

This note is for Adam to consume the backend from the MAUI frontend.

Current state:
- the initial backend implementation is complete on branch `backend-initial-implementation`
- the backend PR has not necessarily been opened or merged yet
- the source of truth remains:
  - `docs/api/api-contract.md`
  - `docs/api/backend-status.md`
  - `docs/requirements/requirements-mapping.md`

## 1. Start the backend locally

Run these commands from the repository root in Windows PowerShell:

```powershell
./backend/scripts/bootstrap.ps1
./backend/scripts/test-stage12.ps1
./backend/scripts/smoke-stage12.ps1
./backend/scripts/run-api.ps1 -Port 8000 -SeedOnStartup
```

What each does:
- `bootstrap.ps1`: creates the local backend environment and installs dependencies
- `test-stage12.ps1`: runs backend syntax validation and the full backend pytest suite
- `smoke-stage12.ps1`: verifies health, DB creation, seed behaviour, and static file serving
- `run-api.ps1`: starts the API locally for frontend integration

When the API is running, Swagger/OpenAPI is available at:

```text
http://127.0.0.1:8000/docs
```

Health check:

```text
GET http://127.0.0.1:8000/health
```

## 2. Base URL guidance

For Windows local development on the same machine:

```text
http://localhost:8000
```

Important:
- backend `image_url` values are relative paths such as `/images/grilled-chicken-burger.jpg`
- the frontend must prepend the backend base URL before binding an image source

Example:

```text
http://localhost:8000/images/grilled-chicken-burger.jpg
```

If Android testing is needed later:
- emulator/device base URL handling should be configured in the frontend service layer
- do not hardcode the base URL separately in each page

## 3. Page to endpoint mapping

Use these endpoints for the current page structure:

### Starters page
```text
GET /api/menu?category=appetizer
```

### Mains page
```text
GET /api/menu?category=main
```

### Desserts page
```text
GET /api/menu?category=dessert
```

Optional:
- `available_only=true` is the default
- `GET /api/menu/{item_id}` is available if the frontend later needs single-item detail lookups

## 4. DTOs to implement in MAUI

### MenuItemDto
```json
{
  "id": 1,
  "name": "Grilled Chicken Burger",
  "description": "Chargrilled chicken fillet with lettuce and house sauce.",
  "category": "main",
  "price": 8.99,
  "image_url": "/images/grilled-chicken-burger.jpg",
  "is_available": true
}
```

### CreateOrderRequestDto
```json
{
  "items": [
    {
      "menu_item_id": 1,
      "quantity": 2
    }
  ]
}
```

### OrderConfirmationDto
```json
{
  "id": 101,
  "status": "confirmed",
  "total_items": 3,
  "grand_total": 21.97,
  "message": "Order placed successfully"
}
```

### OrderReadDto
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
    }
  ]
}
```

Implementation notes:
- keep `price`, `unit_price`, `line_total`, and `grand_total` numeric in code
- format currency in the UI, not in the DTOs
- current backend order status is only `confirmed`

## 5. Order flow

Recommended frontend flow:

1. load category pages with `GET /api/menu`
2. keep current order in a shared frontend state service
3. show local item count and local total in the summary bar
4. when the user taps **Place Order**, send:

```text
POST /api/orders
```

5. use the backend response as the authoritative result:
- `id`
- `status`
- `total_items`
- `grand_total`
- `message`

6. if full order detail is needed after checkout, call:

```text
GET /api/orders/{order_id}
```

Important:
- frontend totals are display-only until checkout
- backend totals are the final truth

## 6. Error handling expectations

The frontend should handle:
- network/server unavailable
- `400` invalid business request
- `404` missing item or order
- `422` validation error

Error shape:

```json
{
  "detail": "Validation error message here"
}
```

## 7. Admin scope

Admin endpoints exist, but they are not needed in the MAUI customer app.

Admin routes:
- require `x-admin-key`
- are intended for Swagger/OpenAPI usage at `/docs`

## 8. Current frontend rework required

Before integration, the frontend should replace:
- local mock menu data
- the current `MenuItemModel` shape
- page-local `new MenuItemViewModel()` usage

The frontend still needs:
- an API service layer
- shared order state service
- Order Summary page and route
- real image binding using `baseUrl + image_url`

## 9. Quick smoke checklist for Adam

1. Start the backend with `run-api.ps1`
2. Open `http://localhost:8000/health`
3. Open `http://localhost:8000/docs`
4. Confirm `GET /api/menu?category=main` returns menu items
5. Confirm a browser can load one image URL if image assets have been added
6. Wire one MAUI page to live backend data before refactoring all pages

