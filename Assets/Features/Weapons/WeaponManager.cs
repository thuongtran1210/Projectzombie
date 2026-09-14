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

        [Tooltip("Prefab dự phòng an toàn nếu WeaponData bị thiếu weaponPrefab (Tránh crash game trên mobile)")]
        [SerializeField] private WeaponBase defaultFallbackWeaponPrefab;

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

            // 1. Khởi tạo chuẩn Data-Driven từ Prefab
            if (data.weaponPrefab != null)
            {
                newWeapon = Instantiate(data.weaponPrefab, parent);
            }
            else
            {
                // 2. Fallback phòng vệ (Defensive Fallback)
                Debug.LogError($"<color=#FF4444>[WeaponManager] LỖI CẤU HÌNH:</color> WeaponData '<b>{data.weaponName}</b>' (ID: {data.weaponId}) chưa được gán weaponPrefab!");
                
                if (defaultFallbackWeaponPrefab != null)
                {
                    Debug.LogWarning($"[WeaponManager] Đang sử dụng defaultFallbackWeaponPrefab cho '{data.weaponName}' để bảo đảm trận đấu không bị crash.");
                    newWeapon = Instantiate(defaultFallbackWeaponPrefab, parent);
                }
                else
                {
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

                // Đồng bộ cấu hình Hybrid Relic (v6.0)
                newWeapon.isPassiveRelic = data.isPassiveRelic;
                newWeapon.activeCooldown = data.activeCooldown;
                newWeapon.activeDuration = data.activeDuration;
                newWeapon.skillActionName = data.skillActionName;
                
                AddWeapon(newWeapon);
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
        /// Pháp bảo hộ thân duy nhất đang trang bị.
        /// </summary>
        public WeaponBase EquippedRelic => _activeWeapons.Count > 0 ? _activeWeapons[0] : null;

        /// <summary>
        /// Kiểm tra xem người chơi có đang trang bị Pháp Bảo Chủ Động (Active Relic) hay không.
        /// </summary>
        public bool HasActiveRelic(out WeaponBase activeRelic)
        {
            for (int i = 0; i < _activeWeapons.Count; i++)
            {
                var w = _activeWeapons[i];
                if (w != null && !w.isPassiveRelic && !w.isPrimaryActiveWeapon)
                {
                    activeRelic = w;
                    return true;
                }
            }
            activeRelic = null;
            return false;
        }

        /// <summary>
        /// Kích hoạt Kỹ năng Chủ Động của Pháp Bảo Hộ Thân với đầy đủ dữ liệu ngắm chiêu MOBA (AimResult).
        /// </summary>
        public bool TriggerEquippedRelicSkill(Combat.Aiming.AimResult aimResult)
        {
            if (HasActiveRelic(out var activeRelic))
            {
                return activeRelic.TriggerActiveRelicSkill(aimResult);
            }
            return false;
        }

        /// <summary>
        /// Kích hoạt Kỹ năng Chủ Động của Pháp Bảo Hộ Thân khi người chơi nhấn nút Kỹ Năng Pháp Bảo / Phím E. Hỗ trợ ngắm bắn định hướng MOBA.
        /// </summary>
        public bool TriggerEquippedRelicSkill(Vector2 customAimDirection = default)
        {
            if (HasActiveRelic(out var activeRelic))
            {
                return activeRelic.TriggerActiveRelicSkill(customAimDirection);
            }
            return false;
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
            // Chỉ tick vũ khí/pháp bảo khi trận đấu đang thực sự diễn ra (không Pause, không LevelUp, không MainMenu)
            if (!GameStateManager.IsPlaying) return;

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
