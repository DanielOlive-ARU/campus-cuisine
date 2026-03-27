"""Request and response schemas for orders."""

from __future__ import annotations

from datetime import datetime

from pydantic import BaseModel, ConfigDict, Field, field_validator, model_validator

from app.models.order import OrderStatus


class OrderSchemaBase(BaseModel):
    """Base settings shared by order response schemas."""

    model_config = ConfigDict(from_attributes=True)


class OrderRequestBase(BaseModel):
    """Base settings shared by order request schemas."""

    model_config = ConfigDict(extra="forbid")


class OrderItemCreate(OrderRequestBase):
    """One order line requested by the client."""

    menu_item_id: int
    quantity: int

    @field_validator("quantity")
    @classmethod
    def validate_quantity(cls, value: int) -> int:
        if value <= 0:
            raise ValueError("Quantity must be greater than 0.")
        return value


class CreateOrderRequest(OrderRequestBase):
    """Order creation request body."""

    items: list[OrderItemCreate]

    @model_validator(mode="after")
    def validate_items(self) -> "CreateOrderRequest":
        if not self.items:
            raise ValueError("Order must include at least one item.")
        return self


class OrderLineRead(OrderSchemaBase):
    """Order line response schema."""

    menu_item_id: int
    name: str = Field(validation_alias="name_snapshot")
    unit_price: float = Field(validation_alias="unit_price_snapshot")
    quantity: int
    line_total: float


class OrderRead(OrderSchemaBase):
    """Full order response schema."""

    id: int
    status: OrderStatus
    total_items: int
    grand_total: float
    created_at: datetime
    items: list[OrderLineRead]


class OrderConfirmation(OrderSchemaBase):
    """Order confirmation response schema."""

    id: int
    status: OrderStatus
    total_items: int
    grand_total: float
    message: str
