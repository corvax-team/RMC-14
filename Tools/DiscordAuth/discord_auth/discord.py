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


class DiscordGuildClient:
    def __init__(self, config: AppConfig) -> None:
        self._config = config
        self._client = httpx.AsyncClient(
            base_url="https://discord.com/api/v10",
            timeout=20,
            headers={
                "Authorization": f"Bot {config.discord_bot_token}",
                "User-Agent": "RMC-DiscordAuth/1.0",
            },
        )
        self._linked_role_id: str | None = config.discord_linked_role_id
        self._linked_role_name = config.discord_linked_role_name

    async def sync_linked_role(self, discord_id: str, linked: bool) -> None:
        role_id = await self._resolve_linked_role_id()
        if role_id is None:
            return

        member = await self._get_member(discord_id)
        if member is None:
            return

        if linked:
            await self._add_role(discord_id, role_id)
        else:
            await self._remove_role(discord_id, role_id)

    async def close(self) -> None:
        await self._client.aclose()

    async def _resolve_linked_role_id(self) -> str | None:
        if self._linked_role_id:
            return self._linked_role_id

        response = await self._client.get(f"/guilds/{self._config.discord_guild_id}/roles")
        response.raise_for_status()
        for role in response.json():
            if str(role.get("name", "")).strip().lower() == self._linked_role_name.lower():
                self._linked_role_id = str(role["id"])
                return self._linked_role_id
        return None

    async def _get_member(self, discord_id: str) -> dict[str, object] | None:
        response = await self._client.get(f"/guilds/{self._config.discord_guild_id}/members/{discord_id}")
        if response.status_code == 404:
            return None
        response.raise_for_status()
        return response.json()

    async def _add_role(self, discord_id: str, role_id: str) -> None:
        await self._client.put(f"/guilds/{self._config.discord_guild_id}/members/{discord_id}/roles/{role_id}")

    async def _remove_role(self, discord_id: str, role_id: str) -> None:
        await self._client.delete(f"/guilds/{self._config.discord_guild_id}/members/{discord_id}/roles/{role_id}")

