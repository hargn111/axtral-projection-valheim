# Axtral Projection for Valheim

**v0.2.1 gameplay candidate — statically verified, not yet playtested.** A focused BepInEx mod inspired by RuneScape: Dragonwilds. No RuneScape assets are included.

Hold **G** to aim and release **G** to cast; moving does not release the spell. Unmet Elder requirements warn before aiming/highlighting. Right mouse/Escape cancels. The highest-tier unbroken inventory axe is automatically equipped and must remain equipped at release. Defaults require **wood cutting level 15** and **the Elder's power active** (`Active`).

Targeting uses the tree base at ground level, not canopy/collider bounds. Its X/Z base point must fall inside the terrain-following trapezoid: 1 meter wide at the start, widening at 30 degrees over 15 meters, with base elevations within ±10 meters. Up to 15 counted targets are selected nearest-forward first. `Birch_Sapling` is included without consuming target slots; exclusions still apply. An angle of 0 makes a straight rectangle. Native chop damage preserves drops, falling logs, wards and tool-tier restrictions.

Charges are progressive: 2% max axe durability per non-stump hit; the breaking hit lands and stops the remaining flight. Skill XP defaults to zero. Successful nonempty casts apply **Axtral Exhaustion** for 180 seconds in the normal status-effect area. No custom cooldown UI or relog persistence layer remains; Exhaustion follows normal Valheim status-effect lifecycle.

## Install and configure

See the [player guide](package/README.md). Install BepInExPack_Valheim and Jotunn 2.30.2 separately, then extract the ZIP preserving both project DLLs. Config: `BepInEx/config/haragon.AxtralProjectionValheim.cfg`; no migration from the previous GUID.

`Controls.CastKey` is a rebindable shortcut. `Controls.GamepadButton` is an independent optional controller binding, default None. Gameplay options remain in `Spell`:

| Spell setting | Default | Bounds / meaning |
|---|---|---|
| Range | 15 | 10–50 meters |
| ConeAngle | 30 | 0–45 degrees, full widening angle |
| StartWidth | 1 | 0–3 meters, full width at origin |
| MaxTrees | 15 | 2–50 counted targets, nearest forward first; Birch_Sapling is uncapped |
| Cooldown | 180 | 0–600 seconds; 0 disables Exhaustion |
| WoodCuttingLevel | 15 | 0–100, required wood cutting skill level |
| TravelSpeed | 10 | 1–50 meters/second |
| DurabilityCostPercent | 2 | 0–25% max axe durability per non-stump hit; 0 disables |
| SkillXpPerTree | 0 | 0–1 skill gain per non-stump hit; 0 disables |
| ElderRequirement | Active | None / Slotted / Active |
| ClearStumps | false | Include existing stumps; not newly created ones |
| AdditionalPrefabs | empty | Comma-separated exact Destructible prefab names from other mods |
| ExcludedPrefabs | empty | Exact prefab names to never fell; overrides inclusion |

Changes via a configuration manager apply to the next cast. In-flight values are snapshotted; cached input changes cancel aim safely. Manual file edits require the config tool's reload or a game restart. Names are trimmed, exact/case-sensitive, and `(Clone)` is stripped. Exclusion wins; disabled stumps cannot be force-included.

Rune item/recipe, resource config and payment are deprecated to avoid multiplayer custom-item conflicts. Dormant source remains under `Deprecated/Runes`, inactive unless explicitly reintegrated with `ENABLE_RUNES`.

Clients can use the mod on vanilla servers. Settings sync when the server also has the mod and Jotunn. Version checks are not enforced (`VersionCheckOnly`, `None`); this is trusted-client co-op, not server-authoritative anti-cheat. Custom VFX stay local; native destruction replicates.

## Upgrading from 0.2.0

Existing config values are preserved, not overwritten by new defaults. Reset these entries with your configuration manager (or edit the config while the game is closed): `Controls.CastKey = G`, `Spell.ElderRequirement = Active`, `Spell.StartWidth = 1`, `Spell.DurabilityCostPercent = 2`, `Spell.Range = 15`. `Spell.ConeAngle` stays 30; its new allowed minimum is 0. StartWidth is now clamped to 0–3 meters. Saplings still receive normal non-stump durability/XP handling; only their target-cap accounting is exempt.

## Build and test

Requires .NET 8 SDK and Python 3. Reuse the existing compile-reference preparation:

```sh
python3 scripts/prepare-references.py
# Copy DLLs from the Valheim 1.0.2 valheim_Data/Managed folder to ignored references/.
dotnet build AxtralProjection.sln -c Release
dotnet test tests/AxtralProjection.Tests -c Release
python3 scripts/audit-imports
python3 scripts/check-version.py
python3 -m unittest discover -s tests/scripts -v
python3 scripts/package.py
```

Baseline: supplied Valheim **1.0.2** game and Unity DLLs, Jotunn **2.30.2**, BepInEx **5.4.21** compile API. Required DLLs include assembly_valheim, assembly_utils, Assembly-CSharp and UnityEngine/CoreModule/AnimationModule/PhysicsModule/InputLegacyModule/IMGUIModule/ImageConversionModule. Supplying the complete matching Managed set avoids transitive-reference gaps. Override `GameReferences` for builds if necessary; packaging and audit use `references/`.

The import audit resolves game/Unity method and field references and fails on unresolved imports. HUD messages use a reflected signature-compatible wrapper. Icon decoding uses Jotunn's net48-compatible `AssetUtils.LoadImage` bridge; reflection-based runtime behavior still needs playtesting.

Packaging always rebuilds from source and runs validations. Output: `artifacts/AxtralProjection-0.2.1.zip`. Only the two project DLLs and player-facing files ship; never game/reference or third-party binaries. No CI workflow is added because current game references are local-only.

## Documentation

- [Design and safeguards](docs/DESIGN.md)
- [Testing and pending playtests](docs/TESTING.md)
- [Local-first release instructions](docs/RELEASING.md)
- [Sources](docs/SOURCES.md)
