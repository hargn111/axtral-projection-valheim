using AxtralProjection.Core;
using Xunit;
namespace AxtralProjection.Tests;
public class DurabilityTests
{
    [Theory] [InlineData(100, 1, 1)] [InlineData(200, 2, 4)] [InlineData(100, 0, 0)]
    public void PercentageCost(float max, float percent, float expected) => Assert.Equal(expected, DurabilityRules.Cost(max, percent));
    [Fact] public void BreakingHitLandsAndRemainingTargetsAreUntouched()
    {
        float current = 1; int hits = 0;
        for (int i = 0; i < 5 && DurabilityRules.CanHit(true, true, current); i++)
        { hits++; current = DurabilityRules.AfterHit(current, 100, 1, true, false); }
        Assert.Equal(1, hits); Assert.Equal(0, current);
    }
    [Fact] public void MissingAxeAborts() => Assert.False(DurabilityRules.CanHit(false, true, 100));
    [Fact] public void NonDurableAxeCanHitAtZero() => Assert.True(DurabilityRules.CanHit(true, false, 0));
    [Fact] public void StumpsDoNotCharge() => Assert.Equal(50, DurabilityRules.AfterHit(50, 100, 25, true, true));
    [Fact] public void DisabledChargeDoesNotChangeDurability() => Assert.Equal(50, DurabilityRules.AfterHit(50, 100, 0, true, false));
}
