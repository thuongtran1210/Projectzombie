using System;
using UnityEngine;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Features.Defense3D.Loadout
{
    /// <summary>
    /// Adapter Runtime kết nối giữa dữ liệu Loadout thuần C# đã phân giải (DefenseResolvedLoadout),
    /// hệ thống tiến trình Meta (Cấp Sao Tướng, Cấp Sao Pháp Bảo, Cây Thiên Phú Miếu Cổ)
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
        /// Lấy cấp sao hiệu lực (1..5★) của Tướng, đồng bộ xuyên suốt giữa ID Defense 3D và ID Meta cũ.
        /// </summary>
        public static int GetEffectiveHeroStarLevel(string heroId, MetaProgressionSaveData saveData = null)
        {
            if (string.IsNullOrWhiteSpace(heroId)) return 1;

            int star = 0;
            string clean = heroId.Trim();
            string legacyAlias = GetLegacyHeroAlias(clean);

            try
            {
                if (saveData != null)
                {
                    star = Mathf.Max(star, saveData.GetCharacterStarLevel(clean));
                    if (!string.IsNullOrEmpty(legacyAlias))
                    {
                        star = Mathf.Max(star, saveData.GetCharacterStarLevel(legacyAlias));
                    }
                }
                else
                {
                    var progManager = CharacterProgressionManager.Instance;
                    if (progManager != null)
                    {
                        star = Mathf.Max(star, progManager.GetCharacterStarLevel(clean));
                        if (!string.IsNullOrEmpty(legacyAlias))
                        {
                            star = Mathf.Max(star, progManager.GetCharacterStarLevel(legacyAlias));
                        }
                    }

                    var gmSave = ProjectZombie.Core.Save.GameManager.Instance?.SaveData;
                    if (gmSave != null)
                    {
                        star = Mathf.Max(star, gmSave.GetCharacterStarLevel(clean));
                        if (!string.IsNullOrEmpty(legacyAlias))
                        {
                            star = Mathf.Max(star, gmSave.GetCharacterStarLevel(legacyAlias));
                        }
                    }
                }
            }
            catch
            {
                // An toàn khi chạy trong môi trường unit test độc lập
            }

            return Mathf.Clamp(star <= 0 ? 1 : star, 1, 5);
        }

        /// <summary>
        /// Tính toán hệ số cộng thêm từ Cấp Sao Tướng (Máu & Tốc chạy).
        /// </summary>
        public static void EvaluateHeroStarBonuses(
            int starLevel,
            CharacterStarProgressionSO config,
            out float hpMultiplierBonus,
            out float moveSpeedMultiplierBonus)
        {
            hpMultiplierBonus = 0f;
            moveSpeedMultiplierBonus = 0f;
            if (starLevel <= 1) return;

            if (config == null)
            {
                try { config = CharacterProgressionManager.Instance?.ProgressionConfig; } catch { }
            }

            if (config != null)
            {
                var step = config.GetStepConfig(starLevel);
                if (step != null)
                {
                    hpMultiplierBonus = Mathf.Max(0f, step.healthMultiplierBonus);
                    moveSpeedMultiplierBonus = Mathf.Max(0f, step.moveSpeedMultiplierBonus);
                    return;
                }
            }

            // Fallback chuẩn theo bảng cấu hình 5 cấp sao nếu chưa gán SO
            if (starLevel == 2) hpMultiplierBonus = 0.15f;
            else if (starLevel == 3) { hpMultiplierBonus = 0.15f; moveSpeedMultiplierBonus = 0.10f; }
            else if (starLevel == 4) { hpMultiplierBonus = 0.40f; moveSpeedMultiplierBonus = 0.10f; }
            else if (starLevel >= 5) { hpMultiplierBonus = 0.70f; moveSpeedMultiplierBonus = 0.20f; }
        }

        /// <summary>
        /// Lấy cấp sao hiệu lực (1..5★) của Pháp Bảo, đồng bộ giữa ID Defense 3D và ID Meta.
        /// </summary>
        public static int GetEffectiveRelicStarLevel(string relicId, MetaProgressionSaveData saveData = null)
        {
            if (string.IsNullOrWhiteSpace(relicId)) return 1;

            int star = 0;
            string clean = relicId.Trim();
            string[] aliases = GetLegacyRelicAliases(clean);

            try
            {
                if (saveData != null)
                {
                    star = Mathf.Max(star, saveData.GetRelicStarLevel(clean));
                    for (int i = 0; i < aliases.Length; i++)
                    {
                        star = Mathf.Max(star, saveData.GetRelicStarLevel(aliases[i]));
                    }
                }
                else
                {
                    var relicMgr = RelicInventoryManager.Instance;
                    if (relicMgr != null)
                    {
                        star = Mathf.Max(star, relicMgr.GetRelicStarLevel(clean));
                        for (int i = 0; i < aliases.Length; i++)
                        {
                            star = Mathf.Max(star, relicMgr.GetRelicStarLevel(aliases[i]));
                        }
                    }

                    var gmSave = ProjectZombie.Core.Save.GameManager.Instance?.SaveData;
                    if (gmSave != null)
                    {
                        star = Mathf.Max(star, gmSave.GetRelicStarLevel(clean));
                        for (int i = 0; i < aliases.Length; i++)
                        {
                            star = Mathf.Max(star, gmSave.GetRelicStarLevel(aliases[i]));
                        }
                    }
                }
            }
            catch
            {
                // An toàn khi chạy trong môi trường unit test độc lập
            }

            return Mathf.Clamp(star <= 0 ? 1 : star, 1, 5);
        }

        /// <summary>
        /// Tính toán cộng dồn chỉ số Pháp Bảo từ bậc 2★ đến cấp sao hiện tại.
        /// </summary>
        public static void EvaluateRelicStarBonuses(
            int starLevel,
            RelicStarProgressionSO config,
            out float damageMultiplierBonus,
            out float cooldownReductionPercent,
            out float areaMultiplierBonus)
        {
            damageMultiplierBonus = 0f;
            cooldownReductionPercent = 0f;
            areaMultiplierBonus = 0f;
            if (starLevel <= 1) return;

            if (config == null)
            {
                try { config = RelicInventoryManager.Instance?.ProgressionConfig; } catch { }
            }

            if (config != null)
            {
                for (int s = 2; s <= starLevel; s++)
                {
                    var step = config.GetStepConfig(s);
                    if (step != null)
                    {
                        damageMultiplierBonus += Mathf.Max(0f, step.damageMultiplierBonus);
                        cooldownReductionPercent += Mathf.Max(0f, step.cooldownReductionPercent);
                        areaMultiplierBonus += Mathf.Max(0f, step.areaMultiplierBonus);
                    }
                }
                cooldownReductionPercent = Mathf.Clamp(cooldownReductionPercent, 0f, 0.8f);
                return;
            }

            // Fallback chuẩn theo cấu hình mặc định của RelicStarProgressionSO
            if (starLevel >= 2) damageMultiplierBonus += 0.15f;
            if (starLevel >= 3) cooldownReductionPercent += 0.15f;
            if (starLevel >= 4) areaMultiplierBonus += 0.30f;
            if (starLevel >= 5) damageMultiplierBonus += 0.30f;
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
            DefenseHeroVisualController3D visual,
            MetaProgressionSaveData saveData = null,
            CharacterStarProgressionSO progressionConfig = null)
        {
            if (resolved == null) return;

            DefenseHeroDefinitionSO heroDef = null;
            if (catalog != null)
            {
                heroDef = catalog.GetHeroById(resolved.HeroId);
            }

            if (heroDef != null)
            {
                int star = GetEffectiveHeroStarLevel(resolved.HeroId, saveData);
                EvaluateHeroStarBonuses(star, progressionConfig, out float hpBonus, out float speedBonus);

                float hpMultiplier = 1f + hpBonus;
                float speedMultiplier = 1f + speedBonus;

                if (health != null)
                {
                    health.SetMaxHealth(heroDef.BaseMaxHp * hpMultiplier);
                }

                if (motor != null)
                {
                    motor.SetMovementParameters(heroDef.BaseMoveSpeed * speedMultiplier, heroDef.TurnRate);
                }

                if (attack != null)
                {
                    attack.SetBaseCombatStats(heroDef.BaseDamage, heroDef.BaseAttackRange);
                }

                if (visual != null && heroDef.HeroKit != null)
                {
                    visual.SetKit(heroDef.HeroKit);
                }
            }
        }

        /// <summary>
        /// Áp dụng toàn bộ nâng cấp từ Cây Thiên Phú Miếu Cổ (PermanentUpgradeTreeData) vào phiên chơi Defense 3D.
        /// Trả về số lượt Đổi Thẻ (Reroll) cộng thêm đầu trận.
        /// </summary>
        public static int ApplySanctuaryUpgrades(
            DefenseRunStats sessionStats,
            DefenseHeroHealth3D health,
            DefenseHeroMotor3D motor,
            PermanentUpgradeTreeData tree = null,
            MetaProgressionSaveData saveData = null)
        {
            if (saveData == null)
            {
                try { saveData = ProjectZombie.Core.Save.GameManager.Instance?.SaveData; } catch { }
            }
            if (saveData == null || saveData.upgradeNodeLevels == null || saveData.upgradeNodeLevels.Length == 0)
            {
                return 0;
            }

            if (tree == null)
            {
                tree = Resources.Load<PermanentUpgradeTreeData>("PermanentUpgradeTree")
                    ?? Resources.Load<PermanentUpgradeTreeData>("Meta/PermanentUpgradeTree");
#if UNITY_EDITOR
                if (tree == null)
                {
                    tree = UnityEditor.AssetDatabase.LoadAssetAtPath<PermanentUpgradeTreeData>("Assets/_Data/Meta/PermanentUpgradeTree.asset");
                }
#endif
            }

            float totalDamageMult = 0f;
            float totalAttackSpeedMult = 0f;
            float totalMaxHpFlat = 0f;
            float totalMoveSpeedFlat = 0f;
            int bonusRerolls = 0;

            if (tree != null && tree.nodes != null)
            {
                for (int i = 0; i < tree.nodes.Length; i++)
                {
                    var node = tree.nodes[i];
                    if (node == null) continue;
                    int lvl = Mathf.Clamp(saveData.GetUpgradeLevel(i), 0, node.maxLevel);
                    if (lvl <= 0) continue;

                    if (string.Equals(node.nodeId, "atk_power", StringComparison.OrdinalIgnoreCase))
                    {
                        totalDamageMult += 0.05f * lvl;
                    }
                    else if (string.Equals(node.nodeId, "atk_speed", StringComparison.OrdinalIgnoreCase))
                    {
                        float perLevel = node.statBonusPerLevel.attackSpeedBonus > 0f ? node.statBonusPerLevel.attackSpeedBonus : 0.04f;
                        totalAttackSpeedMult += perLevel * lvl;
                    }
                    else if (string.Equals(node.nodeId, "def_hp", StringComparison.OrdinalIgnoreCase))
                    {
                        float perLevel = node.statBonusPerLevel.maxHealthBonus > 0f ? node.statBonusPerLevel.maxHealthBonus : 15f;
                        totalMaxHpFlat += perLevel * lvl;
                    }
                    else if (string.Equals(node.nodeId, "def_speed", StringComparison.OrdinalIgnoreCase))
                    {
                        float perLevel = node.statBonusPerLevel.moveSpeedBonus > 0f ? node.statBonusPerLevel.moveSpeedBonus : 0.3f;
                        totalMoveSpeedFlat += perLevel * lvl;
                    }
                    else if (string.Equals(node.nodeId, "util_reroll", StringComparison.OrdinalIgnoreCase))
                    {
                        bonusRerolls += lvl;
                    }
                }
            }

            if (sessionStats != null)
            {
                if (totalDamageMult > 0f)
                {
                    sessionStats.TryApplyCard(new DefenseUpgradeCard("meta_atk_power", "Miếu Cổ: Ngoại Công", string.Empty, DefenseUpgradeType.HeroAttackDamage, totalDamageMult));
                }
                if (totalAttackSpeedMult > 0f)
                {
                    sessionStats.TryApplyCard(new DefenseUpgradeCard("meta_atk_speed", "Miếu Cổ: Phong Lôi", string.Empty, DefenseUpgradeType.HeroAttackSpeed, totalAttackSpeedMult));
                }
                if (totalMaxHpFlat > 0f)
                {
                    // Tăng máu Nhà Chính tương ứng 2x lượng máu cộng thêm của Tướng
                    sessionStats.TryApplyCard(new DefenseUpgradeCard("meta_base_hp", "Miếu Cổ: Kim Cang", string.Empty, DefenseUpgradeType.BaseMaxHp, totalMaxHpFlat * 2f));
                }
            }

            if (health != null && totalMaxHpFlat > 0f)
            {
                health.SetMaxHealth(health.MaxHP + totalMaxHpFlat);
            }

            if (motor != null && totalMoveSpeedFlat > 0f)
            {
                motor.SetMovementParameters(motor.MoveSpeed + totalMoveSpeedFlat, motor.TurnRate);
            }

            return bonusRerolls;
        }

        private static string GetLegacyHeroAlias(string heroId)
        {
            string mapped = DefenseLoadoutIdMapper.MapHeroId(heroId);
            if (string.Equals(mapped, Defense3DCatalog.HERO_THU_SINH, StringComparison.OrdinalIgnoreCase)) return "C001_ThuSinh";
            if (string.Equals(mapped, Defense3DCatalog.HERO_DAO_SI, StringComparison.OrdinalIgnoreCase)) return "C002_DaoSi";
            if (string.Equals(mapped, Defense3DCatalog.HERO_THANH_DONG, StringComparison.OrdinalIgnoreCase)) return "C003_ThanhDong";
            if (string.Equals(mapped, Defense3DCatalog.HERO_AN_SI, StringComparison.OrdinalIgnoreCase)) return "C004_AnSi";
            return null;
        }

        private static string[] GetLegacyRelicAliases(string relicId)
        {
            string mapped = DefenseLoadoutIdMapper.MapRelicId(relicId);
            if (string.Equals(mapped, Defense3DCatalog.RELIC_DEP_TO_ONG, StringComparison.OrdinalIgnoreCase))
            {
                return new[] { "R001_Deptoong", "R001", "wp_kiem_truc", Defense3DCatalog.RELIC_DEP_TO_ONG };
            }
            return new[] { relicId };
        }
    }
}
