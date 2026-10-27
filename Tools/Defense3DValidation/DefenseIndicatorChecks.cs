using System;
using ProjectZombie.Features.Defense3D;

public static class DefenseIndicatorChecks
{
    private static int _checks;
    private static void Check(bool value, string message)
    {
        if (!value) throw new Exception(message);
        _checks++;
        Console.WriteLine("PASS " + message);
    }
    private static bool Near(float a, float b) => Math.Abs(a - b) < .01f;
    private static DefenseIndicatorProjection Project(float x, float y, float z = 1f)
    {
        if (!DefenseIndicatorProjection.TryProject(new DefensePoint3(x, y, z), 0f, 0f, 800f, 600f, 38f, out var p)) throw new Exception("projection");
        return p;
    }
    public static void Main()
    {
        var center = Project(.5f, .5f);
        Check(!center.IsOffscreen && Near(center.X, 400f) && Near(center.Y, 300f), "Visible target projects at camera center");
        var right = Project(2f, .5f);
        Check(right.IsOffscreen && Near(right.X, 762f) && Near(right.Y, 300f) && Near(right.Angle, 0f), "Right target clamps with right-facing arrow");
        var left = Project(-1f, .5f);
        Check(left.IsOffscreen && Near(left.X, 38f) && Near(Math.Abs(left.Angle), 180f), "Left target keeps safe margin and left bearing");
        var top = Project(.5f, 2f);
        Check(top.IsOffscreen && Near(top.Y, 562f) && Near(top.Angle, 90f), "Upper target points upward in Unity pixel space");
        var bottom = Project(.5f, -1f);
        Check(bottom.IsOffscreen && Near(bottom.Y, 38f) && Near(bottom.Angle, -90f), "Lower target points downward");
        var diagonal = Project(2f, 2f);
        Check(diagonal.IsOffscreen && diagonal.X <= 762f && diagonal.Y <= 562f && diagonal.Angle > 0f, "Diagonal ray intersects safe rectangle");
        var behind = Project(.2f, .5f, -1f);
        Check(behind.IsOffscreen && Near(behind.X, 762f), "Behind-camera mirrored projection restores target bearing");
        var axial = Project(.5f, .5f, -1f);
        Check(axial.IsOffscreen && Near(axial.Y, 38f), "Direct rear target has deterministic finite fallback");
        Check(Project(.96f, .5f).IsOffscreen && !Project(.94f, .5f).IsOffscreen, "Vongxuyen comfortable viewport threshold retained");
        Check(Project(.5f, .95f).IsOffscreen, "Vertical comfortable threshold retained");
        Check(DefenseIndicatorProjection.TryProject(new DefensePoint3(.5f, .5f, 1f), 100f, 50f, 600f, 400f, 38f, out var inset) &&
            Near(inset.X, 400f) && Near(inset.Y, 250f), "Camera rectangle offset respected");
        Check(DefenseIndicatorProjection.TryProject(new DefensePoint3(2f, .5f, 1f), 80f, 20f, 600f, 400f, 38f, out inset) &&
            Near(inset.X, 642f), "Device safe-area intersection retains inset margin");
        Check(!DefenseIndicatorProjection.TryProject(new DefensePoint3(float.NaN, 0f, 1f), 0f, 0f, 800f, 600f, 38f, out _), "Non-finite projection rejected");
        Check(!DefenseIndicatorProjection.TryProject(default, 0f, 0f, 60f, 60f, 38f, out _), "Viewport smaller than badge margin rejected");
        Check(!DefenseIndicatorProjection.TryProject(default, 0f, 0f, 800f, 600f, -1f, out _), "Negative screen margin rejected");
        Check(DefenseIndicatorProjection.TryProject(new DefensePoint3(float.MaxValue, float.MaxValue, 1f), 0f, 0f, 800f, 600f, 38f, out inset) &&
            inset.X <= 762f && inset.Y <= 562f, "Extreme finite target cannot overflow projection");
        var slots = new DefensePoint3[32];
        Check(DefenseIndicatorProjection.TryPlace(right, 0f, 0f, 800f, 600f, 38f, 46f, slots, 0, out slots[0]), "First badge keeps its projected position");
        Check(DefenseIndicatorProjection.TryPlace(right, 0f, 0f, 800f, 600f, 38f, 46f, slots, 1, out slots[1]) && Near(slots[1].Y, 346f), "Second right-edge badge separates vertically");
        Check(DefenseIndicatorProjection.TryPlace(right, 0f, 0f, 800f, 600f, 38f, 46f, slots, 2, out slots[2]) && Near(slots[2].Y, 254f), "Third edge badge uses stable opposite offset");
        Check(DefenseIndicatorProjection.TryPlace(top, 0f, 0f, 800f, 600f, 38f, 46f, slots, 3, out slots[3]) && Near(slots[3].Y, 562f), "Top-edge badges remain on top edge");
        Check(!DefenseIndicatorProjection.TryPlace(center, 0f, 0f, 800f, 600f, 38f, 46f, slots, 33, out _), "Pool count cannot exceed authored buffer");
        Check(!DefenseIndicatorProjection.TryPlace(new DefenseIndicatorProjection(float.NaN, 0f, 0f, false), 0f, 0f, 800f, 600f, 38f, 46f, slots, 0, out _), "Invalid placement geometry rejected");
        Check(DefenseIndicatorProjection.WarningFraction(12f, 18f, 5f) == 0f, "Warning remains hidden before lead window");
        Check(Near(DefenseIndicatorProjection.WarningFraction(13f, 18f, 5f), 1f), "Warning begins five seconds before scheduled spawn");
        Check(Near(DefenseIndicatorProjection.WarningFraction(15.5f, 18f, 5f), .5f), "Countdown fraction uses remaining gameplay time");
        Check(DefenseIndicatorProjection.WarningFraction(18f, 18f, 5f) == 0f, "Warning expires at scheduled spawn boundary");
        Check(DefenseIndicatorProjection.WarningFraction(25f, 18f, 5f) == 0f, "Long frame cannot replay expired warning");
        Check(DefenseIndicatorProjection.WarningFraction(0f, 0f, 5f) == 0f, "Time-zero spawn is not falsely reported as upcoming");
        Check(Near(DefenseIndicatorProjection.WarningFraction(0f, 2f, 5f), .4f), "Early event gets honest shortened countdown");
        Check(DefenseIndicatorProjection.WarningFraction(float.NaN, 18f, 5f) == 0f && DefenseIndicatorProjection.WarningFraction(1f, 18f, 0f) == 0f,
            "Invalid warning times fail closed");
        using (var session = new DefenseSession())
        {
            session.Start(); session.Tick(14f, 1000f, false);
            float fraction = DefenseIndicatorProjection.WarningFraction(session.ElapsedSeconds, 18f, 5f);
            session.Pause(); session.Tick(20f, 1000f, false);
            Check(Near(DefenseIndicatorProjection.WarningFraction(session.ElapsedSeconds, 18f, 5f), fraction), "Pause freezes warning countdown");
            session.Resume(); session.Tick(4f, 1000f, false);
            Check(DefenseIndicatorProjection.WarningFraction(session.ElapsedSeconds, 18f, 5f) == 0f, "Resume reaches same authored spawn boundary");
            session.Restart();
            Check(DefenseIndicatorProjection.WarningFraction(session.ElapsedSeconds, 18f, 5f) == 0f, "Restart cannot retain previous wave warning");
        }
        Console.WriteLine("TOTAL " + _checks + " PASS (standalone indicator projection/countdown; no Unity runtime)");
    }
}
