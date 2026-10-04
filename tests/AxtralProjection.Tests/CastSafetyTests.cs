using AxtralProjection.Core;
using Xunit;
namespace AxtralProjection.Tests;
public class CastSafetyTests
{
    [Theory]
    [InlineData(10, 10, 1, CastFailure.Skill)]
    [InlineData(11, 9, 1, CastFailure.Runes)]
    [InlineData(11, 10, null, CastFailure.Axe)]
    public void RequirementsCheckedBeforeAiming(double skill, int runes, int? tier, CastFailure expected)
    { var g = new CastGate(); Assert.Equal(expected, g.Begin(0, skill, runes, tier, 11, 10)); Assert.False(g.Aiming); }
    [Theory]
    [InlineData(10, 10, 1, CastFailure.Skill)]
    [InlineData(11, 9, 1, CastFailure.Runes)]
    [InlineData(11, 10, null, CastFailure.Axe)]
    public void RequirementsRecheckedOnRelease(double skill, int runes, int? tier, CastFailure expected)
    { var g = new CastGate(); g.Begin(0, 11, 10, 1, 11, 10); Assert.Equal(expected, g.Release(1, skill, runes, tier, 11, 10, 45, 1, n => throw new System.Exception("Must not pay"))); Assert.Equal(0, g.ReadyAt); }
    [Fact]
    public void CancelDoesNotPayOrStartCooldown()
    { var g = new CastGate(); g.Begin(0, 11, 10, 1, 11, 10); g.Cancel(); Assert.Equal(CastFailure.InvalidState, g.Release(0, 11, 10, 1, 11, 10, 45, 1, n => throw new System.Exception())); Assert.Equal(0, g.ReadyAt); }
    [Fact]
    public void EmptyConeDoesNotPay()
    { var g = new CastGate(); g.Begin(0, 11, 10, 1, 11, 10); Assert.Equal(CastFailure.NoTargets, g.Release(0, 11, 10, 1, 11, 10, 45, 0, n => throw new System.Exception())); Assert.False(g.Aiming); Assert.Equal(0, g.ReadyAt); }
    [Fact]
    public void FailedPaymentDoesNotStartCooldown()
    { var g = new CastGate(); g.Begin(0, 11, 10, 1, 11, 10); Assert.Equal(CastFailure.PaymentFailed, g.Release(0, 11, 10, 1, 11, 10, 45, 1, n => false)); Assert.Equal(0, g.ReadyAt); }
    [Fact]
    public void DuplicateReleaseCannotPayAgain()
    { var g = new CastGate(); g.Begin(0, 11, 10, 1, 11, 10); g.Release(0, 11, 10, 1, 11, 10, 45, 1, n => true); Assert.Equal(CastFailure.InvalidState, g.Release(0, 11, 10, 1, 11, 10, 45, 1, n => throw new System.Exception())); }
    [Fact]
    public void CooldownSurvivesControllerRecreationAndAllowsExactBoundary()
    { var g = new CastGate(145); Assert.Equal(CastFailure.Cooldown, g.Begin(144, 11, 10, 1, 11, 10)); Assert.Equal(CastFailure.None, g.Begin(145, 11, 10, 1, 11, 10)); }
}
