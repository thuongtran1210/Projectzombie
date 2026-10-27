using System;
using ProjectZombie.Features.Defense3D;

public static class DefenseCombatChecks
{
    private static int _checks;
    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception(name);
        _checks++;
        Console.WriteLine("PASS " + name);
    }
    private static bool Close(float a, float b) => Math.Abs(a - b) < 0.001f;
    private static DefenseUpgradeCard Card(DefenseUpgradeType type, float value) => new DefenseUpgradeCard("check", "check", "check", type, value);

    public static void Main()
    {
        CheckHealth();
        CheckDamageScope();
        CheckTargets();
        CheckModifiers();
        var valid = new DefenseStatusRequest(DefenseStatusKind.Slow, 2f, 0.4f, new DefensePoint3(1f, 0f, 0f));
        Check(valid.IsValid && !new DefenseStatusRequest(DefenseStatusKind.Stun, float.NaN, 0f, default).IsValid &&
            !new DefenseStatusRequest((DefenseStatusKind)999, 2f, 0f, default).IsValid, "Status boundary validates finite duration, strength and effect kind");
        Console.WriteLine("TOTAL " + _checks + " PASS (standalone combat core, no Unity runtime)");
    }

    private static void CheckHealth()
    {
        var health = new DefenseHealthCore(100f);
        Check(!health.ApplyDamage(new DefenseDamageContext(float.NaN)) && !health.ApplyDamage(new DefenseDamageContext(float.PositiveInfinity)) &&
            !health.ApplyDamage(new DefenseDamageContext(-1f)) && !health.ApplyDamage(new DefenseDamageContext(0f)) &&
            !health.ApplyDamage(new DefenseDamageContext(1f, hitPoint: new DefensePoint3(float.NaN, 0f, 0f))) && health.CurrentHp == 100f,
            "Health rejects non-finite, non-positive damage and invalid hit points");
        int deaths = 0;
        health.Defeated += () => deaths++;
        health.ApplyDamage(new DefenseDamageContext(200f));
        health.ApplyDamage(new DefenseDamageContext(200f));
        Check(health.CurrentHp == 0f && deaths == 1, "Lethal damage clamps HP and publishes defeat once");
        health.Reset();
        health.ApplyDamage(new DefenseDamageContext(100f));
        Check(deaths == 2, "Reset starts a new health life with one new defeat");

        var reentrant = new DefenseHealthCore(100f);
        int nestedDeaths = 0, updates = 0;
        bool nested = false;
        reentrant.Defeated += () => nestedDeaths++;
        reentrant.HealthChanged += (_, __) => updates++;
        reentrant.Damaged += _ => { if (!nested) { nested = true; reentrant.ApplyDamage(new DefenseDamageContext(100f)); } };
        reentrant.ApplyDamage(new DefenseDamageContext(1f));
        Check(nestedDeaths == 1 && updates == 1 && reentrant.IsDead, "Reentrant damage cannot publish a stale health update or duplicate death");

        var resetDuringDamage = new DefenseHealthCore(100f);
        bool resetOnce = true;
        int obsoleteDeaths = 0;
        resetDuringDamage.Damaged += _ => { if (resetOnce) { resetOnce = false; resetDuringDamage.Reset(); } };
        resetDuringDamage.Defeated += () => obsoleteDeaths++;
        resetDuringDamage.ApplyDamage(new DefenseDamageContext(100f));
        Check(obsoleteDeaths == 0 && resetDuringDamage.CurrentHp == 100f, "Reset during damage suppresses the obsolete life's defeat");
        resetDuringDamage.ApplyDamage(new DefenseDamageContext(100f));
        Check(obsoleteDeaths == 1, "A new life remains damageable after a reentrant reset");

        var capacityDuringDeath = new DefenseHealthCore(100f);
        int capacityDeaths = 0;
        capacityDuringDeath.Damaged += _ => capacityDuringDeath.SetCapacity(200f, true);
        capacityDuringDeath.Defeated += () => capacityDeaths++;
        capacityDuringDeath.ApplyDamage(new DefenseDamageContext(100f));
        Check(capacityDuringDeath.IsDead && capacityDeaths == 1, "Capacity changes cannot revive or suppress a lethal transition");

        var capacity = new DefenseHealthCore(1000f);
        capacity.ApplyDamage(new DefenseDamageContext(100f));
        capacity.SetCapacity(1200f, true);
        Check(capacity.MaxHp == 1200f && capacity.CurrentHp == 1100f, "Base capacity grant preserves missing HP and fills only added capacity");
        capacity.ApplyDamage(new DefenseDamageContext(5000f));
        capacity.SetCapacity(1400f, true);
        Check(capacity.IsDead && capacity.CurrentHp == 0f, "Capacity upgrades do not resurrect dead actors");

        var hero = new DefenseHeroRespawnTracker();
        hero.TakeDamage(200f);
        hero.Tick(0f, true);
        hero.Tick(20f, false);
        Check(hero.IsDown && hero.RemainingRespawnTime == 0f, "Terminal state permanently cancels pending Hero respawn");
        hero.Reset();
        hero.TakeDamage(200f);
        hero.Tick(5f, false);
        Check(!hero.IsDown && hero.CurrentHp == 200f, "Explicit reset restores the next Hero respawn lifecycle");
    }

    private static void CheckDamageScope()
    {
        using (var a = new DefenseSession())
        using (var b = new DefenseSession())
        {
            var request = new DefenseDamageContext(10f, runGeneration: a.RunGeneration, sessionIdentity: a.SessionIdentity);
            Check(!DefenseDamageRules.CanApply(request, a, false), "Unstarted sessions reject damage");
            a.Start(); b.Start();
            Check(DefenseDamageRules.CanApply(request, a, false) && !DefenseDamageRules.CanApply(request, b, false), "Damage accepts its own session and rejects foreign sessions");
            a.Pause();
            Check(!DefenseDamageRules.CanApply(request, a, false), "Paused session rejects direct damage calls");
            a.Resume(); a.OpenLevelUp();
            Check(!DefenseDamageRules.CanApply(request, a, false), "LevelUp rejects direct damage calls");
            a.Resume(); a.Restart();
            Check(!DefenseDamageRules.CanApply(request, a, false), "Previous-run damage is rejected after restart");
            request.RunGeneration = a.RunGeneration;
            Check(DefenseDamageRules.CanApply(request, a, false) && !DefenseDamageRules.CanApply(request, a, true), "Current generation accepts live targets and rejects dead targets");
        }
    }

    private sealed class Target : IDefenseTargetPosition
    {
        public object Identity { get; } = new object();
        public bool IsAvailable { get; set; } = true;
        public bool IsAirborne { get; set; }
        public DefensePoint3 Position { get; set; }
        public Target(float x, float z) { Position = new DefensePoint3(x, 0f, z); }
    }

    private static void CheckTargets()
    {
        using (var a = new DefenseSession())
        using (var b = new DefenseSession())
        {
            var first = new Target(1f, 0f);
            var second = new Target(-1f, 0f);
            var foreign = new Target(0.1f, 0f);
            a.TargetRegistry.Register(first); a.TargetRegistry.Register(second); b.TargetRegistry.Register(foreign);
            Check(ReferenceEquals(a.Targets.FindNearest(default, 5f), first) && ReferenceEquals(b.Targets.FindNearest(default, 5f), foreign), "Target queries are isolated by session");
            Check(!a.TargetRegistry.Register(first) && a.TargetRegistry.Count == 2, "Duplicate target registration is idempotent");
            first.IsAvailable = false;
            Check(ReferenceEquals(a.Targets.FindNearest(default, 5f), second), "Unavailable targets are skipped");
            first.IsAvailable = true;
            Check(ReferenceEquals(a.Targets.FindNearest(default, 5f), first), "Equal-distance targets use stable registration order");
            var air = new Target(0.5f, 0f) { IsAirborne = true };
            a.TargetRegistry.Register(air);
            Check(ReferenceEquals(a.Targets.FindNearest(default, 5f, DefenseTargetMask.Ground), first) &&
                ReferenceEquals(a.Targets.FindNearest(default, 5f, DefenseTargetMask.Air), air), "Ground and air target masks are independent");
            air.Position = new DefensePoint3(float.NaN, 0f, 0f);
            Check(ReferenceEquals(a.Targets.FindNearest(default, 5f), first) && a.Targets.FindNearest(default, float.PositiveInfinity) == null, "Target query rejects invalid coordinates and non-finite ranges");
            a.Start(); a.Restart();
            Check(a.TargetRegistry.Count == 0 && b.TargetRegistry.Count == 1, "Restart clears only the owning session's targets");
        }
        var disposed = new DefenseTargetIndex();
        disposed.Dispose();
        bool rejected = false;
        try { disposed.Register(new Target(0f, 0f)); } catch (ObjectDisposedException) { rejected = true; }
        Check(rejected && disposed.FindNearest(default, 5f) == null, "Disposed target scopes reject registration and return no candidates");
    }

    private static void CheckModifiers()
    {
        using (var session = new DefenseSession())
        {
            var deck = new DefenseUpgradeDeck();
            deck.BindModifiers(session.Modifiers);
            deck.SelectUpgrade(Card(DefenseUpgradeType.HeroAttackDamage, 0.20f));
            Check(Close(session.Stats.Evaluate(DefenseUpgradeType.HeroAttackDamage, 35f), 42f), "Hero damage reads the shared modifier ledger");
            deck.SelectUpgrade(Card(DefenseUpgradeType.HeroAttackSpeed, 0.15f));
            Check(Close(session.Stats.EvaluateHeroAttackInterval(0.65f), 0.65f / 1.15f), "Attack-speed upgrades reduce subsequent attack intervals");
            deck.SelectUpgrade(Card(DefenseUpgradeType.HeroMoveSpeed, 0.10f));
            Check(Close(session.Stats.Evaluate(DefenseUpgradeType.HeroMoveSpeed, 5f), 5.5f), "Movement upgrades evaluate from authored base speed");
            Check(session.Stats.Evaluate(DefenseUpgradeType.HeroMoveSpeed, 0f) == 0f, "Zero authored movement speed remains a valid stationary baseline");
            deck.SelectUpgrade(Card(DefenseUpgradeType.TowerDamage, 0.25f));
            deck.SelectUpgrade(Card(DefenseUpgradeType.TowerAttackRange, 0.20f));
            Check(Close(session.Stats.Evaluate(DefenseUpgradeType.TowerDamage, 22f), 27.5f) && Close(session.Stats.Evaluate(DefenseUpgradeType.TowerAttackRange, 8f), 9.6f), "Tower damage and range consume session modifiers");
            deck.SelectUpgrade(Card(DefenseUpgradeType.BaseMaxHp, 200f));
            Check(session.Stats.Evaluate(DefenseUpgradeType.BaseMaxHp, 1000f) == 1200f, "Base HP uses a flat grant rather than a percentage");
            deck.SelectUpgrade(Card(DefenseUpgradeType.HeroAttackDamage, 0.20f));
            Check(Close(session.Stats.Evaluate(DefenseUpgradeType.HeroAttackDamage, 35f), 49f), "Repeated upgrades stack against the original baseline");
            int changes = 0;
            session.Modifiers.Changed += () => changes++;
            Check(!session.Modifiers.TryApplyCard(Card(DefenseUpgradeType.HeroAttackDamage, float.NaN)) &&
                !session.Modifiers.TryApplyCard(Card(DefenseUpgradeType.HeroAttackDamage, -1f)) &&
                !session.Modifiers.TryApplyCard(Card((DefenseUpgradeType)999, 1f)) && changes == 0,
                "Invalid modifier input cannot poison the ledger or emit changes");
            session.Start(); session.Restart();
            Check(session.Stats.GetModifier(DefenseUpgradeType.HeroAttackDamage) == 0f && session.Stats.Evaluate(DefenseUpgradeType.BaseMaxHp, 1000f) == 1000f,
                "Restart restores session modifiers without rewriting authored baselines");
        }
    }
}
