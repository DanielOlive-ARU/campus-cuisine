"""Application configuration loaded from environment variables."""

from functools import lru_cache
from pathlib import Path

from pydantic import Field
from pydantic_settings import BaseSettings, SettingsConfigDict

BACKEND_DIR = Path(__file__).resolve().parents[2]


class Settings(BaseSettings):
    """Runtime settings for the backend application."""

    model_config = SettingsConfigDict(
        env_file=BACKEND_DIR / ".env",
        env_file_encoding="utf-8",
        case_sensitive=False,
    )

    app_name: str = "Campus Kitchen API"
    app_version: str = "0.1.0"
    api_prefix: str = "/api"
    database_url: str = "sqlite:///./campus_kitchen.db"
    admin_api_key: str = "change-me"
    admin_api_key_header: str = "x-admin-key"
    seed_on_startup: bool = True
    static_mount_path: str = "/images"
    static_dir: str = "app/static/images"
    cors_origins: list[str] = Field(default_factory=lambda: ["*"])

    @property
    def backend_dir(self) -> Path:
        return BACKEND_DIR

    @property
    def static_dir_path(self) -> Path:
        return (BACKEND_DIR / self.static_dir).resolve()


@lru_cache
def get_settings() -> Settings:
    """Return cached application settings."""

    return Settings()
