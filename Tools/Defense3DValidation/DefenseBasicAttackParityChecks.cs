using System;
using ProjectZombie.Features.Defense3D;

public static class DefenseBasicAttackParityChecks
{
    private static int _checks;
    private static void Check(bool value, string description)
    {
        if (!value) throw new Exception(description);
        _checks++;
        Console.WriteLine("PASS " + description);
    }
    public static void Main()
    {
        Check(DefenseBasicAttackRules.Windup == .18f && DefenseBasicAttackRules.RepeatInterval == .25f && DefenseBasicAttackRules.Recovery == .85f,
            "Basic hit, repeat gate and uncompressed pose timing match PlayerCombatController");
        Check(Math.Abs(Math.Cos(DefenseBasicAttackRules.ConeAngle * Math.PI / 360d) + .26d) < .000001d,
            "Basic sector reproduces source dot threshold -0.26");
        var origin = new DefensePoint3(0f, 0f, 0f);
        var facing = new DefensePoint3(0f, 0f, 1f);
        var cone = new DefenseConeSnapshot(origin, facing, 2.05f, DefenseBasicAttackRules.ConeAngle);
        Check(cone.Contains(new DefensePoint3((float)Math.Sqrt(1d - .26d * .26d), 0f, -.26f)), "Side boundary remains within the source broad arc");
        Check(!cone.Contains(new DefensePoint3((float)Math.Sqrt(1d - .27d * .27d), 0f, -.27f)), "Rear target beyond the source threshold is excluded");
        Check(DefenseBasicAttackRules.TargetScore(origin, facing, new DefensePoint3(0f, 0f, 1f)) <
            DefenseBasicAttackRules.TargetScore(origin, facing, new DefensePoint3(0f, 0f, -.9f)), "Facing preference can outweigh a slightly closer rear enemy");
        Check(DefenseBasicAttackRules.SearchRange(2.05f) == 4.5f && Math.Abs(DefenseBasicAttackRules.SearchRange(3f) - 5.4f) < .00001f,
            "Auto-facing searches beyond damage radius without extending hit range");
        var a = new DefensePoint3(0f, .2f, 0f); var b = new DefensePoint3(1f, .2f, 0f); var c = new DefensePoint3(0f, .2f, 1f);
        Check(DefenseBasicAttackRules.TryTriangleHeight(new DefensePoint3(.25f, 100f, .25f), a, b, c, out float y) && Math.Abs(y - .2f) < .00001f,
            "Visible ground height comes from the terrain triangle, not the actor center or world zero");
        Check(DefenseBasicAttackRules.TryTriangleHeight(new DefensePoint3(.5f, 2f, .25f), origin, new DefensePoint3(1f, 1f, 0f), new DefensePoint3(0f, 0f, 1f), out y)
            && Math.Abs(y - .5f) < .00001f, "Sloped terrain uses interpolated height instead of mesh bounding-box top");
        Check(!DefenseBasicAttackRules.TryTriangleHeight(new DefensePoint3(2f, 0f, 2f), a, b, c, out y), "A distant terrain patch cannot raise the local indicator");
        Check(DefenseBasicAttackRules.TryTriangleHeight(new DefensePoint3(1f, 0f, 0f), a, b, c, out y), "Triangle edges remain valid surface samples");
        Check(!DefenseBasicAttackRules.TryTriangleHeight(origin, origin, new DefensePoint3(0f, 1f, 0f), new DefensePoint3(0f, 0f, 1f), out y),
            "Vertical geometry cannot masquerade as ground");
        using (var session = new DefenseSession())
        {
            session.Start();
            var windup = new DefenseConeTelegraphRuntime(session, DefenseBasicAttackRules.Windup, 2.05f, DefenseBasicAttackRules.ConeAngle);
            windup.TryBegin(origin, facing); session.Tick(.17f, 1000f, false);
            bool early = windup.TryResolve(out _);
            session.Tick(.02f, 1000f, false);
            Check(!early && windup.TryResolve(out _) && !windup.TryResolve(out _), "A basic strike resolves once after windup, never on pointer release");
        }
        Console.WriteLine("TOTAL " + _checks + " PASS (source basic attack geometry/timing and ground math; no Unity rendering or pointer QA)");
    }
}
