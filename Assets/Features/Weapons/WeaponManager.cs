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

        private readonly List<WeaponBase> _cachedRelicWeapons = new List<WeaponBase>();

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
                for (int i = 0; i < _activeWeapons.Count; i++)
                {
                    if (_activeWeapons[i] != null && _activeWeapons[i].isPrimaryActiveWeapon)
                        return _activeWeapons[i];
                }
                return _activeWeapons[0];
            }
        }

        /// <summary>
        /// Danh sách các Pháp bảo phụ trợ (Relics) đang trang bị (Zero-GC Cache).
        /// </summary>
        public IReadOnlyList<WeaponBase> RelicWeapons => _cachedRelicWeapons;

        private void RebuildRelicCache()
        {
            _cachedRelicWeapons.Clear();
            var primary = PrimaryWeapon;
            for (int i = 0; i < _activeWeapons.Count; i++)
            {
                var w = _activeWeapons[i];
                if (w != null && w != primary)
                {
                    _cachedRelicWeapons.Add(w);
                }
            }

            // Nếu không có Primary Weapon riêng (dùng CharacterCombat), mọi vũ khí trong này đều là Relic
            if (_cachedRelicWeapons.Count == 0 && _activeWeapons.Count > 0 && GetComponent<CharacterCombat>() != null)
            {
                for (int i = 0; i < _activeWeapons.Count; i++)
                {
                    if (_activeWeapons[i] != null) _cachedRelicWeapons.Add(_activeWeapons[i]);
                }
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

            RebuildRelicCache();
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

        private static readonly Dictionary<string, System.Type> _weaponTypeRegistry = new Dictionary<string, System.Type>(System.StringComparer.OrdinalIgnoreCase)
        {
            { "W_SLIPPER", typeof(Weapon_Slipper) },
            { "W_POT", typeof(Weapon_Pot) },
            { "W_PIPE", typeof(Weapon_Pipe) },
            { "R007", typeof(Relic_SleepingMat) },
            { "R008", typeof(Relic_ChickenFeatherBroom) },
            { "W001", typeof(Weapon_Crossbow) },
            { "W002", typeof(Weapon_Targeted) },
            { "W003", typeof(Weapon_Boomerang) },
            { "W004", typeof(Weapon_Flamethrower) },
            { "W005", typeof(Weapon_Orbit) },
            { "W006", typeof(Weapon_GrenadeLauncher) },
            { "W007", typeof(Weapon_DirectionalTorch) },
            { "W008", typeof(Weapon_DualSlash) },
            { "W009", typeof(Weapon_LightningOrb) },
            { "W010", typeof(Weapon_PoisonDrone) },
            { "W011", typeof(Weapon_HolyWater) },
            { "W012", typeof(Weapon_RandomProjectile) }
        };

        /// <summary>
        /// Đăng ký Type vũ khí mới vào Registry động (phục vụ mở rộng từ plugin / DLC / modding).
        /// </summary>
        public static void RegisterWeaponType(string weaponId, System.Type weaponType)
        {
            if (!string.IsNullOrEmpty(weaponId) && weaponType != null)
            {
                _weaponTypeRegistry[weaponId] = weaponType;
            }
        }

        private System.Type GetWeaponTypeById(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId)) return null;

            // 1. Tìm trong Registry đã đăng ký
            if (_weaponTypeRegistry.TryGetValue(weaponId, out var registeredType))
            {
                return registeredType;
            }

            // 2. Tự động suy luận Type qua Reflection (Auto-Discovery)
            string[] candidateTypeNames = new[]
            {
                $"ProjectZombie.Features.Weapons.Weapon_{weaponId}, Assembly-CSharp",
                $"ProjectZombie.Features.Weapons.Relic_{weaponId}, Assembly-CSharp",
                $"ProjectZombie.Features.Weapons.{weaponId}, Assembly-CSharp"
            };

            foreach (var typeName in candidateTypeNames)
            {
                var foundType = System.Type.GetType(typeName, false, true);
                if (foundType != null && typeof(WeaponBase).IsAssignableFrom(foundType))
                {
                    _weaponTypeRegistry[weaponId] = foundType;
                    return foundType;
                }
            }

            return null;
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
            RebuildRelicCache();
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

            // Cho phép tất cả vũ khí hoạt động (Duyệt ngược và lọc bỏ các vũ khí đã bị Destroy)
            for (int i = _activeWeapons.Count - 1; i >= 0; i--)
            {
                var weapon = _activeWeapons[i];
                if (weapon == null)
                {
                    _activeWeapons.RemoveAt(i);
                    continue;
                }

                weapon.Tick();
            }
        }
    }
}
