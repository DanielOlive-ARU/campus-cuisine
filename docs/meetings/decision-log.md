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

This has been recorded as a technical-debt item in `docs/project-management/backlog-and-milestones.md` and as Issue 31 in `docs/project-management/github-issues-seed.md`.

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

This has been recorded as a technical-debt item in `docs/project-management/backlog-and-milestones.md` and as Issue 32 in `docs/project-management/github-issues-seed.md`.

### Summary Bar and Shared State Hardening

#### Context
The frontend previously implemented the category summary bar behaviour directly inside the Starters, Mains, and Desserts pages, and it used the concrete `OrderState` service registered in MAUI DI. The duplicated summary bar UI has now been refactored into a reusable `OrderSummaryBar`, and the order-state DI has now been hardened with `IOrderStateService` over the same shared singleton instance.

#### Decision
Stabilise the branch first, then complete the reusable `OrderSummaryBar` refactor and the `IOrderStateService` hardening step once the frontend core/test checkpoint is in place.

#### Why
The reusable summary bar improves alignment with the reusable UI component requirement and removes duplicated UI logic from the three category pages without altering the user journey. `IOrderStateService` improves DI clarity and testability without changing runtime behaviour, because the interface resolves to the same underlying singleton `OrderState` instance.

#### Current State
The reusable summary bar and `IOrderStateService` hardening have both been completed on `submission-hardening-and-testing`.
