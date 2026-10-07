# Changelog

All notable changes are documented here using Keep a Changelog-style sections.

## [0.2.1] - 2026-10-07

### Fixed
- Starting/holding aim now ignores unrelated movement keys. Configured modifiers gate starting only; release follows the initiating main key or controller button.
- Unmet Slotted or Active Elder requirements warn before aiming/highlighting; release still rechecks them.
- Target eligibility uses the ground-level tree-base point, without collider-radius padding. Preview boundary follows terrain while retaining matching X/Z geometry.

### Changed
- Default keybind is G; default Elder requirement is Active.
- Default range is 15m, starting width 1m (bounds 0–3m), and durability cost 2%.
- ConeAngle permits 0 degrees for a straight rectangle; default remains 30 degrees.

### Added
- Birch_Sapling is eligible without consuming MaxTrees slots; exclusion, ward, tier, geometry and normal non-stump durability/XP rules still apply.
- Regression coverage for strict base-point bounds, input-device release ownership, Elder pre-aim denial and uncapped saplings.

## [0.2.0] - 2026-10-07

### Added
- Radius-aware trapezoid targeting and preview, starting width, nearest-forward target cap and exclusion list.
- Elder Forsaken Power requirement (Slotted by default).
- Progressive percentage-based axe durability and opt-in wood cutting XP.
- Rebindable modifier shortcut and independent optional gamepad control.
- Native Axtral Exhaustion status effect with embedded icon and timer.
- Game/Unity import audit, version consistency checks and rebuild-only local packaging/release scripts.

### Changed
- Plugin GUID is now `haragon.AxtralProjectionValheim`; a fresh config is generated without migration.
- Defaults: range 20m, travel speed 10m/s, cooldown 180s, wood cutting level 15, stumps disabled.
- Config keys are now WoodCuttingLevel and AdditionalPrefabs, with bounded numeric reads.
- Vanilla-server clients are permitted; config sync is used when the server also installs the mod.

### Deprecated
- Rune item, recipe, resource configuration and payment; dormant source remains isolated.

### Removed
- Custom cooldown UI and saved ready-at timestamp; Exhaustion follows normal Valheim lifecycle, with no custom relog persistence.

### Fixed
- Stale Character.Message import by compiling against supplied Valheim 1.0.2 DLLs and adding a signature-compatible HUD helper.
- Cleanup for cancelled/aborted flights and scene unload; use Jotunn's registered input names.

## [0.1.0] - 2026-10-04

### Added
- Initial compiled gameplay candidate with cone targeting, rune resource gates and spectral axe effects.
