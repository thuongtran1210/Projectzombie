using NUnit.Framework;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.UI;

public sealed class DefenseSpawnWarningTests
{
    private static DefenseSpawnJob Job(string id = "wave", float time = 18f, int slot = 0,
        DefenseSpawnKind kind = DefenseSpawnKind.Enemy) => new DefenseSpawnJob(id, slot, "magiap", "magiap", "east", time, kind);

    [Test]
    public void ScheduledWarningCountsDownThenRemainsPendingUntilActualSpawn()
    {
        var source = new DefenseSpawnWarningTimeline();
        var job = Job(); source.Add(job, default);
        Assert.IsFalse(source.GetNotice(0).TryDisplay(12f, 5f, .8f, out _, out _));
        Assert.IsTrue(source.GetNotice(0).TryDisplay(15.5f, 5f, .8f, out float fraction, out bool pending));
        Assert.AreEqual(.5f, fraction); Assert.IsFalse(pending);
        Assert.IsTrue(source.GetNotice(0).TryDisplay(30f, 5f, .8f, out fraction, out pending));
        Assert.AreEqual(0f, fraction); Assert.IsTrue(pending);
        source.Complete(job, 30f, true, new DefensePoint3(3, 1, 2));
        Assert.IsTrue(source.GetNotice(0).TryDisplay(30.4f, 5f, .8f, out fraction, out pending));
        Assert.AreEqual(1f, fraction); Assert.IsFalse(pending);
        Assert.IsFalse(source.GetNotice(0).TryDisplay(31f, 5f, .8f, out _, out _));
    }

    [Test]
    public void TimeZeroWaveHasPendingOrArrivalBadgeWithoutDelayingSpawn()
    {
        var source = new DefenseSpawnWarningTimeline(); var job = Job(time: 0f);
        source.Add(job, default);
        Assert.IsTrue(source.GetNotice(0).TryDisplay(0f, 5f, .8f, out float fraction, out bool pending));
        Assert.AreEqual(0f, fraction); Assert.IsTrue(pending);
        source.Complete(job, 0f, true);
        Assert.IsTrue(source.GetNotice(0).TryDisplay(.2f, 5f, .8f, out fraction, out pending));
        Assert.AreEqual(1f, fraction); Assert.IsFalse(pending);
        Assert.IsFalse(source.GetNotice(0).TryDisplay(1f, 5f, .8f, out _, out _));
    }

    [Test]
    public void LandingIsVisibleFromLaunchAndDoesNotEnqueueUntilTouchdown()
    {
        var source = new DefenseSpawnWarningTimeline();
        var planner = new DefenseSpawnPlanner();
        var job = Job("landing:cannon:1", 25f, kind: DefenseSpawnKind.Landing);
        source.Add(job, new DefensePoint3(-5, 0, 3), 10f, true);
        Assert.AreEqual(0, planner.PendingCount);
        Assert.IsFalse(source.GetNotice(0).TryDisplay(9f, 5f, .8f, out _, out _));
        Assert.IsTrue(source.GetNotice(0).TryDisplay(10f, 5f, .8f, out float fraction, out _));
        Assert.AreEqual(1f, fraction);
        Assert.IsTrue(source.GetNotice(0).TryDisplay(17.5f, 5f, .8f, out fraction, out _));
        Assert.AreEqual(.5f, fraction);
        Assert.IsTrue(source.GetNotice(0).TryDisplay(26f, 5f, .8f, out _, out bool pending));
        Assert.IsTrue(pending);
        Assert.IsTrue(planner.EnqueueJob(job, 25f));
        planner.JobSpawned += j => source.Complete(j, 26f, true);
        Assert.AreEqual(1, planner.ProcessSpawns(26f));
        Assert.AreEqual(DefenseSpawnNoticeState.Spawned, source.GetNotice(0).State);
    }

    [Test]
    public void FactoryFailureAndBlockedPlacementKeepWarningUntilPlannerCommits()
    {
        var source = new DefenseSpawnWarningTimeline(); var planner = new DefenseSpawnPlanner(); var job = Job(time: 0f);
        source.Add(job, default); planner.EnqueueJob(job, 0f);
        planner.JobSpawned += j => source.Complete(j, 3f, true);
        Assert.AreEqual(0, planner.ProcessSpawns(1f, j => false));
        Assert.AreEqual(0, planner.ProcessSpawns(2f, j => true, j => false));
        Assert.AreEqual(DefenseSpawnNoticeState.Scheduled, source.GetNotice(0).State);
        Assert.IsTrue(source.GetNotice(0).TryDisplay(2f, 5f, .8f, out _, out bool pending)); Assert.IsTrue(pending);
        Assert.AreEqual(1, planner.ProcessSpawns(3f, j => true, j => true));
        Assert.AreEqual(1, planner.SpawnedJobCount);
        Assert.AreEqual(DefenseSpawnNoticeState.Spawned, source.GetNotice(0).State);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ExpiredAndOverflowedJobsCloseWarning(bool overflow)
    {
        var source = new DefenseSpawnWarningTimeline(); var planner = new DefenseSpawnPlanner(maxPending: 1, lifetime: 2f);
        var first = Job("first", 0f); var second = Job("second", 0f);
        source.Add(first, default); source.Add(second, default);
        planner.JobExpired += j => source.Complete(j, 3f, false);
        planner.JobOverflowed += j => source.Complete(j, 0f, false);
        planner.EnqueueJob(first, 0f);
        if (overflow) planner.EnqueueJob(second, 0f);
        else planner.ExpirePending(3f);
        int index = overflow ? 1 : 0;
        Assert.AreEqual(DefenseSpawnNoticeState.Closed, source.GetNotice(index).State);
        Assert.IsFalse(source.GetNotice(index).TryDisplay(3f, 5f, .8f, out _, out _));
    }

    [Test]
    public void ResetDropsFlightsAndRestoresAuthoredPositionAndScheduledState()
    {
        var source = new DefenseSpawnWarningTimeline(); var job = Job();
        source.Add(job, new DefensePoint3(1, 2, 3));
        source.Add(Job("landing:cannon:1", 20f), default, 10f, true);
        source.Complete(job, 19f, true, new DefensePoint3(9, 9, 9));
        source.Reset(); source.Reset();
        Assert.AreEqual(1, source.Count); Assert.AreEqual(1f, source.GetNotice(0).Position.X);
        Assert.AreEqual(DefenseSpawnNoticeState.Scheduled, source.GetNotice(0).State);
        Assert.IsTrue(source.Add(Job("landing:cannon:1", 20f), default, 10f, true));
    }

    [Test]
    public void CancelledFlightAndDuplicateOutcomeCannotResurrectBadge()
    {
        var source = new DefenseSpawnWarningTimeline(); var job = Job("flight", 10f);
        Assert.IsTrue(source.Add(job, default, 5f, true));
        Assert.IsFalse(source.Add(job, default, 5f, true));
        source.Complete(job, 7f, false); source.Complete(job, 10f, true);
        Assert.AreEqual(1, source.Count);
        Assert.IsFalse(source.GetNotice(0).TryDisplay(10f, 5f, .8f, out _, out _));
    }

    [Test]
    public void SnapshotMutationCannotChangeRuntimeJobIdentity()
    {
        var source = new DefenseSpawnWarningTimeline(); source.Add(Job(), default);
        var snapshot = source.GetNotice(0); var job = snapshot.Job; job.EventId = "changed";
        Assert.AreEqual("wave", source.GetNotice(0).Job.EventId);
    }

    [TestCase("magiap", DefenseSpawnWarningSymbol.Armored)]
    [TestCase("magiap_boss", DefenseSpawnWarningSymbol.Commander)]
    [TestCase("yaksha_boss", DefenseSpawnWarningSymbol.Boss)]
    [TestCase("huyetdoi", DefenseSpawnWarningSymbol.Bat)]
    [TestCase("thuyquai", DefenseSpawnWarningSymbol.Fish)]
    [TestCase("phicuong", DefenseSpawnWarningSymbol.Elite)]
    [TestCase("quaidu", DefenseSpawnWarningSymbol.Parachute)]
    [TestCase("ghost_cannon", DefenseSpawnWarningSymbol.Cannon)]
    public void KnownEnemiesHaveExplicitSymbols(string id, DefenseSpawnWarningSymbol expected)
    {
        Assert.AreEqual(expected, DefenseSpawnWarningPresenter3D.SymbolFor(id, DefenseSpawnKind.Enemy));
    }
}
