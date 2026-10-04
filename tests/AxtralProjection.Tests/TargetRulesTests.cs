using AxtralProjection.Core;
using Xunit;
namespace AxtralProjection.Tests;
public class TargetRulesTests
{
    [Fact] public void RecognizesVanillaStumpSuffix() => Assert.True(TargetRules.IsStump("Beech_Stub"));
    [Fact] public void EligibleTreeAtMatchingTier() => Assert.True(TargetRules.Eligible(true, true, 2, 2));
}
