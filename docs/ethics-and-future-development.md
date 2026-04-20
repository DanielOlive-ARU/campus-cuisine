# Ethics and Future Development

This document records how ethical considerations shaped Campus Cuisine's design and implementation, how AI tooling was used during development, and where the project would extend next if work continued beyond submission.

## 1. Ethical Approach

Campus Cuisine is a student project for a university coursework brief, but it is structured as a real ordering app: a customer adds items to a cart, the backend prices and confirms the order, and the user trusts what the screen tells them. That implied trust shaped four ethical pillars - data minimisation, accessibility, security, and transparency - which were applied as design constraints rather than retro-fitted documentation.

## 2. Data Handling and Privacy

### Position
The app collects no personal data by design. There is no account system, no email capture, no profile, and no analytics. An order is a transient list of menu-item ids and quantities that becomes a confirmed order on the server with a numeric id. No user identity is attached.

### Why
The brief allows guest-style flow as a MAY item. Building authentication or analytics would have required collecting personal data the project has no defendable purpose for. The simplest ethical posture is to not collect what we do not need.

### Honest limitation
Because no account exists, an order placed cannot be associated with the user who placed it. Order history (a flagged future feature) would re-introduce a need for either an identifier or local persistence. That trade is recorded here so it is a conscious choice rather than an oversight.

## 3. Accessibility

### Position
The MAUI app uses platform-default text sizing, high-contrast brand tokens (`BrandInk` text on light brand surfaces, defined in `FrontEnd/Resources/Styles/Colors.xaml`), generous touch target sizing on the menu-card `+` and `-` buttons, and explicit visible labels (no icon-only controls).

### Why
The user base for a campus restaurant ordering app spans age, vision capability, and motor ability. Text-as-icon-only controls and low contrast are the two most common accessibility regressions in restaurant apps; both were avoided.

### Honest limitations
- The app has not been validated against a screen reader. MAUI does provide `SemanticProperties`, but they have not been audited end-to-end.
- Font scaling beyond OS-default has not been tested.
- Colour-blind safe-palette validation was a visual judgement, not a tool-validated check.

These limitations are recorded so the viva discussion of accessibility reflects what was actually verified, not what was aspired to.

## 4. Security

### Position
- Public customer endpoints (`GET /api/menu`, `POST /api/orders`) are unauthenticated by design, mirroring guest-style ordering.
- Admin endpoints (`POST/PUT/DELETE /api/admin/menu-items`, `PATCH /api/admin/orders/{id}/status`) require an `x-admin-key` header, validated as a router-level FastAPI dependency.
- All write endpoints validate inputs with Pydantic / SQLModel schemas: `extra = "forbid"`, positive prices, positive quantities, non-blank names, known menu-item ids.
- Server is the price authority. The frontend cannot influence totals - it submits item ids and quantities, the backend re-prices.

### Trade-off recorded honestly
The `x-admin-key` model is a single shared secret, not a multi-user RBAC system. It is enough to separate public from admin flows in a student project demo, but it would be inadequate for production. The decision is documented in [docs/api/api-contract.md](api/api-contract.md) and is defendable on three grounds: it is appropriate to the brief's scope, it is verifiable in the OpenAPI docs at `/docs`, and it is replaceable later (one dependency function would change).

### Server-as-price-authority
This is an ethical choice as much as a technical one. The user sees totals on the order summary screen, but those numbers are recomputed by the backend on `POST /api/orders` and the confirmation message returns the server's totals. If a malicious or buggy client submitted manipulated prices, the server would override them. This protects the user from a tampered checkout.

## 5. User Trust

### Position
- Destructive actions (remove a line, clear the order) require confirmation through `IDialogService.ConfirmAsync`. The dialogs name the action and give a Cancel option as the destructive default.
- The Place Order button briefly animates on tap (`ScaleToAsync` 80ms in / 80ms out) so the user gets immediate tactile feedback that the tap was received before the network round-trip.
- The Place Order button text and `IsEnabled` are driven by `OrderSummaryPageViewModel.IsPlacingOrder`, so a user cannot double-submit the same order.
- Order confirmation surfaces three values from the backend response: order id, status, estimated preparation time. This is the user's receipt.

### Why
Restaurant ordering carries real user expectations. An accidentally cleared cart, a double-charged order, or a "did my tap register?" moment are all common pain points. Each has a deliberate guard.

## 6. Transparency

### Position
- API failures are surfaced to the user with a readable message, not silently swallowed. `ApiService` throws `ApiException` with HTTP-status-aware messages (see `CampusCuisine.Core/Services/ApiException.cs` and `ApiService.cs`).
- Backend-down on a category page renders a visible error region rather than an empty list (originally surfaced by Adam in commit `106bb80`).
- The offline cache (`CachedApiService` decorator) is deliberately silent: when the cache rescues a failed fetch the user sees the menu as normal. There is no fake "offline mode" banner. The user only ever sees a degraded experience if no cached data is available.

### Why this is the right transparency model
A banner on every cached load would be honest but disruptive. The current model is: be loud about failures the user must act on (place-order failure), be quiet about resilience the user does not need to act on (cached category list). The honest counter-position is that an "offline" indicator could help a user understand stale data; that is recorded in the future-development list below.

## 7. Use of AI During Development

This section is included because the assessment rubric explicitly asks for it across multiple viva criteria.

### Where AI tooling was used
- **Documentation drafting**: viva notes, this ethics document, the requirements mapping table, README polish, the decision log structure. AI-generated drafts were reviewed line-by-line by Dan and edited for accuracy against the actual code.
- **Refactor planning**: the MVVM refactor was scoped through AI dialogue (sequence of phases, rollback anchors, test strategy) before any code changed. The plan was committed as `docs/viva/frontend-mvvm-refactor-notes.md` and traced commit-by-commit in [docs/meetings/decision-log.md](meetings/decision-log.md).
- **Code generation for low-risk patterns**: hand-rolled `RelayCommand` / `AsyncRelayCommand` types, `OrderConfirmationPresenter` formatting helper, and a portion of the unit-test scaffolding for the new view-model layer. Each generated unit was verified by `dotnet build` + `dotnet test` to green before it was kept.
- **Bug diagnosis assistance**: the Order Summary Shell navigation bug (documented in [docs/reflection/development-reflection.md](reflection/development-reflection.md)) was investigated through AI dialogue alongside manual reproduction. The fix - treating Order Summary as a top-level Shell route - was the human conclusion after manually narrowing the trigger.
- **Documentation consistency review**: AI-driven cross-reads of README, requirements-mapping, test-plan, and decision-log surfaced stale numbers and missing references that were then corrected by hand.

### Where AI tooling was not used
- API contract design (`docs/api/api-contract.md`) was authored by Dan from the brief.
- The decision to defer payments, full authentication, and order history was Dan's scope call, recorded in [docs/project-management/backlog-and-milestones.md](project-management/backlog-and-milestones.md) and the decision log.
- Manual Windows GUI smoke testing after every refactor phase was done by Dan, not by AI.
- Team coordination, when-to-merge, and what risks were acceptable for the deadline were decided by the team.

### How AI outputs were evaluated
1. Every AI-suggested code change was committed as a separate, green-building commit so it could be reverted in isolation. The `pre-homepage`, `pre-summary-vm`, and `pre-commands` git tags mark known-good rollback anchors before each riskier phase.
2. `dotnet build` and `dotnet test` ran on every commit. A failing commit blocked the next one rather than stacking broken state.
3. Architectural suggestions were traced through the existing code base before acceptance, not adopted on trust.
4. Documentation drafts were checked against the actual code. Where they did not match, the documentation was edited rather than the code reshaped to match the draft.

### What stayed human
Architectural trade-offs (singleton `OrderState` vs full DDD split, pragmatic MVVM vs pure MVVM, prep time confirmation-only vs persisted), scope decisions (what to ship vs defer), submission readiness judgements, and the viva narrative.

## 8. Future Development

The list below is ordered by a deliberate combination of *user value*, *implementation cost*, and *demonstrability in viva*. Each item carries the reason it sits where it sits.

### Priority 1 - High value, low cost, near-immediate demoability
**Order history page** - pairs with the existing `GET /api/orders/{id}` endpoint. Needs a `GET /api/orders` listing route (small backend change) and a History page in the flyout. Unlocks the MAY items "view past orders" and "reorder from history" together. First because it is the largest single rubric move per implementation hour.

**Promo code / discount field** - frontend is a single Entry on Order Summary; backend gains a discount calculation in the order pricing path. Small surface, defendable, and the viva can show before/after totals visibly.

### Priority 2 - High value but architecturally weighty
**Richer order-status workflow** - currently confirmed -> cancelled only. A real workflow (received -> preparing -> ready -> collected, or similar) would need a status-transition table on the Order entity, a richer admin UI or admin API, and a customer-side polling or push update mechanism. This is the right next infrastructure investment but it is not a one-day item.

**Authentication / accounts** - touches data handling (re-introduces personal data), security (password hashing, session management, RBAC), persistence (user table, order ownership), and every page that currently assumes guest flow. High-value because it unlocks ordering history, payment, and admin separation, but it is a foundational change.

### Priority 3 - Higher cost or compliance surface
**Payment sandbox integration (Stripe / equivalent)** - depends on having real authentication and a real persisted order with status transitions; without those the payment is decorative. PCI considerations apply even in sandbox mode.

**Real-time kitchen workflow** - websockets or push, plus a separate kitchen UI surface. Useful only when the order-status workflow above is live.

**Analytics dashboard** - useful for the operator but introduces data-handling questions: aggregate vs personal, retention, opt-out. Should not be added until the data-minimisation posture is consciously revisited.

**Multi-branch restaurant support** - schema change (branch_id on menu and order) plus a per-branch admin model. Useful if the project moves beyond one-restaurant scope.

### Priority 4 - Production polish
**Staged GitHub Actions pipeline** that runs tests, then on green builds Windows and Android assessment artifacts. Documented as a stretch goal in [docs/project-management/ci-cd-notes.md](project-management/ci-cd-notes.md) and currently held back to control GitHub Actions usage costs during the academic submission window.

**Android image-loading fix** - the current limitation is documented in the README. Solvable by serving images from the same origin the Android emulator can reach, or by hosting images on a static CDN.

**Accessibility hardening pass** - screen-reader audit, font-scaling validation, colour-contrast tooling.

## 9. Reflection Prompts

These remain useful for the viva and for any follow-up reflection assignment.

- Which design decisions reduced risk the most? *(Server-as-price-authority and the singleton `OrderState` covered most of the cross-page consistency and security surface in two design choices.)*
- Which integration caused the biggest challenge? *(The Order Summary Shell navigation bug - documented in development-reflection.md - was the largest single diagnostic effort.)*
- How did the team manage division and coordination? *(See the LO5 evidence in commit history and `docs/project-management/`.)*
- Which SHOULD feature added the most value for the effort? *(Offline menu browsing - one decorator and a Preferences cache - delivered a visibly different user experience for very little code.)*
- What would you improve first if work continued? *(Order history; see Priority 1 above.)*
