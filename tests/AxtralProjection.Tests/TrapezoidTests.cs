using AxtralProjection.Core;
using Xunit;
namespace AxtralProjection.Tests;
public class TrapezoidTests
{
    [Theory]
    [InlineData(0, 1, true)]
    [InlineData(2, 2, true)]
    [InlineData(3, 2, false)]
    [InlineData(2.1, 2, false)]
    [InlineData(0, -1, false)]
    [InlineData(0, 20.2, false)]
    [InlineData(0, 21, false)]
    public void BasePointBounds(double lateral, double forward, bool expected) =>
        Assert.Equal(expected, Trapezoid.Contains(lateral, 0, forward, 0, 1, 20, 3, 30));
    [Fact] public void OutsideRootIsNotExpandedByTreeSize() => Assert.False(Trapezoid.Contains(5, 0, 2, 0, 1, 15, 1, 30));
    [Fact] public void RectangleIncludesEdge() => Assert.True(Trapezoid.Contains(0.5, 0, 15, 0, 1, 15, 1, 0));
    [Fact] public void RectangleExcludesOutsideRoot() => Assert.False(Trapezoid.Contains(0.51, 0, 7, 0, 1, 15, 1, 0));
    [Fact] public void HeadingIsNormalized() => Assert.True(Trapezoid.Contains(0, 0, 10, 0, 10, 20, 3, 30));
    [Fact] public void BaseHeightBandAllowsSlopes() => Assert.True(Trapezoid.Contains(0, 9, 10, 0, 1, 20, 3, 30));
    [Fact] public void BaseHeightBandRejectsDifferentElevation() => Assert.False(Trapezoid.Contains(0, 11, 10, 0, 1, 20, 3, 30));
    [Fact] public void InvalidHeadingIsRejected() => Assert.False(Trapezoid.Contains(0, 0, 1, 0, 0, 20, 3, 30));
}
