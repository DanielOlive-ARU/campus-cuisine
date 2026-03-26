"""Public order routes."""

from fastapi import APIRouter, Depends, status
from sqlmodel import Session

from app.core.config import get_settings
from app.db.engine import get_session
from app.schemas import CreateOrderRequest, OrderConfirmation, OrderLineRead, OrderRead
from app.services.order_service import create_order, get_order_with_lines

settings = get_settings()

router = APIRouter(prefix=settings.api_prefix, tags=["orders"])


@router.post(
    "/orders",
    response_model=OrderConfirmation,
    status_code=status.HTTP_201_CREATED,
    summary="Create an order",
    description="Create an order and return the backend-calculated confirmation payload.",
)
def create_order_route(
    payload: CreateOrderRequest,
    session: Session = Depends(get_session),
) -> OrderConfirmation:
    """Create a new order."""

    order = create_order(session, payload)
    return OrderConfirmation(
        id=order.id,
        status=order.status,
        total_items=order.total_items,
        grand_total=order.grand_total,
        message="Order placed successfully",
    )


@router.get(
    "/orders/{order_id}",
    response_model=OrderRead,
    summary="Get an order",
    description="Return one order with snapshot line items.",
)
def get_order_route(
    order_id: int,
    session: Session = Depends(get_session),
) -> OrderRead:
    """Return one order with its line items."""

    order, lines = get_order_with_lines(session, order_id)
    return OrderRead(
        id=order.id,
        status=order.status,
        total_items=order.total_items,
        grand_total=order.grand_total,
        created_at=order.created_at,
        items=[OrderLineRead.model_validate(line) for line in lines],
    )
