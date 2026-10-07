using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AxtralProjection.Core;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace AxtralProjection;
[BepInPlugin(Guid, "Axtral Projection", "0.1.0")]
[BepInDependency(Jotunn.Main.ModGuid)]
[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Patch)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string Guid = "haragon.AxtralProjectionValheim";
    public const string RunePrefab = "AxtralRune";
    private const string RuneName = "$item_axtral_rune";
    private const string CooldownKey = Guid + ".readyAt";
    private static Plugin? instance;
    private Harmony harmony = null!;
    private ConfigEntry<KeyCode> castKey = null!;
    private ConfigEntry<float> range = null!, angle = null!, cooldown = null!, skill = null!, speed = null!;
    private ConfigEntry<int> runeCost = null!;
    private ConfigEntry<bool> stumps = null!;
    private ConfigEntry<string> vines = null!;
    private Player? player;
    private CastGate gate = new CastGate();
    private ItemDrop.ItemData? aimAxe;
    private List<Target> preview = new List<Target>();
    private readonly PreviewEffects effects = new PreviewEffects();
    private float nextPreview;
    private Coroutine? flight;
    private GameObject? axeVisual;
    private string feedback = "";
    private float feedbackUntil;
    private Transform? posedArm, posedForearm;
    private Quaternion originalArm, originalForearm;
    private static double Now => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;

    private void Awake()
    {
        instance = this;
        GameCompat.Log = Logger;
        castKey = Config.Bind("Controls", "CastKey", KeyCode.G, "Hold to aim; release to cast. Right mouse or Escape cancels.");
        range = Synced("Range", 30f, 1f, 100f, "Maximum range in meters.");
        angle = Synced("ConeAngle", 30f, 1f, 180f, "Full cone width in degrees, not half-angle.");
        cooldown = Synced("Cooldown", 45f, 0f, 600f, "Cooldown in seconds after a successful cast.");
        skill = Synced("WoodcuttingLevel", 11f, 0f, 100f, "Required Woodcutting skill.");
        speed = Synced("TravelSpeed", 30f, 1f, 100f, "Astral wave travel speed in meters per second.");
        runeCost = Config.Bind("Spell", "RuneCost", 10, new ConfigDescription("Axtral Runes spent per successful cast.", new AcceptableValueRange<int>(0, 100), new ConfigurationManagerAttributes { IsAdminOnly = true }));
        stumps = Config.Bind("Spell", "ClearStumps", true, new ConfigDescription("Include existing tree stumps. Newly spawned stumps are left for a later cast.", null, new ConfigurationManagerAttributes { IsAdminOnly = true }));
        vines = Config.Bind("Spell", "VinePrefabs", "", new ConfigDescription("Comma-separated exact Destructible prefab names from other mods. Empty by default: no verified vanilla cave-vine equivalent.", null, new ConfigurationManagerAttributes { IsAdminOnly = true }));
        PrefabManager.OnVanillaPrefabsAvailable += RegisterRune;
        harmony = new Harmony(Guid);
        harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo("Axtral Projection 0.1.0 loaded. Hold G to aim, release to cast.");
    }
    private ConfigEntry<float> Synced(string name, float value, float min, float max, string description) => Config.Bind("Spell", name, value, new ConfigDescription(description, new AcceptableValueRange<float>(min, max), new ConfigurationManagerAttributes { IsAdminOnly = true }));
    private void RegisterRune()
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
    private static bool InputBlocked() => UnityEngine.Application.isFocused == false || Time.timeScale == 0 || global::Console.IsVisible() || InventoryGui.IsVisible() || Menu.IsVisible() || TextInput.IsVisible() || (Chat.instance && Chat.instance.HasFocus());
    private static bool Unsafe(Player p) => p.IsDead() || p.IsTeleporting() || p.InCutscene() || p.InPlaceMode() || p.InDodge() || p.IsSwimming() || p.IsAttached() || p.InAttack();
    private static ItemDrop.ItemData? BestAxe(Player p) => p.GetInventory().GetAllItems().Where(i => i.m_shared.m_skillType == Skills.SkillType.Axes && i.GetDamage().m_chop > 0 && (!i.m_shared.m_useDurability || i.m_durability > 0)).OrderByDescending(i => i.m_shared.m_toolTier).ThenByDescending(i => i.GetDamage().m_chop).FirstOrDefault();
    private static Vector3 Direction(Player p)
    {
        var dir = p.GetLookDir(); dir.y = 0;
        return dir.sqrMagnitude > 0.001f ? dir.normalized : p.transform.forward;
    }
    private void Update()
    {
        try { RestorePose(); Tick(); }
        catch (Exception e) { Cancel(); Logger.LogError(e); Show("Axtral Projection failed; see BepInEx log."); }
    }
    private void Tick()
    {
        var current = Player.m_localPlayer;
        if (current != player)
        {
            Cancel(); StopFlight(); player = current;
            double ready = 0;
            if (player && player.m_customData.TryGetValue(CooldownKey, out var saved)) double.TryParse(saved, NumberStyles.Float, CultureInfo.InvariantCulture, out ready);
            gate = new CastGate(ready);
        }
        if (!player) return; // Dedicated server registers content/config but does not cast.
        if (InputBlocked() || Unsafe(player)) { Cancel(); if (player.IsDead() || player.IsTeleporting()) StopFlight(); return; }
        if (gate.Aiming)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)) { Cancel(); return; }
            if (player.GetCurrentWeapon() != aimAxe || BestAxe(player) != aimAxe) { Cancel(); Show("Axe changed: spell cancelled."); return; }
            // GetKey rather than GetKeyUp also handles a remapped key or a lost release event.
            if (!Input.GetKey(castKey.Value)) { Release(); return; }
            if (Time.unscaledTime >= nextPreview)
            {
                preview = Targets.Find(player.transform.position, Direction(player), range.Value, angle.Value, aimAxe!.m_shared.m_toolTier, stumps.Value, vines.Value);
                effects.Set(preview, player.transform.position, Direction(player), range.Value, angle.Value);
                nextPreview = Time.unscaledTime + 0.1f;
            }
        }
        else if (Input.GetKeyDown(castKey.Value) && flight == null)
        {
            var axe = BestAxe(player);
            var failure = gate.Begin(Now, player.GetSkillLevel(Skills.SkillType.WoodCutting), player.GetInventory().CountItems(RuneName), axe?.m_shared.m_toolTier, skill.Value, runeCost.Value);
            if (failure != CastFailure.None) { ShowFailure(failure); return; }
            if (axe == null || (player.GetCurrentWeapon() != axe && !player.EquipItem(axe))) { Cancel(); Show("Unable to equip axe."); return; }
            aimAxe = axe; nextPreview = 0;
        }
    }
    private void Release()
    {
        if (!player) { Cancel(); return; }
        var p = player!; var origin = p.transform.position; var dir = Direction(p); var axe = BestAxe(p);
        float castRange = range.Value, castAngle = angle.Value, castSpeed = speed.Value;
        var targets = Targets.Find(origin, dir, castRange, castAngle, axe?.m_shared.m_toolTier ?? -1, stumps.Value, vines.Value);
        var failure = gate.Release(Now, p.GetSkillLevel(Skills.SkillType.WoodCutting), p.GetInventory().CountItems(RuneName), axe?.m_shared.m_toolTier, skill.Value, runeCost.Value, cooldown.Value, targets.Count, n => PayRunes(p, n));
        effects.Clear(); preview.Clear(); aimAxe = null;
        if (failure != CastFailure.None) { ShowFailure(failure); return; }
        p.m_customData[CooldownKey] = gate.ReadyAt.ToString("R", CultureInfo.InvariantCulture);
        // Set cooldown before any effect or world operation. A thrown visual error cannot permit a free repeat.
        flight = StartCoroutine(Launch(p, origin, dir, axe!.m_shared.m_toolTier, targets, castRange, castAngle, castSpeed));
        Show("Axtral Projection cast!");
    }
    private static bool PayRunes(Player p, int amount)
    {
        var inv = p.GetInventory(); int before = inv.CountItems(RuneName);
        if (before < amount) return false;
        inv.RemoveItem(RuneName, amount);
        return inv.CountItems(RuneName) == before - amount;
    }
    private IEnumerator Launch(Player p, Vector3 origin, Vector3 dir, int tier, List<Target> targets, float castRange, float castAngle, float castSpeed)
    {
        try
        {
            axeVisual = SpellEffects.CreateAxe(); float traveled = 0; int index = 0;
            while (traveled <= castRange)
            {
                if (!p || p != Player.m_localPlayer || p.IsDead() || p.IsTeleporting()) yield break;
                SpellEffects.MoveAxe(axeVisual, origin + Vector3.up * 1.5f + dir * traveled, dir, traveled, castAngle);
                while (index < targets.Count && (!targets[index].Object || Vector3.Distance(origin, targets[index].Position) <= traveled))
                {
                    var target = targets[index++];
                    if (!target.Object) continue;
                    try { Targets.Hit(target, p, origin, dir, tier, castRange, castAngle); }
                    catch (Exception e) { Logger.LogWarning("Could not hit target: " + e.Message); }
                }
                if (traveled >= castRange) break;
                yield return null;
                traveled = Mathf.Min(castRange, traveled + Time.deltaTime * castSpeed);
            }
        }
        finally { if (axeVisual) Destroy(axeVisual); axeVisual = null; flight = null; }
    }
    private void Cancel() { RestorePose(); gate.Cancel(); effects.Clear(); preview.Clear(); aimAxe = null; }
    private void StopFlight() { if (flight != null) StopCoroutine(flight); flight = null; if (axeVisual) Destroy(axeVisual); axeVisual = null; }
    private void LateUpdate()
    {
        if (!gate.Aiming || !player) return;
        // Local procedural hold pose; no custom AnimatorController or Unity asset bundle required.
        var animator = player!.GetComponentInChildren<Animator>();
        if (!animator || !animator.isHuman) return;
        var arm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
        var forearm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        if (arm) { posedArm = arm; originalArm = arm.localRotation; arm.localRotation = originalArm * Quaternion.Euler(-95, 0, -25); }
        if (forearm) { posedForearm = forearm; originalForearm = forearm.localRotation; forearm.localRotation = originalForearm * Quaternion.Euler(-40, 0, 0); }
    }
    private void RestorePose()
    {
        // Undo last frame's offset before animation, including when the Animator is culled.
        if (posedArm) posedArm!.localRotation = originalArm;
        if (posedForearm) posedForearm!.localRotation = originalForearm;
        posedArm = null; posedForearm = null;
    }
    private void Show(string message) { feedback = message; feedbackUntil = Time.unscaledTime + 3; if (player) GameCompat.Message(player!, MessageHud.MessageType.Center, message); }
    private void ShowFailure(CastFailure failure)
    {
        switch (failure)
        {
            case CastFailure.Cooldown: Show($"Axtral Projection: {Math.Ceiling(gate.ReadyAt - Now)}s cooldown."); break;
            case CastFailure.Skill: Show($"Requires Woodcutting level {skill.Value}."); break;
            case CastFailure.Axe: Show("Requires an unbroken woodcutting axe in inventory."); break;
            case CastFailure.Runes: Show($"Requires {runeCost.Value} Axtral Runes."); break;
            case CastFailure.NoTargets: Show("No eligible trees or stumps in the cone."); break;
            case CastFailure.PaymentFailed: Show("Rune payment failed; spell cancelled."); break;
            default: Show("Spell cancelled."); break;
        }
    }
    private void OnGUI()
    {
        if (!player || InputBlocked()) return;
        string text = gate.Aiming ? $"Axtral Projection — {preview.Count} targets\nRelease {castKey.Value} to cast · Right mouse to cancel" : Now < gate.ReadyAt ? $"Axtral Projection · {Math.Ceiling(gate.ReadyAt - Now)}s" : Time.unscaledTime < feedbackUntil ? feedback : "";
        if (text.Length > 0) GUI.Box(new Rect(Screen.width / 2f - 220, Screen.height * 0.72f, 440, 55), text);
    }
    private void OnDestroy() { Cancel(); StopFlight(); effects.Dispose(); PrefabManager.OnVanillaPrefabsAvailable -= RegisterRune; harmony?.UnpatchSelf(); instance = null; }
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.StartAttack))]
    private static class PreventAttackWhileAiming
    {
        private static bool Prefix(Humanoid __instance) => instance == null || !instance.gate.Aiming || __instance != Player.m_localPlayer;
    }
}
