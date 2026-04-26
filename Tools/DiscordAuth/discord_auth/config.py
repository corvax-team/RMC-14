from __future__ import annotations

from dataclasses import dataclass
from os import environ
from pathlib import Path
from typing import Mapping

from dotenv import load_dotenv


DEFAULT_LINKED_ROLE_NAME = "\u041f\u0440\u0438\u0432\u044f\u0437\u0430\u043d"


@dataclass(frozen=True)
class AppConfig:
    port: int
    discord_client_id: str
    discord_client_secret: str
    discord_redirect_uri: str
    public_base_url: str
    database_provider: str
    database_url: str | None
    sqlite_path: str | None
    oauth_state_secret: str
    discord_bot_token: str
    discord_guild_id: str
    discord_linked_role_id: str | None
    discord_linked_role_name: str


def load_config(env: Mapping[str, str] | None = None) -> AppConfig:
    if env is None:
        load_dotenv(Path(__file__).resolve().parents[2] / ".env", override=False)
        env = environ

    provider = env.get("DATABASE_PROVIDER", "postgres").lower()
    if provider not in {"postgres", "sqlite"}:
        raise ValueError("DATABASE_PROVIDER must be postgres or sqlite.")

    port = int(env.get("PORT", "2424"))
    if port <= 0:
        raise ValueError("PORT must be a positive integer.")

    database_url = env.get("DATABASE_URL")
    sqlite_path = env.get("SQLITE_PATH")
    if provider == "postgres" and not database_url:
        raise ValueError("DATABASE_URL is required when DATABASE_PROVIDER=postgres.")
    if provider == "sqlite" and not sqlite_path:
        raise ValueError("SQLITE_PATH is required when DATABASE_PROVIDER=sqlite.")

    return AppConfig(
        port=port,
        discord_client_id=_require(env, "DISCORD_CLIENT_ID"),
        discord_client_secret=_require(env, "DISCORD_CLIENT_SECRET"),
        discord_redirect_uri=_require(env, "DISCORD_REDIRECT_URI"),
        public_base_url=_require(env, "PUBLIC_BASE_URL"),
        database_provider=provider,
        database_url=database_url,
        sqlite_path=sqlite_path,
        oauth_state_secret=_require(env, "OAUTH_STATE_SECRET"),
        discord_bot_token=_require(env, "DISCORD_BOT_TOKEN"),
        discord_guild_id=_require(env, "DISCORD_GUILD_ID"),
        discord_linked_role_id=_optional(env, "DISCORD_LINKED_ROLE_ID"),
        discord_linked_role_name=env.get("DISCORD_LINKED_ROLE_NAME", DEFAULT_LINKED_ROLE_NAME).strip() or DEFAULT_LINKED_ROLE_NAME,
    )


def _require(env: Mapping[str, str], key: str) -> str:
    value = env.get(key, "").strip()
    if not value:
        raise ValueError(f"{key} is required.")
    return value


def _optional(env: Mapping[str, str], key: str) -> str | None:
    value = env.get(key, "").strip()
    return value or None
