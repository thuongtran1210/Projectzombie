using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D.Loadout
{
    /// <summary>
    /// Adapter Runtime kết nối giữa dữ liệu Loadout thuần C# đã phân giải (DefenseResolvedLoadout)
    /// và các MonoBehaviour / ScriptableObject trong Scene Defense 3D.
    /// </summary>
    public static class DefenseLoadoutRuntimeAdapter
    {
        /// <summary>
        /// Áp dụng danh sách vai trò lính được phép tuyển mộ vào DefenseVillageShop3D.
        /// </summary>
        public static void ApplyToShop(DefenseResolvedLoadout resolved, DefenseVillageShop3D shop)
        {
            if (resolved == null || shop == null) return;
            shop.SetAllowedRoles(resolved.AllowedMinionRoles);
        }

        /// <summary>
        /// Áp dụng Pháp Bảo (Relic) được chọn từ Loadout vào Hero Kit của Tướng.
        /// </summary>
        public static void ApplyToHeroKit(
            DefenseResolvedLoadout resolved,
            DefenseHeroKitSO heroKit,
            DefenseSkillDefinitionSO relicDefinition = null)
        {
            if (resolved == null || heroKit == null) return;
            if (relicDefinition != null)
            {
                heroKit.SetRelic(relicDefinition);
            }
        }

        /// <summary>
        /// Áp dụng dữ liệu Anh Hùng được chọn từ Loadout (HeroId) vào trận đấu 3D:
        /// Đồng bộ chỉ số cơ bản, kit kỹ năng và visual controller.
        /// </summary>
        public static void ApplyHero(
            DefenseResolvedLoadout resolved,
            DefenseHeroCatalogSO catalog,
            DefenseHeroHealth3D health,
            DefenseHeroMotor3D motor,
            DefenseHeroAutoAttack3D attack,
            DefenseHeroVisualController3D visual)
        {
            if (resolved == null) return;

            DefenseHeroDefinitionSO heroDef = null;
            if (catalog != null)
            {
                heroDef = catalog.GetHeroById(resolved.HeroId);
            }

            if (heroDef != null)
            {
                heroDef.ApplyStatsToHero(health, motor, attack);
                if (visual != null && heroDef.HeroKit != null)
                {
                    visual.SetKit(heroDef.HeroKit);
                }
            }
        }
    }
}
