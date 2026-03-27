"""Request and response schemas for menu items."""

from __future__ import annotations

import re

from pydantic import BaseModel, ConfigDict, Field, field_validator

from app.models.menu_item import MenuCategory

_MULTISPACE_RE = re.compile(r"\s+")


def _normalize_name(value: str) -> str:
    normalized = _MULTISPACE_RE.sub(" ", value.strip())
    if not normalized:
        raise ValueError("Name must not be blank.")
    return normalized


def _normalize_required_text(value: str, field_name: str) -> str:
    normalized = value.strip()
    if not normalized:
        raise ValueError(f"{field_name} must not be blank.")
    return normalized


def _normalize_image_path(value: str) -> str:
    normalized = value.strip()
    if not normalized:
        raise ValueError("Image URL must not be blank.")
    if not normalized.startswith("/images/"):
        raise ValueError("Image URL must be a relative backend path under /images/.")
    return normalized


class MenuSchemaBase(BaseModel):
    """Base settings shared by menu response schemas."""

    model_config = ConfigDict(from_attributes=True)


class MenuRequestBase(BaseModel):
    """Base settings shared by menu request schemas."""

    model_config = ConfigDict(extra="forbid")


class MenuItemRead(MenuSchemaBase):
    """Public/admin menu item response schema."""

    id: int
    name: str
    description: str
    category: MenuCategory
    price: float
    image_url: str
    is_available: bool


class MenuItemCreate(MenuRequestBase):
    """Menu item create schema."""

    name: str = Field(min_length=1, max_length=100)
    description: str = Field(min_length=1, max_length=500)
    category: MenuCategory
    price: float = Field(gt=0)
    image_url: str = Field(min_length=1, max_length=255)
    is_available: bool = True

    @field_validator("name")
    @classmethod
    def normalize_name(cls, value: str) -> str:
        return _normalize_name(value)

    @field_validator("description")
    @classmethod
    def normalize_description(cls, value: str) -> str:
        return _normalize_required_text(value, "Description")

    @field_validator("image_url")
    @classmethod
    def normalize_image_url(cls, value: str) -> str:
        return _normalize_image_path(value)


class MenuItemUpdate(MenuRequestBase):
    """Menu item partial update schema."""

    name: str | None = Field(default=None, min_length=1, max_length=100)
    description: str | None = Field(default=None, min_length=1, max_length=500)
    category: MenuCategory | None = None
    price: float | None = Field(default=None, gt=0)
    image_url: str | None = Field(default=None, min_length=1, max_length=255)
    is_available: bool | None = None

    @field_validator("name")
    @classmethod
    def normalize_optional_name(cls, value: str | None) -> str | None:
        if value is None:
            return None
        return _normalize_name(value)

    @field_validator("description")
    @classmethod
    def normalize_optional_description(cls, value: str | None) -> str | None:
        if value is None:
            return None
        return _normalize_required_text(value, "Description")

    @field_validator("image_url")
    @classmethod
    def normalize_optional_image_url(cls, value: str | None) -> str | None:
        if value is None:
            return None
        return _normalize_image_path(value)
