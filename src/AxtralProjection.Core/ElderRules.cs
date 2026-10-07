namespace AxtralProjection.Core;
public enum ElderRequirement { None, Slotted, Active }
public static class ElderRules
{
    public static bool Allowed(ElderRequirement mode, bool slotted, bool active) => mode switch
    { ElderRequirement.None => true, ElderRequirement.Slotted => slotted, ElderRequirement.Active => active, _ => false };
}
