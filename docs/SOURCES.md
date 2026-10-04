# Documentation sources

The user's detailed Dragonwilds spell description is the authoritative feature specification for this adaptation. Numerical defaults are taken from that description; the rune recipe and controls are Valheim-specific choices.

## BepInEx 5.4.21

- https://docs.bepinex.dev/v5.4.21/
- https://docs.bepinex.dev/v5.4.21/api/index.html
- https://github.com/BepInEx/bepinex-docs/tree/v5.4.21/articles/dev_guide/plugin_tutorial
- https://github.com/BepInEx/BepInEx/tree/v5.4.21

Read the version-matched plugin structure/metadata and configuration tutorials. The documentation site returned HTTP 403 to direct programmatic fetching but loaded in the browser. GitHub's `v5.4.21` docs branch supplies the matching tutorial text. The API index loaded; a guessed BaseUnityPlugin HTML URL returned 404, so implementation signatures are also checked through the pinned BepInEx assembly and source rather than assuming that URL exists.

Applied: BaseUnityPlugin/Awake lifecycle, BepInPlugin/BepInDependency metadata, ConfigFile.Bind with value ranges, ManualLogSource and Harmony runtime patches.

## Valheim and Jotunn

- https://valheim-modding.github.io/Jotunn/guides/guide.html?tabs=tabid-1
- https://valheim-modding.github.io/Jotunn/tutorials/items.html
- https://valheim-modding.github.io/Jotunn/tutorials/config.html
- https://valtools.org/wiki.php?page=Setting-Up-Mod-Development-Environment
- https://valtools.org/wiki.php?page=Creating-Your-First-Mod
- https://github.com/Valheim-Modding/JotunnModStub
- https://github.com/Valheim-Modding/Jotunn
- https://www.nuget.org/packages/ValheimGameLibs/0.221.4

Applied: Valheim-specific BepInEx runtime pack, net48 plugin, vanilla-prefab-available event, cloned CustomItem/ItemConfig/recipe, Jotunn admin-only synchronized configuration, network compatibility enforcement.

The Valtools pages are mirrors of the community wiki. Their first-mod tutorial explicitly targets game 0.219.16; it is used for architecture, not claimed as proof of current gameplay compatibility. The project instead compiles against pinned 0.221.4 reference signatures. These stripped/publicized references expose signatures but have no executable game method bodies and cannot replace runtime testing.

Inspected reference signatures for TreeBase/Destructible/HitData, Player/Character/Humanoid, Inventory/ItemDrop, ZNetView, PrivateArea and input UI classes. No game binaries are committed or packaged.
