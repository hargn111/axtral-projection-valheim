using AxtralProjection.Core;
using Xunit;
namespace AxtralProjection.Tests;
public class TrapezoidTests
{
    [Theory]
    [InlineData(0, 1, 0, true)]
    [InlineData(2, 2, 0, true)]
    [InlineData(3, 2, 0, false)]
    [InlineData(2.1, 2, 0.1, true)]
    [InlineData(0, -1, 0.2, false)]
    [InlineData(0, 20.2, 0.3, true)]
    [InlineData(0, 21, 0.3, false)]
    public void RadiusAwareBounds(double lateral, double forward, double radius, bool expected) =>
        Assert.Equal(expected, Trapezoid.Contains(lateral, 0, forward, 0, 1, radius, 20, 3, 30));
    [Fact] public void HeadingIsNormalized() => Assert.True(Trapezoid.Contains(0, 0, 10, 0, 10, 0, 20, 3, 30));
    [Fact] public void BaseHeightBandAllowsSlopes() => Assert.True(Trapezoid.Contains(0, 9, 10, 0, 1, 0, 20, 3, 30));
    [Fact] public void BaseHeightBandRejectsDifferentElevation() => Assert.False(Trapezoid.Contains(0, 11, 10, 0, 1, 0, 20, 3, 30));
    [Fact] public void InvalidHeadingIsRejected() => Assert.False(Trapezoid.Contains(0, 0, 1, 0, 0, 0, 20, 3, 30));
}
