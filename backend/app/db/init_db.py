"""Database initialization helpers."""

from sqlmodel import SQLModel

from app.db.engine import get_engine
from app.models import MenuItem, Order, OrderLine  # noqa: F401


def create_db_and_tables() -> None:
    """Create all configured database tables."""

    SQLModel.metadata.create_all(get_engine())
