"""Database engine and session dependency helpers."""

from functools import lru_cache
from typing import Iterator

from sqlmodel import Session, create_engine

from app.core.config import get_settings


@lru_cache
def get_engine():
    """Create and cache the SQLModel engine."""

    settings = get_settings()
    connect_args = {"check_same_thread": False} if settings.database_url.startswith("sqlite") else {}
    return create_engine(settings.database_url, connect_args=connect_args)


engine = get_engine()


def get_session() -> Iterator[Session]:
    """Provide a request-scoped database session."""

    with Session(get_engine()) as session:
        yield session
