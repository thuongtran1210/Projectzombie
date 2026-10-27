using System;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// U3D-019: Pure C# experience and level advancement tracker.
    /// Invariants:
    /// - Experience requirements scale by level.
    /// - Extra XP carries over.
    /// - Level-ups queue up if multiple levels are achieved simultaneously.
    /// </summary>
    public sealed class DefenseExperienceCore
    {
        public int CurrentLevel { get; private set; }
        public int CurrentExp { get; private set; }
        public int PendingLevelUps { get; private set; }

        public event Action<int> LevelGained;
        public event Action<int, int> ExpChanged;

        public DefenseExperienceCore(int startingLevel = 1)
        {
            CurrentLevel = Math.Max(1, startingLevel);
            CurrentExp = 0;
            PendingLevelUps = 0;
        }

        public static int GetExpRequiredForLevel(int level)
        {
            // Standard roguelite progression curve: 20 * level + 10
            return 20 * level + 10;
        }

        public void AddExp(int amount)
        {
            if (amount <= 0) return;

            CurrentExp += amount;
            while (CurrentExp >= GetExpRequiredForLevel(CurrentLevel + PendingLevelUps))
            {
                CurrentExp -= GetExpRequiredForLevel(CurrentLevel + PendingLevelUps);
                PendingLevelUps++;
                LevelGained?.Invoke(CurrentLevel + PendingLevelUps);
            }
            ExpChanged?.Invoke(CurrentExp, GetExpRequiredForLevel(CurrentLevel + PendingLevelUps));
        }

        public bool TryConsumePendingLevelUp()
        {
            if (PendingLevelUps <= 0) return false;
            PendingLevelUps--;
            CurrentLevel++;
            return true;
        }

        public void Reset()
        {
            CurrentLevel = 1;
            CurrentExp = 0;
            PendingLevelUps = 0;
            ExpChanged?.Invoke(0, GetExpRequiredForLevel(1));
        }
    }
}
