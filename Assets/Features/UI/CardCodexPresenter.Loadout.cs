using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectZombie.Features.Defense3D.Loadout;
using ProjectZombie.Features.MetaProgression;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.UI
{
    public partial class CardCodexPresenter
    {
        private DefenseLoadoutRepository _loadoutRepository;
        private LoadoutSelection _loadout;
        private ILoadoutCatalog _defenseCatalog;
        private ILoadoutOwnership _defenseOwnership;
        private string _inspectedDefenseId;
        private string _replacementId;
        private bool _choosingLoadoutSlot;
        private readonly Dictionary<string, CodexSlotItemView> _defenseSlotViewMap
            = new Dictionary<string, CodexSlotItemView>(StringComparer.OrdinalIgnoreCase);

        private void InitializeLoadout()
        {
            _loadoutRepository = _loadoutRepository ?? new DefenseLoadoutRepository();
            _defenseCatalog = Defense3DCatalog.CreateDefault();
            var saveData = ProjectZombie.Core.Save.GameManager.HasInstance ? ProjectZombie.Core.Save.GameManager.Instance.SaveData : null;
            var ownership = DefenseMetaOwnershipAdapter.CreateDefault(saveData);
            if (saveData != null && saveData.unlockedTowerIds != null)
            {
                for (int i = 0; i < saveData.unlockedTowerIds.Count; i++)
                {
                    ownership.Grant(saveData.unlockedTowerIds[i]);
                }
            }
            _defenseOwnership = ownership;
            var dto = _loadoutRepository.Load();
            _loadout = new LoadoutSelection(dto.HeroId, dto.RelicId, dto.MinionIds, dto.TowerIds);
            RefreshLoadoutSummary();
        }

        private string ResolveSupportedId(string id, bool hero)
        {
            if (string.IsNullOrEmpty(id)) return null;
            string clean = id.Trim();
            var type = hero ? LoadoutItemType.Hero : LoadoutItemType.Relic;
            if (_defenseCatalog != null && _defenseCatalog.TryGetEntry(clean, out var entry) && entry.Type == type)
            {
                return entry.Id;
            }

            if (hero)
            {
                return DefenseLoadoutIdMapper.TryMapHeroId(clean, out var mappedHero) ? mappedHero : null;
            }

            return DefenseLoadoutIdMapper.TryMapRelicId(clean, out var mappedRelic) ? mappedRelic : null;
        }

        private bool IsEquipped(string id)
        {
            if (string.IsNullOrEmpty(id) || _loadout == null) return false;
            return string.Equals(id, _loadout.HeroId, StringComparison.OrdinalIgnoreCase)
                || string.Equals(id, _loadout.RelicId, StringComparison.OrdinalIgnoreCase)
                || _loadout.MinionIds.Contains(id, StringComparer.OrdinalIgnoreCase)
                || _loadout.TowerIds.Contains(id, StringComparer.OrdinalIgnoreCase);
        }

        private void RefreshEquipAction()
        {
            if (_view == null || _loadout == null) return;
            if (!_equipmentMode)
            {
                _view.ShowEquipAction(string.Empty, false);
                return;
            }
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
                : multiple && count >= max ? "ĐÃ ĐỦ SLOT" : "TRANG BỊ XUẤT TRẬN";
            bool replacing = multiple && _replacementId != null;
            bool enabled = owned && (multiple ? (equipped ? count > min : count < max) : !equipped);
            string reason = string.Empty;
            if (!owned && supported)
            {
                reason = "Mở khóa thẻ trước khi đưa vào đội hình.";
            }
            else if (replacing)
            {
                enabled = owned && !equipped;
                label = equipped ? "CHỌN THẺ KHÁC ĐỂ THAY" : "THAY VỊ TRÍ ĐÃ CHỌN";
                reason = "Thay vị trí: " + DisplayName(_replacementId) + ". Chọn thẻ khác rồi xác nhận.";
            }
            else if (multiple && equipped && count <= min)
            {
                label = $"GIỮ ÍT NHẤT {min} " + (_currentTab == CodexTabType.Minions ? "LOẠI LÍNH" : "LOẠI TRỤ");
                reason = "Đội hình cần giữ số lượng tối thiểu. Chạm vị trí phía trên để thay thẻ.";
            }
            else if (multiple && !equipped && count >= max)
                reason = "Đội hình đã đầy. Chạm vị trí phía trên để chọn thẻ cần thay.";
            else if (_choosingLoadoutSlot && !multiple && !equipped)
                label = _currentTab == CodexTabType.HeroCards ? "CHỌN ANH HÙNG NÀY" : "CHỌN THẦN KHÍ NÀY";
            else if (!multiple && !equipped && owned) label = "ĐƯA VÀO ĐỘI HÌNH";
            _view.ShowEquipAction(label, enabled);
            if (reason.Length > 0) _view.ShowUpgradeOutcome("<color=#FFAA00>" + reason + "</color>");
        }

        private void HandleEquipClicked()
        {
            if (!_equipmentMode) return;
            if (_inspectedDefenseId == null || !_defenseCatalog.TryGetEntry(_inspectedDefenseId, out var entry)
                || !entry.IsAvailable || !_defenseOwnership.IsOwned(entry.Id)) return;
            var rules = LoadoutRules.Default;
            switch (entry.Type)
            {
                case LoadoutItemType.Hero:
                    _loadout.SetHero(entry.Id);
                    SyncEquippedHeroToGlobalState(entry.Id);
                    break;
                case LoadoutItemType.Relic:
                    _loadout.SetRelic(entry.Id);
                    break;
                case LoadoutItemType.Minion:
                    if (_replacementId != null)
                    {
                        if (!_loadout.ReplaceMinion(_replacementId, entry.Id)) return;
                        _replacementId = entry.Id;
                        break;
                    }
                    if (IsEquipped(entry.Id))
                    {
                        if (_loadout.MinionIds.Count <= rules.MinMinionCount) return;
                        _loadout.RemoveMinion(entry.Id);
                    }
                    else
                    {
                        if (_loadout.MinionIds.Count >= rules.MaxMinionCount) return;
                        _loadout.AddMinion(entry.Id);
                    }
                    break;
                case LoadoutItemType.Tower:
                    if (_replacementId != null)
                    {
                        if (!_loadout.ReplaceTower(_replacementId, entry.Id)) return;
                        _replacementId = entry.Id;
                        break;
                    }
                    if (IsEquipped(entry.Id))
                    {
                        if (_loadout.TowerIds.Count <= rules.MinTowerCount) return;
                        _loadout.RemoveTower(entry.Id);
                    }
                    else
                    {
                        if (_loadout.TowerIds.Count >= rules.MaxTowerCount) return;
                        _loadout.AddTower(entry.Id);
                    }
                    break;
            }
            _loadoutRepository.Save(new LoadoutSaveDto(_loadout.HeroId, _loadout.RelicId, _loadout.MinionIds, _loadout.TowerIds, DefenseLoadoutRepository.CURRENT_VERSION));
            _audioService?.PlayWeaponEquip();
            MainHubPresenter.Instance?.RefreshHubState();
            RefreshLoadoutSummary();
            RefreshEquippedBadges();
            foreach (var pair in _defenseSlotViewMap) pair.Value.SetEquipped(IsEquipped(pair.Key));
            if (_defenseCatalog.TryGetEntry(_inspectedDefenseId, out var inspected)
                && (_currentTab == CodexTabType.Minions || _currentTab == CodexTabType.Towers || (_currentTab == CodexTabType.RelicFusion && _selectedRelic == null)))
            {
                InspectDefenseEntry(inspected);
            }
            RefreshEquipAction();
        }

        private void SyncEquippedHeroToGlobalState(string defenseHeroId)
        {
            var matchedSo = _cachedHeroes.FirstOrDefault(h => h != null && string.Equals(ResolveSupportedId(h.characterId, true), defenseHeroId, StringComparison.OrdinalIgnoreCase));
            if (matchedSo != null)
            {
                ProjectZombie.Features.Player.RunLoadoutState.SetCharacter(matchedSo.ToEntry());
                if (ProjectZombie.Core.Save.GameManager.HasInstance && ProjectZombie.Core.Save.GameManager.Instance.SaveData != null)
                {
                    ProjectZombie.Core.Save.GameManager.Instance.SaveData.selectedHeroId = matchedSo.characterId;
                    ProjectZombie.Core.Save.GameManager.Instance.SaveGame();
                }
            }
        }

        private Sprite CatalogIcon(string id)
        {
            foreach (var hero in _cachedHeroes) if (hero != null && ResolveSupportedId(hero.characterId, true) == id && hero.avatar != null) return hero.avatar;
            foreach (var relic in _cachedWeapons) if (relic != null && ResolveSupportedId(relic.weaponId, false) == id && relic.icon != null) return relic.icon;
            return _view != null ? _view.GetCatalogIcon(id) : null;
        }

        private void RefreshEquippedBadges()
        {
            foreach (var pair in _heroSlotViewMap) pair.Value.SetEquipped(IsEquipped(ResolveSupportedId(pair.Key.characterId, true)));
            foreach (var pair in _relicSlotViewMap) pair.Value.SetEquipped(IsEquipped(ResolveSupportedId(pair.Key.weaponId, false)));
            foreach (var pair in _defenseSlotViewMap) pair.Value.SetEquipped(IsEquipped(pair.Key));
        }

        private string DisplayName(string id) => _defenseCatalog.TryGetEntry(id, out var e) ? e.DisplayName : id;

        private void HandleLoadoutSlotClicked(CodexTabType tab, string id)
        {
            SetTab(tab);
            _choosingLoadoutSlot = true;
            _replacementId = tab == CodexTabType.Minions || tab == CodexTabType.Towers ? id : null;
            RefreshLoadoutSummary();
            if (tab == CodexTabType.HeroCards)
            {
                var hero = _cachedHeroes.FirstOrDefault(h => h != null && ResolveSupportedId(h.characterId, true) == id);
                if (hero != null) { SelectHero(hero); return; }
            }
            if (tab == CodexTabType.RelicFusion)
            {
                var relic = _cachedWeapons.FirstOrDefault(w => w != null && ResolveSupportedId(w.weaponId, false) == id);
                if (relic != null) { SelectRelic(relic); return; }
            }
            if (id != null && _defenseCatalog.TryGetEntry(id, out var entry)) InspectDefenseEntry(entry);
            RefreshEquipAction();
        }

        private void RefreshLoadoutSummary()
        {
            if (_view == null || _loadout == null) return;
            var rules = LoadoutRules.Default;
            _view.ShowLoadoutSlots(_loadout.HeroId, _loadout.RelicId, _loadout.MinionIds, _loadout.TowerIds,
                rules.MaxMinionCount, rules.MaxTowerCount, DisplayName, CatalogIcon);
            string context = _replacementId != null ? $" • Thay: {DisplayName(_replacementId)}"
                : _choosingLoadoutSlot ? " • Đang chọn cho đội hình" : " • Bộ sưu tập";
            _view.ShowLoadoutSummary($"Lính {_loadout.MinionIds.Count}/{rules.MaxMinionCount}  •  Trụ {_loadout.TowerIds.Count}/{rules.MaxTowerCount}{context}");
        }

        private void PopulateDefenseGrid(CodexTabType tab)
        {
            _view.ClearGrid();
            _defenseSlotViewMap.Clear();
            _inspectedCard = null;
            var itemType = tab == CodexTabType.RelicFusion ? LoadoutItemType.Relic
                : (tab == CodexTabType.Minions ? LoadoutItemType.Minion : LoadoutItemType.Tower);
            var entries = _defenseCatalog.GetEntriesByType(itemType);
            var relicMgr = RelicInventoryManager.HasInstance ? RelicInventoryManager.Instance : null;
            if (relicMgr != null) relicMgr.EnsureInitialized();

            foreach (var entry in entries)
            {
                var slot = _view.CreateSlotItem();
                if (slot == null) continue;
                int star = GetDefenseUnitStar(entry.Id);
                int shards = relicMgr != null ? relicMgr.GetRelicShardCount(entry.Id) : 0;
                var nextStep = relicMgr != null ? relicMgr.GetNextStepConfig(entry.Id) : null;
                int reqShards = nextStep != null ? nextStep.requiredShards : 5;

                slot.BindRelic(new RelicSlotViewModel
                {
                    WeaponId = entry.Id,
                    WeaponName = entry.DisplayName,
                    Icon = CatalogIcon(entry.Id),
                    Rarity = entry.Type == LoadoutItemType.Tower || entry.Type == LoadoutItemType.Relic ? ItemRarity.Epic : ItemRarity.Rare,
                    NameColor = new Color(0.98f, 0.90f, 0.72f, 1f),
                    StarLevel = star,
                    ShardCount = shards,
                    ReqShards = reqShards,
                    IsUnlocked = _defenseOwnership.IsOwned(entry.Id),
                    IsSelected = entry.Id == _inspectedDefenseId
                }, _cardSlotWoodSprite, _cardSlotSelectedSprite, () =>
                {
                    _audioService?.PlayUIClick();
                    InspectDefenseEntry(entry);
                });
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
            var relicMgr = RelicInventoryManager.HasInstance ? RelicInventoryManager.Instance : null;
            if (relicMgr != null) relicMgr.EnsureInitialized();

            foreach (var entry in _defenseCatalog.GetEntriesByType(hero ? LoadoutItemType.Hero : LoadoutItemType.Relic))
            {
                bool hasCard = hero ? _cachedHeroes.Any(h => h != null && ResolveSupportedId(h.characterId, true) == entry.Id)
                    : (_cachedWeapons.Any(w => w != null && ResolveSupportedId(w.weaponId, false) == entry.Id) || _defenseSlotViewMap.ContainsKey(entry.Id));
                if (hasCard) continue;

                var slot = _view.CreateSlotItem();
                if (slot != null)
                {
                    int star = GetDefenseUnitStar(entry.Id);
                    int shards = relicMgr != null ? relicMgr.GetRelicShardCount(entry.Id) : 0;
                    var nextStep = relicMgr != null ? relicMgr.GetNextStepConfig(entry.Id) : null;
                    int reqShards = nextStep != null ? nextStep.requiredShards : 5;

                    slot.BindRelic(new RelicSlotViewModel
                    {
                        WeaponId = entry.Id,
                        WeaponName = entry.DisplayName,
                        Icon = CatalogIcon(entry.Id),
                        Rarity = ItemRarity.Epic,
                        NameColor = new Color(0.98f, 0.88f, 0.50f, 1f),
                        StarLevel = star,
                        ShardCount = shards,
                        ReqShards = reqShards,
                        IsUnlocked = _defenseOwnership.IsOwned(entry.Id),
                        IsSelected = entry.Id == _inspectedDefenseId
                    }, _cardSlotWoodSprite, _cardSlotSelectedSprite, () =>
                    {
                        _audioService?.PlayUIClick();
                        InspectDefenseEntry(entry);
                    });

                    slot.transform.SetAsFirstSibling();
                    slot.SetEquipped(IsEquipped(entry.Id));
                    _defenseSlotViewMap[entry.Id] = slot;

                    if (!hero && IsEquipped(entry.Id))
                    {
                        InspectDefenseEntry(entry);
                    }
                }
            }
        }

        private int GetDefenseUnitStar(string unitId)
        {
            var relicMgr = RelicInventoryManager.HasInstance ? RelicInventoryManager.Instance : null;
            int star = relicMgr != null ? relicMgr.GetRelicStarLevel(unitId) : 0;
            if (star <= 0 && _defenseOwnership != null && _defenseOwnership.IsOwned(unitId))
            {
                star = 1;
            }
            return Mathf.Clamp(star, 0, 5);
        }

        private void InspectDefenseEntry(LoadoutCatalogEntry entry)
        {
            if (entry == null) return;
            _inspectedDefenseId = entry.Id;
            if (_currentTab == CodexTabType.RelicFusion)
            {
                _selectedRelic = null;
            }

            if (_defenseSlotViewMap.TryGetValue(entry.Id, out var card))
            {
                HighlightCard(card, _defenseOwnership.IsOwned(entry.Id), entry.DisplayName);
            }

            var relicMgr = RelicInventoryManager.HasInstance ? RelicInventoryManager.Instance : null;
            if (relicMgr != null) relicMgr.EnsureInitialized();

            int star = GetDefenseUnitStar(entry.Id);
            int shards = relicMgr != null ? relicMgr.GetRelicShardCount(entry.Id) : 0;
            var nextStep = relicMgr != null ? relicMgr.GetNextStepConfig(entry.Id) : null;
            int reqShards = nextStep != null ? nextStep.requiredShards : 5;
            int cost = nextStep != null ? nextStep.coTienCost : 0;
            bool canFuse = relicMgr != null && relicMgr.CanFuse(entry.Id, out _);

            string btnLabel;
            if (star <= 0)
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

            BuildDefenseEntryDetailText(entry, star, nextStep, out string categoryTag, out string desc);
            _view.DisplayRelicDetail(entry.DisplayName, categoryTag, desc, CatalogIcon(entry.Id), star, shards, reqShards, cost, canFuse, btnLabel);
            ShowUpgradeAvailability(star, shards, reqShards, cost, canFuse,
                relicMgr == null ? "Dữ liệu đang được khởi tạo." : nextStep == null ? "Không tìm thấy cấu hình nâng cấp." : string.Empty);
            RefreshEquipAction();
        }

        private void BuildDefenseEntryDetailText(LoadoutCatalogEntry entry, int star, RelicStarStepConfig nextStep, out string categoryTag, out string desc)
        {
            string equipState = IsEquipped(entry.Id)
                ? "<color=#00FF88>[ĐANG TRANG BỊ TRONG ĐỘI HÌNH]</color>"
                : "<color=#D1D5DB>[CHƯA TRANG BỊ]</color>";
            string baseDesc = !string.IsNullOrEmpty(entry.Description) ? entry.Description : entry.DisplayName;

            if (entry.Type == LoadoutItemType.Minion)
            {
                categoryTag = $"<color=#4CAF50>[QUÂN LÍNH LÀNG 3D]</color>  {equipState}";
                DefenseLoadoutRuntimeAdapter.EvaluateMinionStarBonuses(star, out float hpBonus, out float spdBonus);
                desc = $"{baseDesc}\n\n" +
                       $"<color=#FFD700>Chỉ Số Cấp {star} Sao:</color> Sinh Lực Lính <b>+{hpBonus * 100f:0}%</b>  •  Tốc Chạy <b>+{spdBonus * 100f:0}%</b>\n";
                if (nextStep != null && star < 5)
                {
                    DefenseLoadoutRuntimeAdapter.EvaluateMinionStarBonuses(star + 1, out float nextHp, out float nextSpd);
                    desc += $"<color=#00FF88>Đột Phá Kế Tiếp ({star + 1} Sao):</color> Sinh Lực Lính <b>+{nextHp * 100f:0}%</b>  •  Tốc Chạy <b>+{nextSpd * 100f:0}%</b>";
                }
                return;
            }

            if (entry.Type == LoadoutItemType.Tower)
            {
                categoryTag = $"<color=#00E5FF>[TRỤ PHÒNG THỦ]</color>  {equipState}";
                DefenseLoadoutRuntimeAdapter.EvaluateTowerStarBonuses(star, out float dmgBonus, out float rangeBonus);
                desc = $"{baseDesc}\n\n" +
                       "<color=#D1D5DB>Trong trận, toàn bộ trụ dùng bonus chung theo thẻ trụ có sao cao nhất trong đội hình. Nâng thẻ thấp hơn mốc này chưa làm tăng bonus chung.</color>\n\n" +
                       $"<color=#FFD700>Chỉ Số Cấp {star} Sao:</color> Sát Thương Trụ <b>+{dmgBonus * 100f:0}%</b>  •  Tầm Bắn <b>+{rangeBonus * 100f:0}%</b>\n";
                if (nextStep != null && star < 5)
                {
                    DefenseLoadoutRuntimeAdapter.EvaluateTowerStarBonuses(star + 1, out float nextDmg, out float nextRng);
                    desc += $"<color=#00FF88>Đột Phá Kế Tiếp ({star + 1} Sao):</color> Sát Thương Trụ <b>+{nextDmg * 100f:0}%</b>  •  Tầm Bắn <b>+{nextRng * 100f:0}%</b>";
                }
                return;
            }

            categoryTag = $"<color=#FFD700>[PHÁP BẢO CHIẾN TRƯỜNG 3D]</color>  {equipState}";
            DefenseLoadoutRuntimeAdapter.EvaluateRelicStarBonuses(star, null, out float relicDmg, out float relicCd, out float relicArea);
            desc = $"{baseDesc}\n\n" +
                   $"<color=#FFD700>Chỉ Số Cấp {star} Sao:</color> Sát Thương <b>+{relicDmg * 100f:0}%</b>  •  Giảm Hồi Chiêu <b>-{relicCd * 100f:0}%</b>  •  Phạm Vi <b>+{relicArea * 100f:0}%</b>\n";
            if (nextStep != null && star < 5)
            {
                desc += $"<color=#00FF88>Đột Phá Kế Tiếp ({nextStep.targetStar} Sao):</color> {nextStep.perkDescription}";
            }
        }
    }
}
