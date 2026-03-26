"""Order creation and lookup helpers."""

from __future__ import annotations

from fastapi import HTTPException, status
from sqlmodel import Session, select

from app.models import MenuItem, Order, OrderLine, OrderStatus
from app.schemas import CreateOrderRequest


def create_order(session: Session, payload: CreateOrderRequest) -> Order:
    """Create an order and its snapshot line items."""

    requested_item_ids = [item.menu_item_id for item in payload.items]
    menu_items = {
        item.id: item
        for item in session.exec(
            select(MenuItem).where(MenuItem.id.in_(set(requested_item_ids)))
        ).all()
        if item.id is not None
    }

    missing_ids = sorted(set(requested_item_ids) - set(menu_items))
    if missing_ids:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=f"Unknown menu item ids: {', '.join(str(item_id) for item_id in missing_ids)}",
        )

    unavailable_ids = sorted(
        item_id
        for item_id in set(requested_item_ids)
        if not menu_items[item_id].is_available
    )
    if unavailable_ids:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=(
                "Unavailable menu item ids: "
                + ", ".join(str(item_id) for item_id in unavailable_ids)
            ),
        )

    order_lines: list[OrderLine] = []
    total_items = 0
    grand_total = 0.0

    for requested_item in payload.items:
        menu_item = menu_items[requested_item.menu_item_id]
        line_total = round(menu_item.price * requested_item.quantity, 2)
        total_items += requested_item.quantity
        grand_total = round(grand_total + line_total, 2)

        order_lines.append(
            OrderLine(
                menu_item_id=requested_item.menu_item_id,
                name_snapshot=menu_item.name,
                unit_price_snapshot=menu_item.price,
                quantity=requested_item.quantity,
                line_total=line_total,
            )
        )

    order = Order(
        status=OrderStatus.CONFIRMED.value,
        total_items=total_items,
        grand_total=grand_total,
    )
    session.add(order)
    session.flush()

    for line in order_lines:
        line.order_id = order.id
        session.add(line)

    session.commit()
    session.refresh(order)
    return order


def get_order_with_lines(session: Session, order_id: int) -> tuple[Order, list[OrderLine]]:
    """Return one order and its line items or raise a 404 error."""

    order = session.get(Order, order_id)
    if order is None:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Order not found",
        )

    lines = list(
        session.exec(
            select(OrderLine)
            .where(OrderLine.order_id == order_id)
            .order_by(OrderLine.id)
        ).all()
    )
    return order, lines
