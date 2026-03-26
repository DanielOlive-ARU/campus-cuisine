"""Menu retrieval and CRUD service helpers."""

from __future__ import annotations

from fastapi import HTTPException, status
from sqlmodel import Session, select

from app.models import MenuCategory, MenuItem
from app.models.menu_item import utc_now
from app.schemas import MenuItemCreate, MenuItemUpdate


def list_menu_items(
    session: Session,
    category: MenuCategory | str | None = None,
    available_only: bool = True,
) -> list[MenuItem]:
    """Return menu items with optional category and availability filters."""

    statement = select(MenuItem)

    if category is not None:
        category_value = category.value if isinstance(category, MenuCategory) else category
        statement = statement.where(MenuItem.category == category_value)

    if available_only:
        statement = statement.where(MenuItem.is_available.is_(True))

    statement = statement.order_by(MenuItem.id)
    return list(session.exec(statement).all())


def get_menu_item_or_404(session: Session, item_id: int) -> MenuItem:
    """Return one menu item or raise a 404 error."""

    statement = select(MenuItem).where(MenuItem.id == item_id)
    item = session.exec(statement).first()
    if item is None:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Menu item not found",
        )
    return item


def create_menu_item(session: Session, payload: MenuItemCreate) -> MenuItem:
    """Create and persist one menu item."""

    item = MenuItem(**_menu_payload_values(payload))
    session.add(item)
    session.commit()
    session.refresh(item)
    return item


def update_menu_item(
    session: Session,
    item_id: int,
    payload: MenuItemUpdate,
) -> MenuItem:
    """Patch a menu item and return the saved model."""

    item = get_menu_item_or_404(session, item_id)
    for field_name, value in _menu_payload_values(payload).items():
        setattr(item, field_name, value)

    item.updated_at = utc_now()
    session.add(item)
    session.commit()
    session.refresh(item)
    return item


def delete_menu_item(session: Session, item_id: int) -> None:
    """Delete a menu item or raise a 404 error."""

    item = get_menu_item_or_404(session, item_id)
    session.delete(item)
    session.commit()


def _menu_payload_values(payload: MenuItemCreate | MenuItemUpdate) -> dict[str, object]:
    """Return schema values in a form suitable for SQLModel persistence."""

    values = payload.model_dump(exclude_unset=True)
    category = values.get("category")
    if isinstance(category, MenuCategory):
        values["category"] = category.value
    return values
