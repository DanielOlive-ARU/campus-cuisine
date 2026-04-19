# Frontend MVVM Refactor Viva Notes

These notes support the viva defence of the post-submission MVVM refactor that lives on branch `mvvm-ordersummary-refactor`. The submitted `main` branch is the safe fallback; this branch strengthens the LO3 "structured architectural pattern (e.g. MVVM)" clause from "pragmatic MVVM" to a view-model layer that owns state, validation, and user commands without leaking MAUI framework types into the Core library.

Cross-references:
- `docs/meetings/decision-log.md` — decisions taken and superseded
- `docs/reflection/development-reflection.md` — what went well / what I learned
- `docs/requirements/requirements-mapping.md` — TECH-06, TECH-08, TECH-09 evidence pointers
- `docs/project-management/backlog-and-milestones.md` — Issue 32 closure

## What did the MVVM refactor deliver?

Twenty-four green-building commits on `mvvm-ordersummary-refactor` grouped into five phases:

**Phase 1 — Order Summary (5 commits)** — `OrderSummaryLineViewModel`, `OrderSummaryLineSync` (match-by-MenuItemId incremental diff preserving Entry focus), stripped `OrderLineDto.QuantityText`, and `OrderConfirmationPresenter` for the confirmation message.

**Phase 2 — HomePage (2 commits)** — extracted `HomePageViewModel` holding all thirteen observable properties, both category loaders, and the OrderState subscription. `HomePage.xaml.cs` went from 358 lines to ~60.

**Phase 3 — OrderLineEntry cascade + MenuItemView (6 commits)** — introduced `OrderLineEntry` wrapping immutable `MenuItemSnapshot`, migrated `OrderState.Lines` onto it, stripped `OrderLineDto` to `MenuItemId` + `Quantity` only, extracted `MenuItemCardViewModel` with INPC, and added `MenuItemCardSync`.

**Phase 4 — OrderSummaryPageViewModel + layout polish (4 commits)** — `OrderSummaryPageViewModel` owns the Lines collection, totals, sync lifecycle, and HasOrder; page becomes `BindingContext = _vm` like `HomePage`. Menu-card layout stabilised (fixed-height label row) so card geometry does not shift when quantity state changes.

**Phase 5 — Commands and constructor DI (7 commits)** — hand-rolled `RelayCommand` / `AsyncRelayCommand`, `IDialogService` / `INavigationService` abstractions in Core with MAUI-side implementations wrapping `Shell.Current.DisplayAlertAsync` / `Shell.Current.GoToAsync`, view-models now expose `ICommand` properties that XAML binds to directly, and every Shell-declared page is registered in DI so page constructors receive their view-models (or `IApiService`) via constructor injection instead of pulling from `App.Services.GetRequiredService`.

Test count: **47 → 210** (163 new xUnit tests). Build: `dotnet build` on `net10.0-windows10.0.19041.0` → 0 warnings / 0 errors throughout.

## Why was this refactor needed?

Before the refactor, four architectural smells sat in the frontend:

1. `OrderLineDto` carried a `QuantityText` UI edit buffer plus four `[JsonIgnore]` frontend snapshot fields. A data-transfer object is not the right home for UI edit state or a rendering cache.
2. `OrderSummaryPage.xaml.cs` was 263 lines mixing quantity validation, confirmation dialogs, busy-state toggles, navigation, animation, and confirmation-message construction.
3. `HomePage.xaml.cs` was 358 lines with 13 observable properties baked into the page (`BindingContext = this`), two cached category lists, two async loaders, and an order-totals subscription.
4. `MenuItemView.xaml.cs` used a wholesale `DisplayItems.Clear()` + re-add pattern on every cart change, plus an `ItemsCollection.ItemsSource = null/DisplayItems` force-rebind — losing scroll position and focus on every interaction.

All four were defendable as "pragmatic MVVM" for a submission under time pressure, but the module is an MVVM full-stack module — LO3 benefits materially from a cleaner layer split.

## How is the frontend now structured?

| Layer | Responsibility | Example files |
|---|---|---|
| Models (serialisation only) | Backend wire contract | `OrderLineDto` (MenuItemId + Quantity only), `CreateOrderRequestDto`, `OrderConfirmationDto` |
| Models (frontend representation) | In-cart line + immutable snapshot | `OrderLineEntry`, `MenuItemSnapshot` |
| Services (singleton state) | Single source of truth for the cart | `OrderState` / `IOrderStateService` |
| Services (abstraction interfaces) | Platform-neutral contracts consumed by view-models | `IDialogService`, `INavigationService`, `IApiService`, `IMenuCache` |
| Services (MAUI implementations) | Wrap MAUI framework APIs | `MauiDialogService`, `ShellNavigationService`, `ApiService`, `CachedApiService`, `PreferencesMenuCache` |
| View-model layer | Per-line / per-page UI state + commands | `OrderSummaryLineViewModel`, `OrderSummaryPageViewModel`, `HomePageViewModel`, `MenuItemCardViewModel`, `MenuItemViewModel` |
| View-model plumbing | ICommand primitives, sync projections, formatting helpers | `RelayCommand`, `AsyncRelayCommand`, `OrderSummaryLineSync`, `MenuItemCardSync`, `OrderConfirmationPresenter` |
| Views | XAML + thin code-behind (animation + Entry validation only) | `OrderSummaryPage.xaml[.cs]`, `HomePage.xaml[.cs]`, `MenuItemView.xaml[.cs]` |

`CampusCuisine.Core` never references the MAUI framework. Every view-model and plumbing type is unit-tested against fakes (`FakeApiService`, `FakeDialogService`, `FakeNavigationService`) or the real `OrderState`.

## Why is `OrderSummaryLineSync` a separate class from the view-model?

A view-model describes the shape of one row on the page. A sync describes the projection of a service-owned collection into a view-facing collection of view-models. They have different lifetimes (the VM lives as long as the row is visible; the sync lives as long as the page is appearing) and different responsibilities (binding surface vs. subscription coordination). Splitting them makes both testable in isolation — the `OrderSummaryLineSyncTests` use a real `OrderState` with no mocks because the sync is pure coordination over public interfaces.

## Why does `OrderLineEntry` wrap an immutable `MenuItemSnapshot`?

The snapshot is the menu item's display data at the moment it was added to the cart. It should not change underneath the user — if an admin updates the price in the backend, the line already in the cart keeps its captured price until placement. Immutability makes that invariant obvious: the `MenuItemSnapshot` is a C# `record`, so reference-equality guards cost nothing and the snapshot cannot be accidentally mutated from outside. The one case where a snapshot *should* update is fill-if-empty (a later `AddLine` supplies data the earlier call lacked) — for that case `OrderLineEntry` exposes a setter that replaces the snapshot wholesale and raises `PropertyChanged` for every derived field in one step.

## How do you preserve `Entry` focus during unrelated cart updates?

Two mechanisms working together:

1. **Match-by-MenuItemId incremental diff** — `OrderSummaryLineSync` never does `target.Clear()` + re-add. It maintains a `Dictionary<OrderLineEntry, OrderSummaryLineViewModel>` so that an `Add` inserts the new row at the right index, a `Remove` removes only the matching VM, and only a `Reset` rebuilds (and the VM being edited is gone when `Clear()` fires anyway).
2. **Idempotent mirror setters** — when an entry's `PropertyChanged` fires for `Quantity`, the sync guards with `if (vm.Quantity != entry.Quantity) vm.Quantity = entry.Quantity;` before mutating. This stops spurious `PropertyChanged` fires on the VM when the service and the VM already agree, which in turn prevents binding system feedback that could disrupt the active `Entry`'s text cursor.

Evidence: test `SetQuantity_UpdatesVmInPlace_SameInstance` in `OrderSummaryLineSyncTests.cs` asserts `Assert.Same(vmBefore, target[0])` after `state.SetQuantity(1, 5)`.

## Why is `ToCreateOrderRequest` still constructing fresh `OrderLineDto` instances?

`OrderLineDto` is now a minimal serialisation shape — `MenuItemId` and `Quantity` only. `OrderState.ToCreateOrderRequest` projects the `OrderLineEntry` collection into a new `List<OrderLineDto>` at the moment of placement. This keeps the wire shape stable against the FastAPI backend (no backend change needed) while the frontend representation is richer. The DTO is transient — born at serialisation time, discarded when the HTTP call completes.

## How do view-models raise confirmations and navigate without depending on MAUI?

Two interfaces live in `CampusCuisine.Core/Services/`:

```csharp
public interface IDialogService
{
  Task ShowAsync(string title, string message, string ok);
  Task<bool> ConfirmAsync(string title, string message, string accept, string cancel);
}

public interface INavigationService
{
  Task GoToAsync(string route);
}
```

MAUI-side implementations in `FrontEnd/Services/` wrap `Shell.Current.DisplayAlertAsync` and `Shell.Current.GoToAsync` respectively. View-models receive these through constructor injection (optional nullable parameters — when null, the command degrades to a safe no-op). Unit tests inject `FakeDialogService` / `FakeNavigationService` from `CampusCuisine.Tests/TestDoubles/` to verify exactly which confirmations were raised and which routes were requested without spinning up the MAUI runtime.

This is what keeps `CampusCuisine.Core` free of any `Microsoft.Maui.*` reference while still letting commands own the dialog-and-navigate flow of a button tap.

## Why hand-roll `RelayCommand` / `AsyncRelayCommand` rather than take a NuGet dependency?

`CommunityToolkit.Mvvm` is the community-standard source for these types. We chose to implement our own at approximately 100 lines of code in `Commands.cs` for three reasons: the project then has *zero* new external dependencies, the implementation is transparent and reviewable as part of the submission, and the hand-rolled `AsyncRelayCommand.IsExecuting` pattern is what drives the Place Order button's `IsEnabled` and text binding — understanding what that command does under the hood supports the viva defence better than deferring to an opaque library. Fifteen tests (`CommandsTests.cs`) cover parameter passing, canExecute gating, reentry guard, exception resilience, and null-guard constructors.

## What stayed in code-behind, and why?

Three things:

1. **The Place Order button's press animation** (`OnPlaceOrderClicked` → `PlaceOrderButton.ScaleToAsync(0.96, 80)` and back to 1.0). Animation is inherently view-scoped — it needs the XAML-named `Button` instance. The button's `Command` binding handles the business flow independently; Clicked fires alongside Command on the same tap so animation and API call run together.

2. **Entry validation on the quantity field** (`OnQuantityEntryCompleted` / `OnQuantityEntryUnfocused` → `HandleQuantityEntryAsync` → `OrderSummaryLineViewModel.TryValidateQuantity`). MAUI's `Entry` does not expose `CompletedCommand` or equivalent out of the box, so the handlers stay in code-behind and delegate into the VM's pure `TryValidateQuantity` helper for the actual validation logic.

3. **Constructing `MenuItemViewModel` with a runtime category string** (e.g. `new MenuItemViewModel(api, "Mains")`). The category is not a DI-resolvable dependency — it depends on which page was opened. The page therefore receives `IApiService` via constructor DI and instantiates the view-model explicitly. A factory interface could abstract this but would add a layer for one string.

Every *other* click handler has been moved onto an ICommand on the relevant view-model.

## How did you test the non-deterministic random pick in `HomePage`?

`LoadFeaturedAsync` does `Random.Shared.Next(_cachedMains.Count)` — non-deterministic. The tests seed the `FakeApiService` with a single-element list for each category, which collapses `Random.Shared.Next(1)` to a deterministic `0`. That lets us assert exact values (`"Chicken Burger"`, `"£8.50"`, etc.) without mocking `Random`. For the cache-reuse test we also count handler invocations per category via a closure counter — three successive `InitializeAsync` calls should produce one `main` fetch and one `dessert` fetch if caching works correctly.

## How did the test count jump from 47 to 210?

| Target | Count | Purpose |
|---|---|---|
| `OrderSummaryLineViewModelTests` | 30 | Quantity/QuantityText normalisation, LineTotal math, UpdateFrom, TryValidateQuantity, plus the three per-line command paths with and without a dialog service |
| `OrderSummaryLineSyncTests` | 12 | Seed, Add, Remove, SetQuantity (instance identity), Clear, Dispose, AddRemoveAdd same id |
| `OrderSummaryPageViewModelTests` | 23 | Ctor / Attach / Detach lifecycle, Clear / Place Order commands including confirm-accept, confirm-cancel, empty cart, happy path, null response, network exception, IsPlacingOrder toggling |
| `OrderConfirmationPresenterTests` | 4 | Full message, missing status, missing prep, whitespace status |
| `HomePageViewModelTests` | 25 | Defaults, Featured/Indulgence loaders, error paths, cache reuse, retry-after-failure, OrderState subscription updates, StartNewOrder / ContinueOrder / NavigateTo commands across empty / confirm-accept / confirm-cancel / invalid parameter |
| `MenuItemSnapshotTests` | 5 | Record value equality on each field |
| `OrderLineEntryTests` | 14 | Ctor, Quantity/Snapshot INPC, delegation, LineTotal math |
| `MenuItemCardViewModelTests` | 16 | Ctor-from-MenuItemModel, INPC, HasQuantity/QuantityText, Add/Decrease commands with and without a service |
| `MenuItemCardSyncTests` | 18 | Seed, Items Add/Remove/Reset, Cart Add/Remove/Set/Clear, orphaned ids, Dispose |
| `CommandsTests` | 15 | RelayCommand + AsyncRelayCommand full contract (parameters, canExecute, reentry, exception resilience) |
| `MenuItemSnapshotTests` + existing tests | 5 + 43 | Value-equality tests + the 47 original tests (less 4 that needed shape updates for the OrderLineEntry migration) |
| **Total** | **~210** | All green on every commit in the branch |

## Why is `OrderState` still a singleton?

Two reasons: the cart must survive page navigation (`OrderStatePersistenceTests` pin this contract — resolving `IOrderStateService` twice returns the same instance), and the brief's "Shared service/state management" (TECH-03) is explicitly singleton-shaped. Moving to a scoped or transient registration would break the cross-page persistence guarantee. The refactor does not remove the singleton — it layers view-models on top of it.

The honest smell in the singleton: it combines state ownership (`_lines`), business operations (`AddLine` with fill-if-empty, `SetQuantity`, `Clear`, `ToCreateOrderRequest`), and change notification (INPC). A textbook DDD split (`Cart` aggregate / `CartService` / `ICartRepository`) would be over-engineering for a single cart with no persistence layer. Defendable; just not a pure SRP decomposition.

## What was the scope discipline?

The refactor was delivered in five phases with a rollback anchor before each risky phase (`pre-homepage` before Phase 2, `pre-summary-vm` before Phase 4, `pre-commands` before Phase 5). Every commit runs `dotnet test` and `dotnet build` to green before the next one starts; a failing commit stops the phase rather than stacks broken state. The Phase 3 `OrderLineEntry` migration (which changes the public shape of `IOrderStateService.Lines`) is atomic — all downstream consumers and tests migrate in a single commit rather than in a half-migrated intermediate. The Windows GUI smoke test runs after every phase.

## What would you do next if you had more time?

1. **Animation through a behavior or attached property** — the one remaining click handler in the code-behind (`OnPlaceOrderClicked` → `ScaleToAsync`) could be moved to a Behavior bound to the `PlaceOrderCommand.IsExecuting` state. That would shrink the Order Summary code-behind to just the Entry validation handlers.
2. **`IMenuItemViewModelFactory`** — so the category pages can receive a factory from DI and call `factory.Create("Mains")` instead of `new MenuItemViewModel(api, "Mains")`. Formal purity over the one remaining constructor-side service locator.
3. **`Entry.CompletedCommand` via Behavior** — wraps the quantity validation handler into a pure command, removing the last two event handlers from code-behind.
4. **Property-level tests for the `CollectionChanged` adapters** — fuzz tests varying the event sequence to strengthen confidence in both syncs.
5. **`IDisposable` wiring on `MenuItemView`** — currently the MenuItemCardSync subscribes to the singleton OrderState and is only disposed when the `Items` BindableProperty changes. A `ContentView.Unloaded` hook would make the lifetime deterministic even if the containing page is destroyed.

## How "MVVM" is this project now?

Honest updated assessment: roughly **9/10** on a strict MVVM scale.

**What is MVVM in this project**:
- View-model layer exists for every stateful page and component, each with `INotifyPropertyChanged` and private-set guarded properties.
- Every meaningful user action (add, decrease, remove, increase, clear, place order, navigate) is an `ICommand` on a view-model bound from XAML, not a code-behind click handler.
- Both customer-facing pages with substantive state use `BindingContext = _vm`.
- View-models own their dialog / navigation flow via `IDialogService` / `INavigationService` abstractions, keeping `CampusCuisine.Core` free of MAUI framework types.
- Pages are registered in DI and receive their view-models through constructor injection; no page body pulls from `App.Services.GetRequiredService`.
- Sync projections preserve view-model identity across cart mutations (match-by-id incremental diff, not wholesale rebuild).
- DTO is purely a wire contract; frontend display state lives on `OrderLineEntry` + immutable `MenuItemSnapshot`.
- Compiled XAML bindings with `x:DataType`.
- 210 unit tests, mostly on view-models and sync layers, using fakes rather than mocks.

**The remaining ~1 point of honest gap**:
- One click handler survives in code-behind: the Place Order press animation. Defensible — animation is view-scoped — but strictly a click handler.
- Two Entry-event handlers stay in code-behind (`Completed`, `Unfocused`) because MAUI `Entry` has no `CompletedCommand`. Delegates to the VM's `TryValidateQuantity` for the actual logic.
- `MenuItemViewModel` is constructed inline on category pages because its category is a runtime string, not a DI-resolvable type. `IApiService` itself IS injected via constructor DI on the page.
- `OrderState` combines three responsibilities (state + ops + INPC). Defendable as cohesion; not a textbook SRP split.

None of these are accidental smells. Each is a conscious trade between architectural purity and MAUI ergonomics, and each has an honest defence if pushed.

## One-line summary

A view-model layer that owns editable state, validation, commands, dialogs, navigation, and busy-state — with MAUI framework dependencies pushed to the seam between DI-registered pages and MAUI-scoped service implementations — delivered in 24 green-building commits with 210 unit tests covering everything except the three specific view-layer hooks that MAUI ergonomics don't cleanly allow to move.
