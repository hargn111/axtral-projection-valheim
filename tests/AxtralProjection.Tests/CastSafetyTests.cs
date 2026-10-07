using AxtralProjection.Core;
using Xunit;
namespace AxtralProjection.Tests;
public class CastSafetyTests
{
    [Theory] [InlineData(14, true, false, CastFailure.Skill)] [InlineData(15, false, false, CastFailure.Axe)] [InlineData(15, true, true, CastFailure.Cooldown)]
    public void RequirementsRecheckedOnRelease(double skill, bool axe, bool exhausted, CastFailure expected)
    { var g = new CastGate(); g.Begin(15, true, 15, false); Assert.Equal(expected, g.Release(skill, axe, 15, exhausted, 1)); Assert.False(g.Aiming); }
    [Fact] public void CancelledCastCannotRelease() { var g = new CastGate(); g.Begin(15, true, 15, false); g.Cancel(); Assert.Equal(CastFailure.InvalidState, g.Release(15, true, 15, false, 1)); }
    [Fact] public void EmptyVolumeDoesNotStartCast() { var g = new CastGate(); g.Begin(15, true, 15, false); Assert.Equal(CastFailure.NoTargets, g.Release(15, true, 15, false, 0)); }
    [Fact] public void DuplicateReleaseCannotCastAgain() { var g = new CastGate(); g.Begin(15, true, 15, false); g.Release(15, true, 15, false, 1); Assert.Equal(CastFailure.InvalidState, g.Release(15, true, 15, false, 1)); }
    [Theory] [InlineData(double.NaN)] [InlineData(double.PositiveInfinity)] public void InvalidSkillRejected(double skill) => Assert.Equal(CastFailure.InvalidState, new CastGate().Begin(skill, true, 15, false));
}
