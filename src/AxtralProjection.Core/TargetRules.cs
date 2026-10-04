using System;
namespace AxtralProjection.Core;
public static class TargetRules
{
    public static bool IsStump(string prefabName) => !string.IsNullOrEmpty(prefabName) && prefabName.EndsWith("Stub", StringComparison.OrdinalIgnoreCase);
    public static bool Eligible(bool supportedKind, bool wardAllowed, int minimumTier, int axeTier) => supportedKind && wardAllowed && axeTier >= 0 && minimumTier >= 0 && axeTier >= minimumTier;
}
