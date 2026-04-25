from __future__ import annotations

from dataclasses import dataclass

import httpx

from .config import AppConfig


@dataclass(frozen=True)
class DiscordTokenResponse:
    access_token: str
    token_type: str
    expires_in: int
    scope: str


@dataclass(frozen=True)
class DiscordUser:
    id: str
    username: str | None = None
    global_name: str | None = None


class DiscordClient:
    async def exchange_code(self, code: str) -> DiscordTokenResponse:
        raise NotImplementedError

    async def get_current_user(self, access_token: str) -> DiscordUser:
        raise NotImplementedError


class HttpDiscordClient(DiscordClient):
    def __init__(self, config: AppConfig) -> None:
        self._config = config
        self._client = httpx.AsyncClient(timeout=20)

    async def exchange_code(self, code: str) -> DiscordTokenResponse:
        response = await self._client.post(
            "https://discord.com/api/v10/oauth2/token",
            data={
                "client_id": self._config.discord_client_id,
                "client_secret": self._config.discord_client_secret,
                "grant_type": "authorization_code",
                "code": code,
                "redirect_uri": self._config.discord_redirect_uri,
            },
            headers={"Content-Type": "application/x-www-form-urlencoded"},
        )
        response.raise_for_status()
        payload = response.json()
        return DiscordTokenResponse(
            access_token=payload["access_token"],
            token_type=payload["token_type"],
            expires_in=int(payload["expires_in"]),
            scope=payload["scope"],
        )

    async def get_current_user(self, access_token: str) -> DiscordUser:
        response = await self._client.get(
            "https://discord.com/api/v10/users/@me",
            headers={"Authorization": f"Bearer {access_token}"},
        )
        response.raise_for_status()
        payload = response.json()
        return DiscordUser(
            id=payload["id"],
            username=payload.get("username"),
            global_name=payload.get("global_name"),
        )

    async def close(self) -> None:
        await self._client.aclose()

