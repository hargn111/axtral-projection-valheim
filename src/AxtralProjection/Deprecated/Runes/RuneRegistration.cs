// Deprecated due to multiplayer custom-item conflicts. Not compiled by default.
// To re-enable: define ENABLE_RUNES and restore registration + resource gates/config
// from the pre-deprecation commit. This isolated item registration alone is insufficient.
#if ENABLE_RUNES
using System;
using System.Collections.Generic;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
namespace AxtralProjection;
internal static class RuneRegistration
{
    private const string RunePrefab = "AxtralRune";
    private const string RuneName = "$item_axtral_rune";
    public static void RegisterRune()
    {
        var config = new ItemConfig { Name = RuneName, Description = "$item_axtral_rune_desc", Amount = 10, CraftingStation = "piece_workbench" };
        config.AddRequirement("Resin", 2); config.AddRequirement("GreydwarfEye", 1);
        var item = new CustomItem(RunePrefab, "Amber", config);
        item.ItemDrop.m_itemData.m_shared.m_maxStackSize = 100;
        item.ItemDrop.m_itemData.m_shared.m_weight = 0.1f;
        if (!ItemManager.Instance.AddItem(item)) throw new InvalidOperationException("AxtralRune registration failed.");
        LocalizationManager.Instance.GetLocalization().AddTranslation("English", new Dictionary<string, string> { { "item_axtral_rune", "Axtral Rune" }, { "item_axtral_rune_desc", "A rune charged with forest magic. Ten fuel one Axtral Projection." } });
        PrefabManager.OnVanillaPrefabsAvailable -= RegisterRune;
    }
}
#endif
