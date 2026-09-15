using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Upgrades.Weighting
{
    /// <summary>
    /// Pipeline tính toán trọng số tổng hợp từ nhiều Weighter độc lập (Composite Pattern).
    /// Hỗ trợ Unit Test và mở rộng quy tắc tính trọng số không gây ảnh hưởng lẫn nhau.
    /// </summary>
    public class UpgradeWeightPipeline
    {
        private readonly List<IUpgradeWeighter> _weighters = new List<IUpgradeWeighter>();

        public void RegisterWeighter(IUpgradeWeighter weighter)
        {
            if (weighter != null && !_weighters.Contains(weighter))
            {
                _weighters.Add(weighter);
            }
        }

        public void UnregisterWeighter(IUpgradeWeighter weighter)
        {
            if (weighter != null)
            {
                _weighters.Remove(weighter);
            }
        }

        public void ClearWeighters()
        {
            _weighters.Clear();
        }

        /// <summary>
        /// Tính toán trọng số thực tế cuối cùng (Effective Weight) của thẻ nâng cấp.
        /// </summary>
        public float CalculateFinalWeight(UpgradeData upgrade, PlayerContext context)
        {
            if (upgrade == null) return 0f;

            float weight = Mathf.Max(0.1f, upgrade.spawnWeight);

            // Chạy tuần tự qua các Strategy Weighter đã đăng ký
            for (int i = 0; i < _weighters.Count; i++)
            {
                weight *= _weighters[i].CalculateMultiplier(upgrade, context);
            }

            return weight;
        }
    }
}
