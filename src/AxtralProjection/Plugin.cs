using System;
using System.Collections;
using System.Collections.Generic;
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
[NetworkCompatibility(CompatibilityLevel.VersionCheckOnly, VersionStrictness.None)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string Guid = "haragon.AxtralProjectionValheim";
    private static Plugin? instance;
    private Harmony harmony = null!;
    private const string CastButton = "AxtralProjectionCast";
    private ConfigEntry<KeyboardShortcut> castKey = null!;
    private ConfigEntry<InputManager.GamepadButton> gamepad = null!;
    private ConfigEntry<float> range = null!, angle = null!, cooldown = null!, skill = null!, speed = null!, startWidth = null!, durability = null!, xp = null!;
    private ConfigEntry<int> maxTrees = null!;
    private ConfigEntry<ElderRequirement> elder = null!;
    private ConfigEntry<bool> stumps = null!;
    private ConfigEntry<string> additional = null!, excluded = null!;
    private Player? player;
    private CastGate gate = new CastGate();
    private ItemDrop.ItemData? aimAxe;
    private List<Target> preview = new List<Target>();
    private readonly PreviewEffects effects = new PreviewEffects();
    private float nextPreview;
    private Coroutine? flight;
    private GameObject? axeVisual;
    private Exhaustion exhaustion = null!;
    private Transform? posedArm, posedForearm;
    private Quaternion originalArm, originalForearm;
    private bool IsExhausted(Player p) => p.GetSEMan().HaveStatusEffect(Exhaustion.Hash);

    private void Awake()
    {
        instance = this;
        GameCompat.Log = Logger;
        castKey = Config.Bind("Controls", "CastKey", new KeyboardShortcut(KeyCode.G, KeyCode.LeftShift), "Hold to aim; release to cast. Right mouse or Escape cancels.");
        gamepad = Config.Bind("Controls", "GamepadButton", InputManager.GamepadButton.None, "Optional gamepad cast binding; None disables it to avoid vanilla conflicts.");
        InputManager.Instance.AddButton(Guid, new ButtonConfig { Name = CastButton, ShortcutConfig = castKey, GamepadConfig = gamepad, ActiveInGUI = false, ActiveInCustomGUI = false });
        castKey.SettingChanged += ControlsChanged;
        gamepad.SettingChanged += ControlsChanged;
        range = Synced("Range", 20f, 10f, 50f, "Maximum range in meters.");
        angle = Synced("ConeAngle", 30f, 20f, 45f, "Full cone width in degrees, not half-angle.");
        cooldown = Synced("Cooldown", 180f, 0f, 600f, "Cooldown in seconds after a successful cast.");
        skill = Synced("WoodCuttingLevel", 15f, 0f, 100f, "Required wood cutting skill level.");
        speed = Synced("TravelSpeed", 10f, 1f, 50f, "Astral wave travel speed in meters per second.");
        stumps = Config.Bind("Spell", "ClearStumps", false, new ConfigDescription("Include existing tree stumps. Newly spawned stumps are left for a later cast.", null, new ConfigurationManagerAttributes { IsAdminOnly = true }));
        additional = Config.Bind("Spell", "AdditionalPrefabs", "", new ConfigDescription("Comma-separated exact Destructible prefab names from other mods.", null, new ConfigurationManagerAttributes { IsAdminOnly = true }));
        startWidth = Synced("StartWidth", 3f, 0f, 10f, "Full starting width in meters.");
        durability = Synced("DurabilityCostPercent", 1f, 0f, 25f, "Percent of axe max durability per tree hit; 0 disables.");
        xp = Synced("SkillXpPerTree", 0f, 0f, 1f, "Wood cutting skill gain per tree hit; 0 disables.");
        maxTrees = Config.Bind("Spell", "MaxTrees", 15, new ConfigDescription("Maximum trees per cast, nearest forward first.", new AcceptableValueRange<int>(2, 50), new ConfigurationManagerAttributes { IsAdminOnly = true }));
        excluded = Config.Bind("Spell", "ExcludedPrefabs", "", new ConfigDescription("Comma-separated exact prefab names to never fell. Exclusion wins.", null, new ConfigurationManagerAttributes { IsAdminOnly = true }));
        elder = Config.Bind("Spell", "ElderRequirement", ElderRequirement.Slotted, new ConfigDescription("None, Slotted (chosen Forsaken Power), or Active.", null, new ConfigurationManagerAttributes { IsAdminOnly = true }));
        exhaustion = new Exhaustion();
        harmony = new Harmony(Guid);
        harmony.PatchAll(typeof(Plugin).Assembly);
        Logger.LogInfo("Axtral Projection 0.1.0 loaded. Hold the configured shortcut to aim; release to cast.");
    }
    private ConfigEntry<float> Synced(string name, float value, float min, float max, string description) => Config.Bind("Spell", name, value, new ConfigDescription(description, new AcceptableValueRange<float>(min, max), new ConfigurationManagerAttributes { IsAdminOnly = true }));
    private float Range => ConfigBounds.Clamp(range.Value, 10, 50, 20);
    private float Angle => ConfigBounds.Clamp(angle.Value, 20, 45, 30);
    private float Cooldown => ConfigBounds.Clamp(cooldown.Value, 0, 600, 180);
    private float RequiredSkill => ConfigBounds.Clamp(skill.Value, 0, 100, 15);
    private float Speed => ConfigBounds.Clamp(speed.Value, 1, 50, 10);
    private float StartWidth => ConfigBounds.Clamp(startWidth.Value, 0, 10, 3);
    private float DurabilityPercent => ConfigBounds.Clamp(durability.Value, 0, 25, 1);
    private float SkillXp => ConfigBounds.Clamp(xp.Value, 0, 1, 0);
    private int MaxTrees => ConfigBounds.Clamp(maxTrees.Value, 2, 50);
    private static bool InputBlocked() => UnityEngine.Application.isFocused == false || Time.timeScale == 0 || global::Console.IsVisible() || InventoryGui.IsVisible() || Menu.IsVisible() || TextInput.IsVisible() || (Chat.instance && Chat.instance.HasFocus()) || (Minimap.instance && Minimap.InTextInput());
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
            gate = new CastGate();
        }
        if (!player) return; // Dedicated server registers content/config but does not cast.
        if (InputBlocked() || Unsafe(player)) { Cancel(); if (player.IsDead() || player.IsTeleporting()) StopFlight(); return; }
        if (gate.Aiming)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)) { Cancel(); return; }
            if (player.GetCurrentWeapon() != aimAxe || BestAxe(player) != aimAxe) { Cancel(); Show("Axe changed: spell cancelled."); return; }
            // GetKey rather than GetKeyUp also handles a remapped key or a lost release event.
            if (!ZInput.GetButton(CastButton)) { Release(); return; }
            if (Time.unscaledTime >= nextPreview)
            {
                preview = Targets.Find(player.transform.position, Direction(player), Range, Angle, aimAxe!.m_shared.m_toolTier, stumps.Value, additional.Value, excluded.Value, StartWidth, MaxTrees);
                effects.Set(preview, player.transform.position, Direction(player), Range, Angle, StartWidth);
                nextPreview = Time.unscaledTime + 0.1f;
            }
        }
        else if (ZInput.GetButtonDown(CastButton) && flight == null)
        {
            var axe = BestAxe(player);
            var failure = gate.Begin(player.GetSkillLevel(Skills.SkillType.WoodCutting), axe != null, RequiredSkill, IsExhausted(player));
            if (failure != CastFailure.None) { ShowFailure(failure); return; }
            if (axe == null || (player.GetCurrentWeapon() != axe && !player.EquipItem(axe))) { Cancel(); Show("Unable to equip axe."); return; }
            aimAxe = axe; nextPreview = 0;
        }
    }
    private void Release()
    {
        if (!player) { Cancel(); return; }
        var p = player!; var origin = p.transform.position; var dir = Direction(p); var axe = aimAxe;
        if (axe == null || p.GetCurrentWeapon() != axe || !p.GetInventory().ContainsItem(axe) || (axe.m_shared.m_useDurability && axe.m_durability <= 0))
        { Cancel(); Show("Requires an equipped, unbroken woodcutting axe."); return; }
        if (!ElderRules.Allowed(elder.Value, p.GetGuardianPowerName() == "GP_TheElder", p.GetSEMan().HaveStatusEffect("GP_TheElder".GetStableHashCode())))
        { Cancel(); Show(elder.Value == ElderRequirement.Active ? "Requires the Elder's power to be active." : "Requires the Elder's power to be your Forsaken Power."); return; }
        float castRange = Range, castAngle = Angle, castSpeed = Speed, castWidth = StartWidth;
        var targets = Targets.Find(origin, dir, castRange, castAngle, axe?.m_shared.m_toolTier ?? -1, stumps.Value, additional.Value, excluded.Value, StartWidth, MaxTrees);
        var failure = gate.Release(p.GetSkillLevel(Skills.SkillType.WoodCutting), axe != null, RequiredSkill, IsExhausted(p), targets.Count);
        effects.Clear(); preview.Clear(); aimAxe = null;
        if (failure != CastFailure.None) { ShowFailure(failure); return; }
        if (!exhaustion.Apply(p, Cooldown)) { Show("Unable to apply Axtral Exhaustion; spell cancelled."); return; }
        // Set cooldown before any effect or world operation. A thrown visual error cannot permit a free repeat.
        flight = StartCoroutine(Launch(p, origin, dir, axe!, targets, castRange, castAngle, castSpeed, castWidth, DurabilityPercent, SkillXp));
        Show("Axtral Projection cast!");
    }
    private IEnumerator Launch(Player p, Vector3 origin, Vector3 dir, ItemDrop.ItemData axe, List<Target> targets, float castRange, float castAngle, float castSpeed, float castWidth, float costPercent, float xpPerTree)
    {
        try
        {
            axeVisual = SpellEffects.CreateAxe(); float traveled = 0; int index = 0; int tier = axe.m_shared.m_toolTier;
            while (traveled <= castRange)
            {
                if (!p || p != Player.m_localPlayer || p.IsDead() || p.IsTeleporting()) yield break;
                if (!DurabilityRules.CanHit(p.GetInventory().ContainsItem(axe), axe.m_shared.m_useDurability, axe.m_durability)) yield break;
                SpellEffects.MoveAxe(axeVisual, origin + Vector3.up * 1.5f + dir * traveled, dir, traveled, castAngle);
                while (index < targets.Count && (!targets[index].Object || targets[index].ForwardDistance(origin, dir) <= traveled))
                {
                    var target = targets[index++];
                    if (!target.Object) continue;
                    if (!DurabilityRules.CanHit(p.GetInventory().ContainsItem(axe), axe.m_shared.m_useDurability, axe.m_durability)) yield break;
                    bool hit = false;
                    try { hit = Targets.Hit(target, p, origin, dir, tier, castRange, castAngle, castWidth); }
                    catch (Exception e) { Logger.LogWarning("Could not hit target: " + e.Message); }
                    if (!hit || target.IsStump) continue;
                    if (costPercent > 0 && axe.m_shared.m_useDurability)
                        axe.m_durability = DurabilityRules.AfterHit(axe.m_durability, axe.GetMaxDurability(), costPercent, true, false);
                    try { GameCompat.RaiseWoodCutting(p, xpPerTree); }
                    catch (Exception e) { Logger.LogWarning("Wood cutting XP failed: " + e.Message); }
                    if (axe.m_shared.m_useDurability && axe.m_durability <= 0)
                    { Show("Your axe broke; Axtral Projection stopped."); yield break; }
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
    private void Show(string message) { if (player) GameCompat.Message(player!, MessageHud.MessageType.Center, message); }
    private void ShowFailure(CastFailure failure)
    {
        switch (failure)
        {
            case CastFailure.Cooldown: Show($"Axtral Projection: {Math.Ceiling(player ? player!.GetSEMan().GetStatusEffect(Exhaustion.Hash)?.GetRemaningTime() ?? 0 : 0)}s cooldown."); break;
            case CastFailure.Skill: Show($"Requires Woodcutting level {RequiredSkill}."); break;
            case CastFailure.Axe: Show("Requires an unbroken woodcutting axe in inventory."); break;
            case CastFailure.NoTargets: Show("No eligible trees or stumps in the targeting volume."); break;
            default: Show("Spell cancelled."); break;
        }
    }
    private void OnGUI()
    {
        if (!player || InputBlocked() || !gate.Aiming) return;
        GUI.Box(new Rect(Screen.width / 2f - 220, Screen.height * 0.72f, 440, 55), $"Axtral Projection — {preview.Count} targets\nRelease {castKey.Value} to cast · Right mouse to cancel");
    }
    private void ControlsChanged(object sender, EventArgs e) { Cancel(); }
    private void OnDisable() { Cancel(); StopFlight(); }
    private void OnDestroy() { castKey.SettingChanged -= ControlsChanged; gamepad.SettingChanged -= ControlsChanged; Cancel(); StopFlight(); effects.Dispose(); harmony?.UnpatchSelf(); instance = null; }
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.StartAttack))]
    private static class PreventAttackWhileAiming
    {
        private static bool Prefix(Humanoid __instance) => instance == null || !instance.gate.Aiming || __instance != Player.m_localPlayer;
    }
}
