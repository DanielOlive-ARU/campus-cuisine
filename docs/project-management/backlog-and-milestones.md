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
1. Implement shared order state service
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
7. Review whether the manual backend workflow should be expanded into scoped GitHub Actions later in the project

### Acceptance Criteria
- All MUST requirements demonstrated in repository
- Tests pass locally
- Both students can explain their code and design decisions confidently

## Technical Debt / Hardening Backlog

These items should only be started after the MVP order flow, Shell navigation, and Order Summary editing are stable.

### Introduce `IOrderStateService`
**Priority:** Planned pre-submission hardening after MVP stability
**Owner:** Adam
**Reason:** The frontend currently registers and resolves the concrete `OrderState` singleton through MAUI DI. This satisfies the dependency injection requirement, but an `IOrderStateService` abstraction would make the design cleaner, easier to test, and easier to defend as formal dependency injection.

**Scope:**
1. Add `IOrderStateService`.
2. Make `OrderState` implement `IOrderStateService`.
3. Register `builder.Services.AddSingleton<IOrderStateService, OrderState>();`.
4. Update consumers to depend on `IOrderStateService` where practical.
5. Run full Windows build and order-flow regression tests.

**Do not start until:**
1. MVP order flow is stable.
2. Shell navigation fix is validated.
3. Order Summary editing is stable.

### Completed: refactor category summary bars into reusable `OrderSummaryBar`
**Status:** Completed on `submission-hardening-and-testing`
**Owner:** Adam
**Outcome:** Category-page summary bar duplication has been removed. A reusable `OrderSummaryBar` component now owns the shared totals display and navigation behaviour for Starters, Mains, and Desserts.

**Delivered scope:**
1. Created reusable `OrderSummaryBar` component.
2. Moved count/total/navigation behaviour out of page-specific duplicated XAML.
3. Reused the component on Starters, Mains, and Desserts.
4. Re-ran Windows build, frontend tests, and manual order-flow regression checks.
5. Updated requirements and architecture documentation to reference the reusable component directly.

### Move Order Summary quantity edit buffer into a view model layer
**Priority:** SHOULD after MVP stability
**Owner:** Adam
**Reason:** The current frontend uses a temporary `QuantityText` buffer on `OrderLineDto` so the editable quantity `Entry` in Order Summary can stay in sync without letting invalid values affect totals. This is acceptable for MVP, but it mixes UI editing state into the DTO and is weaker than a cleaner MVVM design.

**Scope:**
1. Introduce a dedicated order-summary view model or order-line view model for editable quantity state.
2. Move `QuantityText` or equivalent UI-buffer behaviour out of `OrderLineDto`.
3. Keep backend payloads unchanged.
4. Re-run Windows build and manual order-flow regression tests.
5. Update documentation to explain the final MVVM structure.

**Do not start until:**
1. MVP order flow is stable.
2. Order Summary editing and validation are stable.
3. Frontend tests are in place or planned closely enough to protect the refactor.

### Expand GitHub Actions into a staged test-and-build pipeline
**Priority:** MAY after all MUST requirements, SHOULD requirements, and likely most MAY work are complete
**Owner:** Shared
**Reason:** A fuller CI/CD pipeline is a worthwhile stretch goal, but it should not compete with completing the assessed application behaviour first. The intended later state is for GitHub Actions to run the automated test suite and, only on passing runs, build assessment-ready Windows and Android artifacts.

**Scope:**
1. Keep backend and frontend automated tests as the first pipeline gate.
2. Add scoped GitHub Actions jobs for frontend/backend validation.
3. Build a Windows runnable artifact only after the test stage passes.
4. Build an Android APK only after the test stage passes.
5. Keep expensive packaging or release publication on manual or tagged workflows.
6. Update README and CI/CD notes with the final workflow structure.

**Do not start until:**
1. Frontend and backend test projects exist and run locally.
2. Current MUST and SHOULD application behaviour is stable on the default branch.
3. Packaging targets for Windows and Android are agreed.

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
