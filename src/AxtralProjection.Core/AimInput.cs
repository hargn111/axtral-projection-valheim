using System.Collections.Generic;
using System.Linq;
namespace AxtralProjection.Core;
/// <summary>Only configured modifiers gate starting. Once aiming, the starting device's main button owns release.</summary>
public static class AimInput
{
    public static bool CanBegin(bool mainKeyDown, IEnumerable<bool> configuredModifiersHeld)
        => mainKeyDown && configuredModifiersHeld.All(held => held);
    public static bool Held(bool controllerAim, bool mainKeyHeld, bool controllerHeld)
        => controllerAim ? controllerHeld : mainKeyHeld;
}
