namespace AxtralProjection.Core;
public enum CastFailure { None, InvalidState, Cooldown, Skill, Axe, NoTargets, Elder }
/// <summary>Pure aim/release state. The adapter owns the native Exhaustion effect.</summary>
public sealed class CastGate
{
    public bool Aiming { get; private set; }
    public CastFailure Begin(double skill, bool axe, double requiredSkill, bool exhausted, bool elderAllowed = true)
    {
        if (Aiming) return CastFailure.InvalidState;
        var failure = Check(skill, axe, requiredSkill, exhausted, elderAllowed);
        Aiming = failure == CastFailure.None;
        return failure;
    }
    public CastFailure Release(double skill, bool axe, double requiredSkill, bool exhausted, int targets, bool elderAllowed = true)
    {
        if (!Aiming) return CastFailure.InvalidState;
        Aiming = false;
        var failure = Check(skill, axe, requiredSkill, exhausted, elderAllowed);
        return failure != CastFailure.None ? failure : targets > 0 ? CastFailure.None : CastFailure.NoTargets;
    }
    private static CastFailure Check(double skill, bool axe, double requiredSkill, bool exhausted, bool elderAllowed = true)
    {
        if (double.IsNaN(skill) || double.IsInfinity(skill) || double.IsNaN(requiredSkill) || double.IsInfinity(requiredSkill) || requiredSkill < 0) return CastFailure.InvalidState;
        if (!elderAllowed) return CastFailure.Elder;
        if (exhausted) return CastFailure.Cooldown;
        if (skill < requiredSkill) return CastFailure.Skill;
        return axe ? CastFailure.None : CastFailure.Axe;
    }
    public void Cancel() { Aiming = false; }
}
