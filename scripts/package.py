#!/usr/bin/env python3
"""Package only this mod's assemblies, never game or dependency DLLs."""
from pathlib import Path
import hashlib
import zipfile

root = Path(__file__).resolve().parents[1]
output = root / "artifacts" / "AxtralProjection-0.1.0.zip"
output.parent.mkdir(exist_ok=True)
build = root / "src/AxtralProjection/bin/Release/net48"
with zipfile.ZipFile(output, "w", zipfile.ZIP_DEFLATED) as package:
    for name in ("AxtralProjection.dll", "AxtralProjection.Core.dll"):
        source = build / name
        if not source.is_file():
            raise SystemExit(f"Build Release first; missing {source}")
        package.write(source, f"BepInEx/plugins/AxtralProjection/{name}")
    package.write(root / "README.md", "README.md")
    for name in ("TESTING.md", "DESIGN.md", "SOURCES.md"):
        package.write(root / "docs" / name, "docs/" + name)
print(output)
print("SHA256:", hashlib.sha256(output.read_bytes()).hexdigest())
