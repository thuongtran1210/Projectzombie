namespace ProjectZombie.Features.Defense3D
{
    public enum MinionRecruitResult { Success, NotEquipped, Unavailable, InsufficientWood, Full, DeploymentFailed, StaleSession }

    /// <summary>Recruitment requests consumed by UI without knowing the pool or economy implementation.</summary>
    public interface IMinionRecruitmentCommands { MinionRecruitResult TryRecruit(string id); }

    /// <summary>Only the economy operations required to commit a recruitment.</summary>
    public interface IMinionRecruitWallet
    {
        int Wood { get; }
        bool TrySpend(int amount);
    }

    public interface IMinionRecruitContext
    {
        bool CanRecruit { get; }
        object SessionIdentity { get; }
        int RunGeneration { get; }
    }

    /// <summary>A deployed actor can be returned if the transaction becomes invalid before payment.</summary>
    public interface IMinionRecruitLease { void ReleaseRecruit(); }

    public interface IMinionRecruitPool
    {
        int Count(string id);
        int Capacity(string id);
        bool TryDeploy(string id, out IMinionRecruitLease lease);
    }

    /// <summary>Coordinates a bounded deployment/payment transaction without Unity, NavMesh or UI dependencies.</summary>
    public sealed class MinionRecruitmentService
    {
        private readonly IMinionRosterReader _roster;
        private readonly IMinionRecruitContext _context;
        private readonly IMinionRecruitWallet _wallet;
        private readonly IMinionRecruitPool _pool;
        private bool _busy;

        public MinionRecruitmentService(IMinionRosterReader roster, IMinionRecruitContext context,
            IMinionRecruitWallet wallet, IMinionRecruitPool pool)
        {
            _roster = roster ?? throw new System.ArgumentNullException(nameof(roster));
            _context = context ?? throw new System.ArgumentNullException(nameof(context));
            _wallet = wallet ?? throw new System.ArgumentNullException(nameof(wallet));
            _pool = pool ?? throw new System.ArgumentNullException(nameof(pool));
        }

        public MinionRecruitResult TryRecruit(string id)
        {
            if (!_roster.TryGetSquad(id, out var squad)) return MinionRecruitResult.NotEquipped;
            if (_busy || !_context.CanRecruit) return MinionRecruitResult.Unavailable;
            int price = squad.Definition.WoodCost;
            if (_wallet.Wood < price) return MinionRecruitResult.InsufficientWood;
            if (_pool.Count(id) >= System.Math.Min(squad.Definition.Capacity, _pool.Capacity(id))) return MinionRecruitResult.Full;
            object session = _context.SessionIdentity;
            int generation = _context.RunGeneration;
            _busy = true;
            IMinionRecruitLease lease = null;
            bool committed = false;
            try
            {
                if (!_pool.TryDeploy(id, out lease) || lease == null) return MinionRecruitResult.DeploymentFailed;
                if (!_context.CanRecruit || !ReferenceEquals(session, _context.SessionIdentity) || generation != _context.RunGeneration)
                    return MinionRecruitResult.StaleSession;
                if (!_wallet.TrySpend(price)) return MinionRecruitResult.InsufficientWood;
                committed = true;
                return MinionRecruitResult.Success;
            }
            finally
            {
                try { if (!committed && lease != null) lease.ReleaseRecruit(); }
                finally { _busy = false; }
            }
        }
    }
}
