# Wireframe Notes

These notes can be used to create simple Figma, Excalidraw, Balsamiq, or hand-drawn wireframes.

## 1. Welcome Page
### Main Sections
- Restaurant logo/name at the top
- Short welcome text / restaurant description
- Order status card showing:
  - total items
  - current total price
- Primary CTA: **Start New Order**
- Secondary CTA: **Continue Current Order** (only visible if order exists)
- Optional promotional banner / featured dish card

### UX Notes
- Keep this page visually clean and branded
- Use strong heading and friendly restaurant language
- Make the two main CTA buttons obvious and accessible

## 2. Main Course Page
### Main Sections
- Page title: Main Courses
- Scrollable list/grid of dish cards
- Each dish card includes:
  - image
  - dish name
  - description
  - price
  - add / increase / decrease controls
- Sticky order summary bar at bottom

### UX Notes
- Card layout should prioritise readability
- Quantity controls should be easy to tap on mobile
- Summary bar should always show latest totals

## 3. Dessert / Appetizer Page
### Main Sections
- Page title: Desserts / Appetizers
- Same layout pattern as Main Course page
- Distinct accent colour theme from mains page
- Sticky order summary bar at bottom

### UX Notes
- Reuse the same component structure for consistency
- Different colour theme helps users recognise category change

## 4. Order Summary Page
### Main Sections
- Page title: Order Summary
- Empty state panel when no items exist
- List of order line items showing:
  - name
  - unit price
  - quantity control
  - line total
  - remove action
- Grand total section
- Buttons:
  - Clear Order
  - Place Order

### UX Notes
- Destructive actions must ask for confirmation
- Place Order button should be visually dominant when order exists
- Empty state should guide user back to menu pages

## 5. Optional Help Page
### Main Sections
- Short explanation of app usage
- Contact/support note
- FAQ style content

## Shared Component Notes
### DishCard Component
- image left/top
- name and price prominent
- description smaller
- quantity buttons grouped clearly

### OrderSummaryBar Component
- total items
- total price
- “View Order” indicator/button
- pinned to bottom of category pages
