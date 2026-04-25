# TGMC Discord Bot

Standalone Discord role sync worker for CCM sponsorship.

This service does not handle OAuth login. OAuth linking stays in `Tools/DiscordAuth`.
The bot uses:

- `rmc_linked_accounts`: which game account is linked to which Discord account
- `ccm_player_sponsorship`: active stored CCM sponsorship for the player

## How it works

Every sync cycle the bot:

1. Loads all linked game accounts from `rmc_linked_accounts`
2. Fetches the linked member from Discord guild API
3. Maps Discord roles from `.env` to `CCMSponsorshipTier`
4. Picks the highest matching CCM tier
5. Inserts, updates, or removes the row in `ccm_player_sponsorship`

If a Discord account is no longer in the guild or no longer has sponsor roles, the bot removes the CCM sponsorship row.
If Discord API returns an error for a specific user, that user is skipped for the current cycle so the bot does not revoke access because of a transient failure.

## Required setup

Fill the Discord role IDs in `.env`:

- `DISCORD_SPONSOR_I_ROLE_ID`
- `DISCORD_SPONSOR_II_ROLE_ID`
- `DISCORD_SPONSOR_III_ROLE_ID`

The bot writes rolling expirations into `ccm_player_sponsorship`.
`CCM_SPONSORSHIP_ROLLING_DAYS` controls how far into the future the stored subscription is extended on every successful sync.

## Configuration

Copy `.env.example` to `.env` and fill real values:

```powershell
Copy-Item .env.example .env
```

```text
DISCORD_BOT_TOKEN=
DISCORD_GUILD_ID=
DATABASE_PROVIDER=postgres
DATABASE_URL=Host=127.0.0.1;Port=5432;Database=tgmc14;Username=postgres;Password=postgres
SQLITE_PATH=
DISCORD_SPONSOR_I_ROLE_ID=
DISCORD_SPONSOR_II_ROLE_ID=
DISCORD_SPONSOR_III_ROLE_ID=
CCM_SPONSORSHIP_ROLLING_DAYS=31
DISCORD_ROLE_SYNC_INTERVAL_SECONDS=900
DISCORD_REQUEST_TIMEOUT_SECONDS=20
DISCORD_MAX_CONCURRENCY=10
```

Use the same database as the game server if you want the bot to manage live CCM sponsorship state.
`SQLITE_PATH` should stay empty in the normal Postgres setup.

## Run

```powershell
cd C:\Users\admin\Documents\GitHub\TGMC-14\Tools\DiscordBot
python -m venv .venv
.\.venv\Scripts\python -m pip install -r requirements.txt
.\.venv\Scripts\python -m discord_bot.main
```

## Notes

- The bot uses Discord HTTP API with the bot token.
- It expects the bot to be a member of the target guild.
- The bot should have access to view the guild members it needs to query.
- The sync interval is set in seconds. `900` is 15 minutes.
- CCM tier mapping is:
  - `SponsorI = 1`
  - `SponsorII = 2`
  - `SponsorIII = 3`
