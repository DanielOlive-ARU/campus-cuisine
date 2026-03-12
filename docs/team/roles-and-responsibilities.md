# Roles and Responsibilities

## Team Members
- **Dan** – Backend lead
- **Adam** – Frontend lead

## Responsibility Split

### Dan – Backend Lead
Dan owns the backend architecture and API delivery.

#### Primary Responsibilities
- FastAPI project setup
- Router/service/repository structure
- SQLite schema and persistence
- Menu retrieval endpoints
- Admin CRUD endpoints for food items
- Order creation and order status tracking endpoints
- Validation and sanitisation rules
- Optional basic protection for admin endpoints
- Backend tests
- Backend setup/run documentation

#### Viva Focus
- Why FastAPI was selected
- How REST endpoints map to brief requirements
- How validation protects data integrity
- How CRUD satisfies “add food items without changing code”
- How backend tests validate core rules

### Adam – Frontend Lead
Adam owns the mobile app user experience and state-driven UI.

#### Primary Responsibilities
- MAUI Shell flyout navigation
- XAML page layouts
- MVVM structure and ViewModels
- Welcome page, Main Course page, Dessert page, Order Summary page
- Reusable UI components
- App branding, styles, and page themes
- Shared order state service integration
- Frontend tests for order logic/service mapping
- Wireframes and UI screenshots

#### Viva Focus
- Why MAUI Shell and MVVM were selected
- How flyout navigation satisfies the brief
- How shared order state persists during navigation
- How reusable components reduce duplication
- How UI design supports usability

## Shared Responsibilities
Both members are expected to contribute to:
- Requirements analysis
- GitHub project planning
- Documentation updates
- Meeting notes
- Integration debugging
- End-to-end testing
- README finalisation
- Reflection, ethics, and future development notes
- Viva preparation

## Collaboration Rules
1. Agree API contracts before major implementation begins.
2. Create issues before starting substantial tasks.
3. Use feature branches and reviewed pull requests where possible.
4. Record decisions in meeting notes and the decision log.
5. Commit frequently rather than in one bulk upload.

## Review Expectations
- Dan reviews frontend integration assumptions that affect API payloads.
- Adam reviews backend payload shapes from the perspective of UI needs.
- Both members verify that implementation still matches the requirements map.

## Commit Expectations
Each member should show regular contributions in:
- code
- documentation
- testing
- planning/evidence

This is important for LO5 collaboration evidence and for the viva.
