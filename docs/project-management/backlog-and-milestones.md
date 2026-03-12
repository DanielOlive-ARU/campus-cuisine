# Backlog and Milestones

This backlog is structured so that all **MUST** requirements are completed before the viva. SHOULD features are only attempted after the end-to-end flow is stable.

## Suggested GitHub Project Columns
- Backlog
- To Do
- In Progress
- In Review
- Done

## Labels
- `frontend`
- `backend`
- `documentation`
- `testing`
- `bug`
- `must`
- `should`
- `may`

## Milestone 1 – Planning and Setup
**Target:** Week 1

### Tasks
1. Create repository structure
2. Create GitHub Project board and labels
3. Write requirements mapping
4. Produce wireframes for core pages
5. Agree API contract and sample payloads
6. Agree coding conventions and branching strategy

### Acceptance Criteria
- Repo exists and both members have pushed commits
- Requirements mapping stored in `/docs/requirements`
- Wireframes stored in `/docs/wireframes`
- API contract stored in `/docs/api`
- GitHub board screenshot stored in `/docs/project-management`

## Milestone 2 – Core Foundations
**Target:** Week 2

### Backend Tasks (Dan)
1. Create FastAPI project structure
2. Create SQLite schema for menu items and orders
3. Implement `GET /api/menu`
4. Implement category filtering
5. Implement admin CRUD skeleton for menu items
6. Seed starter menu data and image URLs

### Frontend Tasks (Adam)
1. Create MAUI Shell with flyout navigation
2. Create Welcome page
3. Create page shells for Main Course, Dessert, and Order Summary
4. Set up MVVM structure and dependency injection
5. Create reusable `DishCard` component skeleton
6. Create `OrderSummaryBar` component skeleton

### Acceptance Criteria
- App navigates between all core pages
- Backend returns menu data successfully
- Frontend can display static or mock menu data in the agreed layout

## Milestone 3 – Shared Order Flow
**Target:** Week 3

### Backend Tasks (Dan)
1. Implement `POST /api/orders`
2. Implement `GET /api/orders/{order_id}`
3. Add order status field and default status rules
4. Add validation and sanitisation
5. Protect admin endpoints with basic auth/API key if implemented

### Frontend Tasks (Adam)
1. Implement shared `IOrderStateService`
2. Connect Main Course page to live menu API
3. Connect Dessert page to live menu API
4. Implement add/increase/decrease/remove actions
5. Bind order summary bar to shared order totals

### Acceptance Criteria
- User can add items from multiple pages into one shared order
- Quantities and totals update correctly
- Backend accepts order submission payload format

## Milestone 4 – Checkout and Integration
**Target:** Week 4

### Shared Tasks
1. Complete Order Summary page
2. Implement confirmation dialogs for remove/clear actions
3. Implement place order flow and confirmation display
4. Handle empty order state
5. Complete dynamic image loading from backend
6. End-to-end integration testing

### Acceptance Criteria
- End-to-end journey works: welcome → browse → summary → place order
- Order confirmation returns order ID and status
- App remains stable during navigation

## Milestone 5 – Testing, Documentation, Viva Preparation
**Target:** Week 5

### Shared Tasks
1. Write/complete unit tests
2. Add API/service/state tests
3. Finalise README setup/run/test sections
4. Export project board evidence
5. Add meeting notes and reflection
6. Prepare viva walkthrough and speaking roles
7. Add GitHub Actions if time allows

### Acceptance Criteria
- All MUST requirements demonstrated in repository
- Tests pass locally
- Both students can explain their code and design decisions confidently

## Task Allocation Summary

| Area | Dan | Adam | Shared |
|---|---|---|---|
| Backend API | Lead | Support in testing | Review contract |
| Database / persistence | Lead | - | Review fields |
| MAUI navigation / XAML | - | Lead | Review UX |
| Shared order logic | Support data model | Lead | Test together |
| API integration | Lead backend support | Lead frontend integration | Joint debugging |
| Documentation | Backend docs | Frontend docs / wireframes | README / reflection |
| Testing | Backend unit/API tests | Frontend/service/state tests | integration verification |
| Viva prep | Backend architecture | Frontend UX/flow | collaboration and reflection |

## Definition of Done
A task is only moved to **Done** when:
1. It is implemented.
2. It has been reviewed by the other team member.
3. It does not break the app.
4. It is linked to the relevant requirement(s).
5. Any relevant tests or documentation have been added.
