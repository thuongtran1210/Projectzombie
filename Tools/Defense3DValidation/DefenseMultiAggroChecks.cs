using System;
using ProjectZombie.Features.Defense3D;

public static class DefenseMultiAggroChecks
{
    private sealed class Candidate : IDefenseAggroCandidate
    {
        public object Identity { get; } = new object();
        public bool IsAvailable { get; set; } = true;
        public DefensePoint3 Position { get; set; }
        public DefenseFriendlyKind Kind { get; set; }
        public bool Reachable = true;
    }
    private static int _checks;
    private static void Check(bool value, string name)
    {
        if (!value) throw new Exception(name);
        _checks++; Console.WriteLine("PASS " + name);
    }
    private static bool Reachable(IDefenseAggroCandidate candidate) => ((Candidate)candidate).Reachable;
    public static void Main()
    {
        var rules = new DefenseAggroRules(4.2f, 7f, 0.75f, 0.25f, 3f);
        var policy = new DefenseAggroPolicy(rules);
        var hero = new Candidate { Kind = DefenseFriendlyKind.Hero, Position = new DefensePoint3(3f, 0f, 0f) };
        var support = new Candidate { Kind = DefenseFriendlyKind.Support, Position = new DefensePoint3(4f, 0f, 0f) };
        IDefenseAggroCandidate[] roster = { hero, support };
        DefensePoint3 origin = default;
        Check(ReferenceEquals(policy.Select(0f, origin, roster, false, Reachable), hero), "Ground acquires nearest eligible Hero");
        support.Position = new DefensePoint3(1f, 0f, 0f);
        Check(ReferenceEquals(policy.Select(0.1f, origin, roster, false, Reachable), hero), "Closer Support does not steal an active focus");
        hero.Position = new DefensePoint3(8f, 0f, 0f);
        Check(ReferenceEquals(policy.Select(0.2f, origin, roster, false, Reachable), hero), "Short target lock prevents immediate switchback at retain boundary");
        Check(ReferenceEquals(policy.Select(1f, origin, roster, false, Reachable), support), "Expired lock releases distant Hero to nearby Support");
        support.IsAvailable = false;
        Check(policy.Select(1.1f, origin, roster, false, Reachable) == null, "Dead Support releases immediately despite active lock and falls back to Base");
        hero.Position = new DefensePoint3(3f, 0f, 0f);
        Check(ReferenceEquals(policy.Select(1.2f, origin, roster, false, Reachable), hero), "Available Hero can replace a lost Support");
        hero.Reachable = false;
        Check(policy.Select(1.3f, origin, roster, false, Reachable) == null, "Opposite-bank or lost-path target cannot retain a lock");
        hero.Reachable = true; hero.IsAvailable = false;
        Check(policy.Select(2f, origin, roster, true, Reachable) == null, "Down or inactive Hero cannot be acquired by Air");
        hero.IsAvailable = true; hero.Position = new DefensePoint3(5f, 0f, 0f);
        support.IsAvailable = true; support.Position = new DefensePoint3(6f, 0f, 0f);
        Check(policy.Select(3f, origin, roster, false, Reachable) == null, "Unprovoked targets beyond acquire range keep Base focus");
        Check(ReferenceEquals(policy.Select(3.1f, origin, roster, false, Reachable, support.Identity), support), "Only the actual Support damage source gains extended provoked acquisition");
        support.Position = new DefensePoint3(7f, 0f, 0f);
        Check(ReferenceEquals(policy.Select(5f, origin, roster, false, Reachable), support), "Acquired target remains through exact retain boundary");
        support.Position = new DefensePoint3(7.01f, 0f, 0f);
        Check(policy.Select(6f, origin, roster, false, Reachable) == null, "Target leaving retain range returns enemy to Base");
        policy.Reset();
        Check(ReferenceEquals(policy.Select(7f, origin, roster, false, Reachable, hero.Identity), hero), "Hero provocation can acquire within extended range");
        policy.Reset();
        Check(policy.Select(7f, origin, roster, false, Reachable) == null, "Expired provocation cannot start an out-of-range pursuit");
        hero.Position = new DefensePoint3(3f, 0f, 0f); support.Position = new DefensePoint3(2f, 100f, 0f);
        Check(ReferenceEquals(policy.Select(8f, origin, roster, false, Reachable), support), "Ground selection keeps XZ semantics");
        policy.Reset();
        Check(ReferenceEquals(policy.Select(8f, origin, roster, true, Reachable), hero), "Air uses XYZ and rejects vertically distant Support");
        hero.Reachable = false; support.Position = new DefensePoint3(4f, 0f, 0f); policy.Reset();
        Check(ReferenceEquals(policy.Select(9f, origin, roster, true, Reachable), support), "Blocked nearest candidate cannot starve reachable Support");
        hero.Reachable = true; support.Position = hero.Position; policy.Reset();
        Check(ReferenceEquals(policy.Select(10f, origin, roster, true, Reachable), hero), "Equal-distance acquisition preserves authored roster order");
        Check(ReferenceEquals(policy.Select(10.1f, origin, new IDefenseAggroCandidate[] { support }, true, Reachable), support), "Removed roster member loses focus immediately");
        policy.Reset();
        Check(policy.Select(11f, origin, Array.Empty<IDefenseAggroCandidate>(), false, Reachable) == null, "Empty roster falls back to Base");
        Check(policy.Select(float.NaN, origin, roster, false, Reachable) == null && policy.Focus == null, "Invalid clock clears focus");
        Check(policy.Select(12f, new DefensePoint3(float.NaN, 0f, 0f), roster, false, Reachable) == null, "Invalid actor position fails closed");
        Check(policy.Select(12f, origin, roster, false, null) == null, "Missing reachability strategy cannot acquire targets");
        hero.Kind = (DefenseFriendlyKind)99; support.IsAvailable = false;
        Check(policy.Select(13f, origin, roster, false, Reachable) == null, "Unknown friendly category cannot be targeted");
        hero.Kind = DefenseFriendlyKind.Hero; hero.Position = new DefensePoint3(float.PositiveInfinity, 0f, 0f);
        Check(policy.Select(13f, origin, roster, false, Reachable) == null, "Invalid candidate geometry is rejected");
        bool rejected = false;
        try { new DefenseAggroRules(7f, 4f, 0.75f, 0.25f, 3f); } catch (ArgumentException) { rejected = true; }
        Check(rejected, "Shared profile rejects inverted acquire and retain ranges");
        rejected = false;
        try { new DefenseAggroRules(4.2f, 7f, 0.75f, 0f, 3f); } catch (ArgumentException) { rejected = true; }
        Check(rejected, "Shared profile rejects zero think interval");
        Console.WriteLine("TOTAL " + _checks + " PASS (standalone multi-domain aggro policy, no Unity runtime)");
    }
}
