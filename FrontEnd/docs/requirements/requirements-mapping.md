# Requirements Mapping

This file translates the assignment brief into implementable requirements and links each requirement to a design response, owner, and expected evidence.

## Priority Key
- **MUST** = required for baseline pass / satisfactory implementation
- **SHOULD** = high-value enhancement for stronger marks
- **MAY** = optional if time allows

## Functional Requirements Map

| ID | Priority | Requirement | Design / Implementation Response | Owner | Evidence in Repo |
|---|---|---|---|---|---|
| GEN-01 | MUST | App uses flyout navigation | MAUI Shell with flyout containing all primary pages | Adam | `AppShell.xaml`, screenshots |
| GEN-02 | MUST | App runs without crashing | Stable navigation, validation, error handling | Shared | test evidence, demo video/screenshots |
| GEN-03 | MUST | At least four primary pages | Welcome, Main Course, Dessert/Appetizer, Order Summary | Adam | `/Views`, wireframes |
| GEN-04 | MUST | All primary pages accessible from flyout | Shell flyout items for each main page | Adam | `AppShell.xaml` |
| GEN-05 | MUST | Persistent order state during navigation | Shared `IOrderStateService` singleton injected into ViewModels | Adam | service code, state tests |
| GEN-06 | MUST | Support creating and managing customer orders | Frontend add/update/remove/clear order + backend order endpoint | Shared | service code, API tests |
| GEN-07 | MUST | User can place an order | Order Summary page submits order to backend and shows confirmation | Shared | integration demo |
| GEN-08 | SHOULD | Consistent branding | Shared styles, colours, typography resource dictionaries | Adam | `/Styles` |
| GEN-09 | MAY | View past orders | Optional order history page backed by saved orders | Shared | optional feature evidence |
| GEN-10 | MAY | Authentication or guest checkout | Defer unless all core features complete | Shared | backlog decision log |
| PAGE-01 | MUST | Welcome page included | Separate landing page with restaurant intro and CTAs | Adam | `WelcomePage.xaml` |
| PAGE-02 | MUST | Main Course page included | Category page loads mains from backend | Adam | `MainCoursePage.xaml` |
| PAGE-03 | MUST | Dessert/Appetizer page included | Category page loads desserts/appetizers from backend | Adam | `DessertPage.xaml` |
| PAGE-04 | MUST | Order Summary page included | Editable order summary and checkout action | Adam | `OrderSummaryPage.xaml` |
| PAGE-05 | SHOULD | Additional page such as Help/Profile/History | Add Help page if time allows | Adam | optional page |
| HOME-01 | MUST | Welcome page displays restaurant name | Static heading bound or configured in app constants | Adam | wireframe + view |
| HOME-02 | MUST | Welcome page shows description/introduction | Short intro paragraph on landing page | Adam | wireframe + view |
| HOME-03 | MUST | Welcome page shows current order status | Summary tile bound to order state service | Adam | view + VM |
| HOME-04 | MUST | Start New Order button | Clears order after confirmation and navigates to menu | Adam | view logic |
| HOME-05 | MUST | Continue Current Order button when order exists | Conditional visibility based on item count > 0 | Adam | VM/UI behaviour |
| HOME-06 | SHOULD | Professional restaurant-style interface | Hero banner, clean cards, consistent spacing | Adam | screenshots |
| HOME-07 | MAY | Promotions/featured dishes | Optional featured section loaded from backend flags | Shared | optional feature |
| MAIN-01 | MUST | List of main course dishes | GET menu by category = main | Shared | API contract + page |
| MAIN-02 | MUST | Name, description, price shown | Dish card component shows required fields | Adam | `DishCard` |
| MAIN-03 | MUST | Add item option | Add button on each dish card | Adam | component + service |
| MAIN-04 | MUST | Increase/decrease quantity | Plus/minus controls update shared state | Adam | component + tests |
| MAIN-05 | MUST | Removing item updates immediately | Order state notifies UI instantly | Adam | state tests |
| MAIN-06 | MUST | Order summary bar displayed | Reusable bottom summary bar component | Adam | `OrderSummaryBar` |
| MAIN-07 | MUST | Summary bar shows total items + total price | Bound to shared order totals | Adam | component |
| MAIN-08 | MUST | Summary bar navigates to Order Summary page | Tapping bar routes to summary page | Adam | component + shell routing |
| MAIN-09 | SHOULD | Images loaded dynamically from backend | Image URLs returned by API and displayed in app | Shared | API sample + page screenshots |
| DESS-01 | MUST | Dessert page displays desserts | GET menu by category = dessert/appetizer | Shared | page + API |
| DESS-02 | MUST | Name, description, price shown | Reuse DishCard component | Adam | component |
| DESS-03 | MUST | Added to same shared order | Same order state service across all pages | Adam | state tests |
| DESS-04 | MUST | Multiple quantity supported | Quantity buttons and aggregation logic | Adam | service tests |
| DESS-05 | MUST | Summary bar displayed | Same reusable component | Adam | component |
| DESS-06 | MUST | Summary bar shows items + total | Shared state binding | Adam | component |
| DESS-07 | MUST | Summary bar navigates to summary page | Reuse same action as main course page | Adam | component |
| DESS-08 | SHOULD | Distinct colour theme | Different accent styling for desserts page | Adam | styles |
| DESS-09 | SHOULD | Images from backend | Same image strategy as mains | Shared | screenshots |
| SUM-01 | MUST | Display all current order items | Collection view bound to order items | Adam | page + VM |
| SUM-02 | MUST | Show quantity, item price, line total | Editable line items with totals | Adam | page |
| SUM-03 | MUST | Show grand total | Bound computed order total | Adam | page |
| SUM-04 | MUST | Remove/change quantities | Editable controls on summary page | Adam | page + tests |
| SUM-05 | MUST | Confirm before removing item | Confirmation dialog | Adam | interaction evidence |
| SUM-06 | MUST | Clear Order option | Button with confirmation dialog | Adam | page |
| SUM-07 | MUST | Confirm before clearing order | Alert/dialog before action | Adam | interaction evidence |
| SUM-08 | MUST | Place Order button | Submit current order to backend | Shared | integration evidence |
| SUM-09 | MUST | Generate order confirmation | Response shows order ID/status | Shared | API response + UI |
| SUM-10 | MUST | Handle empty order state | Empty state panel and disabled checkout | Adam | page |
| SUM-11 | MUST | Allow direct quantity editing | Stepper/buttons/text entry with validation | Adam | page |
| ORD-01 | MUST | Multiple quantities supported | Shared order service aggregates by item id | Adam | unit tests |
| ORD-02 | MUST | Accurate totals | Business logic covered by tests | Shared | unit tests |
| ORD-03 | MUST | Consistent order across pages | Singleton state service + VM bindings | Adam | navigation/state tests |
| ORD-04 | SHOULD | Order cancellation before checkout | Cancel action resets pending order before submission | Adam | optional feature |
| ORD-05 | MAY | Reorder from history | Deferred unless optional order history complete | Shared | backlog |
| API-01 | MUST | RESTful menu retrieval | `/api/menu` endpoints returning menu data | Dan | FastAPI routers |
| API-02 | MUST | CRUD for food items via OpenAPI webpage | Admin menu endpoints in FastAPI swagger docs | Dan | `/docs` screenshot |
| API-03 | MUST | Order status tracking | Order entity includes status field and status endpoint | Dan | API tests |
| API-04 | MUST | Validation and sanitisation | Pydantic schemas, value limits, string cleanup | Dan | schema code + tests |
| API-05 | SHOULD | Role-based access for admin actions | Simple API key / bearer token for admin routes | Dan | security doc + tests |
| API-06 | MAY | Analytics | Out of scope unless finished early | Dan | backlog |
| TECH-01 | MUST | Use .NET MAUI | Frontend project built in MAUI | Adam | project file |
| TECH-02 | MUST | Use XAML + C# | XAML views and C# viewmodels/services | Adam | repo structure |
| TECH-03 | MUST | Shared service/state management | `IOrderStateService` registered in DI | Adam | service registration |
| TECH-04 | MUST | Add food items without code changes | Menu stored in SQLite and managed by CRUD | Dan | DB + admin endpoints |
| TECH-05 | MUST | Communicate with backend via REST | API service uses HTTP client and DTO mapping | Shared | API service |
| TECH-06 | MUST | Structured architecture pattern | MVVM on frontend; layered API backend | Shared | architecture section in README |
| TECH-07 | MUST | Separate UI, business logic, data access | Views/VMs/Services in app; routers/services/repositories in API | Shared | repo structure |
| TECH-08 | MUST | Dependency injection | MAUI DI and FastAPI dependency pattern | Shared | startup code |
| TECH-09 | MUST | Reusable UI components/styles | DishCard and OrderSummaryBar with shared resources | Adam | component folder |
| TECH-10 | MUST | Dynamic dish images from backend | Image URLs served by backend and displayed by app | Shared | integration evidence |
| TECH-11 | SHOULD | Offline browsing | Cache last menu response locally | Adam | optional implementation |
| TECH-12 | SHOULD | Animations/custom alerts/transitions | Add simple navigation or add-to-order animation | Adam | UX polish evidence |
| TEST-01 | MUST | Unit tests for order logic | Tests for add/remove/update/calculate totals | Shared | `/tests` |
| TEST-02 | SHOULD | Test service/API data loading | Mock API service tests return expected structures | Shared | `/tests` |
| TEST-03 | SHOULD | Test order state persistence across navigation | Service-level state persistence tests | Adam | `/tests` |
| TEST-04 | MUST | All tests stored in dedicated test projects | Separate frontend/backend test folders | Shared | repo structure |
| TEST-05 | SHOULD | README explains how to run tests | Include commands in root README | Shared | `README.md` |
| TEST-06 | SHOULD | Edge case tests | Invalid remove, empty data, multi-quantity | Shared | test suite |
| TEST-07 | MAY | CI automation | GitHub Actions runs a subset or all tests | Shared | workflow file |
| DOC-01 | MUST | Source code hosted in GitHub | Single repo containing frontend, backend, docs | Shared | repository |
| DOC-02 | MUST | Supporting documentation in repo | Meeting notes, requirements, reflection, wireframes | Shared | `/docs` |
| DOC-03 | MUST | README with setup/run/test instructions | Detailed root README | Shared | `README.md` |
| PM-01 | MUST | Use GitHub Issues or project management tool | GitHub Projects with linked issues and assignees | Shared | screenshots |
| PM-02 | MUST | Evidence of planning/task allocation | Export or screenshot board in docs | Shared | `/docs/project-management` |
| ETH-01 | SHOULD | Explain ethics considerations | Accessibility, security, trust, data handling | Shared | ethics doc |
| FUT-01 | SHOULD | Explain future development | Prioritised roadmap beyond MVP | Shared | ethics/future doc |
| COL-01 | SHOULD | Commit history from each member | Regular commits from Dan and Adam | Shared | Git history |

## Requirements That Should Be Explicitly Declared Out of Scope
- Full payment gateway integration
- Full authentication and user account lifecycle
- Complex analytics dashboards
- Real-time kitchen system

These features can remain in the backlog under **MAY** or **future work** unless all MUST requirements are stable.

## Design Decisions Summary
1. **SQLite** is used so menu items can be added without code changes.
2. **FastAPI** is used because it provides REST support, validation, and automatic OpenAPI documentation.
3. **MAUI Shell + MVVM** supports the flyout navigation requirement and keeps UI logic separate.
4. **Shared Order State Service** is the core mechanism for maintaining persistent order state across pages.
5. **Reusable UI Components** reduce duplication and improve consistency between category pages.
