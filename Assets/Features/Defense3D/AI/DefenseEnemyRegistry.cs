using System;
using System.Collections.Generic;

namespace ProjectZombie.Features.Defense3D
{
    public struct DefenseEnemyStats
    {
        public string EnemyId;
        public string DisplayName;
        public DefenseEnemyLocomotionType Locomotion;
        public float MaxHp;
        public float Speed;
        public float Damage;
        public int WoodReward;
        public int ExpReward;
        public bool IsBoss;
        public float AttackInterval;
        public float AttackRange;

        public DefenseEnemyStats(string id, string name, DefenseEnemyLocomotionType loco, float hp, float speed, float dmg, int wood, int exp, bool isBoss = false)
        {
            EnemyId = id;
            DisplayName = name;
            Locomotion = loco;
            MaxHp = hp;
            Speed = speed;
            Damage = dmg;
            WoodReward = wood;
            ExpReward = exp;
            IsBoss = isBoss;
            AttackInterval = 2f;
            AttackRange = 1.2f;
        }
    }

    /// <summary>
    /// U3D-006 pure C# registry for the 7 official enemy archetypes.
    /// Provides benchmark default stats even in headless or unit-test environments.
    /// </summary>
    public static class DefenseEnemyRegistry
    {
        public const string ID_MAGIAP = "magiap";
        public const string ID_THUYQUAI = "thuyquai";
        public const string ID_HUYETDOI = "huyetdoi";
        public const string ID_QUAIDU = "quaidu";
        public const string ID_PHICUONG = "phicuong";
        public const string ID_MAGIAP_BOSS = "magiap_boss";
        public const string ID_YAKSHA_BOSS = "yaksha_boss";

        private static readonly Dictionary<string, DefenseEnemyStats> Archetypes = new Dictionary<string, DefenseEnemyStats>(StringComparer.OrdinalIgnoreCase)
        {
            { ID_MAGIAP, new DefenseEnemyStats(ID_MAGIAP, "Ma Giáp", DefenseEnemyLocomotionType.GroundNavMesh, 130f, 1.45f, 16f, 10, 5) },
            { ID_THUYQUAI, new DefenseEnemyStats(ID_THUYQUAI, "Thủy Quái", DefenseEnemyLocomotionType.AmphibianWaterToShore, 95f, 2.05f, 12f, 12, 6) },
            { ID_HUYETDOI, new DefenseEnemyStats(ID_HUYETDOI, "Huyết Dơi", DefenseEnemyLocomotionType.AirFlight, 50f, 3.10f, 8f, 8, 4) },
            { ID_QUAIDU, new DefenseEnemyStats(ID_QUAIDU, "Quái Dù", DefenseEnemyLocomotionType.ParatrooperLanding, 90f, 1.75f, 14f, 15, 8) },
            { ID_PHICUONG, new DefenseEnemyStats(ID_PHICUONG, "Phi Cương", DefenseEnemyLocomotionType.AirFlight, 260f, 1.65f, 18f, 25, 15) },
            { ID_MAGIAP_BOSS, new DefenseEnemyStats(ID_MAGIAP_BOSS, "Ma Giáp Thống Lĩnh", DefenseEnemyLocomotionType.GroundNavMesh, 700f, 1.18f, 24f, 80, 50, true) },
            { ID_YAKSHA_BOSS, new DefenseEnemyStats(ID_YAKSHA_BOSS, "Dạ Xoa", DefenseEnemyLocomotionType.GroundNavMesh, 3200f, 1.35f, 38f, 300, 200, true) }
        };

        public static bool TryGetStats(string id, out DefenseEnemyStats stats)
        {
            if (string.IsNullOrEmpty(id))
            {
                stats = default;
                return false;
            }
            return Archetypes.TryGetValue(id, out stats);
        }

        public static DefenseEnemyStats GetStats(string id)
        {
            if (TryGetStats(id, out var stats)) return stats;
            throw new KeyNotFoundException($"Enemy archetype '{id}' is not registered.");
        }

        public static bool IsKnownArchetype(string id)
        {
            return !string.IsNullOrEmpty(id) && Archetypes.ContainsKey(id);
        }
    }
}
