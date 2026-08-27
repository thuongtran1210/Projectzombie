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

        private CharacterCombat _characterCombat;

        public event System.Action OnWeaponsChanged;

        private void Awake()
        {
            _playerStats = GetComponent<PlayerStats>();
            _playerPassives = GetComponent<PlayerPassives>();
            _characterCombat = GetComponent<CharacterCombat>();
        }

        private void Start()
        {
            // Kết nối sự kiện đánh tay của Hero tới Pháp Bảo Hộ Thân
            if (_characterCombat != null)
            {
                _characterCombat.OnHitEnemy += HandleHeroHitEnemy;
                _characterCombat.OnAttackExecuted += HandleHeroAttackExecuted;
            }

            ReloadEquippedWeapons();
        }

        /// <summary>
        /// Nạp lại toàn bộ Pháp Bảo / Vũ Khí theo RunLoadoutState hiện tại (dùng khi đổi trang bị ở Sảnh Chờ hoặc Xuất Trận).
        /// </summary>
        public void ReloadEquippedWeapons()
        {
            // 1. Dọn dẹp toàn bộ vũ khí / pháp bảo đang active cũ
            for (int i = _activeWeapons.Count - 1; i >= 0; i--)
            {
                if (_activeWeapons[i] != null)
                {
                    Destroy(_activeWeapons[i].gameObject);
                }
            }
            _activeWeapons.Clear();

            // Dọn dẹp thêm các WeaponBase con nếu còn sót trong weaponHolder
            Transform parent = weaponHolder != null ? weaponHolder : transform;
            var oldWeapons = parent.GetComponentsInChildren<WeaponBase>(true);
            foreach (var w in oldWeapons)
            {
                if (w != null) Destroy(w.gameObject);
            }

            // 2. Kiểm tra nếu có Loadout tùy chỉnh từ Sảnh Chờ (Meta Hub Loadout)
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

            // 3. Tìm vũ khí có sẵn trong hierarchy (để hỗ trợ tương thích ngược / test nhanh)
            WeaponBase[] attachedWeapons = GetComponentsInChildren<WeaponBase>();
            foreach (var w in attachedWeapons)
            {
                if (!_activeWeapons.Contains(w)) // Tránh add trùng lặp với vũ khí vừa sinh
                {
                    AddWeapon(w);
                }
            }

            OnWeaponsChanged?.Invoke();
            Debug.Log($"<color=#00FF88>[WeaponManager]</color> Đã nạp lại vũ khí/pháp bảo: ActiveCount={_activeWeapons.Count}");
        }

        private void OnDestroy()
        {
            if (_characterCombat != null)
            {
                _characterCombat.OnHitEnemy -= HandleHeroHitEnemy;
                _characterCombat.OnAttackExecuted -= HandleHeroAttackExecuted;
            }
        }

        private void HandleHeroHitEnemy(DamageData damageData, Collider2D enemyCol)
        {
            // Chuyển tiếp tín hiệu chém trúng quái tới tất cả Pháp Bảo đang mang
            for (int i = 0; i < _activeWeapons.Count; i++)
            {
                if (_activeWeapons[i] != null)
                {
                    _activeWeapons[i].OnHeroHitEnemy(damageData, enemyCol);
                }
            }
        }

        private void HandleHeroAttackExecuted(int comboStep)
        {
            if (comboStep == 3)
            {
                Vector2 forwardDir = transform.localScale.x >= 0 ? Vector2.right : Vector2.left;
                for (int i = 0; i < _activeWeapons.Count; i++)
                {
                    if (_activeWeapons[i] != null)
                    {
                        _activeWeapons[i].OnHeroComboFinished(comboStep, forwardDir);
                    }
                }
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
                // Pháp Bảo Dân Gian (Slapstick Relics)
                case "W_SLIPPER": return typeof(Weapon_Slipper);
                case "W_POT": return typeof(Weapon_Pot);
                case "W_PIPE": return typeof(Weapon_Pipe);
                case "R007": return typeof(Relic_SleepingMat);
                case "R008": return typeof(Relic_ChickenFeatherBroom);

                // 12 Pháp Bảo Cổ Phong Đông Sơn (W001 - W012)
                case "W001": return typeof(Weapon_Crossbow);          // Nỏ Thần
                case "W002": return typeof(Weapon_Targeted);          // Bút Phán Quan
                case "W003": return typeof(Weapon_Boomerang);         // Bùa Trấn Yêu
                case "W004": return typeof(Weapon_Flamethrower);      // Cửu Vĩ Hồ Trảo
                case "W005": return typeof(Weapon_Orbit);             // Trống Đồng Đông Sơn
                case "W006": return typeof(Weapon_GrenadeLauncher);   // Lựu Đạn Thần Sa
                case "W007": return typeof(Weapon_DirectionalTorch);   // Cung Thạch Sanh
                case "W008": return typeof(Weapon_DualSlash);          // Đao Cửu Vĩ
                case "W009": return typeof(Weapon_LightningOrb);      // Trượng Long Vương
                case "W010": return typeof(Weapon_PoisonDrone);       // Linh Phù Ma Da
                case "W011": return typeof(Weapon_HolyWater);         // Nước Thánh Chùa Hương
                case "W012": return typeof(Weapon_RandomProjectile);  // Phi Tiêu Bát Quái

                default: return null;
            }
        }

        public void AddWeapon(WeaponBase weapon)
        {
            if (!_activeWeapons.Contains(weapon))
            {
                weapon.Initialize(_playerStats);
                
                // Mọi Pháp Bảo Hộ Thân (Relic) mang vào đều là Auto-Attack / Passive Orbit
                weapon.isPrimaryActiveWeapon = false;
                if (weapon.weaponRole == WeaponRole.PrimaryWeapon)
                {
                    weapon.weaponRole = WeaponRole.RelicOrbitalShield;
                }

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
