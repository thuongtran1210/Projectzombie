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
        /// Áp dụng danh sách loại trụ phòng thủ được phép xây vào DefenseTowerBuildPrototype3D.
        /// </summary>
        public static void ApplyTowers(DefenseResolvedLoadout resolved, DefenseTowerBuildPrototype3D towerBuilder)
        {
            if (resolved == null || towerBuilder == null) return;
            towerBuilder.SetAllowedTowers(resolved.TowerIds);
        }

        /// <summary>
        /// Áp dụng Pháp Bảo (Relic) được chọn từ Loadout vào Hero Kit của Tướng.
        /// Tự động nạp SkillDefinition tương ứng từ Resources nếu chưa được truyền vào.
        /// </summary>
        public static void ApplyToHeroKit(
            DefenseResolvedLoadout resolved,
            DefenseHeroKitSO heroKit,
            DefenseSkillDefinitionSO relicDefinition = null)
        {
            if (resolved == null || heroKit == null) return;

            if (relicDefinition == null && !string.IsNullOrEmpty(resolved.RelicId))
            {
                if (string.Equals(resolved.RelicId, Defense3DCatalog.RELIC_DEP_TO_ONG, StringComparison.OrdinalIgnoreCase))
                {
                    relicDefinition = Resources.Load<DefenseSkillDefinitionSO>("Defense3D/Skill_DepToOng");
                }
            }

            if (relicDefinition != null)
            {
                heroKit.SetRelic(relicDefinition);
            }
        }

        /// <summary>
        /// Áp dụng dữ liệu Anh Hùng được chọn từ Loadout (HeroId) vào trận đấu 3D:
        /// Đồng bộ chỉ số cơ bản, kit kỹ năng, visual controller và cộng dồn bonus từ Meta Progression (cấp sao).
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
                float hpMultiplier = 1f;
                float speedMultiplier = 1f;

                // Tích hợp bonus từ Cấp Sao Nhân Vật (Meta Progression) nếu có
                try
                {
                    var progManager = ProjectZombie.Features.MetaProgression.CharacterProgressionManager.Instance;
                    if (progManager != null)
                    {
                        int star = progManager.GetCharacterStarLevel(resolved.HeroId);
                        if (star > 1 && progManager.ProgressionConfig != null)
                        {
                            var stepConfig = progManager.ProgressionConfig.GetStepConfig(star);
                            if (stepConfig != null)
                            {
                                hpMultiplier += stepConfig.healthMultiplierBonus;
                                speedMultiplier += stepConfig.moveSpeedMultiplierBonus;
                            }
                        }
                    }
                }
                catch
                {
                    // An toàn không làm đứt đoạn vòng đời khởi tạo trận đấu
                }

                if (health != null)
                {
                    health.SetMaxHealth(heroDef.BaseMaxHp * hpMultiplier);
                }

                if (motor != null)
                {
                    motor.SetMovementParameters(heroDef.BaseMoveSpeed * speedMultiplier, heroDef.TurnRate);
                }

                if (visual != null && heroDef.HeroKit != null)
                {
                    visual.SetKit(heroDef.HeroKit);
                }
            }
        }
    }
}
