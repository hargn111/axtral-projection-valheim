using System;
using System.Collections.Generic;
using System.Linq;
using AxtralProjection.Core;
using UnityEngine;

namespace AxtralProjection;
internal sealed class Target
{
    public Component Object { get; }
    public IDestructible Damageable { get; }
    public Collider Collider { get; }
    public int MinimumTier { get; }
    public Vector3 Position => Object.transform.position;
    public Target(Component obj, IDestructible damageable, Collider collider, int tier)
    { Object = obj; Damageable = damageable; Collider = collider; MinimumTier = tier; }
}
internal static class Targets
{
    // Deliberately do not select TreeLog, characters, buildings or arbitrary destructibles.
    public static List<Target> Find(Vector3 origin, Vector3 forward, float range, float angle, int tier, bool stumps, string vineNames)
    {
        var extras = new HashSet<string>(vineNames.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(n => n.Trim()), StringComparer.Ordinal);
        var seen = new HashSet<int>();
        var result = new List<Target>();
        foreach (var collider in Physics.OverlapSphere(origin, range, ~0, QueryTriggerInteraction.Ignore))
        {
            Component? component = null;
            IDestructible? destructible = null;
            int minTier = 0;
            var tree = collider.GetComponentInParent<TreeBase>();
            if (tree) { component = tree; destructible = tree; minTier = tree.m_minToolTier; }
            else
            {
                var other = collider.GetComponentInParent<Destructible>();
                if (!other || other.GetComponentInParent<Piece>() || other.GetComponentInParent<Character>() || other.GetComponentInParent<ItemDrop>()) continue;
                string name = other.gameObject.name.Replace("(Clone)", "").Trim();
                bool stump = stumps && other.m_destructibleType == DestructibleType.Tree && TargetRules.IsStump(name);
                if (!stump && !extras.Contains(name)) continue;
                component = other; destructible = other; minTier = other.m_minToolTier;
            }
            if (component == null || destructible == null || !seen.Add(component.GetInstanceID())) continue;
            var nview = component.GetComponent<ZNetView>();
            if (!nview || !nview.IsValid()) continue;
            var delta = component.transform.position - origin;
            if (!Cone.Contains(delta.x, delta.y, delta.z, forward.x, forward.z, range, angle)) continue;
            if (!TargetRules.Eligible(true, PrivateArea.CheckAccess(component.transform.position, 0, false), minTier, tier)) continue;
            result.Add(new Target(component, destructible, collider, minTier));
        }
        return result.OrderBy(t => Vector3.Distance(origin, t.Position)).ToList();
    }
    public static void Hit(Target target, Player player, Vector3 origin, Vector3 forward, int tier, float range, float angle)
    {
        if (!target.Object || !player || player.IsDead() || !target.Collider) return;
        var view = target.Object.GetComponent<ZNetView>();
        if (!view || !view.IsValid() || !PrivateArea.CheckAccess(target.Position, 0, false)) return;
        var delta = target.Position - origin;
        if (!Cone.Contains(delta.x, delta.y, delta.z, forward.x, forward.z, range, angle) || tier < target.MinimumTier) return;
        var hit = new HitData { m_toolTier = (short)Math.Min(tier, short.MaxValue), m_point = target.Collider.ClosestPoint(target.Position + Vector3.up), m_dir = forward, m_pushForce = 0 };
        hit.m_damage.m_chop = 1000000f;
        hit.SetAttacker(player);
        // Vanilla Damage routes the hit to the ZNetView owner and preserves falling logs/drops.
        target.Damageable.Damage(hit);
    }
}
