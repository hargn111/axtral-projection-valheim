# Axtral Projection

A spectral axe sweeps through nearby trees, delivering native chop damage while preserving falling logs and resource drops. Inspired by RuneScape: Dragonwilds; no RuneScape assets are included.

## Cast

Hold **LeftShift + G** to aim; release to cast. Right mouse or Escape cancels. Rebind `Controls.CastKey` with a configuration manager. `Controls.GamepadButton` supplies a separate gamepad binding; default `None` avoids vanilla conflicts.

**Default requirements: wood cutting level 15, an unbroken woodcutting axe, and the Elder's power selected as your Forsaken Power.** The highest-tier eligible inventory axe is automatically equipped. It must stay equipped through release. The chosen axe must remain in inventory during flight.

The preview is a trapezoid: 3 meters wide at your feet, widening at a 30-degree angle over 20 meters. Up to 15 targets are selected, nearest forward first. Stumps are off by default. Logs, creatures, buildings and dropped items are never selected.

Each non-stump target hit costs **1% of that axe's maximum durability**. Charges happen as the wave reaches targets, not upfront. The breaking hit lands; remaining hits stop. No skill XP is granted by default.

## Axtral Exhaustion

A successful nonempty cast applies **Axtral Exhaustion** for 180 seconds, visible in the normal status-effect area with an icon and timer. Casting is blocked until it expires. There is no custom cooldown display or saved relog timer. The effect follows normal Valheim lifecycle behavior; relog persistence is not promised. Death clears it. Failed or cancelled aim and empty volumes apply no effect and spend no durability.

## Install

Install BepInExPack_Valheim and Jotunn **2.30.2** separately. Extract this ZIP into the game or mod-profile directory, preserving `BepInEx/plugins/AxtralProjection/`. Both project DLLs are required. Compile baseline is Valheim **1.0.2**; this candidate still needs in-game validation.

Launch once to generate `BepInEx/config/haragon.AxtralProjectionValheim.cfg`. The GUID rename creates a fresh config; previous config files are not migrated. Rune items/recipes and resource requirements are no longer registered.

## Configuration

Spell values have visible bounds and are clamped when read. Configuration-manager changes apply on the next cast; an in-flight cast keeps its snapshot. Rebinding while aiming cancels safely. For manual config-file edits, reload via your config tool or restart the game.

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

On vanilla servers the spell runs client-side. If the server also installs the mod and Jotunn, gameplay settings sync from it. Without server installation, limits are self-imposed: this is trusted-client co-op, not anti-cheat. Preview, pose and spectral axe are local visuals; native tree destruction replicates.

Test first in a disposable world: falling trees retain native physics. Supply sanitized BepInEx logs when reporting errors.
