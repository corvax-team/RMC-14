# Discord Bot

Отдельный Python-сервис для синхронизации Discord-ролей и CCM sponsorship.

Он не обрабатывает привязку аккаунта. Привязка идёт через `Tools/DiscordAuth` из лобби.

## Что делает

- Каждые `DISCORD_ROLE_SYNC_INTERVAL_SECONDS` секунд проверяет только `rmc_linked_accounts`.
- Для каждого привязанного аккаунта точечно запрашивает конкретного участника Discord.
- Сравнивает роли спонсора и записывает результат в `ccm_player_sponsorship`.
- Синхронизирует роль `Привязан` для уже привязанных аккаунтов.
- Ставит slash-команды:
  - `/bind-account`
  - `/unlink-account`
  - `/ckey`
  - `/discord`

## Конфиг

Один общий конфиг для обоих сервисов:

```powershell
Copy-Item ..\..\Tools\.env.example ..\..\Tools\.env
```

Основные переменные:

```text
DISCORD_BOT_TOKEN=
DISCORD_GUILD_ID=
DISCORD_LINKED_ROLE_ID=
DISCORD_LINKED_ROLE_NAME=Привязан
DISCORD_SPONSOR_I_ROLE_ID=
DISCORD_SPONSOR_II_ROLE_ID=
DISCORD_SPONSOR_III_ROLE_ID=
DISCORD_ROLE_SYNC_INTERVAL_SECONDS=10800
DISCORD_REQUEST_TIMEOUT_SECONDS=20
DISCORD_MAX_CONCURRENCY=8
CCM_SPONSORSHIP_ROLLING_DAYS=31
DATABASE_PROVIDER=postgres
DATABASE_URL=
SQLITE_PATH=
```

## Запуск

```powershell
cd C:\Users\admin\Documents\GitHub\RMC-14\Tools\DiscordBot
python -m venv .venv
.\.venv\Scripts\python -m pip install -r requirements.txt
.\.venv\Scripts\python -m discord_bot.main
```

## Примечания

- Сервис не сканирует весь Discord-сервер. Он работает только по привязанным аккаунтам из БД.
- Параллелизм ограничен `DISCORD_MAX_CONCURRENCY`.
- Если Discord временно отвечает ошибкой, аккаунт пропускается и будет пересчитан на следующем цикле.
