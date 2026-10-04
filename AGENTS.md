# Project rules

Keep this mod focused on Axtral Projection. No unrelated game changes or broad destruction helpers.

- Read `docs/DESIGN.md` for spell semantics and documented limitations.
- Use pinned compile references; never ship game DLLs, BepInEx, Harmony or Jotunn with the mod.
- Test pure spell logic without Unity. Build the actual plugin as well.
- World damage must go through native `IDestructible.Damage`, not deletion or forced ownership.
- Respect ward access and minimum tool tiers before preview and again before hitting.
- Keep host paths and private agent notes in ignored `OPERATIONS.md` and `AGENT_CHANGELOG.md`.
- Never call a successful build an in-game verification. Update `docs/TESTING.md` with real results only.
