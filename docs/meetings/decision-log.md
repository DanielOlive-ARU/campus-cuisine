# Decision Log

This log records technical and project decisions that may need to be defended in the viva. It uses the structure from `docs/meetings/decision-log-template.md`.

| Date | Decision | Reason | Alternatives Considered | Impact |
|---|---|---|---|---|
| 2026-04-15 | Keep and document the existing dependency injection approach rather than refactor immediately | The course leader confirmed DI must be part of the solution. Review showed DI already exists in the MAUI frontend through `MauiProgram.cs` service registration and in the FastAPI backend through `Depends(...)`. | Refactor frontend pages to full constructor injection now; leave DI undocumented | Confirms the project satisfies the DI requirement while avoiding a risky refactor during frontend patch slices. A future hardening pass can improve DI style if time allows. |
| 2026-04-15 | Treat Order Summary as a top-level MAUI Shell destination | Manual testing found that opening Order Summary as a pushed route could leave the app stuck when the user selected the same flyout item again. Order Summary is a primary page and should be part of the Shell hierarchy. | Keep pushed navigation and call `PopToRoot`; remove Order Summary from the flyout; use Shell item lookup instead of routes | Supports the requirement that all primary pages remain accessible from the flyout and makes navigation behaviour easier to explain and test. |
| 2026-04-15 | Keep a temporary `QuantityText` UI buffer on `OrderLineDto` to stabilise Order Summary editing | Manual testing found that the quantity `Entry` between the `-` and `+` buttons could drift out of sync with the real quantity, while totals remained correct. Direct binding from the editable `Entry` to `Quantity` caused invalid and stale UI states. | Keep direct `Entry` binding to `Quantity`; manage `Entry.Text` manually in code-behind; introduce a dedicated `OrderLineViewModel` immediately | Restores correct user-visible quantity editing for MVP without reopening larger frontend refactors. This should be revisited later to move UI-editing state out of the DTO and into a cleaner MVVM layer. |
| 2026-04-17 | Keep current page-level summary bars and concrete `OrderState` for MVP stability, but record reusable `OrderSummaryBar` and `IOrderStateService` as pre-submission hardening targets | Current functionality is stable, but both areas remain weaker than the desired final architecture for submission | Refactor both immediately before stabilising the current branch; leave them undocumented | Preserves the stable branch while making the intended architectural end state explicit before final submission. |
| 2026-04-17 | Refactor category summary bars into a reusable `OrderSummaryBar` once the frontend core/test split and regression checks were stable | The duplicated category-page summary bar UI had become the next clear architecture weakness after the frontend checkpoint was secured with automated tests and manual Windows validation. | Keep the duplicated XAML until the final submission; combine this refactor with `IOrderStateService` in one larger change | Removes repeated UI and page-specific summary-bar logic while keeping the user-facing category flow unchanged. This closes the reusable summary bar hardening target and leaves `IOrderStateService` as the next DI-focused hardening task. |
| 2026-04-17 | Introduce `IOrderStateService` once the reusable summary bar and frontend regression checkpoint were stable | The remaining frontend DI weakness was direct dependence on the concrete `OrderState` singleton. The interface slice could now be done safely because the core/test split and reusable summary bar refactor were already validated. | Leave the concrete type in place until final submission; combine this change with a broader constructor-injection rewrite | Improves DI clarity and viva defensibility while preserving a single shared runtime state instance. The branch now exposes a clearer contract for order state and backs it with additional notification tests. |
| 2026-04-19 | Complete the deferred MVVM refactor on branch `mvvm-ordersummary-refactor` (off `main`) rather than on the submission branch | The previous `QuantityText` buffer on `OrderLineDto` and 263-line Order Summary code-behind materially weakened the LO3 MVVM defence. With the submission already merged to `main`, a dedicated branch carries no risk to the fallback state. | Ship the pragmatic MVVM state as-is and defend it verbally; refactor directly on `main` | The submitted `main` is untouched and remains the safe fallback if the refactor regresses. A 13-commit branch with 148 green xUnit tests is available for viva walkthrough, with a `pre-homepage` tag at commit `96decae` as an internal rollback anchor. |
| 2026-04-19 | Introduce `OrderLineEntry` + immutable `MenuItemSnapshot` and strip `OrderLineDto` to `MenuItemId` + `Quantity` only | `OrderLineDto` previously carried both the backend wire shape and four `[JsonIgnore]` frontend snapshot fields. Splitting them makes the DTO a true serialisation contract and moves the frontend representation (with `INotifyPropertyChanged`) into its own type. | Keep `OrderLineDto` as-is and add a parallel `Dictionary<int, MenuItemSnapshot>` on `OrderState`; skip this step and rely on `[JsonIgnore]` semantics for separation | `ToCreateOrderRequest` now constructs fresh minimal DTOs at serialisation time. `IOrderStateService.Lines` exposes `ObservableCollection<OrderLineEntry>`. Supersedes the 2026-04-15 `QuantityText` buffer decision. |
| 2026-04-19 | Extract `HomePageViewModel` and replace `MenuItemView`'s wholesale-rebuild with a `MenuItemCardSync` projection | The 358-line `HomePage.xaml.cs` and the `DisplayItems.Clear()` + force-rebind pattern in `MenuItemView` were the two remaining fat view-layer components identified during the refactor scope audit. | Leave both as-is and flag as follow-up branches; refactor HomePage only | View-model layer is now consistent across Home, Order Summary, and the menu-card control. The `MenuItemCardSync` projection preserves view-model instance identity across cart mutations, eliminating the scroll-position-loss regression. |
| 2026-04-19 | Extract `OrderSummaryPageViewModel` to align Order Summary with HomePage's `BindingContext = _vm` pattern | Phase 1 of the refactor kept `BindingContext = this` on OrderSummaryPage to minimise risk while the new VM + sync layer was proving itself. With HomePage already using a dedicated VM, OrderSummaryPage was the last visible "page-as-its-own-view-model" inconsistency. | Leave OrderSummaryPage as-is and defend the inconsistency as a pragmatic Phase 1 choice | Both stateful pages now follow the same pattern. Page code-behind shrinks; the Attach / Detach subscription lifecycle becomes a unit-testable method on the VM rather than an event handler on the page. Pre-refactor safety tag `pre-summary-vm` at `0201376`. |
| 2026-04-19 | Migrate every meaningful click handler onto `ICommand` on a view-model, and wire pages through constructor DI (Gaps 1 and 2) | The broadest remaining MVVM gap was that click handlers and service resolution still lived in page code-behind. Closing both lifts the architectural claim from "pragmatic MVVM" to "MVVM with conscious framework-ergonomics trade-offs". | Keep click handlers in code-behind and defend verbally; take a `CommunityToolkit.Mvvm` dependency for the `ICommand` implementations; skip constructor DI on pages and accept service-locator | 7 commits on top of `pre-commands` at `9264709`. Hand-rolled `RelayCommand` / `AsyncRelayCommand`, `IDialogService` / `INavigationService` abstractions in Core with MAUI-side Shell wrappers, VMs / pages registered in DI, every page constructor receives its dependencies. 47 new tests; 210 green total. Only three view-layer hooks remain in code-behind (Place Order animation, Entry validation, `MenuItemViewModel` category string) — each defensible as a MAUI-ergonomics trade. |
| 2026-04-19 | Merge `mvvm-ordersummary-refactor` to `main` via PR #11 (commit `a6ccbf5`) | All five phases (24 commits) green at every commit; Windows GUI smoke test passed end-to-end across home navigation, menu-card +/-, order-summary commands, and place-order; 210 frontend tests passing; backend untouched. The deferred MVVM hardening is now the live submission state rather than a parallel branch. | Keep the refactor on a parallel branch and defend the original "pragmatic MVVM" submission state verbally; squash the 24 commits into a single merge commit and lose the bisection history | The submission state now reflects the strengthened MVVM architecture. The rollback anchor tags (`pre-homepage`, `pre-summary-vm`, `pre-commands`) remain on origin so any reviewer can compare the pre/post architecture; the per-phase commit history is preserved on `main` for the same reason. |

## Decision Notes

### Dependency Injection Review

#### Context
The assessor explicitly stated that dependency injection must be part of the submitted solution. We reviewed the frontend and backend before continuing with more frontend patch slices.

#### Evidence
The MAUI frontend registers services in `MauiProgram.cs`:

```csharp
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
  client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddSingleton<OrderState>();
builder.Services.AddSingleton<IOrderStateService>(sp => sp.GetRequiredService<OrderState>());
```

The frontend then resolves these registered services where needed, including the shared `OrderState` and API service. The backend uses FastAPI dependency injection with `Depends(...)` for request-scoped database sessions and admin API-key checks.

#### Decision
The current DI implementation is sufficient for the coursework requirement. We will document it clearly and avoid a broad refactor until the core functional slices are stable.

#### Future Improvement
If time allows, the frontend could be hardened by moving more pages and view models toward direct constructor injection instead of resolving services through the application service provider.

This has been recorded as a technical-debt item in `docs/project-management/backlog-and-milestones.md`.

### Order Summary Shell Navigation Fix

#### Context
During manual Windows testing, opening Order Summary from a category page worked initially, but flyout navigation could then get stuck. For example:

1. Open Mains.
2. Tap the page-level Order Summary button.
3. Open the flyout.
4. Tap Mains.
5. The app stays on Order Summary instead of returning to Mains.

The same issue could follow whichever page launched Order Summary.

#### Cause
`OrderSummaryPage` was being opened as a pushed Shell route over the current flyout item. Shell still considered the underlying category page selected, so selecting that same flyout item did not reset the visible page.

#### Decision
Make Order Summary an explicit top-level Shell route and navigate to it with absolute Shell navigation:

```csharp
await Shell.Current.GoToAsync("//OrderSummaryPage");
```

This treats Order Summary as a primary flyout destination rather than a detail page pushed onto another page's navigation stack.

#### Requirement Link
This supports:

- GEN-01: app uses flyout navigation
- GEN-04: all primary pages are accessible from the flyout
- GEN-05: order state persists during navigation
- GEN-02: app should not get stuck during normal use

#### Reflection
This bug was useful because the first symptom looked like an order-state or clear-order issue. Manual reproduction showed the problem followed the page that opened Order Summary, which pointed to Shell route state instead. The fix improved our understanding of MAUI Shell and showed why full user-journey navigation testing is needed, not just page-open testing.

### Order Summary Quantity Editing Buffer

#### Context
During manual frontend testing, Order Summary totals and item counts could remain correct while the editable quantity field between the `-` and `+` buttons showed a stale value. A later fix that bound the `Entry` directly to `Quantity` with `Mode=TwoWay` also allowed invalid values such as `0` or negative numbers to influence totals before validation completed.

#### Cause
The `Entry` was mixing UI editing state with the committed model state. This created two problems:

- the visible text could drift out of sync from the actual quantity
- invalid values could reach the real quantity before validation rolled them back

#### Decision
Use a temporary UI buffer property, `QuantityText`, on `OrderLineDto`. The `Entry` binds to `QuantityText`, validation works against the buffer, and committed valid values continue to update the real `Quantity` through `OrderState.SetQuantity(...)`.

This is a pragmatic stabilisation step for MVP. `QuantityText` is marked `[JsonIgnore]` and does not change the backend contract or order payload shape.

#### MVVM Position
This does not materially break the project's MVVM structure, because `QuantityText` is still bindable UI state and not business logic. However, it is less clean than a dedicated view-model layer because the DTO now carries temporary editing state as well as transport/state data.

#### Future Improvement
Once the MVP frontend flow and tests are stable, move the quantity editing buffer out of `OrderLineDto` into a dedicated order-summary view model or order-line view model. This will strengthen separation of concerns and make the MVVM story easier to defend.

This has been recorded as a technical-debt item in `docs/project-management/backlog-and-milestones.md`.

#### Current Deferral Rationale
We deliberately deferred that refactor after the `IOrderStateService` hardening landed. The proposed fix would introduce a second synchronisation layer over `_orderState.Lines` on the most regression-prone page in the app, which creates a higher risk of stale quantity display, focus loss during typing, or totals drifting out of sync. The current `QuantityText` approach keeps `OrderState` as the single source of truth, the Windows flow is stable, and the frontend regression suite is currently at `39` passing tests. The marginal MVVM gain is therefore smaller than the regression risk at this stage, so the team chose to pivot to remaining SHOULD/MAY work instead.

#### Superseded By
Superseded on 2026-04-19 by the MVVM refactor on branch `mvvm-ordersummary-refactor`. See "Frontend MVVM Refactor" below for the delivered design (dedicated `OrderSummaryLineViewModel` + `OrderSummaryLineSync` projection + stripped `OrderLineDto`) and `docs/viva/frontend-mvvm-refactor-notes.md` for the viva defence narrative.

### Summary Bar and Shared State Hardening

#### Context
The frontend previously implemented the category summary bar behaviour directly inside the Starters, Mains, and Desserts pages, and it used the concrete `OrderState` service registered in MAUI DI. The duplicated summary bar UI has now been refactored into a reusable `OrderSummaryBar`, and the order-state DI has now been hardened with `IOrderStateService` over the same shared singleton instance.

#### Decision
Stabilise the branch first, then complete the reusable `OrderSummaryBar` refactor and the `IOrderStateService` hardening step once the frontend core/test checkpoint is in place.

#### Why
The reusable summary bar improves alignment with the reusable UI component requirement and removes duplicated UI logic from the three category pages without altering the user journey. `IOrderStateService` improves DI clarity and testability without changing runtime behaviour, because the interface resolves to the same underlying singleton `OrderState` instance.

#### Current State
The reusable summary bar and `IOrderStateService` hardening have both been completed on `submission-hardening-and-testing`.

### Frontend MVVM Refactor

#### Context
The submission merged to `main` defends LO3 ("structured architectural pattern (e.g. MVVM)") as "pragmatic MVVM": `OrderLineDto` carried a `QuantityText` UI edit buffer plus four `[JsonIgnore]` frontend snapshot fields, `OrderSummaryPage.xaml.cs` was 263 lines with validation / dialogs / busy-state / navigation / animation tangled together, and `HomePage.xaml.cs` was 358 lines with 13 observable properties baked into the page (`BindingContext = this`). `MenuItemView.xaml.cs` rebuilt its `DisplayItems` collection wholesale on every cart change. All four were defendable under time pressure but materially weaker than a true MVVM separation.

#### Decision
Complete the deferred MVVM refactor on a dedicated branch (`mvvm-ordersummary-refactor`) off `main` rather than on the submission branch. The refactor was delivered in three phases across 13 green-building commits:

1. **Order Summary** — introduced `OrderSummaryLineViewModel`, `OrderSummaryLineSync` (match-by-MenuItemId incremental diff preserving `Entry` focus), and `OrderConfirmationPresenter` (pure formatting helper). `QuantityText` moved off the DTO onto the VM.
2. **HomePage** — extracted `HomePageViewModel` holding all 13 observable properties, both category loaders (`LoadFeaturedAsync` / `LoadIndulgenceAsync`), and the `OrderState` subscription lifecycle. `HomePage.xaml.cs` collapsed from 358 lines to ~60.
3. **OrderLineEntry cascade + MenuItemView** — introduced `OrderLineEntry` wrapping an immutable `MenuItemSnapshot` record. Migrated `OrderState` to expose `ObservableCollection<OrderLineEntry>`; stripped `OrderLineDto` to `MenuItemId` + `Quantity` only. Extracted `MenuItemCardViewModel` (with `INotifyPropertyChanged`) and `MenuItemCardSync` (replacing the wholesale-rebuild with per-id incremental diff).

#### Requirement Link
This strengthens:

- TECH-06: structured architecture pattern (MVVM)
- TECH-07: separate UI, business logic, data access
- TECH-09: reusable UI components (`MenuItemView` now uses a proper view-model layer rather than a nested class)

#### Test Evidence
Test suite grew from `47` to `148` green tests across the 13 commits. `dotnet build` on `net10.0-windows10.0.19041.0` produced 0 warnings and 0 errors on every commit. Windows GUI smoke test passed end-to-end (add from category pages, edit quantities via buttons and direct entry, remove with confirmation, clear order with confirmation, place order, observe backend order status in confirmation alert).

#### Safety Measures
A `pre-homepage` tag at commit `96decae` marks the end of Phase 1, giving a known-good rollback anchor before the larger Phase 3 cascade. The branch was subsequently extended with Phases 4 and 5 (see the "Command Pattern and Constructor DI" section below) and merged to `main` via PR #11 (commit `a6ccbf5`) on 2026-04-19 once Windows GUI smoke testing across all five phases passed and the test suite reached 210 green xUnit tests. The rollback anchor tags (`pre-homepage`, `pre-summary-vm`, `pre-commands`) remain on origin for reviewer traceability.

#### Reflection
The refactor's strongest lesson was the cost of wholesale `Clear()` + re-add patterns in `ObservableCollection` bindings. The match-by-id incremental diff pattern (used in both `OrderSummaryLineSync` and `MenuItemCardSync`) preserves view-model instance identity across updates, which in turn preserves UI focus, scroll position, and animation state. The immutable `MenuItemSnapshot` record also turned out to be load-bearing: because it is value-equal and cannot be mutated from outside, any Entry can freely delegate `Name`/`Description`/`UnitPrice` to its Snapshot without defensive copying. Full narrative in `docs/viva/frontend-mvvm-refactor-notes.md`.

### Command Pattern and Constructor DI (Phases 4 and 5)

#### Context
After the first three phases landed, the honest remaining MVVM gaps were (a) click handlers living in page code-behind rather than as `ICommand` bindings on view-models, (b) `App.Services.GetRequiredService<T>()` service-locator calls inside page constructors rather than pure constructor injection, and (c) a visible inconsistency where `HomePage` already used `BindingContext = _vm` but `OrderSummaryPage` still used `BindingContext = this`. Each was defensible individually but collectively pulled the LO3 claim down from "MVVM" to "pragmatic MVVM".

#### Decision
Close all three gaps across two additional phases:

1. **`OrderSummaryPageViewModel`** — own the `Lines` collection, totals, `OrderSummaryLineSync` lifecycle, and `HasOrder` on a dedicated view-model; page becomes `BindingContext = _vm`. Attach / Detach methods let the page wire the sync to `OnAppearing` / `OnDisappearing`.

2. **`ICommand` everywhere meaningful** — hand-rolled `RelayCommand` / `AsyncRelayCommand` types in `Commands.cs` (no `CommunityToolkit.Mvvm` dependency), plus `IDialogService` / `INavigationService` abstractions in `CampusCuisine.Core/Services/`. MAUI-side implementations wrap `Shell.Current.DisplayAlertAsync` and `Shell.Current.GoToAsync`. Every user-facing action (add, decrease, remove, increase, clear, place order, start new, continue, quick navigate) moves onto a command on the relevant view-model. The Place Order command drives `IsPlacingOrder` and a derived `PlaceOrderButtonText` binding so the button text / enabled state is VM-driven, not page-driven.

3. **Constructor DI on pages** — register `HomePageViewModel`, `OrderSummaryPageViewModel`, and every Shell-declared page (Home / Starters / Mains / Desserts / Order Summary / Help) as transient services in `MauiProgram.cs`. Page constructors now receive their view-model (for stateful pages) or `IApiService` (for category pages) via constructor injection; `App.Services.GetRequiredService<T>()` is gone from every page body.

#### Requirement Link
This strengthens the TECH-06 (structured architecture pattern), TECH-07 (separate UI / business logic / data access), and TECH-08 (dependency injection) mapping entries. It does not change what the app does — all existing tests continue to pass — only how the layers are wired.

#### What stayed in code-behind, and why
Three things: the Place Order press animation (inherently view-scoped, needs the named Button reference), the Entry `Completed` / `Unfocused` quantity validation handlers (MAUI's `Entry` has no `CompletedCommand`), and constructing `MenuItemViewModel(api, "Mains")` on category pages (the category is a runtime string, not a DI-resolvable dependency). Each is a conscious MAUI-ergonomics trade rather than accidental smell.

#### Safety Measures
Pre-slice safety tag `pre-commands` at `9264709` pushed to origin before the work started. Every commit runs `dotnet test` + `dotnet build` to green before the next begins; the Phase 5 atomic nature (commands + DI touches almost every page) means rollback is one `git reset --hard pre-commands && git push --force-with-lease` away. Rollback was never needed — Windows GUI smoke test passed on the four high-risk paths (home navigation, menu-card +/-, order summary commands, place order).

#### Reflection
The biggest surprise was how few click handlers genuinely need to stay in code-behind once you build the `IDialogService` / `INavigationService` seams. The end state has just three handlers remaining (animation, Entry validation, MenuItemViewModel category construction) — each defensible as "MAUI framework idiom makes this awkward in the VM". Full narrative in `docs/viva/frontend-mvvm-refactor-notes.md`.
