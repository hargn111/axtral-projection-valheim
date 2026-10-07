#!/usr/bin/env python3
"""Always rebuild and validate source; package only this project's own DLLs."""
from pathlib import Path
import hashlib
import subprocess
import sys
import zipfile
from version import ROOT, validate


def run(args):
    print("+", " ".join(map(str, args)), flush=True)
    subprocess.run(list(map(str, args)), cwd=ROOT, check=True)


def build():
    version = validate()
    run(["dotnet", "build", "AxtralProjection.sln", "-c", "Release", "--no-incremental", "-t:Rebuild"])
    run(["dotnet", "test", "tests/AxtralProjection.Tests", "-c", "Release", "--no-build"])
    run([sys.executable, "scripts/audit-imports"])
    run([sys.executable, "scripts/check-version.py"])
    run([sys.executable, "-m", "unittest", "discover", "-s", "tests/scripts", "-v"])
    output = ROOT / "artifacts" / f"AxtralProjection-{version}.zip"
    output.parent.mkdir(exist_ok=True)
    source = ROOT / "src/AxtralProjection/bin/Release/net48"
    files = {f"BepInEx/plugins/Haragon-AxtralProjection/{name}": source / name
             for name in ("AxtralProjection.dll", "AxtralProjection.Core.dll")}
    files.update({name: ROOT / "package" / name for name in ("manifest.json", "icon.png", "README.md")})
    files.update({name: ROOT / name for name in ("LICENSE", "CHANGELOG.md")})
    # Write atomically so a failed build/package cannot leave a partially updated ZIP.
    pending = output.with_suffix(".zip.pending")
    try:
        with zipfile.ZipFile(pending, "w", zipfile.ZIP_DEFLATED) as archive:
            for name, path in files.items(): archive.write(path, name)
        with zipfile.ZipFile(pending) as archive:
            if set(archive.namelist()) != set(files) or archive.testzip() is not None:
                raise ValueError("Package contents failed validation")
        pending.replace(output)
    finally:
        pending.unlink(missing_ok=True)
    print(output)
    print("SHA256:", hashlib.sha256(output.read_bytes()).hexdigest())
    return output


if __name__ == "__main__":
    build()
