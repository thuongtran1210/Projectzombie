using System;
using ProjectZombie.Features.Defense3D;

public static class DefenseConeTelegraphChecks
{
    private static int _checks;
    private static void Check(bool value, string name)
    {
        if (!value) throw new Exception(name);
        _checks++; Console.WriteLine("PASS " + name);
    }
    private static DefenseConeTelegraphRuntime Core(DefenseSession s) => new DefenseConeTelegraphRuntime(s, 0.65f, 2.5f, 120f);
    public static void Main()
    {
        var origin = new DefensePoint3(0f, 2f, 0f);
        var forward = new DefensePoint3(0f, 0f, 1f);
        using (var s = new DefenseSession())
        {
            var core = Core(s);
            Check(!core.TryBegin(origin, forward), "Ready session cannot start telegraph");
            s.Start();
            Check(!core.TryBegin(origin, default), "Zero ground-plane facing cannot begin a cone");
            Check(!core.TryBegin(new DefensePoint3(float.NaN, 0f, 0f), forward), "Invalid origin cannot create an attack snapshot");
            Check(core.TryBegin(origin, forward) && core.IsPending, "Valid windup reserves exactly one pending attack");
            Check(!core.TryBegin(origin, new DefensePoint3(1f, 0f, 0f)), "Reentrant begin cannot rotate or replace the pending telegraph");
            Check(!core.TryResolve(out _), "Telegraph has no instant damage at start");
            s.Tick(0.64f, 1000f, false);
            Check(!core.TryResolve(out _), "Damage remains blocked before 0.65-second boundary");
            s.Tick(0.01f, 1000f, false);
            Check(core.TryResolve(out var cone) && !core.IsPending, "Exact windup boundary releases one locked cone");
            Check(cone.Origin.Y == 2f && cone.Forward.Z == 1f, "Resolved attack preserves captured origin and facing");
            Check(!core.TryResolve(out _), "Duplicate resolve cannot deliver damage twice");
            Check(cone.Contains(new DefensePoint3(0f, 2f, 2.5f)), "Exact forward radius boundary is inside cone");
            Check(!cone.Contains(new DefensePoint3(0f, 2f, 2.51f)), "Target outside radius can dodge the hit");
            Check(!cone.Contains(new DefensePoint3(0f, 2f, -1f)), "Target moving behind the attacker is outside locked cone");
            Check(cone.Contains(new DefensePoint3((float)Math.Sqrt(3d), 2f, 1f)), "Exact sixty-degree half-angle boundary is included");
            Check(!cone.Contains(new DefensePoint3(2f, 2f, 0.5f)), "Target beyond the 120-degree cone edge is excluded");
            Check(!cone.Contains(new DefensePoint3(0f, 5f, 0f)), "XYZ range rejects target vertically outside attack volume");
            Check(cone.Contains(origin), "Coincident target remains valid inside the attack volume");
            Check(!new DefenseConeSnapshot(origin, default, 2.5f, 120f).Contains(origin), "Invalid snapshot facing fails closed even at origin");
            Check(!new DefenseConeSnapshot(origin, forward, -1f, 120f).Contains(origin), "Invalid cone radius fails closed");
            Check(core.TryBegin(origin, forward), "Runtime can prepare a later attack when cooldown owner permits");
            core.Cancel(); s.Tick(1f, 1000f, false);
            Check(!core.TryResolve(out _), "Stun or Knockback cancellation cannot leave delayed damage");
            core.TryBegin(origin, forward); s.Pause();
            Check(!core.TryResolve(out _) && !core.IsPending, "Pause cancels pending telegraph instead of hiding a future hit");
            s.Resume(); s.Tick(1f, 1000f, false);
            Check(!core.TryResolve(out _) && core.TryBegin(origin, forward) && !core.TryResolve(out _), "Resume requires a new full windup before any hit");
            s.Restart(); s.Tick(1f, 1000f, false);
            Check(!core.TryResolve(out _) && !core.IsPending, "Old-generation telegraph cannot resolve after restart");
            core.TryBegin(origin, forward); s.Tick(1f, 0f, false);
            Check(!core.TryResolve(out _), "Terminal Base loss blocks pending attack resolution");
        }
        using (var s = new DefenseSession())
        {
            s.Start(); var core = Core(s); core.TryBegin(origin, forward); s.Tick(10f, 1000f, false);
            Check(core.TryResolve(out _) && !core.TryResolve(out _), "Long frame resolves once without catch-up damage bursts");
            core.TryBegin(origin, forward); s.OpenLevelUp();
            Check(!core.TryResolve(out _) && !core.IsPending, "Upgrade modal cancels warning and pending strike");
            s.Resume();
            Check(core.TryBegin(origin, new DefensePoint3(float.MaxValue, 0f, float.MaxValue)) && core.Preview.Forward.IsFinite,
                "Extreme finite direction normalizes without overflow");
        }
        bool rejected = false;
        using (var s = new DefenseSession())
        {
            try { new DefenseConeTelegraphRuntime(s, 0f, 2.5f, 120f); } catch (ArgumentException) { rejected = true; }
            Check(rejected, "Zero windup is rejected so strategy cannot silently become an instant attack");
            rejected = false;
            try { new DefenseConeTelegraphRuntime(s, 0.65f, 2.5f, 361f); } catch (ArgumentException) { rejected = true; }
            Check(rejected, "Invalid angle is rejected at configuration boundary");
        }
        Console.WriteLine("TOTAL " + _checks + " PASS (standalone cone telegraph/geometry, no Unity runtime)");
    }
}
