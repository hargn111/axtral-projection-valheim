namespace AxtralProjection.Core;
public enum CastFailure { None, InvalidState, Cooldown, Skill, Axe, NoTargets }
/// <summary>Pure aim/release state. The adapter owns the native Exhaustion effect.</summary>
public sealed class CastGate
{
    public bool Aiming { get; private set; }
    public CastFailure Begin(double skill, bool axe, double requiredSkill, bool exhausted)
    {
        if (Aiming) return CastFailure.InvalidState;
        var failure = Check(skill, axe, requiredSkill, exhausted);
        Aiming = failure == CastFailure.None;
        return failure;
    }
    public CastFailure Release(double skill, bool axe, double requiredSkill, bool exhausted, int targets)
    {
        if (!Aiming) return CastFailure.InvalidState;
        Aiming = false;
        var failure = Check(skill, axe, requiredSkill, exhausted);
        return failure != CastFailure.None ? failure : targets > 0 ? CastFailure.None : CastFailure.NoTargets;
    }
    private static CastFailure Check(double skill, bool axe, double requiredSkill, bool exhausted)
    {
        if (double.IsNaN(skill) || double.IsInfinity(skill) || double.IsNaN(requiredSkill) || double.IsInfinity(requiredSkill) || requiredSkill < 0) return CastFailure.InvalidState;
        if (exhausted) return CastFailure.Cooldown;
        if (skill < requiredSkill) return CastFailure.Skill;
        return axe ? CastFailure.None : CastFailure.Axe;
    }
    public void Cancel() { Aiming = false; }
}
