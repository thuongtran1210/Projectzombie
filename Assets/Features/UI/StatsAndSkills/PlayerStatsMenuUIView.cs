using UnityEngine;

namespace ProjectZombie.Features.UI.StatsAndSkills
{
    public class PlayerStatsMenuUIView : MonoBehaviour
    {
        [SerializeField] private StatUIEntry _statEntryPrefab;
        [SerializeField] private Transform _statsContainer;

        // Lưu trữ để dễ cập nhật lại giá trị mà không cần xoá/tạo lại
        private StatUIEntry _damageEntry;
        private StatUIEntry _speedEntry;
        private StatUIEntry _critEntry;
        private StatUIEntry _attackSpeedEntry;
        private StatUIEntry _maxHealthEntry;
        private StatUIEntry _dashCooldownEntry;
        private StatUIEntry _pickupRangeEntry;
        private StatUIEntry _expMultiplierEntry;

        private void Awake()
        {
            // Tạo sẵn các dòng chỉ số (có thể config bằng tay trên Editor thay vì code sinh ra)
            if (_statEntryPrefab != null && _statsContainer != null)
            {
                _damageEntry = Instantiate(_statEntryPrefab, _statsContainer);
                _speedEntry = Instantiate(_statEntryPrefab, _statsContainer);
                _critEntry = Instantiate(_statEntryPrefab, _statsContainer);
                _attackSpeedEntry = Instantiate(_statEntryPrefab, _statsContainer);
                _maxHealthEntry = Instantiate(_statEntryPrefab, _statsContainer);
                _dashCooldownEntry = Instantiate(_statEntryPrefab, _statsContainer);
                _pickupRangeEntry = Instantiate(_statEntryPrefab, _statsContainer);
                _expMultiplierEntry = Instantiate(_statEntryPrefab, _statsContainer);
            }
        }

        public void UpdateDamage(string formattedValue)
        {
            if (_damageEntry != null) _damageEntry.Setup("Damage", formattedValue);
        }

        public void UpdateSpeed(string formattedValue)
        {
            if (_speedEntry != null) _speedEntry.Setup("Move Speed", formattedValue);
        }

        public void UpdateCrit(string formattedValue)
        {
            if (_critEntry != null) _critEntry.Setup("Crit Chance", formattedValue);
        }

        public void UpdateAttackSpeed(string formattedValue)
        {
            if (_attackSpeedEntry != null) _attackSpeedEntry.Setup("Attack Speed", formattedValue);
        }

        public void UpdateMaxHealth(string formattedValue)
        {
            if (_maxHealthEntry != null) _maxHealthEntry.Setup("Max Health", formattedValue);
        }

        public void UpdateDashCooldown(string formattedValue)
        {
            if (_dashCooldownEntry != null) _dashCooldownEntry.Setup("Dash Cooldown", formattedValue);
        }

        public void UpdatePickupRange(string formattedValue)
        {
            if (_pickupRangeEntry != null) _pickupRangeEntry.Setup("Pickup Range", formattedValue);
        }

        public void UpdateExpMultiplier(string formattedValue)
        {
            if (_expMultiplierEntry != null) _expMultiplierEntry.Setup("Exp Bonus", formattedValue);
        }
    }
}
