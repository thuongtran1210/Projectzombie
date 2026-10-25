using NUnit.Framework;
using ProjectZombie.Features.Defense3D;
using System.Linq;

public sealed class DefenseG3StageAndSpawnTests
{
    [Test]
    public void Stage01Schedule_GeneratesExact34Events_And214Jobs()
    {
        var events = DefenseStage01Schedule.GenerateEvents();
        Assert.AreEqual(34, events.Count);

        // Check Event #7 (index 6, at 108s) -> D-05: exactly 1 magiap_boss
        var minibossEvent = events[6];
        Assert.AreEqual("benque-07", minibossEvent.EventId);
        Assert.AreEqual(108f, minibossEvent.TriggerTime);
        Assert.AreEqual(DefenseEnemyRegistry.ID_MAGIAP_BOSS, minibossEvent.DeclaredType);
        Assert.AreEqual(1, minibossEvent.JobCount);

        var allJobs = DefenseStage01Schedule.GenerateAllSpawnJobs();
        Assert.AreEqual(214, allJobs.Count);

        // Check D-11/A elite lead counts
        int magiapEvents = events.Count(e => e.DeclaredType == DefenseEnemyRegistry.ID_MAGIAP);
        Assert.AreEqual(20, magiapEvents);

        int phicuongJobs = allJobs.Count(j => j.ActualSpawnType == DefenseEnemyRegistry.ID_PHICUONG);
        int magiapJobs = allJobs.Count(j => j.ActualSpawnType == DefenseEnemyRegistry.ID_MAGIAP);
        int huyetdoiJobs = allJobs.Count(j => j.ActualSpawnType == DefenseEnemyRegistry.ID_HUYETDOI);
        int thuyquaiJobs = allJobs.Count(j => j.ActualSpawnType == DefenseEnemyRegistry.ID_THUYQUAI);
        int minibossJobs = allJobs.Count(j => j.ActualSpawnType == DefenseEnemyRegistry.ID_MAGIAP_BOSS);

        Assert.AreEqual(20, phicuongJobs, "D-11/A: exactly 20 phicuong lead slots");
        Assert.AreEqual(108, magiapJobs, "D-11/A: exactly 108 remaining magiap slots");
        Assert.AreEqual(40, huyetdoiJobs);
        Assert.AreEqual(45, thuyquaiJobs);
        Assert.AreEqual(1, minibossJobs);
        Assert.AreEqual(214, phicuongJobs + magiapJobs + huyetdoiJobs + thuyquaiJobs + minibossJobs);
    }

    [Test]
    public void SpawnPlanner_EnforcesCapAndBudgetPerFrame()
    {
        var planner = new DefenseSpawnPlanner();
        var jobs = DefenseStage01Schedule.GenerateAllSpawnJobs();

        // Enqueue 10 jobs at t=0
        for (int i = 0; i < 10; i++)
        {
            Assert.IsTrue(planner.EnqueueJob(jobs[i], 0f));
        }
        Assert.AreEqual(10, planner.PendingCount);

        // First frame processing (max 4 per frame)
        int spawnedFrame1 = planner.ProcessSpawns(0f);
        Assert.AreEqual(DefenseSpawnPlanner.SpawnBudgetPerFrame, spawnedFrame1);
        Assert.AreEqual(4, planner.CurrentActiveCount);
        Assert.AreEqual(6, planner.PendingCount);

        // Second frame processing
        int spawnedFrame2 = planner.ProcessSpawns(0.1f);
        Assert.AreEqual(4, spawnedFrame2);
        Assert.AreEqual(8, planner.CurrentActiveCount);
        Assert.AreEqual(2, planner.PendingCount);
    }

    [Test]
    public void SpawnPlanner_ExpiresJobsAfterTTL()
    {
        var planner = new DefenseSpawnPlanner();
        var job = new DefenseSpawnJob("benque-01", 0, "magiap", "phicuong", "eastBank", 0f);

        planner.EnqueueJob(job, 0f);
        Assert.AreEqual(1, planner.PendingCount);

        int expiredCount = 0;
        planner.JobExpired += j => expiredCount++;

        // Process at t = 26s (> 25s TTL)
        int spawned = planner.ProcessSpawns(26f);
        Assert.AreEqual(0, spawned);
        Assert.AreEqual(0, planner.PendingCount);
        Assert.AreEqual(1, expiredCount);
    }
}
