# GitHub Issues Seed List

Copy these into GitHub Issues and assign them to the relevant team member.

## Epic 1 – Planning and Documentation

### Issue 1: Create repository structure and initial solution layout
**Labels:** documentation, must
**Owner:** Shared
**Description:** Create the base folder structure for frontend, backend, tests, and docs.
**Done when:** folders, README placeholder, and initial commit exist.

### Issue 2: Produce requirements mapping document
**Labels:** documentation, must
**Owner:** Shared
**Description:** Translate assignment brief into a tracked requirements matrix.
**Done when:** every MUST requirement has a mapped design response and owner.

### Issue 3: Create wireframes for core pages
**Labels:** documentation, must, frontend
**Owner:** Adam
**Description:** Produce low-fidelity wireframes for Welcome, Main Course, Dessert, and Order Summary pages.
**Done when:** images or notes are uploaded to `/docs/wireframes`.

### Issue 4: Define API contract and DTOs
**Labels:** backend, must, documentation
**Owner:** Dan
**Description:** Agree endpoint list, payload shapes, and sample JSON between frontend and backend.
**Done when:** API contract document is approved by both members.

## Epic 2 – Backend

### Issue 5: Set up FastAPI project and routing structure
**Labels:** backend, must
**Owner:** Dan
**Done when:** app runs locally and routers are separated by feature.

### Issue 6: Create SQLite schema and seed menu data
**Labels:** backend, must
**Owner:** Dan
**Done when:** menu items can be stored and retrieved from persistence.

### Issue 7: Implement public menu retrieval endpoints
**Labels:** backend, must
**Owner:** Dan
**Done when:** `/api/menu` supports category filtering and returns valid data.

### Issue 8: Implement admin CRUD for menu items
**Labels:** backend, must
**Owner:** Dan
**Done when:** create, read, update, delete endpoints appear in OpenAPI docs and work locally.

### Issue 9: Implement order creation and status retrieval endpoints
**Labels:** backend, must
**Owner:** Dan
**Done when:** orders can be submitted and retrieved by ID with a status field.

### Issue 10: Add backend validation and sanitisation
**Labels:** backend, must
**Owner:** Dan
**Done when:** invalid prices, quantities, and categories are rejected.

### Issue 11: Protect admin endpoints with basic token/API key
**Labels:** backend, should
**Owner:** Dan
**Done when:** admin endpoints reject unauthorised requests.

### Issue 12: Write backend tests for CRUD and order logic
**Labels:** backend, testing, must
**Owner:** Dan
**Done when:** backend tests run locally and cover core logic.

## Epic 3 – Frontend

### Issue 13: Set up MAUI Shell flyout navigation
**Labels:** frontend, must
**Owner:** Adam
**Done when:** all four primary pages are in the flyout.

### Issue 14: Build Welcome page
**Labels:** frontend, must
**Owner:** Adam
**Done when:** page displays restaurant name, intro, order status, and CTA buttons.

### Issue 15: Build reusable DishCard component
**Labels:** frontend, must
**Owner:** Adam
**Done when:** component displays image, name, description, price, and quantity actions.

### Issue 16: Build reusable OrderSummaryBar component
**Labels:** frontend, must, architecture
**Owner:** Adam
**Description:** Refactor the currently duplicated category-page summary bar UI into a reusable `OrderSummaryBar` component.
**Status:** Completed on `submission-hardening-and-testing`
**Done when:** the component displays item count and total, navigates to `OrderSummaryPage`, and is reused by Starters, Mains, and Desserts without breaking existing behaviour.

### Issue 17: Build Main Course page and bind to menu data
**Labels:** frontend, must
**Owner:** Adam
**Done when:** mains load from API and user can update quantities.

### Issue 18: Build Dessert/Appetizer page and bind to menu data
**Labels:** frontend, must
**Owner:** Adam
**Done when:** desserts load from API and share the same order state.

### Issue 19: Implement shared order state service
**Labels:** frontend, must
**Owner:** Adam
**Done when:** items persist across page navigation and totals stay correct.

### Issue 20: Build Order Summary page
**Labels:** frontend, must
**Owner:** Adam
**Done when:** page supports edit/remove/clear/place order and empty state handling.

### Issue 21: Add styling, theme, and reusable resources
**Labels:** frontend, should
**Owner:** Adam
**Status:** Completed on `submission-hardening-and-testing`
**Done when:** consistent branding and dessert page theme differences are visible.

### Issue 22: Add offline menu cache
**Labels:** frontend, should
**Owner:** Adam
**Done when:** last loaded menu can be displayed without network.

### Issue 23: Write frontend tests for order state and data loading
**Labels:** frontend, testing, must
**Owner:** Adam
**Done when:** unit tests cover add/remove/update/total/state cases.

## Epic 4 – Integration and QA

### Issue 24: Integrate frontend with live backend endpoints
**Labels:** must
**Owner:** Shared
**Done when:** app uses real API data for both categories and order placement.

### Issue 25: Implement dynamic image loading from backend
**Labels:** must
**Owner:** Shared
**Done when:** dish images render using backend-supplied URLs.

### Issue 26: Add confirmation flows for remove and clear actions
**Labels:** must
**Owner:** Adam
**Done when:** destructive actions require confirmation.

### Issue 27: Test state persistence across navigation
**Labels:** testing, should
**Owner:** Shared
**Done when:** evidence shows same order is preserved across pages.

### Issue 28: Prepare screenshots and evidence for docs
**Labels:** documentation, must
**Owner:** Shared
**Done when:** screenshots are placed into docs folders for final submission.

### Issue 29: Expand GitHub Actions into a staged test-and-build pipeline
**Labels:** testing, may
**Owner:** Shared
**Description:** Expand the current manual-only CI position into a staged GitHub Actions pipeline once the assessed application behaviour is substantially complete. The later target is for backend/frontend tests to run first and for passing runs to be able to build a Windows runnable artifact and Android APK through scoped workflows.
**Done when:** backend and frontend tests run in scoped workflows, passing runs can trigger Windows and Android artifact builds, and expensive packaging steps remain manual or tag-based rather than running on every change.

### Issue 30: Final README and viva prep
**Labels:** documentation, must
**Owner:** Shared
**Done when:** setup/run/test instructions and speaking notes are complete.

### Issue 31: Harden frontend order-state DI with interface abstraction
**Labels:** frontend, should, architecture, testing
**Owner:** Adam
**Description:** Introduce `IOrderStateService` so pages/viewmodels can depend on an abstraction rather than the concrete `OrderState` class.
**Reason:** Current MAUI DI registration is valid for the brief, but interface-based DI improves testability, separation of concerns, and viva defensibility.
**Status:** Completed on `submission-hardening-and-testing`
**Done when:** `OrderState` implements `IOrderStateService`, DI registers the interface, consumers use the abstraction where practical, and Windows build/manual order-flow regression passes.

### Issue 32: Move Order Summary quantity edit buffer into a view model
**Labels:** frontend, should, architecture, testing
**Owner:** Adam
**Description:** Remove the temporary `QuantityText` UI buffer from `OrderLineDto` and move editable quantity state into a dedicated order-summary or order-line view model.
**Reason:** The current `QuantityText` buffer is a pragmatic MVP fix that keeps the quantity `Entry` in sync and prevents invalid values from mutating totals. It does not break the current solution, but a dedicated view-model layer would better align the frontend with a stricter MVVM design.
**Done when:** editable quantity state no longer lives on `OrderLineDto`, Order Summary quantity editing still works, backend payload shape is unchanged, and Windows build/manual order-flow regression passes.

### Issue 33: Add Help page to the customer app
**Labels:** frontend, should
**Owner:** Adam
**Status:** Completed on `submission-hardening-and-testing`
**Description:** Add a static Help page to the MAUI Shell flyout so the app includes an additional primary-style page with guidance on ordering and recovery flows.
**Done when:** Help appears in the flyout, the page renders cleanly, and existing order navigation remains unaffected.

### Issue 34: Add estimated preparation time to order confirmation
**Labels:** shared, may, backend, frontend, documentation
**Owner:** Shared
**Status:** Completed on `submission-hardening-and-testing`
**Description:** Return a lightweight server-calculated prep-time estimate in the `POST /api/orders` confirmation payload and show it in the frontend confirmation alert.
**Done when:** backend confirmation includes `estimated_prep_minutes`, frontend displays it, tests pass, and docs explain that the estimate is confirmation-only in this slice.
