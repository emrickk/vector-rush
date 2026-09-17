#!/usr/bin/env python3
"""Verify a portable source checkpoint. --record is publication tooling only."""
import hashlib
import json
from pathlib import Path
import subprocess
import sys

root = Path(__file__).resolve().parents[1]
manifest = root / "current-game-manifest.json"

def digest(path):
    value = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            value.update(block)
    return value.hexdigest()

if "--record" in sys.argv:
    paths = subprocess.check_output(["git", "ls-files", "-z"], cwd=root).decode().split("\0")
    records = []
    for name in sorted(filter(None, paths)):
        if name == manifest.name:
            continue
        path = root / name
        if path.is_symlink():
            raise SystemExit("External/symlink dependency requires review: " + name)
        if path.stat().st_size >= 100 * 1024 * 1024:
            raise SystemExit("Oversized GitHub blob: " + name)
        records.append({"path": name, "bytes": path.stat().st_size, "sha256": digest(path)})
    manifest.write_text(json.dumps({"format": 1, "scope": "Current Vector Rush source checkpoint; excludes this self-referential manifest", "files": records}, indent=2) + "\n")
    print("Recorded", len(records), "payload files")
else:
    data = json.loads(manifest.read_text())
    failures = []
    for item in data["files"]:
        path = root / item["path"]
        if not path.is_file() or path.is_symlink() or path.stat().st_size != item["bytes"] or digest(path) != item["sha256"]:
            failures.append(item["path"])
    if failures:
        raise SystemExit("Missing/changed files:\n" + "\n".join(failures))
    print("PASS:", len(data["files"]), "payload files match the publication manifest.")
