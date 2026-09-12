using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Upgrades;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.MetaProgression;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Presenter điều phối toàn bộ dữ liệu Thư Viện Thần Thẻ & Luyện Khí Gộp Thẻ Vũ Khí / Tướng (Mô hình Clean MVP).
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
        private readonly List<CharacterDataSO> _allHeroes = new List<CharacterDataSO>();
        private readonly List<UpgradeData> _allUpgrades = new List<UpgradeData>();
        private readonly List<FusionUpgradeData> _allFusionUpgrades = new List<FusionUpgradeData>();

        private readonly Dictionary<WeaponData, CodexSlotItemView> _relicSlotViewMap = new Dictionary<WeaponData, CodexSlotItemView>();
        private readonly Dictionary<CharacterDataSO, CodexSlotItemView> _heroSlotViewMap = new Dictionary<CharacterDataSO, CodexSlotItemView>();
        private readonly Dictionary<UpgradeData, CodexSlotItemView> _upgradeSlotViewMap = new Dictionary<UpgradeData, CodexSlotItemView>();

        private CodexTabType _currentTab = CodexTabType.RelicFusion;
        private WeaponData _selectedRelic;
        private CharacterDataSO _selectedHero;
        private UpgradeData _selectedUpgrade;

        private void Awake()
        {
            if (_view == null) _view = GetComponent<CardCodexView>();
            EnsureVisualSprites();
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

        private void EnsureVisualSprites()
        {
            if (_tabActiveSprite == null) _tabActiveSprite = Resources.Load<Sprite>("UI/VongXuyen/Btn_Tab_Wood_Active") ?? Resources.Load<Sprite>("Btn_Tab_Wood_Active");
            if (_tabInactiveSprite == null) _tabInactiveSprite = Resources.Load<Sprite>("UI/VongXuyen/Btn_Tab_Wood_Inactive") ?? Resources.Load<Sprite>("Btn_Tab_Wood_Inactive");
            if (_cardSlotWoodSprite == null) _cardSlotWoodSprite = Resources.Load<Sprite>("UI/VongXuyen/Slot_Inventory_Wood_9Slice") ?? Resources.Load<Sprite>("Slot_Inventory_Wood_9Slice");
            if (_cardSlotSelectedSprite == null) _cardSlotSelectedSprite = Resources.Load<Sprite>("UI/VongXuyen/Slot_Inventory_Selected_Glow") ?? Resources.Load<Sprite>("Slot_Inventory_Selected_Glow");

#if UNITY_EDITOR
            if (_tabActiveSprite == null) _tabActiveSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Tab_Wood_Active.png");
            if (_tabInactiveSprite == null) _tabInactiveSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Btn_Tab_Wood_Inactive.png");
            if (_cardSlotWoodSprite == null) _cardSlotWoodSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Slot_Inventory_Wood_9Slice.png");
            if (_cardSlotSelectedSprite == null) _cardSlotSelectedSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/VongXuyen/Slot_Inventory_Selected_Glow.png");
#endif
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

            if (CharacterProgressionManager.Instance != null)
            {
                CharacterProgressionManager.Instance.OnCharacterShardsChanged -= HandleHeroDataChanged;
                CharacterProgressionManager.Instance.OnCharacterShardsChanged += HandleHeroDataChanged;
                CharacterProgressionManager.Instance.OnCharacterStarUpgraded -= HandleHeroDataChanged;
                CharacterProgressionManager.Instance.OnCharacterStarUpgraded += HandleHeroDataChanged;
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

            if (CharacterProgressionManager.Instance != null)
            {
                CharacterProgressionManager.Instance.OnCharacterShardsChanged -= HandleHeroDataChanged;
                CharacterProgressionManager.Instance.OnCharacterStarUpgraded -= HandleHeroDataChanged;
            }

            if (MetaCurrencyManager.Instance != null)
            {
                MetaCurrencyManager.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
            }
        }

        private void HandleRelicDataChanged(string relicId, int val)
        {
            if (_currentTab == CodexTabType.RelicFusion) RefreshUI();
        }

        private void HandleHeroDataChanged(string heroId, int val)
        {
            if (_currentTab == CodexTabType.HeroCards) RefreshUI();
        }

        private void HandleCurrencyChanged(int newBalance)
        {
            RefreshCurrency();
            if (_selectedRelic != null && _currentTab == CodexTabType.RelicFusion)
            {
                SelectRelic(_selectedRelic);
            }
            else if (_selectedHero != null && _currentTab == CodexTabType.HeroCards)
            {
                SelectHero(_selectedHero);
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
            else if (_selectedHero != null && _currentTab == CodexTabType.HeroCards)
            {
                SelectHero(_selectedHero);
            }
        }

        public void LoadAllData()
        {
            _allUpgrades.Clear();
            _allFusionUpgrades.Clear();
            _allWeapons.Clear();
            _allHeroes.Clear();

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

            // 3. Load Heroes
            var charDb = Resources.Load<CharacterDatabaseSO>("CharacterDatabase");
#if UNITY_EDITOR
            if (charDb == null)
            {
                charDb = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterDatabaseSO>("Assets/Resources/CharacterDatabase.asset")
                      ?? UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterDatabaseSO>("Assets/_Data/CharacterDatabase.asset");
            }
#endif
            if (charDb != null && charDb.Characters != null)
            {
                foreach (var h in charDb.Characters)
                {
                    if (h != null && !_allHeroes.Contains(h)) _allHeroes.Add(h);
                }
            }

            var loadedHeroes = Resources.LoadAll<CharacterDataSO>("Characters");
            if (loadedHeroes != null)
            {
                foreach (var h in loadedHeroes)
                {
                    if (h != null && !_allHeroes.Contains(h)) _allHeroes.Add(h);
                }
            }

#if UNITY_EDITOR
            if (_allHeroes.Count == 0)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:CharacterDataSO");
                foreach (var guid in guids)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    var h = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterDataSO>(path);
                    if (h != null && !_allHeroes.Contains(h)) _allHeroes.Add(h);
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
            _heroSlotViewMap.Clear();
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
            else if (tab == CodexTabType.HeroCards)
            {
                if (_allHeroes.Count > 0)
                {
                    var heroMgr = CharacterProgressionManager.Instance ?? FindObjectOfType<CharacterProgressionManager>();

                    for (int i = 0; i < _allHeroes.Count; i++)
                    {
                        var hero = _allHeroes[i];
                        int star = heroMgr != null ? heroMgr.GetCharacterStarLevel(hero.characterId) : 0;
                        int shards = heroMgr != null ? heroMgr.GetCharacterShardCount(hero.characterId) : 0;
                        var nextStep = heroMgr != null ? heroMgr.GetNextStepConfig(hero.characterId) : null;
                        int reqShards = nextStep != null ? nextStep.requiredShards : 10;
                        bool isUnlocked = star > 0;
                        bool isSelected = _selectedHero == hero;

                        var slotItem = _view.CreateSlotItem();
                        var vm = new HeroSlotViewModel
                        {
                            CharacterId = hero.characterId,
                            CharacterName = hero.characterName,
                            Avatar = hero.avatar,
                            Rarity = hero.rarity,
                            ElementBadge = GetElementBadgeSprite(hero.element),
                            NameColor = GetElementColor(hero.element),
                            StarLevel = star,
                            ShardCount = shards,
                            ReqShards = reqShards,
                            IsUnlocked = isUnlocked,
                            IsSelected = isSelected
                        };

                        slotItem.BindHero(vm, _cardSlotWoodSprite, _cardSlotSelectedSprite, () =>
                        {
                            global::Core.Audio.AudioManager.Instance?.PlayUIClick();
                            SelectHero(hero);
                        });

                        _heroSlotViewMap[hero] = slotItem;
                    }

                    if (_selectedHero == null || !_allHeroes.Contains(_selectedHero))
                    {
                        _selectedHero = _allHeroes[0];
                    }
                    SelectHero(_selectedHero);
                }
                else
                {
                    _view.DisplayCardDetail("Chưa Có Anh Hùng", "", "Đang cập nhật dữ liệu tướng...", null, false);
                }
            }
            else
            {
                List<UpgradeData> filterList = new List<UpgradeData>();
                filterList.AddRange(_allUpgrades.FindAll(u => u.upgradeType == UpgradeType.CommonUpgrade || u.upgradeType == UpgradeType.RareUpgrade || u.upgradeType == UpgradeType.ComboAugment || u.upgradeType == UpgradeType.DashTrait || u.upgradeType == UpgradeType.BreakthroughUltimate));

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
            else if (_currentTab == CodexTabType.HeroCards)
            {
                var heroMgr = CharacterProgressionManager.Instance ?? FindObjectOfType<CharacterProgressionManager>();
                foreach (var kvp in _heroSlotViewMap)
                {
                    var hero = kvp.Key;
                    var slotView = kvp.Value;
                    if (slotView == null) continue;

                    bool isSelected = hero == _selectedHero;
                    int star = heroMgr != null ? heroMgr.GetCharacterStarLevel(hero.characterId) : 0;
                    bool isUnlocked = star > 0;

                    slotView.SetSelected(isSelected, isUnlocked, GetElementColor(hero.element), hero.characterName);
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

        private void SelectHero(CharacterDataSO hero)
        {
            if (hero == null) return;
            _selectedHero = hero;

            UpdateSelectionVisuals();

            var heroMgr = CharacterProgressionManager.Instance ?? FindObjectOfType<CharacterProgressionManager>();
            int star = heroMgr != null ? heroMgr.GetCharacterStarLevel(hero.characterId) : 0;
            int shards = heroMgr != null ? heroMgr.GetCharacterShardCount(hero.characterId) : 0;
            var nextStep = heroMgr != null ? heroMgr.GetNextStepConfig(hero.characterId) : null;

            int reqShards = nextStep != null ? nextStep.requiredShards : 10;
            int cost = nextStep != null ? nextStep.coTienCost : 0;

            bool canUpgrade = heroMgr != null && heroMgr.CanUpgradeCharacter(hero.characterId, out _);

            string btnLabel;
            if (star == 0)
            {
                btnLabel = $"CHIÊU MỘ ANH HÙNG ({shards}/{reqShards} Mảnh)";
            }
            else if (star < 5)
            {
                btnLabel = cost > 0 ? $"THĂNG SAO ({reqShards} Mảnh + {cost:N0} Cổ Tiền)" : $"THĂNG SAO ({reqShards} Mảnh)";
            }
            else
            {
                btnLabel = "THẦN THOẠI ĐẠI THÀNH (5 SAO)";
            }

            string rarityTag = $"<color={hero.rarity.GetHexColor()}>[{hero.rarity.GetDisplayName()}]</color>";
            string elementTag = $"<color={hero.elementHexColor}>[{hero.element}]</color>";
            string category = $"{rarityTag} {elementTag} [ANH HÙNG]";
            string desc = $"<b>Kỹ Năng:</b> {hero.signatureSkillName} - {hero.signatureSkillDesc}\n" +
                          $"<b>Nội Tại:</b> {hero.passiveTraitName} - {hero.passiveTraitDesc}\n\n" +
                          $"<color=#D1D5DB>{hero.description}</color>\n\n";

            if (nextStep != null)
            {
                desc += $"<color=#00FF88>Cảnh Giới Kế Tiếp ({nextStep.targetStar} Sao):</color> {nextStep.perkDescription}";
            }

            _view.DisplayRelicDetail(hero.characterName, category, desc, hero.avatar, star, shards, reqShards, cost, canUpgrade, btnLabel);
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
            if (_currentTab == CodexTabType.RelicFusion)
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
            else if (_currentTab == CodexTabType.HeroCards)
            {
                if (_selectedHero == null) return;

                var heroMgr = CharacterProgressionManager.Instance ?? FindObjectOfType<CharacterProgressionManager>();
                if (heroMgr == null) return;

                var result = heroMgr.TryUpgradeCharacterStar(_selectedHero.characterId);
                if (result == UpgradeCharacterResult.Success)
                {
                    global::Core.Audio.AudioManager.Instance?.PlayUIConfirm();
                    RefreshCurrency();
                    PopulateGridForTab(_currentTab);
                    SelectHero(_selectedHero);
                }
                else
                {
                    global::Core.Audio.AudioManager.Instance?.PlayUIError();
                }
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
