using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectZombie.Features.Defense3D.Loadout
{
    /// <summary>
    /// Bản nháp lựa chọn Loadout của người chơi (Mutable Draft Selection).
    /// Chỉ lưu trữ định danh chuỗi (ID), không tham chiếu UnityEngine.Object hoặc GameObject.
    /// Phát sự kiện OnChanged khi có thay đổi hợp lệ.
    /// </summary>
    public sealed class LoadoutSelection
    {
        private string _heroId = string.Empty;
        private string _relicId = string.Empty;
        private readonly List<string> _minionIds = new List<string>();
        private readonly List<string> _towerIds = new List<string>();

        public event Action OnChanged;

        public string HeroId => _heroId;
        public string RelicId => _relicId;
        public IReadOnlyList<string> MinionIds => _minionIds;
        public IReadOnlyList<string> TowerIds => _towerIds;

        public LoadoutSelection() { }

        public LoadoutSelection(string heroId, string relicId, IEnumerable<string> minionIds = null, IEnumerable<string> towerIds = null)
        {
            _heroId = heroId ?? string.Empty;
            _relicId = relicId ?? string.Empty;
            if (minionIds != null) _minionIds.AddRange(minionIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.OrdinalIgnoreCase));
            if (towerIds != null) _towerIds.AddRange(towerIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.OrdinalIgnoreCase));
        }

        public bool SetHero(string heroId)
        {
            string normalized = heroId ?? string.Empty;
            if (string.Equals(_heroId, normalized, StringComparison.OrdinalIgnoreCase))
                return false;

            _heroId = normalized;
            OnChanged?.Invoke();
            return true;
        }

        public bool SetRelic(string relicId)
        {
            string normalized = relicId ?? string.Empty;
            if (string.Equals(_relicId, normalized, StringComparison.OrdinalIgnoreCase))
                return false;

            _relicId = normalized;
            OnChanged?.Invoke();
            return true;
        }

        public bool AddMinion(string minionId)
        {
            if (string.IsNullOrWhiteSpace(minionId)) return false;
            if (_minionIds.Contains(minionId, StringComparer.OrdinalIgnoreCase)) return false;

            _minionIds.Add(minionId);
            OnChanged?.Invoke();
            return true;
        }

        public bool RemoveMinion(string minionId)
        {
            int index = _minionIds.FindIndex(id => string.Equals(id, minionId, StringComparison.OrdinalIgnoreCase));
            if (index < 0) return false;

            _minionIds.RemoveAt(index);
            OnChanged?.Invoke();
            return true;
        }

        /// <summary>Replaces one occupied minion position without changing order or count.</summary>
        public bool ReplaceMinion(string previousId, string nextId) => Replace(_minionIds, previousId, nextId);

        /// <summary>Replaces one occupied tower position without changing order or count.</summary>
        public bool ReplaceTower(string previousId, string nextId) => Replace(_towerIds, previousId, nextId);

        private bool Replace(List<string> ids, string previousId, string nextId)
        {
            if (string.IsNullOrWhiteSpace(nextId) || ids.Contains(nextId, StringComparer.OrdinalIgnoreCase)) return false;
            int index = ids.FindIndex(id => string.Equals(id, previousId, StringComparison.OrdinalIgnoreCase));
            if (index < 0) return false;
            ids[index] = nextId;
            OnChanged?.Invoke();
            return true;
        }

        public bool SetMinions(IEnumerable<string> minionIds)
        {
            var newList = minionIds?.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
                          ?? new List<string>();

            if (_minionIds.SequenceEqual(newList, StringComparer.OrdinalIgnoreCase))
                return false;

            _minionIds.Clear();
            _minionIds.AddRange(newList);
            OnChanged?.Invoke();
            return true;
        }

        public bool AddTower(string towerId)
        {
            if (string.IsNullOrWhiteSpace(towerId)) return false;
            if (_towerIds.Contains(towerId, StringComparer.OrdinalIgnoreCase)) return false;

            _towerIds.Add(towerId);
            OnChanged?.Invoke();
            return true;
        }

        public bool RemoveTower(string towerId)
        {
            int index = _towerIds.FindIndex(id => string.Equals(id, towerId, StringComparison.OrdinalIgnoreCase));
            if (index < 0) return false;

            _towerIds.RemoveAt(index);
            OnChanged?.Invoke();
            return true;
        }

        public bool SetTowers(IEnumerable<string> towerIds)
        {
            var newList = towerIds?.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
                          ?? new List<string>();

            if (_towerIds.SequenceEqual(newList, StringComparer.OrdinalIgnoreCase))
                return false;

            _towerIds.Clear();
            _towerIds.AddRange(newList);
            OnChanged?.Invoke();
            return true;
        }

        public void Clear()
        {
            bool hadContent = !string.IsNullOrEmpty(_heroId) || !string.IsNullOrEmpty(_relicId) || _minionIds.Count > 0 || _towerIds.Count > 0;
            _heroId = string.Empty;
            _relicId = string.Empty;
            _minionIds.Clear();
            _towerIds.Clear();

            if (hadContent)
                OnChanged?.Invoke();
        }

        public LoadoutSelection Clone()
        {
            return new LoadoutSelection(_heroId, _relicId, _minionIds, _towerIds);
        }

        public DefenseLoadoutSnapshot ToSnapshot()
        {
            return DefenseLoadoutSnapshot.Create(this);
        }
    }
}
