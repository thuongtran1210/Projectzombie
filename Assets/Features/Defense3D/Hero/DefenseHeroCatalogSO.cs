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

            // 3. Fallback cho Thư Sinh (C001 / HERO_THUSHINH / hero_thusinh)
            if (cleanId.StartsWith("C001", StringComparison.OrdinalIgnoreCase) ||
                cleanId.IndexOf("ThuSinh", StringComparison.OrdinalIgnoreCase) >= 0 ||
                cleanId.IndexOf("Scholar", StringComparison.OrdinalIgnoreCase) >= 0 ||
                cleanId.IndexOf("THUSHINH", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                for (int i = 0; i < _heroes.Count; i++)
                {
                    if (_heroes[i] != null && (_heroes[i].HeroId.IndexOf("THUSHINH", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                               _heroes[i].HeroId.IndexOf("thusinh", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                               _heroes[i].HeroId.IndexOf("C001", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        return _heroes[i];
                    }
                }
            }

            // 4. Fallback cho Đạo Sĩ (C002 / HERO_DAOSI / hero_daosi)
            if (cleanId.StartsWith("C002", StringComparison.OrdinalIgnoreCase) ||
                cleanId.IndexOf("DaoSi", StringComparison.OrdinalIgnoreCase) >= 0 ||
                cleanId.IndexOf("Taoist", StringComparison.OrdinalIgnoreCase) >= 0 ||
                cleanId.IndexOf("DAOSI", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                for (int i = 0; i < _heroes.Count; i++)
                {
                    if (_heroes[i] != null && (_heroes[i].HeroId.IndexOf("DAOSI", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                               _heroes[i].HeroId.IndexOf("daosi", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                               _heroes[i].HeroId.IndexOf("C002", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        return _heroes[i];
                    }
                }
            }

            // 5. Fallback cho Thanh Đồng (C003 / HERO_THANHDONG / hero_thanhdong)
            if (cleanId.StartsWith("C003", StringComparison.OrdinalIgnoreCase) ||
                cleanId.IndexOf("ThanhDong", StringComparison.OrdinalIgnoreCase) >= 0 ||
                cleanId.IndexOf("Medium", StringComparison.OrdinalIgnoreCase) >= 0 ||
                cleanId.IndexOf("THANHDONG", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                for (int i = 0; i < _heroes.Count; i++)
                {
                    if (_heroes[i] != null && (_heroes[i].HeroId.IndexOf("THANHDONG", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                               _heroes[i].HeroId.IndexOf("thanhdong", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                               _heroes[i].HeroId.IndexOf("C003", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        return _heroes[i];
                    }
                }
            }

            // 6. Fallback cho Ẩn Sĩ (C004 / HERO_ANSI / hero_ansi)
            if (cleanId.StartsWith("C004", StringComparison.OrdinalIgnoreCase) ||
                cleanId.IndexOf("AnSi", StringComparison.OrdinalIgnoreCase) >= 0 ||
                cleanId.IndexOf("Hermit", StringComparison.OrdinalIgnoreCase) >= 0 ||
                cleanId.IndexOf("ANSI", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                for (int i = 0; i < _heroes.Count; i++)
                {
                    if (_heroes[i] != null && (_heroes[i].HeroId.IndexOf("ANSI", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                               _heroes[i].HeroId.IndexOf("ansi", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                               _heroes[i].HeroId.IndexOf("C004", StringComparison.OrdinalIgnoreCase) >= 0))
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
