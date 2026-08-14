using UnityEngine;
using System.Collections.Generic;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Weapons
{
    using ProjectZombie.Features.Shared;
    using ProjectZombie.Features.Upgrades;

    /// <summary>
    /// Ba lô chứa vũ khí của Player. Quản lý việc gọi Tick() cho tất cả vũ khí đang sở hữu.
    /// </summary>
    [RequireComponent(typeof(PlayerStats))]
    public class WeaponManager : MonoBehaviour
    {
        [Header("Data-Driven Loadout (Hướng 2)")]
        [Tooltip("Danh sách các vũ khí khởi điểm của nhân vật")]
        [SerializeField] private List<WeaponData> startingLoadout = new List<WeaponData>();

        [Tooltip("Transform chứa các vũ khí được sinh ra (Nếu để trống sẽ dùng transform của Player)")]
        [SerializeField] private Transform weaponHolder;

        private PlayerStats _playerStats;
        private PlayerPassives _playerPassives;
        private List<WeaponBase> _activeWeapons = new List<WeaponBase>();

        // Quản lý combo Tương Sinh Ngũ Hành (Kim -> Thủy -> Mộc -> Hỏa -> Thổ -> Kim)
        private ElementType _lastHitElement = ElementType.None;
        private float _lastHitTime;
        private const float COMBO_WINDOW_SECONDS = 3.0f;

        public IReadOnlyList<WeaponBase> ActiveWeapons => _activeWeapons;

        public event System.Action OnWeaponsChanged;

        /// <summary>
        /// Ghi nhận đòn đánh trúng mục tiêu với Element tương ứng và trả về hệ số Cooldown Reduction (0.8f nếu Tương Sinh).
        /// </summary>
        public float RecordElementHitAndGetCooldownMultiplier(ElementType currentElement)
        {
            if (currentElement == ElementType.None)
                return 1.0f;

            bool isSynergy = false;
            if (Time.time - _lastHitTime <= COMBO_WINDOW_SECONDS)
            {
                isSynergy = (_lastHitElement == ElementType.Kim && currentElement == ElementType.Thuy) ||
                            (_lastHitElement == ElementType.Thuy && currentElement == ElementType.Moc) ||
                            (_lastHitElement == ElementType.Moc && currentElement == ElementType.Hoa) ||
                            (_lastHitElement == ElementType.Hoa && currentElement == ElementType.Tho) ||
                            (_lastHitElement == ElementType.Tho && currentElement == ElementType.Kim);
            }

            _lastHitElement = currentElement;
            _lastHitTime = Time.time;

            return isSynergy ? 0.8f : 1.0f; // -20% cooldown khi kích hoạt Tương Sinh
        }

        private void Awake()
        {
            _playerStats = GetComponent<PlayerStats>();
            _playerPassives = GetComponent<PlayerPassives>();
        }

        private void Start()
        {
            // 1. Sinh ra vũ khí từ Data-Driven Loadout (Hướng 2)
            foreach (var weaponData in startingLoadout)
            {
                EquipWeaponFromData(weaponData);
            }

            // 2. Tìm vũ khí có sẵn trong hierarchy (để hỗ trợ tương thích ngược / test nhanh)
            WeaponBase[] attachedWeapons = GetComponentsInChildren<WeaponBase>();
            foreach (var w in attachedWeapons)
            {
                if (!_activeWeapons.Contains(w)) // Tránh add trùng lặp với vũ khí vừa sinh
                {
                    AddWeapon(w);
                }
            }
        }

        public void EquipWeaponFromData(WeaponData data)
        {
            if (data == null || data.weaponPrefab == null)
            {
                Debug.LogWarning("[WeaponManager] WeaponData or WeaponPrefab is null!");
                return;
            }

            // Sinh ra Prefab
            Transform parent = weaponHolder != null ? weaponHolder : transform;
            WeaponBase newWeapon = Instantiate(data.weaponPrefab, parent);
            
            if (newWeapon != null)
            {
                if (string.IsNullOrEmpty(newWeapon.weaponId)) newWeapon.weaponId = data.weaponId;
                if (string.IsNullOrEmpty(newWeapon.displayName)) newWeapon.displayName = data.weaponName;
                if (newWeapon.icon == null) newWeapon.icon = data.icon;
                if (string.IsNullOrEmpty(newWeapon.description)) newWeapon.description = data.description;
            }
            
            AddWeapon(newWeapon);
        }

        public void AddWeapon(WeaponBase weapon)
        {
            if (!_activeWeapons.Contains(weapon))
            {
                weapon.Initialize(_playerStats);
                _activeWeapons.Add(weapon);
                OnWeaponsChanged?.Invoke();
            }
        }

        public void NotifyWeaponsChanged()
        {
            OnWeaponsChanged?.Invoke();
        }

        public WeaponBase GetWeaponById(string id)
        {
            return _activeWeapons.Find(w => w.weaponId == id);
        }

        public void RemoveWeapon(WeaponBase weapon)
        {
            if (_activeWeapons.Contains(weapon))
            {
                _activeWeapons.Remove(weapon);
                Destroy(weapon.gameObject);
                NotifyWeaponsChanged();
            }
        }

        private void Update()
        {
            // Cho phép tất cả vũ khí hoạt động
            foreach (var weapon in _activeWeapons)
            {
                weapon.Tick();
            }
        }
    }
}
