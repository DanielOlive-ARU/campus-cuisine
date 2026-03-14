# Viva / Presentation Outline

Use this outline to structure a 10–15 minute individual presentation.

## 1. Project Overview
- App purpose: mobile restaurant ordering app with flyout navigation
- Stack: .NET MAUI frontend + FastAPI backend
- Team split: Adam frontend, Dan backend

## 2. Requirements Analysis
Explain that the brief was broken into:
- general app requirements
- required pages
- order management requirements
- backend/API requirements
- testing/documentation/collaboration requirements

Show that the **requirements mapping** drove design decisions rather than just listing features.

## 3. Architecture
### Frontend
- MAUI Shell for flyout navigation
- MVVM for separation of concerns
- shared order state service for persistence across pages
- reusable UI components

### Backend
- FastAPI routers, schemas, services, repositories
- SQLite database for menu items and orders
- OpenAPI docs for CRUD operations
- validation with Pydantic

## 4. Key Design Decisions
- Why FastAPI instead of a simpler unstructured backend
- Why SQLite was chosen to allow food items to be added without code changes
- Why shared order state was essential for consistent totals across pages
- Why reusable components reduced duplication between menu pages

## 5. Individual Contribution – Dan
- Designed API contract
- Implemented menu retrieval endpoints
- Implemented admin CRUD for food items
- Implemented order creation and order status endpoints
- Added validation and sanitisation
- Wrote backend tests
- Supported frontend integration with sample responses/payloads

## 6. Individual Contribution – Adam
- Built flyout navigation and core pages
- Implemented XAML layouts and MVVM bindings
- Built reusable DishCard and OrderSummaryBar components
- Implemented shared order state integration
- Styled app and handled user flow
- Wrote frontend/state tests

## 7. Feature Demonstration Flow
Demo order journey in this order:
1. Welcome page
2. Start new order
3. Add main course items
4. Add dessert/appetizer items
5. Open Order Summary
6. Edit quantities/remove item/clear option
7. Place order
8. Show confirmation and backend order status

## 8. Testing
Discuss:
- backend tests for validation, CRUD, order totals
- frontend tests for shared order state and totals
- optional service/integration tests for API responses
- optional GitHub Actions workflow

## 9. Collaboration Evidence
Show:
- GitHub Project board / issues
- commit history from both members
- meeting notes and decision log
- role split with shared integration work

## 10. Ethics and Future Development
### Ethics
- accessibility in UI design
- validation/security for trusted ordering
- avoiding unnecessary personal data collection
- clear confirmation for destructive actions

### Future Work
- order history
- authentication
- promo codes
- payment sandbox
- richer order status updates
- stronger offline support

## Dan’s Likely Viva Questions
### Q: Why did you choose FastAPI?
A: It supports RESTful APIs, strong validation, and automatic OpenAPI docs, which directly matches the brief.

### Q: How do you satisfy “there must be a way to add food items without changing code”?
A: Menu items are stored in SQLite and managed through admin CRUD endpoints, so new dishes can be added through the API rather than hardcoded.

### Q: How is input validation handled?
A: Pydantic schemas validate fields such as category, price, and quantity; invalid requests are rejected before processing.

### Q: How did backend work support the frontend?
A: I agreed endpoint shapes and payload structures early so Adam could build against a stable contract and integrate progressively.
