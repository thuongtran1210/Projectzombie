using System;
using ProjectZombie.Features.Defense3D;

public static class DefenseAirTargetChecks
{
    private static int _checks;
    private sealed class Target : IDefenseTargetPosition
    {
        public object Identity => this;
        public bool IsAvailable { get; set; } = true;
        public bool IsAirborne { get; set; }
        public DefensePoint3 Position { get; set; }
    }
    private static void Check(bool value, string name)
    {
        if (!value) throw new Exception(name);
        _checks++;
        Console.WriteLine("PASS " + name);
    }
    public static void Main()
    {
        var origin = new DefensePoint3(0f, 0f, 0f);
        Check(!DefenseSpatialRules.InRange(origin, new DefensePoint3(0f, 6f, 0f), 5f, true), "Air above weapon range is rejected even at identical XZ");
        Check(DefenseSpatialRules.InRange(origin, new DefensePoint3(3f, 4f, 0f), 5f, true), "Exact XYZ range boundary is included");
        Check(DefenseSpatialRules.InRange(origin, new DefensePoint3(3f, 100f, 0f), 5f, false), "Ground keeps existing planar range behavior");
        Check(!DefenseSpatialRules.InRange(origin, new DefensePoint3(float.NaN, 0f, 0f), 5f, true) &&
            !DefenseSpatialRules.InRange(origin, origin, float.PositiveInfinity, true), "Non-finite spatial inputs fail closed");
        Check(!DefenseSpatialRules.InRange(new DefensePoint3(float.MaxValue, 0f, 0f), new DefensePoint3(-float.MaxValue, 0f, 0f), float.MaxValue, true), "Extreme finite coordinates cannot overflow into a false hit");
        Check(DefenseSpatialRules.WithinHeight(1.4f, 1.4f, 4f) && DefenseSpatialRules.WithinHeight(4f, 1.4f, 4f), "Flight includes exact floor and ceiling");
        Check(!DefenseSpatialRules.WithinHeight(1.39f, 1.4f, 4f) && !DefenseSpatialRules.WithinHeight(4.01f, 1.4f, 4f), "Flight cannot cross altitude limits");
        Check(!DefenseSpatialRules.WithinHeight(2f, 4f, 1f) && !DefenseSpatialRules.WithinHeight(float.NaN, 1f, 4f), "Invalid flight bounds and heights fail closed");
        var index = new DefenseTargetIndex();
        var high = new Target { IsAirborne = true, Position = new DefensePoint3(0f, 20f, 0f) };
        var blocked = new Target { IsAirborne = true, Position = new DefensePoint3(1f, 0f, 0f) };
        var visible = new Target { IsAirborne = true, Position = new DefensePoint3(2f, 1f, 0f) };
        var ground = new Target { IsAirborne = false, Position = new DefensePoint3(3f, 0f, 0f) };
        index.Register(high); index.Register(blocked); index.Register(visible); index.Register(ground);
        Check(ReferenceEquals(index.FindNearestSpatial(origin, 5f, DefenseTargetMask.Air, t => !ReferenceEquals(t, blocked)), visible), "High and occluded Air candidates cannot starve a visible Air target");
        Check(ReferenceEquals(index.FindNearestSpatial(origin, 5f, DefenseTargetMask.Ground, null), ground), "Ground-only weapon cannot select Air");
        Check(index.FindNearestSpatial(origin, 5f, DefenseTargetMask.Air, t => false) == null, "All occluded targets produce no shot candidate");
        Check(index.FindNearestSpatial(origin, 5f, (DefenseTargetMask)8, null) == null &&
            index.FindNearestSpatial(origin, 5f, 0, null) == null, "Invalid capability masks fail closed");
        blocked.IsAvailable = false;
        Check(ReferenceEquals(index.FindNearestSpatial(origin, 5f, DefenseTargetMask.All, null), visible), "Unavailable Air target is skipped");
        var tie = new Target { IsAirborne = true, Position = visible.Position };
        index.Register(tie);
        Check(ReferenceEquals(index.FindNearestSpatial(origin, 5f, DefenseTargetMask.Air, null), visible), "Equal-distance spatial targets preserve registration order");
        index.Clear();
        Check(index.FindNearestSpatial(origin, 5f, DefenseTargetMask.All, null) == null, "Round cleanup clears spatial candidates");
        index.Dispose();
        Check(index.FindNearestSpatial(origin, 5f, DefenseTargetMask.All, null) == null, "Disposed session cannot return spatial candidates");
        Console.WriteLine("TOTAL " + _checks + " PASS (standalone Air geometry/target policy, no Unity runtime)");
    }
}
