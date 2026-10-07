# Verification and playtest checklist

## Static checks

Release build treats warnings as errors. Pure netstandard2.0 logic is exercised by xUnit on .NET 8: strict base-point trapezoid/height, cap/order, prefab parsing and precedence, ward/tool-tier rules, configuration clamping, Elder modes, progressive durability/abort and aim/release gates.

The game/Unity import audit reproduced the old four-argument Character.Message failure against supplied Valheim 1.0.2 DLLs. The rebuilt plugin passes. Package/version/release tests verify metadata consistency and safety guards; package builds fresh source and runs all gates. These results do **not** constitute gameplay verification.

## Pending in-game acceptance (disposable world)

1. Load on Valheim 1.0.2 with BepInExPack_Valheim and Jotunn 2.30.2; inspect logs. Remove old plugin copies. Confirm fresh `haragon.AxtralProjectionValheim.cfg` and no rune registration or recipe.
2. Default G starts and holds aim while already moving or pressing/releasing WASD; only releasing G casts without opening unintended vanilla UI. Test remapping/modifiers (unrelated keys and releasing a modifier must not cast) and independent gamepad binding; the starting device alone owns release. Gamepad defaults disabled. Verify chat, console, inventory, menus, generic text input and minimap pin editing suppress casting.
3. At wood cutting 14, refuse casting; at 15 allow it with other gates satisfied. Highest-tier unbroken axe auto-equips; changed/broken/missing axe cancels appropriately.
4. Elder Slotted requires GP_TheElder as chosen power; Active requires its active effect; None bypasses. Check clear immediate failure messages before aim/pose/highlighting, and no Exhaustion/durability cost on denial. Default Active must require the effect; expiry before release must abort.
5. Preview is a trapezoid matching the yaw-only volume. Test base points just inside/outside each edge and the 15m endpoint, behind-player roots, leaning/large-canopy trees, and slopes. Collider overlap alone must never include an outside root. Outline must follow terrain. StartWidth defaults 1m, bounds 0–3m; angle defaults 30 degrees, minimum 0 gives a rectangle.
6. Verify Birch_Sapling inside/outside bounds, included after MaxTrees is filled, never using a slot, and still honoring exclusion/ward/tier rules. Verify nearest-forward MaxTrees cap, exact inclusion/exclusion precedence, disabled/enabled stumps, modded AdditionalPrefabs and no damage to logs/buildings/creatures/dropped items.
7. Nonempty cast applies native Axtral Exhaustion icon/timer for configured duration. It blocks recast; zero Cooldown disables it. Change settings and verify next cast updates TTL. No custom cooldown display remains. Relog persistence is not required; death clears it.
8. Progressive 2% max durability per non-stump hit: breaking hit lands; farther trees remain untouched. Drop axe mid-flight; stop pending hits. No cost for stumps or zero percent; no XP at zero, optional skill gain when enabled.
9. Cancel aiming, die/teleport/logout during flight and unload the scene: no pending hits, lingering highlights/pose or spectral axe. Attempt another cast after an immediate first-hit break to detect stuck flight state.
10. Native immunity/tool tiers/wards still work, including access revoked between release and impact. Confirm resource drops and falling log behavior.
11. Test on a vanilla server with only the client mod, and with mod/Jotunn on server and two clients for synced gameplay configuration and native owner routing. Other clients need not see custom VFX. These are trusted-client limits, not anti-cheat.
12. Repeat casts and inspect sanitized logs for exceptions or leaked visuals. Record exact game/BepInExPack/Jotunn versions and real outcomes before publishing a release.
