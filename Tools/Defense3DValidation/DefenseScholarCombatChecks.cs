using System;
using ProjectZombie.Features.Defense3D;

public static class DefenseScholarCombatChecks
{
    private static int _checks;
    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception(name);
        _checks++;
        Console.WriteLine("PASS " + name);
    }
    private static bool Near(float a, float b) => Math.Abs(a - b) < 0.001f;
    private sealed class Attack : IDefenseSkillEffect, IDefenseSharedAbilityEffect
    {
        public DefenseAbilityRuntime SharedAbility { get; set; }
        public float CastCooldown { get; set; } = 0.65f;
        public int Hits;
        public bool TryPrepare(DefenseResolvedAim aim, out DefenseResolvedAim prepared) { prepared = aim; return true; }
        public void Execute(DefenseResolvedAim prepared) { Hits++; }
    }
    public static void Main()
    {
        using (var session = new DefenseSession())
        using (var other = new DefenseSession())
        using (var ability = new DefenseAbilityRuntime(session))
        {
            var definition = new DefenseSkillDefinition(DefenseSkillKind.BasicAttack, 2.05f, 2.05f, 35f, .65f, .18f, true, DefenseTargetMask.All);
            bool rejected = false;
            try { new DefenseSkillCastRuntime(other, definition, ability); } catch (ArgumentException) { rejected = true; }
            Check(rejected, "Foreign shared cooldown owner rejected");
            var attack = new Attack { SharedAbility = ability };
            var cast = new DefenseSkillCastRuntime(session, definition, ability);
            Check(ReferenceEquals(cast.Ability, ability), "Manual and auto attack share exactly one ability");
            session.Start();
            DefenseAimResolver.TryResolve(default, new DefensePoint3(2f, 0f, 0f), 2.05f, out var aim);
            var request = new DefenseSkillCastRequest(aim, session.SessionIdentity, session.RunGeneration);
            var foreignEffect = new Attack { SharedAbility = new DefenseAbilityRuntime(other) };
            Check(!cast.TryCast(request, default, true, foreignEffect), "Mismatched effect cannot spend shared attack cooldown");
            foreignEffect.SharedAbility.Dispose();
            attack.CastCooldown = float.NaN;
            Check(!cast.TryCast(request, default, true, attack) && ability.RemainingCooldown == 0f, "Invalid dynamic cooldown cannot execute attack");
            attack.CastCooldown = .65f;
            Check(cast.TryCast(request, default, true, attack) && attack.Hits == 1, "Manual cast executes once");
            Check(!ability.TryActivate(.65f, true), "Auto path cannot bypass manual cooldown");
            Check(!cast.TryCast(request, default, true, attack), "Duplicate pointer or keyboard input blocked");
            session.Tick(.65f, 1000f, false);
            Check(ability.TryActivate(.65f, true), "Optional auto path can use same owner when ready");
            Check(!cast.TryCast(request, default, true, attack), "Manual path cannot bypass auto cooldown");
            session.Tick(.65f, 1000f, false);
            attack.CastCooldown = .4f;
            Check(cast.TryCast(request, default, true, attack) && Near(ability.RemainingCooldown, .4f), "Attack speed modifier uses dynamic shared cooldown");
            session.Tick(.4f, 1000f, false);
            cast.Dispose();
            Check(ability.TryActivate(12f, true), "Disposing presenter leaves externally owned attack ability alive");
            session.Tick(2f, 1000f, false);
            Check(Near(ability.RemainingCooldown, 10f), "Relic elapsed time accounted before refund");
            Check(ability.ReduceRemainingCooldownFraction(.2f) && Near(ability.RemainingCooldown, 8f), "Signature removes twenty percent of remaining time");
            session.Pause();
            Check(!ability.ReduceRemainingCooldownFraction(.2f) && Near(ability.RemainingCooldown, 8f), "Paused signature cannot refund cooldown");
            session.Tick(10f, 1000f, false);
            Check(Near(ability.RemainingCooldown, 8f), "Pause freezes refunded cooldown");
            session.Resume();
            Check(!ability.ReduceRemainingCooldownFraction(float.NaN) && !ability.ReduceRemainingCooldownFraction(-1f) && !ability.ReduceRemainingCooldownFraction(1.1f), "Invalid cooldown refunds rejected");
            Check(ability.ReduceRemainingCooldownFraction(0f) && Near(ability.RemainingCooldown, 8f), "Zero refund preserves ready time");
            Check(ability.ReduceRemainingCooldownFraction(1f) && Near(ability.RemainingCooldown, 0f), "Full refund clamps at ready");
            Check(ability.ReduceRemainingCooldownFraction(.2f) && Near(ability.RemainingCooldown, 0f), "Refund on ready relic cannot create negative cooldown");
            ability.TryActivate(12f, true);
            session.Restart();
            Check(Near(ability.RemainingCooldown, 0f), "Restart clears shared cooldown");
            Check(!cast.TryCast(request, default, true, attack), "Disposed old presenter stays closed across restart");
            var windup = new DefenseConeTelegraphRuntime(session, .18f, 2.05f, 210f);
            Check(windup.TryBegin(default, new DefensePoint3(0f, 0f, 1f)) && !windup.TryResolve(out _), "Attack windup prevents immediate hit");
            session.Tick(.19f, 1000f, false);
            Check(windup.TryResolve(out var cone) && !windup.TryResolve(out _), "Delayed attack resolves exactly once");
            Check(cone.Contains(new DefensePoint3(2f, 0f, 0f)) && !cone.Contains(new DefensePoint3(0f, 0f, -2f)), "Wide attack arc includes side and excludes rear");
            Check(!cone.Contains(new DefensePoint3(0f, 3f, 0f)), "Attack range excludes high airborne targets");
            var signature = new DefenseConeTelegraphRuntime(session, .38f, 6f, 360f);
            signature.TryBegin(default, new DefensePoint3(0f, 0f, 1f));
            signature.Cancel(); session.Tick(.4f, 1000f, false);
            Check(!signature.TryResolve(out _), "Dash interruption cancels pending signature hit");
            signature.TryBegin(default, new DefensePoint3(0f, 0f, 1f));
            session.Tick(.4f, 1000f, false);
            Check(signature.TryResolve(out var circle) && circle.Contains(new DefensePoint3(0f, 0f, -5f)), "Signature covers full circle including rear");
            signature.TryBegin(default, new DefensePoint3(0f, 0f, 1f));
            session.Restart(); session.Tick(.4f, 1000f, false);
            Check(!signature.TryResolve(out _), "Old signature cannot hit a new run");
            ability.Dispose();
            Check(!ability.ReduceRemainingCooldownFraction(.2f), "Disposed cooldown owner rejects refund");
        }
        Console.WriteLine("TOTAL " + _checks + " PASS (standalone scholar combat contracts; no Unity runtime)");
    }
}
