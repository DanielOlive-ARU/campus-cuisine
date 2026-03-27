# API Contract Draft

This is the agreed contract between the MAUI frontend and the FastAPI backend.

## Base URL
```text
http://localhost:8000
```

## Content Type
All requests and responses use:
```http
Content-Type: application/json
```

## Health Endpoint

### GET /health
Returns API health status.

#### Example Response
```json
{
  "status": "ok"
}
```

## Public Endpoints

### GET /api/menu
Returns available menu items by default.

#### Query Parameters
| Name | Type | Required | Notes |
|---|---|---|---|
| category | string | No | `main`, `dessert`, or `appetizer` |
| available_only | bool | No | defaults to `true` |

#### Example Response
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

### GET /api/menu/{item_id}
Returns one menu item by id.

#### Success Response
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

### POST /api/orders
Creates a new order.

#### Request Body
```json
{
  "items": [
    {
      "menu_item_id": 1,
      "quantity": 2
    },
    {
      "menu_item_id": 7,
      "quantity": 1
    }
  ]
}
```

#### Success Response
```json
{
  "id": 101,
  "status": "confirmed",
  "total_items": 3,
  "grand_total": 21.97,
  "message": "Order placed successfully"
}
```

### GET /api/orders/{order_id}
Returns a full order and current status.

#### Example Response
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

## Admin Authentication
Admin routes require the `x-admin-key` header.
Public menu and order routes do not require authentication.

## Admin Endpoints
These endpoints satisfy the requirement that food items can be managed without changing code.

### GET /api/admin/menu-items
Returns all menu items including unavailable items.

### GET /api/admin/menu-items/{item_id}
Returns one menu item for admin inspection/editing.

### POST /api/admin/menu-items
Creates a menu item.

#### Request Body
```json
{
  "name": "Classic Cheesecake",
  "description": "Creamy vanilla cheesecake served chilled.",
  "category": "dessert",
  "price": 4.5,
  "image_url": "/images/classic-cheesecake.jpg",
  "is_available": true
}
```

### PUT /api/admin/menu-items/{item_id}
Updates a menu item.

### DELETE /api/admin/menu-items/{item_id}
Deletes a menu item.

## Validation Rules
- `name`: required, trimmed, max length 100
- `description`: required, trimmed, max length 500
- `category`: one of `main`, `dessert`, `appetizer`
- `price`: decimal > 0
- `quantity`: integer > 0
- `image_url`: valid backend-served relative image path such as `/images/example.jpg`

## Implemented Backend Status Values
Implemented now:
- `confirmed`

Future extension:
- `cancelled`

## Frontend DTOs

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

## Error Response Shape
```json
{
  "detail": "Validation error message here"
}
```

## Integration Notes
- Frontend should treat menu responses as read-only DTOs.
- Order state should be managed locally until the user taps **Place Order**.
- The backend should calculate authoritative totals rather than trusting client totals.
- `image_url` values are relative paths; the app should prepend the backend base URL when loading images.
