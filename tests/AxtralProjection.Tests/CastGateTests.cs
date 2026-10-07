using AxtralProjection.Core;
using Xunit;
namespace AxtralProjection.Tests;
public class CastGateTests
{
    [Fact] public void SuccessfulReleaseEndsAim() { var gate = new CastGate(); Assert.Equal(CastFailure.None, gate.Begin(15, true, 15, false)); Assert.Equal(CastFailure.None, gate.Release(15, true, 15, false, 2)); Assert.False(gate.Aiming); }
    [Fact] public void ExhaustionPreventsAiming() { var gate = new CastGate(); Assert.Equal(CastFailure.Cooldown, gate.Begin(15, true, 15, true)); Assert.False(gate.Aiming); }
    [Fact] public void ZeroCooldownAllowsNextCast() { var gate = new CastGate(); gate.Begin(15, true, 15, false); gate.Release(15, true, 15, false, 2); Assert.Equal(CastFailure.None, gate.Begin(15, true, 15, false)); }
}
