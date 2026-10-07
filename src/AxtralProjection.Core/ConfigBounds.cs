using System;
namespace AxtralProjection.Core;
public static class ConfigBounds
{
    public static float Clamp(float value, float min, float max, float fallback) =>
        float.IsNaN(value) || float.IsInfinity(value) ? fallback : Math.Max(min, Math.Min(max, value));
    public static int Clamp(int value, int min, int max) => Math.Max(min, Math.Min(max, value));
}
