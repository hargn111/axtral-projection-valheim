using System;
namespace AxtralProjection.Core;
public static class Trapezoid
{
    public static bool Contains(double dx, double dy, double dz, double fx, double fz, double range, double width, double angle)
    {
        foreach (var v in new[] { dx, dy, dz, fx, fz, range, width, angle })
            if (double.IsNaN(v) || double.IsInfinity(v)) return false;
        double length = Math.Sqrt(fx * fx + fz * fz);
        if (length <= 0 || range < 0 || width < 0 || angle < 0 || angle >= 180 || Math.Abs(dy) > 10) return false;
        fx /= length; fz /= length;
        double forward = dx * fx + dz * fz;
        if (forward < 0 || forward > range) return false;
        double lateral = Math.Abs(dx * fz - dz * fx);
        return lateral <= width / 2 + Math.Max(forward, 0) * Math.Tan(angle * Math.PI / 360);
    }
}
