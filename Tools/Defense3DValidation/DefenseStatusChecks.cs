using System;
using ProjectZombie.Features.Defense3D;

public static class DefenseStatusChecks
{
    private static int _checks;
    private static void Check(bool value, string name)
    {
        if (!value) throw new Exception(name);
        _checks++; Console.WriteLine("PASS " + name);
    }
    private static bool Near(float value, float expected) => Math.Abs(value - expected) < 0.0001f;
    private static DefenseStatusRequest Request(DefenseSession s, DefenseStatusKind kind, float duration = 1f, float strength = 0.5f,
        DefensePoint3 direction = default) => new DefenseStatusRequest(kind, duration, strength, direction, s.SessionIdentity, s.RunGeneration);
    public static void Main()
    {
        using (var s = new DefenseSession())
        {
            var core = new DefenseStatusRuntime(s, 10f, 2f, 2);
            Check(!core.TryApply(Request(s, DefenseStatusKind.Slow)), "Ready gate rejects statuses");
            s.Start();
            Check(!core.TryApply(new DefenseStatusRequest(DefenseStatusKind.Stun, 1f, 1f, default)), "Unscoped legacy request cannot affect a production status runtime");
            Check(core.TryApply(Request(s, DefenseStatusKind.Slow, 4f, 0.2f)) && Near(core.MovementMultiplier, 0.8f), "Slow scales movement from authored speed without changing stats");
            Check(core.TryApply(Request(s, DefenseStatusKind.Slow, 1f, 0.6f)) && Near(core.MovementMultiplier, 0.4f), "Strongest overlapping Slow wins instead of multiplying");
            Check(!core.TryApply(Request(s, DefenseStatusKind.Slow, 5f, 0.9f)) && Near(core.MovementMultiplier, 0.4f), "Full bounded Slow buffer rejects new effect without evicting live entries");
            s.Tick(1f, 1000f, false);
            Check(Near(core.MovementMultiplier, 0.8f), "Expired strong Slow reveals the still-live weaker long effect");
            Check(core.TryApply(Request(s, DefenseStatusKind.Slow, 1f, 1f)) && core.MovementMultiplier == 0f, "Expired slot is reusable and full Slow immobilizes");
            s.Pause(); s.Tick(100f, 1000f, false);
            Check(core.MovementMultiplier == 0f && !core.TryApply(Request(s, DefenseStatusKind.Stun)), "Pause freezes status expiry and rejects new effects");
            s.Resume(); s.Tick(1f, 1000f, false);
            Check(Near(core.MovementMultiplier, 0.8f), "Resume advances only accepted gameplay time");
            s.Tick(2f, 1000f, false);
            Check(core.MovementMultiplier == 1f, "Final Slow expiry restores normal speed exactly");
            Check(core.TryApply(Request(s, DefenseStatusKind.Stun, 1f)) && core.BlocksActions, "Stun blocks movement/attack actions");
            core.TryApply(Request(s, DefenseStatusKind.Stun, 0.1f)); s.Tick(0.2f, 1000f, false);
            Check(core.IsStunned, "Short Stun refresh cannot shorten an existing Stun");
            core.TryApply(Request(s, DefenseStatusKind.Stun, 2f)); s.Tick(1f, 1000f, false);
            Check(core.IsStunned, "Longer Stun refresh extends the expiry");
            s.OpenLevelUp(); s.Tick(100f, 1000f, false);
            Check(core.IsStunned, "Upgrade modal freezes Stun duration");
            s.Resume(); s.Tick(1f, 1000f, false);
            Check(!core.BlocksActions, "Exact Stun expiry restores actions");
            var knock = Request(s, DefenseStatusKind.Knockback, 0.5f, 0.5f, new DefensePoint3(3f, 4f, 0f));
            Check(core.TryApply(knock) && core.IsKnocking && core.BlocksActions, "Knockback owns a bounded displacement action");
            Check(!core.TryApply(knock), "Active Knockback cannot be restarted or stacked");
            s.Tick(0.25f, 1000f, false); core.TryConsumeKnockStep(out var first);
            Check(Near(first.X, 0.3f) && Near(first.Y, 0.4f), "Knock direction normalizes and emits proportional distance");
            Check(!core.TryConsumeKnockStep(out _), "Same-frame consumption cannot move actor twice");
            s.Pause(); s.Tick(100f, 1000f, false);
            Check(!core.TryConsumeKnockStep(out _) && core.IsKnocking, "Pause cannot consume queued Knockback distance");
            s.Resume(); s.Tick(0.25f, 1000f, false); core.TryConsumeKnockStep(out var last);
            Check(Near(last.X, 0.3f) && Near(last.Y, 0.4f) && !core.IsKnocking, "Final step completes exact one-metre knock distance");
            core.TryApply(knock); s.Tick(5f, 1000f, false); core.TryConsumeKnockStep(out var catchup);
            Check(Near(catchup.X, 0.6f) && Near(catchup.Y, 0.8f) && !core.TryConsumeKnockStep(out _), "Long frame clips to remaining distance instead of overshooting");
            core.TryApply(knock); core.TryApply(Request(s, DefenseStatusKind.Stun, 1f)); core.CancelKnockback();
            Check(!core.IsKnocking && core.IsStunned, "Blocked displacement cancellation preserves independent Stun");
            core.Reset();
            Check(!core.BlocksActions && core.MovementMultiplier == 1f, "Cleanup/reset clears all timed state");
            Check(!core.TryApply(Request(s, DefenseStatusKind.Knockback, 1f, 1f)), "Zero knock direction is rejected");
            Check(!core.TryApply(Request(s, DefenseStatusKind.Slow, 11f)), "Overlong effects are rejected by configured cap");
            Check(!core.TryApply(Request(s, DefenseStatusKind.Slow, 1f, float.NaN)) && !core.TryApply(Request(s, DefenseStatusKind.Stun, -1f)), "Invalid strength and duration fail closed");
            using (var other = new DefenseSession())
            {
                other.Start();
                Check(!core.TryApply(Request(other, DefenseStatusKind.Stun)), "Foreign session status cannot cross combat scope");
            }
            var old = Request(s, DefenseStatusKind.Stun); core.TryApply(old); s.Restart();
            Check(!core.IsStunned && !core.TryApply(old), "Old generation cannot retain or reapply status after restart");
            core.Reset();
            Check(!core.TryApply(old) && core.TryApply(Request(s, DefenseStatusKind.Stun)), "Reset accepts current-generation payload and still rejects old payload");
            core.Reset(); core.TryApply(Request(s, DefenseStatusKind.Knockback, 1f, 1f, new DefensePoint3(1f, 0f, 0f)));
            s.Tick(1f, 0f, false);
            Check(!core.TryConsumeKnockStep(out _) && !core.TryApply(Request(s, DefenseStatusKind.Stun)), "Terminal Base loss closes status delivery and displacement");
        }
        using (var s = new DefenseSession())
        {
            s.Start();
            var huge = Request(s, DefenseStatusKind.Knockback, 1f, 1f, new DefensePoint3(float.MaxValue, 0f, float.MaxValue));
            Check(DefenseStatusRuntime.TryKnockVector(huge, 2f, out var vector) && vector.IsFinite && Near(vector.X, 1.41421356f), "Extreme finite knock direction normalizes without overflow");
            Check(!DefenseStatusRuntime.TryKnockVector(Request(s, DefenseStatusKind.Knockback, 1f, 1f, new DefensePoint3(float.NaN, 0f, 0f)), 2f, out _), "Non-finite knock geometry is rejected");
        }
        Console.WriteLine("TOTAL " + _checks + " PASS (standalone scoped status core, no Unity runtime)");
    }
}
