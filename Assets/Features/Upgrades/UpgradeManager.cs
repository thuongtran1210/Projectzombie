using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Upgrades.Filters;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Shared;
using System.Linq;

namespace ProjectZombie.Features.Upgrades
{
    /// <summary>
    /// Quản lý danh sách tất cả các nâng cấp có thể có trong game (Pool).
    /// Cung cấp các lựa chọn ngẫu nhiên khi người chơi lên cấp thông qua hệ thống Filter Strategy Pattern
    /// và Dynamic Synergy Weight Pipeline (Ưu tiên nâng cấp đồ đang sở hữu + Tương sinh Ngũ Hành).
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        [Header("Upgrade Pool")]
        [SerializeField] private List<UpgradeData> _allAvailableUpgrades = new List<UpgradeData>();

        [Header("Fallback Rewards (Khi cạn pool)")]
        [SerializeField] private List<UpgradeData> _fallbackRewards = new List<UpgradeData>();

        private readonly HashSet<UpgradeData> _bannedUpgrades = new HashSet<UpgradeData>();
        private readonly List<IUpgradeFilter> _filters = new List<IUpgradeFilter>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            InitDefaultFilters();
            InitDefaultFallbackRewards();
            AutoPopulateUpgradesIfEmpty();
        }

        private void InitDefaultFilters()
        {
            _filters.Clear();
            _filters.Add(new BannedUpgradeFilter(_bannedUpgrades));
            _filters.Add(new AvailabilityUpgradeFilter());
            _filters.Add(new YinYangUpgradeFilter());
        }

        private void InitDefaultFallbackRewards()
        {
            if (_fallbackRewards == null || _fallbackRewards.Count == 0)
            {
                var healCard = ScriptableObject.CreateInstance<FallbackRewardUpgradeData>();
                healCard.id = "FB_HEAL";
                healCard.upgradeName = "Tiên Đan Hồi Máu";
                healCard.description = "Hồi phục ngay lập tức <color=#00FF88>40% Máu tối đa</color>.";
                healCard.upgradeType = UpgradeType.CommonUpgrade;
                healCard.rewardType = FallbackRewardType.HealHealth;
                healCard.healPercentage = 0.40f;

                var goldCard = ScriptableObject.CreateInstance<FallbackRewardUpgradeData>();
                goldCard.id = "FB_GOLD";
                goldCard.upgradeName = "Túi Vàng Phong Thủy";
                goldCard.description = "Thu thập ngân lượng tăng thêm <color=#FFD700>+150 Vàng</color>.";
                goldCard.upgradeType = UpgradeType.CommonUpgrade;
                goldCard.rewardType = FallbackRewardType.GrantGold;
                goldCard.goldAmount = 150;

                _fallbackRewards = new List<UpgradeData> { healCard, goldCard };
            }
        }

        public void RegisterFilter(IUpgradeFilter filter)
        {
            if (filter != null && !_filters.Contains(filter))
            {
                _filters.Add(filter);
            }
        }

        public void RemoveFilter(IUpgradeFilter filter)
        {
            if (filter != null)
            {
                _filters.Remove(filter);
            }
        }

        public void AutoPopulateUpgradesIfEmpty()
        {
            if (_allAvailableUpgrades == null || _allAvailableUpgrades.Count == 0)
            {
                PopulateAllAvailableUpgrades();
            }
        }

        [ContextMenu("Populate All Upgrades")]
        public void PopulateAllAvailableUpgrades()
        {
            _allAvailableUpgrades.Clear();

#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:UpgradeData");
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var upgrade = UnityEditor.AssetDatabase.LoadAssetAtPath<UpgradeData>(path);
                if (upgrade != null && !_allAvailableUpgrades.Contains(upgrade) && !(upgrade is FallbackRewardUpgradeData))
                {
                    _allAvailableUpgrades.Add(upgrade);
                }
            }
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log($"[UpgradeManager] Tự động nạp {_allAvailableUpgrades.Count} thẻ UpgradeData từ dự án.");
#else
            var loadedUpgrades = Resources.LoadAll<UpgradeData>("");
            foreach (var u in loadedUpgrades)
            {
                if (u != null && !(u is FallbackRewardUpgradeData))
                {
                    _allAvailableUpgrades.Add(u);
                }
            }
            Debug.Log($"[UpgradeManager] Load {_allAvailableUpgrades.Count} thẻ UpgradeData từ Resources.");
#endif
        }

        public void BanUpgrade(UpgradeData upgrade)
        {
            if (upgrade != null && !_bannedUpgrades.Contains(upgrade))
            {
                _bannedUpgrades.Add(upgrade);
                Debug.Log($"[UpgradeManager] Banned upgrade: {upgrade.upgradeName}");
            }
        }

        public void ResetBannedUpgrades()
        {
            _bannedUpgrades.Clear();
        }

        public bool IsBanned(UpgradeData upgrade)
        {
            return upgrade != null && _bannedUpgrades.Contains(upgrade);
        }

        private bool IsUpgradeAllowed(UpgradeData upgrade, GameObject player)
        {
            if (upgrade == null) return false;
            for (int i = 0; i < _filters.Count; i++)
            {
                if (!_filters[i].IsAllowed(upgrade, player))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Tính toán trọng số xuất hiện động (Dynamic Synergy Weight):
        /// - Đồ đang có trong ba lô: Nhân 2.5x
        /// - Thẻ Tiến Hóa: Nhân 4.0x
        /// - Cùng hệ hoặc Tương Sinh Ngũ Hành: Thêm +35%
        /// </summary>
        private float CalculateEffectiveWeight(
            UpgradeData upgrade, 
            GameObject player, 
            WeaponManager weaponManager, 
            PlayerPassives playerPassives,
            HashSet<ElementType> activeElements)
        {
            float weight = Mathf.Max(1f, upgrade.spawnWeight);

            // 1. Phân loại theo thể loại thẻ
            if (upgrade is WeaponUpgradeData wud)
            {
                if (wud.requiredCurrentLevel > 0)
                {
                    // Thẻ Nâng cấp vũ khí đang có -> ƯU TIÊN CAO ĐỘ (2.5x)
                    weight *= 2.5f;
                }
                else
                {
                    // Thẻ Mở khóa vũ khí mới -> Trọng số gốc
                    weight *= 1.0f;
                }
            }
            else if (upgrade is EvolutionUpgradeData)
            {
                // Thẻ Tiến Hóa khi đã đủ điều kiện -> SIÊU ƯU TIÊN (4.0x)
                weight *= 4.0f;
            }
            else if (upgrade is CommonUpgradeData cud)
            {
                if (playerPassives != null && (playerPassives.HasPassive(cud.id) || playerPassives.HasPassive(cud.upgradeName)))
                {
                    // Đã sở hữu Passive này -> Ưu tiên nâng max (2.0x)
                    weight *= 2.0f;
                }
            }

            // 2. Cộng hưởng Ngũ Hành (Element Synergy Bonus)
            if (upgrade.element != ElementType.None && activeElements != null && activeElements.Count > 0)
            {
                if (activeElements.Contains(upgrade.element))
                {
                    // Đồng Hệ (Cùng nguyên tố) -> +35%
                    weight *= 1.35f;
                }
                else
                {
                    // Tương Sinh (Thủy sinh Mộc, Mộc sinh Hỏa,...)
                    foreach (var activeElem in activeElements)
                    {
                        if (IsElementGenerative(activeElem, upgrade.element))
                        {
                            weight *= 1.25f;
                            break;
                        }
                    }
                }
            }

            // 3. Ưu đãi trọng số cho thẻ Âm Dương khi Cán Cân đang ở đúng trạng thái (Dành cho Đạo Sĩ)
            if (upgrade.checkYinYangState && YinYang.YinYangManager.Instance != null && YinYang.YinYangManager.Instance.IsTrackerActive)
            {
                if (upgrade.requiredYinYangState == YinYang.YinYangManager.Instance.GetState())
                {
                    weight *= 1.5f; // Tăng thêm 50% tỉ lệ xuất hiện thẻ độc quyền theo trạng thái Âm/Dương
                }
            }

            return weight;
        }

        private bool IsElementGenerative(ElementType parent, ElementType child)
        {
            // Kim(1) sinh Thủy(3), Thủy(3) sinh Mộc(2), Mộc(2) sinh Hỏa(4), Hỏa(4) sinh Thổ(5), Thổ(5) sinh Kim(1)
            switch (parent)
            {
                case ElementType.Kim: return child == ElementType.Thuy;
                case ElementType.Thuy: return child == ElementType.Moc;
                case ElementType.Moc: return child == ElementType.Hoa;
                case ElementType.Hoa: return child == ElementType.Tho;
                case ElementType.Tho: return child == ElementType.Kim;
                default: return false;
            }
        }

        /// <summary>
        /// Trả về danh sách nâng cấp ngẫu nhiên đã qua thuật toán cân bằng thông minh.
        /// </summary>
        public List<UpgradeData> GetRandomUpgrades(int count, GameObject player)
        {
            var weaponManager = player != null ? player.GetComponent<WeaponManager>() : null;
            var playerPassives = player != null ? player.GetComponent<PlayerPassives>() : null;

            // Thu thập các nguyên tố Ngũ Hành mà người chơi đang sở hữu
            var activeElements = new HashSet<ElementType>();
            if (weaponManager != null)
            {
                for (int i = 0; i < weaponManager.ActiveWeapons.Count; i++)
                {
                    var w = weaponManager.ActiveWeapons[i];
                    if (w != null && w.element != ElementType.None)
                    {
                        activeElements.Add(w.element);
                    }
                }
            }

            var validUpgrades = new List<UpgradeData>();
            var weights = new List<float>();
            float totalWeight = 0f;

            // 1. Lọc thẻ hợp lệ & tính trọng số động
            for (int i = 0; i < _allAvailableUpgrades.Count; i++)
            {
                var u = _allAvailableUpgrades[i];
                if (IsUpgradeAllowed(u, player))
                {
                    float effectiveWeight = CalculateEffectiveWeight(u, player, weaponManager, playerPassives, activeElements);
                    validUpgrades.Add(u);
                    weights.Add(effectiveWeight);
                    totalWeight += effectiveWeight;
                }
            }

            var selectedUpgrades = new List<UpgradeData>();
            bool hasNewWeaponUnlockSelected = false;

            // 2. Thuật toán Weighted Random có kiểm soát (Max 1 New Weapon Unlock)
            while (selectedUpgrades.Count < count && validUpgrades.Count > 0 && totalWeight > 0f)
            {
                float randomValue = Random.Range(0f, totalWeight);
                float currentSum = 0f;

                for (int i = 0; i < validUpgrades.Count; i++)
                {
                    currentSum += weights[i];
                    if (currentSum >= randomValue)
                    {
                        var chosen = validUpgrades[i];

                        // Nếu đã chọn 1 thẻ Mở Khóa Mới rồi thì không cho phép chọn thêm thẻ Mở Khóa Mới thứ 2
                        bool isNewWeaponUnlock = (chosen is WeaponUpgradeData wud) && wud.requiredCurrentLevel == 0;
                        if (isNewWeaponUnlock && hasNewWeaponUnlockSelected)
                        {
                            // Loại bỏ thẻ mở khóa này khỏi pool roll hiện tại để tìm thẻ khác
                            totalWeight -= weights[i];
                            validUpgrades.RemoveAt(i);
                            weights.RemoveAt(i);
                            break;
                        }

                        if (isNewWeaponUnlock)
                        {
                            hasNewWeaponUnlockSelected = true;
                        }

                        selectedUpgrades.Add(chosen);
                        totalWeight -= weights[i];

                        validUpgrades.RemoveAt(i);
                        weights.RemoveAt(i);
                        break;
                    }
                }
            }

            // 3. Fallback Buffer (Bảo hiểm chống cạn pool khi Max Level toàn bộ)
            int fallbackIndex = 0;
            while (selectedUpgrades.Count < count && _fallbackRewards != null && _fallbackRewards.Count > 0)
            {
                var fallback = _fallbackRewards[fallbackIndex % _fallbackRewards.Count];
                if (!selectedUpgrades.Contains(fallback))
                {
                    selectedUpgrades.Add(fallback);
                }
                fallbackIndex++;
            }

            return selectedUpgrades;
        }
    }
}
