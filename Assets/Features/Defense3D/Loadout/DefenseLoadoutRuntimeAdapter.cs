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
    }
}
