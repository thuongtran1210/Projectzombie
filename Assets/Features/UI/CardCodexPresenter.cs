using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Upgrades;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Presenter điều phối toàn bộ dữ liệu Thư Viện Thần Thẻ & Luyện Khí Gộp Thẻ Vũ Khí (Mô hình Clean MVP).
    /// </summary>
    public class CardCodexPresenter : MonoBehaviour
    {
        [Header("View")]
        [SerializeField] private CardCodexView _view;

        [Header("Visual Sprites")]
        [SerializeField] private Sprite _tabActiveSprite;
        [SerializeField] private Sprite _tabInactiveSprite;
        [SerializeField] private Sprite _cardSlotWoodSprite;
        [SerializeField] private Sprite _cardSlotSelectedSprite;

        [Header("Element Badges")]
        [SerializeField] private Sprite _badgeElementKim;
        [SerializeField] private Sprite _badgeElementMoc;
        [SerializeField] private Sprite _badgeElementThuy;
        [SerializeField] private Sprite _badgeElementHoa;
        [SerializeField] private Sprite _badgeElementTho;

        private readonly List<WeaponData> _allWeapons = new List<WeaponData>();
        private readonly List<UpgradeData> _allUpgrades = new List<UpgradeData>();
        private readonly List<FusionUpgradeData> _allFusionUpgrades = new List<FusionUpgradeData>();

        private readonly Dictionary<WeaponData, CodexSlotItemView> _relicSlotViewMap = new Dictionary<WeaponData, CodexSlotItemView>();
        private readonly Dictionary<UpgradeData, CodexSlotItemView> _upgradeSlotViewMap = new Dictionary<UpgradeData, CodexSlotItemView>();

        private CodexTabType _currentTab = CodexTabType.RelicFusion;
        private WeaponData _selectedRelic;
        private UpgradeData _selectedUpgrade;

        private void Awake()
        {
            if (_view == null) _view = GetComponent<CardCodexView>();
            LoadAllData();

            if (_view != null)
            {
                _view.OnBackClicked += HandleBack;
                _view.OnTabChanged += SetTab;
                _view.OnAlchemyFusionClicked += HandleFusionClicked;
                _view.OnShown += HandleViewShown;
            }

            SubscribeManagers();
        }

        private void Start()
        {
            SubscribeManagers();
            RefreshUI();
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.OnBackClicked -= HandleBack;
                _view.OnTabChanged -= SetTab;
                _view.OnAlchemyFusionClicked -= HandleFusionClicked;
                _view.OnShown -= HandleViewShown;
            }

            UnsubscribeManagers();
        }

        private void HandleViewShown()
        {
            RefreshUI();
        }

        private void OnEnable()
        {
            SubscribeManagers();
            LoadAllData();
            RefreshCurrency();
            SetTab(_currentTab);
        }

        private void OnDisable()
        {
            UnsubscribeManagers();
        }

        private void SubscribeManagers()
        {
            if (RelicInventoryManager.Instance != null)
            {
                RelicInventoryManager.Instance.OnRelicShardsChanged -= HandleRelicDataChanged;
                RelicInventoryManager.Instance.OnRelicShardsChanged += HandleRelicDataChanged;
                RelicInventoryManager.Instance.OnRelicStarUpgraded -= HandleRelicDataChanged;
                RelicInventoryManager.Instance.OnRelicStarUpgraded += HandleRelicDataChanged;
            }

            if (MetaCurrencyManager.Instance != null)
            {
                MetaCurrencyManager.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
                MetaCurrencyManager.Instance.OnCurrencyChanged += HandleCurrencyChanged;
            }
        }

        private void UnsubscribeManagers()
        {
            if (RelicInventoryManager.Instance != null)
            {
                RelicInventoryManager.Instance.OnRelicShardsChanged -= HandleRelicDataChanged;
                RelicInventoryManager.Instance.OnRelicStarUpgraded -= HandleRelicDataChanged;
            }

            if (MetaCurrencyManager.Instance != null)
            {
                MetaCurrencyManager.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
            }
        }

        private void HandleRelicDataChanged(string relicId, int val)
        {
            RefreshUI();
        }

        private void HandleCurrencyChanged(int newBalance)
        {
            RefreshCurrency();
            if (_selectedRelic != null && _currentTab == CodexTabType.RelicFusion)
            {
                SelectRelic(_selectedRelic);
            }
        }

        public void RefreshUI()
        {
            LoadAllData();
            RefreshCurrency();
            PopulateGridForTab(_currentTab);
            if (_selectedRelic != null && _currentTab == CodexTabType.RelicFusion)
            {
                SelectRelic(_selectedRelic);
            }
        }

        public void LoadAllData()
        {
            _allUpgrades.Clear();
            _allFusionUpgrades.Clear();
            _allWeapons.Clear();

            var seenUpgradeIds = new HashSet<string>();
            var seenWeaponIds = new HashSet<string>();

            void TryAddUpgrade(UpgradeData u)
            {
                if (u == null) return;
                string upId = !string.IsNullOrEmpty(u.id) ? u.id : u.name;
                if (seenUpgradeIds.Add(upId))
                {
                    _allUpgrades.Add(u);
                    if (u is FusionUpgradeData f) _allFusionUpgrades.Add(f);
                }
            }

            void TryAddWeapon(WeaponData w)
            {
                if (w == null || string.IsNullOrEmpty(w.weaponId)) return;
                if (seenWeaponIds.Add(w.weaponId))
                {
                    _allWeapons.Add(w);
                }
            }

            // 1. Load Upgrades
            var loadedUpgrades = Resources.LoadAll<UpgradeData>("Upgrades");
            if (loadedUpgrades != null)
            {
                foreach (var u in loadedUpgrades) TryAddUpgrade(u);
            }
            var rootUpgrades = Resources.LoadAll<UpgradeData>("");
            if (rootUpgrades != null)
            {
                foreach (var u in rootUpgrades) TryAddUpgrade(u);
            }

            // 2. Load Weapons across all Resources folders
            var loadedWeapons1 = Resources.LoadAll<WeaponData>("Weapons");
            if (loadedWeapons1 != null)
            {
                foreach (var w in loadedWeapons1) TryAddWeapon(w);
            }
            var loadedWeapons2 = Resources.LoadAll<WeaponData>("ScriptableObjects/Weapons");
            if (loadedWeapons2 != null)
            {
                foreach (var w in loadedWeapons2) TryAddWeapon(w);
            }
            var rootWeapons = Resources.LoadAll<WeaponData>("");
            if (rootWeapons != null)
            {
                foreach (var w in rootWeapons) TryAddWeapon(w);
            }

#if UNITY_EDITOR
            if (_allWeapons.Count == 0)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:WeaponData");
                foreach (var guid in guids)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    var w = UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponData>(path);
                    TryAddWeapon(w);
                }
            }
            if (_allUpgrades.Count == 0)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:UpgradeData");
                foreach (var guid in guids)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    var u = UnityEditor.AssetDatabase.LoadAssetAtPath<UpgradeData>(path);
                    TryAddUpgrade(u);
                }
            }
#endif
        }

        private void RefreshCurrency()
        {
            var curMgr = RelicInventoryManager.Instance != null ? MetaCurrencyManager.Instance : FindObjectOfType<MetaCurrencyManager>();
            int coins = curMgr != null ? curMgr.TotalCurrency : 0;
            if (_view != null) _view.SetCoTienBalance($"<color=#FFD700>{coins:N0}</color>");
        }

        public void SetTab(CodexTabType tab)
        {
            _currentTab = tab;
            if (_view != null) _view.UpdateTabVisuals(tab, _tabActiveSprite, _tabInactiveSprite);
            PopulateGridForTab(tab);
        }

        private void PopulateGridForTab(CodexTabType tab)
        {
            if (_view == null) return;

            _view.ClearGrid();
            _relicSlotViewMap.Clear();
            _upgradeSlotViewMap.Clear();

            if (tab == CodexTabType.RelicFusion)
            {
                if (_allWeapons.Count > 0)
                {
                    var relicMgr = RelicInventoryManager.Instance ?? FindObjectOfType<RelicInventoryManager>();

                    for (int i = 0; i < _allWeapons.Count; i++)
                    {
                        var weapon = _allWeapons[i];
                        int star = relicMgr != null ? relicMgr.GetRelicStarLevel(weapon.weaponId) : 0;
                        int shards = relicMgr != null ? relicMgr.GetRelicShardCount(weapon.weaponId) : 0;
                        var nextStep = relicMgr != null ? relicMgr.GetNextStepConfig(weapon.weaponId) : null;
                        int reqShards = nextStep != null ? nextStep.requiredShards : 5;
                        bool isUnlocked = star > 0;
                        bool isSelected = _selectedRelic == weapon;

                        var slotItem = _view.CreateSlotItem();
                        var vm = new RelicSlotViewModel
                        {
                            WeaponId = weapon.weaponId,
                            WeaponName = weapon.weaponName,
                            Icon = weapon.icon,
                            Rarity = weapon.rarity,
                            ElementBadge = GetElementBadgeSprite(weapon.elementType),
                            NameColor = GetElementColor(weapon.elementType),
                            StarLevel = star,
                            ShardCount = shards,
                            ReqShards = reqShards,
                            IsUnlocked = isUnlocked,
                            IsSelected = isSelected
                        };

                        slotItem.BindRelic(vm, _cardSlotWoodSprite, _cardSlotSelectedSprite, () =>
                        {
                            global::Core.Audio.AudioManager.Instance?.PlayUIClick();
                            SelectRelic(weapon);
                        });

                        _relicSlotViewMap[weapon] = slotItem;
                    }

                    if (_selectedRelic == null || !_allWeapons.Contains(_selectedRelic))
                    {
                        _selectedRelic = _allWeapons[0];
                    }
                    SelectRelic(_selectedRelic);
                }
                else
                {
                    _view.DisplayCardDetail("Chưa Có Thần Binh", "", "Đang cập nhật...", null, false);
                }
            }
            else
            {
                List<UpgradeData> filterList = new List<UpgradeData>();
                if (tab == CodexTabType.Passives)
                {
                    filterList.AddRange(_allUpgrades.FindAll(u => u.upgradeType == UpgradeType.CommonUpgrade || u.upgradeType == UpgradeType.RareUpgrade));
                }
                else
                {
                    filterList.AddRange(_allUpgrades.FindAll(u => u.upgradeType == UpgradeType.ComboAugment || u.upgradeType == UpgradeType.DashTrait || u.upgradeType == UpgradeType.BreakthroughUltimate));
                }

                if (filterList.Count > 0)
                {
                    for (int i = 0; i < filterList.Count; i++)
                    {
                        var data = filterList[i];
                        bool isSelected = _selectedUpgrade == data;

                        var slotItem = _view.CreateSlotItem();
                        var vm = new UpgradeSlotViewModel
                        {
                            UpgradeId = data.id,
                            UpgradeName = data.upgradeName,
                            Icon = data.icon,
                            IsSelected = isSelected
                        };

                        slotItem.BindUpgrade(vm, _cardSlotWoodSprite, _cardSlotSelectedSprite, () =>
                        {
                            global::Core.Audio.AudioManager.Instance?.PlayUIClick();
                            SelectUpgrade(data);
                        });

                        _upgradeSlotViewMap[data] = slotItem;
                    }
                    SelectUpgrade(filterList[0]);
                }
                else
                {
                    _view.DisplayCardDetail("Chưa Có Dữ Liệu", "", "Thư viện đang được cập nhật...", null, false);
                }
            }
        }

        private void UpdateSelectionVisuals()
        {
            if (_currentTab == CodexTabType.RelicFusion)
            {
                var relicMgr = RelicInventoryManager.Instance ?? FindObjectOfType<RelicInventoryManager>();
                foreach (var kvp in _relicSlotViewMap)
                {
                    var weapon = kvp.Key;
                    var slotView = kvp.Value;
                    if (slotView == null) continue;

                    bool isSelected = weapon == _selectedRelic;
                    int star = relicMgr != null ? relicMgr.GetRelicStarLevel(weapon.weaponId) : 0;
                    bool isUnlocked = star > 0;

                    slotView.SetSelected(isSelected, isUnlocked, GetElementColor(weapon.elementType), weapon.weaponName);
                }
            }
            else
            {
                foreach (var kvp in _upgradeSlotViewMap)
                {
                    var data = kvp.Key;
                    var slotView = kvp.Value;
                    if (slotView == null) continue;

                    bool isSelected = data == _selectedUpgrade;
                    slotView.SetSelected(isSelected, true, null, data.upgradeName);
                }
            }
        }

        private void SelectRelic(WeaponData weapon)
        {
            if (weapon == null) return;
            _selectedRelic = weapon;

            UpdateSelectionVisuals();

            var relicMgr = RelicInventoryManager.Instance ?? FindObjectOfType<RelicInventoryManager>();
            int star = relicMgr != null ? relicMgr.GetRelicStarLevel(weapon.weaponId) : 0;
            int shards = relicMgr != null ? relicMgr.GetRelicShardCount(weapon.weaponId) : 0;
            var nextStep = relicMgr != null ? relicMgr.GetNextStepConfig(weapon.weaponId) : null;

            int reqShards = nextStep != null ? nextStep.requiredShards : 5;
            int cost = nextStep != null ? nextStep.coTienCost : 0;

            bool canFuse = relicMgr != null && relicMgr.CanFuse(weapon.weaponId, out _);

            string btnLabel;
            if (star == 0)
            {
                btnLabel = $"GỘP THẺ MỞ KHÓA ({shards}/{reqShards})";
            }
            else if (star < 5)
            {
                btnLabel = cost > 0 ? $"TĂNG SAO ({reqShards} Thẻ + {cost} Cổ Tiền)" : $"TĂNG SAO ({reqShards} Thẻ)";
            }
            else
            {
                btnLabel = "CẢNH GIỚI TỐI ĐA (5 SAO)";
            }

            string rarityTag = $"<color={weapon.rarity.GetHexColor()}>[{weapon.rarity.GetDisplayName()}]</color>";
            string roleTag = weapon.weaponRole == WeaponRole.PrimaryWeapon ? "[VŨ KHÍ CHÍNH]" : "[PHÁP BẢO HỘ THÂN]";
            string category = $"{rarityTag} {roleTag}";
            string desc = $"{weapon.description}\n\n";

            if (nextStep != null)
            {
                desc += $"<color=#00FF88>Đột Phá Kế Tiếp ({nextStep.targetStar} Sao):</color> {nextStep.perkDescription}";
            }

            _view.DisplayRelicDetail(weapon.weaponName, category, desc, weapon.icon, star, shards, reqShards, cost, canFuse, btnLabel);
        }

        private void SelectUpgrade(UpgradeData data)
        {
            if (data == null) return;
            _selectedUpgrade = data;

            UpdateSelectionVisuals();

            bool isFusion = data is FusionUpgradeData;
            string categoryName = data.GetCategoryDisplayName();

            _view.DisplayCardDetail(data.upgradeName, categoryName, data.description, data.icon, isFusion);
        }

        private void HandleFusionClicked()
        {
            if (_selectedRelic == null) return;

            var relicMgr = RelicInventoryManager.Instance ?? FindObjectOfType<RelicInventoryManager>();
            if (relicMgr == null) return;

            if (relicMgr.TryFuseRelic(_selectedRelic.weaponId))
            {
                global::Core.Audio.AudioManager.Instance?.PlayUIConfirm();
                RefreshCurrency();
                PopulateGridForTab(_currentTab);
                SelectRelic(_selectedRelic);
            }
            else
            {
                global::Core.Audio.AudioManager.Instance?.PlayUIError();
            }
        }

        private void HandleBack()
        {
            global::Core.Audio.AudioManager.Instance?.PlayUIClick();
            var metaManager = GetComponentInParent<MetaUIManager>() ?? MetaUIManager.Instance;
            if (metaManager != null)
            {
                metaManager.PopScreen();
            }
        }

        private Color GetElementColor(ElementType element)
        {
            switch (element)
            {
                case ElementType.Kim: return new Color(1.0f, 0.84f, 0.0f, 1f);
                case ElementType.Moc: return new Color(0.30f, 0.75f, 0.35f, 1f);
                case ElementType.Thuy: return new Color(0.20f, 0.65f, 0.95f, 1f);
                case ElementType.Hoa: return new Color(0.95f, 0.28f, 0.22f, 1f);
                case ElementType.Tho: return new Color(0.65f, 0.48f, 0.32f, 1f);
                default: return new Color(0.85f, 0.85f, 0.90f, 1f);
            }
        }

        private Sprite GetElementBadgeSprite(ElementType element)
        {
            switch (element)
            {
                case ElementType.Kim: return _badgeElementKim;
                case ElementType.Moc: return _badgeElementMoc;
                case ElementType.Thuy: return _badgeElementThuy;
                case ElementType.Hoa: return _badgeElementHoa;
                case ElementType.Tho: return _badgeElementTho;
                default: return null;
            }
        }
    }
}
