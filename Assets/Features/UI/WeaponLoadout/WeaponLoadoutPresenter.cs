using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.MetaProgression;
using ProjectZombie.Features.UI.Common;
using ProjectZombie.Core.Save;

namespace ProjectZombie.Features.UI
{
    public enum LoadoutInventoryTab
    {
        PrimaryWeapons,
        Relics
    }

    /// <summary>
    /// Presenter điều phối toàn bộ luồng logic Tàng Bảo Các (Kho Pháp Bảo) chuẩn Clean MVP.
    /// Sử dụng UniversalItemSlotView dùng chung, triệt tiêu toàn bộ code sinh GameObject runtime.
    /// </summary>
    public class WeaponLoadoutPresenter : MonoBehaviour
    {
        [SerializeField] private WeaponLoadoutView _view;

        [Header("Weapon Database")]
        [SerializeField] private List<WeaponData> _allWeapons = new List<WeaponData>();

        [Header("UI Sprites & Badges")]
        [SerializeField] private Sprite _slotWoodSprite;
        [SerializeField] private Sprite _slotSelectedSprite;
        [SerializeField] private Sprite _badgeElementKim;
        [SerializeField] private Sprite _badgeElementMoc;
        [SerializeField] private Sprite _badgeElementThuy;
        [SerializeField] private Sprite _badgeElementHoa;
        [SerializeField] private Sprite _badgeElementTho;

        private CharacterEntry _currentHero;
        private WeaponData _selectedPrimary;
        private readonly List<WeaponData> _selectedRelics = new List<WeaponData>();
        private WeaponData _inspectedWeapon;
        private LoadoutInventoryTab _currentTab = LoadoutInventoryTab.Relics;

        private readonly Dictionary<WeaponData, UniversalItemSlotView> _slotViewMap = new Dictionary<WeaponData, UniversalItemSlotView>();

        private void Awake()
        {
            if (_view == null)
            {
                _view = GetComponent<WeaponLoadoutView>();
                if (_view == null) _view = GetComponentInChildren<WeaponLoadoutView>(true);
            }

            LoadAllWeaponsIfEmpty();

            if (_view != null)
            {
                _view.OnTabPrimaryClicked += () => SetTab(LoadoutInventoryTab.PrimaryWeapons);
                _view.OnTabRelicsClicked += () => SetTab(LoadoutInventoryTab.Relics);
                _view.OnStartBattleClicked += HandleStartBattle;
                _view.OnBackClicked += HandleBack;
            }
        }

        private void Start()
        {
            RefreshUI();
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.OnStartBattleClicked -= HandleStartBattle;
                _view.OnBackClicked -= HandleBack;
            }
        }

        private void OnEnable()
        {
            LoadAllWeaponsIfEmpty();

            if (RunLoadoutState.SelectedCharacter != null)
            {
                SetupForHero(RunLoadoutState.SelectedCharacter);
            }
            else
            {
                RunLoadoutState.EnsureInitialized();
                if (RunLoadoutState.SelectedCharacter != null)
                {
                    SetupForHero(RunLoadoutState.SelectedCharacter);
                }
                else
                {
                    RefreshUI();
                }
            }
        }

        public void LoadAllWeaponsIfEmpty()
        {
            if (_allWeapons == null || _allWeapons.Count == 0)
            {
                _allWeapons = new List<WeaponData>();
                var seenIds = new HashSet<string>();

                void TryAddWeapon(WeaponData w)
                {
                    if (w == null || string.IsNullOrEmpty(w.weaponId)) return;
                    if (seenIds.Add(w.weaponId))
                    {
                        _allWeapons.Add(w);
                    }
                }

                var loaded1 = Resources.LoadAll<WeaponData>("ScriptableObjects/Weapons");
                if (loaded1 != null && loaded1.Length > 0)
                {
                    foreach (var w in loaded1) TryAddWeapon(w);
                }

                var loaded2 = Resources.LoadAll<WeaponData>("Weapons");
                if (loaded2 != null && loaded2.Length > 0)
                {
                    foreach (var w in loaded2) TryAddWeapon(w);
                }

                var loaded3 = Resources.LoadAll<WeaponData>("");
                if (loaded3 != null && loaded3.Length > 0)
                {
                    foreach (var w in loaded3) TryAddWeapon(w);
                }

#if UNITY_EDITOR
                if (_allWeapons.Count == 0)
                {
                    string[] guids = UnityEditor.AssetDatabase.FindAssets("t:WeaponData", new[] { "Assets/_Data/Weapons", "Assets/Resources/Weapons" });
                    foreach (var guid in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                        var wd = UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponData>(path);
                        TryAddWeapon(wd);
                    }
                }
#endif
            }
        }

        public bool IsRelicOwned(WeaponData relic)
        {
            if (relic == null) return false;
            if (relic.weaponId == "wp_kiem_truc") return true;

            if (RelicInventoryManager.Instance != null)
            {
                return RelicInventoryManager.Instance.IsRelicUnlocked(relic.weaponId);
            }

            if (GameManager.Instance != null && GameManager.Instance.SaveData != null)
            {
                return GameManager.Instance.SaveData.GetRelicStarLevel(relic.weaponId) >= 1;
            }

            return false;
        }

        public void SetupForHero(CharacterEntry hero)
        {
            _currentHero = hero;
            LoadAllWeaponsIfEmpty();

            // 1. Vũ Khí Chính
            if (RunLoadoutState.SelectedPrimaryWeapon != null)
            {
                _selectedPrimary = RunLoadoutState.SelectedPrimaryWeapon;
            }
            else if (hero != null && hero.defaultPrimaryWeapon != null)
            {
                _selectedPrimary = hero.defaultPrimaryWeapon;
            }
            else if (_selectedPrimary == null)
            {
                _selectedPrimary = _allWeapons.Find(w => w.weaponRole == WeaponRole.PrimaryWeapon);
            }

            // 2. Pháp Bảo Hộ Thân
            _selectedRelics.Clear();
            if (RunLoadoutState.SelectedRelic != null && IsRelicOwned(RunLoadoutState.SelectedRelic))
            {
                _selectedRelics.Add(RunLoadoutState.SelectedRelic);
            }
            else if (RunLoadoutState.SelectedRelics != null && RunLoadoutState.SelectedRelics.Count > 0 && IsRelicOwned(RunLoadoutState.SelectedRelics[0]))
            {
                _selectedRelics.Add(RunLoadoutState.SelectedRelics[0]);
            }
            else if (hero != null && hero.defaultRelic != null && IsRelicOwned(hero.defaultRelic))
            {
                _selectedRelics.Add(hero.defaultRelic);
            }
            else if (hero != null && hero.defaultRelics != null && hero.defaultRelics.Count > 0 && IsRelicOwned(hero.defaultRelics[0]))
            {
                _selectedRelics.Add(hero.defaultRelics[0]);
            }

            if (_selectedRelics.Count == 0)
            {
                var ownedRelic = _allWeapons.Find(w => w.weaponRole != WeaponRole.PrimaryWeapon && IsRelicOwned(w));
                if (ownedRelic != null) _selectedRelics.Add(ownedRelic);
            }

            _inspectedWeapon = _selectedRelics.Count > 0 ? _selectedRelics[0] : _selectedPrimary;
            _currentTab = LoadoutInventoryTab.Relics;
            RefreshUI();
        }

        public void SetTab(LoadoutInventoryTab tab)
        {
            _currentTab = tab;
            global::Core.Audio.AudioManager.Instance?.PlayUIClick();
            RefreshUI();
        }

        public void SelectPrimaryWeapon(WeaponData weapon)
        {
            if (weapon == null) return;
            _selectedPrimary = weapon;
            _inspectedWeapon = weapon;

            global::Core.Audio.AudioManager.Instance?.PlayWeaponEquip();

            RunLoadoutState.SetLoadout(_currentHero, _selectedPrimary, _selectedRelics);
            if (PlayerProvider.HasPlayer && PlayerProvider.PlayerGameObject != null)
            {
                var wm = PlayerProvider.PlayerGameObject.GetComponent<WeaponManager>();
                if (wm != null) wm.ReloadEquippedWeapons();
            }

            UpdateSelectionDetailsOnly();
        }

        public void ToggleRelic(WeaponData relic)
        {
            if (relic == null) return;

            if (!IsRelicOwned(relic))
            {
                InspectRelic(relic);
                return;
            }

            _selectedRelics.Clear();
            _selectedRelics.Add(relic);
            _inspectedWeapon = relic;

            global::Core.Audio.AudioManager.Instance?.PlayWeaponEquip();

            RunLoadoutState.SetLoadout(_currentHero, _selectedPrimary, _selectedRelics);
            if (PlayerProvider.HasPlayer && PlayerProvider.PlayerGameObject != null)
            {
                var wm = PlayerProvider.PlayerGameObject.GetComponent<WeaponManager>();
                if (wm != null) wm.ReloadEquippedWeapons();
            }

            UpdateSelectionDetailsOnly();
        }

        public void InspectRelic(WeaponData relic)
        {
            if (relic == null) return;
            _inspectedWeapon = relic;
            global::Core.Audio.AudioManager.Instance?.PlayUIClick();
            UpdateSelectionDetailsOnly();
        }

        private void UpdateSelectionDetailsOnly()
        {
            if (_view == null) return;

            // 1. Cột Xuất Trận
            if (_currentHero != null)
            {
                _view.DisplayEquippedLoadout(_currentHero, _selectedRelics);
            }
            else
            {
                _view.DisplayEquippedLoadout(_selectedPrimary, _selectedRelics);
            }

            // 2. Cột Chi Tiết Soi
            if (_inspectedWeapon != null)
            {
                float dmgFill = Mathf.Clamp01(_inspectedWeapon.baseDamage / 35f);
                float cdFill = 1f - Mathf.Clamp01(_inspectedWeapon.baseAttackSpeed / 2.5f);
                _view.DisplayWeaponDetail(_inspectedWeapon, dmgFill, cdFill);
            }

            // 3. Cập nhật Visual trên từng Slot Item trong Grid
            UpdateGridItemStates();
        }

        public void RefreshUI()
        {
            if (_view == null) return;

            // 1. Header
            if (_currentHero != null)
            {
                string elemStr = $"<color={_currentHero.elementHexColor}>Hệ {_currentHero.element}</color>";
                _view.DisplayHeroHeader(_currentHero.characterName, elemStr, _currentHero.avatar);
            }

            // 2. Tab
            _view.SetTabState(_currentTab == LoadoutInventoryTab.PrimaryWeapons);

            // 3. Loadout Cột Phải
            if (_currentHero != null)
            {
                _view.DisplayEquippedLoadout(_currentHero, _selectedRelics);
            }
            else
            {
                _view.DisplayEquippedLoadout(_selectedPrimary, _selectedRelics);
            }

            // 4. Chi tiết Soi
            if (_inspectedWeapon != null)
            {
                float dmgFill = Mathf.Clamp01(_inspectedWeapon.baseDamage / 35f);
                float cdFill = 1f - Mathf.Clamp01(_inspectedWeapon.baseAttackSpeed / 2.5f);
                _view.DisplayWeaponDetail(_inspectedWeapon, dmgFill, cdFill);
            }

            // 5. Grid Slots
            PopulateInventoryGrid();
        }

        private void UpdateGridItemStates()
        {
            var relicMgr = RelicInventoryManager.Instance;

            foreach (var kvp in _slotViewMap)
            {
                var weapon = kvp.Key;
                var slotView = kvp.Value;
                if (weapon == null || slotView == null) continue;

                bool isUnlocked = IsRelicOwned(weapon);
                bool isEquipped = _selectedRelics.Contains(weapon);
                bool isInspected = _inspectedWeapon == weapon;
                int star = relicMgr != null ? relicMgr.GetRelicStarLevel(weapon.weaponId) : (isUnlocked ? 1 : 0);
                int shards = relicMgr != null ? relicMgr.GetRelicShardCount(weapon.weaponId) : 0;
                var nextStep = relicMgr != null ? relicMgr.GetNextStepConfig(weapon.weaponId) : null;
                int reqShards = nextStep != null ? nextStep.requiredShards : 5;

                string bottomText = isEquipped ? "<color=#00FF88>Đang Dùng</color>" : (isUnlocked ? (star >= 5 ? "<color=#00FF88>Tối Đa</color>" : $"<color=#FFAA00>{shards}/{reqShards}</color>") : "<color=#888888>Khóa</color>");

                var vm = new UniversalItemSlotViewModel
                {
                    ItemId = weapon.weaponId,
                    ItemName = weapon.weaponName,
                    Icon = weapon.icon,
                    Rarity = weapon.rarity,
                    Element = weapon.elementType,
                    ElementBadge = GetElementBadgeSprite(weapon.elementType),
                    StarLevel = star,
                    ShardCount = shards,
                    ReqShards = reqShards,
                    IsLocked = !isUnlocked,
                    IsEquipped = isEquipped,
                    IsSelected = isInspected,
                    CustomBottomText = bottomText
                };

                slotView.BindData(vm, _slotWoodSprite, _slotSelectedSprite, () =>
                {
                    if (!isUnlocked) InspectRelic(weapon);
                    else if (weapon.weaponRole == WeaponRole.PrimaryWeapon) SelectPrimaryWeapon(weapon);
                    else ToggleRelic(weapon);
                });
            }
        }

        private void PopulateInventoryGrid()
        {
            if (_view == null || _view.InventoryGridContainer == null) return;

            _view.ClearGrid();
            _slotViewMap.Clear();

            var targetList = new List<WeaponData>();
            foreach (var w in _allWeapons)
            {
                if (w != null && w.weaponRole != WeaponRole.PrimaryWeapon)
                {
                    targetList.Add(w);
                }
            }

            if (targetList.Count == 0) targetList.AddRange(_allWeapons);

            var relicMgr = RelicInventoryManager.Instance;

            for (int i = 0; i < targetList.Count; i++)
            {
                var weapon = targetList[i];
                bool isUnlocked = IsRelicOwned(weapon);
                bool isEquipped = _selectedRelics.Contains(weapon);
                bool isInspected = _inspectedWeapon == weapon;
                int star = relicMgr != null ? relicMgr.GetRelicStarLevel(weapon.weaponId) : (isUnlocked ? 1 : 0);
                int shards = relicMgr != null ? relicMgr.GetRelicShardCount(weapon.weaponId) : 0;
                var nextStep = relicMgr != null ? relicMgr.GetNextStepConfig(weapon.weaponId) : null;
                int reqShards = nextStep != null ? nextStep.requiredShards : 5;

                string bottomText = isEquipped ? "<color=#00FF88>Đang Dùng</color>" : (isUnlocked ? (star >= 5 ? "<color=#00FF88>Tối Đa</color>" : $"<color=#FFAA00>{shards}/{reqShards}</color>") : "<color=#888888>Khóa</color>");

                var slotItem = _view.CreateSlotItem();
                if (slotItem == null) continue;

                var vm = new UniversalItemSlotViewModel
                {
                    ItemId = weapon.weaponId,
                    ItemName = weapon.weaponName,
                    Icon = weapon.icon,
                    Rarity = weapon.rarity,
                    Element = weapon.elementType,
                    ElementBadge = GetElementBadgeSprite(weapon.elementType),
                    StarLevel = star,
                    ShardCount = shards,
                    ReqShards = reqShards,
                    IsLocked = !isUnlocked,
                    IsEquipped = isEquipped,
                    IsSelected = isInspected,
                    CustomBottomText = bottomText
                };

                slotItem.BindData(vm, _slotWoodSprite, _slotSelectedSprite, () =>
                {
                    if (!isUnlocked) InspectRelic(weapon);
                    else if (weapon.weaponRole == WeaponRole.PrimaryWeapon) SelectPrimaryWeapon(weapon);
                    else ToggleRelic(weapon);
                });

                _slotViewMap[weapon] = slotItem;
            }
        }

        private Sprite GetElementBadgeSprite(ElementType element)
        {
            switch (element)
            {
                case ElementType.Kim:
                    if (_badgeElementKim != null) return _badgeElementKim;
                    return Resources.Load<Sprite>("UI/Badges/Badge_Element_Kim") ?? Resources.Load<Sprite>("Badge_Element_Kim");
                case ElementType.Moc:
                    if (_badgeElementMoc != null) return _badgeElementMoc;
                    return Resources.Load<Sprite>("UI/Badges/Badge_Element_Moc") ?? Resources.Load<Sprite>("Badge_Element_Moc");
                case ElementType.Thuy:
                    if (_badgeElementThuy != null) return _badgeElementThuy;
                    return Resources.Load<Sprite>("UI/Badges/Badge_Element_Thuy") ?? Resources.Load<Sprite>("Badge_Element_Thuy");
                case ElementType.Hoa:
                    if (_badgeElementHoa != null) return _badgeElementHoa;
                    return Resources.Load<Sprite>("UI/Badges/Badge_Element_Hoa") ?? Resources.Load<Sprite>("Badge_Element_Hoa");
                case ElementType.Tho:
                    if (_badgeElementTho != null) return _badgeElementTho;
                    return Resources.Load<Sprite>("UI/Badges/Badge_Element_Tho") ?? Resources.Load<Sprite>("Badge_Element_Tho");
                default:
                    return null;
            }
        }

        private void HandleStartBattle()
        {
            Debug.Log($"<color=#00FF88>[WeaponLoadoutPresenter]</color> XÁC NHẬN XUẤT TRẬN: Hero={_currentHero?.characterName}, Primary={_selectedPrimary?.weaponName}, Relics Count={_selectedRelics.Count}");

            RunLoadoutState.SetLoadout(_currentHero, _selectedPrimary, _selectedRelics);

            if (MetaSceneTransitionController.Instance != null)
            {
                MetaSceneTransitionController.Instance.StartRun();
            }
            else
            {
                var transitionCtrl = FindObjectOfType<MetaSceneTransitionController>();
                if (transitionCtrl != null)
                {
                    transitionCtrl.StartRun();
                }
                else if (MetaUIManager.Instance != null)
                {
                    MetaUIManager.Instance.SetMetaCanvasActive(false);
                }
            }
        }

        private void HandleBack()
        {
            RunLoadoutState.SetLoadout(_currentHero, _selectedPrimary, _selectedRelics);

            var mainHubPresenter = FindObjectOfType<MainHubPresenter>(true);
            if (mainHubPresenter != null)
            {
                mainHubPresenter.RefreshHubState();
            }

            if (MetaUIManager.Instance != null)
            {
                MetaUIManager.Instance.PopScreen();
            }
        }
    }
}
