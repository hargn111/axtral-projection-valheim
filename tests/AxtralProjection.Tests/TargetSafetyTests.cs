using AxtralProjection.Core;
using Xunit;
namespace AxtralProjection.Tests;
public class TargetSafetyTests
{
    [Theory]
    [InlineData(true, false, 0, 4)]
    [InlineData(false, true, 0, 4)]
    [InlineData(true, true, 3, 2)]
    [InlineData(true, true, 0, -1)]
    [InlineData(true, true, -1, 1)]
    public void RejectsProtectedUnsupportedOrUnderTierTargets(bool kind, bool ward, int minimum, int axe) => Assert.False(TargetRules.Eligible(kind, ward, minimum, axe));
    [Theory]
    [InlineData("OakStub", true)]
    [InlineData("BirchStub", true)]
    [InlineData("FirTree_Stub", true)]
    [InlineData("Beech1", false)]
    [InlineData("wood_wall", false)]
    [InlineData("", false)]
    public void ClassifiesOnlyStumpSuffix(string name, bool expected) => Assert.Equal(expected, TargetRules.IsStump(name));
}
