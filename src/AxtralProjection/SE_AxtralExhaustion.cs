using System;
using System.IO;
using System.Reflection;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
namespace AxtralProjection;
public sealed class SE_AxtralExhaustion : StatusEffect { }
internal sealed class Exhaustion
{
    public const string EffectName = "SE_AxtralExhaustion";
    public static int Hash => EffectName.GetStableHashCode();
    private readonly SE_AxtralExhaustion template;
    public Exhaustion()
    {
        template = ScriptableObject.CreateInstance<SE_AxtralExhaustion>();
        template.name = EffectName; template.m_name = "Axtral Exhaustion";
        template.m_tooltip = "Axtral Projection is recharging.";
        template.m_icon = CreateIcon(); template.m_ttl = 180;
        if (!ItemManager.Instance.AddStatusEffect(new CustomStatusEffect(template, false)))
            throw new InvalidOperationException("Axtral Exhaustion registration failed.");
    }
    public bool Apply(Player player, float seconds)
    {
        if (seconds <= 0) return true;
        // Resolve the registered asset, which may have been copied into the active DB.
        var registered = ObjectDB.instance ? ObjectDB.instance.GetStatusEffect(Hash) : null;
        if (!registered) return false;
        registered.m_ttl = seconds;
        return player.GetSEMan().AddStatusEffect(Hash, true, 0, 0, -1) != null;
    }
    private static Sprite CreateIcon()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("AxtralProjection.exhaustion_icon.png")
            ?? throw new InvalidOperationException("Missing embedded exhaustion icon.");
        using var bytes = new MemoryStream(); stream.CopyTo(bytes);
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        // The current Unity module references netstandard 2.1 while BepInEx 5 plugins
        // target net48. Reflect this one static entrypoint instead of mixing framework facades.
        var conversion = Type.GetType("UnityEngine.ImageConversion, UnityEngine.ImageConversionModule", true)!;
        var load = conversion.GetMethod("LoadImage", BindingFlags.Public | BindingFlags.Static, null,
            new[] { typeof(Texture2D), typeof(byte[]), typeof(bool) }, null)
            ?? throw new MissingMethodException("Unity ImageConversion.LoadImage(Texture2D, byte[], bool)");
        if (!(bool)load.Invoke(null, new object[] { texture, bytes.ToArray(), false }))
        { UnityEngine.Object.Destroy(texture); throw new InvalidOperationException("Invalid exhaustion icon."); }
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
}
