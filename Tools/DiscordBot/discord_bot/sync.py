from __future__ import annotations

import asyncio
from dataclasses import dataclass
from datetime import UTC, datetime, timedelta
from enum import IntEnum

from .config import AppConfig
from .db import ApplyResult, GameDb, SyncAction
from .discord import DiscordApiError, DiscordClient


class CCMSponsorshipTier(IntEnum):
    None_ = 0
    SponsorI = 1
    SponsorII = 2
    SponsorIII = 3


@dataclass(frozen=True)
class MemberLookup:
    discord_id: str
    role_ids: frozenset[str] | None
    found: bool
    error: str | None = None


@dataclass(frozen=True)
class CycleResult:
    scanned: int
    skipped: int
    actions: int
    db: ApplyResult


class RoleSyncService:
    def __init__(self, config: AppConfig, db: GameDb, discord: DiscordClient) -> None:
        self._config = config
        self._db = db
        self._discord = discord
        self._semaphore = asyncio.Semaphore(config.max_concurrency)
        self._role_to_tier = _build_role_mapping(config)

    async def run_forever(self) -> None:
        while True:
            started = datetime.now(UTC)
            try:
                result = await self.run_cycle()
                print(
                    f"[{started.isoformat()}] sync complete: "
                    f"scanned={result.scanned} skipped={result.skipped} "
                    f"actions={result.actions} inserted={result.db.inserted} "
                    f"updated={result.db.updated} removed={result.db.removed}"
                )
            except Exception as error:
                print(f"[{started.isoformat()}] sync failed: {error}")

            await asyncio.sleep(self._config.sync_interval_seconds)

    async def run_cycle(self) -> CycleResult:
        state = await asyncio.to_thread(self._db.load_sync_state)
        if not self._role_to_tier:
            print("No CCM sponsor role mapping configured; skipping sync cycle.")
            return CycleResult(scanned=0, skipped=0, actions=0, db=ApplyResult(0, 0, 0))

        actions: list[SyncAction] = []
        skipped = 0
        batch_size = max(1, self._config.max_concurrency * 4)
        for offset in range(0, len(state.linked_accounts), batch_size):
            batch = state.linked_accounts[offset : offset + batch_size]
            lookups = await asyncio.gather(*(self._lookup_member(record.discord_id) for record in batch))
            role_sync_tasks: list[asyncio.Task[None]] = []

            for record, lookup in zip(batch, lookups, strict=True):
                if lookup.error is not None:
                    skipped += 1
                    continue

                member_roles = lookup.role_ids or frozenset()
                target_tier = _select_tier(member_roles, self._role_to_tier)
                expiration_unix_seconds = (
                    None
                    if target_tier == CCMSponsorshipTier.None_
                    else _resolve_expiration_unix_seconds(self._config)
                )
                if lookup.found:
                    role_sync_tasks.append(
                        asyncio.create_task(self._sync_linked_role(record.discord_id, linked=True, member_roles=member_roles))
                    )

                if (
                    record.current_tier_id == int(target_tier)
                    and expiration_unix_seconds is not None
                    and record.current_expiration_unix_seconds is not None
                    and record.current_expiration_unix_seconds >= expiration_unix_seconds - 3600
                ):
                    continue

                if record.current_tier_id == int(target_tier) and target_tier == CCMSponsorshipTier.None_:
                    continue

                actions.append(
                    SyncAction(
                        player_id=record.player_id,
                        current_tier_id=record.current_tier_id,
                        target_tier_id=None if target_tier == CCMSponsorshipTier.None_ else int(target_tier),
                        expiration_unix_seconds=expiration_unix_seconds,
                    )
                )

            if role_sync_tasks:
                await asyncio.gather(*role_sync_tasks)

        db_result = await asyncio.to_thread(self._db.apply_actions, actions) if actions else ApplyResult(0, 0, 0)
        return CycleResult(
            scanned=len(state.linked_accounts),
            skipped=skipped,
            actions=len(actions),
            db=db_result,
        )

    async def sync_player(self, player_id: str) -> None:
        record = await asyncio.to_thread(self._db.find_linked_account_by_player, player_id)
        if record is None:
            actions = [
                SyncAction(
                    player_id=player_id,
                    current_tier_id=None,
                    target_tier_id=None,
                    expiration_unix_seconds=None,
                )
            ]
            await asyncio.to_thread(self._db.apply_actions, actions)
            return

        lookup = await self._lookup_member(record.discord_id)
        if lookup.error is not None:
            print(f"Skipping immediate sync for {record.discord_id}: {lookup.error}")
            return

        member_roles = lookup.role_ids or frozenset()
        if not lookup.found:
            await asyncio.to_thread(
                self._db.apply_actions,
                [
                    SyncAction(
                        player_id=player_id,
                        current_tier_id=record.current_tier_id,
                        target_tier_id=None,
                        expiration_unix_seconds=None,
                    )
                ],
            )
            return

        target_tier = _select_tier(member_roles, self._role_to_tier)
        expiration_unix_seconds = (
            None
            if target_tier == CCMSponsorshipTier.None_
            else _resolve_expiration_unix_seconds(self._config)
        )
        await asyncio.to_thread(
            self._db.apply_actions,
            [
                SyncAction(
                    player_id=player_id,
                    current_tier_id=record.current_tier_id,
                    target_tier_id=None if target_tier == CCMSponsorshipTier.None_ else int(target_tier),
                    expiration_unix_seconds=expiration_unix_seconds,
                )
            ],
        )
        await self._sync_linked_role(record.discord_id, linked=True, member_roles=member_roles)

    async def remove_linked_role(self, discord_id: str) -> None:
        await self._sync_linked_role(discord_id, linked=False)

    async def _lookup_member(self, discord_id: str) -> MemberLookup:
        for attempt in range(3):
            async with self._semaphore:
                try:
                    member = await self._discord.get_member(discord_id)
                except DiscordApiError as error:
                    if error.retryable and attempt < 2:
                        await asyncio.sleep(1 << attempt)
                        continue
                    print(f"Skipping {discord_id}: {error}")
                    return MemberLookup(discord_id=discord_id, role_ids=None, found=False, error=str(error))
                except Exception as error:
                    print(f"Skipping {discord_id}: {error}")
                    return MemberLookup(discord_id=discord_id, role_ids=None, found=False, error=repr(error))

                if member is None:
                    return MemberLookup(discord_id=discord_id, role_ids=frozenset(), found=False)

                return MemberLookup(discord_id=discord_id, role_ids=member.role_ids, found=True)

        return MemberLookup(discord_id=discord_id, role_ids=None, found=False, error="retry-exhausted")

    async def _sync_linked_role(
        self,
        discord_id: str,
        linked: bool,
        member_roles: frozenset[str] | None = None,
    ) -> None:
        for attempt in range(3):
            async with self._semaphore:
                try:
                    await self._discord.sync_linked_role(discord_id, linked, member_roles)
                    return
                except DiscordApiError as error:
                    if error.retryable and attempt < 2:
                        await asyncio.sleep(1 << attempt)
                        continue
                    return


def _build_role_mapping(config: AppConfig) -> dict[str, CCMSponsorshipTier]:
    mapping: dict[str, CCMSponsorshipTier] = {}
    if config.sponsor_i_role_id:
        mapping[config.sponsor_i_role_id] = CCMSponsorshipTier.SponsorI
    if config.sponsor_ii_role_id:
        mapping[config.sponsor_ii_role_id] = CCMSponsorshipTier.SponsorII
    if config.sponsor_iii_role_id:
        mapping[config.sponsor_iii_role_id] = CCMSponsorshipTier.SponsorIII
    return mapping


def _select_tier(role_ids: frozenset[str], role_to_tier: dict[str, CCMSponsorshipTier]) -> CCMSponsorshipTier:
    matched_tier = CCMSponsorshipTier.None_
    for role_id in role_ids:
        tier = role_to_tier.get(role_id)
        if tier is not None and tier > matched_tier:
            matched_tier = tier
    return matched_tier


def _resolve_expiration_unix_seconds(config: AppConfig) -> int:
    expiration = datetime.now(UTC) + timedelta(days=config.sponsorship_rolling_days)
    return int(expiration.timestamp())
