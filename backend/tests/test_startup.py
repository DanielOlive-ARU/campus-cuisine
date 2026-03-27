"""Stage 1 and 2 startup validation tests."""

from pathlib import Path
import sqlite3

from app.models import MenuItem, Order, OrderLine
from app.models.menu_item import MenuCategory


def get_table_names(db_path: Path) -> set[str]:
    """Return all table names in the SQLite database."""

    with sqlite3.connect(db_path) as connection:
        rows = connection.execute(
            "SELECT name FROM sqlite_master WHERE type = 'table'"
        ).fetchall()
    return {row[0] for row in rows}


def get_menu_item_count(db_path: Path) -> int:
    """Return the current number of menu items in the database."""

    with sqlite3.connect(db_path) as connection:
        row = connection.execute(f"SELECT COUNT(*) FROM {MenuItem.__table__.name}").fetchone()
    return int(row[0])


def test_settings_load_from_environment(test_environment) -> None:
    from app.core.config import get_settings

    get_settings.cache_clear()
    settings = get_settings()

    assert settings.database_url == f"sqlite:///{test_environment['db_path'].as_posix()}"
    assert settings.static_dir_path == test_environment["static_dir"].resolve()
    assert settings.admin_api_key == "test-admin-key"


def test_database_file_is_created_on_startup(client, test_environment) -> None:
    assert test_environment["db_path"].exists()


def test_startup_creates_expected_tables(client, test_environment) -> None:
    table_names = get_table_names(test_environment["db_path"])

    assert MenuItem.__table__.name in table_names
    assert Order.__table__.name in table_names
    assert OrderLine.__table__.name in table_names


def test_seed_runs_when_menu_table_is_empty(seeded_client, test_environment) -> None:
    del seeded_client

    assert get_menu_item_count(test_environment["db_path"]) == 12


def test_seed_is_idempotent(client_factory, test_environment) -> None:
    with client_factory(seed_on_startup=True):
        pass

    first_count = get_menu_item_count(test_environment["db_path"])

    with client_factory(seed_on_startup=True):
        pass

    second_count = get_menu_item_count(test_environment["db_path"])

    assert first_count == 12
    assert second_count == 12


def test_seeded_menu_contains_expected_categories(seeded_client, test_environment) -> None:
    del seeded_client

    with sqlite3.connect(test_environment["db_path"]) as connection:
        rows = connection.execute(
            f"SELECT DISTINCT category FROM {MenuItem.__table__.name}"
        ).fetchall()

    categories = {row[0] for row in rows}
    assert categories == {
        MenuCategory.MAIN.value,
        MenuCategory.DESSERT.value,
        MenuCategory.APPETIZER.value,
    }


def test_static_mount_serves_files(client_factory, test_environment) -> None:
    sample_file = test_environment["static_dir"] / "probe.txt"
    sample_file.write_text("static-ok", encoding="utf-8")

    with client_factory(seed_on_startup=False) as test_client:
        response = test_client.get("/images/probe.txt")

    assert response.status_code == 200
    assert response.text == "static-ok"
