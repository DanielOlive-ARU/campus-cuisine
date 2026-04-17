"""Order service helper tests."""

from app.services.order_service import estimate_prep_minutes


def test_estimate_prep_minutes_returns_minimum_for_smallest_valid_order() -> None:
    assert estimate_prep_minutes(total_items=1, distinct_lines=1) == 10


def test_estimate_prep_minutes_returns_expected_mid_order_value() -> None:
    assert estimate_prep_minutes(total_items=3, distinct_lines=2) == 15


def test_estimate_prep_minutes_clamps_large_orders_to_ceiling() -> None:
    assert estimate_prep_minutes(total_items=20, distinct_lines=10) == 35


def test_estimate_prep_minutes_counts_duplicate_menu_items_as_one_distinct_line() -> None:
    distinct_lines = len({item_id for item_id in [1, 1]})
    assert estimate_prep_minutes(total_items=2, distinct_lines=distinct_lines) == 12
