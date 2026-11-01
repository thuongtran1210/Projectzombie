using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Native pools over prepared actors. Recruitment never constructs actors or changes their type.</summary>
    public sealed class DefenseMinionSlotPool : IMinionRecruitPool, IDisposable
    {
        private sealed class Group
        {
            public ObjectPool<DefenseMinion3D> Pool;
            public readonly HashSet<DefenseMinion3D> Leased = new HashSet<DefenseMinion3D>();
        }
        private readonly Dictionary<string, Group> _groups = new Dictionary<string, Group>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<DefenseMinion3D, Group> _owners = new Dictionary<DefenseMinion3D, Group>();
        private readonly Dictionary<DefenseMinion3D, int> _indices = new Dictionary<DefenseMinion3D, int>();
        private readonly Transform _home;
        public DefenseMinionSlotPool(DefenseMinion3D[] slots, Transform home)
        {
            _home = home;
            var types = new Dictionary<string, List<DefenseMinion3D>>(StringComparer.OrdinalIgnoreCase);
            for (int index = 0; index < slots.Length; index++)
            {
                var unit = slots[index];
                if (unit == null || unit.Definition == null || unit.gameObject.activeSelf)
                    throw new ArgumentException("Pool actors must be valid and dormant before preparation.");
                if (!types.TryGetValue(unit.Definition.Id, out var units)) { units = new List<DefenseMinion3D>(); types.Add(unit.Definition.Id, units); }
                units.Add(unit); _indices.Add(unit, index);
            }
            foreach (var type in types)
            {
                var source = new Queue<DefenseMinion3D>(type.Value);
                var group = new Group();
                group.Pool = new ObjectPool<DefenseMinion3D>(() => source.Dequeue(), null, null, null,
                    true, type.Value.Count, type.Value.Count);
                var warm = new DefenseMinion3D[type.Value.Count];
                for (int i = 0; i < warm.Length; i++) warm[i] = group.Pool.Get();
                group.Leased.UnionWith(warm); group.Leased.Clear();
                foreach (var unit in warm)
                {
                    group.Pool.Release(unit); _owners.Add(unit, group); unit.Retired += Return;
                }
                _groups.Add(type.Key, group);
            }
        }
        public int Count(string id) => id != null && _groups.TryGetValue(id, out var group) ? group.Pool.CountActive : 0;
        public int Capacity(string id) => id != null && _groups.TryGetValue(id, out var group) ? group.Pool.CountAll : 0;
        public bool TryDeploy(string id, out IMinionRecruitLease lease)
        {
            lease = null;
            if (id == null || !_groups.TryGetValue(id, out var group) || group.Pool.CountInactive == 0) return false;
            var unit = group.Pool.Get(); group.Leased.Add(unit);
            bool deployed = false;
            try
            {
                int index = _indices[unit];
                Vector3 spawn = _home.position + new Vector3(2.6f + index % 3 * .35f, 0f, (index % 5 - 2) * .45f);
                if (!NavMesh.SamplePosition(spawn, out var hit, 3f, NavMesh.AllAreas) || !unit.Deploy(hit.position, index)) return false;
                lease = unit; deployed = true; return true;
            }
            finally
            {
                if (!deployed) { unit.Retire(); Return(unit); }
            }
        }
        private void Return(DefenseMinion3D unit)
        {
            if (_owners.TryGetValue(unit, out var group) && group.Leased.Remove(unit)) group.Pool.Release(unit);
        }
        public void Dispose()
        {
            foreach (var unit in _owners.Keys) if (unit != null) unit.Retired -= Return;
            foreach (var group in _groups.Values) group.Pool.Dispose();
            _groups.Clear(); _owners.Clear(); _indices.Clear();
        }
    }
}
