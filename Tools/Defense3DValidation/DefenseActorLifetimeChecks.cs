using System;
using ProjectZombie.Features.Defense3D;

public static class DefenseActorLifetimeChecks
{
    private static int _checks;
    private static void Check(bool value,string name)
    {
        if (!value) throw new Exception(name);
        _checks++;
        Console.WriteLine("PASS " + name);
    }
    public static void Main()
    {
        using (var session = new DefenseSession())
        {
            var lifetime = new DefenseActorLifetime(session,session.RunGeneration,1);
            Check(!lifetime.TryClaimReward(session) && !lifetime.RewardClaimed,"Ready session cannot claim actor rewards");
            session.Start();
            Check(lifetime.IsCurrent(session),"New actor lifetime belongs to its current session and generation");
            using (var other = new DefenseSession())
            {
                other.Start();
                Check(!lifetime.IsCurrent(other) && !lifetime.TryClaimReward(other),"Foreign session cannot claim another actor reward");
                Check(!lifetime.TryRetire(other,session.RunGeneration,DefenseActorReleaseReason.Defeated),"Foreign retirement cannot close an actor lifetime");
            }
            Check(!lifetime.TryRetire(session,session.RunGeneration+1,DefenseActorReleaseReason.Cleanup) && !lifetime.IsRetired,
                "Wrong generation cannot retire a live actor");
            Check(!lifetime.TryRetire(session,session.RunGeneration,(DefenseActorReleaseReason)999) && !lifetime.IsRetired,
                "Invalid release reason cannot poison retirement state");
            session.Pause();
            Check(!lifetime.TryClaimReward(session) && !lifetime.RewardClaimed,"Pause blocks reward without consuming the reward latch");
            session.Resume();
            Check(lifetime.TryClaimReward(session) && !lifetime.TryClaimReward(session),"Defeat reward is claimed exactly once");
            Check(lifetime.TryRetire(session,session.RunGeneration,DefenseActorReleaseReason.Defeated) &&
                lifetime.ReleaseReason==DefenseActorReleaseReason.Defeated,"Natural release stores its first retirement reason");
            Check(!lifetime.TryRetire(session,session.RunGeneration,DefenseActorReleaseReason.ExternalRemoved) &&
                lifetime.ReleaseReason==DefenseActorReleaseReason.Defeated,"Deferred destruction cannot release or change the reason twice");
            Check(!lifetime.IsCurrent(session) && !lifetime.TryClaimReward(session),"Retired actor cannot reenter target or reward paths");

            var cleanup = new DefenseActorLifetime(session,session.RunGeneration,2);
            cleanup.TryRetire(session,session.RunGeneration,DefenseActorReleaseReason.Cleanup);
            Check(!cleanup.TryClaimReward(session),"Cleanup retirement never grants a death reward");
            var failed = new DefenseActorLifetime(session,session.RunGeneration,3);
            failed.TryRetire(session,session.RunGeneration,DefenseActorReleaseReason.SpawnFailed);
            Check(!failed.TryClaimReward(session),"Failed initialization cannot grant rewards");
            var external = new DefenseActorLifetime(session,session.RunGeneration,4);
            external.TryRetire(session,session.RunGeneration,DefenseActorReleaseReason.ExternalRemoved);
            Check(!external.TryClaimReward(session),"External removal is not a kill reward");
            var disabled = new DefenseActorLifetime(session,session.RunGeneration,9);
            disabled.TryRetire(session,session.RunGeneration,DefenseActorReleaseReason.ExternalDisabled);
            Check(!disabled.TryClaimReward(session),"Disabling an actor cannot grant a kill reward");

            var old = new DefenseActorLifetime(session,session.RunGeneration,5);
            int oldGeneration = session.RunGeneration;
            session.Restart();
            Check(!old.IsCurrent(session) && !old.TryClaimReward(session),"Late actor callback cannot reward a restarted run");
            Check(old.TryRetire(session,oldGeneration,DefenseActorReleaseReason.Cleanup),"Old actor can still be cleaned with its original scoped identity");
            var replacement = new DefenseActorLifetime(session,session.RunGeneration,6);
            Check(replacement.IsCurrent(session) && replacement.LeaseId!=old.LeaseId && replacement.TryClaimReward(session),
                "Replacement spawn has a distinct live reward identity");
            var interrupted = new DefenseActorLifetime(session,session.RunGeneration,7);
            try
            {
                if (interrupted.TryClaimReward(session)) throw new Exception("simulated pickup factory failure");
            }
            catch (Exception) { }
            Check(!interrupted.TryClaimReward(session),"Failed reward production cannot claim the same reward a second time");
            var terminal = new DefenseActorLifetime(session,session.RunGeneration,8);
            session.Tick(0f,0f,false);
            Check(!terminal.TryClaimReward(session),"Terminal match blocks actor rewards");
        }
        bool invalid=false;
        try { new DefenseActorLifetime(new object(),0,0); }
        catch (ArgumentOutOfRangeException) { invalid=true; }
        Check(invalid,"Actor lease IDs must be positive");
        Console.WriteLine("TOTAL " + _checks + " PASS (standalone actor lifecycle, no Unity runtime)");
    }
}
