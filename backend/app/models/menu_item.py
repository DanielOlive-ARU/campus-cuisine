"""Persistence model for menu items."""

from datetime import datetime, timezone
from enum import Enum

from sqlmodel import Field, SQLModel


class MenuCategory(str, Enum):
    """Supported menu categories."""

    MAIN = "main"
    DESSERT = "dessert"
    APPETIZER = "appetizer"


def utc_now() -> datetime:
    """Return the current UTC timestamp."""

    return datetime.now(timezone.utc)


class MenuItem(SQLModel, table=True):
    """Database table for menu items."""

    id: int | None = Field(default=None, primary_key=True)
    name: str = Field(index=True, max_length=100)
    description: str = Field(max_length=500)
    category: str = Field(index=True, max_length=20)
    price: float = Field(gt=0)
    image_url: str = Field(max_length=255)
    is_available: bool = Field(default=True, index=True)
    created_at: datetime = Field(default_factory=utc_now)
    updated_at: datetime = Field(default_factory=utc_now)
