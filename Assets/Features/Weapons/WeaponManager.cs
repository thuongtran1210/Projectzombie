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

        public const int MAX_WEAPONS = 1; // Tối đa 1 Pháp bảo hộ thân mang vào trận

        private PlayerStats _playerStats;
        private PlayerPassives _playerPassives;
        private List<WeaponBase> _activeWeapons = new List<WeaponBase>();

        public IReadOnlyList<WeaponBase> ActiveWeapons => _activeWeapons;
        public bool IsFull() => _activeWeapons.Count >= MAX_WEAPONS;

        /// <summary>
        /// Vũ khí chính dùng để đánh chủ động qua nút Tấn Công.
        /// Mặc định là vũ khí đầu tiên trong danh sách hoặc vũ khí có cờ isPrimaryActiveWeapon = true.
        /// </summary>
        public WeaponBase PrimaryWeapon
        {
            get
            {
                if (_activeWeapons.Count == 0) return null;
                var found = _activeWeapons.Find(w => w.isPrimaryActiveWeapon);
                return found != null ? found : _activeWeapons[0];
            }
        }

        /// <summary>
        /// Danh sách các Pháp bảo phụ trợ (Relics) đang trang bị.
        /// </summary>
        public List<WeaponBase> RelicWeapons
        {
            get
            {
                var relics = new List<WeaponBase>();
                var primary = PrimaryWeapon;
                foreach (var w in _activeWeapons)
                {
                    if (w != primary) relics.Add(w);
                }
                // Nếu không có Primary Weapon riêng (dùng CharacterCombat), mọi vũ khí trong này đều là Relic
                if (relics.Count == 0 && _activeWeapons.Count > 0 && GetComponent<CharacterCombat>() != null)
                {
                    return new List<WeaponBase>(_activeWeapons);
                }
                return relics;
            }
        }

        public int CurrentPrimaryComboStep => PrimaryWeapon != null ? PrimaryWeapon.CurrentComboStep : 1;

        public event System.Action OnWeaponsChanged;

        private void Awake()
        {
            _playerStats = GetComponent<PlayerStats>();
            _playerPassives = GetComponent<PlayerPassives>();
        }

        private void Start()
        {
            // 1. Kiểm tra nếu có Loadout tùy chỉnh từ Sảnh Chờ (Meta Hub Loadout)
            if (RunLoadoutState.HasCustomLoadout)
            {
                // Nạp 1 Pháp Bảo Hộ Thân (Relic)
                if (RunLoadoutState.SelectedRelic != null)
                {
                    EquipWeaponFromData(RunLoadoutState.SelectedRelic, isPrimary: false);
                }
                else if (RunLoadoutState.SelectedRelics != null && RunLoadoutState.SelectedRelics.Count > 0)
                {
                    EquipWeaponFromData(RunLoadoutState.SelectedRelics[0], isPrimary: false);
                }
            }
            else
            {
                // Fallback: Sinh ra vũ khí từ startingLoadout (hoặc hierarchy)
                foreach (var weaponData in startingLoadout)
                {
                    if (_activeWeapons.Count >= MAX_WEAPONS) break;
                    EquipWeaponFromData(weaponData);
                }
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

            // Nếu có ít nhất 1 vũ khí và chưa có vũ khí nào được đánh dấu là primary, đặt vũ khí đầu tiên làm primary
            if (_activeWeapons.Count > 0 && !_activeWeapons.Exists(w => w.isPrimaryActiveWeapon))
            {
                _activeWeapons[0].isPrimaryActiveWeapon = true;
            }
        }

        public void EquipWeaponFromData(WeaponData data, bool isPrimary = false)
        {
            if (data == null) return;

            Transform parent = weaponHolder != null ? weaponHolder : transform;
            WeaponBase newWeapon = null;

            // 1. Khởi tạo từ Prefab nếu có
            if (data.weaponPrefab != null)
            {
                newWeapon = Instantiate(data.weaponPrefab, parent);
            }
            else
            {
                // 2. Tự động Fallback: Tìm script tương ứng theo ID để gắn component động
                System.Type weaponType = GetWeaponTypeById(data.weaponId);
                if (weaponType != null)
                {
                    GameObject weaponObj = new GameObject($"Weapon_{data.weaponId}");
                    weaponObj.transform.SetParent(parent, false);
                    newWeapon = weaponObj.AddComponent(weaponType) as WeaponBase;
                }
                else
                {
                    Debug.LogWarning($"[WeaponManager] WeaponData '{data.weaponName}' (ID: {data.weaponId}) chưa được gán weaponPrefab hoặc Script tương ứng!");
                    return;
                }
            }
            
            if (newWeapon != null)
            {
                if (string.IsNullOrEmpty(newWeapon.weaponId)) newWeapon.weaponId = data.weaponId;
                if (string.IsNullOrEmpty(newWeapon.displayName)) newWeapon.displayName = data.weaponName;
                if (newWeapon.icon == null) newWeapon.icon = data.icon;
                if (string.IsNullOrEmpty(newWeapon.description)) newWeapon.description = data.description;
                if (isPrimary) newWeapon.isPrimaryActiveWeapon = true;
                
                AddWeapon(newWeapon);
            }
        }

        private System.Type GetWeaponTypeById(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId)) return null;

            switch (weaponId.ToUpper())
            {
                case "W_SLIPPER": return typeof(Weapon_Slipper);
                case "W_POT": return typeof(Weapon_Pot);
                case "W_PIPE": return typeof(Weapon_Pipe);
                case "R007": return typeof(Relic_SleepingMat);
                case "R008": return typeof(Relic_ChickenFeatherBroom);
                default: return null;
            }
        }

        public void AddWeapon(WeaponBase weapon)
        {
            if (!_activeWeapons.Contains(weapon))
            {
                weapon.Initialize(_playerStats);
                _activeWeapons.Add(weapon);

                // Nếu đây là vũ khí đầu tiên, mặc định gán làm vũ khí chính
                if (_activeWeapons.Count == 1)
                {
                    weapon.isPrimaryActiveWeapon = true;
                }

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

                // Nếu vừa xóa vũ khí chính, chuyển vũ khí tiếp theo làm chính
                if (_activeWeapons.Count > 0 && !_activeWeapons.Exists(w => w.isPrimaryActiveWeapon))
                {
                    _activeWeapons[0].isPrimaryActiveWeapon = true;
                }

                NotifyWeaponsChanged();
            }
        }

        /// <summary>
        /// Kích hoạt đòn tấn công chủ động của Vũ Khí Chính khi người chơi nhấn Nút Đánh.
        /// </summary>
        public bool TriggerPrimaryAttack()
        {
            WeaponBase primary = PrimaryWeapon;
            if (primary != null)
            {
                return primary.TriggerActiveAttack();
            }
            return false;
        }

        private void Update()
        {
            // Nếu đang ở MainMenu (Sảnh Chờ / Chọn Tướng), tạm dừng auto-tick vũ khí để nhân vật ở sảnh đứng yên tạo dáng sạch sẽ
            if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentState == GameState.MainMenu)
            {
                return;
            }

            // Cho phép tất cả vũ khí hoạt động (Vũ khí chủ động sẽ tự bỏ qua trong Tick)
            foreach (var weapon in _activeWeapons)
            {
                weapon.Tick();
            }
        }
    }
}
