"""FastAPI application entrypoint."""

from contextlib import asynccontextmanager

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.staticfiles import StaticFiles
from sqlmodel import Session

from app.core.config import get_settings
from app.db.engine import get_engine
from app.db.init_db import create_db_and_tables
from app.db.seed import seed_menu_items
from app.routers.admin_menu import router as admin_menu_router
from app.routers.health import router as health_router
from app.routers.menu import router as menu_router
from app.routers.orders import router as orders_router


@asynccontextmanager
async def lifespan(_: FastAPI):
    settings = get_settings()

    create_db_and_tables()

    if settings.seed_on_startup:
        with Session(get_engine()) as session:
            seed_menu_items(session)

    yield


def create_app() -> FastAPI:
    settings = get_settings()
    settings.static_dir_path.mkdir(parents=True, exist_ok=True)

    app = FastAPI(
        title=settings.app_name,
        version=settings.app_version,
        lifespan=lifespan,
    )

    app.add_middleware(
        CORSMiddleware,
        allow_origins=settings.cors_origins,
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )

    app.mount(
        settings.static_mount_path,
        StaticFiles(directory=settings.static_dir_path),
        name="images",
    )

    app.include_router(health_router)
    app.include_router(menu_router)
    app.include_router(admin_menu_router)
    app.include_router(orders_router)

    return app


app = create_app()
