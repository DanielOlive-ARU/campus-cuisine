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

## Delivered SHOULD/MAY Enhancements

### Completed: add Help page and frontend presentation polish
**Status:** Completed on `submission-hardening-and-testing`
**Owner:** Adam
**Outcome:** The app now includes a Help page in the Shell flyout, a more polished Home page, and a visually distinct Desserts page theme that improves perceived completeness without reopening the core order flow.

**Delivered scope:**
1. Added a Help page with ordering guidance, recovery steps, and safe-use reminders.
2. Added a real flyout navigation entry for Help.
3. Polished the Home page with a stronger hero, current-order stat cards, and quick navigation actions.
4. Added a meaningful dessert-specific visual theme while keeping category-page behaviour unchanged.
5. Re-ran Windows build, frontend tests, and manual navigation/order-flow regression checks.

### Completed: add estimated preparation time to order confirmation
**Status:** Completed on `submission-hardening-and-testing`
**Owner:** Shared
**Outcome:** Order confirmation now includes a lightweight server-calculated preparation estimate that is returned by `POST /api/orders` and displayed in the frontend confirmation alert.

**Delivered scope:**
1. Added a backend prep-time helper and confirmation response field.
2. Kept the estimate confirmation-only and out of the persisted order model for now.
3. Updated backend and frontend tests to cover the new response field.
4. Updated the frontend confirmation alert to display the estimate when present.
5. Re-ran backend validation, Windows build, and frontend tests.

### Completed: add admin order status update endpoint
**Status:** Completed on `submission-hardening-and-testing`
**Owner:** Dan
**Outcome:** Order status tracking is now demonstrably complete. An admin-protected `PATCH /api/admin/orders/{order_id}/status` endpoint allows `confirmed → cancelled` transitions; same-status updates are idempotent; reverse transitions return 400. Closes the MUST for order status tracking with a proper API surface.

**Delivered scope:**
1. Added `routers/admin_orders.py` mirroring the `admin_menu.py` pattern (router-level admin API key dependency).
2. Added `OrderStatusUpdate` request schema and `update_order_status` service helper (pure primitives, single-responsibility).
3. Added six API-level tests (unauthorised, cancel happy path, GET-after-cancel, invalid-reverse, 404, idempotent same-status) plus four service-level unit tests.
4. Updated `api-contract.md`, `backend-status.md`, and `requirements-mapping.md` to document the new endpoint and its intentional confirmation-only asymmetry with `GET /api/orders/{id}`.

### Completed: brand vocabulary and consistent styling
**Status:** Completed on `submission-hardening-and-testing`
**Owner:** Shared
**Outcome:** A shared `Brand*` design vocabulary - colour tokens, chrome styles, typography styles - now lives in `Resources/Styles/` and is consumed by every page. The dark-mode OrderSummary background leak is fixed at the Page style level and the app is explicitly pinned to Light theme so every MAUI template control resolves consistently. Mains, Starters, Order Summary, and Help gained matching page chrome. The Dessert page retains its sanctioned distinct palette expressed as `BrandCard`/`BrandTag` overrides via `BasedOn`, preserving structure and only overriding colour.

**Delivered scope:**
1. Promoted the HomePage palette into shared `Brand*` tokens and default Page background.
2. Replaced the Shell's AppThemeBinding leak by pinning `UserAppTheme = AppTheme.Light` and overriding the default Label text colour to `BrandInk`.
3. Migrated `HomePage` and `DessertsPage` to consume the shared vocabulary; dessert styles extend `Brand*` via `BasedOn`.
4. Added consistent chrome (brand sub-title, body text, error style) to Mains, Starters, Order Summary, and Help; removed the stock MAUI template purple/blue references from Help.
5. Adapted `Shell.ForegroundColor` and `Shell.TitleColor` on app start to the device `PlatformAppTheme`, so the OS title-bar hamburger and page title remain readable on dark-mode devices without un-pinning the app.
6. Restyled the default Button as a warm-tan `BrandAccentMid` fill with dark text; added the `BrandAccentMid` token.
7. Hid the Shell NavBar on Windows only (via `OnPlatform`) so pages render a single branded title; kept NavBar visible on Android so the flyout hamburger stays accessible.
8. Trimmed each hero card to two menu-prose tag chips (Home: `Campus menu` / `Made to order`; Desserts: `Sweet finish` / `Rich flavours`).

### Completed: featured cards on Home
**Status:** Completed on `submission-hardening-and-testing`
**Owner:** Adam
**Outcome:** Home now surfaces menu variety through two featured cards, reducing the need to navigate into category pages to see what's available.

**Delivered scope:**
1. Added "Today's pick" card showing a random main course fetched from the backend menu API.
2. Added "Today's indulgence" card showing a random dessert, styled in the Dessert palette so it visually rhymes with the DessertsPage it navigates to.
3. Implemented a shared pattern: per-session list cache with per-visit random pick re-roll - the cache holds the list, not the chosen item, so randomness is preserved without repeated network calls.
4. Silent graceful degradation - the cards stay hidden if the backend is unreachable or the category is empty.

### Completed: order status display in confirmation
**Status:** Completed on `submission-hardening-and-testing`
**Owner:** Shared
**Outcome:** The order confirmation alert on Order Summary now includes the backend-returned `Status` field, closing the SHOULD for displaying order status updates to the user.

**Delivered scope:**
1. Extended the confirmation alert text to show the status line between Order ID and totals.
2. Guarded against empty status so older or misconfigured backends do not surface a blank line.

### Completed: Place Order button press animation
**Status:** Completed on `submission-hardening-and-testing`
**Owner:** Shared
**Outcome:** Closes the SHOULD for animations / transitions / custom alerts. A short scale-down / scale-up animation on Place Order gives tactile click feedback before the network call, via MAUI's `ScaleToAsync` extension.

### Completed: order-state singleton persistence tests
**Status:** Completed on `submission-hardening-and-testing`
**Owner:** Shared
**Outcome:** Three xUnit tests in `CampusCuisine.Tests/Services/OrderStatePersistenceTests.cs` pin down the DI singleton contract - resolving `IOrderStateService` twice returns the same instance, and mutations are visible after a fresh resolution. Stands in for the manual navigation-persistence regression previously referenced under `TEST-03`.

### Completed: offline menu browsing (SHOULD)
**Status:** Completed on `submission-hardening-and-testing`
**Owner:** Shared
**Outcome:** Closes the last outstanding SHOULD. Menu category responses are transparently cached on every successful fetch and silently re-used when the backend is unreachable, so Starters, Mains, and Desserts render real menu content even on a dropped connection provided at least one successful fetch has happened during the app's lifetime.

**Delivered scope:**
1. Added `IMenuCache` interface in `CampusCuisine.Core/Services/` - single-responsibility: get/save a per-category list.
2. Added `CachedApiService` decorator in `CampusCuisine.Core/Services/` that wraps `IApiService`; successful `GetMenuByCategoryAsync` persists the list, failed calls fall back to the cached list if one exists or re-throw if not.
3. Added `PreferencesMenuCache` in the MAUI project, backed by `Microsoft.Maui.Storage.Preferences` with JSON-serialised per-category keys.
4. Wired DI in `MauiProgram.cs` using the decorator pattern: the raw `ApiService` is registered as its concrete type, `IApiService` resolves to `CachedApiService` wrapping it. No page or view-model changes.
5. Added `CachedApiServiceTests` with five unit tests (happy-path caches, failure-with-cache returns cached, failure-without-cache re-throws, successful fetch overwrites stale cache, `GetMenuItemAsync` passes through without caching).
6. Fallback is deliberately silent - no "offline" banner - so the user experience is indistinguishable from online when cached data is available.
7. `GetMenuItemAsync` and `PostOrderAsync` pass through unchanged; order placement and single-item lookup still require the backend (the server is the price authority).

## Technical Debt / Hardening Backlog

These items should only be started after the MVP order flow, Shell navigation, and Order Summary editing are stable.

### Completed: introduce `IOrderStateService`
**Status:** Completed on `submission-hardening-and-testing`
**Owner:** Adam
**Outcome:** The frontend now uses `IOrderStateService` over the shared `OrderState` singleton, improving DI clarity without changing runtime behaviour.

**Delivered scope:**
1. Added `IOrderStateService`.
2. Made `OrderState` implement `IOrderStateService`.
3. Registered `IOrderStateService` to resolve the same underlying `OrderState` singleton instance.
4. Updated frontend consumers to depend on `IOrderStateService` where practical.
5. Added notification-contract tests for aggregate `PropertyChanged` updates.
6. Re-ran Windows build, core build, frontend tests, and manual order-flow regression checks.

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

### Completed: Move Order Summary quantity edit buffer into a view model layer
**Status:** Completed on `mvvm-ordersummary-refactor` (2026-04-19)
**Owner:** Adam
**Outcome:** The `QuantityText` UI buffer has been lifted off `OrderLineDto` onto a dedicated `OrderSummaryLineViewModel`. A separate `OrderSummaryLineSync` projects `IOrderStateService.Lines` into the page's observable collection via match-by-MenuItemId incremental diff, preserving view-model instance identity so the quantity `Entry` never loses focus during unrelated updates. `OrderLineDto` is now a minimal serialisation contract (`MenuItemId` + `Quantity` only) and the frontend line representation lives on a new `OrderLineEntry` + immutable `MenuItemSnapshot` pair.

**Delivered scope:**
1. Added `OrderSummaryLineViewModel` with INPC and a pure `TryValidateQuantity` helper covering the three existing validation rules (non-numeric / non-positive / greater than 999).
2. Added `OrderSummaryLineSync` with `Dictionary`-backed subscription tracking and idempotent mirror setters.
3. Rewired `OrderSummaryPage.xaml.cs` to bind an `ObservableCollection<OrderSummaryLineViewModel>` and use `TryValidateQuantity` for a thin validation path.
4. Introduced `MenuItemSnapshot` (immutable record) and `OrderLineEntry` (INPC wrapper) and migrated `OrderState` onto them atomically in a single commit.
5. Stripped `OrderLineDto` to `MenuItemId` + `Quantity` only; frontend display state and LineTotal now live on `OrderLineEntry` and its Snapshot.
6. Extracted a pure `OrderConfirmationPresenter` for the place-order confirmation message, pinned to invariant culture for consistent `£` formatting.
7. Extracted `HomePageViewModel` (collapsing the 358-line `HomePage.xaml.cs` to ~60 lines) and `MenuItemCardViewModel` + `MenuItemCardSync` (replacing `MenuItemView`'s wholesale rebuild).
8. Ran `dotnet build` on `net10.0-windows10.0.19041.0` (0 warnings / 0 errors) and `dotnet test` on the full suite (47 existing + 101 new = 148 passing) after every commit. Windows GUI smoke test passed end-to-end.
9. Updated `docs/meetings/decision-log.md`, `docs/reflection/development-reflection.md`, `README.md`, and added `docs/viva/frontend-mvvm-refactor-notes.md`.

Closes Issue 32.

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
