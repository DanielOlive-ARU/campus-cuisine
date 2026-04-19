# Frontend MVVM Refactor Viva Notes

These notes support the viva defence of the post-submission MVVM refactor that lives on branch `mvvm-ordersummary-refactor`. The submitted `main` branch is the safe fallback; this branch strengthens the LO3 "structured architectural pattern (e.g. MVVM)" clause from "pragmatic MVVM" to a fully separated view-model layer.

Cross-references:
- `docs/meetings/decision-log.md` — decisions taken and superseded
- `docs/reflection/development-reflection.md` — what went well / what I learned
- `docs/requirements/requirements-mapping.md` — TECH-06, TECH-08, TECH-09 evidence pointers
- `docs/project-management/backlog-and-milestones.md` — Issue 32 closure

## What did the MVVM refactor deliver?

Thirteen green-building commits on `mvvm-ordersummary-refactor` grouped into three phases:

**Phase 1 — Order Summary** (5 commits)
1. Add `OrderSummaryLineViewModel` with unit tests
2. Add `OrderSummaryLineSync` projection with unit tests
3. Rewire `OrderSummaryPage` to use `OrderSummaryLineViewModel` via sync
4. Remove `QuantityText` UI buffer from `OrderLineDto`
5. Extract `OrderConfirmationPresenter` with unit tests

**Phase 2 — HomePage** (2 commits)
6. Add `HomePageViewModel` with unit tests
7. Rewire `HomePage` to `HomePageViewModel` (358-line code-behind → ~60 lines)

**Phase 3 — OrderLineEntry + MenuItemView** (6 commits)
8. Add `MenuItemSnapshot` and `OrderLineEntry` types with tests
9. Migrate `OrderState` and Order Summary view layer onto `OrderLineEntry`
10. Strip `OrderLineDto` to serialisation-only shape (`MenuItemId` + `Quantity` only)
11. Extract `MenuItemCardViewModel` to Core with INPC + tests
12. Add `MenuItemCardSync` projection with unit tests
13. Rewire `MenuItemView` to use `MenuItemCardViewModel` via sync

Test count: **47 → 148** (101 new xUnit tests). Build: `dotnet build` on `net10.0-windows10.0.19041.0` → 0 warnings / 0 errors throughout.

## Why was this refactor needed?

Before the refactor, three architectural smells sat in the frontend:

1. `OrderLineDto` carried a `QuantityText` UI edit buffer plus `Name`/`Description`/`UnitPrice`/`LineTotal` as `[JsonIgnore]` frontend snapshot state. A data-transfer object is not the right home for UI edit state or rendering cache.
2. `OrderSummaryPage.xaml.cs` was 263 lines mixing quantity validation, confirmation dialogs, busy-state toggles, navigation, animation, and confirmation-message construction.
3. `HomePage.xaml.cs` was 358 lines with 13 observable properties baked into the page (`BindingContext = this`), two cached category lists, two async loaders, and an order-totals subscription.
4. `MenuItemView.xaml.cs` used a wholesale `DisplayItems.Clear()` + re-add pattern on every cart change, plus an `ItemsCollection.ItemsSource = null/DisplayItems` force-rebind, losing scroll position and focus on every update.

These were all defendable as "pragmatic MVVM" for a submission under time pressure, but the module is an MVVM full-stack module — LO3 benefits materially from a cleaner layer split.

## How is the frontend now structured?

| Layer | Responsibility | Example files |
|---|---|---|
| Models (serialisation only) | Backend contract | `OrderLineDto` (MenuItemId + Quantity only), `CreateOrderRequestDto`, `OrderConfirmationDto` |
| Models (frontend representation) | In-cart line + immutable snapshot | `OrderLineEntry`, `MenuItemSnapshot` |
| Services (singleton state) | Single source of truth for the cart | `OrderState` / `IOrderStateService` |
| View-models | Per-line / per-page UI state | `OrderSummaryLineViewModel`, `HomePageViewModel`, `MenuItemCardViewModel`, `MenuItemViewModel` |
| Sync projections | Wire service state into view-model collections via match-by-id incremental diff | `OrderSummaryLineSync`, `MenuItemCardSync` |
| Presenters | Pure formatting helpers | `OrderConfirmationPresenter` |
| Views | XAML + thin code-behind (navigation + DisplayAlert + animation) | `OrderSummaryPage.xaml[.cs]`, `HomePage.xaml[.cs]`, `MenuItemView.xaml[.cs]` |

## Why is `OrderSummaryLineSync` a separate class from the view-model?

A view-model describes the shape of one row on the page. A sync describes the projection of a service-owned collection into a view-facing collection of view-models. They have different lifetimes (the VM lives as long as the row is visible; the sync lives as long as the page is appearing) and different responsibilities (binding surface vs. subscription coordination). Splitting them makes both testable in isolation — the 13 `OrderSummaryLineSyncTests` use a real `OrderState` with no mocks because the sync is pure coordination over public interfaces.

## Why does `OrderLineEntry` wrap an immutable `MenuItemSnapshot`?

The snapshot is the menu item's display data at the moment it was added to the cart. It should not change underneath the user — if an admin updates the price in the backend, the line already in the cart keeps its captured price until placement. Immutability makes that invariant obvious: the `MenuItemSnapshot` is a C# `record`, so reference-equality guards cost nothing and the snapshot cannot be accidentally mutated from outside. The one case where a snapshot *should* update is fill-if-empty (a later `AddLine` supplies data the earlier call lacked) — for that case `OrderLineEntry` exposes a setter that replaces the snapshot wholesale and raises `PropertyChanged` for every derived field in one step.

## How do you preserve `Entry` focus during unrelated cart updates?

Two mechanisms working together:

1. **Match-by-MenuItemId incremental diff** — `OrderSummaryLineSync` never does `target.Clear()` + re-add. It maintains a `Dictionary<OrderLineEntry, OrderSummaryLineViewModel>` so that an `Add` inserts the new row at the right index, a `Remove` removes only the matching VM, and a `Reset` is the only path that rebuilds (and the VM being edited is gone when `Clear()` fires anyway).
2. **Idempotent mirror setters** — when an entry's `PropertyChanged` fires for `Quantity`, the sync guards with `if (vm.Quantity != entry.Quantity) vm.Quantity = entry.Quantity;` before mutating. This stops spurious `PropertyChanged` fires on the VM when the service and the VM already agree, which in turn prevents binding system feedback that could disrupt the active `Entry`'s text cursor.

Evidence: test `SetQuantity_UpdatesVmInPlace_SameInstance` in `OrderSummaryLineSyncTests.cs` asserts `Assert.Same(vmBefore, target[0])` after `state.SetQuantity(1, 5)`.

## Why is `ToCreateOrderRequest` still constructing fresh `OrderLineDto` instances?

`OrderLineDto` is now a minimal serialisation shape — `MenuItemId` and `Quantity` only. `OrderState.ToCreateOrderRequest` projects the `OrderLineEntry` collection into a new `List<OrderLineDto>` at the moment of placement. This keeps the wire shape stable against the FastAPI backend (no backend change needed) while the frontend representation is richer. The DTO is transient — it is born at serialisation time and discarded when the HTTP call completes.

## Why did you keep click handlers in the page code-behind rather than moving them to commands on the view-model?

`OnStartNewOrderClicked`, `OnContinueOrderClicked`, `OnQuickNavigateClicked`, `OnPlaceOrderClicked` all do three things that are genuinely view concerns: `DisplayAlertAsync` (MAUI page-scoped API), `Shell.Current.GoToAsync` (navigation), and — in the case of Place Order — a `ScaleToAsync` animation on a named XAML element. A command on the view-model would need either (a) a MAUI-scoped dependency injected into the VM (breaking the Core library's independence from the MAUI framework), or (b) a callback indirection with no practical gain. The pragmatic MVVM line this refactor draws is: view-models own editable UI state and orchestrated logic; the code-behind owns navigation, dialogs, and animations. That split is defendable because it is consistent across every page (Home, Order Summary, Menu pages all follow the same rule).

## How did you test the non-deterministic random pick in `HomePage`?

`LoadFeaturedAsync` does `Random.Shared.Next(_cachedMains.Count)` — non-deterministic. The tests seed the `FakeApiService` with a single-element list for each category, which collapses `Random.Shared.Next(1)` to a deterministic `0`. That lets us assert exact values (`"Chicken Burger"`, `"£8.50"`, etc.) without mocking `Random`. For the cache-reuse test we also count handler invocations per category via a closure counter — three successive `InitializeAsync` calls should produce one `main` fetch and one `dessert` fetch if caching works correctly.

## How did the test count jump from 47 to 148?

| Target | Count | Purpose |
|---|---|---|
| `OrderSummaryLineViewModelTests` | 20 | Quantity/QuantityText normalisation, LineTotal math, UpdateFrom, TryValidateQuantity (happy + 3 failure families) |
| `OrderSummaryLineSyncTests` | 12 | Seed, Add, Remove, SetQuantity (instance identity), Clear, Dispose, AddRemoveAdd same id |
| `OrderConfirmationPresenterTests` | 4 | Full message, missing status, missing prep, whitespace status |
| `HomePageViewModelTests` | 17 | Defaults, Featured/Indulgence loaders, error paths, cache reuse, retry-after-failure, OrderState subscription updates, Dispose |
| `MenuItemSnapshotTests` | 5 | Record value equality, inequality on each field, field access |
| `OrderLineEntryTests` | 14 | Ctor, null checks, Quantity setter INPC, Snapshot setter INPC, delegation of Name/Description/UnitPrice, LineTotal math |
| `MenuItemCardViewModelTests` | 11 | Ctor-from-MenuItemModel, INPC on each property, HasQuantity/QuantityText computation |
| `MenuItemCardSyncTests` | 18 | Seed (empty/items/cart/orphan), Items Add/Remove/Reset, Cart Add/Remove/Set/Clear, AddRemoveAdd same id, Dispose |
| Existing tests (untouched) | 47 | OrderStateTests, OrderStatePersistenceTests, ApiServiceTests, CachedApiServiceTests, MenuItemViewModelTests |
| **Total** | **148** | All green on every commit in the branch |

## Why is `OrderState` still a singleton?

Two reasons: the cart must survive page navigation (`OrderStatePersistenceTests` pin this contract — resolving `IOrderStateService` twice returns the same instance), and the brief's "Shared service/state management" (TECH-03) is explicitly singleton-shaped. Moving to a scoped or transient registration would break the cross-page persistence guarantee. The refactor does not remove the singleton — it layers view-models on top of it. The singleton is the source of truth; the VMs are projections for the view layer.

## What was the scope discipline?

Before starting, I mapped every violation I could find and chose a three-phase sequence (Order Summary → OrderLineEntry cascade → HomePage/MenuItemView). After each commit I ran the full xUnit suite and the Windows MAUI build; a failure would stop the phase and force a re-plan rather than stacking broken commits. The safety net was the `pre-homepage` tag at commit `96decae` (Phase 1 complete) — a `git reset --hard pre-homepage` + `git push --force-with-lease` would return the branch to a known-good state at any time. The cost of that discipline is 13 commits rather than 1; the payoff is that every commit is individually bisectable for viva walkthrough.

## What would you do next if you had more time?

1. **`IDisposable` on `MenuItemView`** — the `ContentView` holds an `MenuItemCardSync` that subscribes to `IOrderStateService`. If the containing page is destroyed without triggering sync disposal, the singleton keeps a delegate pointing at the sync, which keeps the sync alive (and its `_target`). A `Loaded`/`Unloaded` handler pair on the `ContentView` would make the lifetime deterministic.
2. **Thread-marshaling abstraction** — `HomePageViewModel` drops the `MainThread.BeginInvokeOnMainThread` wrapping that the old code-behind used. MAUI's data-binding system handles scalar property updates correctly in practice, but a `IDispatcher` abstraction (injected) would remove the implicit assumption.
3. **Command pattern for navigation** — an `ICommand` exposed by the VM, bound from XAML, would let the code-behind shrink to just `InitializeComponent()` + `OnAppearing → vm.InitializeAsync()`. The navigation callback would need a framework-neutral abstraction over `Shell.Current.GoToAsync`.
4. **Property-level tests for the `CollectionChanged` adapters** — the two syncs translate between different collection-change shapes (OrderState's per-entry events vs. the sync's per-VM target). Fuzz tests varying the event sequence would strengthen confidence.

## One-line summary

I extracted a dedicated view-model layer out of the submitted frontend (five new view-models, two sync projections, a pure presenter, an immutable snapshot record, and a true serialisation-only DTO) with 101 new unit tests and zero behaviour regressions, while keeping `main` untouched as the submission fallback and keeping `OrderState` as the single source of truth for the cart.
