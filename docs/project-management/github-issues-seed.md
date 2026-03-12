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
**Labels:** frontend, must
**Owner:** Adam
**Done when:** component displays item count, total, and navigates to summary page.

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

### Issue 29: Add GitHub Actions test workflow
**Labels:** testing, may
**Owner:** Shared
**Done when:** at least one test workflow runs on push or pull request.

### Issue 30: Final README and viva prep
**Labels:** documentation, must
**Owner:** Shared
**Done when:** setup/run/test instructions and speaking notes are complete.
