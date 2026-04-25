from __future__ import annotations

import base64
import hashlib
import hmac
import json
import secrets
import time
from dataclasses import dataclass
from typing import Literal


@dataclass(frozen=True)
class OAuthStatePayload:
    playerId: str
    expires: int
    nonce: str
    locale: str


@dataclass(frozen=True)
class StateVerifyResult:
    ok: bool
    payload: OAuthStatePayload | None = None
    error: Literal["malformed", "invalid-signature", "expired"] | None = None


def create_state(payload: dict[str, object], secret: str) -> str:
    full_payload = dict(payload)
    full_payload.setdefault("nonce", base64_url_encode(secrets.token_bytes(16)))
    payload_json = json.dumps(full_payload, separators=(",", ":"), ensure_ascii=False).encode("utf-8")
    payload_base64 = base64_url_encode(payload_json)
    return f"{payload_base64}.{sign_payload(payload_base64, secret)}"


def verify_state(state: str, secret: str, now_seconds: int | None = None) -> StateVerifyResult:
    parts = state.split(".")
    if len(parts) != 2 or not parts[0] or not parts[1]:
        return StateVerifyResult(False, error="malformed")

    payload_base64, signature = parts
    if not hmac.compare_digest(signature, sign_payload(payload_base64, secret)):
        return StateVerifyResult(False, error="invalid-signature")

    try:
        raw_payload = json.loads(base64_url_decode(payload_base64).decode("utf-8"))
        payload = OAuthStatePayload(
            playerId=str(raw_payload["playerId"]),
            expires=int(raw_payload["expires"]),
            nonce=str(raw_payload["nonce"]),
            locale=str(raw_payload.get("locale", "")),
        )
    except (KeyError, TypeError, ValueError, json.JSONDecodeError):
        return StateVerifyResult(False, error="malformed")

    now = int(time.time()) if now_seconds is None else now_seconds
    if payload.expires < now:
        return StateVerifyResult(False, error="expired")

    return StateVerifyResult(True, payload=payload)


def sign_payload(payload_base64: str, secret: str) -> str:
    digest = hmac.new(secret.encode("utf-8"), payload_base64.encode("utf-8"), hashlib.sha256).digest()
    return base64_url_encode(digest)


def base64_url_encode(data: bytes) -> str:
    return base64.urlsafe_b64encode(data).decode("ascii").rstrip("=")


def base64_url_decode(data: str) -> bytes:
    padding = "=" * ((4 - len(data) % 4) % 4)
    return base64.urlsafe_b64decode(data + padding)

