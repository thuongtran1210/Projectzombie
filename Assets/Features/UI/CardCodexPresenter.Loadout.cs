using System;
using System.Linq;
using ProjectZombie.Features.Defense3D.Loadout;

namespace ProjectZombie.Features.UI
{
    public partial class CardCodexPresenter
    {
        private DefenseLoadoutRepository _loadoutRepository;
        private LoadoutSelection _loadout;
        private ILoadoutCatalog _defenseCatalog;
        private ILoadoutOwnership _defenseOwnership;
        private string _inspectedDefenseId;
        private readonly System.Collections.Generic.Dictionary<string, CodexSlotItemView> _defenseSlotViewMap
            = new System.Collections.Generic.Dictionary<string, CodexSlotItemView>(StringComparer.OrdinalIgnoreCase);
        private static readonly string[] HeroAliases = { "C001_ThuSinh", "C001", "thusinh", "ThuSinh", "Hero_ThuSinh" };
        private static readonly string[] RelicAliases = { "R001_Deptoong", "R001", "deptoong", "DepToOng", "Skill_DepToOng" };

        private void InitializeLoadout()
        {
            _loadoutRepository = _loadoutRepository ?? new DefenseLoadoutRepository();
            _defenseCatalog = Defense3DCatalog.CreateDefault();
            _defenseOwnership = DefenseMetaOwnershipAdapter.CreateDefault();
            var dto = _loadoutRepository.Load();
            _loadout = new LoadoutSelection(dto.HeroId, dto.RelicId, dto.MinionIds, dto.TowerIds);
            RefreshLoadoutSummary();
        }

        private string ResolveSupportedId(string id, bool hero)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var type = hero ? LoadoutItemType.Hero : LoadoutItemType.Relic;
            if (_defenseCatalog.TryGetEntry(id, out var entry) && entry.Type == type) return id;
            // Unknown legacy IDs must never silently equip the default hero/relic.
            return (hero ? HeroAliases : RelicAliases).Contains(id, StringComparer.OrdinalIgnoreCase)
                ? (hero ? Defense3DCatalog.HERO_THU_SINH : Defense3DCatalog.RELIC_DEP_TO_ONG) : null;
        }

        private bool IsEquipped(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            return string.Equals(id, _loadout.HeroId, StringComparison.OrdinalIgnoreCase)
                || string.Equals(id, _loadout.RelicId, StringComparison.OrdinalIgnoreCase)
                || _loadout.MinionIds.Contains(id, StringComparer.OrdinalIgnoreCase)
                || _loadout.TowerIds.Contains(id, StringComparer.OrdinalIgnoreCase);
        }

        private void RefreshEquipAction()
        {
            if (_view == null || _loadout == null) return;
            bool supported = _inspectedDefenseId != null && _defenseCatalog.TryGetEntry(_inspectedDefenseId, out _);
            bool owned = supported && _defenseOwnership.IsOwned(_inspectedDefenseId);
            bool equipped = supported && IsEquipped(_inspectedDefenseId);
            bool multiple = _currentTab == CodexTabType.Minions || _currentTab == CodexTabType.Towers;
            var rules = LoadoutRules.Default;
            int count = _currentTab == CodexTabType.Minions ? _loadout.MinionIds.Count : _loadout.TowerIds.Count;
            int max = _currentTab == CodexTabType.Minions ? rules.MaxMinionCount : rules.MaxTowerCount;
            int min = _currentTab == CodexTabType.Minions ? rules.MinMinionCount : rules.MinTowerCount;
            string label = !supported ? "CHƯA HỖ TRỢ TRONG DEFENSE" : !owned ? "CHƯA SỞ HỮU"
                : equipped ? (multiple ? "GỠ KHỎI ĐỘI HÌNH" : "ĐANG TRANG BỊ")
                : multiple && count >= max ? "ĐÃ ĐỦ SLOT" : "TRANG BỊ";
            _view.ShowEquipAction(label, owned && (multiple ? (equipped ? count > min : count < max) : !equipped));
        }

        private void HandleEquipClicked()
        {
            if (_inspectedDefenseId == null || !_defenseCatalog.TryGetEntry(_inspectedDefenseId, out var entry)
                || !entry.IsAvailable || !_defenseOwnership.IsOwned(entry.Id)) return;
            var rules = LoadoutRules.Default;
            switch (entry.Type)
            {
                case LoadoutItemType.Hero: _loadout.SetHero(entry.Id); break;
                case LoadoutItemType.Relic: _loadout.SetRelic(entry.Id); break;
                case LoadoutItemType.Minion:
                    if (IsEquipped(entry.Id)) { if (_loadout.MinionIds.Count <= rules.MinMinionCount) return; _loadout.RemoveMinion(entry.Id); }
                    else { if (_loadout.MinionIds.Count >= rules.MaxMinionCount) return; _loadout.AddMinion(entry.Id); }
                    break;
                case LoadoutItemType.Tower:
                    if (IsEquipped(entry.Id)) { if (_loadout.TowerIds.Count <= rules.MinTowerCount) return; _loadout.RemoveTower(entry.Id); }
                    else { if (_loadout.TowerIds.Count >= rules.MaxTowerCount) return; _loadout.AddTower(entry.Id); }
                    break;
            }
            _loadoutRepository.Save(new LoadoutSaveDto(_loadout.HeroId, _loadout.RelicId, _loadout.MinionIds, _loadout.TowerIds, DefenseLoadoutRepository.CURRENT_VERSION));
            _audioService?.PlayWeaponEquip();
            MainHubPresenter.Instance?.RefreshHubState();
            RefreshLoadoutSummary();
            RefreshEquippedBadges();
            foreach (var pair in _defenseSlotViewMap) pair.Value.SetEquipped(IsEquipped(pair.Key));
            if (_defenseCatalog.TryGetEntry(_inspectedDefenseId, out var inspected)
                && (_currentTab == CodexTabType.Minions || _currentTab == CodexTabType.Towers)) InspectDefenseEntry(inspected);
            RefreshEquipAction();
        }

        private UnityEngine.Sprite CatalogIcon(string id)
        {
            foreach (var hero in _cachedHeroes) if (ResolveSupportedId(hero.characterId, true) == id && hero.avatar != null) return hero.avatar;
            foreach (var relic in _cachedWeapons) if (ResolveSupportedId(relic.weaponId, false) == id && relic.icon != null) return relic.icon;
            return _view.GetCatalogIcon(id);
        }

        private void RefreshEquippedBadges()
        {
            foreach (var pair in _heroSlotViewMap) pair.Value.SetEquipped(IsEquipped(ResolveSupportedId(pair.Key.characterId, true)));
            foreach (var pair in _relicSlotViewMap) pair.Value.SetEquipped(IsEquipped(ResolveSupportedId(pair.Key.weaponId, false)));
        }

        private string DisplayName(string id) => _defenseCatalog.TryGetEntry(id, out var e) ? e.DisplayName : id;

        private void HandleLoadoutSlotClicked(CodexTabType tab, string id)
        {
            SetTab(tab);
            if (tab == CodexTabType.HeroCards)
            {
                var hero = _cachedHeroes.FirstOrDefault(h => ResolveSupportedId(h.characterId, true) == id);
                if (hero != null) { SelectHero(hero); return; }
            }
            if (tab == CodexTabType.RelicFusion)
            {
                var relic = _cachedWeapons.FirstOrDefault(w => ResolveSupportedId(w.weaponId, false) == id);
                if (relic != null) { SelectRelic(relic); return; }
            }
            if (id != null && _defenseCatalog.TryGetEntry(id, out var entry)) InspectDefenseEntry(entry);
        }

        private void RefreshLoadoutSummary()
        {
            if (_view == null || _loadout == null) return;
            var rules = LoadoutRules.Default;
            _view.ShowLoadoutSlots(_loadout.HeroId, _loadout.RelicId, _loadout.MinionIds, _loadout.TowerIds,
                rules.MaxMinionCount, rules.MaxTowerCount, DisplayName, CatalogIcon);
            _view.ShowLoadoutSummary($"Lính {_loadout.MinionIds.Count}/{rules.MaxMinionCount}  •  Trụ {_loadout.TowerIds.Count}/{rules.MaxTowerCount}");
        }

        private void PopulateDefenseGrid(CodexTabType tab)
        {
            _view.ClearGrid();
            _defenseSlotViewMap.Clear();
            _inspectedCard = null;
            var entries = _defenseCatalog.GetEntriesByType(tab == CodexTabType.Minions ? LoadoutItemType.Minion : LoadoutItemType.Tower);
            foreach (var entry in entries)
            {
                var slot = _view.CreateSlotItem();
                if (slot == null) continue;
                slot.BindUpgrade(new UpgradeSlotViewModel { UpgradeId = entry.Id, UpgradeName = entry.DisplayName, Icon = CatalogIcon(entry.Id),
                    IsSelected = entry.Id == _inspectedDefenseId }, _cardSlotWoodSprite, _cardSlotSelectedSprite,
                    () => InspectDefenseEntry(entry));
                slot.SetEquipped(IsEquipped(entry.Id));
                _defenseSlotViewMap[entry.Id] = slot;
            }
            if (!entries.Any(e => e.Id == _inspectedDefenseId)) _inspectedDefenseId = entries.FirstOrDefault()?.Id;
            if (_inspectedDefenseId != null && _defenseCatalog.TryGetEntry(_inspectedDefenseId, out var selected)) InspectDefenseEntry(selected);
            else _view.DisplayCardDetail("Chưa có nội dung", "", "Catalog chưa có loại này.", null, false);
        }

        private void AddMissingCatalogCards(CodexTabType tab)
        {
            if (tab != CodexTabType.HeroCards && tab != CodexTabType.RelicFusion) return;
            bool hero = tab == CodexTabType.HeroCards;
            foreach (var entry in _defenseCatalog.GetEntriesByType(hero ? LoadoutItemType.Hero : LoadoutItemType.Relic))
            {
                bool hasCard = hero ? _cachedHeroes.Any(h => ResolveSupportedId(h.characterId, true) == entry.Id)
                    : _cachedWeapons.Any(w => ResolveSupportedId(w.weaponId, false) == entry.Id);
                if (hasCard) continue;
                var slot = _view.CreateSlotItem();
                if (slot != null) slot.BindUpgrade(new UpgradeSlotViewModel { UpgradeId = entry.Id,
                    UpgradeName = entry.DisplayName, Icon = CatalogIcon(entry.Id), IsSelected = entry.Id == _inspectedDefenseId },
                    _cardSlotWoodSprite, _cardSlotSelectedSprite, () => InspectDefenseEntry(entry));
                if (slot != null)
                {
                    slot.SetEquipped(IsEquipped(entry.Id));
                    _defenseSlotViewMap[entry.Id] = slot;
                    if (_inspectedDefenseId == null && IsEquipped(entry.Id)) InspectDefenseEntry(entry);
                }
            }
        }

        private void InspectDefenseEntry(LoadoutCatalogEntry entry)
        {
            _inspectedDefenseId = entry.Id;
            if (_defenseSlotViewMap.TryGetValue(entry.Id, out var card)) HighlightCard(card, true, entry.DisplayName);
            _view.DisplayCardDetail(entry.DisplayName, entry.Type == LoadoutItemType.Minion ? "LÍNH" : entry.Type == LoadoutItemType.Tower ? "TRỤ" : entry.Type == LoadoutItemType.Hero ? "TƯỚNG" : "PHÁP BẢO",
                IsEquipped(entry.Id) ? "Đang có trong đội hình. Mỗi loại chỉ chiếm một slot." : "Trang bị để mang loại này vào trận. Mỗi loại chỉ chiếm một slot.", CatalogIcon(entry.Id), false);
            RefreshEquipAction();
        }
    }
}
