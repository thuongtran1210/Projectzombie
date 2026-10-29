using System;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.UI.Controls;

public static class DefenseGestureAimChecks
{
    private static int _checks;
    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception(name);
        _checks++;
        Console.WriteLine("PASS " + name);
    }
    private static bool Close(float a, float b) => Math.Abs(a - b) < 0.001f;
    private static SkillPointerGesture NewGesture() => new SkillPointerGesture(25f, 140f, 0.12d);

    public static void Main()
    {
        Gestures();
        SessionGestures();
        Aim();
        Console.WriteLine("TOTAL " + _checks + " PASS (standalone gesture/aim core, no Unity runtime)");
    }

    private static void Gestures()
    {
        var gesture = NewGesture();
        bool invalidConfig = false;
        try { new SkillPointerGesture(25f, 20f, 0.12d); } catch (ArgumentOutOfRangeException) { invalidConfig = true; }
        Check(invalidConfig, "Gesture rejects unordered configuration");
        Check(gesture.Release(7, 0f, 0f, 0d, false).Signal == SkillPointerSignal.None, "Release without press is ignored");
        Check(gesture.Press(7, 0f, 0f, 0d).Signal == SkillPointerSignal.Pressed, "Quick tap starts without indicator flash");
        Check(gesture.Press(8, 0f, 0f, 0d).Signal == SkillPointerSignal.None &&
            gesture.Release(8, 100f, 0f, 0.01d, false).Signal == SkillPointerSignal.None && gesture.IsPressed,
            "Second pointer cannot steal or release first pointer");
        Check(gesture.Release(7, 0f, 0f, 0.05d, false).Signal == SkillPointerSignal.TapReleased && !gesture.IsPressed,
            "Quick tap releases once and clears ownership before dispatch");
        Check(gesture.Release(7, 0f, 0f, 0.06d, false).Signal == SkillPointerSignal.None, "Duplicate pointer-up cannot dispatch twice");
        gesture.Press(7, 0f, 0f, 1d);
        Check(gesture.Tick(1.119d).Signal == SkillPointerSignal.Pressed, "Hold remains pending before threshold");
        var held = gesture.Tick(1.121d);
        Check(held.Signal == SkillPointerSignal.Aiming && held.AimStarted && !gesture.Tick(1.122d).AimStarted,
            "Hold enters aim once without coroutine");
        Check(gesture.Release(7, 0f, 0f, 1.13d, false).Signal == SkillPointerSignal.AimReleased, "Hold release is not treated as quick tap");
        gesture.Press(-1, 0f, 0f, 2d);
        var drag = gesture.Move(-1, 84f, 112f, 2.01d, false);
        Check(drag.AimStarted && Close(drag.DirectionX, 0.6f) && Close(drag.DirectionY, 0.8f) && drag.Pull == 1f,
            "Drag normalizes direction and clamps full pull");
        Check(gesture.Move(-1, 0f, 0f, 2.02d, false).Signal == SkillPointerSignal.Aiming && gesture.IsDragging,
            "Returning to button does not turn an aimed gesture into a tap");
        gesture.Move(-1, 30f, 0f, 2.03d, true);
        Check(gesture.Release(-1, 30f, 0f, 2.04d, true).Signal == SkillPointerSignal.Cancelled && !gesture.IsPressed,
            "Cancel-zone release cancels instead of casting");
        gesture.Press(1, 0f, 0f, 3d);
        Check(gesture.Release(1, 25f, 0f, 3.01d, false).Signal == SkillPointerSignal.AimReleased,
            "Release rechecks drag threshold even without a move event");
        gesture.Press(1, 0f, 0f, 4d);
        Check(gesture.Tick(3d).Signal == SkillPointerSignal.Cancelled && !gesture.IsPressed, "Regressing pointer clock cancels safely");
        gesture.Press(1, 0f, 0f, 5d);
        Check(gesture.Move(1, float.NaN, 0f, 5.01d, false).Signal == SkillPointerSignal.Cancelled, "Invalid pointer coordinates cancel ownership");
        gesture.Press(1, -float.MaxValue, 0f, 6d);
        var extreme = gesture.Move(1, float.MaxValue, 0f, 6.01d, false);
        Check(extreme.DirectionX == 1f && extreme.Pull == 1f, "Extreme finite screen delta cannot overflow gesture math");
        Check(gesture.Cancel().Signal == SkillPointerSignal.Cancelled && gesture.Cancel().Signal == SkillPointerSignal.None,
            "Cancellation dispatches once");
        var immediate = new SkillPointerGesture(25f, 140f, 0.12d, false);
        Check(immediate.Press(1, 0f, 0f, 0d).AimStarted, "Immediate-aim legacy mode remains supported");
    }

    private static void SessionGestures()
    {
        using (var session = new DefenseSession())
        using (var ability = new DefenseAbilityRuntime(session))
        using (var gesture = new DefenseSkillGestureSession(session, ability, NewGesture()))
        {
            int releases = 0, cancels = 0;
            DefenseSkillGestureSample released = default;
            gesture.Changed += sample =>
            {
                if (sample.Pointer.Signal == SkillPointerSignal.TapReleased || sample.Pointer.Signal == SkillPointerSignal.AimReleased)
                { releases++; released = sample; }
                if (sample.Pointer.Signal == SkillPointerSignal.Cancelled) cancels++;
            };
            session.Start();
            gesture.Press(1, 0f, 0f, 0d);
            Check(!gesture.IsPressed, "Unbound availability fails closed");
            gesture.SetAvailable(true);
            gesture.Press(1, 0f, 0f, 1d);
            Check(ability.Phase == DefenseAbilityPhase.Aiming && gesture.IsPressed, "Pointer reserves one ability aim owner");
            using (var other = new DefenseSkillGestureSession(session, ability, NewGesture()))
            {
                other.SetAvailable(true);
                other.Press(2, 0f, 0f, 1d);
                other.Cancel();
                Check(!other.IsPressed && ability.Phase == DefenseAbilityPhase.Aiming, "Second control cannot steal or cancel reserved ability");
            }
            session.Pause();
            session.Resume();
            gesture.Release(1, 0f, 0f, 1.1d, false);
            Check(cancels == 1 && releases == 0 && ability.RemainingCooldown == 0f, "Pause cancels pointer and stale release without cooldown");
            gesture.Press(1, 0f, 0f, 2d);
            gesture.Release(1, 0f, 0f, 2.01d, false);
            Check(releases == 1 && released.RunGeneration == session.RunGeneration && ReferenceEquals(released.SessionIdentity, session),
                "Release carries exact session and generation scope");
            Check(ability.RemainingCooldown == 0f && ability.TryActivate(2f, true), "Gesture only requests cast; executor owns cooldown commit");
            gesture.Press(1, 0f, 0f, 3d);
            Check(!gesture.IsPressed, "Cooldown blocks new pointer gesture");
            session.Restart();
            gesture.Press(1, 0f, 0f, 4d);
            gesture.SetAvailable(false);
            Check(!gesture.IsPressed && ability.Phase == DefenseAbilityPhase.Ready, "Down or focus loss cancels reserved aim");
            gesture.SetAvailable(true);
            gesture.Press(1, 0f, 0f, 5d);
            session.Restart();
            gesture.Release(1, 0f, 0f, 5.01d, false);
            Check(releases == 1 && !gesture.IsPressed, "Restart cannot deliver old pointer release");
            gesture.Press(1, 0f, 0f, 6d);
            session.Tick(0f, 0f, false);
            Check(!gesture.IsPressed && !ability.TryActivate(1f, true), "Terminal state cancels aim and closes cast gate");
        }
        using (var first = new DefenseSession())
        using (var second = new DefenseSession())
        using (var ability = new DefenseAbilityRuntime(first))
        {
            bool rejected = false;
            try { new DefenseSkillGestureSession(second, ability, NewGesture()); } catch (ArgumentException) { rejected = true; }
            Check(rejected, "Gesture rejects ability from another session");
        }
    }

    private static void Aim()
    {
        var origin = new DefensePoint3(10f, 7f, 20f);
        Check(DefenseAimResolver.TryResolve(origin, new DefensePoint3(16f, 99f, 28f), 5f, out var aim) &&
            Close(aim.Target.X, 13f) && Close(aim.Target.Z, 24f) && aim.Target.Y == 7f && Close(aim.Distance, 5f),
            "Resolved aim clamps XZ range while preserving origin plane height");
        Check(Close(aim.Direction.X, 0.6f) && Close(aim.Direction.Z, 0.8f) && aim.Direction.Y == 0f, "Direction is normalized in XZ");
        Check(DefenseAimResolver.TryResolve(origin, origin, 0f, out aim) && aim.Distance == 0f && aim.Target.X == 10f,
            "Zero-range self aim is valid");
        Check(!DefenseAimResolver.TryResolve(origin, origin, float.NaN, out aim) &&
            !DefenseAimResolver.TryResolve(origin, origin, -1f, out aim), "Invalid range fails closed");
        Check(!DefenseAimResolver.TryResolve(origin, new DefensePoint3(float.PositiveInfinity, 0f, 0f), 5f, out aim),
            "Non-finite target is rejected");
        var right = new DefensePoint3(0f, 0f, -1f);
        var up = new DefensePoint3(1f, 0f, 0f);
        Check(DefenseAimResolver.TryResolveDrag(origin, right, up, 1f, 0f, 0.5f, 10f, up, out aim) &&
            Close(aim.Target.X, 10f) && Close(aim.Target.Z, 15f), "Camera-relative right drag rotates into world XZ");
        Check(DefenseAimResolver.TryResolveDrag(origin, right, up, 0f, 1f, 2f, 10f, up, out aim) &&
            Close(aim.Target.X, 20f) && Close(aim.Target.Z, 20f), "Pull beyond full range is clamped");
        Check(DefenseAimResolver.TryResolveDrag(origin, right, up, 0f, 0f, 1f, 10f, right, out aim) && aim.Target.Z == 10f,
            "Zero drag can resolve explicit actor-facing fallback");
        Check(!DefenseAimResolver.TryResolveDrag(origin, right, right, 1f, 0f, 1f, 10f, up, out aim),
            "Collapsed camera basis is rejected");
        Check(DefenseAimResolver.TryResolve(new DefensePoint3(-float.MaxValue, 0f, 0f),
            new DefensePoint3(float.MaxValue, 0f, 0f), 10f, out aim) && aim.Target.IsFinite && aim.Direction.X == 1f,
            "Extreme finite world delta does not overflow normalization");
    }
}
