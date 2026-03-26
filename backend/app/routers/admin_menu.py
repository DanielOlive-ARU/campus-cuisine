"""Protected admin menu CRUD routes."""

from fastapi import APIRouter, Depends, Response, status
from sqlmodel import Session

from app.core.config import get_settings
from app.core.security import require_admin_api_key
from app.db.engine import get_session
from app.models import MenuCategory
from app.schemas import MenuItemCreate, MenuItemRead, MenuItemUpdate
from app.services.menu_service import (
    create_menu_item,
    delete_menu_item,
    get_menu_item_or_404,
    list_menu_items,
    update_menu_item,
)

settings = get_settings()

router = APIRouter(
    prefix=f"{settings.api_prefix}/admin/menu-items",
    tags=["admin-menu"],
    dependencies=[Depends(require_admin_api_key)],
)


@router.get(
    "",
    response_model=list[MenuItemRead],
    summary="List menu items for admin",
    description="Return menu items for admin CRUD, including unavailable items by default.",
)
def get_admin_menu_items(
    category: MenuCategory | None = None,
    available_only: bool = False,
    session: Session = Depends(get_session),
) -> list[MenuItemRead]:
    """Return menu items for the protected admin view."""

    items = list_menu_items(
        session=session,
        category=category,
        available_only=available_only,
    )
    return [MenuItemRead.model_validate(item) for item in items]


@router.get(
    "/{item_id}",
    response_model=MenuItemRead,
    summary="Get one menu item for admin",
    description="Return one menu item for admin CRUD, including unavailable items.",
)
def get_admin_menu_item(
    item_id: int,
    session: Session = Depends(get_session),
) -> MenuItemRead:
    """Return one menu item for the protected admin view."""

    return MenuItemRead.model_validate(get_menu_item_or_404(session, item_id))


@router.post(
    "",
    response_model=MenuItemRead,
    status_code=status.HTTP_201_CREATED,
    summary="Create a menu item",
    description="Create one menu item through the protected admin API.",
)
def create_admin_menu_item(
    payload: MenuItemCreate,
    session: Session = Depends(get_session),
) -> MenuItemRead:
    """Create a menu item."""

    return MenuItemRead.model_validate(create_menu_item(session, payload))


@router.put(
    "/{item_id}",
    response_model=MenuItemRead,
    summary="Update a menu item",
    description="Patch one menu item through the protected admin API.",
)
def update_admin_menu_item(
    item_id: int,
    payload: MenuItemUpdate,
    session: Session = Depends(get_session),
) -> MenuItemRead:
    """Update a menu item."""

    return MenuItemRead.model_validate(update_menu_item(session, item_id, payload))


@router.delete(
    "/{item_id}",
    status_code=status.HTTP_204_NO_CONTENT,
    summary="Delete a menu item",
    description="Delete one menu item through the protected admin API.",
)
def delete_admin_menu_item(
    item_id: int,
    session: Session = Depends(get_session),
) -> Response:
    """Delete a menu item."""

    delete_menu_item(session, item_id)
    return Response(status_code=status.HTTP_204_NO_CONTENT)
