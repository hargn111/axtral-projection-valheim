using AxtralProjection.Core;
using Xunit;
namespace AxtralProjection.Tests;
public class CastGateTests
{
    [Fact]
    public void SuccessfulReleasePaysTenRunesAndStartsFortyFiveSecondCooldown()
    {
        var gate = new CastGate();
        Assert.Equal(CastFailure.None, gate.Begin(100, 11, 10, 1, 11, 10));
        int paid = 0;
        Assert.Equal(CastFailure.None, gate.Release(100, 11, 10, 1, 11, 10, 45, 2, n => { paid += n; return true; }));
        Assert.Equal(10, paid); Assert.Equal(145, gate.ReadyAt); Assert.False(gate.Aiming);
        Assert.Equal(CastFailure.Cooldown, gate.Begin(144, 11, 10, 1, 11, 10));
    }
}
