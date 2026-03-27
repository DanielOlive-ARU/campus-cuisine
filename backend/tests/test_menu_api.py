"""Phase 4 public menu API tests."""

from sqlmodel import Session

from app.db.engine import get_engine
from app.models import MenuItem


def test_get_menu_returns_seeded_items(seeded_client) -> None:
    response = seeded_client.get("/api/menu")

    assert response.status_code == 200
    payload = response.json()

    assert len(payload) == 12
    assert payload[0]["name"] == "Grilled Chicken Burger"
    assert all(item["is_available"] is True for item in payload)


def test_get_menu_filters_by_category(seeded_client) -> None:
    response = seeded_client.get("/api/menu", params={"category": "dessert"})

    assert response.status_code == 200
    payload = response.json()

    assert payload
    assert all(item["category"] == "dessert" for item in payload)


def test_get_menu_item_returns_one_item(seeded_client) -> None:
    response = seeded_client.get("/api/menu/1")

    assert response.status_code == 200
    payload = response.json()

    assert payload["id"] == 1
    assert payload["name"] == "Grilled Chicken Burger"


def test_get_menu_item_returns_404_for_unknown_id(seeded_client) -> None:
    response = seeded_client.get("/api/menu/999")

    assert response.status_code == 404
    assert response.json() == {"detail": "Menu item not found"}


def test_get_menu_hides_unavailable_items_by_default(seeded_client) -> None:
    with Session(get_engine()) as session:
        item = session.get(MenuItem, 1)
        assert item is not None
        item.is_available = False
        session.add(item)
        session.commit()

    response = seeded_client.get("/api/menu")

    assert response.status_code == 200
    payload = response.json()

    assert all(item["id"] != 1 for item in payload)


def test_get_menu_can_include_unavailable_items(seeded_client) -> None:
    with Session(get_engine()) as session:
        item = session.get(MenuItem, 1)
        assert item is not None
        item.is_available = False
        session.add(item)
        session.commit()

    response = seeded_client.get("/api/menu", params={"available_only": "false"})

    assert response.status_code == 200
    payload = response.json()

    assert any(item["id"] == 1 for item in payload)


def test_get_menu_item_returns_404_for_unavailable_item(seeded_client) -> None:
    with Session(get_engine()) as session:
        item = session.get(MenuItem, 1)
        assert item is not None
        item.is_available = False
        session.add(item)
        session.commit()

    response = seeded_client.get("/api/menu/1")

    assert response.status_code == 404
    assert response.json() == {"detail": "Menu item not found"}
