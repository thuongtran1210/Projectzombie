using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.MetaProgression;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.UI
{
    /// <summary>
    /// Presenter điều phối toàn bộ dữ liệu Thư Viện Thần Thẻ & Luyện Khí Gộp Thẻ Vũ Khí / Tướng / Lính / Trụ (Mô hình Clean MVP).
    /// </summary>
    public partial class CardCodexPresenter : MonoBehaviour, ProjectZombie.Core.Audio.IAudioServiceConsumer
    {
        private ProjectZombie.Core.Audio.IAudioService _audioService;
        public void InjectAudioService(ProjectZombie.Core.Audio.IAudioService audioService) => _audioService = audioService;
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

        private static readonly List<WeaponData> _cachedWeapons = new List<WeaponData>();
        private static readonly List<CharacterDataSO> _cachedHeroes = new List<CharacterDataSO>();
        private static bool _isDataLoaded = false;
        private static Task _loadingDataTask = null;

        private readonly Dictionary<WeaponData, CodexSlotItemView> _relicSlotViewMap = new Dictionary<WeaponData, CodexSlotItemView>();
        private readonly Dictionary<CharacterDataSO, CodexSlotItemView> _heroSlotViewMap = new Dictionary<CharacterDataSO, CodexSlotItemView>();

        private static CodexTabType? _pendingInitialTab;
        private CodexTabType _currentTab = CodexTabType.HeroCards;
        private WeaponData _selectedRelic;
        private CharacterDataSO _selectedHero;

        /// <summary>
        /// Chỉ định Tab khởi đầu khi mở Bách Bảo Các từ Sảnh Chính hoặc màn hình Chuẩn Bị Xuất Trận.
        /// </summary>
        public static void RequestInitialTab(CodexTabType tab)
        {
            _pendingEquipmentMode = false;
            _hasPendingLoadoutSlot = false;
            _pendingInitialTab = tab;
        }

        private static bool _hasPendingLoadoutSlot;
        private static bool? _pendingEquipmentMode;
        private bool _equipmentMode;
        private static string _pendingLoadoutSlotId;

        /// <summary>Opens the collection to choose a card for an explicit loadout position.</summary>
        public static void RequestLoadoutSlot(CodexTabType tab, string currentId)
        {
            _pendingEquipmentMode = true;
            _pendingInitialTab = tab;
            _pendingLoadoutSlotId = currentId;
            _hasPendingLoadoutSlot = true;
        }

        private void ApplyPendingLoadoutSlot()
        {
            if (!_hasPendingLoadoutSlot) return;
            _hasPendingLoadoutSlot = false;
            HandleLoadoutSlotClicked(_currentTab, _pendingLoadoutSlotId);
            _pendingLoadoutSlotId = null;
        }

        private void Awake()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            if (_view == null) _view = GetComponent<CardCodexView>();
            InitializeLoadout();
            EnsureVisualSprites();
            LoadAllData();

            if (_view != null)
            {
                _view.OnLoadoutSlotClicked += HandleLoadoutSlotClicked;
                _view.OnEquipClicked += HandleEquipClicked;
                _view.OnBackClicked += HandleBack;
                _view.OnTabChanged += SetTab;
                _view.OnAlchemyFusionClicked += HandleFusionClicked;
                _view.OnShown += HandleViewShown;

                // Prewarm sẵn 20 slots để giảm triệt để lag spike khi vẽ grid
                _view.PrewarmSlots(20);
            }

            SubscribeManagers();
            ProjectZombie.Core.Architecture.AppBootGate.OnCoreReady -= HandleCoreReady;
            ProjectZombie.Core.Architecture.AppBootGate.OnCoreReady += HandleCoreReady;
            sw.Stop();
            Debug.Log($"<color=#00FF88>[CardCodexPresenter] Awake hoàn tất trong: {sw.ElapsedMilliseconds} ms (Prewarm 20 slots & Sprites)</color>");
        }

        private void HandleCoreReady()
        {
            if (this == null || !isActiveAndEnabled) return;
            SubscribeManagers();
            InitializeLoadout();
            RefreshCurrency();
            PopulateGridForTab(_currentTab);
        }

        private void EnsureVisualSprites()
        {
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
        }

        private void OnDestroy()
        {
            ProjectZombie.Core.Architecture.AppBootGate.OnCoreReady -= HandleCoreReady;
            if (_view != null)
            {
                _view.OnLoadoutSlotClicked -= HandleLoadoutSlotClicked;
                _view.OnEquipClicked -= HandleEquipClicked;
                _view.OnBackClicked -= HandleBack;
                _view.OnTabChanged -= SetTab;
                _view.OnAlchemyFusionClicked -= HandleFusionClicked;
                _view.OnShown -= HandleViewShown;
            }

            UnsubscribeManagers();
        }

        private void HandleViewShown()
        {
            if (_pendingEquipmentMode.HasValue)
            {
                _equipmentMode = _pendingEquipmentMode.Value;
                _pendingEquipmentMode = null;
            }
            if (_pendingInitialTab.HasValue)
            {
                _currentTab = _pendingInitialTab.Value;
                _pendingInitialTab = null;
                SetTab(_currentTab);
            }
            ApplyPendingLoadoutSlot();
            UpdateScreenPurpose();
        }

        private void OnEnable()
        {
            if (_pendingEquipmentMode.HasValue) _equipmentMode = _pendingEquipmentMode.Value;
            var sw = System.Diagnostics.Stopwatch.StartNew();
            SubscribeManagers();
            LoadAllData();
            InitializeLoadout();
            RefreshCurrency();
            if (_pendingInitialTab.HasValue)
            {
                _currentTab = _pendingInitialTab.Value;
                _pendingInitialTab = null;
            }
            SetTab(_currentTab);
            sw.Stop();
            Debug.Log($"<color=#00FF88>[CardCodexPresenter] OnEnable hoàn tất trong: {sw.ElapsedMilliseconds} ms (Tab: {_currentTab})</color>");
        }

        private void OnDisable()
        {
            UnsubscribeManagers();
        }

        private void SubscribeManagers()
        {
            if (RelicInventoryManager.HasInstance)
            {
                var relicMgr = RelicInventoryManager.Instance;
                relicMgr.OnRelicShardsChanged -= HandleRelicDataChanged;
                relicMgr.OnRelicShardsChanged += HandleRelicDataChanged;
                relicMgr.OnRelicStarUpgraded -= HandleRelicDataChanged;
                relicMgr.OnRelicStarUpgraded += HandleRelicDataChanged;
            }

            if (CharacterProgressionManager.HasInstance)
            {
                var heroMgr = CharacterProgressionManager.Instance;
                heroMgr.OnCharacterShardsChanged -= HandleHeroDataChanged;
                heroMgr.OnCharacterShardsChanged += HandleHeroDataChanged;
                heroMgr.OnCharacterStarUpgraded -= HandleHeroDataChanged;
                heroMgr.OnCharacterStarUpgraded += HandleHeroDataChanged;
            }

            if (MetaCurrencyManager.HasInstance)
            {
                var curMgr = MetaCurrencyManager.Instance;
                curMgr.OnCurrencyChanged -= HandleCurrencyChanged;
                curMgr.OnCurrencyChanged += HandleCurrencyChanged;
            }
        }

        private void UnsubscribeManagers()
        {
            if (RelicInventoryManager.HasInstance)
            {
                var relicMgr = RelicInventoryManager.Instance;
                relicMgr.OnRelicShardsChanged -= HandleRelicDataChanged;
                relicMgr.OnRelicStarUpgraded -= HandleRelicDataChanged;
            }

            if (CharacterProgressionManager.HasInstance)
            {
                var heroMgr = CharacterProgressionManager.Instance;
                heroMgr.OnCharacterShardsChanged -= HandleHeroDataChanged;
                heroMgr.OnCharacterStarUpgraded -= HandleHeroDataChanged;
            }

            if (MetaCurrencyManager.HasInstance)
            {
                MetaCurrencyManager.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
            }
        }

        private void HandleRelicDataChanged(string relicId, int val)
        {
            if (_upgradeInProgress) return;
            if (_currentTab == CodexTabType.RelicFusion || _currentTab == CodexTabType.Minions || _currentTab == CodexTabType.Towers)
            {
                RefreshUI();
            }
        }

        private void HandleHeroDataChanged(string heroId, int val)
        {
            if (_upgradeInProgress) return;
            if (_currentTab == CodexTabType.HeroCards)
            {
                RefreshUI();
            }
        }

        private void HandleCurrencyChanged(int newBalance)
        {
            if (_upgradeInProgress) return;
            RefreshCurrency();
            if (_selectedRelic != null && _currentTab == CodexTabType.RelicFusion)
            {
                SelectRelic(_selectedRelic);
            }
            else if (_selectedHero != null && _currentTab == CodexTabType.HeroCards)
            {
                SelectHero(_selectedHero);
            }
            else if (!string.IsNullOrEmpty(_inspectedDefenseId) && _defenseCatalog != null && _defenseCatalog.TryGetEntry(_inspectedDefenseId, out var entry))
            {
                InspectDefenseEntry(entry);
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
            else if (!string.IsNullOrEmpty(_inspectedDefenseId) && _defenseCatalog != null && _defenseCatalog.TryGetEntry(_inspectedDefenseId, out var entry))
            {
                InspectDefenseEntry(entry);
            }
        }

        public void LoadAllData()
        {
            if (_isDataLoaded && _cachedHeroes.Count > 0)
            {
                return;
            }

#if UNITY_EDITOR
            LoadDataFromEditorFallback();
#endif

            if (_loadingDataTask == null || _loadingDataTask.IsCompleted)
            {
                _loadingDataTask = LoadAllDataAsync();
            }
        }

#if UNITY_EDITOR
        private void LoadDataFromEditorFallback()
        {
            var seenWeaponIds = new HashSet<string>();

            foreach (var w in _cachedWeapons)
            {
                if (w != null && !string.IsNullOrEmpty(w.weaponId)) seenWeaponIds.Add(w.weaponId);
            }

            if (_cachedWeapons.Count == 0)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:WeaponData");
                foreach (var guid in guids)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    var w = UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponData>(path);
                    if (!path.Contains("Archive_Legacy_2D") && w != null && !string.IsNullOrEmpty(w.weaponId) && ResolveSupportedId(w.weaponId, false) != null && seenWeaponIds.Add(w.weaponId))
                    {
                        _cachedWeapons.Add(w);
                    }
                }
            }
            if (_cachedHeroes.Count == 0)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:CharacterDataSO");
                foreach (var guid in guids)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    var h = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterDataSO>(path);
                    if (!path.Contains("Archive_Legacy_2D") && h != null && ResolveSupportedId(h.characterId, true) != null && !_cachedHeroes.Contains(h)) _cachedHeroes.Add(h);
                }
            }

            if (_cachedHeroes.Count > 0)
            {
                _isDataLoaded = true;
            }
        }
#endif

        public async Task LoadAllDataAsync()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            bool hasNewData = false;

            var seenWeaponIds = new HashSet<string>();
            foreach (var w in _cachedWeapons)
            {
                if (w != null && !string.IsNullOrEmpty(w.weaponId)) seenWeaponIds.Add(w.weaponId);
            }

            if (ProjectZombie.Core.Services.Data.GameDataService.Instance != null)
            {
                if (_cachedHeroes.Count == 0)
                {
                    try
                    {
                        var db = Resources.Load<CharacterDatabaseSO>("CharacterDatabase")
                            ?? await ProjectZombie.Core.Services.Data.GameDataService.Instance.GetAsync<CharacterDatabaseSO>("CharacterDatabase");
                        if (db != null && db.Characters != null)
                        {
                            foreach (var h in db.Characters)
                            {
                                if (h != null && ResolveSupportedId(h.characterId, true) != null && !_cachedHeroes.Contains(h))
                                {
                                    _cachedHeroes.Add(h);
                                    hasNewData = true;
                                }
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[CardCodexPresenter] Load CharacterDatabase warning: {ex.Message}");
                    }
                }
            }

            _isDataLoaded = _cachedHeroes.Count > 0;
            sw.Stop();
            Debug.Log($"<color=#00FF88>[CardCodexPresenter] LoadAllDataAsync hoàn tất: ({_cachedWeapons.Count} Vũ khí, {_cachedHeroes.Count} Tướng) trong {sw.ElapsedMilliseconds} ms.</color>");

            if (hasNewData && gameObject.activeInHierarchy && _view != null && _view.gameObject.activeInHierarchy)
            {
                PopulateGridForTab(_currentTab);
            }
        }

        private void RefreshCurrency()
        {
            var curMgr = MetaCurrencyManager.HasInstance ? MetaCurrencyManager.Instance : null;
            int coins = curMgr != null ? curMgr.TotalCurrency : 0;
            if (_view != null) _view.SetCoTienBalance($"<color=#FFD700>{coins:N0}</color>");
        }

        public void SetTab(CodexTabType tab)
        {
            _choosingLoadoutSlot = false;
            _replacementId = null;
            _currentTab = tab;
            UpdateScreenPurpose();
            _inspectedDefenseId = null;
            RefreshLoadoutSummary();
            if (_view != null) _view.UpdateTabVisuals(tab, _tabActiveSprite, _tabInactiveSprite);
            PopulateGridForTab(tab);
        }

        private void PopulateGridForTab(CodexTabType tab)
        {
            if (_view == null) return;

            var sw = System.Diagnostics.Stopwatch.StartNew();
            _inspectedCard = null;
            _defenseSlotViewMap.Clear();
            _view.ClearGrid();
            _relicSlotViewMap.Clear();
            _heroSlotViewMap.Clear();

            if (tab == CodexTabType.RelicFusion && _cachedWeapons.Count == 0)
            {
                PopulateDefenseGrid(tab);
            }
            else if (tab == CodexTabType.RelicFusion)
            {
                if (_cachedWeapons.Count > 0)
                {
                    var relicMgr = RelicInventoryManager.HasInstance ? RelicInventoryManager.Instance : null;
                    if (relicMgr != null) relicMgr.EnsureInitialized();

                    for (int i = 0; i < _cachedWeapons.Count; i++)
                    {
                        var weapon = _cachedWeapons[i];
                        int star = relicMgr != null ? relicMgr.GetRelicStarLevel(weapon.weaponId) : 0;
                        int shards = relicMgr != null ? relicMgr.GetRelicShardCount(weapon.weaponId) : 0;
                        var nextStep = relicMgr != null ? relicMgr.GetNextStepConfig(weapon.weaponId) : null;
                        int reqShards = nextStep != null ? nextStep.requiredShards : 5;
                        bool isUnlocked = star > 0 || (_defenseOwnership.IsOwned(ResolveSupportedId(weapon.weaponId, false) ?? ""));
                        if (star <= 0 && isUnlocked) star = 1;
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
                            _audioService?.PlayUIClick();
                            SelectRelic(weapon);
                        });

                        _relicSlotViewMap[weapon] = slotItem;
                    }

                    if (_selectedRelic != null && _cachedWeapons.Contains(_selectedRelic))
                    {
                        SelectRelic(_selectedRelic);
                    }
                }
                else
                {
                    _view.DisplayCardDetail("Chưa Có Thần Binh", "", "Đang cập nhật...", null, false);
                }
            }
            else if (tab == CodexTabType.HeroCards)
            {
                if (_cachedHeroes.Count > 0)
                {
                    var heroMgr = CharacterProgressionManager.HasInstance ? CharacterProgressionManager.Instance : null;
                    if (heroMgr != null) heroMgr.EnsureInitialized();

                    for (int i = 0; i < _cachedHeroes.Count; i++)
                    {
                        var hero = _cachedHeroes[i];
                        int star = heroMgr != null ? heroMgr.GetCharacterStarLevel(hero.characterId) : 0;
                        int shards = heroMgr != null ? heroMgr.GetCharacterShardCount(hero.characterId) : 0;
                        var nextStep = heroMgr != null ? heroMgr.GetNextStepConfig(hero.characterId) : null;
                        int reqShards = nextStep != null ? nextStep.requiredShards : 10;
                        bool isUnlocked = star > 0 || _defenseOwnership.IsOwned(ResolveSupportedId(hero.characterId, true) ?? "");
                        if (star <= 0 && isUnlocked) star = 1;
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
                            _audioService?.PlayUIClick();
                            SelectHero(hero);
                        });

                        _heroSlotViewMap[hero] = slotItem;
                    }

                    if (_selectedHero == null || !_cachedHeroes.Contains(_selectedHero))
                    {
                        _selectedHero = _cachedHeroes[0];
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
                PopulateDefenseGrid(tab);
            }

            AddMissingCatalogCards(tab);
            RefreshEquippedBadges();
            RefreshEquipAction();
            sw.Stop();
            Debug.Log($"<color=#00FF88>[CardCodexPresenter] PopulateGridForTab ({tab}) hoàn tất trong: {sw.ElapsedMilliseconds} ms.</color>");
        }

        private CodexSlotItemView _inspectedCard;
        private bool _inspectedCardOwned;
        private string _inspectedCardName;

        private void HighlightCard(CodexSlotItemView card, bool owned, string name)
        {
            if (_inspectedCard != null && _inspectedCard != card)
                _inspectedCard.SetSelected(false, _inspectedCardOwned, null, _inspectedCardName);
            _inspectedCard = card; _inspectedCardOwned = owned; _inspectedCardName = name;
            if (card != null) card.SetSelected(true, owned, null, name);
        }

        private void UpdateSelectionVisuals()
        {
            if (_currentTab == CodexTabType.RelicFusion && _selectedRelic != null
                && _relicSlotViewMap.TryGetValue(_selectedRelic, out var relicSlot))
            {
                bool owned = (RelicInventoryManager.HasInstance && RelicInventoryManager.Instance.GetRelicStarLevel(_selectedRelic.weaponId) > 0)
                    || _defenseOwnership.IsOwned(ResolveSupportedId(_selectedRelic.weaponId, false) ?? "");
                HighlightCard(relicSlot, owned, _selectedRelic.weaponName);
            }
            else if (_currentTab == CodexTabType.HeroCards && _selectedHero != null
                && _heroSlotViewMap.TryGetValue(_selectedHero, out var heroSlot))
            {
                bool owned = (CharacterProgressionManager.HasInstance && CharacterProgressionManager.Instance.GetCharacterStarLevel(_selectedHero.characterId) > 0)
                    || _defenseOwnership.IsOwned(ResolveSupportedId(_selectedHero.characterId, true) ?? "");
                HighlightCard(heroSlot, owned, _selectedHero.characterName);
            }
        }

        private void SelectRelic(WeaponData weapon)
        {
            if (weapon == null) return;
            _selectedRelic = weapon;
            _inspectedDefenseId = ResolveSupportedId(weapon.weaponId, false);
            RefreshEquipAction();

            UpdateSelectionVisuals();

            var relicMgr = RelicInventoryManager.HasInstance ? RelicInventoryManager.Instance : null;
            if (relicMgr != null) relicMgr.EnsureInitialized();
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
                btnLabel = cost > 0 ? $"TĂNG SAO ({reqShards} Thẻ + {cost:N0} Cổ Tiền)" : $"TĂNG SAO ({reqShards} Thẻ)";
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
            ShowUpgradeAvailability(star, shards, reqShards, cost, canFuse,
                relicMgr == null ? "Dữ liệu đang được khởi tạo." : nextStep == null ? "Không tìm thấy cấu hình nâng cấp." : string.Empty);
        }

        private void SelectHero(CharacterDataSO hero)
        {
            if (hero == null) return;
            _selectedHero = hero;
            _inspectedDefenseId = ResolveSupportedId(hero.characterId, true);
            RefreshEquipAction();

            UpdateSelectionVisuals();

            var heroMgr = CharacterProgressionManager.HasInstance ? CharacterProgressionManager.Instance : null;
            if (heroMgr != null) heroMgr.EnsureInitialized();
            int star = heroMgr != null ? heroMgr.GetCharacterStarLevel(hero.characterId) : 0;
            if (star <= 0 && _inspectedDefenseId != null && _defenseOwnership != null && _defenseOwnership.IsOwned(_inspectedDefenseId)) star = 1;
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
            ProjectZombie.Features.Defense3D.Loadout.DefenseLoadoutRuntimeAdapter.EvaluateHeroStarBonuses(star, null, out float hpBonus, out float spdBonus);
            ProjectZombie.Features.Defense3D.Loadout.DefenseLoadoutRuntimeAdapter.EvaluateHeroCombatStarBonuses(star, null, out float dmgBonus, out float signatureCd);
            string desc = $"<color=#FFD700>Chỉ Số Cấp {star} Sao:</color> Sinh Lực <b>+{hpBonus * 100f:0}%</b>  •  Tốc Chạy <b>+{spdBonus * 100f:0}%</b>\n\n" +
                          $"Sát Thương Đánh Thường / Bản Mệnh <b>+{dmgBonus * 100f:0}%</b>  •  Hồi Chiêu Bản Mệnh <b>-{signatureCd * 100f:0}%</b>\n\n" +
                          $"<b>Kỹ Năng:</b> {hero.signatureSkillName} - {hero.signatureSkillDesc}\n" +
                          $"<b>Nội Tại:</b> {hero.passiveTraitName} - {hero.passiveTraitDesc}\n\n" +
                          $"<color=#D1D5DB>{hero.description}</color>\n\n";

            if (nextStep != null)
            {
                var adapterStar = nextStep.targetStar;
                ProjectZombie.Features.Defense3D.Loadout.DefenseLoadoutRuntimeAdapter.EvaluateHeroStarBonuses(adapterStar, null, out float nextHp, out float nextSpeed);
                ProjectZombie.Features.Defense3D.Loadout.DefenseLoadoutRuntimeAdapter.EvaluateHeroCombatStarBonuses(adapterStar, null, out float nextDamage, out float nextCooldown);
                desc += $"<color=#00FF88>Cấp {adapterStar} Sao:</color> Máu +{nextHp * 100f:0}% • Tốc chạy +{nextSpeed * 100f:0}% • Sát thương +{nextDamage * 100f:0}% • Giảm hồi chiêu bản mệnh {nextCooldown * 100f:0}%";
            }

            _view.DisplayRelicDetail(hero.characterName, category, desc, hero.avatar, star, shards, reqShards, cost, canUpgrade, btnLabel);
            ShowUpgradeAvailability(star, shards, reqShards, cost, canUpgrade,
                heroMgr == null ? "Dữ liệu đang được khởi tạo." : nextStep == null ? "Không tìm thấy cấu hình nâng cấp." : string.Empty);
        }

        private void HandleFusionClicked()
        {
            PerformSelectedUpgrade();
        }

        private void HandleBack()
        {
            _audioService?.PlayUIClick();
            var metaManager = GetComponentInParent<MetaUIManager>() ?? MetaUIManager.Instance;
            if (MainHubPresenter.Instance != null) MainHubPresenter.Instance.RefreshHubState();
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
