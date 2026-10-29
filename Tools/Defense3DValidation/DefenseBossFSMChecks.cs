using System;
using System.IO;
using System.Globalization;
using ProjectZombie.Features.Defense3D;

public static class DefenseBossFSMChecks
{
    private static int _checks;
    private static void Check(bool value, string name)
    {
        if (!value) throw new Exception(name);
        _checks++; Console.WriteLine("PASS " + name);
    }
    private static float ReadFloat(string path, string field)
    {
        foreach (var line in File.ReadAllLines(path))
        {
            string text = line.Trim();
            if (text.StartsWith(field + ":", StringComparison.Ordinal)) return float.Parse(text.Substring(field.Length + 1), CultureInfo.InvariantCulture);
        }
        throw new Exception("Missing authored field " + field);
    }
    private static DefenseYakshaBehaviorFSM Native(DefenseHealthCore health, float duration = 45f) =>
        new DefenseYakshaBehaviorFSM(health, duration, new DefenseYakshaArenaCycle(3f, 1.1f, 30f));
    public static void Main()
    {
        string stage = "Assets/Resources/Defense3D/Stages/Stage01_Production.asset";
        string prefab = "Assets/Resources/Defense3D/DefenseYaksha3D.prefab";
        Check(ReadFloat(stage, "bossHealth") == 1200f && ReadFloat(stage, "arenaDuration") == 45f, "Real Stage retains 1200 Boss HP and 45-second arena");
        Check(ReadFloat(prefab, "blastInterval") == 3f && ReadFloat(prefab, "warningDuration") == 1.1f && ReadFloat(prefab, "blastDamage") == 30f,
            "Real prefab keeps 3s cadence, 1.1s warning and 30 damage rather than orphan FSM defaults");
        var health = new DefenseHealthCore(1200f);
        using (var fsm = Native(health))
        {
            Check(ReferenceEquals(fsm.Health, health), "FSM shares exact actor health object instead of creating a second HP pool");
            Check(fsm.Phase == DefenseYakshaPhase.Dormant && !fsm.HasSpawned, "Boss remains dormant until owned acquisition");
            int warnings = 0, hits = 0, stops = 0, deaths = 0; float damage = 0f;
            fsm.AttackTelegraphed += type => warnings++;
            fsm.AttackExecuted += (type, value) => { hits++; damage = value; };
            fsm.CombatStopped += () => stops++;
            fsm.PhaseChanged += phase => { if (phase == DefenseYakshaPhase.Defeated) deaths++; };
            fsm.SpawnInArena(); fsm.SpawnInArena();
            Check(fsm.HasSpawned && fsm.Phase == DefenseYakshaPhase.ArenaCombat, "Spawn enters Arena once without replacing health");
            health.ApplyDamage(new DefenseDamageContext(200f));
            Check(fsm.Health.CurrentHp == 1000f, "Actor damage immediately updates FSM health read model");
            Check(!fsm.TryStartTelegraph(DefenseYakshaAttackTelegraph.TelegraphSlam), "Production cycle cannot bypass first-attack cooldown");
            fsm.Tick(2.9f, false);
            Check(warnings == 0 && hits == 0, "No warning or hit before authored first interval");
            fsm.Tick(0.1f, false);
            Check(warnings == 1 && hits == 0 && Math.Abs(fsm.TelegraphTimerRemaining - 1.1f) < 0.0001f,
                "Cadence begins full authored warning without instant damage");
            float remaining = fsm.ArenaTimerRemaining;
            fsm.Tick(100f, true);
            Check(hits == 0 && fsm.ArenaTimerRemaining == remaining && fsm.TelegraphTimerRemaining == 1.1f, "Pause freezes arena and telegraph on the same FSM clock");
            fsm.Tick(1.1f, false);
            Check(hits == 1 && damage == 30f && fsm.ActiveTelegraph == DefenseYakshaAttackTelegraph.None, "FSM executes one prototype-strength blast after full warning");
            fsm.Tick(0f, false);
            Check(hits == 1, "Repeated same-time tick cannot duplicate detonation");
            Check(!fsm.TryStartTelegraph(DefenseYakshaAttackTelegraph.TelegraphSlam), "Recovery cannot be bypassed by manual trigger");
            fsm.Tick(1.8f, false);
            Check(warnings == 1, "Recovery keeps warning hidden before next cadence");
            fsm.Tick(0.1f, false);
            Check(warnings == 2 && hits == 1, "Next warning uses existing interval-minus-windup recovery");
            fsm.StopCombat(); fsm.StopCombat(); fsm.Tick(100f, false);
            Check(fsm.IsStopped && stops == 1 && hits == 1 && fsm.ActiveTelegraph == DefenseYakshaAttackTelegraph.None, "Stop cancels pending damage once and prevents further attacks");
            Check(health.CurrentHp == 1000f && fsm.Phase != DefenseYakshaPhase.Defeated, "Stopping or external removal is not a kill and does not reset HP");
            health.ApplyDamage(new DefenseDamageContext(1000f));
            Check(fsm.Phase == DefenseYakshaPhase.Defeated && deaths == 1 && stops == 1, "Real health death remains authoritative even after combat was stopped");
            health.ApplyDamage(new DefenseDamageContext(1000f)); fsm.Tick(100f, false);
            Check(deaths == 1 && hits == 1, "Duplicate death and ticks cannot publish another defeat or attack");
        }
        using (var legacy = new DefenseYakshaBehaviorFSM(3200f, 45f))
        {
            legacy.SpawnInArena(); legacy.Health.ApplyDamage(new DefenseDamageContext(1000f));
            int hits = 0; float damage = 0f;
            legacy.AttackExecuted += (type, amount) => { hits++; damage = amount; };
            Check(!legacy.TryStartTelegraph(DefenseYakshaAttackTelegraph.None) && !legacy.TryStartTelegraph(DefenseYakshaAttackTelegraph.TelegraphCrescent),
                "Unsupported/None intents fail closed instead of masquerading as Cleave");
            legacy.TryStartTelegraph(DefenseYakshaAttackTelegraph.TelegraphSlam); legacy.Tick(1.2f, false); legacy.Tick(43.8f, false);
            Check(hits == 1 && damage == DefenseYakshaBehaviorFSM.SlamDamage, "Standalone legacy manual FSM contract retains its own test defaults");
            Check(legacy.Phase == DefenseYakshaPhase.AssaultBase && legacy.Health.CurrentHp == 2200f && legacy.HasEnteredAssault,
                "Legacy float delta boundary enters Assault without healing damaged Boss");
            legacy.Health.ApplyDamage(new DefenseDamageContext(2200f));
            Check(legacy.HasEnteredAssault && legacy.Phase == DefenseYakshaPhase.Defeated, "Historical assault read model remains true after defeat");
        }
        using (var cutoff = Native(new DefenseHealthCore(1200f), 4.1f))
        {
            int hits = 0; cutoff.AttackExecuted += (type, amount) => hits++;
            cutoff.SpawnInArena(); cutoff.Tick(3f, false); cutoff.Tick(1.1f, false);
            Check(cutoff.Phase == DefenseYakshaPhase.AssaultBase && hits == 0 && cutoff.ActiveTelegraph == DefenseYakshaAttackTelegraph.None,
                "Arena cutoff takes priority over a detonation due on the same tick");
        }
        using (var skipped = Native(new DefenseHealthCore(1200f)))
        {
            int hits = 0; skipped.AttackExecuted += (type, amount) => hits++;
            skipped.SpawnInArena(); skipped.Tick(50f, false);
            Check(skipped.Phase == DefenseYakshaPhase.AssaultBase && hits == 0, "Long frame skipping arena cannot synthesize unseen blast hits");
        }
        using (var hitch = Native(new DefenseHealthCore(1200f), 100f))
        {
            int hits = 0; hitch.AttackExecuted += (type, amount) => hits++;
            hitch.SpawnInArena(); hitch.Tick(20f, false);
            Check(hitch.ActiveTelegraph != DefenseYakshaAttackTelegraph.None && hitch.TelegraphTimerRemaining == 1.1f && hits == 0,
                "Overdue first interval starts a fresh warning rather than consuming hidden windup");
            hitch.Tick(20f, false);
            Check(hits == 1, "Long pending-warning tick resolves at most one blast");
        }
        var retiredHealth = new DefenseHealthCore(1200f);
        var retired = Native(retiredHealth); retired.SpawnInArena(); retired.Dispose(); retired.Dispose();
        Check(retiredHealth.CurrentHp == 1200f, "FSM disposal does not own or destroy actor health");
        retiredHealth.ApplyDamage(new DefenseDamageContext(1200f));
        Check(retired.Phase != DefenseYakshaPhase.Defeated, "Disposed round listener cannot observe later health death");
        using (var deathInWarning = Native(new DefenseHealthCore(1200f)))
        {
            int hits = 0; deathInWarning.AttackExecuted += (type, amount) => hits++;
            deathInWarning.AttackTelegraphed += type => deathInWarning.Health.ApplyDamage(new DefenseDamageContext(1200f));
            deathInWarning.SpawnInArena(); deathInWarning.Tick(3f, false); deathInWarning.Tick(10f, false);
            Check(deathInWarning.Phase == DefenseYakshaPhase.Defeated && hits == 0, "Reentrant death while warning starts cannot leave delayed damage");
        }
        using (var session = new DefenseSession())
        using (var boss = Native(new DefenseHealthCore(1200f)))
        {
            session.Start(); boss.SpawnInArena(); session.Tick(520f, 1000f, false);
            boss.Health.ApplyDamage(new DefenseDamageContext(1200f)); session.Tick(0f, 1000f, boss.Phase == DefenseYakshaPhase.Defeated);
            Check(session.Phase == DefenseGamePhase.Playing, "FSM-confirmed early Boss kill cannot win before 600 seconds");
            session.Tick(80f, 1000f, boss.Phase == DefenseYakshaPhase.Defeated);
            Check(session.Phase == DefenseGamePhase.Win, "Session alone publishes Win at 600 with FSM-confirmed defeat");
        }
        using (var session = new DefenseSession())
        using (var boss = Native(new DefenseHealthCore(1200f)))
        {
            session.Start(); boss.SpawnInArena(); session.Tick(600f, 1000f, false);
            Check(session.IsOvertime, "Alive Boss at 600 enters Overtime");
            boss.Health.ApplyDamage(new DefenseDamageContext(1200f)); session.Tick(0f, 1000f, boss.Phase == DefenseYakshaPhase.Defeated);
            Check(session.Phase == DefenseGamePhase.Win, "Overtime defeat feeds the same session result owner");
        }
        using (var session = new DefenseSession())
        using (var boss = Native(new DefenseHealthCore(1200f)))
        {
            session.Start(); boss.SpawnInArena(); boss.Health.ApplyDamage(new DefenseDamageContext(1200f));
            session.Tick(600f, 0f, boss.Phase == DefenseYakshaPhase.Defeated);
            Check(session.Phase == DefenseGamePhase.Loss, "Same-tick Base destruction beats FSM-confirmed Boss defeat");
        }
        bool rejected = false;
        try { new DefenseYakshaBehaviorFSM(new DefenseHealthCore(1200f), float.NaN); } catch (ArgumentException) { rejected = true; }
        Check(rejected, "Invalid arena duration is rejected before binding");
        using (var invalid = Native(new DefenseHealthCore(1200f)))
        {
            rejected = false; try { invalid.Tick(-1f, false); } catch (ArgumentOutOfRangeException) { rejected = true; }
            Check(rejected, "Invalid gameplay delta cannot extend arena or reverse timers");
        }
        var observerHealth = new DefenseHealthCore(1200f); var failing = Native(observerHealth); failing.SpawnInArena();
        failing.CombatStopped += () => { throw new Exception("observer"); };
        try { failing.Dispose(); } catch (Exception) { }
        observerHealth.ApplyDamage(new DefenseDamageContext(1200f));
        Check(failing.Phase != DefenseYakshaPhase.Defeated, "Disposal detaches shared-health listener even when a stop observer throws");
        Console.WriteLine("TOTAL " + _checks + " PASS (standalone shared-health Boss FSM and session results, no Unity runtime)");
    }
}
