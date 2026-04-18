"""Order service helper tests."""

import pytest
from fastapi import HTTPException
from sqlmodel import Session

from app.db.engine import get_engine
from app.models import OrderStatus
from app.schemas import CreateOrderRequest
from app.services.order_service import (
    create_order,
    estimate_prep_minutes,
    update_order_status,
)


def test_estimate_prep_minutes_returns_minimum_for_smallest_valid_order() -> None:
    assert estimate_prep_minutes(total_items=1, distinct_lines=1) == 10


def test_estimate_prep_minutes_returns_expected_mid_order_value() -> None:
    assert estimate_prep_minutes(total_items=3, distinct_lines=2) == 15


def test_estimate_prep_minutes_clamps_large_orders_to_ceiling() -> None:
    assert estimate_prep_minutes(total_items=20, distinct_lines=10) == 35


def test_estimate_prep_minutes_counts_duplicate_menu_items_as_one_distinct_line() -> None:
    distinct_lines = len({item_id for item_id in [1, 1]})
    assert estimate_prep_minutes(total_items=2, distinct_lines=distinct_lines) == 12


def test_update_order_status_cancels_confirmed_order(seeded_client) -> None:
    with Session(get_engine()) as session:
        order, _ = create_order(
            session,
            CreateOrderRequest(items=[{"menu_item_id": 1, "quantity": 1}]),
        )

        updated_order = update_order_status(session, order.id, OrderStatus.CANCELLED)

        assert updated_order.status == OrderStatus.CANCELLED.value


def test_update_order_status_returns_same_order_for_same_status(seeded_client) -> None:
    with Session(get_engine()) as session:
        order, _ = create_order(
            session,
            CreateOrderRequest(items=[{"menu_item_id": 1, "quantity": 1}]),
        )

        updated_order = update_order_status(session, order.id, OrderStatus.CONFIRMED)

        assert updated_order.status == OrderStatus.CONFIRMED.value


def test_update_order_status_rejects_invalid_transition(seeded_client) -> None:
    with Session(get_engine()) as session:
        order, _ = create_order(
            session,
            CreateOrderRequest(items=[{"menu_item_id": 1, "quantity": 1}]),
        )
        update_order_status(session, order.id, OrderStatus.CANCELLED)

        with pytest.raises(HTTPException) as exc_info:
            update_order_status(session, order.id, OrderStatus.CONFIRMED)

        assert exc_info.value.status_code == 400
        assert exc_info.value.detail == "Transition from current status is not allowed"


def test_update_order_status_returns_404_for_unknown_order(seeded_client) -> None:
    with Session(get_engine()) as session:
        with pytest.raises(HTTPException) as exc_info:
            update_order_status(session, 999, OrderStatus.CANCELLED)

        assert exc_info.value.status_code == 404
        assert exc_info.value.detail == "Order not found"
