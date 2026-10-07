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
    public bool IsStump { get; }
    public bool CountsTowardLimit => !TargetSelection.IsUncappedSapling(Object.gameObject.name);
    public float ForwardDistance(Vector3 origin, Vector3 forward) => Mathf.Max(0, Vector3.Dot(Position - origin, forward));
    public Vector3 Position => Object.transform.position;
    public Target(Component obj, IDestructible damageable, Collider collider, int tier, bool stump)
    { Object = obj; Damageable = damageable; Collider = collider; MinimumTier = tier; IsStump = stump; }
}
internal static class Targets
{
    // Deliberately do not select TreeLog, characters, buildings or arbitrary destructibles.
    public static List<Target> Find(Vector3 origin, Vector3 forward, float range, float angle, int tier, bool stumps, string additionalNames, string excludedNames, float width, int maxTrees)
    {
        var extras = TargetSelection.Parse(additionalNames);
        var excluded = TargetSelection.Parse(excludedNames);
        var seen = new HashSet<int>();
        var result = new List<Target>();
        foreach (var collider in Physics.OverlapSphere(origin, Mathf.Sqrt(range * range + Mathf.Pow(width / 2 + range * Mathf.Tan(angle * Mathf.Deg2Rad / 2), 2) + 100) + 2, ~0, QueryTriggerInteraction.Ignore))
        {
            Component? component = null;
            IDestructible? destructible = null;
            int minTier = 0; bool isStump = false;
            var tree = collider.GetComponentInParent<TreeBase>();
            if (tree) { component = tree; destructible = tree; minTier = tree.m_minToolTier; }
            else
            {
                var other = collider.GetComponentInParent<Destructible>();
                if (!other || other.GetComponentInParent<Piece>() || other.GetComponentInParent<Character>() || other.GetComponentInParent<ItemDrop>()) continue;
                string name = other.gameObject.name.Replace("(Clone)", "").Trim();
                isStump = other.m_destructibleType == DestructibleType.Tree && TargetRules.IsStump(name);
                component = other; destructible = other; minTier = other.m_minToolTier;
            }
            if (component == null || destructible == null || !seen.Add(component.GetInstanceID())) continue;
            if (!TargetSelection.Allowed(component.gameObject.name, tree, isStump, stumps, extras, excluded)) continue;
            var basePosition = component.transform.position;
            var trunkCollider = component.GetComponentsInChildren<Collider>()
                .Where(c => !c.isTrigger && c.bounds.min.y <= basePosition.y + 1.5f && c.bounds.max.y >= basePosition.y - 0.5f)
                .OrderBy(c => (new Vector2(c.bounds.center.x - basePosition.x, c.bounds.center.z - basePosition.z)).sqrMagnitude)
                .FirstOrDefault() ?? collider;
            var nview = component.GetComponent<ZNetView>();
            if (!nview || !nview.IsValid()) continue;
            var delta = component.transform.position - origin;
            if (!Trapezoid.Contains(delta.x, delta.y, delta.z, forward.x, forward.z, range, width, angle)) continue;
            if (!TargetRules.Eligible(true, PrivateArea.CheckAccess(component.transform.position, 0, false), minTier, tier)) continue;
            result.Add(new Target(component, destructible, trunkCollider, minTier, isStump));
        }
        return TargetSelection.Nearest(result, t => t.ForwardDistance(origin, forward), maxTrees, t => t.CountsTowardLimit);
    }
    public static bool Hit(Target target, Player player, Vector3 origin, Vector3 forward, int tier, float range, float angle, float width)
    {
        if (!target.Object || !player || player.IsDead() || !target.Collider) return false;
        var view = target.Object.GetComponent<ZNetView>();
        if (!view || !view.IsValid() || !PrivateArea.CheckAccess(target.Position, 0, false)) return false;
        var delta = target.Position - origin;
        if (!Trapezoid.Contains(delta.x, delta.y, delta.z, forward.x, forward.z, range, width, angle) || tier < target.MinimumTier) return false;
        var hit = new HitData { m_toolTier = (short)Math.Min(tier, short.MaxValue), m_point = target.Collider.ClosestPoint(target.Position + Vector3.up), m_dir = forward, m_pushForce = 0 };
        hit.m_damage.m_chop = 1000000f;
        hit.SetAttacker(player);
        // Vanilla Damage routes the hit to the ZNetView owner and preserves falling logs/drops.
        target.Damageable.Damage(hit);
        return true;
    }
}
