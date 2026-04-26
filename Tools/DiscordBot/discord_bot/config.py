from __future__ import annotations

from dataclasses import dataclass
from os import environ
from pathlib import Path
from typing import Mapping

from dotenv import load_dotenv


DEFAULT_LINKED_ROLE_NAME = "\u041f\u0440\u0438\u0432\u044f\u0437\u0430\u043d"


@dataclass(frozen=True)
class AppConfig:
    database_provider: str
    database_url: str | None
    sqlite_path: str | None
    discord_bot_token: str
    discord_guild_id: str
    sponsor_i_role_id: str | None
    sponsor_ii_role_id: str | None
    sponsor_iii_role_id: str | None
    linked_role_id: str | None
    linked_role_name: str
    sponsorship_rolling_days: int
    sync_interval_seconds: int
    request_timeout_seconds: float
    max_concurrency: int


def load_config(env: Mapping[str, str] | None = None) -> AppConfig:
    if env is None:
        load_dotenv(Path(__file__).resolve().parents[2] / ".env", override=False)
        env = environ

    provider = env.get("DATABASE_PROVIDER", "postgres").lower()
    if provider not in {"postgres", "sqlite"}:
        raise ValueError("DATABASE_PROVIDER must be postgres or sqlite.")

    database_url = env.get("DATABASE_URL")
    sqlite_path = env.get("SQLITE_PATH")
    if provider == "postgres" and not database_url:
        raise ValueError("DATABASE_URL is required when DATABASE_PROVIDER=postgres.")
    if provider == "sqlite" and not sqlite_path:
        raise ValueError("SQLITE_PATH is required when DATABASE_PROVIDER=sqlite.")

    sync_interval_seconds = int(env.get("DISCORD_ROLE_SYNC_INTERVAL_SECONDS", "10800"))
    if sync_interval_seconds <= 0:
        raise ValueError("DISCORD_ROLE_SYNC_INTERVAL_SECONDS must be a positive integer.")

    request_timeout_seconds = float(env.get("DISCORD_REQUEST_TIMEOUT_SECONDS", "20"))
    if request_timeout_seconds <= 0:
        raise ValueError("DISCORD_REQUEST_TIMEOUT_SECONDS must be a positive number.")

    max_concurrency = int(env.get("DISCORD_MAX_CONCURRENCY", "8"))
    if max_concurrency <= 0:
        raise ValueError("DISCORD_MAX_CONCURRENCY must be a positive integer.")

    linked_role_id = _optional(env, "DISCORD_LINKED_ROLE_ID")
    linked_role_name = env.get("DISCORD_LINKED_ROLE_NAME", DEFAULT_LINKED_ROLE_NAME).strip() or DEFAULT_LINKED_ROLE_NAME

    sponsorship_rolling_days = int(env.get("CCM_SPONSORSHIP_ROLLING_DAYS", "31"))
    if sponsorship_rolling_days <= 0:
        raise ValueError("CCM_SPONSORSHIP_ROLLING_DAYS must be a positive integer.")

    sponsor_i_role_id = _optional(env, "DISCORD_SPONSOR_I_ROLE_ID")
    sponsor_ii_role_id = _optional(env, "DISCORD_SPONSOR_II_ROLE_ID")
    sponsor_iii_role_id = _optional(env, "DISCORD_SPONSOR_III_ROLE_ID")
    if not any((sponsor_i_role_id, sponsor_ii_role_id, sponsor_iii_role_id)):
        raise ValueError(
            "At least one of DISCORD_SPONSOR_I_ROLE_ID, DISCORD_SPONSOR_II_ROLE_ID, "
            "or DISCORD_SPONSOR_III_ROLE_ID must be configured."
        )

    return AppConfig(
        database_provider=provider,
        database_url=database_url,
        sqlite_path=sqlite_path,
        discord_bot_token=_require(env, "DISCORD_BOT_TOKEN"),
        discord_guild_id=_require(env, "DISCORD_GUILD_ID"),
        sponsor_i_role_id=sponsor_i_role_id,
        sponsor_ii_role_id=sponsor_ii_role_id,
        sponsor_iii_role_id=sponsor_iii_role_id,
        linked_role_id=linked_role_id,
        linked_role_name=linked_role_name,
        sponsorship_rolling_days=sponsorship_rolling_days,
        sync_interval_seconds=sync_interval_seconds,
        request_timeout_seconds=request_timeout_seconds,
        max_concurrency=max_concurrency,
    )


def _require(env: Mapping[str, str], key: str) -> str:
    value = env.get(key, "").strip()
    if not value:
        raise ValueError(f"{key} is required.")
    return value


def _optional(env: Mapping[str, str], key: str) -> str | None:
    value = env.get(key, "").strip()
    return value or None
