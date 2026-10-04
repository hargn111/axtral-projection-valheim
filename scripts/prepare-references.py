#!/usr/bin/env python3
"""Fetch compile-only BepInEx 5.4.21 references. Never deploy these to the game."""
from pathlib import Path
import hashlib
import io
import urllib.request
import zipfile

root = Path(__file__).resolve().parents[1] / ".deps"
root.mkdir(exist_ok=True)
url = "https://github.com/BepInEx/BepInEx/releases/download/v5.4.21/BepInEx_unix_5.4.21.0.zip"
with urllib.request.urlopen(url, timeout=90) as response:
    data = response.read()
expected = "8e5cec2cda757e5ebffc59d8833379776510c364111b8f5ec0f7c3ba6a6953cf"
actual = hashlib.sha256(data).hexdigest()
if actual != expected:
    raise SystemExit("BepInEx archive checksum mismatch; refusing references")
print("Verified BepInEx archive SHA256:", actual)
with zipfile.ZipFile(io.BytesIO(data)) as archive:
    for name in ("BepInEx.dll", "0Harmony.dll"):
        member = next(n for n in archive.namelist() if n.endswith("/core/" + name))
        (root / name).write_bytes(archive.read(member))
        print("Prepared", name)
