"""Public menu routes."""

from fastapi import APIRouter, Depends, HTTPException, status
from sqlmodel import Session

from app.core.config import get_settings
from app.db.engine import get_session
from app.models import MenuCategory
from app.schemas import MenuItemRead
from app.services.menu_service import get_menu_item_or_404, list_menu_items

settings = get_settings()

router = APIRouter(prefix=settings.api_prefix, tags=["menu"])


@router.get(
    "/menu",
    response_model=list[MenuItemRead],
    summary="List menu items",
    description="Return menu items, filtered by category and availability.",
)
def get_menu_items(
    category: MenuCategory | None = None,
    available_only: bool = True,
    session: Session = Depends(get_session),
) -> list[MenuItemRead]:
    """Return menu items for the public app."""

    items = list_menu_items(
        session=session,
        category=category,
        available_only=available_only,
    )
    return [MenuItemRead.model_validate(item) for item in items]


@router.get(
    "/menu/{item_id}",
    response_model=MenuItemRead,
    summary="Get one menu item",
    description="Return one public menu item by id.",
)
def get_menu_item(
    item_id: int,
    session: Session = Depends(get_session),
) -> MenuItemRead:
    """Return one public menu item by id."""

    item = get_menu_item_or_404(session, item_id)
    if not item.is_available:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Menu item not found",
        )
    return MenuItemRead.model_validate(item)
