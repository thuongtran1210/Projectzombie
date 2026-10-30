using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Map-owned tower definitions and prefabs. Shared IDs also appear in the loadout catalog.</summary>
    [CreateAssetMenu(menuName = "ProjectZombie/Defense3D/Tower Catalog")]
    public sealed class DefenseTowerCatalogSO : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            public SO_TowerData Data;
            public DefenseTowerPrototype3D Prefab;
        }
        [SerializeField] private Entry[] _entries = Array.Empty<Entry>();
        public int Count => _entries.Length;
        public Entry Get(int index) => _entries[index];
        public Entry Find(string id)
        {
            foreach (var entry in _entries)
                if (entry != null && entry.Data != null && string.Equals(entry.Data.Id, id, StringComparison.OrdinalIgnoreCase)) return entry;
            return null;
        }
        public void Validate()
        {
            if (_entries.Length == 0) throw new InvalidOperationException("Tower catalog is empty.");
            for (int i = 0; i < Count; i++)
            {
                var entry = Get(i);
                if (entry == null || entry.Data == null || entry.Prefab == null || entry.Data.MaxLevel == 0 ||
                    string.IsNullOrWhiteSpace(entry.Data.Id) || entry.Data.Attack == null)
                    throw new InvalidOperationException("Incomplete tower entry " + i);
                for (int level = 1; level <= entry.Data.MaxLevel; level++)
                {
                    var stats = entry.Data.GetLevelStats(level);
                    if (stats.buildCost < 0 || !DefenseDamageRules.IsFinite(stats.damage) || stats.damage <= 0f ||
                        !DefenseDamageRules.IsFinite(stats.attackRange) || stats.attackRange <= 0f ||
                        !DefenseDamageRules.IsFinite(stats.attackRate) || stats.attackRate <= 0f)
                        throw new InvalidOperationException("Invalid tower level: " + entry.Data.Id);
                }
                ValidateAttack(entry.Data.Attack);
                if (entry.Data.SpecialAttack != null) ValidateAttack(entry.Data.SpecialAttack);
                for (int j = 0; j < i; j++)
                    if (string.Equals(Get(j).Data.Id, entry.Data.Id, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("Duplicate tower id: " + entry.Data.Id);
            }
        }
        private static void ValidateAttack(DefenseTowerAttackProfileSO attack)
        {
            var definition = attack.Projectile;
            if (definition != null && (!DefenseDamageRules.IsFinite(definition.Speed) || definition.Speed <= 0f ||
                !DefenseDamageRules.IsFinite(definition.MaxLifetime) || definition.MaxLifetime <= 0f ||
                !DefenseDamageRules.IsFinite(definition.ArcHeight) || definition.ArcHeight < 0f ||
                !DefenseDamageRules.IsFinite(definition.AoERadius) || definition.AoERadius < 0f ||
                !DefenseDamageRules.IsFinite(attack.DamageMultiplier) || attack.DamageMultiplier <= 0f ||
                !DefenseDamageRules.IsFinite(attack.Cooldown) || attack.Cooldown < 0f))
                throw new InvalidOperationException("Attack requires finite positive projectile and damage parameters.");
            if (attack.Projectile == null || attack.Projectile.VisualPrefab == null ||
                !attack.Projectile.VisualPrefab.TryGetComponent<ProjectileCore3D>(out _) ||
                attack.Projectile.PayloadType == DefensePayloadType.Piercing)
                throw new InvalidOperationException("Attack requires a supported projectile prefab/payload. Piercing needs a collision implementation.");
        }
    }
}
