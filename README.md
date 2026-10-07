# Axtral Projection for Valheim

**v0.2.0 gameplay candidate — statically verified, not yet playtested.** A focused BepInEx mod inspired by RuneScape: Dragonwilds. No RuneScape assets are included.

Hold **LeftShift + G** to aim and release to cast; right mouse/Escape cancels. The highest-tier unbroken inventory axe is automatically equipped and must remain equipped at release. Defaults require **wood cutting level 15** and **the Elder's power selected as your Forsaken Power** (`Slotted`, not necessarily active).

Targeting uses a radius-aware horizontal trapezoid, not a pointed cone. A 3-meter starting width catches trees directly ahead; the volume widens at 30 degrees over 20 meters and tolerates base elevations within ±10 meters. Up to 15 targets are selected nearest-forward first. Native chop damage preserves drops, falling logs, wards and tool-tier restrictions.

Charges are progressive: 1% max axe durability per non-stump hit; the breaking hit lands and stops the remaining flight. Skill XP defaults to zero. Successful nonempty casts apply **Axtral Exhaustion** for 180 seconds in the normal status-effect area. No custom cooldown UI or relog persistence layer remains; Exhaustion follows normal Valheim status-effect lifecycle.

## Install and configure

See the [player guide](package/README.md). Install BepInExPack_Valheim and Jotunn 2.30.2 separately, then extract the ZIP preserving both project DLLs. Config: `BepInEx/config/haragon.AxtralProjectionValheim.cfg`; no migration from the previous GUID.

`Controls.CastKey` is a rebindable shortcut. `Controls.GamepadButton` is an independent optional controller binding, default None. Gameplay options remain in `Spell`:

| Spell setting | Default | Bounds / meaning |
|---|---|---|
| Range | 20 | 10–50 meters |
| ConeAngle | 30 | 20–45 degrees, full widening angle |
| StartWidth | 3 | 0–10 meters, full width at origin |
| MaxTrees | 15 | 2–50 targets, nearest forward first |
| Cooldown | 180 | 0–600 seconds; 0 disables Exhaustion |
| WoodCuttingLevel | 15 | 0–100, required wood cutting skill level |
| TravelSpeed | 10 | 1–50 meters/second |
| DurabilityCostPercent | 1 | 0–25% max axe durability per non-stump hit; 0 disables |
| SkillXpPerTree | 0 | 0–1 skill gain per non-stump hit; 0 disables |
| ElderRequirement | Slotted | None / Slotted / Active |
| ClearStumps | false | Include existing stumps; not newly created ones |
| AdditionalPrefabs | empty | Comma-separated exact Destructible prefab names from other mods |
| ExcludedPrefabs | empty | Exact prefab names to never fell; overrides inclusion |

Changes via a configuration manager apply to the next cast. In-flight values are snapshotted; cached input changes cancel aim safely. Manual file edits require the config tool's reload or a game restart. Names are trimmed, exact/case-sensitive, and `(Clone)` is stripped. Exclusion wins; disabled stumps cannot be force-included.

Rune item/recipe, resource config and payment are deprecated to avoid multiplayer custom-item conflicts. Dormant source remains under `Deprecated/Runes`, inactive unless explicitly reintegrated with `ENABLE_RUNES`.

Clients can use the mod on vanilla servers. Settings sync when the server also has the mod and Jotunn. Version checks are not enforced (`VersionCheckOnly`, `None`); this is trusted-client co-op, not server-authoritative anti-cheat. Custom VFX stay local; native destruction replicates.

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

Packaging always rebuilds from source and runs validations. Output: `artifacts/AxtralProjection-0.2.0.zip`. Only the two project DLLs and player-facing files ship; never game/reference or third-party binaries. No CI workflow is added because current game references are local-only.

## Documentation

- [Design and safeguards](docs/DESIGN.md)
- [Testing and pending playtests](docs/TESTING.md)
- [Local-first release instructions](docs/RELEASING.md)
- [Sources](docs/SOURCES.md)
