using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectZombie.Features.Shared;
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

        [Header("Element Badges")]
        [SerializeField] private Sprite _badgeElementKim;
        [SerializeField] private Sprite _badgeElementMoc;
        [SerializeField] private Sprite _badgeElementThuy;
        [SerializeField] private Sprite _badgeElementHoa;
        [SerializeField] private Sprite _badgeElementTho;

        [Header("Rarity Borders")]
        [SerializeField] private Sprite _borderCommon;
        [SerializeField] private Sprite _borderRare;
        [SerializeField] private Sprite _borderEpic;
        [SerializeField] private Sprite _borderLegendary;

        private Color GetElementColor(ElementType element)
        {
            switch (element)
            {
                case ElementType.Kim: return new Color(1.0f, 0.84f, 0.0f, 1f); // Vàng Kim
                case ElementType.Moc: return new Color(0.30f, 0.75f, 0.35f, 1f); // Xanh Lục
                case ElementType.Thuy: return new Color(0.20f, 0.65f, 0.95f, 1f); // Xanh Lam
                case ElementType.Hoa: return new Color(0.95f, 0.28f, 0.22f, 1f); // Đỏ Chu Sa
                case ElementType.Tho: return new Color(0.65f, 0.48f, 0.32f, 1f); // Nâu Đất Đồng
                default: return new Color(0.85f, 0.85f, 0.90f, 1f);
            }
        }

        private Sprite GetElementBadgeSprite(ElementType element)
        {
            switch (element)
            {
                case ElementType.Kim:
                    if (_badgeElementKim != null) return _badgeElementKim;
                    var bKim = Resources.Load<Sprite>("UI/Badges/Badge_Element_Kim") ?? Resources.Load<Sprite>("Badge_Element_Kim");
                    if (bKim != null) return bKim;
#if UNITY_EDITOR
                    return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Badges/Badge_Element_Kim.png");
#else
                    return null;
#endif
                case ElementType.Moc:
                    if (_badgeElementMoc != null) return _badgeElementMoc;
                    var bMoc = Resources.Load<Sprite>("UI/Badges/Badge_Element_Moc") ?? Resources.Load<Sprite>("Badge_Element_Moc");
                    if (bMoc != null) return bMoc;
#if UNITY_EDITOR
                    return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Badges/Badge_Element_Moc.png");
#else
                    return null;
#endif
                case ElementType.Thuy:
                    if (_badgeElementThuy != null) return _badgeElementThuy;
                    var bThuy = Resources.Load<Sprite>("UI/Badges/Badge_Element_Thuy") ?? Resources.Load<Sprite>("Badge_Element_Thuy");
                    if (bThuy != null) return bThuy;
#if UNITY_EDITOR
                    return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Badges/Badge_Element_Thuy.png");
#else
                    return null;
#endif
                case ElementType.Hoa:
                    if (_badgeElementHoa != null) return _badgeElementHoa;
                    var bHoa = Resources.Load<Sprite>("UI/Badges/Badge_Element_Hoa") ?? Resources.Load<Sprite>("Badge_Element_Hoa");
                    if (bHoa != null) return bHoa;
#if UNITY_EDITOR
                    return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Badges/Badge_Element_Hoa.png");
#else
                    return null;
#endif
                case ElementType.Tho:
                    if (_badgeElementTho != null) return _badgeElementTho;
                    var bTho = Resources.Load<Sprite>("UI/Badges/Badge_Element_Tho") ?? Resources.Load<Sprite>("Badge_Element_Tho");
                    if (bTho != null) return bTho;
#if UNITY_EDITOR
                    return UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Badges/Badge_Element_Tho.png");
#else
                    return null;
#endif
                default:
                    return null;
            }
        }

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
                ProjectZombie.Features.Shared.ItemRarity.Legendary => "Assets/Art/UI/VongXuyen/Frame_Card_Evolution_Gold_9Slice.png",
                ProjectZombie.Features.Shared.ItemRarity.Epic => "Assets/Art/UI/VongXuyen/Frame_Card_Synergy_9Slice.png",
                ProjectZombie.Features.Shared.ItemRarity.Rare => "Assets/Art/UI/VongXuyen/Frame_Card_Jade_9Slice.png",
                _ => "Assets/Art/UI/VongXuyen/Frame_Card_Wood_9Slice.png"
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

            _relicSlotMap.Clear();
            _upgradeSlotMap.Clear();

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
            bool isUnlocked = star > 0;
            bool isSelected = _selectedRelic == weapon;

            // 1. Root Slot Object (120 x 140)
            GameObject slotObj = new GameObject($"Card_{weapon.weaponId}", typeof(RectTransform));
            slotObj.transform.SetParent(_view.CardsGridContainer, false);

            var slotRT = slotObj.GetComponent<RectTransform>();
            slotRT.sizeDelta = new Vector2(120, 140);

            // 2. Khung Ô Vật Phẩm (Box 110 x 110)
            GameObject boxObj = new GameObject("Box", typeof(RectTransform), typeof(Image), typeof(Button));
            boxObj.transform.SetParent(slotObj.transform, false);
            var boxRT = boxObj.GetComponent<RectTransform>();
            boxRT.anchorMin = new Vector2(0.5f, 1f);
            boxRT.anchorMax = new Vector2(0.5f, 1f);
            boxRT.pivot = new Vector2(0.5f, 1f);
            boxRT.anchoredPosition = Vector2.zero;
            boxRT.sizeDelta = new Vector2(110, 110);

            var boxImg = boxObj.GetComponent<Image>();
            boxImg.type = Image.Type.Sliced;
            boxImg.raycastTarget = true;

            Sprite slotWood = _cardSlotWoodSprite;
            Sprite slotSelected = _cardSlotSelectedSprite;
            if (slotWood == null) slotWood = Resources.Load<Sprite>("UI/VongXuyen/Slot_Inventory_Wood_9Slice") ?? Resources.Load<Sprite>("Slot_Inventory_Wood_9Slice");
            if (slotSelected == null) slotSelected = Resources.Load<Sprite>("UI/VongXuyen/Slot_Inventory_Selected_Glow") ?? Resources.Load<Sprite>("Slot_Inventory_Selected_Glow");
#if UNITY_EDITOR
            if (slotWood == null) slotWood = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Slot_Inventory_Wood_9Slice.png");
            if (slotSelected == null) slotSelected = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Slot_Inventory_Selected_Glow.png");
#endif

            if (isSelected)
            {
                if (slotSelected != null) boxImg.sprite = slotSelected;
                boxImg.color = Color.white;
            }
            else
            {
                if (slotWood != null) boxImg.sprite = slotWood;
                boxImg.color = isUnlocked ? Color.white : new Color(0.40f, 0.35f, 0.30f, 0.6f);
            }

            // 3. Nền Bên Trong (Inner Background - Đậm nét chuẩn Cổ Phong)
            GameObject innerObj = new GameObject("InnerBg", typeof(RectTransform), typeof(Image));
            innerObj.transform.SetParent(boxObj.transform, false);
            var inRT = innerObj.GetComponent<RectTransform>();
            inRT.anchorMin = Vector2.zero;
            inRT.anchorMax = Vector2.one;
            inRT.offsetMin = new Vector2(5, 5);
            inRT.offsetMax = new Vector2(-5, -5);

            var inImg = innerObj.GetComponent<Image>();
            inImg.color = new Color(0.12f, 0.09f, 0.16f, 0.85f);
            inImg.raycastTarget = false;

            // 4. Icon Pháp Bảo
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(innerObj.transform, false);
            var iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.anchorMin = Vector2.zero;
            iconRT.anchorMax = Vector2.one;
            iconRT.offsetMin = new Vector2(6, 6);
            iconRT.offsetMax = new Vector2(-6, -6);

            var iconImg = iconObj.GetComponent<Image>();
            iconImg.sprite = weapon.icon;
            iconImg.enabled = weapon.icon != null;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            iconImg.color = isUnlocked ? Color.white : new Color(0.35f, 0.30f, 0.35f, 0.45f);

            // 5. Huy hiệu Hệ Ngũ Hành ở góc trên trái
            if (isUnlocked)
            {
                Sprite elemBadge = GetElementBadgeSprite(weapon.elementType);
                if (elemBadge != null)
                {
                    GameObject elemBadgeObj = new GameObject("Badge_Element", typeof(RectTransform), typeof(Image));
                    elemBadgeObj.transform.SetParent(boxObj.transform, false);
                    var ebRT = elemBadgeObj.GetComponent<RectTransform>();
                    ebRT.anchorMin = new Vector2(0, 1);
                    ebRT.anchorMax = new Vector2(0, 1);
                    ebRT.pivot = new Vector2(0, 1);
                    ebRT.anchoredPosition = new Vector2(2, -2);
                    ebRT.sizeDelta = new Vector2(24, 24);
                    var ebImg = elemBadgeObj.GetComponent<Image>();
                    ebImg.sprite = elemBadge;
                    ebImg.preserveAspect = true;
                    ebImg.raycastTarget = false;
                }
            }

            // 6. Huy hiệu Cấp Sao ở góc trên phải
            if (star > 0)
            {
                GameObject starObj = new GameObject("Badge_Star", typeof(RectTransform), typeof(TextMeshProUGUI));
                starObj.transform.SetParent(boxObj.transform, false);
                var starRT = starObj.GetComponent<RectTransform>();
                starRT.anchorMin = new Vector2(1, 1);
                starRT.anchorMax = new Vector2(1, 1);
                starRT.pivot = new Vector2(1, 1);
                starRT.anchoredPosition = new Vector2(-4, -2);
                starRT.sizeDelta = new Vector2(48, 20);

                var starTMP = starObj.GetComponent<TextMeshProUGUI>();
                starTMP.text = $"<color=#FFD700><b>{star}★</b></color>";
                starTMP.fontSize = 12;
                starTMP.alignment = TextAlignmentOptions.Right;
                starTMP.raycastTarget = false;
            }

            // 7. Thanh Tiến Độ Thẻ Mảnh (Góc dưới trong ô)
            GameObject shardObj = new GameObject("Txt_Shards", typeof(RectTransform), typeof(TextMeshProUGUI));
            shardObj.transform.SetParent(boxObj.transform, false);
            var shardRT = shardObj.GetComponent<RectTransform>();
            shardRT.anchorMin = new Vector2(0, 0);
            shardRT.anchorMax = new Vector2(1, 0);
            shardRT.pivot = new Vector2(0.5f, 0);
            shardRT.anchoredPosition = new Vector2(0, 3);
            shardRT.sizeDelta = new Vector2(-8, 16);

            var shardTMP = shardObj.GetComponent<TextMeshProUGUI>();
            if (star >= 5)
            {
                shardTMP.text = "<color=#00FF88><b>TỐI ĐA</b></color>";
            }
            else
            {
                string colorHex = shards >= reqShards ? "00FF88" : "FFAA00";
                shardTMP.text = $"<color=#{colorHex}><b>{shards}/{reqShards}</b></color>";
            }
            shardTMP.fontSize = 11f;
            shardTMP.alignment = TextAlignmentOptions.Center;
            shardTMP.raycastTarget = false;

            // 8. Nhãn Tên Vũ Khí bên dưới
            GameObject lblObj = new GameObject("Txt_Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblObj.transform.SetParent(slotObj.transform, false);
            var lblRT = lblObj.GetComponent<RectTransform>();
            lblRT.anchorMin = new Vector2(0, 0);
            lblRT.anchorMax = new Vector2(1, 0);
            lblRT.pivot = new Vector2(0.5f, 0);
            lblRT.anchoredPosition = Vector2.zero;
            lblRT.sizeDelta = new Vector2(0, 24);

            var lblTMP = lblObj.GetComponent<TextMeshProUGUI>();
            lblTMP.fontSize = 13.5f;
            lblTMP.alignment = TextAlignmentOptions.Center;
            lblTMP.fontStyle = FontStyles.Bold;
            lblTMP.overflowMode = TextOverflowModes.Ellipsis;
            lblTMP.raycastTarget = false;

            if (!isUnlocked)
            {
                lblTMP.text = isSelected ? "<color=#FFCC88>Chưa Mở Khóa</color>" : "<color=#665544>Chưa Mở Khóa</color>";
            }
            else
            {
                Color elemColor = GetElementColor(weapon.elementType);
                string nameColorHex = isSelected ? "FFFFFF" : ColorUtility.ToHtmlStringRGB(elemColor);
                lblTMP.text = $"<color=#{nameColorHex}>{weapon.weaponName}</color>";
            }

            // Lưu mapping để update selection
            _relicSlotMap[weapon] = (slotObj, boxImg, lblTMP, isUnlocked);

            // Xử lý Click
            var btn = boxObj.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                global::Core.Audio.AudioManager.Instance?.PlayUIClick();
                SelectRelic(weapon);
            });
        }

        private void CreateUpgradeCardItemView(UpgradeData data)
        {
            // 1. Root Slot Object (120 x 140)
            GameObject slotObj = new GameObject($"CardItem_{data.id}", typeof(RectTransform));
            slotObj.transform.SetParent(_view.CardsGridContainer, false);

            var slotRT = slotObj.GetComponent<RectTransform>();
            slotRT.sizeDelta = new Vector2(120, 140);

            // 2. Khung Box (110 x 110)
            GameObject boxObj = new GameObject("Box", typeof(RectTransform), typeof(Image), typeof(Button));
            boxObj.transform.SetParent(slotObj.transform, false);
            var boxRT = boxObj.GetComponent<RectTransform>();
            boxRT.anchorMin = new Vector2(0.5f, 1f);
            boxRT.anchorMax = new Vector2(0.5f, 1f);
            boxRT.pivot = new Vector2(0.5f, 1f);
            boxRT.anchoredPosition = Vector2.zero;
            boxRT.sizeDelta = new Vector2(110, 110);

            var boxImg = boxObj.GetComponent<Image>();
            boxImg.type = Image.Type.Sliced;
            boxImg.raycastTarget = true;

            Sprite slotWood = _cardSlotWoodSprite;
            Sprite slotSelected = _cardSlotSelectedSprite;
            if (slotWood == null) slotWood = Resources.Load<Sprite>("UI/VongXuyen/Slot_Inventory_Wood_9Slice") ?? Resources.Load<Sprite>("Slot_Inventory_Wood_9Slice");
            if (slotSelected == null) slotSelected = Resources.Load<Sprite>("UI/VongXuyen/Slot_Inventory_Selected_Glow") ?? Resources.Load<Sprite>("Slot_Inventory_Selected_Glow");
#if UNITY_EDITOR
            if (slotWood == null) slotWood = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Slot_Inventory_Wood_9Slice.png");
            if (slotSelected == null) slotSelected = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Slot_Inventory_Selected_Glow.png");
#endif
            bool isSelected = _selectedUpgrade == data;
            if (isSelected)
            {
                if (slotSelected != null) boxImg.sprite = slotSelected;
                boxImg.color = Color.white;
            }
            else
            {
                if (slotWood != null) boxImg.sprite = slotWood;
                boxImg.color = Color.white;
            }

            // 3. Inner Background
            GameObject innerObj = new GameObject("InnerBg", typeof(RectTransform), typeof(Image));
            innerObj.transform.SetParent(boxObj.transform, false);
            var inRT = innerObj.GetComponent<RectTransform>();
            inRT.anchorMin = Vector2.zero;
            inRT.anchorMax = Vector2.one;
            inRT.offsetMin = new Vector2(5, 5);
            inRT.offsetMax = new Vector2(-5, -5);

            var inImg = innerObj.GetComponent<Image>();
            inImg.color = new Color(0.12f, 0.09f, 0.16f, 0.85f);
            inImg.raycastTarget = false;

            // 4. Icon
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(innerObj.transform, false);
            var iconRT = iconObj.GetComponent<RectTransform>();
            iconRT.anchorMin = Vector2.zero;
            iconRT.anchorMax = Vector2.one;
            iconRT.offsetMin = new Vector2(6, 6);
            iconRT.offsetMax = new Vector2(-6, -6);

            var iconImg = iconObj.GetComponent<Image>();
            iconImg.sprite = data.icon;
            iconImg.enabled = data.icon != null;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;

            // 5. Tên Upgrade bên dưới
            GameObject lblObj = new GameObject("Txt_Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lblObj.transform.SetParent(slotObj.transform, false);
            var lblRT = lblObj.GetComponent<RectTransform>();
            lblRT.anchorMin = new Vector2(0, 0);
            lblRT.anchorMax = new Vector2(1, 0);
            lblRT.pivot = new Vector2(0.5f, 0);
            lblRT.anchoredPosition = Vector2.zero;
            lblRT.sizeDelta = new Vector2(0, 24);

            var lblTMP = lblObj.GetComponent<TextMeshProUGUI>();
            lblTMP.text = isSelected ? $"<color=#FFFFFF>{data.upgradeName}</color>" : $"<color=#F1E6C8>{data.upgradeName}</color>";
            lblTMP.fontSize = 12.5f;
            lblTMP.fontStyle = FontStyles.Bold;
            lblTMP.alignment = TextAlignmentOptions.Center;
            lblTMP.overflowMode = TextOverflowModes.Ellipsis;
            lblTMP.raycastTarget = false;

            // Lưu mapping để update selection
            _upgradeSlotMap[data] = (slotObj, boxImg, lblTMP);

            var btn = boxObj.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                global::Core.Audio.AudioManager.Instance?.PlayUIClick();
                SelectUpgrade(data);
            });
        }

        private readonly Dictionary<WeaponData, (GameObject slotObj, Image boxImg, TextMeshProUGUI lblTMP, bool isUnlocked)> _relicSlotMap = new Dictionary<WeaponData, (GameObject, Image, TextMeshProUGUI, bool)>();
        private readonly Dictionary<UpgradeData, (GameObject slotObj, Image boxImg, TextMeshProUGUI lblTMP)> _upgradeSlotMap = new Dictionary<UpgradeData, (GameObject, Image, TextMeshProUGUI)>();

        private void UpdateSelectionVisuals()
        {
            Sprite slotWood = _cardSlotWoodSprite;
            Sprite slotSelected = _cardSlotSelectedSprite;
            if (slotWood == null) slotWood = Resources.Load<Sprite>("UI/VongXuyen/Slot_Inventory_Wood_9Slice") ?? Resources.Load<Sprite>("Slot_Inventory_Wood_9Slice");
            if (slotSelected == null) slotSelected = Resources.Load<Sprite>("UI/VongXuyen/Slot_Inventory_Selected_Glow") ?? Resources.Load<Sprite>("Slot_Inventory_Selected_Glow");
#if UNITY_EDITOR
            if (slotWood == null) slotWood = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Slot_Inventory_Wood_9Slice.png");
            if (slotSelected == null) slotSelected = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Slot_Inventory_Selected_Glow.png");
#endif

            if (_currentTab == CodexTabType.RelicFusion)
            {
                foreach (var kvp in _relicSlotMap)
                {
                    var weapon = kvp.Key;
                    var (slotObj, boxImg, lblTMP, isUnlocked) = kvp.Value;
                    if (slotObj == null || boxImg == null) continue;

                    bool isSelected = weapon == _selectedRelic;

                    if (isSelected)
                    {
                        if (slotSelected != null) boxImg.sprite = slotSelected;
                        boxImg.color = Color.white;
                    }
                    else
                    {
                        if (slotWood != null) boxImg.sprite = slotWood;
                        boxImg.color = isUnlocked ? Color.white : new Color(0.40f, 0.35f, 0.30f, 0.6f);
                    }

                    if (lblTMP != null)
                    {
                        if (!isUnlocked)
                        {
                            lblTMP.text = isSelected ? "<color=#FFCC88>Chưa Mở Khóa</color>" : "<color=#665544>Chưa Mở Khóa</color>";
                        }
                        else
                        {
                            Color elemColor = GetElementColor(weapon.elementType);
                            string nameColorHex = isSelected ? "FFFFFF" : ColorUtility.ToHtmlStringRGB(elemColor);
                            lblTMP.text = $"<color=#{nameColorHex}>{weapon.weaponName}</color>";
                        }
                    }
                }
            }
            else
            {
                foreach (var kvp in _upgradeSlotMap)
                {
                    var data = kvp.Key;
                    var (slotObj, boxImg, lblTMP) = kvp.Value;
                    if (slotObj == null || boxImg == null) continue;

                    bool isSelected = data == _selectedUpgrade;

                    if (isSelected)
                    {
                        if (slotSelected != null) boxImg.sprite = slotSelected;
                        boxImg.color = Color.white;
                    }
                    else
                    {
                        if (slotWood != null) boxImg.sprite = slotWood;
                        boxImg.color = Color.white;
                    }

                    if (lblTMP != null)
                    {
                        lblTMP.text = isSelected ? $"<color=#FFFFFF>{data.upgradeName}</color>" : $"<color=#F1E6C8>{data.upgradeName}</color>";
                    }
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
    }
}
