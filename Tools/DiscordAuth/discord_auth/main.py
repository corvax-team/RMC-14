from __future__ import annotations

from contextlib import asynccontextmanager
from html import escape
from typing import Callable
from urllib.parse import urlencode

from fastapi import FastAPI, Query
from fastapi.responses import HTMLResponse, RedirectResponse, Response

from .config import AppConfig, load_config
from .db import GameDb, LinkResult, create_game_db
from .discord import DiscordClient, DiscordGuildClient, HttpDiscordClient
from .state import verify_state


def create_app(
    config: AppConfig | None = None,
    db: GameDb | None = None,
    discord: DiscordClient | None = None,
) -> FastAPI:
    resolved_config = config or load_config()
    resolved_db = db or create_game_db(resolved_config)
    resolved_discord = discord or HttpDiscordClient(resolved_config)
    resolved_guild = DiscordGuildClient(resolved_config)

    @asynccontextmanager
    async def lifespan(_service: FastAPI):
        try:
            yield
        finally:
            close = getattr(resolved_discord, "close", None)
            if close:
                maybe_awaitable = close()
                if hasattr(maybe_awaitable, "__await__"):
                    await maybe_awaitable
            await resolved_guild.close()
            resolved_db.close()

    service = FastAPI(lifespan=lifespan)

    @service.get("/health")
    def health() -> dict[str, bool]:
        return {"ok": True}

    @service.get("/auth/login")
    def auth_login(state: str | None = Query(default=None)) -> Response:
        if not state:
            return send_error(400, "Missing state", "Open this link from the game lobby.")

        verified = verify_state(state, resolved_config.oauth_state_secret)
        if not verified.ok:
            return send_error(400, "Invalid state", f"State validation failed: {verified.error}.")

        query = urlencode(
            {
                "client_id": resolved_config.discord_client_id,
                "response_type": "code",
                "redirect_uri": resolved_config.discord_redirect_uri,
                "scope": "identify",
                "state": state,
            }
        )
        return RedirectResponse(f"https://discord.com/oauth2/authorize?{query}")

    @service.get("/auth/callback")
    async def auth_callback(
        code: str | None = Query(default=None),
        state: str | None = Query(default=None),
    ) -> Response:
        if not code or not state:
            return send_error(400, "Missing OAuth data", "Discord did not return the expected callback parameters.")

        verified = verify_state(state, resolved_config.oauth_state_secret)
        if not verified.ok or verified.payload is None:
            return send_error(400, "Invalid state", f"State validation failed: {verified.error}.")

        try:
            token = await resolved_discord.exchange_code(code)
            user = await resolved_discord.get_current_user(token.access_token)
            result = resolved_db.link_account(verified.payload.playerId, user.id)
            try:
                await resolved_guild.sync_linked_role(user.id, linked=True)
            except Exception as error:
                print("Discord role sync after OAuth link failed.", error)
            return send_link_result(result, user.global_name or user.username or user.id)
        except Exception as error:
            print("Discord OAuth callback failed.", error)
            return send_error(500, "Unable to link Discord", "Try again later or contact staff.")

    return service


class LazyApp:
    def __init__(self, factory: Callable[[], FastAPI]) -> None:
        self._factory = factory
        self._app: FastAPI | None = None

    async def __call__(self, scope, receive, send) -> None:  # type: ignore[no-untyped-def]
        if self._app is None:
            self._app = self._factory()
        await self._app(scope, receive, send)


def send_link_result(result: LinkResult, display_name: str) -> HTMLResponse:
    if result.status == "linked":
        return send_page(200, "Discord linked", f"Discord account {escape(display_name)} is now linked.")
    if result.status == "already-linked":
        return send_page(200, "Discord already linked", "This Discord account is already linked to your game account.")
    if result.status == "player-conflict":
        return send_error(409, "Game account already linked", "This game account is already linked to another Discord account.")
    return send_error(409, "Discord account already linked", "This Discord account is already linked to another game account.")


def send_error(status: int, title: str, details: str) -> HTMLResponse:
    return send_page(status, title, details)


def send_page(status: int, title: str, details: str) -> HTMLResponse:
    return HTMLResponse(
        f"""<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>{escape(title)}</title>
  <style>
    body {{ margin: 0; min-height: 100vh; display: grid; place-items: center; background: #10131a; color: #eef1ff; font-family: system-ui, sans-serif; }}
    main {{ width: min(560px, calc(100% - 32px)); border: 1px solid #5865f2; padding: 24px; background: #171b25; }}
    h1 {{ margin: 0 0 12px; font-size: 24px; }}
    p {{ margin: 0; line-height: 1.5; color: #cfd6ff; }}
  </style>
</head>
<body>
  <main>
    <h1>{escape(title)}</h1>
    <p>{details}</p>
  </main>
</body>
</html>""",
        status_code=status,
    )


app = LazyApp(create_app)
