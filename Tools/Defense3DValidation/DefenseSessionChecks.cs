using System;
using System.Collections.Generic;
using ProjectZombie.Features.Defense3D;

public static class DefenseSessionChecks
{
    private static int _checks;
    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception(name);
        _checks++;
        Console.WriteLine("PASS " + name);
    }

    public static void Main()
    {
        using (var session = new DefenseSession())
        {
            var gates = new List<bool>();
            session.GameplayGateChanged += gates.Add;
            Check(!session.AdvancesGameplay && session.Tick(1f, 100f, false) == 0f, "Ready is closed");
            Check(session.Start() && gates.Count == 1 && gates[0], "Start opens one shared gate");
            session.Tick(2f, 100f, false);
            session.Pause();
            session.Tick(20f, 100f, false);
            Check(session.ElapsedSeconds == 2f && !session.AdvancesGameplay, "Pause freezes gameplay time");
            Check(session.Resume() && session.ElapsedSeconds == 2f, "Resume preserves clock");
            Check(session.OpenLevelUp() && session.RuntimeAdapter.Run.State.Phase == DefenseGamePhase.LevelUp, "Level selection uses LevelUp phase");
            session.Tick(20f, 100f, false);
            Check(session.ElapsedSeconds == 2f && session.Resume(), "LevelUp freezes then resumes");
            session.Tick(598f, 100f, false);
            Check(session.RuntimeAdapter.Run.State.Phase == DefenseGamePhase.Overtime && session.AdvancesGameplay, "Overtime stays active");
            session.Tick(0f, 0f, true);
            Check(session.RuntimeAdapter.Run.State.Phase == DefenseGamePhase.Loss && !session.AdvancesGameplay, "Same-tick Base loss beats Boss defeat");
        }
        using (var session = new DefenseSession())
        {
            session.Start();
            session.Tick(10f, 100f, true);
            Check(session.RuntimeAdapter.Run.State.Phase == DefenseGamePhase.Playing, "Early Boss defeat cannot win");
            session.Tick(590f, 100f, true);
            Check(session.RuntimeAdapter.Run.State.Phase == DefenseGamePhase.Win && !session.AdvancesGameplay, "600 seconds plus Boss defeat closes gate");
        }
        using (var session = new DefenseSession())
        {
            int cleanups = 0, resets = 0, resetSignals = 0;
            session.RuntimeAdapter.RegisterOwner(() =>
            {
                if (session.AdvancesGameplay) throw new Exception("Cleanup must run behind closed gate");
                cleanups++;
            }, () => resets++);
            session.RunReset += () =>
            {
                if (session.AdvancesGameplay || session.ElapsedSeconds != 0f) throw new Exception("Reset signal ordering");
                resetSignals++;
            };
            session.Start();
            for (int i = 0; i < 3; i++) { session.Tick(5f, 100f, false); session.Restart(); }
            Check(cleanups == 3 && resets == 3 && resetSignals == 3 && session.RunGeneration == 3 && session.AdvancesGameplay, "Three restarts clean/reset once and advance generation");
        }
        using (var session = new DefenseSession())
        {
            bool failOnce = true;
            session.RuntimeAdapter.RegisterOwner(() =>
            {
                if (failOnce) { failOnce = false; throw new Exception("Expected cleanup failure"); }
            }, () => { });
            session.Start();
            bool failed = false;
            try { session.Restart(); } catch (Exception) { failed = true; }
            Check(failed && !session.AdvancesGameplay && session.RunGeneration == 0, "Failed restart stays closed without advancing generation");
        }
        var disposed = new DefenseSession();
        disposed.Start();
        bool closed = false;
        disposed.GameplayGateChanged += active => closed = !active;
        disposed.Dispose();
        disposed.Dispose();
        bool rejected = false;
        try { disposed.Tick(1f, 100f, false); } catch (ObjectDisposedException) { rejected = true; }
        Check(closed && !disposed.AdvancesGameplay && rejected, "Disposal is idempotent and rejects further ticks");
        CheckShutdownOwnership();
        CheckSpawnTransactions();
        Console.WriteLine("TOTAL " + _checks + " PASS (standalone C#, no Unity runtime)");
    }

    private sealed class Resource : IDisposable
    {
        private readonly Action _dispose;
        public Resource(Action dispose) { _dispose = dispose; }
        public void Dispose() => _dispose();
    }

    private static void CheckShutdownOwnership()
    {
        var session = new DefenseSession();
        int actorCleanup = 0, ownerCleanup = 0, resourceDisposal = 0;
        session.RunCleanup += () => actorCleanup++;
        session.RuntimeAdapter.RegisterOwner(() => ownerCleanup++, () => { });
        var resource = new Resource(() => resourceDisposal++);
        session.Own(resource);
        bool duplicateRejected = false;
        try { session.Own(resource); } catch (InvalidOperationException) { duplicateRejected = true; }
        session.Start();
        session.Restart();
        Check(actorCleanup == 1 && ownerCleanup == 1 && resourceDisposal == 0, "Restart cleans round resources without disposing session services");
        session.Dispose();
        session.Dispose();
        Check(actorCleanup == 2 && ownerCleanup == 2 && resourceDisposal == 1 && duplicateRejected, "Shutdown releases actors and owned service exactly once");

        var failing = new DefenseSession();
        int continued = 0;
        failing.GameplayGateChanged += active => { if (!active) throw new Exception("Gate observer"); };
        failing.RuntimeAdapter.RegisterOwner(() => continued++, () => { });
        failing.RuntimeAdapter.RegisterOwner(() => { throw new Exception("Owner cleanup"); }, () => { });
        failing.RunCleanup += () => { throw new Exception("Observer cleanup"); };
        failing.RunCleanup += () => continued++;
        failing.Own(new Resource(() => continued++));
        failing.Own(new Resource(() => { throw new Exception("Service disposal"); }));
        failing.Start();
        bool reported = false;
        try { failing.Dispose(); } catch (AggregateException) { reported = true; }
        Check(reported && continued == 3 && failing.IsDisposed && !failing.AdvancesGameplay, "Shutdown attempts every owner, observer and service despite failures");
        failing.Dispose();

        using (var gated = new DefenseSession())
        {
            int stopped = 0;
            Action<bool> faultyGate = active => { if (!active) throw new Exception("Bad gate observer"); };
            gated.GameplayGateChanged += faultyGate;
            gated.GameplayGateChanged += active => { if (!active) stopped++; };
            gated.Start();
            bool gateError = false;
            try { gated.Pause(); } catch (AggregateException) { gateError = true; }
            Check(gateError && stopped == 1 && !gated.AdvancesGameplay, "One failing gate observer cannot prevent other actors from stopping");
            gated.GameplayGateChanged -= faultyGate;
        }
    }

    private static void CheckSpawnTransactions()
    {
        var job = new DefenseSpawnJob("event", 0, "magiap", "phicuong", "eastBank", 0f);
        var planner = new DefenseSpawnPlanner();
        planner.EnqueueJob(job, 0f);
        Check(planner.ProcessSpawns(0f, null, _ => false) == 0 && planner.PendingCount == 1 && planner.CurrentActiveCount == 0,
            "Failed factory preserves queued job and active capacity");
        bool identityPreserved = false;
        planner.ProcessSpawns(1f, null, candidate =>
        {
            identityPreserved = candidate.EventId == job.EventId && candidate.SlotIndex == 0 && candidate.ActualSpawnType == "phicuong";
            return true;
        });
        Check(identityPreserved && planner.PendingCount == 0 && planner.CurrentActiveCount == 1, "Retry commits the original elite slot exactly once");

        planner.Reset();
        planner.EnqueueJob(job, 0f);
        bool threw = false;
        try { planner.ProcessSpawns(0f, null, _ => { throw new Exception("Unexpected executor error"); }); }
        catch (Exception) { threw = true; }
        Check(threw && planner.PendingCount == 1 && planner.CurrentActiveCount == 0, "Throwing executor cannot lose a job or consume capacity");

        planner.Reset();
        for (int i = 0; i < 10; i++) planner.EnqueueJob(new DefenseSpawnJob("batch", i, "magiap", "magiap", "eastBank", 0f), 0f);
        int attempts = 0;
        planner.ProcessSpawns(0f, null, _ => { attempts++; return false; });
        Check(attempts == 4 && planner.PendingCount == 10 && planner.CurrentActiveCount == 0, "Factory failures obey the per-frame attempt budget");
        int expired = 0;
        planner.JobExpired += _ => expired++;
        planner.ProcessSpawns(26f, null, _ => true);
        Check(expired == 10 && planner.PendingCount == 0, "Retries retain original TTL instead of extending jobs indefinitely");

        var events = new List<DefenseScheduledSpawn>
        {
            new DefenseScheduledSpawn("first", 0f, "magiap", "phicuong", "eastBank", 2, DefenseSpawnKind.Enemy)
        };
        var scheduler = new DefenseStageSpawnScheduler(events);
        var jobs = new List<DefenseSpawnJob>();
        scheduler.EnqueueDue(0f, jobs.Add);
        scheduler.EnqueueDue(1f, jobs.Add);
        Check(jobs.Count == 2 && jobs[0].ActualSpawnType == "phicuong" && jobs[1].ActualSpawnType == "magiap", "Scheduler catch-up does not duplicate jobs or add elite slots");
    }
}
