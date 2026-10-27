using System;

namespace ProjectZombie.Features.Defense3D
{
    public enum DefenseTowerAction
    {
        Build,
        Upgrade,
        Repair
    }

    /// <summary>
    /// U3D-015: Pure C# Tower Socket state and upgrade logic.
    /// Invariants:
    /// - Sockets start Empty (Level 0).
    /// - Max Level is 3 (Level 1 -> Level 2 -> Level 3).
    /// - Build/Upgrade cost scales with level.
    /// - Repair restores HP using wood with cooldown/limit.
    /// </summary>
    public sealed class DefenseTowerSocketCore
    {
        public const int MaxLevel = 3;
        public const int DefaultBuildCost = 25;
        public const int DefaultUpgradeCostLv2 = 40;
        public const int DefaultUpgradeCostLv3 = 60;
        public const int DefaultRepairCost = 15;
        public const float BaseHpPerLevel = 150f;

        public int SocketIndex { get; }
        public int Level { get; private set; }
        public bool IsOccupied => Level > 0;
        public DefenseHealthCore Health { get; private set; }

        public event Action<int, int> LevelChanged;

        public DefenseTowerSocketCore(int socketIndex)
        {
            SocketIndex = socketIndex;
            Level = 0;
            Health = null;
        }

        public int GetRequiredWood(DefenseTowerAction action)
        {
            switch (action)
            {
                case DefenseTowerAction.Build:
                    return Level == 0 ? DefaultBuildCost : -1;
                case DefenseTowerAction.Upgrade:
                    if (Level == 1) return DefaultUpgradeCostLv2;
                    if (Level == 2) return DefaultUpgradeCostLv3;
                    return -1; // Max level
                case DefenseTowerAction.Repair:
                    if (Level > 0 && Health != null && Health.CurrentHp < Health.MaxHp)
                        return DefaultRepairCost;
                    return -1;
                default:
                    return -1;
            }
        }

        public bool TryBuild(int availableWood, out int spentWood)
        {
            spentWood = 0;
            if (Level > 0) return false;

            int cost = GetRequiredWood(DefenseTowerAction.Build);
            if (availableWood < cost) return false;

            Level = 1;
            Health = new DefenseHealthCore(BaseHpPerLevel * Level);
            spentWood = cost;
            LevelChanged?.Invoke(SocketIndex, Level);
            return true;
        }

        public bool TryUpgrade(int availableWood, out int spentWood)
        {
            spentWood = 0;
            if (Level <= 0 || Level >= MaxLevel) return false;

            int cost = GetRequiredWood(DefenseTowerAction.Upgrade);
            if (availableWood < cost) return false;

            Level++;
            Health.Reset(BaseHpPerLevel * Level);
            spentWood = cost;
            LevelChanged?.Invoke(SocketIndex, Level);
            return true;
        }

        public bool TryRepair(int availableWood, out int spentWood)
        {
            spentWood = 0;
            if (Level <= 0 || Health == null || Health.CurrentHp >= Health.MaxHp) return false;

            int cost = GetRequiredWood(DefenseTowerAction.Repair);
            if (availableWood < cost) return false;

            Health.Reset(Health.MaxHp);
            spentWood = cost;
            return true;
        }

        public void Reset()
        {
            Level = 0;
            Health = null;
            LevelChanged?.Invoke(SocketIndex, 0);
        }
    }
}
