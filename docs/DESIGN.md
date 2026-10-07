# Axtral Projection design

## Aim, release and flight

1. Configurable keyboard shortcut (default LeftShift+G) or independent optional gamepad binding begins aim. Silent Elder gating is deferred to release. Automatically equip the highest-tier unbroken woodcutting axe, breaking ties by chop damage.
2. At 10 Hz, gather candidates with one bounded physics overlap, deduplicate colliders, filter by volume/ward/tool tier/prefab rules, and show green highlights, trapezoid outline and count. Preserve the procedural held-axe pose.
3. Release rechecks the equipped cast-time axe, skill level, selected/active Elder requirement and native Exhaustion. Empty volumes are free. Successful casts snapshot configuration and apply Exhaustion before visuals or world hits.
4. The unchanged spectral axe moves outward; a forward-distance frontier reaches snapshot targets nearest first. Native `IDestructible.Damage` preserves network ownership, falling logs and drops. There is no forced ownership, deletion or per-frame throttling.
5. Charge percentage durability and optional XP only after a native non-stump hit is dispatched. The breaking hit lands, then flight stops. Skip charges at zero percent and for non-durable axes. Abort pending hits on missing/broken axe, death, teleport, logout/player replacement, disable or scene unload. Cleanup visual and pose/highlight resources.

## Geometry

Heading is normalized and horizontal. For forward distance `f`, half-width is `StartWidth/2 + max(f,0)*tan(ConeAngle/2)`. Include a target if `-r <= f <= Range+r` and absolute lateral distance is no greater than half-width plus trunk radius `r`. Base elevation tolerance is ±10 meters. Preview draws the center-volume trapezoid; tree radius expands individual eligibility.

The physics overlap encloses the far trapezoid corners and height band, plus a bounded margin. Collider horizontal bounds approximate trunk radius, currently limited to 2 meters to avoid canopy-sized selection. A runtime test must confirm this approximation on supported tree prefabs. Deduplicate components; never scan the entire world.

Logs, characters, pieces and item drops are excluded. Existing tree stumps are optional and cannot bypass ClearStumps via AdditionalPrefabs. Exact prefab names are trimmed and have `(Clone)` removed; exclusions override vanilla and additional inclusion. Recheck existence, ward access, geometry and tool tier at impact. New stumps/logs are not added to the snapshot.

## Exhaustion and compatibility

Jotunn `CustomStatusEffect` registers `SE_AxtralExhaustion` in ObjectDB, including its copied DB lifecycle. Immediately before applying, update the registered asset's TTL from the cast's Cooldown. Zero applies no effect. Use native icon/timer presentation, not a custom cooldown box. Aiming guidance remains.

The embedded 64×64 icon is isolated in one factory and decoded through Jotunn's AssetUtils compatibility bridge. There is **no custom relog persistence**; normal status-effect lifecycle applies, and death clears Exhaustion. Player.Save/Load in the supplied assembly does not serialize arbitrary effects; registration is for native lookup/cloning, not a promise of relog persistence.

GameCompat.Message discovers the current Character.Message signature and supplies trailing defaults. The import audit resolves all emitted game/Unity method and field references against local matching DLLs. It caught the prior four-argument Character.Message import. This mod does not patch CraftingStation.Interact; investigate that error among other installed mods.

## Config, multiplayer and deprecated code

See README for bounds/defaults. Clamp numeric reads; no gameplay config cache is retained. Input bindings are Jotunn config-backed and use the registered GUID-suffixed names. Separate controller binding avoids requiring keyboard modifiers on a gamepad. Chat, console, inventory, menus, text entry, minimap pin input, lost focus and unsafe player states suppress aiming/casting.

Jotunn `VersionCheckOnly` is the non-obsolete replacement for OnlySyncWhenInstalled. VersionStrictness.None permits vanilla-server clients; gameplay settings sync when both sides install the mod. This is trusted-client co-op; no authoritative enforcement is claimed. VFX remain local.

Rune registration and resource-controller source are preserved under Deprecated/Runes, off by default. Re-enabling requires explicit reintegration, resource gates/config and a multiplayer compatibility policy. No active item, recipe, payment or RuneCost setting remains.

Compile baseline is supplied Valheim 1.0.2 game/Unity DLLs, Jotunn 2.30.2 and BepInEx 5.4.21 API. DLLs are ignored and never shipped. Static checks cannot prove runtime input ordering, tree geometry, status-effect behavior, rig/shader appearance or peer ownership.

## Out of scope

Real equipped-axe projectile (TODO marker only), per-frame throttling, tier-scaled geometry, Elder cooldown reductions, new CI workflows, replicated custom VFX and server-authoritative anti-cheat.
