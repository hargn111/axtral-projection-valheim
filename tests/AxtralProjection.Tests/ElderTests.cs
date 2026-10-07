using AxtralProjection.Core;
using Xunit;
namespace AxtralProjection.Tests;
public class ElderTests
{
    [Theory]
    [InlineData(ElderRequirement.None, false, false, true)]
    [InlineData(ElderRequirement.Slotted, true, false, true)]
    [InlineData(ElderRequirement.Slotted, false, true, false)]
    [InlineData(ElderRequirement.Active, true, false, false)]
    [InlineData(ElderRequirement.Active, false, true, true)]
    [InlineData((ElderRequirement)999, false, false, false)]
    public void RequirementModes(ElderRequirement mode, bool slotted, bool active, bool expected) => Assert.Equal(expected, ElderRules.Allowed(mode, slotted, active));
}
