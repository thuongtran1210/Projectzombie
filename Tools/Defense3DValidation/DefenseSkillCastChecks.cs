using System;
using ProjectZombie.Features.Defense3D;

public static class DefenseSkillCastChecks
{
    private static int _checks;
    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception(name);
        _checks++;
        Console.WriteLine("PASS " + name);
    }

    private static DefenseSkillDefinition Definition() => new DefenseSkillDefinition(DefenseSkillKind.DepToOng,
        10f, 3.5f, 180f, 12f, 0.2f, true, DefenseTargetMask.All);
    private static DefenseResolvedAim Aim(float x = 5f)
    {
        DefenseAimResolver.TryResolve(default, new DefensePoint3(x, 0f, 0f), Math.Abs(x), out var result);
        return result;
    }

    public static void Main()
    {
        Casts();
        Areas();
        Console.WriteLine("TOTAL " + _checks + " PASS (standalone skill cast core, no Unity runtime)");
    }

    private static void Casts()
    {
        using (var session = new DefenseSession())
        using (var runtime = new DefenseSkillCastRuntime(session, Definition()))
        {
            var effect = new Effect();
            var request = new DefenseSkillCastRequest(Aim(), session, session.RunGeneration);
            Check(!runtime.TryCast(request, default, true, effect), "Unstarted skill cast fails closed");
            session.Start();
            request = new DefenseSkillCastRequest(Aim(), session, session.RunGeneration);
            Check(!runtime.TryCast(request, default, false, effect) && effect.Prepares == 0, "Down or disabled actor never prepares an effect");
            Check(!runtime.TryCast(new DefenseSkillCastRequest(Aim(), new object(), session.RunGeneration), default, true, effect),
                "Foreign-session release cannot cast");
            Check(!runtime.TryCast(new DefenseSkillCastRequest(Aim(11f), session, session.RunGeneration), default, true, effect),
                "Out-of-range target cannot cast");
            Check(!runtime.TryCast(request, new DefensePoint3(2f, 0f, 0f), true, effect), "Stale actor origin invalidates delayed aim request");
            effect.Accept = false;
            Check(!runtime.TryCast(request, default, true, effect) && runtime.Ability.RemainingCooldown == 0f,
                "Blocked preparation does not consume cooldown");
            effect.Accept = true;
            effect.Prepared = Aim(20f);
            Check(!runtime.TryCast(request, default, true, effect) && runtime.Ability.RemainingCooldown == 0f,
                "Invalid prepared endpoint is rejected before cooldown commit");
            effect.Prepared = Aim(3f);
            Check(runtime.TryCast(request, default, true, effect) && effect.Executions == 1 &&
                effect.Last.Target.X == 3f && runtime.Ability.RemainingCooldown == 12f,
                "Prepared clipped endpoint is executed once after cooldown commit");
            Check(!runtime.TryCast(request, default, true, effect) && effect.Executions == 1, "Duplicate release cannot execute twice");
            session.Restart();
            Check(!runtime.TryCast(request, default, true, effect), "Old-generation release is rejected after restart");
            request = new DefenseSkillCastRequest(Aim(), session, session.RunGeneration);
            effect.OnPrepare = () => session.Pause();
            Check(!runtime.TryCast(request, default, true, effect) && runtime.Ability.RemainingCooldown == 0f,
                "Gate is rechecked after preparation callbacks");
            session.Resume();
            effect.OnPrepare = null;
            bool nested = true;
            effect.OnExecute = () => nested = runtime.TryCast(request, default, true, effect);
            Check(runtime.TryCast(request, default, true, effect) && !nested,
                "Effect callbacks cannot reenter the same skill cast");
            session.Restart();
            request = new DefenseSkillCastRequest(Aim(), session, session.RunGeneration);
            effect.OnExecute = () => { throw new InvalidOperationException("simulated effect failure"); };
            bool threw = false;
            try { runtime.TryCast(request, default, true, effect); } catch (InvalidOperationException) { threw = true; }
            Check(threw && runtime.Ability.RemainingCooldown > 0f && !runtime.TryCast(request, default, true, effect),
                "Throwing effect retains committed cooldown and cannot grant free repeated casts");
            session.Restart();
            request = new DefenseSkillCastRequest(Aim(), session, session.RunGeneration);
            effect.OnExecute = null;
            effect.OnPrepare = () => session.Restart();
            Check(!runtime.TryCast(request, default, true, effect) && runtime.Ability.RemainingCooldown == 0f,
                "Restart during preparation cannot cast into the new round");
            runtime.Dispose();
            Check(!runtime.TryCast(new DefenseSkillCastRequest(Aim(), session, session.RunGeneration), default, true, effect),
                "Disposed cast owner stays closed");
        }
        bool invalid = false;
        try { new DefenseSkillDefinition(DefenseSkillKind.Dash, float.NaN, 1f, 0f, 3f, 0.25f, false, DefenseTargetMask.All); }
        catch (ArgumentOutOfRangeException) { invalid = true; }
        Check(invalid, "Definition snapshot rejects invalid authoring values");
    }

    private static void Areas()
    {
        using (var index = new DefenseTargetIndex())
        {
            var first = new Target(1f, false);
            var second = new Target(2f, true);
            var outside = new Target(20f, false);
            index.Register(first); index.Register(first); index.Register(second); index.Register(outside);
            var buffer = new IDefenseTargetPosition[3];
            Check(index.TryCopyWithin(default, 3.5f, DefenseTargetMask.All, buffer, out var count) && count == 2 &&
                ReferenceEquals(buffer[0], first) && ReferenceEquals(buffer[1], second), "Area query snapshots unique in-range targets in stable order");
            index.Unregister(first.Identity);
            Check(count == 2 && ReferenceEquals(buffer[1], second), "Death unregister callbacks do not skip later snapshotted targets");
            Check(index.TryCopyWithin(default, 3.5f, DefenseTargetMask.Ground, buffer, out count) && count == 0,
                "Area query respects target masks");
            index.Register(first);
            var small = new IDefenseTargetPosition[1];
            Check(!index.TryCopyWithin(default, 3.5f, DefenseTargetMask.All, small, out count) && count == 0 && small[0] == null,
                "Saturated area buffer rejects entire cast instead of silently dropping hits");
            second.IsAvailable = false;
            Check(index.TryCopyWithin(default, 3.5f, DefenseTargetMask.All, small, out count) && count == 1,
                "Unavailable targets do not consume area buffer capacity");
            Check(!index.TryCopyWithin(default, float.NaN, DefenseTargetMask.All, small, out count) &&
                !index.TryCopyWithin(default, 1f, (DefenseTargetMask)99, small, out count), "Area query rejects invalid range or target mask");
        }
    }

    private sealed class Effect : IDefenseSkillEffect
    {
        public int Prepares, Executions;
        public bool Accept = true;
        public DefenseResolvedAim Prepared = Aim();
        public DefenseResolvedAim Last;
        public Action OnPrepare, OnExecute;
        public bool TryPrepare(DefenseResolvedAim requested, out DefenseResolvedAim prepared)
        {
            Prepares++;
            prepared = Prepared;
            OnPrepare?.Invoke();
            return Accept;
        }
        public void Execute(DefenseResolvedAim prepared) { Executions++; Last = prepared; OnExecute?.Invoke(); }
    }

    private sealed class Target : IDefenseTargetPosition
    {
        public object Identity => this;
        public bool IsAvailable { get; set; } = true;
        public bool IsAirborne { get; }
        public DefensePoint3 Position { get; }
        public Target(float x, bool air) { Position = new DefensePoint3(x, 0f, 0f); IsAirborne = air; }
    }
}
