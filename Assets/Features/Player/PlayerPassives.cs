using ProjectZombie.Features.Upgrades;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// Quản lý danh sách các thẻ bị động (Passives) mà người chơi đã nhặt.
    /// Dùng để kiểm tra điều kiện tiến hóa (Evolution) hoặc tương tác khác.
    /// </summary>
    public class PlayerPassives : MonoBehaviour
    {
        private List<string> _activePassives = new List<string>();

        /// <summary>Theo dõi số lần nâng cấp từng loại (dùng cho upgrade có giới hạn cấp).</summary>
        private Dictionary<string, int> _upgradeCounters = new Dictionary<string, int>();

        /// <summary>Lưu trữ metadata của các UpgradeData đã nhận để hiển thị lên UI Panel_Skills.</summary>
        private Dictionary<string, UpgradeData> _passiveDataMap = new Dictionary<string, UpgradeData>();

        public IReadOnlyList<string> ActivePassives => _activePassives;
        public IReadOnlyDictionary<string, UpgradeData> PassiveDataMap => _passiveDataMap;

        public event System.Action OnPassivesChanged;

        public void AddPassive(string passiveId, UpgradeData data = null)
        {
            if (string.IsNullOrEmpty(passiveId)) return;
            
            if (data != null && !_passiveDataMap.ContainsKey(passiveId))
            {
                _passiveDataMap[passiveId] = data;
            }

            if (!_activePassives.Contains(passiveId))
            {
                _activePassives.Add(passiveId);
                Debug.Log($"[PlayerPassives] Added new passive: {passiveId}");
            }

            OnPassivesChanged?.Invoke();
        }

        public bool HasPassive(string passiveId)
        {
            if (string.IsNullOrEmpty(passiveId)) return false;
            return _activePassives.Contains(passiveId);
        }

        /// <summary>Lấy số lần nâng cấp của một upgrade key.</summary>
        public int GetUpgradeCount(string key)
        {
            if (string.IsNullOrEmpty(key)) return 0;
            return _upgradeCounters.TryGetValue(key, out int count) ? count : 0;
        }

        /// <summary>Tăng số lần nâng cấp của một upgrade key lên 1.</summary>
        public void IncrementUpgradeCount(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            if (_upgradeCounters.ContainsKey(key))
                _upgradeCounters[key]++;
            else
                _upgradeCounters[key] = 1;
        }
    }
}
