#!/usr/bin/env python3
"""Explicit local-first release. --dry-run rebuilds but never tags/pushes/publishes."""
import argparse
import json
from pathlib import Path
import subprocess
from package import build
from version import ROOT, notes, validate


def run(args, root=ROOT):
    print("+", " ".join(map(str, args)), flush=True)
    subprocess.run(list(map(str, args)), cwd=root, check=True)


def git(*args, root=ROOT):
    return subprocess.run(["git", *args], cwd=root, check=True, text=True, capture_output=True).stdout.strip()


def preflight(root, version, dry_run=False):
    tag = "v" + version
    if git("status", "--porcelain", root=root):
        raise ValueError("Refusing a dirty tree; commit source changes first")
    if git("tag", "--list", tag, root=root):
        raise ValueError("Local tag already exists: " + tag)
    if git("ls-remote", "--tags", "origin", "refs/tags/" + tag, root=root):
        raise ValueError("Remote tag already exists: " + tag)
    if not dry_run:
        if git("branch", "--show-current", root=root) != "main":
            raise ValueError("Publish only from main; --dry-run permits a feature branch")
        head = git("rev-parse", "HEAD", root=root)
        remote = git("ls-remote", "--heads", "origin", "refs/heads/main", root=root).split()
        if not remote or remote[0] != head:
            raise ValueError("Local main must match remote main exactly")


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--dry-run", action="store_true")
    args = parser.parse_args(argv)
    version = validate()
    preflight(ROOT, version, args.dry_run)
    head = git("rev-parse", "HEAD")
    package = build()  # Never accept or reuse a pre-existing binary/ZIP.
    notes_file = ROOT / "artifacts" / f"release-notes-{version}.md"
    notes_file.write_text(notes())
    tag = "v" + version
    if args.dry_run:
        print(f"DRY RUN: rebuilt {package}; would tag {head} as {tag}, push that tag, and publish with:")
        print(f"gh release create {tag} {package} --verify-tag --title {tag} --notes-file {notes_file}")
        print("No tag, push or release was created.")
        return
    preflight(ROOT, version)  # Recheck immediately before external writes.
    if git("rev-parse", "HEAD") != head:
        raise ValueError("HEAD changed during build; refusing publication")
    run(["gh", "auth", "status"])
    run(["git", "tag", "-a", tag, head, "-m", "Axtral Projection " + version])
    run(["git", "push", "origin", "refs/tags/" + tag])
    run(["gh", "release", "create", tag, str(package), "--verify-tag", "--draft=false", "--prerelease=false", "--title", tag, "--notes-file", str(notes_file)])
    # Read back the exact release and tag before reporting success.
    result = subprocess.run(["gh", "release", "view", tag, "--json", "tagName,assets,body,isDraft,isPrerelease,url"], cwd=ROOT, check=True, text=True, capture_output=True)
    release = json.loads(result.stdout)
    assets = release["assets"]
    if (release["tagName"] != tag or release["isDraft"] or release["isPrerelease"]
            or release["body"].strip() != notes().strip()
            or len(assets) != 1 or assets[0]["name"] != package.name
            or assets[0]["size"] != package.stat().st_size):
        raise ValueError("Release readback differs from intended package/notes")
    print("Verified release:", release["url"])
    remote = git("ls-remote", "--tags", "origin", "refs/tags/" + tag + "^{}").split()
    if not remote or remote[0] != head:
        raise ValueError("Published tag does not resolve to tested HEAD")


if __name__ == "__main__":
    try:
        main()
    except (ValueError, OSError, subprocess.CalledProcessError) as error:
        raise SystemExit(str(error))
