from __future__ import annotations

from dataclasses import dataclass

import httpx

from .config import AppConfig


@dataclass(frozen=True)
class GuildMember:
    user_id: str
    role_ids: frozenset[str]


class DiscordApiError(RuntimeError):
    pass


class DiscordClient:
    def __init__(self, config: AppConfig) -> None:
        self._guild_id = config.discord_guild_id
        self._client = httpx.AsyncClient(
            base_url="https://discord.com/api/v10",
            timeout=config.request_timeout_seconds,
            headers={
                "Authorization": f"Bot {config.discord_bot_token}",
                "User-Agent": "TGMC-DiscordBot/1.0",
            },
        )

    async def get_member(self, discord_id: str) -> GuildMember | None:
        response = await self._client.get(f"/guilds/{self._guild_id}/members/{discord_id}")
        if response.status_code == 404:
            return None

        if response.status_code >= 400:
            raise DiscordApiError(
                f"Discord API returned {response.status_code} for member {discord_id}: {response.text}"
            )

        payload = response.json()
        user = payload.get("user") or {}
        return GuildMember(
            user_id=str(user.get("id", discord_id)),
            role_ids=frozenset(str(role_id) for role_id in payload.get("roles", [])),
        )

    async def close(self) -> None:
        await self._client.aclose()
