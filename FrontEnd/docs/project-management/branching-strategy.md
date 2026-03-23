# Suggested Branching Strategy

## Branches
- `main` – stable submission-ready branch
- `develop` – optional shared integration branch
- `feature/backend-*` – Dan feature branches
- `feature/frontend-*` – Adam feature branches
- `docs/*` – documentation updates if preferred

## Workflow
1. Create an issue.
2. Create a branch linked to that issue.
3. Commit little and often.
4. Open a pull request or perform peer review informally if time is tight.
5. Merge only after the feature works and does not break the app.

## Commit Message Suggestions
- `feat(api): add menu category filter`
- `feat(frontend): add order summary bar`
- `test(order): cover remove item edge case`
- `docs(requirements): map MUST requirements`
- `fix(summary): prevent negative quantity`

## Good Practice Notes
- Avoid one large deadline-day upload.
- Link commits and pull requests to issues where possible.
- Keep documentation updated as development progresses.
