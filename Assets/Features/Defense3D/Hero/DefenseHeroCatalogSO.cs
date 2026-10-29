using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Danh mục Catalog toàn bộ Anh Hùng 3D (ScriptableObject Database).
    /// Đóng vai trò Single Source of Truth cho việc tra cứu, nạp dữ liệu và xuất trận trong Defense 3D.
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
        /// Tra cứu định nghĩa Tướng 3D theo mã định danh.
        /// Tự động chuẩn hóa và ánh xạ các ID cũ (C001, C001_Hero, HERO_THUSHINH) và fallback an toàn.
        /// </summary>
        public DefenseHeroDefinitionSO GetHeroById(string heroId)
        {
            if (string.IsNullOrWhiteSpace(heroId))
            {
                return DefaultHero;
            }

            string cleanId = heroId.Trim();

            // 1. Tìm chính xác theo HeroId
            for (int i = 0; i < _heroes.Count; i++)
            {
                if (_heroes[i] != null && string.Equals(_heroes[i].HeroId, cleanId, StringComparison.OrdinalIgnoreCase))
                {
                    return _heroes[i];
                }
            }

            // 2. Tìm theo DisplayName
            for (int i = 0; i < _heroes.Count; i++)
            {
                if (_heroes[i] != null && string.Equals(_heroes[i].DisplayName, cleanId, StringComparison.OrdinalIgnoreCase))
                {
                    return _heroes[i];
                }
            }

            // 3. Fallback cho Thư Sinh (C001)
            if (cleanId.StartsWith("C001", StringComparison.OrdinalIgnoreCase) ||
                cleanId.IndexOf("ThuSinh", StringComparison.OrdinalIgnoreCase) >= 0 ||
                cleanId.IndexOf("Scholar", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                for (int i = 0; i < _heroes.Count; i++)
                {
                    if (_heroes[i] != null && (_heroes[i].HeroId.Contains("THUSHINH") || _heroes[i].HeroId.Contains("C001")))
                    {
                        return _heroes[i];
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
