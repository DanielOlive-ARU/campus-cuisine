# Test Plan

This project requires automated testing of core logic and should also demonstrate service/state testing where appropriate.

## Test Strategy
Testing is split into three layers:

1. **Backend unit/API tests** – validate FastAPI business rules and endpoints.
2. **Frontend core logic tests** – validate shared order state, totals, and edge cases.
3. **Integration-style service tests** – validate data loading and mapping between API responses and frontend DTOs.

## Test Projects Structure
```text
backend/
  tests/
FrontEnd/
CampusCuisine.Core/
CampusCuisine.Tests/
```

## Current Backend Validation Status
Backend implementation through Phase 6 has been validated locally:
- locally on Windows PowerShell using the backend script platform
- with direct `pytest` inside the backend virtual environment

Phase 1 and Phase 2 also have one successful clean-runner validation in GitHub Actions.

Current implemented backend tests cover:
- health endpoint success
- startup DB creation
- table creation
- seed-on-empty behaviour
- seed idempotency
- seeded category coverage
- static file mount
- menu listing, filtering, and item lookup
- admin menu authentication and CRUD
- order creation, retrieval, and server-side totals

## Backend Test Scope (Dan)

### Menu Tests
- Return all menu items
- Filter menu by category
- Reject invalid category on admin create/update
- Create menu item successfully
- Update menu item successfully
- Delete menu item successfully
- Reject negative or zero price
- Reject blank name

### Order Tests
- Create order with valid items
- Reject order with empty item list
- Reject order with quantity 0
- Reject order with invalid menu item id
- Calculate total correctly for multiple items
- Aggregate order totals correctly
- Retrieve order by id
- Return order status field

## Frontend Test Scope (Adam)

### Order State Service Tests
- Add first item to order
- Add same item again increases quantity
- Remove item reduces quantity or removes line
- Update quantity directly
- Calculate total item count correctly
- Calculate total price correctly
- Clear order resets state
- Removing an item not present does not crash

### API/Service Tests
- Menu view model maps frontend categories to backend categories correctly
- Menu service parses menu response correctly
- Relative image URLs are converted to absolute URLs
- Absolute image URLs are preserved
- Invalid/network responses are handled gracefully when mocked
- Order submission response maps to confirmation DTO
- API error status codes map to expected frontend exception messages

### Navigation/State Tests
- Order state persists when switching pages
- Summary bar reflects latest totals after quantity changes
- Empty order state shown correctly on summary page

## Current Frontend Validation Status
The frontend now has a dedicated automated test project at `CampusCuisine.Tests` backed by the plain .NET class library `CampusCuisine.Core`.

Current implemented frontend automated tests cover:
- `OrderState` add/remove/set/clear behavior
- total item count and grand total calculation
- `OrderState` `PropertyChanged` notifications for aggregate state updates
- safe handling of missing-line removal and invalid add quantities
- request DTO mapping for placed orders
- `MenuItemViewModel` category mapping and success/failure handling
- `MenuItemViewModel` busy-state re-entry guard
- `ApiService` image URL normalization
- `ApiService` order-confirmation mapping
- `ApiService` HTTP/network error translation

Current validated result:
- `39` frontend tests passing locally with `dotnet test`
- MAUI app build, core library build, and frontend test project build all succeed on the current branch

## Priority Test Cases

| ID | Priority | Test Case | Layer |
|---|---|---|---|
| T01 | MUST | Add item updates quantity and total | Frontend |
| T02 | MUST | Remove item updates order immediately | Frontend |
| T03 | MUST | Total calculation is accurate | Frontend + Backend |
| T04 | MUST | Place order accepts valid payload | Backend |
| T05 | MUST | Invalid order input rejected | Backend |
| T06 | SHOULD | Menu data loads and parses correctly | Frontend service |
| T07 | SHOULD | Shared order state persists across navigation | Frontend |
| T08 | SHOULD | Empty menu dataset handled | Frontend service |
| T09 | SHOULD | Removing absent item handled safely | Frontend |
| T10 | MAY | Scoped or manual CI runs tests in a clean environment | Shared |
| T11 | MUST | Shell flyout navigation returns from Order Summary to the selected primary page | Frontend manual/regression |

## Example Edge Cases
- Decreasing quantity below 1
- Clearing an already empty order
- Loading menu when backend is unavailable
- Submitting order with stale or unavailable menu item
- Image URL missing or broken

## Manual Test Checklist for Demo
- Open app and navigate through flyout
- Start a new order
- Add items from main course page
- Add items from dessert page
- Confirm totals update in summary bar
- Open order summary and edit quantities
- Remove one item and confirm dialog appears
- Clear order and confirm dialog appears
- Add items again and place order
- Verify confirmation with order ID/status

## Manual Regression: Shell Navigation After Order Summary

### Purpose
Verify that Order Summary behaves as a top-level flyout page and does not trap the user on a pushed navigation stack.

### Steps
1. Open Mains.
2. Tap Order Summary.
3. Open the flyout.
4. Tap Mains.
5. Confirm Mains displays.
6. Repeat from Starters.
7. Repeat from Desserts.
8. Open Order Summary from Home using Continue Current Order.
9. From Order Summary, use the flyout to open Home, Starters, Mains, and Desserts.

### Expected Result
The app navigates to each selected flyout page correctly and does not remain stuck on Order Summary.

### Requirement Coverage
- GEN-01: app uses flyout navigation.
- GEN-04: all primary pages are accessible from the flyout.
- GEN-05: order state persists during navigation.
- GEN-02: app remains stable during normal navigation.

## Current CI/CD Position
- Manual GitHub Actions backend validation exists and has passed once for the earlier Phase 1/2 baseline.
- Automatic workflow triggers are currently disabled to control usage.
- The backend workflow is retained as a manual `workflow_dispatch` job.
- Day-to-day backend validation uses the local testing platform.

## Future GitHub Actions Plan
- Re-enable backend CI with scoped triggers later, preferably pull requests only.
- Add frontend/core validation using `CampusCuisine.Tests` when the MAUI build pipeline is ready.
- Add packaging/release workflows later for the assessment deliverable.

## Evidence to Store in Repo
- Test project source code
- README test instructions
- Optional screenshots of passing local test runs
- Manual GitHub Actions workflow file and run evidence

## README Test Commands Placeholder

### Backend
```powershell
./backend/scripts/bootstrap.ps1
./backend/scripts/test-stage12.ps1
./backend/scripts/smoke-stage12.ps1
```

Direct backend test command:
```powershell
./backend/.venv/Scripts/python.exe -m pytest backend/tests -q
```

### Frontend
```powershell
dotnet test ".\CampusCuisine.Tests\CampusCuisine.Tests.csproj" -c Debug
```

Optional targeted frontend test run:
```powershell
dotnet test ".\CampusCuisine.Tests\CampusCuisine.Tests.csproj" -c Debug --filter OrderState
```
