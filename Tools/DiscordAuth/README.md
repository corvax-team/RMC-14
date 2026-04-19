# TGMC Discord Auth

Standalone Python service for Discord OAuth2 account linking.

This tool only handles the player link flow:

```text
game server -> /auth/login?state=... -> Discord OAuth2 -> /auth/callback -> game database
```

Discord role sync and admin slash commands live separately in `Tools/DiscordBot`.

## Configuration

Copy `.env.example` to `.env` and fill real values:

```powershell
Copy-Item .env.example .env
```

```text
DISCORD_CLIENT_ID=
DISCORD_CLIENT_SECRET=
DISCORD_REDIRECT_URI=https://auth.example.org/auth/callback
PUBLIC_BASE_URL=https://auth.example.org
DATABASE_PROVIDER=sqlite
SQLITE_PATH=discord_auth.sqlite
DATABASE_URL=
OAUTH_STATE_SECRET=same-secret-as-rmc.discord_oauth_state_secret
PORT=2424
```

For a simple local setup, `sqlite` is enough and does not require Docker or a separate database service.
Switch to `postgres` only if you specifically need the auth service to write into a shared PostgreSQL game database.

The game server must use the same state secret:

```text
rmc.discord_oauth_base_url=https://auth.example.org
rmc.discord_oauth_state_secret=<same OAUTH_STATE_SECRET>
```

If local Discord auth is not needed, keep these values empty in the server preset and the linking button flow will stay disabled.

## Run

```powershell
cd C:\Users\admin\Documents\GitHub\TGMC-14\Tools\DiscordAuth
python -m venv .venv
.\.venv\Scripts\python -m pip install -r requirements.txt
.\.venv\Scripts\python -m uvicorn discord_auth.main:app --host 0.0.0.0 --port 2424
```

## Check

```powershell
.\.venv\Scripts\python -m pytest
.\.venv\Scripts\python -m compileall .
```
