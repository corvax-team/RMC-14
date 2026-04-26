from __future__ import annotations

import asyncio
from contextlib import suppress

import discord
from discord import app_commands
from discord.ext import commands

from .config import AppConfig, load_config
from .db import GameDb, LinkResult, create_game_db
from .discord import DiscordClient
from .sync import RoleSyncService


class AccountCommandsCog(commands.Cog):
    def __init__(self, bot: "DiscordBot", config: AppConfig, db: GameDb, sync_service: RoleSyncService) -> None:
        self.bot = bot
        self.config = config
        self.db = db
        self.sync_service = sync_service

    @app_commands.command(name="ping", description="Check whether the Discord bot is responding.")
    @app_commands.guild_only()
    async def ping(self, interaction: discord.Interaction) -> None:
        await interaction.response.send_message("pong", ephemeral=True)

    @app_commands.command(name="bind-account", description="Manually bind a Discord account to an SS14 ckey.")
    @app_commands.guild_only()
    @app_commands.checks.has_permissions(manage_roles=True)
    async def bind_account(self, interaction: discord.Interaction, user: discord.Member, ckey: str) -> None:
        await interaction.response.defer(ephemeral=True)
        player = await asyncio.to_thread(self.db.find_player_by_ckey, ckey)
        if player is None:
            await interaction.followup.send(f"No player found for ckey `{ckey.strip()}`.", ephemeral=True)
            return

        result = await asyncio.to_thread(self.db.link_account, player.player_id, str(user.id))
        await self._sync_after_link(result)
        await interaction.followup.send(_format_link_result(result, player.ckey), ephemeral=True)

    @app_commands.command(name="unlink-account", description="Manually unlink a Discord account.")
    @app_commands.guild_only()
    @app_commands.checks.has_permissions(manage_roles=True)
    async def unlink_account(self, interaction: discord.Interaction, user: discord.Member) -> None:
        await interaction.response.defer(ephemeral=True)
        result = await asyncio.to_thread(self.db.unlink_account, str(user.id))
        if result.status == "not-linked" or result.player_id is None:
            await interaction.followup.send("This Discord account is not linked.", ephemeral=True)
            return

        try:
            await self.sync_service.sync_player(result.player_id)
            await self.sync_service.remove_linked_role(result.discord_id or str(user.id))
        except Exception as error:
            print(f"Immediate unlink sync failed for {user.id}: {error}")
        await interaction.followup.send(f"Unlinked `{result.player_id}` from {user.mention}.", ephemeral=True)

    async def _sync_after_link(self, result: LinkResult) -> None:
        try:
            if result.evicted_player_id:
                await self.sync_service.sync_player(result.evicted_player_id)
            if result.player_id:
                await self.sync_service.sync_player(result.player_id)
            if result.displaced_discord_id:
                await self.sync_service.remove_linked_role(result.displaced_discord_id)
        except Exception as error:
            print(f"Immediate link sync failed: {error}")

    @app_commands.command(name="ckey", description="Find the SS14 ckey linked to a Discord user.")
    @app_commands.guild_only()
    @app_commands.checks.has_permissions(manage_roles=True)
    async def find_ckey(self, interaction: discord.Interaction, user: discord.Member) -> None:
        await interaction.response.defer(ephemeral=True)
        record = await asyncio.to_thread(self.db.find_linked_account_by_discord, str(user.id))
        if record is None:
            await interaction.followup.send(f"{user.mention} is not linked to any SS14 account.", ephemeral=True)
            return

        ckey = await asyncio.to_thread(self._resolve_ckey, record.player_id)
        await interaction.followup.send(f"{user.mention} -> `{ckey or record.player_id}`", ephemeral=True)

    @app_commands.command(name="discord", description="Mention the linked Discord user by SS14 ckey.")
    @app_commands.guild_only()
    @app_commands.checks.has_permissions(manage_roles=True)
    async def find_discord(self, interaction: discord.Interaction, ckey: str) -> None:
        await interaction.response.defer(ephemeral=True)
        player = await asyncio.to_thread(self.db.find_player_by_ckey, ckey)
        if player is None:
            await interaction.followup.send(f"No player found for ckey `{ckey.strip()}`.", ephemeral=True)
            return

        linked = await asyncio.to_thread(self.db.find_linked_account_by_player, player.player_id)
        if linked is None:
            await interaction.followup.send(f"`{player.ckey}` is not linked to any Discord account.", ephemeral=True)
            return

        await interaction.followup.send(f"`{player.ckey}` -> <@{linked.discord_id}>", ephemeral=True)

    def _resolve_ckey(self, player_id: str) -> str | None:
        record = self.db.find_player_by_id(player_id)
        if record is None:
            return None
        return record.ckey


class DiscordBot(commands.Bot):
    def __init__(
        self,
        config: AppConfig,
        db: GameDb,
        rest_client: DiscordClient,
        sync_service: RoleSyncService,
    ) -> None:
        intents = discord.Intents.default()
        intents.guilds = True
        super().__init__(command_prefix=commands.when_mentioned, intents=intents)
        self.config = config
        self.db = db
        self.rest_client = rest_client
        self.sync_service = sync_service
        self.account_commands: AccountCommandsCog | None = None
        self._sync_task: asyncio.Task[None] | None = None

    async def setup_hook(self) -> None:
        self.account_commands = AccountCommandsCog(self, self.config, self.db, self.sync_service)
        await self.add_cog(self.account_commands)
        guild = discord.Object(id=int(self.config.discord_guild_id))
        self.tree.clear_commands(guild=guild)
        self.tree.copy_global_to(guild=guild)
        synced = await self.tree.sync(guild=guild)
        print(f"Synced {len(synced)} Discord slash commands to guild {self.config.discord_guild_id}.")
        self._sync_task = asyncio.create_task(self.sync_service.run_forever())

    async def close(self) -> None:
        if self._sync_task is not None:
            self._sync_task.cancel()
            with suppress(asyncio.CancelledError):
                await self._sync_task
        await self.rest_client.close()
        await super().close()


def _format_link_result(result: LinkResult, ckey: str) -> str:
    if result.status == "already-linked":
        return f"`{ckey}` is already linked."

    message = f"Linked SS14 account `{ckey}`."
    if result.evicted_player_id:
        message += f" Replaced previous Discord link for player `{result.evicted_player_id}`."
    if result.displaced_discord_id:
        message += f" Removed the old Discord link `<@{result.displaced_discord_id}>`."
    return message


async def _main() -> None:
    config = load_config()
    db = create_game_db(config)
    rest_client = DiscordClient(config)
    sync_service = RoleSyncService(config, db, rest_client)
    bot = DiscordBot(config, db, rest_client, sync_service)

    try:
        await bot.start(config.discord_bot_token)
    finally:
        db.close()


def main() -> None:
    asyncio.run(_main())


if __name__ == "__main__":
    main()
