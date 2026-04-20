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

## Frontend Test Scope

The frontend test surface was authored end-to-end by Dan, in two waves: the initial test project structure with 47 baseline tests for `OrderState`, `ApiService`, and `MenuItemViewModel` (committed 2026-04-17 alongside the `CampusCuisine.Core` / `CampusCuisine.Tests` split), and the post-merge view-model / sync / command suite added during the MVVM refactor that took the suite from 47 to 210 tests across 24 commits. The groupings below describe what each test suite covers, not who wrote it.

### Service-layer tests
- `OrderState` add/decrease/set/clear behavior, total item count, grand total
- `OrderState` `PropertyChanged` notifications for aggregate state updates
- Safe handling of missing-line removal and invalid (zero / negative) add quantities
- Request DTO mapping (`ToCreateOrderRequest` produces the minimal `OrderLineDto` shape)
- `OrderState` singleton persistence under DI resolution (proxy for cross-page navigation persistence)
- `ApiService` image URL normalization (relative -> absolute, absolute preserved)
- `ApiService` order-confirmation mapping
- `ApiService` HTTP / network error translation (`ApiException` with status-aware messages)
- `CachedApiService` decorator: success caches, failure falls back to cache, no-cache re-throws, single-item passthrough

### View-model tests
- `MenuItemViewModel` category mapping, success / failure handling, busy-state re-entry guard
- `MenuItemCardViewModel` ctor-from-`MenuItemModel`, INPC, `HasQuantity` / `QuantityText`, `AddCommand` / `DecreaseCommand` (with and without an injected service)
- `OrderSummaryLineViewModel` quantity / `QuantityText` normalisation, `LineTotal` math, `UpdateFrom`, `TryValidateQuantity`, per-line `IncreaseCommand` / `DecreaseCommand` / `RemoveCommand` paths
- `OrderSummaryPageViewModel` ctor / Attach / Detach lifecycle, `ClearOrderCommand` and `PlaceOrderCommand` paths covering confirm-accept, confirm-cancel, empty cart, happy path, null response, network exception, and `IsPlacingOrder` toggling
- `HomePageViewModel` defaults, featured / indulgence loaders, error paths, cache reuse, retry-after-failure, `OrderState` subscription updates, `StartNewOrderCommand` / `ContinueOrderCommand` / `NavigateToCommand` across empty / confirm-accept / confirm-cancel / invalid-parameter

### Sync / projection tests
- `OrderSummaryLineSync` seed, Add, Remove, `SetQuantity` (instance identity preserved via match-by-id), Clear, Dispose, AddRemoveAdd same id
- `MenuItemCardSync` seed, items Add / Remove / Reset, cart Add / Remove / Set / Clear, orphaned ids, Dispose

### Command and presenter tests
- `RelayCommand` and `AsyncRelayCommand` parameter passing, `CanExecute` gating, reentry guard, exception resilience, null-guard constructors
- `OrderConfirmationPresenter` full message, missing status, missing prep, whitespace status

### Model tests
- `MenuItemSnapshot` record value equality on each field
- `OrderLineEntry` ctor, `Quantity` / `Snapshot` INPC, delegated property reads, `LineTotal` math

## Current Frontend Validation Status
The frontend has a dedicated automated test project at `CampusCuisine.Tests` backed by the platform-neutral class library `CampusCuisine.Core` (no MAUI references). The test project uses xUnit and runs under `dotnet test` without spinning up a MAUI runtime.

Current validated result:
- **`210` frontend tests passing locally** with `dotnet test ".\CampusCuisine.Tests\CampusCuisine.Tests.csproj" -c Debug`
- MAUI app build, core library build, and frontend test project build all succeed on `net10.0-windows10.0.19041.0` with 0 warnings / 0 errors
- The MVVM refactor branch grew the suite from `47` to `210` tests across 24 commits; every commit on the refactor branch was green at both `dotnet build` and `dotnet test`

Test doubles live alongside the tests in `CampusCuisine.Tests/TestDoubles/`:
- `FakeApiService`, `FakeHttpMessageHandler`, `FakeMenuCache` for the service layer
- `FakeDialogService`, `FakeNavigationService` for the view-model commands

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
