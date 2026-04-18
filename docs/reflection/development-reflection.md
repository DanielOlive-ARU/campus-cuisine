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

## Use of AI Assistance

AI assistance was used to help diagnose the navigation bug, compare implementation options, and draft documentation. The final decisions were based on manual testing, code inspection, and project requirements rather than accepting suggestions uncritically.
