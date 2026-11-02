using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Core.Architecture;
using ProjectZombie.Features.MetaProgression;
using ProjectZombie.Features.Shared.Loadout;

namespace ProjectZombie.Features.Defense3D.Loadout
{
    /// <summary>
    /// Triển khai thực tế ILoadoutService cho chế độ Defense 3D.
    /// Đóng gói việc truy cập Repository, Catalog, Ownership Adapter và Runtime Bonus Adapters.
    /// </summary>
    public class DefenseLoadoutService : ILoadoutService
    {
        private readonly ILoadoutRepository _repository;
        private readonly ILoadoutCatalog _catalog;
        private readonly ILoadoutOwnership _ownership;

        public LoadoutRules Rules => LoadoutRules.Default;

        public DefenseLoadoutService(ILoadoutRepository repository, ILoadoutCatalog catalog, ILoadoutOwnership ownership)
        {
            _repository = repository ?? new DefenseLoadoutRepository();
            _catalog = catalog ?? Defense3DCatalog.CreateExpanded();
            _ownership = ownership ?? DefenseMetaOwnershipAdapter.CreateDefault(
                ProjectZombie.Core.Save.GameManager.HasInstance ? ProjectZombie.Core.Save.GameManager.Instance.SaveData : null);
        }

        /// <summary>
        /// Tạo một instance mặc định dựa trên dữ liệu lưu và catalog mở rộng hiện tại.
        /// </summary>
        public static DefenseLoadoutService CreateDefault()
        {
            var repo = new DefenseLoadoutRepository();
            var catalog = Defense3DCatalog.CreateExpanded();
            var saveData = ProjectZombie.Core.Save.GameManager.HasInstance ? ProjectZombie.Core.Save.GameManager.Instance.SaveData : null;
            var ownership = DefenseMetaOwnershipAdapter.CreateDefault(saveData);
            if (saveData != null && saveData.unlockedTowerIds != null)
            {
                for (int i = 0; i < saveData.unlockedTowerIds.Count; i++)
                {
                    ownership.Grant(saveData.unlockedTowerIds[i]);
                }
            }
            return new DefenseLoadoutService(repo, catalog, ownership);
        }

        public LoadoutSelection LoadCurrentLoadout()
        {
            var dto = _repository.Load();
            return new LoadoutSelection(dto.HeroId, dto.RelicId, dto.MinionIds, dto.TowerIds);
        }

        public void SaveLoadout(LoadoutSelection selection)
        {
            if (selection == null) return;
            _repository.Save(new LoadoutSaveDto(
                selection.HeroId,
                selection.RelicId,
                selection.MinionIds,
                selection.TowerIds,
                DefenseLoadoutRepository.CURRENT_VERSION));
        }

        public bool IsOwned(string id)
        {
            return _ownership != null && _ownership.IsOwned(id);
        }

        public bool TryGetCatalogEntry(string id, out LoadoutCatalogEntry entry)
        {
            if (_catalog != null)
            {
                return _catalog.TryGetEntry(id, out entry);
            }
            entry = null;
            return false;
        }

        public IReadOnlyList<LoadoutCatalogEntry> GetCatalogEntriesByType(LoadoutItemType type)
        {
            return _catalog != null ? _catalog.GetEntriesByType(type) : Array.Empty<LoadoutCatalogEntry>();
        }

        public string ResolveSupportedId(string id, bool isHero)
        {
            if (string.IsNullOrEmpty(id)) return null;
            string clean = id.Trim();
            var type = isHero ? LoadoutItemType.Hero : LoadoutItemType.Relic;
            if (_catalog != null && _catalog.TryGetEntry(clean, out var entry) && entry.Type == type)
            {
                return entry.Id;
            }

            if (isHero)
            {
                return DefenseLoadoutIdMapper.TryMapHeroId(clean, out var mappedHero) ? mappedHero : null;
            }

            return DefenseLoadoutIdMapper.TryMapRelicId(clean, out var mappedRelic) ? mappedRelic : null;
        }

        public Sprite ResolveItemIcon(string id)
        {
            return DefenseLoadoutRuntimeAdapter.ResolveItemIcon(id);
        }

        public string GetDisplayName(string id)
        {
            return _catalog != null && _catalog.TryGetEntry(id, out var e) ? e.DisplayName : id;
        }

        public void EvaluateMinionStarBonuses(int star, out float hpBonus, out float spdBonus)
        {
            DefenseLoadoutRuntimeAdapter.EvaluateMinionStarBonuses(star, out hpBonus, out spdBonus);
        }

        public void EvaluateTowerStarBonuses(int star, out float dmgBonus, out float rangeBonus)
        {
            DefenseLoadoutRuntimeAdapter.EvaluateTowerStarBonuses(star, out dmgBonus, out rangeBonus);
        }

        public void EvaluateRelicStarBonuses(int star, RelicStarProgressionSO config, out float dmgBonus, out float cdReduction, out float areaBonus)
        {
            DefenseLoadoutRuntimeAdapter.EvaluateRelicStarBonuses(star, config, out dmgBonus, out cdReduction, out areaBonus);
        }
    }
}
