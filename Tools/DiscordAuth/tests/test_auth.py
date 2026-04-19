from __future__ import annotations

import sqlite3
from pathlib import Path

from fastapi.testclient import TestClient

from discord_auth.config import AppConfig
from discord_auth.db import SqliteGameDb
from discord_auth.discord import DiscordTokenResponse, DiscordUser
from discord_auth.main import create_app
from discord_auth.state import create_state, verify_state


SECRET = "test-secret"
PLAYER_ID = "b7247188-f10e-4282-abcd-0749a14a9585"


class FakeDiscord:
    async def exchange_code(self, code: str) -> DiscordTokenResponse:
        assert code == "code-123"
        return DiscordTokenResponse("token-123", "Bearer", 3600, "identify")

    async def get_current_user(self, access_token: str) -> DiscordUser:
        assert access_token == "token-123"
        return DiscordUser("624873548527173632", "test-user", "Test User")


def test_state_roundtrip() -> None:
    state = create_state({"playerId": PLAYER_ID, "expires": 2_000_000_000, "locale": "ru-RU"}, SECRET)
    verified = verify_state(state, SECRET, now_seconds=1_900_000_000)
    assert verified.ok
    assert verified.payload
    assert verified.payload.playerId == PLAYER_ID


def test_state_rejects_tampering() -> None:
    state = create_state({"playerId": PLAYER_ID, "expires": 2_000_000_000, "locale": "ru-RU"}, SECRET)
    assert not verify_state(state + "x", SECRET, now_seconds=1_900_000_000).ok


def test_state_rejects_expired() -> None:
    state = create_state({"playerId": PLAYER_ID, "expires": 10, "locale": "ru-RU"}, SECRET)
    result = verify_state(state, SECRET, now_seconds=11)
    assert not result.ok
    assert result.error == "expired"


def test_login_redirects_to_discord(tmp_path: Path) -> None:
    app = create_app(_config(tmp_path), _db(tmp_path), FakeDiscord())
    state = create_state({"playerId": PLAYER_ID, "expires": 2_000_000_000, "locale": "ru-RU"}, SECRET)
    response = TestClient(app).get("/auth/login", params={"state": state}, follow_redirects=False)
    assert response.status_code == 307
    assert response.headers["location"].startswith("https://discord.com/oauth2/authorize")


def test_callback_links_account(tmp_path: Path) -> None:
    db = _db(tmp_path)
    app = create_app(_config(tmp_path), db, FakeDiscord())
    state = create_state({"playerId": PLAYER_ID, "expires": 2_000_000_000, "locale": "ru-RU"}, SECRET)

    response = TestClient(app).get("/auth/callback", params={"code": "code-123", "state": state})
    assert response.status_code == 200

    rows = db._conn.execute("SELECT player_id, discord_id FROM rmc_linked_accounts").fetchall()  # noqa: SLF001
    assert rows == [(PLAYER_ID, 624873548527173632)]


def _config(tmp_path: Path) -> AppConfig:
    return AppConfig(
        port=2424,
        discord_client_id="client-id",
        discord_client_secret="client-secret",
        discord_redirect_uri="https://auth.example.test/auth/callback",
        public_base_url="https://auth.example.test",
        database_provider="sqlite",
        database_url=None,
        sqlite_path=str(tmp_path / "auth.db"),
        oauth_state_secret=SECRET,
    )


def _db(tmp_path: Path) -> SqliteGameDb:
    path = tmp_path / "auth.db"
    conn = sqlite3.connect(path)
    conn.executescript(
        """
        CREATE TABLE IF NOT EXISTS rmc_discord_accounts (rmc_discord_accounts_id INTEGER PRIMARY KEY);
        CREATE TABLE IF NOT EXISTS rmc_linked_accounts (player_id TEXT PRIMARY KEY, discord_id INTEGER UNIQUE);
        CREATE TABLE IF NOT EXISTS rmc_linked_accounts_logs (player_id TEXT, discord_id INTEGER, at TEXT);
        """
    )
    conn.close()
    return SqliteGameDb(str(path))

