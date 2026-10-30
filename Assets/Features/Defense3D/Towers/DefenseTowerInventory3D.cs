using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Bounded per-map tower inventory, populated before gameplay; building never instantiates.</summary>
    public sealed class DefenseTowerInventory3D
    {
        private readonly DefenseTowerCatalogSO _catalog;
        private DefenseTowerPrototype3D[][] _instances;
        public readonly List<DefenseSupportTarget3D> SupportTargets = new List<DefenseSupportTarget3D>();
        public DefenseTowerInventory3D(DefenseTowerCatalogSO catalog) => _catalog = catalog;

        public async Task PrepareAsync(Transform owner, int slots, IDefenseGameplayContext gameplay,
            DefenseProjectilePool3D launcher, Action<DefenseTowerPrototype3D> released, IReadOnlyList<string> allowed)
        {
            _catalog.Validate();
            _instances = new DefenseTowerPrototype3D[_catalog.Count][];
            var staging = new GameObject("Tower Inventory");
            staging.transform.SetParent(owner, false);
            staging.SetActive(false);
            for (int type = 0; type < _catalog.Count; type++)
            {
                var entry = _catalog.Get(type);
                bool selected = false;
                foreach (string id in allowed) if (string.Equals(id, entry.Data.Id, StringComparison.OrdinalIgnoreCase)) selected = true;
                if (!selected) continue;
                _instances[type] = new DefenseTowerPrototype3D[slots];
                for (int i = 0; i < slots; i++)
                {
                    if (owner == null || (gameplay is DefenseSession session && session.IsDisposed)) throw new OperationCanceledException("Map left while prewarming towers.");
                    var tower = UnityEngine.Object.Instantiate(entry.Prefab, staging.transform);
                    tower.gameObject.SetActive(false);
                    tower.Configure(entry.Data, launcher);
                    await tower.PrepareVisualsAsync();
                    if (tower.TryGetComponent<DefenseSupportTarget3D>(out var health)) SupportTargets.Add(health);
                    tower.BindGameplay(gameplay);
                    tower.Released += released;
                    _instances[type][i] = tower;
                    await Task.Yield();
                }
            }
            staging.SetActive(true);
        }

        public DefenseTowerPrototype3D FindIdle(string id)
        {
            if (_instances == null) return null;
            for (int i = 0; i < _catalog.Count; i++)
            {
                if (!string.Equals(_catalog.Get(i).Data.Id, id, StringComparison.OrdinalIgnoreCase)) continue;
                if (_instances[i] == null) return null;
                foreach (var tower in _instances[i]) if (tower != null && !tower.gameObject.activeSelf) return tower;
            }
            return null;
        }
        public bool Supports(string id)
        {
            if (_instances == null) return false;
            for (int i = 0; i < _catalog.Count; i++)
                if (string.Equals(id, _catalog.Get(i).Data.Id, StringComparison.OrdinalIgnoreCase)) return _instances[i] != null;
            return false;
        }

        public void Reset()
        {
            if (_instances == null) return;
            foreach (var row in _instances)
                if (row != null) foreach (var tower in row) if (tower != null) tower.gameObject.SetActive(false);
        }
    }
}
