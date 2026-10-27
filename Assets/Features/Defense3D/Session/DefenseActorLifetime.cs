using System;

namespace ProjectZombie.Features.Defense3D
{
    public enum DefenseActorReleaseReason { Defeated, Cleanup, SpawnFailed, ExternalRemoved, ExternalDisabled }

    /// <summary>Immutable spawn identity with once-only reward and retirement latches; never reused across spawns.</summary>
    public sealed class DefenseActorLifetime
    {
        public object SessionIdentity { get; }
        public int RunGeneration { get; }
        public long LeaseId { get; }
        public bool IsRetired { get; private set; }
        public DefenseActorReleaseReason? ReleaseReason { get; private set; }
        public bool RewardClaimed { get; private set; }

        public DefenseActorLifetime(object sessionIdentity, int runGeneration, long leaseId)
        {
            SessionIdentity = sessionIdentity ?? throw new ArgumentNullException(nameof(sessionIdentity));
            if (runGeneration < 0 || leaseId <= 0) throw new ArgumentOutOfRangeException(nameof(leaseId));
            RunGeneration = runGeneration;
            LeaseId = leaseId;
        }

        public bool IsCurrent(IDefenseGameplayContext gameplay) => !IsRetired && gameplay != null &&
            ReferenceEquals(SessionIdentity,gameplay.SessionIdentity) && RunGeneration == gameplay.RunGeneration;

        public bool TryClaimReward(IDefenseGameplayContext gameplay)
        {
            if (RewardClaimed || !IsCurrent(gameplay) || !gameplay.AdvancesGameplay) return false;
            RewardClaimed = true;
            return true;
        }

        public bool TryRetire(object sessionIdentity, int runGeneration, DefenseActorReleaseReason reason)
        {
            if (IsRetired || !ReferenceEquals(SessionIdentity,sessionIdentity) || RunGeneration != runGeneration ||
                (uint)reason > (uint)DefenseActorReleaseReason.ExternalDisabled) return false;
            IsRetired = true;
            ReleaseReason = reason;
            return true;
        }
    }
}
