using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.Upgrades;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Presenter điều phối toàn bộ dữ liệu Thư Viện Thần Thẻ & Luyện Khí Gộp Thẻ Vũ Khí (MVP).
    /// </summary>
    public class CardCodexPresenter : MonoBehaviour
    {
        [SerializeField] private CardCodexView _view;

        [Header("UI Visual Sprites")]
        [SerializeField] private Sprite _tabActiveSprite;
        [SerializeField] private Sprite _tabInactiveSprite;
        [SerializeField] private Sprite _cardSlotWoodSprite;
        [SerializeField] private Sprite _cardSlotSelectedSprite;
        [SerializeField] private Sprite _starBadgeSprite;

        [Header("Rarity Borders")]
        [SerializeField] private Sprite _borderCommon;
        [SerializeField] private Sprite _borderRare;
        [SerializeField] private Sprite _borderEpic;
        [SerializeField] private Sprite _borderLegendary;

        public Sprite GetRarityBorder(ProjectZombie.Features.Shared.ItemRarity rarity)
        {
            switch (rarity)
            {
                case ProjectZombie.Features.Shared.ItemRarity.Legendary:
                    if (_borderLegendary != null) return _borderLegendary;
                    break;
                case ProjectZombie.Features.Shared.ItemRarity.Epic:
                    if (_borderEpic != null) return _borderEpic;
                    break;
                case ProjectZombie.Features.Shared.ItemRarity.Rare:
                    if (_borderRare != null) return _borderRare;
                    break;
                default:
                    if (_borderCommon != null) return _borderCommon;
                    break;
            }

#if UNITY_EDITOR
            string borderPath = rarity switch
            {
                ProjectZombie.Features.Shared.ItemRarity.Legendary => "Assets/Art/UI/Gacha/Border_Rarity_Legendary.png",
                ProjectZombie.Features.Shared.ItemRarity.Epic => "Assets/Art/UI/Gacha/Border_Rarity_Epic.png",
                ProjectZombie.Features.Shared.ItemRarity.Rare => "Assets/Art/UI/Gacha/Border_Rarity_Rare.png",
                _ => "Assets/Art/UI/Gacha/Border_Rarity_Common.png"
            };
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(borderPath);
#else
            return _borderCommon;
#endif
        }

        private List<WeaponData> _allWeapons = new List<WeaponData>();
        private List<UpgradeData> _allUpgrades = new List<UpgradeData>();
        private List<FusionUpgradeData> _allFusionUpgrades = new List<FusionUpgradeData>();
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
            }

            SubscribeManagers();
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.OnBackClicked -= HandleBack;
                _view.OnTabChanged -= SetTab;
                _view.OnAlchemyFusionClicked -= HandleFusionClicked;
            }

            UnsubscribeManagers();
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
            if (loadedUpgrades == null || loadedUpgrades.Length == 0) loadedUpgrades = Resources.LoadAll<UpgradeData>("");
            if (loadedUpgrades != null)
            {
                foreach (var u in loadedUpgrades) TryAddUpgrade(u);
            }

            // 2. Load Weapons
            var loadedWeapons = Resources.LoadAll<WeaponData>("Weapons");
            if (loadedWeapons == null || loadedWeapons.Length == 0) loadedWeapons = Resources.LoadAll<WeaponData>("");
            if (loadedWeapons != null)
            {
                foreach (var w in loadedWeapons) TryAddWeapon(w);
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
            if (_view == null || _view.CardsGridContainer == null) return;

            for (int i = _view.CardsGridContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_view.CardsGridContainer.GetChild(i).gameObject);
            }

            if (tab == CodexTabType.RelicFusion)
            {
                // Hiển thị danh sách Thần Binh & Pháp Bảo theo dạng thẻ mảnh
                if (_allWeapons.Count > 0)
                {
                    for (int i = 0; i < _allWeapons.Count; i++)
                    {
                        var w = _allWeapons[i];
                        CreateRelicCardItemView(w);
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
                // Tab Passives hoặc Combo
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
                        CreateUpgradeCardItemView(data);
                    }
                    SelectUpgrade(filterList[0]);
                }
                else
                {
                    _view.DisplayCardDetail("Chưa Có Dữ Liệu", "", "Thư viện đang được cập nhật...", null, false);
                }
            }
        }

        private void CreateRelicCardItemView(WeaponData weapon)
        {
            var relicMgr = RelicInventoryManager.Instance ?? FindObjectOfType<RelicInventoryManager>();
            int star = relicMgr != null ? relicMgr.GetRelicStarLevel(weapon.weaponId) : 0;
            int shards = relicMgr != null ? relicMgr.GetRelicShardCount(weapon.weaponId) : 0;
            var nextStep = relicMgr != null ? relicMgr.GetNextStepConfig(weapon.weaponId) : null;
            int reqShards = nextStep != null ? nextStep.requiredShards : 5;

            GameObject itemObj = new GameObject($"Card_{weapon.weaponId}", typeof(RectTransform), typeof(Image), typeof(Button));
            itemObj.transform.SetParent(_view.CardsGridContainer, false);

            var rt = itemObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(110, 130);

            var img = itemObj.GetComponent<Image>();
            img.type = Image.Type.Sliced;
            if (_cardSlotWoodSprite != null) img.sprite = _cardSlotWoodSprite;
            img.color = star > 0 ? Color.white : new Color(0.6f, 0.55f, 0.5f, 0.8f);

            // Viền Rarity 9-Slice
            GameObject borderObj = new GameObject("Border_Rarity", typeof(RectTransform), typeof(Image));
            borderObj.transform.SetParent(itemObj.transform, false);
            var borderRT = borderObj.GetComponent<RectTransform>();
            borderRT.anchorMin = Vector2.zero;
            borderRT.anchorMax = Vector2.one;
            borderRT.offsetMin = Vector2.zero;
            borderRT.offsetMax = Vector2.zero;
            var borderImg = borderObj.GetComponent<Image>();
            borderImg.type = Image.Type.Sliced;
            borderImg.sprite = GetRarityBorder(weapon.rarity);
            borderImg.raycastTarget = false;
            borderImg.color = star > 0 ? Color.white : new Color(0.7f, 0.7f, 0.7f, 0.6f);

            // Icon
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(itemObj.transform, false);
            var iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.sizeDelta = new Vector2(64, 64);
            iconRT.anchoredPosition = new Vector2(0, 14);

            var iconImg = iconObj.GetComponent<Image>();
            iconImg.sprite = weapon.icon;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            iconImg.color = star > 0 ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.7f);

            // Badge Sao ở góc trên trái
            if (star > 0)
            {
                GameObject starObj = new GameObject("Txt_Star", typeof(RectTransform), typeof(TextMeshProUGUI));
                starObj.transform.SetParent(itemObj.transform, false);
                var starRT = starObj.GetComponent<RectTransform>();
                starRT.anchorMin = new Vector2(0, 1);
                starRT.anchorMax = new Vector2(0, 1);
                starRT.pivot = new Vector2(0, 1);
                starRT.anchoredPosition = new Vector2(4, -4);
                starRT.sizeDelta = new Vector2(50, 20);

                var starTMP = starObj.GetComponent<TextMeshProUGUI>();
                starTMP.text = $"<color=#FFD700>{star} Sao</color>";
                starTMP.fontSize = 11;
                starTMP.fontStyle = FontStyles.Bold;
            }

            // Thanh tiến độ thẻ mảnh (X/Y Thẻ)
            GameObject shardObj = new GameObject("Txt_Shards", typeof(RectTransform), typeof(TextMeshProUGUI));
            shardObj.transform.SetParent(itemObj.transform, false);
            var shardRT = shardObj.GetComponent<RectTransform>();
            shardRT.anchorMin = new Vector2(0, 0);
            shardRT.anchorMax = new Vector2(1, 0);
            shardRT.pivot = new Vector2(0.5f, 0);
            shardRT.anchoredPosition = new Vector2(0, 22);
            shardRT.sizeDelta = new Vector2(-8, 16);

            var shardTMP = shardObj.GetComponent<TextMeshProUGUI>();
            if (star >= 5)
            {
                shardTMP.text = "<color=#00FF88>MAX</color>";
            }
            else
            {
                string colorHex = shards >= reqShards ? "00FF88" : "FFAA00";
                shardTMP.text = $"<color=#{colorHex}>{shards}/{reqShards}</color>";
            }
            shardTMP.fontSize = 11;
            shardTMP.alignment = TextAlignmentOptions.Center;
            shardTMP.fontStyle = FontStyles.Bold;

            // Tên Vũ Khí
            GameObject txtObj = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(itemObj.transform, false);
            var txtRT = txtObj.GetComponent<RectTransform>();
            txtRT.anchorMin = new Vector2(0, 0);
            txtRT.anchorMax = new Vector2(1, 0);
            txtRT.pivot = new Vector2(0.5f, 0);
            txtRT.anchoredPosition = new Vector2(0, 4);
            txtRT.sizeDelta = new Vector2(-6, 18);

            var tmp = txtObj.GetComponent<TextMeshProUGUI>();
            tmp.text = weapon.weaponName;
            tmp.fontSize = 10.5f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = star > 0 ? new Color(0.95f, 0.88f, 0.70f, 1f) : new Color(0.7f, 0.65f, 0.6f, 0.9f);
            tmp.overflowMode = TextOverflowModes.Ellipsis;

            var btn = itemObj.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                global::Core.Audio.AudioManager.Instance?.PlayUIClick();
                SelectRelic(weapon);
            });
        }

        private void CreateUpgradeCardItemView(UpgradeData data)
        {
            GameObject itemObj = new GameObject($"CardItem_{data.id}", typeof(RectTransform), typeof(Image), typeof(Button));
            itemObj.transform.SetParent(_view.CardsGridContainer, false);

            var rt = itemObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(110, 130);

            var img = itemObj.GetComponent<Image>();
            img.type = Image.Type.Sliced;
            if (_cardSlotWoodSprite != null) img.sprite = _cardSlotWoodSprite;
            img.color = Color.white;

            // Xác định Rarity cho Upgrade
            var rarity = data.upgradeType switch
            {
                UpgradeType.BreakthroughUltimate or UpgradeType.EvolutionUpgrade or UpgradeType.RelicFusion => ProjectZombie.Features.Shared.ItemRarity.Legendary,
                UpgradeType.ComboAugment or UpgradeType.DashTrait or UpgradeType.ConditionalPassive => ProjectZombie.Features.Shared.ItemRarity.Epic,
                UpgradeType.RareUpgrade => ProjectZombie.Features.Shared.ItemRarity.Rare,
                _ => ProjectZombie.Features.Shared.ItemRarity.Common
            };

            // Viền Rarity 9-Slice
            GameObject borderObj = new GameObject("Border_Rarity", typeof(RectTransform), typeof(Image));
            borderObj.transform.SetParent(itemObj.transform, false);
            var borderRT = borderObj.GetComponent<RectTransform>();
            borderRT.anchorMin = Vector2.zero;
            borderRT.anchorMax = Vector2.one;
            borderRT.offsetMin = Vector2.zero;
            borderRT.offsetMax = Vector2.zero;
            var borderImg = borderObj.GetComponent<Image>();
            borderImg.type = Image.Type.Sliced;
            borderImg.sprite = GetRarityBorder(rarity);
            borderImg.raycastTarget = false;

            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(itemObj.transform, false);
            var iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.5f, 0.5f);
            iconRT.anchorMax = new Vector2(0.5f, 0.5f);
            iconRT.sizeDelta = new Vector2(64, 64);
            iconRT.anchoredPosition = new Vector2(0, 10);

            var iconImg = iconObj.GetComponent<Image>();
            iconImg.sprite = data.icon;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            GameObject txtObj = new GameObject("Name", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(itemObj.transform, false);
            var txtRT = txtObj.GetComponent<RectTransform>();
            txtRT.anchorMin = new Vector2(0, 0);
            txtRT.anchorMax = new Vector2(1, 0);
            txtRT.pivot = new Vector2(0.5f, 0);
            txtRT.anchoredPosition = new Vector2(0, 4);
            txtRT.sizeDelta = new Vector2(-6, 26);

            var tmp = txtObj.GetComponent<TextMeshProUGUI>();
            tmp.text = data.upgradeName;
            tmp.fontSize = 11;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.95f, 0.88f, 0.70f, 1f);
            tmp.enableWordWrapping = true;

            var btn = itemObj.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                global::Core.Audio.AudioManager.Instance?.PlayUIClick();
                SelectUpgrade(data);
            });
        }

        private void SelectRelic(WeaponData weapon)
        {
            if (weapon == null) return;
            _selectedRelic = weapon;

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

            string category = weapon.weaponRole == WeaponRole.PrimaryWeapon ? "[VŨ KHÍ CHÍNH]" : "[PHÁP BẢO HỘ THÂN]";
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
    }
}
