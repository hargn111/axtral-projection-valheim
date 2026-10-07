using AxtralProjection.Core;
using Xunit;
namespace AxtralProjection.Tests;
public class ElderTests
{
    [Theory]
    [InlineData(ElderRequirement.Slotted)] [InlineData(ElderRequirement.Active)]
    public void UnmetRequirementNeverEntersAim(ElderRequirement mode)
    {
        var gate = new CastGate();
        var allowed = ElderRules.Allowed(mode, false, false);
        Assert.Equal(CastFailure.Elder, gate.Begin(20, true, 15, false, allowed));
        Assert.False(gate.Aiming);
    }
    [Fact] public void ExpiredRequirementRejectsRelease()
    {
        var gate = new CastGate(); gate.Begin(20, true, 15, false, true);
        Assert.Equal(CastFailure.Elder, gate.Release(20, true, 15, false, 1, false));
        Assert.False(gate.Aiming);
    }
    [Theory]
    [InlineData(ElderRequirement.None, false, false, true)]
    [InlineData(ElderRequirement.Slotted, true, false, true)]
    [InlineData(ElderRequirement.Slotted, false, true, false)]
    [InlineData(ElderRequirement.Active, true, false, false)]
    [InlineData(ElderRequirement.Active, false, true, true)]
    [InlineData((ElderRequirement)999, false, false, false)]
    public void RequirementModes(ElderRequirement mode, bool slotted, bool active, bool expected) => Assert.Equal(expected, ElderRules.Allowed(mode, slotted, active));
}
