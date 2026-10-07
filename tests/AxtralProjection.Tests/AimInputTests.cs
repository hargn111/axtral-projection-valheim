using System.Collections.Generic;
using AxtralProjection.Core;
using Xunit;
namespace AxtralProjection.Tests;
public class AimInputTests
{
    [Theory]
    [InlineData("G", true)] [InlineData("GW", true)] [InlineData("GWASD", true)]
    [InlineData("GShiftW", true)] [InlineData("W", false)]
    public void ExtraMovementKeysDoNotBlockStarting(string keys, bool expected)
    {
        Assert.Equal(expected, AimInput.CanBegin(keys.Contains("G"), new bool[0]));
    }
    [Fact] public void ExplicitModifierMustBeHeldAtStart() => Assert.False(AimInput.CanBegin(true, new[]{false}));
    [Fact] public void ExplicitModifierAllowsOtherKeys() => Assert.True(AimInput.CanBegin(true, new[]{true}));
    [Theory]
    [InlineData(false, true, false, true)] [InlineData(false, true, true, true)]
    [InlineData(false, false, true, false)] [InlineData(true, true, false, false)]
    [InlineData(true, false, true, true)]
    public void OnlyStartingDeviceControlsRelease(bool controllerAim, bool keyHeld, bool controllerHeld, bool expected)
        => Assert.Equal(expected, AimInput.Held(controllerAim, keyHeld, controllerHeld));
}
