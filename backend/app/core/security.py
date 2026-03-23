"""Security helpers for protected routes."""

from fastapi import Depends, HTTPException, status
from fastapi.security import APIKeyHeader

from app.core.config import get_settings


def get_admin_api_key_header() -> APIKeyHeader:
    """Return the configured API key header helper."""

    settings = get_settings()
    return APIKeyHeader(name=settings.admin_api_key_header, auto_error=False)


def require_admin_api_key(
    provided_key: str | None = Depends(get_admin_api_key_header),
) -> str:
    """Validate the configured admin API key."""

    settings = get_settings()

    if provided_key != settings.admin_api_key:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail="Invalid or missing admin API key",
        )

    return provided_key
