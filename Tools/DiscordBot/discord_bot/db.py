from __future__ import annotations

import sqlite3
from dataclasses import dataclass
from typing import Protocol

from .config import AppConfig


@dataclass(frozen=True)
class LinkedAccountRecord:
    player_id: str
    discord_id: str
    current_tier_id: int | None
    current_expiration_unix_seconds: int | None


@dataclass(frozen=True)
class SyncState:
    linked_accounts: list[LinkedAccountRecord]


@dataclass(frozen=True)
class SyncAction:
    player_id: str
    current_tier_id: int | None
    target_tier_id: int | None
    expiration_unix_seconds: int | None


@dataclass(frozen=True)
class ApplyResult:
    inserted: int
    updated: int
    removed: int


class GameDb(Protocol):
    def load_sync_state(self) -> SyncState:
        ...

    def apply_actions(self, actions: list[SyncAction]) -> ApplyResult:
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

    def load_sync_state(self) -> SyncState:
        with self._psycopg.connect(self._connection_string) as conn:
            with conn.cursor() as cur:
                self._ensure_ccm_sponsorship_storage(cur)

                cur.execute(
                    """
                    SELECT
                        la.player_id::text,
                        la.discord_id::text,
                        s.tier,
                        s.expiration_unix_seconds
                    FROM rmc_linked_accounts la
                    LEFT JOIN ccm_player_sponsorship s ON s.player_id = la.player_id::text
                    ORDER BY la.player_id
                    """
                )
                linked_accounts = [
                    LinkedAccountRecord(
                        player_id=str(row[0]),
                        discord_id=str(row[1]),
                        current_tier_id=int(row[2]) if row[2] is not None else None,
                        current_expiration_unix_seconds=int(row[3]) if row[3] is not None else None,
                    )
                    for row in cur.fetchall()
                ]

        return SyncState(linked_accounts=linked_accounts)

    def apply_actions(self, actions: list[SyncAction]) -> ApplyResult:
        inserted = 0
        updated = 0
        removed = 0

        with self._psycopg.connect(self._connection_string) as conn:
            with conn.cursor() as cur:
                self._ensure_ccm_sponsorship_storage(cur)
                for action in actions:
                    if action.target_tier_id is None:
                        cur.execute(
                            "DELETE FROM ccm_player_sponsorship WHERE player_id = %s",
                            (action.player_id,),
                        )
                        removed += cur.rowcount
                        continue

                    if action.expiration_unix_seconds is None:
                        continue

                    if action.current_tier_id is None:
                        cur.execute(
                            """
                            INSERT INTO ccm_player_sponsorship (player_id, tier, expiration_unix_seconds)
                            VALUES (%s, %s, %s)
                            ON CONFLICT (player_id)
                            DO UPDATE SET
                                tier = EXCLUDED.tier,
                                expiration_unix_seconds = EXCLUDED.expiration_unix_seconds
                            """,
                            (action.player_id, action.target_tier_id, action.expiration_unix_seconds),
                        )
                        inserted += 1
                        continue

                    cur.execute(
                        """
                        UPDATE ccm_player_sponsorship
                        SET
                            tier = %s,
                            expiration_unix_seconds = %s
                        WHERE player_id = %s
                        """,
                        (action.target_tier_id, action.expiration_unix_seconds, action.player_id),
                    )
                    updated += cur.rowcount
            conn.commit()

        return ApplyResult(inserted=inserted, updated=updated, removed=removed)

    def close(self) -> None:
        return None

    @staticmethod
    def _ensure_ccm_sponsorship_storage(cur) -> None:
        cur.execute(
            """
            CREATE TABLE IF NOT EXISTS ccm_player_sponsorship (
                player_id TEXT PRIMARY KEY,
                tier INTEGER NOT NULL,
                expiration_unix_seconds BIGINT NOT NULL
            )
            """
        )


class SqliteGameDb:
    def __init__(self, path: str) -> None:
        self._conn = sqlite3.connect(path, isolation_level=None, check_same_thread=False)
        self._conn.execute("PRAGMA foreign_keys = ON")

    def load_sync_state(self) -> SyncState:
        self._ensure_ccm_sponsorship_storage()

        linked_accounts = [
            LinkedAccountRecord(
                player_id=str(row[0]),
                discord_id=str(row[1]),
                current_tier_id=int(row[2]) if row[2] is not None else None,
                current_expiration_unix_seconds=int(row[3]) if row[3] is not None else None,
            )
            for row in self._conn.execute(
                """
                SELECT
                    la.player_id,
                    la.discord_id,
                    s.tier,
                    s.expiration_unix_seconds
                FROM rmc_linked_accounts la
                LEFT JOIN ccm_player_sponsorship s ON s.player_id = la.player_id
                ORDER BY la.player_id
                """
            ).fetchall()
        ]

        return SyncState(linked_accounts=linked_accounts)

    def apply_actions(self, actions: list[SyncAction]) -> ApplyResult:
        inserted = 0
        updated = 0
        removed = 0

        with self._conn:
            self._ensure_ccm_sponsorship_storage()
            for action in actions:
                if action.target_tier_id is None:
                    cursor = self._conn.execute(
                        "DELETE FROM ccm_player_sponsorship WHERE player_id = ?",
                        (action.player_id,),
                    )
                    removed += cursor.rowcount
                    continue

                if action.expiration_unix_seconds is None:
                    continue

                if action.current_tier_id is None:
                    self._conn.execute(
                        """
                        INSERT INTO ccm_player_sponsorship (player_id, tier, expiration_unix_seconds)
                        VALUES (?, ?, ?)
                        ON CONFLICT(player_id) DO UPDATE SET
                            tier = excluded.tier,
                            expiration_unix_seconds = excluded.expiration_unix_seconds
                        """,
                        (action.player_id, action.target_tier_id, action.expiration_unix_seconds),
                    )
                    inserted += 1
                    continue

                cursor = self._conn.execute(
                    """
                    UPDATE ccm_player_sponsorship
                    SET
                        tier = ?,
                        expiration_unix_seconds = ?
                    WHERE player_id = ?
                    """,
                    (action.target_tier_id, action.expiration_unix_seconds, action.player_id),
                )
                updated += cursor.rowcount

        return ApplyResult(inserted=inserted, updated=updated, removed=removed)

    def close(self) -> None:
        self._conn.close()

    def _ensure_ccm_sponsorship_storage(self) -> None:
        self._conn.execute(
            """
            CREATE TABLE IF NOT EXISTS ccm_player_sponsorship (
                player_id TEXT PRIMARY KEY,
                tier INTEGER NOT NULL,
                expiration_unix_seconds BIGINT NOT NULL
            )
            """
        )
