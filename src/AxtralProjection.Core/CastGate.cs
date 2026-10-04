using System;
namespace AxtralProjection.Core;
public enum CastFailure { None, InvalidState, Cooldown, Skill, Axe, Runes, NoTargets, PaymentFailed }
/// <summary>Payment is the commit point. Failed/cancelled/empty casts spend nothing.</summary>
public sealed class CastGate
{
    public bool Aiming { get; private set; }
    public double ReadyAt { get; private set; }
    public CastGate(double readyAt = 0) { ReadyAt = double.IsNaN(readyAt) || double.IsInfinity(readyAt) ? 0 : readyAt; }
    public CastFailure Begin(double now, double skill, int runes, int? axeTier, double requiredSkill, int runeCost)
    {
        if (Aiming) return CastFailure.InvalidState;
        var failure = Check(now, skill, runes, axeTier, requiredSkill, runeCost);
        if (failure == CastFailure.None) Aiming = true;
        return failure;
    }
    public CastFailure Release(double now, double skill, int runes, int? axeTier, double requiredSkill, int runeCost, double cooldown, int targets, Func<int, bool> pay)
    {
        if (!Aiming) return CastFailure.InvalidState;
        Aiming = false;
        if (double.IsNaN(cooldown) || double.IsInfinity(cooldown) || cooldown < 0) return CastFailure.InvalidState;
        var failure = Check(now, skill, runes, axeTier, requiredSkill, runeCost);
        if (failure != CastFailure.None) return failure;
        if (targets <= 0) return CastFailure.NoTargets;
        if (!pay(runeCost)) return CastFailure.PaymentFailed;
        ReadyAt = now + cooldown;
        return CastFailure.None;
    }
    private CastFailure Check(double now, double skill, int runes, int? axeTier, double requiredSkill, int runeCost)
    {
        if (double.IsNaN(now) || double.IsInfinity(now) || double.IsNaN(skill) || double.IsInfinity(skill)
            || double.IsNaN(requiredSkill) || double.IsInfinity(requiredSkill) || requiredSkill < 0 || runeCost < 0) return CastFailure.InvalidState;
        if (now < ReadyAt) return CastFailure.Cooldown;
        if (skill < requiredSkill) return CastFailure.Skill;
        if (!axeTier.HasValue || axeTier.Value < 0) return CastFailure.Axe;
        if (runes < runeCost) return CastFailure.Runes;
        return CastFailure.None;
    }
    public void Cancel() { Aiming = false; }
}
