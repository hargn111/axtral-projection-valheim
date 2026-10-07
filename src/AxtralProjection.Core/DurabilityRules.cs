using System;
namespace AxtralProjection.Core;
public static class DurabilityRules
{
    public static float Cost(float max, float percent) => Math.Max(0, max) * ConfigBounds.Clamp(percent, 0, 25, 0) / 100;
    public static bool CanHit(bool present, bool usesDurability, float current) => present && (!usesDurability || current > 0);
    public static float AfterHit(float current, float max, float percent, bool usesDurability, bool stump) =>
        !usesDurability || stump || percent <= 0 ? current : Math.Max(0, current - Cost(max, percent));
}
