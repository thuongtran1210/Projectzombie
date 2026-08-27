using ProjectZombie.Features.Upgrades;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// Quản lý danh sách các thẻ bị động (Passives) mà người chơi đã nhặt.
    /// Dùng để kiểm tra điều kiện tiến hóa (Evolution) hoặc tương tác khác.
    /// Giới hạn tối đa 6 slot Bị động theo chuẩn Roguelite.
    /// </summary>
    public class PlayerPassives : MonoBehaviour
    {
        public const int MAX_PASSIVES = 6;

        private List<string> _activePassives = new List<string>();
        private HashSet<string> _distinctPassiveIds = new HashSet<string>();

        /// <summary>Theo dõi số lần nâng cấp từng loại (dùng cho upgrade có giới hạn cấp).</summary>
        private Dictionary<string, int> _upgradeCounters = new Dictionary<string, int>();

        /// <summary>Lưu trữ metadata của các UpgradeData đã nhận để hiển thị lên UI Panel_Skills.</summary>
        private Dictionary<string, UpgradeData> _passiveDataMap = new Dictionary<string, UpgradeData>();

        public IReadOnlyList<string> ActivePassives => _activePassives;
        public IReadOnlyCollection<string> DistinctPassives => _distinctPassiveIds;
        public IReadOnlyDictionary<string, UpgradeData> PassiveDataMap => _passiveDataMap;

        public int DistinctPassiveCount => _distinctPassiveIds.Count;
        public bool IsFull() => _distinctPassiveIds.Count >= MAX_PASSIVES;

        public event System.Action OnPassivesChanged;

        public void AddPassive(string passiveKey, UpgradeData data = null)
        {
            if (string.IsNullOrEmpty(passiveKey)) return;
            
            // Lưu metadata hiển thị
            if (data != null && !_passiveDataMap.ContainsKey(passiveKey))
            {
                _passiveDataMap[passiveKey] = data;
            }

            // Đăng ký key truyền vào
            if (!_activePassives.Contains(passiveKey))
            {
                _activePassives.Add(passiveKey);
            }

            // Đăng ký cả mã ID chuẩn (VD: P001) và Name nếu có dữ liệu UpgradeData
            string primaryKey = passiveKey;
            if (data != null)
            {
                if (!string.IsNullOrEmpty(data.id) && !_activePassives.Contains(data.id))
                {
                    _activePassives.Add(data.id);
                    primaryKey = data.id;
                }
                if (!string.IsNullOrEmpty(data.upgradeName) && !_activePassives.Contains(data.upgradeName))
                {
                    _activePassives.Add(data.upgradeName);
                }
            }

            _distinctPassiveIds.Add(primaryKey);
            Debug.Log($"[PlayerPassives] Registered passive '{primaryKey}' (Total slots: {_distinctPassiveIds.Count}/{MAX_PASSIVES})");

            OnPassivesChanged?.Invoke();
        }

        public bool HasPassive(string passiveIdOrName)
        {
            if (string.IsNullOrEmpty(passiveIdOrName)) return false;
            return _activePassives.Contains(passiveIdOrName);
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
