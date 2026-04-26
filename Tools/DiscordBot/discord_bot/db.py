from __future__ import annotations

import sqlite3
from dataclasses import dataclass
from datetime import UTC, datetime
from typing import Literal, Protocol

from .config import AppConfig


LinkStatus = Literal["linked", "already-linked", "player-conflict", "discord-conflict", "not-found"]
UnlinkStatus = Literal["unlinked", "not-linked"]


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
class LinkCodeRecord:
    player_id: str
    ckey: str
    code: str
    creation_time: datetime


@dataclass(frozen=True)
class PlayerRecord:
    player_id: str
    ckey: str


@dataclass(frozen=True)
class LinkResult:
    status: LinkStatus
    player_id: str | None = None
    evicted_player_id: str | None = None
    displaced_discord_id: str | None = None


@dataclass(frozen=True)
class UnlinkResult:
    status: UnlinkStatus
    player_id: str | None = None
    discord_id: str | None = None


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
    def find_player_by_ckey(self, ckey: str) -> PlayerRecord | None:
        ...

    def find_player_by_id(self, player_id: str) -> PlayerRecord | None:
        ...

    def find_link_code(self, code: str) -> LinkCodeRecord | None:
        ...

    def find_linked_account_by_discord(self, discord_id: str) -> LinkedAccountRecord | None:
        ...

    def find_linked_account_by_player(self, player_id: str) -> LinkedAccountRecord | None:
        ...

    def link_account(self, player_id: str, discord_id: str) -> LinkResult:
        ...

    def unlink_account(self, discord_id: str) -> UnlinkResult:
        ...

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

    def find_player_by_ckey(self, ckey: str) -> PlayerRecord | None:
        with self._psycopg.connect(self._connection_string) as conn:
            with conn.cursor() as cur:
                cur.execute(
                    """
                    SELECT user_id::text, last_seen_user_name
                    FROM player
                    WHERE lower(last_seen_user_name) = lower(%s)
                    """,
                    (ckey.strip(),),
                )
                row = cur.fetchone()
                if row is None:
                    return None
                return PlayerRecord(player_id=str(row[0]), ckey=str(row[1]))

    def find_player_by_id(self, player_id: str) -> PlayerRecord | None:
        with self._psycopg.connect(self._connection_string) as conn:
            with conn.cursor() as cur:
                cur.execute(
                    """
                    SELECT user_id::text, last_seen_user_name
                    FROM player
                    WHERE user_id = %s::uuid
                    """,
                    (player_id,),
                )
                row = cur.fetchone()
                if row is None:
                    return None
                return PlayerRecord(player_id=str(row[0]), ckey=str(row[1]))

    def find_link_code(self, code: str) -> LinkCodeRecord | None:
        with self._psycopg.connect(self._connection_string) as conn:
            with conn.cursor() as cur:
                cur.execute(
                    """
                    SELECT p.user_id::text, p.last_seen_user_name, lc.code::text, lc.creation_time
                    FROM rmc_linking_codes lc
                    JOIN player p ON p.user_id = lc.player_id
                    WHERE lc.code::text = %s
                    """,
                    (code.strip(),),
                )
                row = cur.fetchone()
                if row is None:
                    return None
                return LinkCodeRecord(
                    player_id=str(row[0]),
                    ckey=str(row[1]),
                    code=str(row[2]),
                    creation_time=_as_utc_datetime(row[3]),
                )

    def find_linked_account_by_discord(self, discord_id: str) -> LinkedAccountRecord | None:
        with self._psycopg.connect(self._connection_string) as conn:
            with conn.cursor() as cur:
                cur.execute(
                    """
                    SELECT
                        la.player_id::text,
                        la.discord_id::text,
                        s.tier,
                        s.expiration_unix_seconds
                    FROM rmc_linked_accounts la
                    LEFT JOIN ccm_player_sponsorship s ON s.player_id = la.player_id::text
                    WHERE la.discord_id = %s::numeric
                    """,
                    (discord_id,),
                )
                row = cur.fetchone()
                return _linked_account_from_row(row) if row is not None else None

    def find_linked_account_by_player(self, player_id: str) -> LinkedAccountRecord | None:
        with self._psycopg.connect(self._connection_string) as conn:
            with conn.cursor() as cur:
                cur.execute(
                    """
                    SELECT
                        la.player_id::text,
                        la.discord_id::text,
                        s.tier,
                        s.expiration_unix_seconds
                    FROM rmc_linked_accounts la
                    LEFT JOIN ccm_player_sponsorship s ON s.player_id = la.player_id::text
                    WHERE la.player_id = %s::uuid
                    """,
                    (player_id,),
                )
                row = cur.fetchone()
                return _linked_account_from_row(row) if row is not None else None

    def link_account(self, player_id: str, discord_id: str) -> LinkResult:
        with self._psycopg.connect(self._connection_string) as conn:
            with conn.cursor() as cur:
                current_discord = self._fetch_link_by_discord(cur, discord_id)
                current_player = self._fetch_link_by_player(cur, player_id)

                if current_discord is not None and current_discord[0] == player_id:
                    return LinkResult("already-linked", player_id=player_id)

                evicted_player_id = current_discord[0] if current_discord is not None else None
                displaced_discord_id = (
                    current_player[1]
                    if current_player is not None and current_player[1] != discord_id
                    else None
                )

                if current_discord is not None:
                    cur.execute(
                        "DELETE FROM rmc_linked_accounts WHERE player_id = %s::uuid AND discord_id = %s::numeric",
                        (current_discord[0], discord_id),
                    )

                if current_player is not None:
                    cur.execute(
                        "DELETE FROM rmc_linked_accounts WHERE player_id = %s::uuid AND discord_id = %s::numeric",
                        (player_id, current_player[1]),
                    )

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
                    ON CONFLICT DO NOTHING
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

        return LinkResult(
            "linked",
            player_id=player_id,
            evicted_player_id=evicted_player_id,
            displaced_discord_id=displaced_discord_id,
        )

    def unlink_account(self, discord_id: str) -> UnlinkResult:
        with self._psycopg.connect(self._connection_string) as conn:
            with conn.cursor() as cur:
                linked = self._fetch_link_by_discord(cur, discord_id)
                if linked is None:
                    return UnlinkResult("not-linked")

                cur.execute(
                    "DELETE FROM rmc_linked_accounts WHERE player_id = %s::uuid AND discord_id = %s::numeric",
                    (linked[0], discord_id),
                )
            conn.commit()

        return UnlinkResult("unlinked", player_id=linked[0], discord_id=discord_id)

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

    def _fetch_link_by_discord(self, cur, discord_id: str) -> tuple[str, str] | None:
        cur.execute(
            """
            SELECT player_id::text, discord_id::text
            FROM rmc_linked_accounts
            WHERE discord_id = %s::numeric
            """,
            (discord_id,),
        )
        row = cur.fetchone()
        if row is None:
            return None
        return str(row[0]), str(row[1])

    def _fetch_link_by_player(self, cur, player_id: str) -> tuple[str, str] | None:
        cur.execute(
            """
            SELECT player_id::text, discord_id::text
            FROM rmc_linked_accounts
            WHERE player_id = %s::uuid
            """,
            (player_id,),
        )
        row = cur.fetchone()
        if row is None:
            return None
        return str(row[0]), str(row[1])

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

    def find_player_by_ckey(self, ckey: str) -> PlayerRecord | None:
        row = self._conn.execute(
            """
            SELECT user_id, last_seen_user_name
            FROM player
            WHERE lower(last_seen_user_name) = lower(?)
            """,
            (ckey.strip(),),
        ).fetchone()
        if row is None:
            return None
        return PlayerRecord(player_id=str(row[0]), ckey=str(row[1]))

    def find_player_by_id(self, player_id: str) -> PlayerRecord | None:
        row = self._conn.execute(
            """
            SELECT user_id, last_seen_user_name
            FROM player
            WHERE user_id = ?
            """,
            (player_id,),
        ).fetchone()
        if row is None:
            return None
        return PlayerRecord(player_id=str(row[0]), ckey=str(row[1]))

    def find_link_code(self, code: str) -> LinkCodeRecord | None:
        row = self._conn.execute(
            """
            SELECT p.user_id, p.last_seen_user_name, lc.code, lc.creation_time
            FROM rmc_linking_codes lc
            JOIN player p ON p.user_id = lc.player_id
            WHERE lc.code = ?
            """,
            (code.strip(),),
        ).fetchone()
        if row is None:
            return None
        return LinkCodeRecord(
            player_id=str(row[0]),
            ckey=str(row[1]),
            code=str(row[2]),
            creation_time=_as_utc_datetime(row[3]),
        )

    def find_linked_account_by_discord(self, discord_id: str) -> LinkedAccountRecord | None:
        row = self._conn.execute(
            """
            SELECT
                la.player_id,
                la.discord_id,
                s.tier,
                s.expiration_unix_seconds
            FROM rmc_linked_accounts la
            LEFT JOIN ccm_player_sponsorship s ON s.player_id = la.player_id
            WHERE la.discord_id = ?
            """,
            (int(discord_id),),
        ).fetchone()
        return _linked_account_from_row(row) if row is not None else None

    def find_linked_account_by_player(self, player_id: str) -> LinkedAccountRecord | None:
        row = self._conn.execute(
            """
            SELECT
                la.player_id,
                la.discord_id,
                s.tier,
                s.expiration_unix_seconds
            FROM rmc_linked_accounts la
            LEFT JOIN ccm_player_sponsorship s ON s.player_id = la.player_id
            WHERE la.player_id = ?
            """,
            (player_id,),
        ).fetchone()
        return _linked_account_from_row(row) if row is not None else None

    def link_account(self, player_id: str, discord_id: str) -> LinkResult:
        with self._conn:
            current_discord = self._fetch_link_by_discord(discord_id)
            current_player = self._fetch_link_by_player(player_id)

            if current_discord is not None and current_discord[0] == player_id:
                return LinkResult("already-linked", player_id=player_id)

            evicted_player_id = current_discord[0] if current_discord is not None else None
            displaced_discord_id = (
                current_player[1]
                if current_player is not None and current_player[1] != discord_id
                else None
            )

            if current_discord is not None:
                self._conn.execute(
                    "DELETE FROM rmc_linked_accounts WHERE player_id = ? AND discord_id = ?",
                    (current_discord[0], int(discord_id)),
                )

            if current_player is not None:
                self._conn.execute(
                    "DELETE FROM rmc_linked_accounts WHERE player_id = ? AND discord_id = ?",
                    (player_id, int(current_player[1])),
                )

            self._conn.execute(
                "INSERT OR IGNORE INTO rmc_discord_accounts (rmc_discord_accounts_id) VALUES (?)",
                (int(discord_id),),
            )
            self._conn.execute(
                "INSERT OR REPLACE INTO rmc_linked_accounts (player_id, discord_id) VALUES (?, ?)",
                (player_id, int(discord_id)),
            )
            self._conn.execute(
                "INSERT INTO rmc_linked_accounts_logs (player_id, discord_id, at) VALUES (?, ?, datetime('now'))",
                (player_id, int(discord_id)),
            )

        return LinkResult(
            "linked",
            player_id=player_id,
            evicted_player_id=evicted_player_id,
            displaced_discord_id=displaced_discord_id,
        )

    def unlink_account(self, discord_id: str) -> UnlinkResult:
        with self._conn:
            linked = self._fetch_link_by_discord(discord_id)
            if linked is None:
                return UnlinkResult("not-linked")

            self._conn.execute(
                "DELETE FROM rmc_linked_accounts WHERE player_id = ? AND discord_id = ?",
                (linked[0], int(discord_id)),
            )

        return UnlinkResult("unlinked", player_id=linked[0], discord_id=discord_id)

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

    def _fetch_link_by_discord(self, discord_id: str) -> tuple[str, str] | None:
        row = self._conn.execute(
            """
            SELECT player_id, discord_id
            FROM rmc_linked_accounts
            WHERE discord_id = ?
            """,
            (int(discord_id),),
        ).fetchone()
        if row is None:
            return None
        return str(row[0]), str(row[1])

    def _fetch_link_by_player(self, player_id: str) -> tuple[str, str] | None:
        row = self._conn.execute(
            """
            SELECT player_id, discord_id
            FROM rmc_linked_accounts
            WHERE player_id = ?
            """,
            (player_id,),
        ).fetchone()
        if row is None:
            return None
        return str(row[0]), str(row[1])

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


def _linked_account_from_row(row: tuple[object, object, object, object] | None) -> LinkedAccountRecord | None:
    if row is None:
        return None
    return LinkedAccountRecord(
        player_id=str(row[0]),
        discord_id=str(row[1]),
        current_tier_id=int(row[2]) if row[2] is not None else None,
        current_expiration_unix_seconds=int(row[3]) if row[3] is not None else None,
    )


def _as_utc_datetime(value: object) -> datetime:
    if isinstance(value, datetime):
        if value.tzinfo is None:
            return value.replace(tzinfo=UTC)
        return value.astimezone(UTC)

    text = str(value).replace(" ", "T")
    parsed = datetime.fromisoformat(text)
    if parsed.tzinfo is None:
        return parsed.replace(tzinfo=UTC)
    return parsed.astimezone(UTC)
