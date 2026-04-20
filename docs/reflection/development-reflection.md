# Development Reflection

This document records short reflective notes from development. It is intended to support the final report and viva by linking issues, decisions, testing evidence, and learning.

## Order Summary Navigation Bug

### What Happened
During manual testing, the Order Summary page opened correctly from menu pages, but returning through the flyout sometimes failed. A typical case was:

1. Open Mains.
2. Tap Order Summary.
3. Open the flyout.
4. Tap Mains.
5. The app remained on Order Summary.

Further manual testing showed that the issue followed whichever page had opened Order Summary. After reproducing it from Desserts, Desserts could also appear to be stuck behind Order Summary.

### Diagnosis
The issue was not caused by the backend, order totals, or the Clear Order button. It was caused by how MAUI Shell navigation was being used. Order Summary was opened as a pushed route over the current Shell item, so Shell still considered the previous flyout item selected.

### Decision
We decided to treat Order Summary as a primary Shell/Flyout destination. The page should have an explicit Shell route and be opened with absolute Shell navigation, for example:

```csharp
await Shell.Current.GoToAsync("//OrderSummaryPage");
```

This makes Order Summary behave like the primary page required by the brief rather than like a child detail page.

### Learning
The page opening successfully was not enough to prove navigation was correct. We needed to test complete user journeys, including navigating away after editing or clearing an order. This also showed the value of manual exploratory testing alongside automated tests.

## Dependency Injection Requirement Review

### What Happened
The course leader explicitly confirmed that dependency injection must be included in the solution. We reviewed whether the project already met that requirement before making further frontend changes.

### Findings
The project already uses dependency injection:

- The MAUI frontend registers `IApiService`/`ApiService` with `AddHttpClient`.
- The MAUI frontend registers `OrderState` as a singleton shared service.
- The backend uses FastAPI `Depends(...)` for database sessions and protected admin routes.

The frontend currently resolves some services through the application service provider. This is DI-backed, although it is not as strict as constructor injection throughout every page.

### Decision
We decided not to perform a broad DI refactor during the current patch slices. The existing DI approach satisfies the requirement and is stable enough for the current milestone. We will document the design clearly and consider constructor-injection hardening later if time allows.

### Learning
The review clarified the difference between using a DI container and applying the strictest possible constructor-injection pattern. For this coursework, the important point is that services are registered centrally, shared state is managed through DI, and backend dependencies are injected per request.

## MVVM Refactor (Post-Submission)

### What Happened
After the submission branch merged to `main`, we held a deferred `QuantityText`-on-`OrderLineDto` item in the hardening backlog and a broader "tighten MVVM" goal for the viva defence of LO3. Rather than defend the submitted "pragmatic MVVM" verbally, we took a dedicated branch (`mvvm-ordersummary-refactor`) off `main` and delivered the refactor across twenty-four green-building commits in five phases: Order Summary, HomePage, an `OrderLineEntry` cascade that also cleaned up `MenuItemView`, an `OrderSummaryPageViewModel` extraction so both stateful pages share the same `BindingContext = _vm` pattern, and a Commands + constructor-DI migration that closed the remaining MVVM gaps. The branch was merged into `main` via PR #11 (commit `a6ccbf5`) on 2026-04-19 once all five phases passed Windows GUI smoke testing.

### Diagnosis
A pre-refactor audit identified three concrete smells beyond the known `QuantityText` issue. `OrderSummaryPage.xaml.cs` was 263 lines mixing validation, dialogs, busy-state, navigation, and animation. `HomePage.xaml.cs` was 358 lines with 13 observable properties baked onto the page (`BindingContext = this`). `MenuItemView.xaml.cs` rebuilt its `DisplayItems` collection wholesale on every cart change, losing scroll position and focus. All four were defendable individually but together weakened the LO3 defence.

### Decision
Each phase was split into green-building commits so bisection would be clean and any single slice could be rolled back without losing the rest. The `OrderLineEntry` cascade (Phase 3) was the riskiest because it changed the public shape of `IOrderStateService.Lines`. It was delivered atomically in one commit with all downstream consumers and tests migrated together, rather than in a half-migrated intermediate state. A `pre-homepage` tag at commit `96decae` was created before Phase 3 as a rollback anchor, and branch protection was preserved by force-pushing with `--force-with-lease` only where commit-message rewrites (stripping an earlier `Co-Authored-By` trailer) required it.

### Testing
The test suite grew from `47` to `210` green xUnit tests across the refactor — full per-target breakdown is in [docs/viva/frontend-mvvm-refactor-notes.md](../viva/frontend-mvvm-refactor-notes.md). The 47 pre-existing tests all continued to pass unchanged because `OrderLineEntry` exposes the same `Name`/`Description`/`UnitPrice`/`Quantity` members the old DTO did, so any consumer that only read those fields saw an unchanged contract. `dotnet build` on the Windows MAUI target produced 0 warnings and 0 errors on every commit. Windows GUI smoke testing confirmed the end-to-end order flow at every phase boundary.

### Learning
The most load-bearing design decision was making `MenuItemSnapshot` an immutable record. Because the snapshot cannot be mutated and is value-equal, `OrderLineEntry` can freely delegate `Name`/`Description`/`UnitPrice` through it and the 47 pre-existing tests passed without any assertion changes — the Entry looked like the old DTO to any consumer that only read those fields. The second lesson was that wholesale `ObservableCollection.Clear()` + re-add patterns are genuinely harmful in MAUI binding scenarios, not just aesthetically ugly: the `MenuItemView` rebuild was directly responsible for the scroll-position regression before this refactor. A match-by-id incremental diff fixes both issues in one pass and the pattern generalises (`OrderSummaryLineSync` and `MenuItemCardSync` share the same approach).

### Safety Discipline
The refactor was held on a parallel branch through Phase 1 to keep `main` as a safe fallback during the riskiest cascade (the `OrderLineEntry` migration that changed the public shape of `IOrderStateService.Lines`). After all five phases passed Windows GUI smoke testing and 210 tests stayed green, the branch was merged to `main` via PR #11 (commit `a6ccbf5`) on 2026-04-19. The rollback anchor tags (`pre-homepage`, `pre-summary-vm`, `pre-commands`) were pushed to origin and retained, so the pre-refactor architecture remains accessible for diff comparison from any clone.

## Use of AI Assistance

AI assistance was used to help diagnose the navigation bug, compare implementation options, and draft documentation. The final decisions were based on manual testing, code inspection, and project requirements rather than accepting suggestions uncritically.
