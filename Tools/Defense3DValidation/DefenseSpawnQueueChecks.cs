using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using ProjectZombie.Features.Defense3D;

public static class DefenseSpawnQueueChecks
{
    private static int _checks;
    private static void Check(bool condition,string name)
    {
        if (!condition) throw new Exception(name);
        _checks++;
        Console.WriteLine("PASS " + name);
    }
    private static DefenseSpawnJob Job(string name,int slot=0) => new DefenseSpawnJob(name,slot,"magiap","magiap","eastBank",0f);

    public static void Main()
    {
        var planner=new DefenseSpawnPlanner(1,2,1,25f);
        planner.EnqueueJob(Job("active"),0f);
        planner.ProcessSpawns(0f);
        planner.EnqueueJob(Job("old"),0f);
        planner.EnqueueJob(Job("fresh"),20f);
        Check(planner.ExpirePending(26f)==1 && planner.PendingCount==1 && planner.CurrentActiveCount==1,
            "Expired pending jobs are reclaimed while the active cap remains full");
        planner.Reset();
        planner.EnqueueJob(Job("old"),0f);
        planner.EnqueueJob(Job("fresh"),20f);
        planner.ProcessSpawns(20f,j=>j.EventId!="old",j=>false);
        Check(planner.EnqueueJob(Job("incoming"),26f) && planner.OverflowJobCount==0 && planner.PendingCount==2,
            "Rotated stale jobs behind a fresh head are expired before queue capacity is tested");
        Check(!planner.EnqueueJob(Job("truly-full"),26f) && planner.OverflowJobCount==1,
            "Genuinely full live queues still report overflow and enforce the pending limit");
        planner.Reset();
        planner.EnqueueJob(Job("boundary"),0f);
        Check(planner.ExpirePending(25f)==0 && planner.ExpirePending(25.001f)==1,
            "TTL keeps the existing strict greater-than boundary");
        int expirationCallbacks=0;
        planner.JobExpired+=j=>expirationCallbacks++;
        planner.EnqueueJob(Job("once"),30f);
        planner.ExpirePending(56f);planner.ExpirePending(56f);
        Check(expirationCallbacks==1,"Expired job notification is published once");
        planner.Reset();
        Check(planner.TotalJobCount==0 && planner.SpawnedJobCount==0 && planner.OverflowJobCount==0 && planner.ExpiredJobCount==0,
            "Restart resets diagnostic accounting with queue state");
        planner.EnqueueJob(Job("placement"),0f);
        planner.ProcessSpawns(0f,j=>false,j=>true);
        Check(planner.LastPredicateBlocked==1 && planner.LastFactoryFailed==0,"Placement blocking is counted separately from factory failure");
        planner.ProcessSpawns(0f,j=>true,j=>false);
        Check(planner.LastPredicateBlocked==0 && planner.LastFactoryFailed==1,"Factory failure diagnostics identify retries without committing a slot");
        var resetDuringExpiry=new DefenseSpawnPlanner(1,2,1,1f);
        resetDuringExpiry.EnqueueJob(Job("old-reset"),0f);
        resetDuringExpiry.EnqueueJob(Job("old-reset",1),0f);
        resetDuringExpiry.JobExpired+=j=>{ resetDuringExpiry.Reset();resetDuringExpiry.EnqueueJob(Job("new-round"),2f); };
        resetDuringExpiry.ExpirePending(2f);
        Check(resetDuringExpiry.PendingCount==1 && resetDuringExpiry.TotalJobCount==1 && resetDuringExpiry.ExpiredJobCount==0,
            "Reset during expiry callbacks cannot consume jobs from the new round");

        var fast=ReadProfile("Stage01_FastSpawn");
        var production=ReadProfile("Stage01_Production");
        Check(fast.Count==35 && production.Count==35 && Count(fast)==215 && Count(production)==215,
            "Real authored profiles preserve 34 events with 214 ordinary slots plus the independent Boss");
        Check(Math.Abs(Find(fast,"benque-21").TriggerTime-18f)<0.001f &&
            Math.Abs(Find(production,"benque-21").TriggerTime-360f)<0.001f,
            "The reported benque-21 event is scheduled 20 times earlier by FastSpawn");
        var fastResult=Simulate(fast,60f);
        Check(fastResult.OverflowJobCount>0 && fastResult.CurrentActiveCount==32,
            "FastSpawn reproduces overflow under the authored 32-active cap with no actor removals");
        var productionResult=Simulate(production,600f);
        Check(productionResult.OverflowJobCount==0 && productionResult.CurrentActiveCount==32,
            "Production cadence fits the pending cap through 600 seconds even with no actor removals in this simulation");
        Check(fastResult.TotalJobCount==Count(fast) && productionResult.TotalJobCount==Count(production) &&
            Conserved(fastResult) && Conserved(productionResult),
            "Every scheduled slot remains accounted as spawned, pending, expired or overflowed");
        Console.WriteLine("TOTAL " + _checks + " PASS (standalone spawn queue and real profile schedules, no Unity runtime)");
    }

    private static bool Conserved(DefenseSpawnPlanner planner) => planner.TotalJobCount==planner.SpawnedJobCount+
        planner.PendingCount+planner.ExpiredJobCount+planner.OverflowJobCount;
    private static int Count(List<DefenseScheduledSpawn> events) { int count=0;foreach (var e in events)count+=e.Count;return count; }
    private static DefenseScheduledSpawn Find(List<DefenseScheduledSpawn> events,string name) => events.Find(e=>e.EventId==name);

    private static List<DefenseScheduledSpawn> ReadProfile(string name)
    {
        string path=Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"..","Assets","Resources","Defense3D","Stages",name+".asset"));
        var events=new List<DefenseScheduledSpawn>();
        string id=null;
        float time=0f,boss=float.NaN;
        foreach (string raw in File.ReadAllLines(path))
        {
            string line=raw.Trim();
            if (line.StartsWith("- EventId: ")) id=line.Substring("- EventId: ".Length);
            else if (line.StartsWith("TriggerTime: ")) time=float.Parse(line.Substring("TriggerTime: ".Length),CultureInfo.InvariantCulture);
            else if (line.StartsWith("Count: ") && id!=null)
            {
                events.Add(new DefenseScheduledSpawn(id,time,"magiap",null,"profile-zone",
                    int.Parse(line.Substring("Count: ".Length)),DefenseSpawnKind.Enemy));
                id=null;
            }
            else if (line.StartsWith("bossSpawnTime: ")) boss=float.Parse(line.Substring("bossSpawnTime: ".Length),CultureInfo.InvariantCulture);
        }
        if (float.IsNaN(boss)) throw new Exception("Missing Boss trigger in authored profile.");
        events.Add(new DefenseScheduledSpawn("arena-boss",boss,"yaksha_boss",null,"arenaEast",1,DefenseSpawnKind.ArenaBoss));
        events.Sort((a,b)=>a.TriggerTime.CompareTo(b.TriggerTime));
        return events;
    }
    private static DefenseSpawnPlanner Simulate(List<DefenseScheduledSpawn> events,float duration)
    {
        var scheduler=new DefenseStageSpawnScheduler(events);
        var planner=new DefenseSpawnPlanner();
        float now=0f;
        Action<DefenseSpawnJob> enqueue=j=>planner.EnqueueJob(j,now);
        for (int tick=0;tick<=(int)(duration*10);tick++)
        {
            now=tick/10f;
            planner.ExpirePending(now);
            scheduler.EnqueueDue(now,enqueue);
            planner.ProcessSpawns(now);
            if (planner.PendingCount>40 || planner.CurrentActiveCount>32) throw new Exception("Capacity contract violated.");
        }
        return planner;
    }
}
