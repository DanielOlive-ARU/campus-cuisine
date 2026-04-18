"""Protected admin order routes."""

from fastapi import APIRouter, Depends
from sqlmodel import Session

from app.core.config import get_settings
from app.core.security import require_admin_api_key
from app.db.engine import get_session
from app.schemas import OrderLineRead, OrderRead, OrderStatusUpdate
from app.services.order_service import get_order_with_lines, update_order_status

settings = get_settings()

router = APIRouter(
    prefix=f"{settings.api_prefix}/admin/orders",
    tags=["admin-orders"],
    dependencies=[Depends(require_admin_api_key)],
)


@router.patch(
    "/{order_id}/status",
    response_model=OrderRead,
    summary="Update one order status",
    description=(
        "Admin-only. Allowed transitions: confirmed to cancelled. "
        "Same-status updates are idempotent. Reverse transitions return 400."
    ),
)
def patch_admin_order_status(
    order_id: int,
    payload: OrderStatusUpdate,
    session: Session = Depends(get_session),
) -> OrderRead:
    """Update one order status through the protected admin API."""

    update_order_status(session, order_id, payload.status)
    order, lines = get_order_with_lines(session, order_id)
    return OrderRead(
        id=order.id,
        status=order.status,
        total_items=order.total_items,
        grand_total=order.grand_total,
        created_at=order.created_at,
        items=[OrderLineRead.model_validate(line) for line in lines],
    )
