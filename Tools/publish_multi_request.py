#!/usr/bin/env python3

import argparse
import requests
import os
import subprocess
from typing import Iterable

FORK_ID = os.environ.get("FORK_ID", "colonialmarines")
PUBLISH_TOKEN = os.environ["PUBLISH_TOKEN"]
VERSION = os.environ["GITHUB_SHA"]

RELEASE_DIR = "release"
ROBUST_CDN_URL = "https://cdn.corvaxcm.space/"

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--fork-id", default=FORK_ID, help="ID форка для публикации")
    args = parser.parse_args()
    fork_id = args.fork_id

    session = requests.Session()
    session.headers = {
        "Authorization": f"Bearer {PUBLISH_TOKEN}",
    }

    print(f"Starting publish on Robust.Cdn for fork '{fork_id}' and version {VERSION}")

    data = {
        "version": VERSION,
        "engineVersion": get_engine_version(),
    }
    headers = {
        "Content-Type": "application/json"
    }
    resp = session.post(f"{ROBUST_CDN_URL}fork/{fork_id}/publish/start", json=data, headers=headers)
    resp.raise_for_status()
    print("Publish successfully started, adding files...")

    for file in get_files_to_publish():
        print(f"Publishing {file}")
        with open(file, "rb") as f:
            headers = {
                "Content-Type": "application/octet-stream",
                "Robust-Cdn-Publish-File": os.path.basename(file),
                "Robust-Cdn-Publish-Version": VERSION
            }
            resp = session.post(f"{ROBUST_CDN_URL}fork/{fork_id}/publish/file", data=f, headers=headers)
            resp.raise_for_status()

    print("Successfully pushed files, finishing publish...")

    data = {"version": VERSION}
    headers = {"Content-Type": "application/json"}
    resp = session.post(f"{ROBUST_CDN_URL}fork/{fork_id}/publish/finish", json=data, headers=headers)
    resp.raise_for_status()

    print("SUCCESS!")

def get_files_to_publish() -> Iterable[str]:
    for file in os.listdir(RELEASE_DIR):
        yield os.path.join(RELEASE_DIR, file)

def get_engine_version() -> str:
    try:
        proc = subprocess.run(
            ["git", "describe", "--tags", "--abbrev=0"],
            stdout=subprocess.PIPE,
            cwd="RobustToolbox",
            check=True,
            encoding="UTF-8"
        )
        tag = proc.stdout.strip()

        if not tag:
            raise Exception("empty tag")

        if tag.startswith("v"):
            return tag[1:]

        return tag

    except Exception:
        # fallback commit hash
        proc = subprocess.run(
            ["git", "rev-parse", "HEAD"],
            stdout=subprocess.PIPE,
            cwd="RobustToolbox",
            check=True,
            encoding="UTF-8"
        )
        return proc.stdout.strip()

if __name__ == '__main__':
    main()