using UnityEngine;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Features.MetaProgression.Gacha.Core
{
    /// <summary>
    /// Bộ xử lý thanh toán Gacha mặc định sử dụng Cổ Tiền (MetaCurrencyManager).
    /// </summary>
    public class CoTienCurrencyProcessor : ICurrencyProcessor
    {
        public GachaCurrencyType CurrencyType => GachaCurrencyType.CoTien;

        public bool HasEnough(int amount)
        {
            if (MetaCurrencyManager.Instance == null) return false;
            return MetaCurrencyManager.Instance.TotalCurrency >= amount;
        }

        public bool TrySpend(int amount)
        {
            if (MetaCurrencyManager.Instance == null) return false;
            return MetaCurrencyManager.Instance.SpendCurrency(amount);
        }

        public int GetBalance()
        {
            if (MetaCurrencyManager.Instance == null) return 0;
            return MetaCurrencyManager.Instance.TotalCurrency;
        }
    }
}
