"""Phase 6 order API tests."""

from sqlmodel import Session

from app.db.engine import get_engine
from app.models import MenuItem


def test_create_order_returns_confirmation(seeded_client) -> None:
    response = seeded_client.post(
        "/api/orders",
        json={
            "items": [
                {"menu_item_id": 1, "quantity": 2},
                {"menu_item_id": 7, "quantity": 1},
            ]
        },
    )

    assert response.status_code == 201
    payload = response.json()

    assert payload["status"] == "confirmed"
    assert payload["total_items"] == 3
    assert payload["grand_total"] == 21.97
    assert payload["message"] == "Order placed successfully"


def test_get_order_returns_full_order_with_snapshot_lines(seeded_client) -> None:
    create_response = seeded_client.post(
        "/api/orders",
        json={"items": [{"menu_item_id": 1, "quantity": 2}]},
    )
    order_id = create_response.json()["id"]

    with Session(get_engine()) as session:
        item = session.get(MenuItem, 1)
        assert item is not None
        item.name = "Changed Later"
        item.price = 99.99
        session.add(item)
        session.commit()

    fetch_response = seeded_client.get(f"/api/orders/{order_id}")

    assert fetch_response.status_code == 200
    payload = fetch_response.json()

    assert payload["id"] == order_id
    assert payload["status"] == "confirmed"
    assert payload["total_items"] == 2
    assert payload["grand_total"] == 17.98
    assert len(payload["items"]) == 1
    assert payload["items"][0]["name"] == "Grilled Chicken Burger"
    assert payload["items"][0]["unit_price"] == 8.99
    assert payload["items"][0]["line_total"] == 17.98


def test_create_order_rejects_unknown_menu_item(seeded_client) -> None:
    response = seeded_client.post(
        "/api/orders",
        json={"items": [{"menu_item_id": 999, "quantity": 1}]},
    )

    assert response.status_code == 400
    assert response.json() == {"detail": "Unknown menu item ids: 999"}


def test_create_order_rejects_unavailable_menu_item(seeded_client) -> None:
    with Session(get_engine()) as session:
        item = session.get(MenuItem, 1)
        assert item is not None
        item.is_available = False
        session.add(item)
        session.commit()

    response = seeded_client.post(
        "/api/orders",
        json={"items": [{"menu_item_id": 1, "quantity": 1}]},
    )

    assert response.status_code == 400
    assert response.json() == {"detail": "Unavailable menu item ids: 1"}


def test_get_order_returns_404_for_unknown_id(seeded_client) -> None:
    response = seeded_client.get("/api/orders/999")

    assert response.status_code == 404
    assert response.json() == {"detail": "Order not found"}
