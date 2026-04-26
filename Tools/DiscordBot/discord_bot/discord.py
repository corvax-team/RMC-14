from __future__ import annotations

from dataclasses import dataclass

import httpx

from .config import AppConfig


@dataclass(frozen=True)
class GuildMember:
    user_id: str
    role_ids: frozenset[str]


class DiscordApiError(RuntimeError):
    def __init__(self, message: str, *, retryable: bool = False) -> None:
        super().__init__(message)
        self.retryable = retryable


class DiscordClient:
    def __init__(self, config: AppConfig) -> None:
        self._config = config
        self._client = httpx.AsyncClient(
            base_url="https://discord.com/api/v10",
            timeout=config.request_timeout_seconds,
            headers={
                "Authorization": f"Bot {config.discord_bot_token}",
                "User-Agent": "RMC-DiscordBot/1.0",
            },
        )
        self._linked_role_id = config.linked_role_id
        self._linked_role_name = config.linked_role_name

    async def get_member(self, discord_id: str) -> GuildMember | None:
        response = await self._client.get(f"/guilds/{self._config.discord_guild_id}/members/{discord_id}")
        if response.status_code == 404:
            return None
        if response.status_code == 429 or response.status_code >= 500:
            raise DiscordApiError(
                f"Discord API returned {response.status_code} for member {discord_id}: {response.text}",
                retryable=True,
            )
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

    async def sync_linked_role(
        self,
        discord_id: str,
        linked: bool,
        member_roles: frozenset[str] | None = None,
    ) -> None:
        role_id = await self.resolve_linked_role_id()
        if role_id is None:
            return

        has_role = role_id in member_roles if member_roles is not None else await self._member_has_role(discord_id, role_id)
        if linked and not has_role:
            await self._client.put(f"/guilds/{self._config.discord_guild_id}/members/{discord_id}/roles/{role_id}")
        elif not linked and has_role:
            await self._client.delete(f"/guilds/{self._config.discord_guild_id}/members/{discord_id}/roles/{role_id}")

    async def close(self) -> None:
        await self._client.aclose()

    async def resolve_linked_role_id(self) -> str | None:
        if self._linked_role_id:
            return self._linked_role_id

        response = await self._client.get(f"/guilds/{self._config.discord_guild_id}/roles")
        if response.status_code == 429 or response.status_code >= 500:
            raise DiscordApiError(
                f"Discord API returned {response.status_code} while resolving linked role: {response.text}",
                retryable=True,
            )
        response.raise_for_status()
        for role in response.json():
            if str(role.get("name", "")).strip().lower() == self._linked_role_name.lower():
                self._linked_role_id = str(role["id"])
                return self._linked_role_id
        return None

    async def _member_has_role(self, discord_id: str, role_id: str) -> bool:
        member = await self.get_member(discord_id)
        if member is None:
            return False
        return role_id in member.role_ids
