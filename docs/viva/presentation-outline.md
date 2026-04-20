# Viva / Presentation Outline

Use this outline to structure a 10–15 minute individual presentation.

## 1. Project Overview
- App purpose: mobile restaurant ordering app with flyout navigation
- Stack: .NET MAUI frontend + FastAPI backend
- Team split: Adam owned the original frontend foundation, the wireframe, the Azure DevOps board snapshots, and a first-pass presentation deck. Dan owned the backend end-to-end, the entire automated test suite (backend pytest + frontend xUnit including the test-project structure and the test doubles), and the post-merge frontend hardening (reusable summary bar, `IOrderStateService`, brand vocabulary, offline cache, animations, prep time, admin status endpoint, and the MVVM refactor). The contribution split is verifiable in the GitHub commit history.

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

### Backend (sole owner)
- Designed the REST API contract (`docs/api/api-contract.md`)
- Implemented FastAPI service with SQLModel + SQLite: public menu retrieval, admin menu CRUD via OpenAPI, order creation with server-calculated totals and prep time, and the `PATCH /api/admin/orders/{id}/status` admin endpoint for `confirmed -> cancelled` transitions
- Implemented input validation and sanitisation (Pydantic schemas with `extra = "forbid"`, positive prices, positive quantities, non-blank names, known menu-item ids)
- Implemented the admin `x-admin-key` router-level dependency
- Set up the manual GitHub Actions backend validation workflow and captured a clean Phase 7 run

### Testing (sole owner across the project)
- Backend: 51 pytest tests covering health endpoint, seed behaviour, menu listing / filtering / lookup, admin authentication and CRUD, order creation with totals, prep-time calculation, and order status transitions
- Frontend: 210 xUnit tests across the service layer, view-models, sync projections, commands, presenter, and models (full breakdown in [docs/testing/test-plan.md](../testing/test-plan.md))
- Built the test infrastructure: the `CampusCuisine.Core` / `CampusCuisine.Tests` project split that allows view-models to be unit-tested without spinning up MAUI, plus the test doubles (`FakeApiService`, `FakeHttpMessageHandler`, `FakeMenuCache`, `FakeDialogService`, `FakeNavigationService`)
- Authored every test file in both waves: the initial 47-test baseline (`OrderState`, `ApiService`, `MenuItemViewModel` — committed 2026-04-17) and the post-MVVM-refactor +163 tests across the new view-model / sync / command surface

### Frontend hardening and post-merge MVVM refactor
- Refactored the duplicated category-page summary bar into the reusable `OrderSummaryBar` component
- Introduced `IOrderStateService` over the shared `OrderState` singleton, with order-state singleton persistence tests that pin the DI contract
- Introduced the Campus Cuisine brand vocabulary (`Brand*` colour tokens, chrome and typography styles) and pinned the app to Light theme to fix dark-mode control bleed
- Built the Help page and the distinct Desserts palette
- Built the Today's pick / Today's indulgence featured cards on Home
- Built the offline menu browsing layer (`CachedApiService` decorator + `PreferencesMenuCache`)
- Built the Place Order press animation (`ScaleToAsync`)
- Built the estimated prep time display in the order confirmation alert
- Delivered the post-merge MVVM refactor on `mvvm-ordersummary-refactor` (24 green-building commits, five phases): `OrderLineEntry` + `MenuItemSnapshot`, `OrderSummaryLineViewModel` + `OrderSummaryLineSync`, `HomePageViewModel`, `MenuItemCardViewModel` + `MenuItemCardSync`, `OrderSummaryPageViewModel`, hand-rolled `RelayCommand` / `AsyncRelayCommand`, `IDialogService` / `INavigationService` abstractions with MAUI-side implementations, and constructor-injection on every Shell-declared page

### Documentation
- `docs/viva/backend-viva-notes.md`, `docs/viva/frontend-mvvm-refactor-notes.md`
- `docs/meetings/decision-log.md` entries for DI review, Order Summary Shell navigation fix, MVVM refactor phases
- `docs/reflection/development-reflection.md`
- `docs/ethics-and-future-development.md` rewrite
- README polish, requirements-mapping refresh, .gitattributes line-ending policy

## 6. Individual Contribution – Adam

### Original frontend foundation
The frontend foundation Adam delivered between 14 March and 12 April established the architecture that the post-merge hardening and MVVM refactor were built on top of.

- Created the MAUI frontend project on .NET 10 (`Add Maui frontend project`, 14 March)
- Migrated deprecated MAUI controls for .NET 10 compatibility (`Frame` -> `Border`, namespace cleanup, `MainPage` deprecation, AppShell + `CreateWindow` rework)
- Built the unified `MenuItemView` component with the menu-item injection model used by every category page (Mains, Starters, Desserts)
- Built the API integration on the category pages (the `MenuItemViewModel` async loader and the page-level wiring)
- Built the original shared `OrderState` service: snapshot ownership pattern, the `Add` / `Decrease` / `Set` / `Clear` operations, and the `[JsonIgnore]` snapshot fields that later evolved into `MenuItemSnapshot`
- Built the original Order Summary page and checkout flow
- Built the menu quantity controls (`+` / `-` buttons on each menu card)
- Made the API base URL platform-aware (Windows vs Android emulator)
- Built the original page-level summary bar on each category page (later refactored into the shared `OrderSummaryBar` component)
- Surfaced backend-down errors on category pages
- Built the Start New Order / Continue Current Order buttons on the Home page
- Added navigation from the Home page Start Order button into the menu
- Repo hygiene: stopping environment-specific files from being tracked (`.csproj.user`, build artifacts)

### Project-management and presentation evidence
- Created the Initial Wireframe (`docs/wireframes/Initial Wireframe.jpg`)
- Created the Weekly meetings document (`docs/meetings/Weekly meetings.docx`)
- Captured the Azure DevOps board snapshots for Weeks 1-5 (`docs/project-management/week-by-week boards/`)
- Authored the first-pass Campus Cuisine presentation deck. The current working copy of the deck is maintained off-repo to allow continued slide work without conflicting with the repo submission state.

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
- Azure DevOps weekly board snapshots
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

## Q&A Reference Material

For deep backend Q&A during viva preparation see [backend-viva-notes.md](backend-viva-notes.md) - 15 questions covering tech-stack rationale, the CRUD interface, order handling, server-side totals, status workflow, admin auth, validation, testing, prep time, and deferred scope.

For deep frontend / MVVM Q&A see [frontend-mvvm-refactor-notes.md](frontend-mvvm-refactor-notes.md) - architectural reasoning behind the post-merge view-model layer, the sync projections, immutable snapshots, the command pattern, and the three remaining MAUI-ergonomics trade-offs left in code-behind.
