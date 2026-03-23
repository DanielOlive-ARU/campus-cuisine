"""Seed data for local development."""

from datetime import datetime, timezone

from sqlmodel import Session, select

from app.models.menu_item import MenuCategory, MenuItem


def utc_now() -> datetime:
    """Return the current UTC timestamp."""

    return datetime.now(timezone.utc)


def build_seed_menu_items() -> list[MenuItem]:
    """Return the starter menu used for local development."""

    return [
        MenuItem(
            name="Grilled Chicken Burger",
            description="Chargrilled chicken fillet with lettuce and house sauce.",
            category=MenuCategory.MAIN.value,
            price=8.99,
            image_url="/images/grilled-chicken-burger.jpg",
            is_available=True,
            created_at=utc_now(),
            updated_at=utc_now(),
        ),
        MenuItem(
            name="Beef Lasagne",
            description="Layered pasta baked with rich beef ragu and cheese.",
            category=MenuCategory.MAIN.value,
            price=9.49,
            image_url="/images/beef-lasagne.jpg",
            is_available=True,
            created_at=utc_now(),
            updated_at=utc_now(),
        ),
        MenuItem(
            name="Margherita Pizza",
            description="Stone-baked pizza with mozzarella, basil, and tomato sauce.",
            category=MenuCategory.MAIN.value,
            price=8.25,
            image_url="/images/margherita-pizza.jpg",
            is_available=True,
            created_at=utc_now(),
            updated_at=utc_now(),
        ),
        MenuItem(
            name="BBQ Chicken Wrap",
            description="Grilled chicken, salad, and smoky BBQ sauce in a toasted wrap.",
            category=MenuCategory.MAIN.value,
            price=7.95,
            image_url="/images/bbq-chicken-wrap.jpg",
            is_available=True,
            created_at=utc_now(),
            updated_at=utc_now(),
        ),
        MenuItem(
            name="Veggie Pasta Bake",
            description="Roasted vegetables and penne pasta baked in tomato sauce.",
            category=MenuCategory.MAIN.value,
            price=7.75,
            image_url="/images/veggie-pasta-bake.jpg",
            is_available=True,
            created_at=utc_now(),
            updated_at=utc_now(),
        ),
        MenuItem(
            name="Fish and Chips",
            description="Crispy battered fish served with chips and tartar sauce.",
            category=MenuCategory.MAIN.value,
            price=9.95,
            image_url="/images/fish-and-chips.jpg",
            is_available=True,
            created_at=utc_now(),
            updated_at=utc_now(),
        ),
        MenuItem(
            name="Chocolate Brownie",
            description="Warm chocolate brownie with a soft centre.",
            category=MenuCategory.DESSERT.value,
            price=3.99,
            image_url="/images/chocolate-brownie.jpg",
            is_available=True,
            created_at=utc_now(),
            updated_at=utc_now(),
        ),
        MenuItem(
            name="Classic Cheesecake",
            description="Creamy vanilla cheesecake served chilled.",
            category=MenuCategory.DESSERT.value,
            price=4.50,
            image_url="/images/classic-cheesecake.jpg",
            is_available=True,
            created_at=utc_now(),
            updated_at=utc_now(),
        ),
        MenuItem(
            name="Garlic Bread",
            description="Toasted bread brushed with garlic butter and herbs.",
            category=MenuCategory.APPETIZER.value,
            price=3.25,
            image_url="/images/garlic-bread.jpg",
            is_available=True,
            created_at=utc_now(),
            updated_at=utc_now(),
        ),
        MenuItem(
            name="Onion Rings",
            description="Golden fried onion rings with a crisp coating.",
            category=MenuCategory.APPETIZER.value,
            price=3.45,
            image_url="/images/onion-rings.jpg",
            is_available=True,
            created_at=utc_now(),
            updated_at=utc_now(),
        ),
        MenuItem(
            name="Ice Cream Sundae",
            description="Vanilla ice cream with chocolate sauce and sprinkles.",
            category=MenuCategory.DESSERT.value,
            price=4.15,
            image_url="/images/ice-cream-sundae.jpg",
            is_available=True,
            created_at=utc_now(),
            updated_at=utc_now(),
        ),
        MenuItem(
            name="Apple Pie",
            description="Baked apple pie served warm with cinnamon.",
            category=MenuCategory.DESSERT.value,
            price=4.20,
            image_url="/images/apple-pie.jpg",
            is_available=True,
            created_at=utc_now(),
            updated_at=utc_now(),
        ),
    ]


def seed_menu_items(session: Session) -> int:
    """Seed starter menu data if the table is empty."""

    existing_item = session.exec(select(MenuItem.id)).first()
    if existing_item is not None:
        return 0

    menu_items = build_seed_menu_items()
    session.add_all(menu_items)
    session.commit()
    return len(menu_items)
