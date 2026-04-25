from __future__ import annotations

import sqlite3
from dataclasses import dataclass
from typing import Literal, Protocol

from .config import AppConfig


LinkStatus = Literal["linked", "already-linked", "player-conflict", "discord-conflict"]


@dataclass(frozen=True)
class LinkResult:
    status: LinkStatus


class GameDb(Protocol):
    def link_account(self, player_id: str, discord_id: str) -> LinkResult:
        ...

    def close(self) -> None:
        ...


def create_game_db(config: AppConfig) -> GameDb:
    if config.database_provider == "sqlite":
        return SqliteGameDb(config.sqlite_path or "")
    return PostgresGameDb(config.database_url or "")


class PostgresGameDb:
    def __init__(self, connection_string: str) -> None:
        import psycopg

        self._psycopg = psycopg
        self._connection_string = connection_string

    def link_account(self, player_id: str, discord_id: str) -> LinkResult:
        with self._psycopg.connect(self._connection_string) as conn:
            with conn.cursor() as cur:
                cur.execute(
                    """
                    SELECT player_id::text, discord_id::text
                    FROM rmc_linked_accounts
                    WHERE player_id = %s::uuid OR discord_id = %s::numeric
                    """,
                    (player_id, discord_id),
                )
                result = _get_conflict_result(cur.fetchall(), player_id, discord_id)
                if result.status != "linked":
                    conn.rollback()
                    return result

                cur.execute(
                    """
                    INSERT INTO rmc_discord_accounts (rmc_discord_accounts_id)
                    VALUES (%s::numeric)
                    ON CONFLICT DO NOTHING
                    """,
                    (discord_id,),
                )
                cur.execute(
                    """
                    INSERT INTO rmc_linked_accounts (player_id, discord_id)
                    VALUES (%s::uuid, %s::numeric)
                    """,
                    (player_id, discord_id),
                )
                cur.execute(
                    """
                    INSERT INTO rmc_linked_accounts_logs (player_id, discord_id, at)
                    VALUES (%s::uuid, %s::numeric, NOW())
                    """,
                    (player_id, discord_id),
                )
            conn.commit()
        return LinkResult("linked")

    def close(self) -> None:
        return None


class SqliteGameDb:
    def __init__(self, path: str) -> None:
        self._conn = sqlite3.connect(path, isolation_level=None, check_same_thread=False)
        self._conn.execute("PRAGMA foreign_keys = ON")

    def link_account(self, player_id: str, discord_id: str) -> LinkResult:
        with self._conn:
            rows = self._conn.execute(
                """
                SELECT player_id, discord_id
                FROM rmc_linked_accounts
                WHERE player_id = ? OR discord_id = ?
                """,
                (player_id, int(discord_id)),
            ).fetchall()
            result = _get_conflict_result(rows, player_id, discord_id)
            if result.status != "linked":
                return result

            self._conn.execute(
                "INSERT OR IGNORE INTO rmc_discord_accounts (rmc_discord_accounts_id) VALUES (?)",
                (int(discord_id),),
            )
            self._conn.execute(
                "INSERT INTO rmc_linked_accounts (player_id, discord_id) VALUES (?, ?)",
                (player_id, int(discord_id)),
            )
            self._conn.execute(
                "INSERT INTO rmc_linked_accounts_logs (player_id, discord_id, at) VALUES (?, ?, datetime('now'))",
                (player_id, int(discord_id)),
            )
        return LinkResult("linked")

    def close(self) -> None:
        self._conn.close()


def _get_conflict_result(rows: list[tuple[object, object]], player_id: str, discord_id: str) -> LinkResult:
    normalized_player_id = player_id.lower()
    for row_player_id, row_discord_id in rows:
        if str(row_player_id).lower() == normalized_player_id:
            return LinkResult("already-linked" if str(row_discord_id) == discord_id else "player-conflict")

    for _row_player_id, row_discord_id in rows:
        if str(row_discord_id) == discord_id:
            return LinkResult("discord-conflict")

    return LinkResult("linked")
