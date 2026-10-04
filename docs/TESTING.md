# Verification and first in-game test

## Verified without Unity

- Actual `net48` BepInEx plugin compiles with warnings treated as errors.
- Pure `netstandard2.0` spell logic is exercised by xUnit tests on .NET 8.
- Geometry tests cover range, height, steering, full versus half-angle and invalid inputs.
- Cast tests cover skill/rune/axe gates, cancellation, changed requirements, empty cones, failed payment, duplicate release and persisted ready-at timestamps.
- Target rules cover ward access, unsupported targets and tool tiers.
- Packaging contains only the two project DLLs and instructions, not third-party/game libraries.

**Not yet tested inside Valheim.** A compile against reference assemblies verifies API signatures, not game behavior. No game client or Unity project was used in this pass.

## Manual acceptance checklist (use a disposable world)

1. Install BepInExPack_Valheim, Jotunn 2.30.2 and this candidate on a compatible game build. Confirm the plugin's load log and no exceptions.
2. Craft ten Axtral Runes at a workbench from 2 Resin + 1 Greydwarf Eye. Confirm stacking, saving/loading, translation, and no changes to vanilla Amber.
3. With Woodcutting 10, casting must fail. At level 11 it must work. Test missing/broken axe and nine versus ten runes. Make sure the axe equipped is the highest eligible tier, not merely the currently held axe.
4. Hold G. Check raised-axe pose, green targets, cone outline and target count. Turn left/right: the preview must track yaw. Place targets just inside/outside ±15° and 30 meters; test slopes/height.
5. Release: ten runes disappear once; blue axe/wave travels outward; eligible trees fall once when reached. Check falling logs and resource drops. Trees above axe tier must remain intact.
6. Try casting again before 45 seconds, at the boundary, and after a save/relog. Confirm cooldown UI and no extra cost on denial.
7. Cancel via right mouse/Escape, open inventory/chat/menu, lose focus, change/remove/break the axe, die or teleport while aiming. No runes or cooldown should be spent; no lingering green highlights or raised pose.
8. Cast at an empty cone; no cost. Check existing stumps, ClearStumps=false, and the fact that new stumps/logs are NOT swept up by the same cast. Ensure buildings, creatures, rocks and dropped items are untouched.
9. Test a ward without access. No preview or damage. Test access being revoked between release and impact.
10. Test death/teleport during the flight: stop effects/hits but keep the already-paid cost and cooldown. Test several casts for leaked renderers/materials and log errors.
11. If using modded cave vines, identify the exact prefab name, configure VinePrefabs and test only that named destructible. Do not put doors/building pieces in the list.
12. Host + second client + dedicated server: everyone installs the same version. Verify owner routing for a tree owned by another peer, synced configuration, rune save/load and version-mismatch rejection. Only the caster sees custom VFX in this version.

Record the exact game, Unity, BepInExPack and Jotunn versions plus observed results before calling this a verified gameplay release. Send BepInEx/LogOutput.log after any failure (remove private identifiers first).
