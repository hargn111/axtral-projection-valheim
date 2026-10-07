"""Version/package metadata checks. csproj Version is authoritative."""
from pathlib import Path
import json
import re
import struct
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[1]


def project_version(root=ROOT):
    value = ET.parse(root / "src/AxtralProjection/AxtralProjection.csproj").findtext(".//Version")
    if not value or not re.fullmatch(r"[0-9]+\.[0-9]+\.[0-9]+", value):
        raise ValueError("csproj Version must be X.Y.Z")
    return value


def notes(root=ROOT):
    version = project_version(root)
    text = (root / "CHANGELOG.md").read_text()
    match = re.search(r"^## \[" + re.escape(version) + r"\][^\n]*\n(.*?)(?=^## |\Z)", text, re.M | re.S)
    if not match or not match.group(1).strip():
        raise ValueError("Missing nonempty changelog section for " + version)
    return match.group(1).strip() + "\n"


def validate(root=ROOT):
    version = project_version(root)
    plugin = (root / "src/AxtralProjection/Plugin.cs").read_text()
    match = re.search(r'public const string Version = "([^"\n]+)";', plugin)
    manifest = json.loads((root / "package/manifest.json").read_text())
    top = re.search(r"^## \[([^\]]+)\]", (root / "CHANGELOG.md").read_text(), re.M)
    values = {"csproj": version, "plugin": match.group(1) if match else None,
              "manifest": manifest.get("version_number"), "changelog": top.group(1) if top else None}
    if any(v != version for v in values.values()):
        raise ValueError("Version mismatch: " + repr(values))
    if '[BepInPlugin(Guid, "Axtral Projection", Version)]' not in plugin:
        raise ValueError("BepInPlugin must use the version constant")
    if set(manifest) != {"name", "version_number", "website_url", "description", "dependencies"}:
        raise ValueError("Unexpected/missing manifest fields")
    if not re.fullmatch(r"[A-Za-z0-9_]+", manifest["name"]):
        raise ValueError("Invalid Thunderstore package name")
    if not isinstance(manifest["description"], str) or not 0 < len(manifest["description"]) <= 250:
        raise ValueError("Description must contain 1–250 characters")
    if not isinstance(manifest["website_url"], str) or not manifest["website_url"].startswith("https://"):
        raise ValueError("Expected HTTPS website_url")
    if not isinstance(manifest["dependencies"], list) or not manifest["dependencies"] or any(
            not isinstance(d, str) or not re.fullmatch(r"[A-Za-z0-9_]+-[A-Za-z0-9_]+-[0-9]+\.[0-9]+\.[0-9]+", d)
            for d in manifest["dependencies"]):
        raise ValueError("Invalid dependencies")
    icon = (root / "package/icon.png").read_bytes()
    if len(icon) < 24 or icon[:8] != b"\x89PNG\r\n\x1a\n" or icon[12:16] != b"IHDR" or struct.unpack(">II", icon[16:24]) != (256, 256):
        raise ValueError("Package icon must be a 256x256 PNG")
    notes(root)
    return version
