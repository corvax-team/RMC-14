# Discord Auth

Автономный Python-сервис для OAuth-привязки Discord-аккаунта к игровому аккаунту.

Это отдельный сервис. Он не зависит от старого C#-бота и не требует C#-проекта.

## Что делает

- Отдаёт страницу входа через Discord OAuth2.
- После успешного callback пишет привязку в БД.
- Сразу выдаёт роль `Привязан` конкретному пользователю в Discord.

## Конфиг

Используется общий `Tools/.env`:

```powershell
Copy-Item ..\..\Tools\.env.example ..\..\Tools\.env
```

Основные переменные:

```text
DISCORD_CLIENT_ID=
DISCORD_CLIENT_SECRET=
DISCORD_REDIRECT_URI=http://127.0.0.1:2424/auth/callback
PUBLIC_BASE_URL=http://127.0.0.1:2424
OAUTH_STATE_SECRET=
DISCORD_BOT_TOKEN=
DISCORD_GUILD_ID=
DISCORD_LINKED_ROLE_ID=
DISCORD_LINKED_ROLE_NAME=Привязан
DATABASE_PROVIDER=postgres
DATABASE_URL=
SQLITE_PATH=
PORT=2424
```

## Запуск

```powershell
cd C:\Users\admin\Documents\GitHub\RMC-14\Tools\DiscordAuth
python -m venv .venv
.\.venv\Scripts\python -m pip install -r requirements.txt
.\.venv\Scripts\python -m uvicorn discord_auth.main:app --host 0.0.0.0 --port 2424
```

## Примечания

- Если роль `Привязан` не задана по `DISCORD_LINKED_ROLE_ID`, сервис попробует найти её по имени.
- Если выдача роли временно не удалась, сама привязка в БД всё равно сохранится.
- Для локальной разработки можно оставить `DATABASE_PROVIDER=sqlite`.
