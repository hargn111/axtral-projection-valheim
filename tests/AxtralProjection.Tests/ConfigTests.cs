using AxtralProjection.Core;
using Xunit;
namespace AxtralProjection.Tests;
public class ConfigTests
{
    [Theory]
    [InlineData(-10, 10)] [InlineData(100, 50)] [InlineData(20, 20)]
    public void RangeIsClamped(float input, float expected) => Assert.Equal(expected, ConfigBounds.Clamp(input, 10, 50, 20));
    [Theory]
    [InlineData(float.NaN)] [InlineData(float.PositiveInfinity)] [InlineData(float.NegativeInfinity)]
    public void NonFiniteFallsBack(float input) => Assert.Equal(20, ConfigBounds.Clamp(input, 10, 50, 20));
}
