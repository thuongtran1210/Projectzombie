using NUnit.Framework;
using ProjectZombie.Features.Defense3D;

public sealed class DefenseHeroRespawnTests
{
    [Test]
    public void HeroDown_FreezesCountdownDuringPauseAndLevelUp()
    {
        var run = new DefenseRunLifecycle();
        var tracker = new DefenseHeroRespawnTracker(maxHp: 100f, respawnDelay: 5f);

        run.Start();
        tracker.TakeDamage(100f);
        Assert.IsTrue(tracker.IsDown);
        Assert.AreEqual(5f, tracker.RemainingRespawnTime);

        // Advance 2s in playing
        float dt1 = run.Tick(2f, 100f, false);
        tracker.Tick(dt1, run.State.IsTerminal);
        Assert.AreEqual(3f, tracker.RemainingRespawnTime);
        Assert.IsTrue(tracker.IsDown);

        // Pause for 10 real seconds -> run.Tick accepts 0s
        run.Pause();
        float dtPause = run.Tick(10f, 100f, false);
        Assert.AreEqual(0f, dtPause);
        tracker.Tick(dtPause, run.State.IsTerminal);
        Assert.AreEqual(3f, tracker.RemainingRespawnTime); // countdown must freeze
        Assert.IsTrue(tracker.IsDown);

        // Resume and advance 3s -> Hero respawns
        run.Resume();
        float dt2 = run.Tick(3f, 100f, false);
        tracker.Tick(dt2, run.State.IsTerminal);
        Assert.IsFalse(tracker.IsDown);
        Assert.AreEqual(100f, tracker.CurrentHp);
        Assert.AreEqual(0f, tracker.RemainingRespawnTime);
    }

    [Test]
    public void TerminalState_CancelsPendingHeroRespawn()
    {
        var run = new DefenseRunLifecycle();
        var tracker = new DefenseHeroRespawnTracker(maxHp: 100f, respawnDelay: 5f);

        run.Start();
        tracker.TakeDamage(100f);
        Assert.IsTrue(tracker.IsDown);

        // Base destroyed -> Terminal Loss
        run.Tick(1f, 0f, false);
        Assert.AreEqual(DefenseGamePhase.Loss, run.State.Phase);
        Assert.IsTrue(run.State.IsTerminal);

        tracker.Tick(1f, run.State.IsTerminal);
        Assert.IsTrue(tracker.IsDown);
        Assert.AreEqual(0f, tracker.RemainingRespawnTime);

        // Further tick cannot revive hero
        tracker.Tick(5f, run.State.IsTerminal);
        Assert.IsTrue(tracker.IsDown);
    }
}
