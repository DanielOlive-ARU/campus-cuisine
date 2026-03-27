"""Phase 3 schema validation tests."""

from datetime import datetime, timezone

import pytest
from pydantic import ValidationError

from app.models import MenuCategory, MenuItem, OrderLine, OrderStatus
from app.schemas import (
    CreateOrderRequest,
    MenuItemCreate,
    MenuItemRead,
    MenuItemUpdate,
    OrderLineRead,
)


def test_menu_item_create_normalizes_text_fields() -> None:
    payload = MenuItemCreate(
        name="  Grilled   Chicken   Burger  ",
        description="  Chargrilled chicken with lettuce.  ",
        category=MenuCategory.MAIN,
        price=8.99,
        image_url="  /images/grilled-chicken-burger.jpg  ",
    )

    assert payload.name == "Grilled Chicken Burger"
    assert payload.description == "Chargrilled chicken with lettuce."
    assert payload.image_url == "/images/grilled-chicken-burger.jpg"


def test_menu_item_create_rejects_blank_name() -> None:
    with pytest.raises(ValidationError):
        MenuItemCreate(
            name="   ",
            description="Valid description",
            category=MenuCategory.MAIN,
            price=8.99,
            image_url="/images/item.jpg",
        )


def test_menu_item_create_rejects_non_positive_price() -> None:
    with pytest.raises(ValidationError):
        MenuItemCreate(
            name="Burger",
            description="Valid description",
            category=MenuCategory.MAIN,
            price=0,
            image_url="/images/item.jpg",
        )


def test_menu_item_create_rejects_invalid_image_path() -> None:
    with pytest.raises(ValidationError):
        MenuItemCreate(
            name="Burger",
            description="Valid description",
            category=MenuCategory.MAIN,
            price=8.99,
            image_url="http://localhost:8000/images/item.jpg",
        )


def test_menu_item_update_allows_partial_payload() -> None:
    payload = MenuItemUpdate(name="  Classic   Cheesecake ")

    assert payload.name == "Classic Cheesecake"
    assert payload.description is None
    assert payload.category is None


def test_menu_item_request_rejects_unknown_fields() -> None:
    with pytest.raises(ValidationError):
        MenuItemCreate(
            name="Burger",
            description="Valid description",
            category=MenuCategory.MAIN,
            price=8.99,
            image_url="/images/item.jpg",
            unexpected=True,
        )


def test_order_request_rejects_empty_items() -> None:
    with pytest.raises(ValidationError):
        CreateOrderRequest(items=[])


def test_order_item_rejects_zero_quantity() -> None:
    with pytest.raises(ValidationError):
        CreateOrderRequest(items=[{"menu_item_id": 1, "quantity": 0}])


def test_menu_item_read_maps_from_model() -> None:
    item = MenuItem(
        id=1,
        name="Grilled Chicken Burger",
        description="Chargrilled chicken fillet with lettuce and house sauce.",
        category=MenuCategory.MAIN.value,
        price=8.99,
        image_url="/images/grilled-chicken-burger.jpg",
        is_available=True,
    )

    result = MenuItemRead.model_validate(item)

    assert result.id == 1
    assert result.category == MenuCategory.MAIN


def test_order_line_read_maps_snapshot_fields() -> None:
    line = OrderLine(
        id=1,
        order_id=10,
        menu_item_id=7,
        name_snapshot="Chocolate Brownie",
        unit_price_snapshot=3.99,
        quantity=2,
        line_total=7.98,
    )

    result = OrderLineRead.model_validate(line)

    assert result.name == "Chocolate Brownie"
    assert result.unit_price == 3.99


def test_order_status_enum_uses_confirmed_value() -> None:
    assert OrderStatus.CONFIRMED.value == "confirmed"

