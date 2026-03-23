"""Persistence models for orders."""

from datetime import datetime, timezone
from enum import Enum

from sqlmodel import Field, SQLModel


class OrderStatus(str, Enum):
    """Supported order states."""

    CONFIRMED = "confirmed"
    CANCELLED = "cancelled"


def utc_now() -> datetime:
    """Return the current UTC timestamp."""

    return datetime.now(timezone.utc)


class Order(SQLModel, table=True):
    """Database table for orders."""

    id: int | None = Field(default=None, primary_key=True)
    status: str = Field(default=OrderStatus.CONFIRMED.value, index=True, max_length=20)
    total_items: int = Field(default=0, ge=0)
    grand_total: float = Field(default=0.0, ge=0)
    created_at: datetime = Field(default_factory=utc_now)


class OrderLine(SQLModel, table=True):
    """Database table for order line items."""

    id: int | None = Field(default=None, primary_key=True)
    order_id: int = Field(foreign_key="order.id", index=True)
    menu_item_id: int = Field(index=True)
    name_snapshot: str = Field(max_length=100)
    unit_price_snapshot: float = Field(ge=0)
    quantity: int = Field(gt=0)
    line_total: float = Field(ge=0)
