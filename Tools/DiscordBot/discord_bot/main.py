from __future__ import annotations

import asyncio

from .config import load_config
from .db import create_game_db
from .discord import DiscordClient
from .sync import RoleSyncService


async def _main() -> None:
    config = load_config()
    db = create_game_db(config)
    discord = DiscordClient(config)

    try:
        service = RoleSyncService(config, db, discord)
        await service.run_forever()
    finally:
        await discord.close()
        db.close()


def main() -> None:
    asyncio.run(_main())


if __name__ == "__main__":
    main()
