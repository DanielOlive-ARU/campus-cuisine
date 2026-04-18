"""Admin order status API tests."""

ADMIN_HEADERS = {"x-admin-key": "test-admin-key"}


def test_admin_order_routes_require_api_key(seeded_client) -> None:
    response = seeded_client.patch(
        "/api/admin/orders/1/status",
        json={"status": "cancelled"},
    )

    assert response.status_code == 401
    assert response.json() == {"detail": "Invalid or missing admin API key"}


def test_admin_can_cancel_confirmed_order(seeded_client) -> None:
    create_response = seeded_client.post(
        "/api/orders",
        json={"items": [{"menu_item_id": 1, "quantity": 2}]},
    )
    order_id = create_response.json()["id"]

    response = seeded_client.patch(
        f"/api/admin/orders/{order_id}/status",
        headers=ADMIN_HEADERS,
        json={"status": "cancelled"},
    )

    assert response.status_code == 200
    payload = response.json()

    assert payload["id"] == order_id
    assert payload["status"] == "cancelled"
    assert payload["total_items"] == 2
    assert len(payload["items"]) == 1


def test_admin_same_status_update_is_idempotent(seeded_client) -> None:
    create_response = seeded_client.post(
        "/api/orders",
        json={"items": [{"menu_item_id": 1, "quantity": 1}]},
    )
    order_id = create_response.json()["id"]

    response = seeded_client.patch(
        f"/api/admin/orders/{order_id}/status",
        headers=ADMIN_HEADERS,
        json={"status": "confirmed"},
    )

    assert response.status_code == 200
    payload = response.json()
    assert payload["status"] == "confirmed"


def test_get_order_returns_cancelled_status_after_admin_update(seeded_client) -> None:
    create_response = seeded_client.post(
        "/api/orders",
        json={"items": [{"menu_item_id": 1, "quantity": 1}]},
    )
    order_id = create_response.json()["id"]

    seeded_client.patch(
        f"/api/admin/orders/{order_id}/status",
        headers=ADMIN_HEADERS,
        json={"status": "cancelled"},
    )

    response = seeded_client.get(f"/api/orders/{order_id}")

    assert response.status_code == 200
    assert response.json()["status"] == "cancelled"


def test_admin_cannot_reopen_cancelled_order(seeded_client) -> None:
    create_response = seeded_client.post(
        "/api/orders",
        json={"items": [{"menu_item_id": 1, "quantity": 1}]},
    )
    order_id = create_response.json()["id"]

    seeded_client.patch(
        f"/api/admin/orders/{order_id}/status",
        headers=ADMIN_HEADERS,
        json={"status": "cancelled"},
    )

    response = seeded_client.patch(
        f"/api/admin/orders/{order_id}/status",
        headers=ADMIN_HEADERS,
        json={"status": "confirmed"},
    )

    assert response.status_code == 400
    assert response.json() == {"detail": "Transition from current status is not allowed"}


def test_admin_order_status_update_returns_404_for_unknown_order(seeded_client) -> None:
    response = seeded_client.patch(
        "/api/admin/orders/999/status",
        headers=ADMIN_HEADERS,
        json={"status": "cancelled"},
    )

    assert response.status_code == 404
    assert response.json() == {"detail": "Order not found"}
