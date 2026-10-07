# Axtral Projection for Valheim

A BepInEx mod inspired by RuneScape: Dragonwilds' **Axtral Projection** spell.

**Status: v0.1.0 compiled gameplay candidate. Automated spell-logic tests pass; in-game validation is still required.** No RuneScape assets are included.

## Play

- Hold **G** to raise your axe and aim a green target preview; release to cast.
- Right mouse or Escape cancels without spending resources.
- Defaults: **30 m range**, **30° full-width cone**, **45 s cooldown**, **Woodcutting level 11**, **10 Axtral Runes**.
- Your highest-tier unbroken woodcutting axe in inventory controls what trees can be felled; it is automatically equipped.
- Craft **10 Axtral Runes** at a workbench using **2 Resin + 1 Greydwarf Eye**. The first version reuses Amber's model/icon.
- The astral axe and wave move outward and deliver native chop damage to eligible standing trees and existing stumps. Logs and new stumps are left for later.

## Install the candidate

1. Install the Valheim-specific **BepInExPack_Valheim** (not an arbitrary BepInEx 6 build).
2. Install **Jotunn 2.30.2**.
3. Extract the candidate ZIP into the game/mod-profile directory, preserving `BepInEx/plugins/AxtralProjection/`.
4. Confirm BOTH `AxtralProjection.dll` and `AxtralProjection.Core.dll` are installed.
5. Launch once to generate `BepInEx/config/haragon.AxtralProjectionValheim.cfg`.

For multiplayer, all clients **and the server** need the same mod and Jotunn. Spell settings sync from the server; casting is trusted-client rather than server-authoritative. The flying axe, hold pose and targeting preview are local visuals. Native tree destruction replicates.

Use a disposable world for the first test: falling trees retain native physics. Read [the acceptance checklist](docs/TESTING.md) before testing against a world you care about.

## Configuration

`Controls.CastKey` defaults to G. `Spell` contains Range, ConeAngle (full width), Cooldown, WoodcuttingLevel, RuneCost, TravelSpeed, ClearStumps and VinePrefabs. Gameplay settings are admin-only and synchronized through Jotunn. Edit the config while the game is stopped, or use a compatible configuration-manager UI.

`VinePrefabs` is empty by default: no vanilla cave-vine mapping has been verified. Supply exact modded destructible prefab names if needed. The mod never broadly destroys arbitrary objects.

## Build and test

Requirements: **.NET 8 SDK**, **Python 3**, network access for pinned compile dependencies. A Unity editor or installed game is not required for this reference build.

```sh
python3 scripts/prepare-references.py
dotnet build AxtralProjection.sln -c Release
dotnet test tests/AxtralProjection.Tests -c Release
python3 scripts/package.py
```

Artifact: `artifacts/AxtralProjection-0.1.0.zip`. Only this project's two DLLs and instructions are packaged. Never copy game/reference DLLs, Jotunn, BepInEx or Harmony from build output into a game installation.

Pinned compile baseline: ValheimGameLibs 0.221.4, UnityEngine.Modules 2021.3.33, Jotunn 2.30.2, BepInEx 5.4.21 API. Later game builds are not verified by this compile. The BepInEx download script pins the archive checksum. No CI workflow is configured in this repository; local verification is the current build gate.

## Documentation

- [Spell semantics, safeguards and limitations](docs/DESIGN.md)
- [In-game testing checklist](docs/TESTING.md)
- [Documentation sources used](docs/SOURCES.md)

No Unity/MCP setup is currently required to build. The next required input is a **Valheim test client** and its game/BepInEx/Jotunn versions, for gameplay, shader and pose verification. Unity authoring can follow later if bespoke animation or art is desired.
