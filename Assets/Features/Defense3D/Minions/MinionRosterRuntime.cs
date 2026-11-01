using System;
using System.Collections.Generic;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Read-only squad data for consumers; commands are issued separately from queries.</summary>
    public interface IMinionRosterReader
    {
        IReadOnlyList<MinionSquadRuntime> Squads { get; }
        int Revision { get; }
        event Action Changed;
        bool TryGetSquad(string id, out MinionSquadRuntime squad);
    }

    /// <summary>Commands identify a squad, independently of the behavior shared by its actors.</summary>
    public interface IMinionSquadCommands
    {
        bool CycleOrder(string id);
    }

    /// <summary>Session state of one equipped type. Two types with the same AI role never share this state.</summary>
    public sealed class MinionSquadRuntime
    {
        public DefenseMinionDefinitionSO Definition { get; }
        public string Id => Definition.Id;
        public DefenseMinionOrder Order { get; internal set; }
        public float HpMultiplier { get; internal set; } = 1f;
        public float SpeedMultiplier { get; internal set; } = 1f;
        internal MinionSquadRuntime(DefenseMinionDefinitionSO definition) { Definition = definition; }
    }

    /// <summary>Owns the equipped roster and independent orders. Selection is validated before replacing the old roster.</summary>
    public sealed class MinionRosterRuntime : IMinionRosterReader, IMinionSquadCommands
    {
        private readonly List<MinionSquadRuntime> _squads = new List<MinionSquadRuntime>();
        private readonly Dictionary<string, MinionSquadRuntime> _byId = new Dictionary<string, MinionSquadRuntime>(StringComparer.OrdinalIgnoreCase);
        private readonly IReadOnlyList<MinionSquadRuntime> _readOnly;
        public IReadOnlyList<MinionSquadRuntime> Squads => _readOnly;
        public int Revision { get; private set; }
        public event Action Changed;

        public MinionRosterRuntime() { _readOnly = _squads.AsReadOnly(); }

        /// <summary>Called at composition/rebind, never in the gameplay loop.</summary>
        public void Select(IEnumerable<string> selectedIds, IReadOnlyList<DefenseMinionDefinitionSO> catalog)
        {
            if (selectedIds == null || catalog == null) throw new ArgumentNullException();
            var definitions = new Dictionary<string, DefenseMinionDefinitionSO>(StringComparer.OrdinalIgnoreCase);
            foreach (var definition in catalog)
            {
                if (definition == null || string.IsNullOrWhiteSpace(definition.Id) || definition.WoodCost <= 0 || definition.Capacity <= 0)
                    throw new ArgumentException("Minion catalog contains an invalid definition.");
                if (definitions.ContainsKey(definition.Id)) throw new ArgumentException("Duplicate minion ID: " + definition.Id);
                definitions.Add(definition.Id, definition);
            }
            var selected = new List<MinionSquadRuntime>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string id in selectedIds)
            {
                if (string.IsNullOrWhiteSpace(id) || !definitions.TryGetValue(id, out var definition))
                    throw new ArgumentException("Equipped minion is missing from the authored catalog: " + id);
                if (!seen.Add(id)) throw new ArgumentException("Duplicate equipped minion ID: " + id);
                selected.Add(new MinionSquadRuntime(definition));
            }
            _squads.Clear(); _byId.Clear();
            foreach (var squad in selected) { _squads.Add(squad); _byId.Add(squad.Id, squad); }
            Revision++;
            Changed?.Invoke();
        }

        public bool TryGetSquad(string id, out MinionSquadRuntime squad)
        {
            squad = null;
            return id != null && _byId.TryGetValue(id, out squad);
        }

        public bool CycleOrder(string id)
        {
            if (!TryGetSquad(id, out var squad)) return false;
            switch (squad.Order)
            {
                case DefenseMinionOrder.Defend: squad.Order = DefenseMinionOrder.Follow; break;
                case DefenseMinionOrder.Follow: squad.Order = DefenseMinionOrder.Advance; break;
                default: squad.Order = DefenseMinionOrder.Defend; break;
            }
            Changed?.Invoke(); return true;
        }

        /// <summary>Preserves the equipped types and upgrades while resetting attempt-specific commands.</summary>
        public void ResetOrders()
        {
            foreach (var squad in _squads) squad.Order = DefenseMinionOrder.Defend;
            Changed?.Invoke();
        }
    }
}
