using System;
using ProjectZombie.Features.Defense3D;

public static class DefenseAbilityInputChecks
{
    private static int _checks;
    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception(name);
        _checks++;
        Console.WriteLine("PASS " + name);
    }
    private static bool Close(float a, float b) => Math.Abs(a - b) < 0.001f;

    public static void Main()
    {
        CheckCommands();
        CheckAbilities();
        Console.WriteLine("TOTAL " + _checks + " PASS (standalone input/ability core, no Unity runtime)");
    }

    private static void CheckCommands()
    {
        using (var session = new DefenseSession())
        using (var commands = new DefenseHeroCommands(session))
        {
            commands.Submit(1f, 1f, true, true);
            Check(commands.Movement.X == 0f && !commands.ConsumeBuild(), "Unstarted session rejects input");
            session.Start();
            commands.Submit(1f, 1f, true, true);
            Check(Close(commands.Movement.X, 0.7071068f) && Close(commands.Movement.Z, 0.7071068f) &&
                commands.Movement.Y == 0f, "Movement maps to XZ and clamps diagonal speed");
            Check(commands.ConsumeBuild() && !commands.ConsumeBuild(), "Build request is consumed exactly once");
            commands.Submit(float.MaxValue, float.MaxValue, false, true);
            Check(commands.Movement.IsFinite && Close(commands.Movement.X, 0.7071068f), "Large finite input normalizes without overflow");
            commands.Submit(float.NaN, 1f, true, true);
            Check(commands.Movement.X == 0f && !commands.ConsumeBuild(), "Invalid input clears old movement and build");
            commands.Submit(1f, 0f, true, true);
            session.Pause();
            session.Resume();
            Check(commands.Movement.X == 0f && !commands.ConsumeBuild(), "Pause clears commands before resume");
            commands.Submit(1f, 0f, true, true);
            session.OpenLevelUp();
            commands.Submit(1f, 0f, true, true);
            session.Resume();
            Check(commands.Movement.X == 0f && !commands.ConsumeBuild(), "Level-up blocks submissions and clears queued actions");
            commands.Submit(1f, 0f, true, true);
            commands.Submit(1f, 0f, true, false);
            Check(commands.Movement.X == 0f && !commands.ConsumeBuild(), "Unavailable actor or focus clears input");
            commands.Submit(1f, 0f, true, true);
            session.Restart();
            Check(commands.Movement.X == 0f && !commands.ConsumeBuild(), "Restart cannot replay old-generation input");
            using (var other = new DefenseSession())
            using (var otherCommands = new DefenseHeroCommands(other))
            {
                other.Start();
                otherCommands.Submit(-1f, 0f, true, true);
                Check(commands.Movement.X == 0f && otherCommands.Movement.X == -1f, "Input channels are isolated between sessions");
            }
            commands.Submit(1f, 0f, true, true);
            session.Tick(0f, 0f, false);
            Check(commands.Movement.X == 0f && !commands.ConsumeBuild(), "Terminal transition drops pending input");
        }
        var live = new DefenseSession();
        live.Start();
        var disposed = new DefenseHeroCommands(live);
        disposed.Dispose();
        disposed.Dispose();
        disposed.Submit(1f, 0f, true, true);
        Check(disposed.Movement.X == 0f && !disposed.ConsumeBuild(), "Disposed channel stays closed and disposal is idempotent");
        live.Dispose();
    }

    private static void CheckAbilities()
    {
        using (var session = new DefenseSession())
        using (var ability = new DefenseAbilityRuntime(session))
        using (var second = new DefenseAbilityRuntime(session))
        {
            Check(!ability.TryActivate(2f, true), "Ability rejects activation before session start");
            session.Start();
            Check(!ability.TryActivate(2f, false) && !ability.BeginAim(false), "Unavailable actor cannot cast or aim");
            Check(!ability.TryActivate(0f, true) && !ability.TryActivate(-1f, true) &&
                !ability.TryActivate(float.NaN, true) && !ability.TryActivate(float.PositiveInfinity, true), "Ability rejects invalid cooldowns");
            Check(ability.BeginAim(true) && ability.Phase == DefenseAbilityPhase.Aiming, "Ready ability enters aim state");
            ability.CancelAim();
            Check(ability.Phase == DefenseAbilityPhase.Ready && ability.RemainingCooldown == 0f, "Aim cancellation spends no cooldown");
            ability.BeginAim(true);
            session.Pause();
            session.Resume();
            Check(ability.Phase == DefenseAbilityPhase.Ready, "Pause cancels aim without spending cooldown");
            Check(ability.TryActivate(2f, true) && !ability.TryActivate(2f, true), "Activation commits cooldown once before effect execution");
            Check(second.TryActivate(4f, true), "Different abilities own independent cooldowns");
            session.Tick(0.5f, 100f, false);
            Check(Close(ability.RemainingCooldown, 1.5f), "Cooldown advances with session gameplay time");
            session.OpenLevelUp();
            session.Tick(10f, 100f, false);
            Check(Close(ability.RemainingCooldown, 1.5f) && !ability.TryActivate(1f, true), "Modal freezes cooldown and blocks activation");
            session.Resume();
            session.Tick(1.5f, 100f, false);
            Check(ability.Phase == DefenseAbilityPhase.Ready && ability.TryActivate(1f, true), "Cooldown becomes ready at exact gameplay boundary");
            Check(Close(second.RemainingCooldown, 2f), "Casting one ability does not reset another");
            session.Restart();
            Check(ability.Phase == DefenseAbilityPhase.Ready && second.RemainingCooldown == 0f, "Restart resets all subscribed ability owners");
            ability.BeginAim(true);
            session.Tick(0f, 0f, false);
            Check(ability.Phase == DefenseAbilityPhase.Ready && !ability.TryActivate(1f, true), "Terminal cancels aim and rejects activation");
        }
        using (var session = new DefenseSession())
        {
            session.Start();
            var ability = new DefenseAbilityRuntime(session);
            ability.Dispose();
            ability.Dispose();
            Check(!ability.BeginAim(true) && !ability.TryActivate(1f, true), "Disposed ability cannot aim or activate");
        }
    }
}
