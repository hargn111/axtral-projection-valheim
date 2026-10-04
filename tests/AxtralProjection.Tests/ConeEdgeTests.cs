using System;
using AxtralProjection.Core;
using Xunit;
namespace AxtralProjection.Tests;
public class ConeEdgeTests
{
    [Theory]
    [InlineData(0, 0, 30, true)]
    [InlineData(0, 0, 30.01, false)]
    [InlineData(0, 0, -1, false)]
    [InlineData(1, 0, 0, false)]
    [InlineData(0, 31, 0, false)]
    [InlineData(0, 0, 0, true)]
    [InlineData(0, 18, 24, true)]
    [InlineData(0, 19, 24, false)]
    public void RangeIsSpherical(double x, double y, double z, bool expected) => Assert.Equal(expected, Cone.Contains(x, y, z, 0, 1, 30, 30));
    [Theory]
    [InlineData(15, true)]
    [InlineData(15.1, false)]
    [InlineData(-15, true)]
    [InlineData(-15.1, false)]
    public void ThirtyDegreesIsFullWidth(double angle, bool expected)
    {
        double a = angle * Math.PI / 180;
        Assert.Equal(expected, Cone.Contains(10 * Math.Sin(a), 0, 10 * Math.Cos(a), 0, 1, 30, 30));
    }
    [Fact] public void SteeringRotatesCone() { Assert.True(Cone.Contains(10, 0, 0, 1, 0, 30, 30)); Assert.False(Cone.Contains(10, 0, 0, 0, 1, 30, 30)); }
    [Theory]
    [InlineData(double.NaN, 30)]
    [InlineData(30, double.NaN)]
    [InlineData(-1, 30)]
    [InlineData(30, 181)]
    [InlineData(30, 0)]
    public void InvalidSettingsFailClosed(double range, double angle) => Assert.False(Cone.Contains(0, 0, 1, 0, 1, range, angle));
    [Fact] public void ZeroDirectionFailsClosed() => Assert.False(Cone.Contains(0, 0, 1, 0, 0, 30, 30));
    [Fact] public void NonFinitePositionFailsClosed() => Assert.False(Cone.Contains(double.NaN, 0, 1, 0, 1, 30, 30));
}
