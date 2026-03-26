"""Phase 5 admin menu API tests."""

from sqlmodel import Session

from app.db.engine import get_engine
from app.models import MenuItem

ADMIN_HEADERS = {"x-admin-key": "test-admin-key"}


def test_admin_routes_require_api_key(seeded_client) -> None:
    response = seeded_client.get("/api/admin/menu-items")

    assert response.status_code == 401
    assert response.json() == {"detail": "Invalid or missing admin API key"}


def test_admin_list_includes_unavailable_items_by_default(seeded_client) -> None:
    with Session(get_engine()) as session:
        item = session.get(MenuItem, 1)
        assert item is not None
        item.is_available = False
        session.add(item)
        session.commit()

    response = seeded_client.get("/api/admin/menu-items", headers=ADMIN_HEADERS)

    assert response.status_code == 200
    payload = response.json()

    assert any(item["id"] == 1 and item["is_available"] is False for item in payload)


def test_admin_get_item_returns_unavailable_item(seeded_client) -> None:
    with Session(get_engine()) as session:
        item = session.get(MenuItem, 1)
        assert item is not None
        item.is_available = False
        session.add(item)
        session.commit()

    response = seeded_client.get("/api/admin/menu-items/1", headers=ADMIN_HEADERS)

    assert response.status_code == 200
    assert response.json()["is_available"] is False


def test_admin_create_menu_item(seeded_client) -> None:
    response = seeded_client.post(
        "/api/admin/menu-items",
        headers=ADMIN_HEADERS,
        json={
            "name": "  Lemon   Tart  ",
            "description": "  Sharp citrus tart with cream.  ",
            "category": "dessert",
            "price": 4.95,
            "image_url": "/images/lemon-tart.jpg",
            "is_available": False,
        },
    )

    assert response.status_code == 201
    payload = response.json()

    assert payload["name"] == "Lemon Tart"
    assert payload["description"] == "Sharp citrus tart with cream."
    assert payload["category"] == "dessert"
    assert payload["price"] == 4.95
    assert payload["image_url"] == "/images/lemon-tart.jpg"
    assert payload["is_available"] is False


def test_admin_update_menu_item_partial_patch(seeded_client) -> None:
    response = seeded_client.put(
        "/api/admin/menu-items/1",
        headers=ADMIN_HEADERS,
        json={
            "name": "  Firecracker   Burger ",
            "price": 9.49,
            "is_available": False,
        },
    )

    assert response.status_code == 200
    payload = response.json()

    assert payload["id"] == 1
    assert payload["name"] == "Firecracker Burger"
    assert payload["price"] == 9.49
    assert payload["is_available"] is False


def test_admin_delete_menu_item(seeded_client) -> None:
    response = seeded_client.delete(
        "/api/admin/menu-items/1",
        headers=ADMIN_HEADERS,
    )

    assert response.status_code == 204
    assert response.content == b""

    fetch_response = seeded_client.get(
        "/api/admin/menu-items/1",
        headers=ADMIN_HEADERS,
    )
    assert fetch_response.status_code == 404
    assert fetch_response.json() == {"detail": "Menu item not found"}
