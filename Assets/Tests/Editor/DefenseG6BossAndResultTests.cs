using NUnit.Framework;
using ProjectZombie.Features.Defense3D;

public sealed class DefenseG6BossAndResultTests
{
    [Test]
    public void YakshaFSM_ArenaToAssault_PreservesHealth()
    {
        var yaksha = new DefenseYakshaBehaviorFSM(maxHp: 3200f, arenaDuration: 45f);
        Assert.AreEqual(DefenseYakshaPhase.Dormant, yaksha.Phase);

        // Spawn
        yaksha.SpawnInArena();
        Assert.AreEqual(DefenseYakshaPhase.ArenaCombat, yaksha.Phase);

        // Take damage during arena
        yaksha.Health.ApplyDamage(new DefenseDamageContext(1000f));
        Assert.AreEqual(2200f, yaksha.Health.CurrentHp);

        // Telegraph a slam
        Assert.IsTrue(yaksha.TryStartTelegraph(DefenseYakshaAttackTelegraph.TelegraphSlam));
        Assert.AreEqual(DefenseYakshaAttackTelegraph.TelegraphSlam, yaksha.ActiveTelegraph);

        // Tick 1.2s -> executes attack
        bool executed = false;
        yaksha.AttackExecuted += (type, dmg) =>
        {
            executed = true;
            Assert.AreEqual(DefenseYakshaAttackTelegraph.TelegraphSlam, type);
            Assert.AreEqual(DefenseYakshaBehaviorFSM.SlamDamage, dmg);
        };
        yaksha.Tick(1.2f, isPaused: false);
        Assert.IsTrue(executed);
        Assert.AreEqual(DefenseYakshaAttackTelegraph.None, yaksha.ActiveTelegraph);

        // Tick remaining arena time (45 - 1.2 = 43.8s) -> auto transition to AssaultBase
        yaksha.Tick(43.8f, isPaused: false);
        Assert.AreEqual(DefenseYakshaPhase.AssaultBase, yaksha.Phase);

        // Invariant: HP remains 2200, never resets on phase shift
        Assert.AreEqual(2200f, yaksha.Health.CurrentHp);
    }

    [Test]
    public void MatchResultEvaluator_EnforcesD03Rules()
    {
        // 1. Base HP <= 0 -> immediate LOSS
        Assert.AreEqual(DefenseGamePhase.Loss, DefenseMatchResultEvaluator.EvaluatePhase(0f, false, 200f, DefenseGamePhase.Playing));
        Assert.AreEqual(DefenseGamePhase.Loss, DefenseMatchResultEvaluator.EvaluatePhase(0f, true, 600f, DefenseGamePhase.Playing));

        // 2. Boss killed early at 400s -> still Playing, defense must hold until 600s
        Assert.AreEqual(DefenseGamePhase.Playing, DefenseMatchResultEvaluator.EvaluatePhase(1000f, true, 400f, DefenseGamePhase.Playing));

        // 3. 600s elapsed and Boss killed -> WIN
        Assert.AreEqual(DefenseGamePhase.Win, DefenseMatchResultEvaluator.EvaluatePhase(500f, true, 600f, DefenseGamePhase.Playing));

        // 4. 600s elapsed but Boss still alive -> OVERTIME
        Assert.AreEqual(DefenseGamePhase.Overtime, DefenseMatchResultEvaluator.EvaluatePhase(500f, false, 600f, DefenseGamePhase.Playing));

        // 5. Overtime: Boss killed later at 650s with Base alive -> WIN
        Assert.AreEqual(DefenseGamePhase.Win, DefenseMatchResultEvaluator.EvaluatePhase(200f, true, 650f, DefenseGamePhase.Overtime));

        // 6. Overtime: Base destroyed at 650s -> LOSS
        Assert.AreEqual(DefenseGamePhase.Loss, DefenseMatchResultEvaluator.EvaluatePhase(0f, false, 650f, DefenseGamePhase.Overtime));
    }
}
