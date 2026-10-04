using System;
namespace AxtralProjection.Core;
/// <summary>A horizontal aiming wedge, clipped to a three-dimensional range sphere.
/// Angle is the FULL width: 30 degrees means 15 degrees either side.</summary>
public static class Cone
{
    public static bool Contains(double dx, double dy, double dz, double forwardX, double forwardZ, double range, double fullAngleDegrees)
    {
        if (!Finite(dx) || !Finite(dy) || !Finite(dz) || !Finite(forwardX) || !Finite(forwardZ)
            || !Finite(range) || !Finite(fullAngleDegrees) || range <= 0 || fullAngleDegrees <= 0 || fullAngleDegrees > 180) return false;
        double forwardLength = Math.Sqrt(forwardX * forwardX + forwardZ * forwardZ);
        if (forwardLength < 1e-8 || dx * dx + dy * dy + dz * dz > range * range + 1e-8) return false;
        double horizontalLength = Math.Sqrt(dx * dx + dz * dz);
        if (horizontalLength < 1e-8) return true;
        double dot = (dx * forwardX + dz * forwardZ) / (horizontalLength * forwardLength);
        return dot + 1e-8 >= Math.Cos(fullAngleDegrees * Math.PI / 360.0);
    }
    private static bool Finite(double v) => !double.IsNaN(v) && !double.IsInfinity(v);
}
