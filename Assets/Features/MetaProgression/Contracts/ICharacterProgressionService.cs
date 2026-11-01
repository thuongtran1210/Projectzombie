using System;

namespace ProjectZombie.Features.MetaProgression
{
    /// <summary>
    /// Giao diện trừu tượng hóa cho dịch vụ Quản lý Tiến trình Tướng / Anh Hùng (SOLID: Interface Segregation & Dependency Inversion).
    /// </summary>
    public interface ICharacterProgressionService
    {
        CharacterStarProgressionSO ProgressionConfig { get; }

        event Action<string, int> OnCharacterStarUpgraded;
        event Action<string, int> OnCharacterShardsChanged;
        event Action<string> OnCharacterUnlocked;

        int GetCharacterStarLevel(string characterId);
        int GetCharacterShardCount(string characterId);
        bool IsCharacterUnlocked(string characterId);
        UpgradeCharacterResult TryUpgradeCharacterStar(string characterId);
    }
}
