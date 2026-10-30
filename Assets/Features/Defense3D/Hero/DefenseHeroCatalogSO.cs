using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Defense3D.Loadout;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Danh mục Catalog toàn bộ Anh Hùng 3D (ScriptableObject Database).
    /// Đóng vai trò Single Source of Truth cho việc tra cứu, nạp dữ liệu và xuất trận trong Defense 3D.
    /// Sử dụng DefenseLoadoutIdMapper thống nhất thay vì tự tạo mapping cục bộ.
    /// </summary>
    [CreateAssetMenu(fileName = "DefenseHeroCatalog", menuName = "Defense3D/Hero Catalog")]
    public sealed class DefenseHeroCatalogSO : ScriptableObject
    {
        [Header("Danh Sách Anh Hùng 3D")]
        [SerializeField] private List<DefenseHeroDefinitionSO> _heroes = new List<DefenseHeroDefinitionSO>();

        [Header("Anh Hùng Mặc Định (Fallback)")]
        [SerializeField] private DefenseHeroDefinitionSO _defaultHero;

        public IReadOnlyList<DefenseHeroDefinitionSO> Heroes => _heroes;
        public DefenseHeroDefinitionSO DefaultHero => _defaultHero != null ? _defaultHero : (_heroes.Count > 0 ? _heroes[0] : null);

        /// <summary>
        /// Tra cứu định nghĩa Tướng 3D theo mã định danh thông qua DefenseLoadoutIdMapper (Single Source of Truth).
        /// </summary>
        public DefenseHeroDefinitionSO GetHeroById(string heroId)
        {
            if (string.IsNullOrWhiteSpace(heroId))
            {
                return DefaultHero;
            }

            string cleanId = heroId.Trim();

            // 1. Tìm chính xác theo HeroId hoặc DisplayName
            for (int i = 0; i < _heroes.Count; i++)
            {
                var hero = _heroes[i];
                if (hero == null) continue;

                if (string.Equals(hero.HeroId, cleanId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(hero.DisplayName, cleanId, StringComparison.OrdinalIgnoreCase))
                {
                    return hero;
                }
            }

            // 2. Chuẩn hóa qua DefenseLoadoutIdMapper (Single Source of Truth)
            if (DefenseLoadoutIdMapper.TryMapHeroId(cleanId, out string canonicalHeroId))
            {
                for (int i = 0; i < _heroes.Count; i++)
                {
                    var hero = _heroes[i];
                    if (hero == null) continue;

                    if (string.Equals(hero.HeroId, canonicalHeroId, StringComparison.OrdinalIgnoreCase) ||
                        (DefenseLoadoutIdMapper.TryMapHeroId(hero.HeroId, out string entryCanonicalId) &&
                         string.Equals(entryCanonicalId, canonicalHeroId, StringComparison.OrdinalIgnoreCase)))
                    {
                        return hero;
                    }
                }
            }

            return DefaultHero;
        }

        public void SetHeroes(List<DefenseHeroDefinitionSO> list, DefenseHeroDefinitionSO defaultHero = null)
        {
            _heroes = list ?? new List<DefenseHeroDefinitionSO>();
            _defaultHero = defaultHero;
        }
    }
}
