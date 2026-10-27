using System;
using ProjectZombie.Features.Defense3D;

public static class DefenseLandingChecks
{
    private static int _checks;
    private static void Check(bool value, string name)
    {
        if (!value) throw new Exception(name);
        _checks++; Console.WriteLine("PASS " + name);
    }
    private static DefenseLandingFlight Flight(DefenseSession session) => new DefenseLandingFlight(session,
        new DefensePoint3(18f, 2f, 12f), new DefensePoint3(-9f, 0f, 12f), 2f, 3f, 5f, 4f);
    private static DefenseSpawnJob Job(string id, DefenseSpawnKind kind) => new DefenseSpawnJob(id, 0,
        kind == DefenseSpawnKind.Landing ? "quaidu" : "magiap", kind == DefenseSpawnKind.Landing ? "quaidu" : "magiap",
        kind == DefenseSpawnKind.Landing ? "cannon-west-landing" : "east", 0f, kind);
    public static void Main()
    {
        using (var session = new DefenseSession())
        {
            bool rejected = false;
            try { Flight(session); } catch (ArgumentException) { rejected = true; }
            Check(rejected, "Cannot launch while session is Ready");
            session.Start();
            var flight = Flight(session);
            Check(flight.Phase == DefenseLandingPhase.Ballistic && flight.Position.X == 18f, "Launch starts at cannon in ballistic phase");
            Check(!flight.TryTakeLanding(session, out _), "No actor delivery at launch");
            session.Tick(1f, 1000f, false); flight.Advance(session);
            Check(flight.Position.X == 4.5f && flight.Position.Y == 8f, "Ballistic midpoint crosses canal on an elevated arc");
            session.Pause(); session.Tick(100f, 1000f, false); flight.Advance(session);
            Check(flight.Position.X == 4.5f && !flight.TryTakeLanding(session, out _), "Pause freezes flight and prevents delivery");
            session.Resume(); session.Tick(1f, 1000f, false); flight.Advance(session);
            Check(flight.Phase == DefenseLandingPhase.Parachuting && flight.Position.X == -9f && flight.Position.Y == 4f, "Exact ballistic boundary opens parachute over West anchor");
            Check(!flight.TryTakeLanding(session, out _), "Parachuting cannot create a ground actor");
            session.OpenLevelUp(); session.Tick(100f, 1000f, false); flight.Advance(session);
            Check(flight.Position.Y == 4f, "Upgrade modal freezes parachute descent");
            session.Resume(); session.Tick(1.5f, 1000f, false); flight.Advance(session);
            Check(flight.Position.Y == 2f && flight.Phase == DefenseLandingPhase.Parachuting, "Descent interpolates only Y above fixed landing XZ");
            session.Tick(1.5f, 1000f, false);
            Check(flight.TryTakeLanding(session, out var landing) && landing.X == -9f && landing.Y == 0f,
                "Exact touchdown emits the authored West landing once");
            Check(!flight.TryTakeLanding(session, out _), "Duplicate tick cannot create another landing request");
            var cancelled = Flight(session); cancelled.Cancel(); session.Tick(6f, 1000f, false);
            Check(!cancelled.TryTakeLanding(session, out _) && cancelled.Phase == DefenseLandingPhase.Cancelled, "Cleanup cancellation permanently suppresses landing");
            var old = Flight(session); session.Restart(); session.Tick(6f, 1000f, false);
            Check(!old.TryTakeLanding(session, out _) && old.Phase == DefenseLandingPhase.Cancelled, "Restart rejects previous-generation projectile");
            using (var other = new DefenseSession())
            {
                other.Start(); other.Tick(6f, 1000f, false);
                var scoped = Flight(session);
                Check(!scoped.TryTakeLanding(other, out _), "Foreign session cannot deliver a projectile");
            }
            var terminal = Flight(session); session.Tick(6f, 0f, false);
            Check(!terminal.TryTakeLanding(session, out _), "Base loss closes flight delivery gate");
        }
        using (var session = new DefenseSession())
        {
            session.Start(); var flight = Flight(session); session.Tick(20f, 1000f, false);
            Check(flight.TryTakeLanding(session, out _) && !flight.TryTakeLanding(session, out _), "Long frame catches up to one touchdown without duplicating intermediate delivery");
            bool rejected = false;
            try { new DefenseLandingFlight(session, new DefensePoint3(float.NaN, 0f, 0f), default, 2f, 3f, 5f, 4f); }
            catch (ArgumentException) { rejected = true; }
            Check(rejected, "Non-finite projectile geometry is rejected");
        }
        var cadence = new DefenseCannonSchedule(30f, 12f, 600f);
        Check(!cadence.TryFire(29f, true, true, false), "Cannon waits for authored first-shot time");
        Check(!cadence.TryFire(30f, false, true, false) && !cadence.TryFire(30f, true, false, false), "Pause and destroyed source cannot launch");
        Check(!cadence.TryFire(30f, true, true, true) && cadence.TryFire(30f, true, true, false), "One authored projectile slot cannot be overwritten");
        Check(!cadence.TryFire(30f, true, true, false) && cadence.TryFire(42f, true, true, false), "Cadence commits exactly one shot and respects interval");
        Check(cadence.TryFire(100f, true, true, false) && !cadence.TryFire(100f, true, true, false), "Delayed frame fires once instead of replaying all missed intervals");
        Check(!cadence.TryFire(600f, true, true, false), "No new cannon launch at or beyond configured overtime cutoff");
        cadence.Reset();
        Check(cadence.TryFire(30f, true, true, false), "Restart restores the authored first-shot schedule");
        using (var sourceSession = new DefenseSession())
        {
            sourceSession.Start(); var alreadyFired = Flight(sourceSession);
            Check(!cadence.TryFire(50f, true, false, false), "Destroyed cannon stops future launches");
            sourceSession.Tick(6f, 1000f, false);
            Check(alreadyFired.TryTakeLanding(sourceSession, out _), "Already-fired projectile may land after source destruction");
        }
        var planner = new DefenseSpawnPlanner(1, 2, 1, 25f);
        planner.EnqueueJob(Job("wave", DefenseSpawnKind.Enemy), 0f);
        planner.EnqueueJob(Job("cannon", DefenseSpawnKind.Landing), 0f);
        Check(planner.ProcessSpawns(0f, null, j => true) == 1 && planner.CurrentActiveCount == 1 && planner.PendingCount == 1,
            "Wave and landing use one active cap and frame budget");
        Check(planner.ProcessSpawns(1f, null, j => true) == 0, "Landing cannot bypass a full shared active cap");
        planner.NotifyEnemyDefeatedOrRemoved();
        Check(planner.ProcessSpawns(2f, null, j => true) == 1 && planner.CurrentActiveCount == 1, "Released wave slot admits the pending landing");
        planner.Reset(); planner.EnqueueJob(Job("bad-ground", DefenseSpawnKind.Landing), 0f);
        Check(planner.ProcessSpawns(1f, j => false, j => true) == 0 && planner.PendingCount == 1 && planner.CurrentActiveCount == 0,
            "Invalid landing geometry remains pending without reserving active capacity");
        planner.ExpirePending(25.1f);
        Check(planner.PendingCount == 0 && planner.ExpiredJobCount == 1, "Blocked landing expires under the unchanged TTL");
        planner.Reset(); planner.EnqueueJob(Job("factory-retry", DefenseSpawnKind.Landing), 0f);
        Check(planner.ProcessSpawns(1f, null, j => false) == 0 && planner.PendingCount == 1 && planner.CurrentActiveCount == 0,
            "Failed landed actor acquisition preserves request identity and capacity");
        planner.Reset();
        Check(planner.PendingCount == 0 && planner.TotalJobCount == 0 && planner.CurrentActiveCount == 0, "Restart clears landing queue bookkeeping");
        Console.WriteLine("TOTAL " + _checks + " PASS (standalone landing flight/shared planner, no Unity runtime)");
    }
}
