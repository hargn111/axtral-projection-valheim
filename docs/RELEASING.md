# Releasing Axtral Projection

A GitHub release consists of a **git tag** (for example `v0.2.0`) identifying the source revision, **attached files** (the player ZIP), and **release notes** explaining the changes. Building a ZIP does not publish a release.

## Make a test artifact first

1. Supply matching Valheim 1.0.2 `valheim_Data/Managed/` DLLs in ignored `references/`. See README for required files. Never commit or distribute those DLLs.
2. Install .NET 8 SDK, Python 3, and prepare the existing compile dependencies with `python3 scripts/prepare-references.py`.
3. Run `python3 scripts/package.py`.

The script **always rebuilds Release from source**, runs the core tests, import audit, version consistency check and release-script tests, then writes `artifacts/AxtralProjection-<version>.zip`. It never packages a leftover binary. Install that ZIP and follow docs/TESTING.md before publishing.

## Set a version

The plugin csproj `<Version>` is the authoritative version. Update it, the plugin `Version` constant, `package/manifest.json` version_number and the top `CHANGELOG.md` entry together. `python3 scripts/check-version.py` refuses disagreement. Commit the version/source changes before a release.

## Preview without publishing

```sh
python3 scripts/release.py --dry-run
```

This requires a clean tree and unused local/remote tag, **rebuilds the package**, extracts the matching changelog section into an ignored notes file, and prints the tag/publication commands. It does not create tags, push anything, or publish a GitHub release. Feature branches are permitted for this rehearsal.

## Publish only after playtesting

Switch to main and ensure local main matches remote main exactly. Authenticate GitHub CLI (`gh auth login`), then deliberately run:

```sh
python3 scripts/release.py
```

The script refuses a dirty tree, existing local/remote tag, a non-main branch or remote-head mismatch. It rebuilds again and rechecks source state. It creates an annotated `vX.Y.Z` tag at the tested commit, pushes only that tag and runs `gh release create` with the **exact new ZIP**, `--verify-tag`, title and changelog notes. It reads the release and remote tag back afterward.

If publication fails after tagging/pushing, stop and inspect `gh release view vX.Y.Z` and `git ls-remote --tags origin`. The script intentionally will not overwrite an existing tag or silently delete external state. Investigate and recover deliberately; do not blindly rerun with a new version.

## Package format

The supplied sample uses Thunderstore `manifest.json`, not a separate `metadata.json` format. The ZIP preserves that schema: ASCII letters/digits/underscores in name, X.Y.Z version_number, website_url, description of at most 250 characters, and dependency identifiers. Jotunn is listed explicitly alongside BepInExPack_Valheim.

At ZIP root: `manifest.json`, 256×256 `icon.png`, `LICENSE`, `CHANGELOG.md`, and the **player-facing** `package/README.md` renamed to README.md. Only AxtralProjection.dll and AxtralProjection.Core.dll are placed under `BepInEx/plugins/Haragon-AxtralProjection/`. No game, Jotunn, Harmony or BepInEx DLLs are shipped. A GitHub release does not automatically upload to Thunderstore; that is a separate, explicit action.

## Why no GitHub Actions workflow

The current baseline uses locally supplied game DLLs, not publicly resolvable pinned game packages. They cannot be committed or redistributed with the mod. Local validation is therefore the build gate; no CI workflow/stub is introduced. A future CI arrangement would require an approved private reference supply mechanism.

Static validation cannot replace gameplay testing. Publishing is a separate decision from building or merging this candidate.
