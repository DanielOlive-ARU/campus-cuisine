"""Shared pytest fixtures for backend tests."""

from contextlib import contextmanager
import sys
from pathlib import Path

import pytest
from fastapi.testclient import TestClient

BACKEND_DIR = Path(__file__).resolve().parents[1]
if str(BACKEND_DIR) not in sys.path:
    sys.path.insert(0, str(BACKEND_DIR))


def clear_backend_caches() -> None:
    """Clear cached backend settings and engine state."""

    from app.core.config import get_settings
    from app.db.engine import get_engine

    get_settings.cache_clear()
    get_engine.cache_clear()


@pytest.fixture
def test_environment(tmp_path, monkeypatch) -> dict[str, Path]:
    """Provide an isolated database and static directory for a test."""

    db_path = tmp_path / "test_campus_kitchen.db"
    static_dir = tmp_path / "static-images"
    static_dir.mkdir(parents=True, exist_ok=True)

    monkeypatch.setenv("DATABASE_URL", f"sqlite:///{db_path.as_posix()}")
    monkeypatch.setenv("STATIC_DIR", static_dir.as_posix())
    monkeypatch.setenv("ADMIN_API_KEY", "test-admin-key")

    clear_backend_caches()

    yield {
        "db_path": db_path,
        "static_dir": static_dir,
    }

    clear_backend_caches()


@pytest.fixture
def client_factory(test_environment, monkeypatch):
    """Return a factory for booting test clients with configurable startup seeding."""

    @contextmanager
    def factory(*, seed_on_startup: bool = False):
        monkeypatch.setenv("SEED_ON_STARTUP", "true" if seed_on_startup else "false")
        clear_backend_caches()

        from app.main import create_app

        with TestClient(create_app()) as test_client:
            yield test_client

        clear_backend_caches()

    return factory


@pytest.fixture
def client(client_factory):
    """Return an unseeded test client."""

    with client_factory(seed_on_startup=False) as test_client:
        yield test_client


@pytest.fixture
def seeded_client(client_factory):
    """Return a test client that seeds startup data."""

    with client_factory(seed_on_startup=True) as test_client:
        yield test_client
