# Axtral Projection: first playable candidate

The supplied Dragonwilds spell description is the gameplay specification. This is an independent Valheim adaptation, not a port of RuneScape assets.

## Defaults and cast flow

1. Hold **G** to begin aiming, requiring Woodcutting **11**, **10 Axtral Runes**, and an unbroken inventory axe with chop damage.
2. Equip the highest tool-tier eligible axe; chop damage breaks ties. Normal attacks are blocked only for the local player while aiming.
3. Aim with normal look controls. The horizontal cone is **30 degrees total** (±15 degrees) and its target roots must lie inside a **30-meter 3D range sphere**. Height counts toward distance, but the cone yaw ignores camera pitch.
4. Update green renderer tints, ground rings, cone outline and target count at 10 Hz. A local procedural arm pose raises the equipped axe where the Animator exposes humanoid bones.
5. Release G to snapshot the origin, heading, targets and travel settings. Recheck requirements and charge the rune cost exactly once. An empty cone is free.
6. Set a **45-second cooldown** before starting effects or touching world state. Store the UTC ready-at timestamp in the character's custom data; character saves preserve it across relogs. Real time continues while offline or paused.
7. Launch a procedural blue axe and expanding cone wave at **30 m/s**. Hit snapshot targets once as the distance frontier reaches them. Native chop damage is intentionally overwhelming, but native immunity and tool-tier checks remain in force.

Right mouse, Escape, menus, chat, loss of focus, changed/removed/broken axe, placement mode, swimming, dodging, attacking, teleporting or death cancel pre-casting without cost. Death, teleporting or a player change stop an already-paid flight; that cost is not refunded. The best axe stays equipped after cancellation/casting.

## Rune adaptation

Valheim has no Dragonwilds rune inventory. Register `AxtralRune` by cloning Amber with Jotunn; it currently retains Amber's visual/icon. At a level-one workbench, **2 Resin + 1 Greydwarf Eye produce 10 runes**. Stack limit 100, weight 0.1. This recipe is a first-pass Valheim balancing choice, not a claimed Dragonwilds recipe.

## Supported targets and safeguards

- Standing trees: `TreeBase`.
- Existing stumps: `Destructible`, tree type, prefab name ending in `Stub`, case-insensitive. Configurable on/off. A cave/mod stump using another naming scheme is not assumed safe.
- Optional vines: exact `Destructible` prefab names in `VinePrefabs`, empty by default. No vanilla cave-vine mapping has been verified. Explicit configuration is required for another mod's assets.
- Never directly select logs, characters, dropped items or building pieces.
- Deduplicate target components across colliders.
- Require a valid network view, ward access and sufficient axe tool tier.
- Recheck existence, ward access, cone/range and tier at impact.
- Use native `Damage(HitData)` so native ownership, falling trunks, drops and effects remain intact. Do not delete objects or claim their network ownership.

Newly created stumps and fallen logs are deliberately excluded from the snapshot; collect/chop logs normally and use a later cast for remaining stumps. The spell has no terrain/wall line-of-sight test, matching the supplied cone description. Falling trunks retain native physics and can still hurt players.

## Configuration and multiplayer

BepInEx writes `mod.axtralprojection.valheim.cfg`. `CastKey` is local. Spell settings are Jotunn admin-only synchronized values: Range 1–100, ConeAngle 1–180, Cooldown 0–600, WoodcuttingLevel 0–100, RuneCost 0–100, TravelSpeed 1–100, ClearStumps and VinePrefabs.

All peers and dedicated servers must install the same mod version and Jotunn because a custom inventory item is added. Dedicated servers register the item/config without running player input. Gameplay hits use native network routing. Aim highlights, procedural pose and flying axe are **local to the caster**; other players receive native tree destruction, not the custom visual effects.

This is a **trusted-client co-op** implementation, not anti-cheat. Synchronized config does not make client skill, inventory, cooldown or damage claims server-authoritative. A malicious client can bypass these checks. Do not advertise hardened server enforcement.

## What requires runtime access

No Unity editor is needed for this implementation. Valheim client access is needed to verify load order, rune/recipe registration, renderer shaders, bone axes, input order, actual tree damage and multiplayer ownership. Unity would be useful later for a polished held-axe animation, custom rune art and networked VFX, but is not a prerequisite for the current build.

Compile baseline is explicitly pinned to ValheimGameLibs **0.221.4**, UnityEngine.Modules **2021.3.33**, Jotunn **2.30.2** and BepInEx API **5.4.21**. This is not a claim of compatibility with an untested later game version. Reference packages are compile-only and are never deployed with the mod.
