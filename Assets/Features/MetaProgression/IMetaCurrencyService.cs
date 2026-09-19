using System;
using ProjectZombie.Core.Save;

namespace ProjectZombie.Features.MetaProgression
{
    /// <summary>
    /// Contract giao tiếp chung cho việc quản lý số dư Cổ Tiền (Meta Currency).
    /// Giúp các UI Presenters không bị gắn chặt vào class cụ thể MetaCurrencyManager.
    /// </summary>
    public interface IMetaCurrencyService
    {
        int TotalCurrency { get; }
        event Action<int> OnCurrencyChanged;
        void AddCurrency(int amount);
        bool SpendCurrency(int amount);
        MetaProgressionSaveData GetSaveData();
        bool IsCharacterUnlocked(string heroId);
        void UnlockCharacter(string heroId);
    }
}
