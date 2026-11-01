using ProjectZombie.Features.Defense3D.UI;
using System;
using UnityEngine;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Features.Defense3D.Loadout
{
    /// <summary>
    /// Adapter Runtime kết nối giữa dữ liệu Loadout thuần C# đã phân giải (DefenseResolvedLoadout),
    /// hệ thống tiến trình Meta (Cấp Sao Tướng, Cấp Sao Pháp Bảo / Lính / Trụ, Cây Thiên Phú Miếu Cổ)
    /// và các MonoBehaviour / ScriptableObject trong Scene Defense 3D.
    /// Mọi chỉ số thăng sao đều đọc trực tiếp từ ScriptableObject (CharacterStarProgressionSO / RelicStarProgressionSO).
    /// </summary>
    public static class DefenseLoadoutRuntimeAdapter
    {
        private static CharacterStarProgressionSO _cachedCharacterConfig;
        private static RelicStarProgressionSO _cachedRelicConfig;

        private static CharacterStarProgressionSO ResolveCharacterProgressionConfig(CharacterStarProgressionSO config = null)
        {
            if (config != null) return config;
            try
            {
                if (CharacterProgressionManager.HasInstance && CharacterProgressionManager.Instance.ProgressionConfig != null)
                {
                    return CharacterProgressionManager.Instance.ProgressionConfig;
                }
            }
            catch { }

            if (_cachedCharacterConfig == null)
            {
                _cachedCharacterConfig = Resources.Load<CharacterStarProgressionSO>("CharacterStarProgressionConfig");
#if UNITY_EDITOR
                if (_cachedCharacterConfig == null)
                {
                    _cachedCharacterConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterStarProgressionSO>("Assets/_Data/CharacterStarProgressionConfig.asset");
                }
#endif
                if (_cachedCharacterConfig == null)
                {
                    _cachedCharacterConfig = ScriptableObject.CreateInstance<CharacterStarProgressionSO>();
                }
            }
            return _cachedCharacterConfig;
        }

        private static RelicStarProgressionSO ResolveRelicProgressionConfig(RelicStarProgressionSO config = null)
        {
            if (config != null) return config;
            try
            {
                if (RelicInventoryManager.HasInstance && RelicInventoryManager.Instance.ProgressionConfig != null)
                {
                    return RelicInventoryManager.Instance.ProgressionConfig;
                }
            }
            catch { }

            if (_cachedRelicConfig == null)
            {
                _cachedRelicConfig = Resources.Load<RelicStarProgressionSO>("RelicStarProgressionConfig");
#if UNITY_EDITOR
                if (_cachedRelicConfig == null)
                {
                    _cachedRelicConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<RelicStarProgressionSO>("Assets/_Data/RelicStarProgressionConfig.asset");
                }
#endif
                if (_cachedRelicConfig == null)
                {
                    _cachedRelicConfig = ScriptableObject.CreateInstance<RelicStarProgressionSO>();
                }
            }
            return _cachedRelicConfig;
        }

        private static readonly System.Collections.Generic.Dictionary<string, Sprite> _cachedIcons =
            new System.Collections.Generic.Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Tra cứu và nạp icon đại diện cho các thực thể Loadout (Tướng, Pháp Bảo, Lính, Trụ).
        /// Tự động cache kết quả để đảm bảo 0 GC allocations trong gameplay loop.
        /// Hỗ trợ cả môi trường Editor lẫn Android APK runtime.
        /// </summary>
        public static Sprite ResolveItemIcon(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;
            string key = id.Trim();
            if (_cachedIcons.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            Sprite loaded = null;
            string path = null;

            if (string.Equals(key, Defense3DCatalog.MINION_XA_THU, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, "minion_xathu", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(key, "xathu", StringComparison.OrdinalIgnoreCase))
            {
                path = "Assets/Art/Weapons/Icon_W001_NoThan.png";
            }
            else if (string.Equals(key, Defense3DCatalog.MINION_DAN_BINH, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(key, "minion_danbinh", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(key, "danbinh", StringComparison.OrdinalIgnoreCase))
            {
                path = "Assets/Art/UI/Codex/DanBinh.png";
            }
            else if (string.Equals(key, Defense3DCatalog.MINION_PHU_TAP_DICH, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(key, "minion_phutapdich", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(key, "phutapdich", StringComparison.OrdinalIgnoreCase))
            {
                path = "Assets/Art/UI/Codex/PhuTapDich.png";
            }
            else if (string.Equals(key, Defense3DCatalog.MINION_THAY_LANG, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(key, "minion_thaylang", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(key, "thaylang", StringComparison.OrdinalIgnoreCase))
            {
                path = "Assets/Art/UI/Codex/ThayLang.png";
            }
            else if (string.Equals(key, Defense3DCatalog.RELIC_DIEU_CAY, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(key, "relic_dieucay", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(key, "dieucay", StringComparison.OrdinalIgnoreCase))
            {
                path = "Assets/Art/Weapons/Icon_W_PIPE.png";
            }
            else if (string.Equals(key, Defense3DCatalog.RELIC_DEP_TO_ONG, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(key, "relic_deptoong", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(key, "deptoong", StringComparison.OrdinalIgnoreCase))
            {
                path = "Assets/Art/Weapons/Icon_W_SLIPPER.png";
            }
            else if (string.Equals(key, Defense3DCatalog.TOWER_CUNG_TIEN, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(key, "tower_cungtien", StringComparison.OrdinalIgnoreCase))
            {
                path = "Assets/Art/UI/Codex/ThapCungTien.png";
            }
            else if (string.Equals(key, Defense3DCatalog.TOWER_BANG_HA, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(key, "tower_bangha", StringComparison.OrdinalIgnoreCase))
            {
                path = "Assets/Art/Weapons/Icon_W011_NuocThanh.png";
            }
            else if (string.Equals(key, Defense3DCatalog.TOWER_HOA_LONG, StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(key, "tower_hoalong", StringComparison.OrdinalIgnoreCase))
            {
                path = "Assets/Art/Weapons/Icon_W006_LuuDan.png";
            }

            if (path != null)
            {
#if UNITY_EDITOR
                loaded = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
#endif
            }

            if (loaded != null)
            {
                _cachedIcons[key] = loaded;
            }
            return loaded;
        }

        /// <summary>
        /// Tra cứu icon đặc thù cho vai trò lính trong Village Shop và Command Dock.
        /// </summary>
        public static Sprite ResolveMinionIcon(string minionId, int role)
        {
            if (!string.IsNullOrEmpty(minionId))
            {
                var icon = ResolveItemIcon(minionId);
                if (icon != null) return icon;
            }

            switch (role)
            {
                case DefenseLoadoutResolver.ROLE_MILITIA:
                    return ResolveItemIcon(Defense3DCatalog.MINION_DAN_BINH);
                case DefenseLoadoutResolver.ROLE_GATHERER:
                    return ResolveItemIcon(Defense3DCatalog.MINION_PHU_TAP_DICH);
                case DefenseLoadoutResolver.ROLE_HEALER:
                    return ResolveItemIcon(Defense3DCatalog.MINION_THAY_LANG);
                default:
                    return null;
            }
        }

        public static void ApplyToShop(DefenseResolvedLoadout resolved, DefenseVillageShop3D shop)
        {
            if (resolved == null || shop == null) return;

            shop.ConfigureRoster(resolved.MinionIds);
        }

        public static void ApplyTowers(DefenseResolvedLoadout resolved, DefenseTowerBuildPrototype3D towerBuilder)
        {
            if (resolved == null || towerBuilder == null) return;
            towerBuilder.SetAllowedTowers(resolved.TowerIds);
        }

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
                else if (string.Equals(resolved.RelicId, Defense3DCatalog.RELIC_DIEU_CAY, StringComparison.OrdinalIgnoreCase))
                {
                    relicDefinition = Resources.Load<DefenseSkillDefinitionSO>("Defense3D/Skill_DieuCay");
                }
            }

            if (relicDefinition != null)
            {
                heroKit.SetRelic(relicDefinition);
            }

            if (!string.IsNullOrEmpty(resolved.HeroId))
            {
                if (string.Equals(resolved.HeroId, Defense3DCatalog.HERO_DAO_SI, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(resolved.HeroId, "dao-si", StringComparison.OrdinalIgnoreCase))
                {
                    var sigDef = Resources.Load<DefenseSkillDefinitionSO>("Defense3D/Skill_DaoSi_Signature");
                    if (sigDef != null) heroKit.SetSignature(sigDef);
                }
                else if (string.Equals(resolved.HeroId, Defense3DCatalog.HERO_THU_SINH, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(resolved.HeroId, "thu-sinh", StringComparison.OrdinalIgnoreCase))
                {
                    var sigDef = Resources.Load<DefenseSkillDefinitionSO>("Defense3D/Skill_ScholarSignature");
                    if (sigDef != null) heroKit.SetSignature(sigDef);
                }
            }
        }

        public static int GetEffectiveHeroStarLevel(string heroId, MetaProgressionSaveData saveData = null)
        {
            if (string.IsNullOrWhiteSpace(heroId)) return 1;

            int star = 0;
            string clean = heroId.Trim();
            string metaId = DefenseLoadoutIdMapper.GetMetaCharacterId(clean);

            try
            {
                if (saveData != null)
                {
                    star = Mathf.Max(star, saveData.GetCharacterStarLevel(clean));
                    if (!string.IsNullOrEmpty(metaId))
                    {
                        star = Mathf.Max(star, saveData.GetCharacterStarLevel(metaId));
                    }
                }
                else
                {
                    if (CharacterProgressionManager.HasInstance)
                    {
                        var progManager = CharacterProgressionManager.Instance;
                        if (progManager != null)
                        {
                            star = Mathf.Max(star, progManager.GetCharacterStarLevel(clean));
                            if (!string.IsNullOrEmpty(metaId))
                            {
                                star = Mathf.Max(star, progManager.GetCharacterStarLevel(metaId));
                            }
                        }
                    }

                    if (ProjectZombie.Core.Save.GameManager.HasInstance)
                    {
                        var gmSave = ProjectZombie.Core.Save.GameManager.Instance?.SaveData;
                        if (gmSave != null)
                        {
                            star = Mathf.Max(star, gmSave.GetCharacterStarLevel(clean));
                            if (!string.IsNullOrEmpty(metaId))
                            {
                                star = Mathf.Max(star, gmSave.GetCharacterStarLevel(metaId));
                            }
                        }
                    }
                }
            }
            catch { }

            return Mathf.Clamp(star <= 0 ? 1 : star, 1, 5);
        }

        /// <summary>
        /// Tính toán hệ số cộng thêm từ Cấp Sao Tướng bằng cách đọc trực tiếp từ CharacterStarProgressionSO.
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

            var effectiveConfig = ResolveCharacterProgressionConfig(config);
            if (effectiveConfig == null) return;

            for (int s = 2; s <= starLevel; s++)
            {
                var step = effectiveConfig.GetStepConfig(s);
                if (step != null)
                {
                    hpMultiplierBonus += Mathf.Max(0f, step.healthMultiplierBonus);
                    moveSpeedMultiplierBonus += Mathf.Max(0f, step.moveSpeedMultiplierBonus);
                }
            }
        }

        /// <summary>Returns cumulative hero attack damage and signature cooldown bonuses.</summary>
        public static void EvaluateHeroCombatStarBonuses(int starLevel, CharacterStarProgressionSO config,
            out float damageBonus, out float signatureCooldownReduction)
        {
            damageBonus = 0f;
            signatureCooldownReduction = 0f;
            var effectiveConfig = ResolveCharacterProgressionConfig(config);
            if (effectiveConfig == null) return;
            for (int star = 2; star <= Mathf.Clamp(starLevel, 1, 5); star++)
            {
                var step = effectiveConfig.GetStepConfig(star);
                if (step == null) continue;
                damageBonus += Mathf.Max(0f, step.damageMultiplierBonus);
                signatureCooldownReduction += Mathf.Max(0f, step.cooldownReductionBonus);
            }
            signatureCooldownReduction = Mathf.Clamp(signatureCooldownReduction, 0f, 0.8f);
        }

        public static int GetEffectiveRelicStarLevel(string relicId, MetaProgressionSaveData saveData = null)
        {
            if (string.IsNullOrWhiteSpace(relicId)) return 1;

            int star = 0;
            string clean = relicId.Trim();
            string[] aliases = DefenseLoadoutIdMapper.GetMetaRelicAliases(clean);

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
                    if (RelicInventoryManager.HasInstance)
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
                    }

                    if (ProjectZombie.Core.Save.GameManager.HasInstance)
                    {
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
            }
            catch { }

            return Mathf.Clamp(star <= 0 ? 1 : star, 1, 5);
        }

        /// <summary>
        /// Tính toán cộng dồn chỉ số Pháp Bảo từ bậc 2★ đến cấp sao hiện tại từ RelicStarProgressionSO.
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

            var effectiveConfig = ResolveRelicProgressionConfig(config);
            if (effectiveConfig == null) return;

            for (int s = 2; s <= starLevel; s++)
            {
                var step = effectiveConfig.GetStepConfig(s);
                if (step != null)
                {
                    damageMultiplierBonus += Mathf.Max(0f, step.damageMultiplierBonus);
                    cooldownReductionPercent += Mathf.Max(0f, step.cooldownReductionPercent);
                    areaMultiplierBonus += Mathf.Max(0f, step.areaMultiplierBonus);
                }
            }
            cooldownReductionPercent = Mathf.Clamp(cooldownReductionPercent, 0f, 0.8f);
        }

        /// <summary>
        /// Tính toán hệ số cộng thêm từ Cấp Sao Lính Làng đọc trực tiếp từ RelicStarProgressionSO.
        /// </summary>
        public static void EvaluateMinionStarBonuses(
            int starLevel,
            out float hpMultiplierBonus,
            out float speedMultiplierBonus,
            RelicStarProgressionSO config = null)
        {
            hpMultiplierBonus = 0f;
            speedMultiplierBonus = 0f;
            if (starLevel <= 1) return;

            var effectiveConfig = ResolveRelicProgressionConfig(config);
            var step = effectiveConfig != null ? effectiveConfig.GetStepConfig(Mathf.Clamp(starLevel, 1, 5)) : null;
            if (step != null)
            {
                hpMultiplierBonus = Mathf.Max(0f, step.minionHpMultiplierBonus);
                speedMultiplierBonus = Mathf.Max(0f, step.minionSpeedMultiplierBonus);
            }
        }

        /// <summary>
        /// Tính toán hệ số cộng thêm từ Cấp Sao Trụ Phòng Thủ đọc trực tiếp từ RelicStarProgressionSO.
        /// </summary>
        public static void EvaluateTowerStarBonuses(
            int starLevel,
            out float damageMultiplierBonus,
            out float rangeMultiplierBonus,
            RelicStarProgressionSO config = null)
        {
            damageMultiplierBonus = 0f;
            rangeMultiplierBonus = 0f;
            if (starLevel <= 1) return;

            var effectiveConfig = ResolveRelicProgressionConfig(config);
            var step = effectiveConfig != null ? effectiveConfig.GetStepConfig(Mathf.Clamp(starLevel, 1, 5)) : null;
            if (step != null)
            {
                damageMultiplierBonus = Mathf.Max(0f, step.towerDamageMultiplierBonus);
                rangeMultiplierBonus = Mathf.Max(0f, step.towerRangeMultiplierBonus);
            }
        }

        public static void ApplyTowerAndMinionStarBonuses(
            DefenseResolvedLoadout resolved,
            DefenseRunStats sessionStats,
            DefenseVillageShop3D shop = null,
            MetaProgressionSaveData saveData = null)
        {
            if (resolved == null) return;

            if (shop != null)
            {
                foreach (var squad in shop.Roster.Squads)
                {
                    int minionStar = GetEffectiveRelicStarLevel(squad.Id, saveData);
                    EvaluateMinionStarBonuses(minionStar, out float hpBonus, out float spdBonus);
                    shop.SetSquadStatMultipliers(squad.Id, 1f + hpBonus, 1f + spdBonus);
                }
            }

            if (sessionStats != null && resolved.TowerIds != null && resolved.TowerIds.Count > 0)
            {
                int bestTowerStar = 1;
                for (int i = 0; i < resolved.TowerIds.Count; i++)
                {
                    int st = GetEffectiveRelicStarLevel(resolved.TowerIds[i], saveData);
                    if (st > bestTowerStar) bestTowerStar = st;
                }

                EvaluateTowerStarBonuses(bestTowerStar, out float towerDmgBonus, out float towerRngBonus);
                if (towerDmgBonus > 0f)
                {
                    sessionStats.TryApplyCard(new DefenseUpgradeCard("meta_tower_star_dmg", "Tinh Luyện Tháp: Hỏa Lực", string.Empty, DefenseUpgradeType.TowerDamage, towerDmgBonus));
                }
                if (towerRngBonus > 0f)
                {
                    sessionStats.TryApplyCard(new DefenseUpgradeCard("meta_tower_star_rng", "Tinh Luyện Tháp: Tầm Xa", string.Empty, DefenseUpgradeType.TowerAttackRange, towerRngBonus));
                }
            }
        }

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
                    EvaluateHeroCombatStarBonuses(star, progressionConfig, out float damageBonus, out _);
                    attack.SetMetaDamageBonus(damageBonus);
                }

                if (visual != null && heroDef.HeroKit != null)
                {
                    visual.SetKit(heroDef.HeroKit);
                }

                if (visual != null && heroDef.HeroPrefab != null)
                {
                    visual.MountHeroPrefab(heroDef.HeroPrefab);
                }
            }
        }
        /// <summary>
        /// Nạp động kỹ năng Tuyệt Kỹ và Pháp Bảo vào Presenter UI và Component hiệu ứng (Clean/SOLID).
        /// </summary>
        public static void ApplyDynamicSkillBindings(
            DefenseResolvedLoadout resolved,
            DefenseSkillPresenter3D[] skillPresenters,
            DefenseHeroHealth3D heroHealth,
            DefenseHeroAutoAttack3D attack,
            DefenseHeroMotor3D motor,
            DefenseHeroVisualController3D visual,
            DefenseSession session)
        {
            if (resolved == null || skillPresenters == null) return;

            DefenseSkillPresenter3D signaturePresenter = null;
            DefenseSkillPresenter3D relicPresenter = null;

            foreach (var presenter in skillPresenters)
            {
                if (presenter == null) continue;
                if (presenter.Kind == DefenseSkillKind.Signature || presenter.Kind == DefenseSkillKind.ScholarSignature)
                {
                    signaturePresenter = presenter;
                }
                else if (presenter.Kind == DefenseSkillKind.Relic || presenter.Kind == DefenseSkillKind.DepToOng)
                {
                    relicPresenter = relicPresenter ?? presenter;
                }
            }

            // 1. Pháp Bảo Điếu Cày Khói Lửa
            if (string.Equals(resolved.RelicId, Defense3DCatalog.RELIC_DIEU_CAY, StringComparison.OrdinalIgnoreCase))
            {
                var dieuCayDef = Resources.Load<DefenseSkillDefinitionSO>("Defense3D/Skill_DieuCay");
                if (dieuCayDef != null && relicPresenter != null && heroHealth != null)
                {
                    var pipeEffect = heroHealth.GetComponent<DefensePipeRelicEffect3D>();
                    if (pipeEffect == null)
                    {
                        pipeEffect = heroHealth.gameObject.AddComponent<DefensePipeRelicEffect3D>();
                    }
                    if (session != null) pipeEffect.BindGameplay(session);

                    Sprite pipeIcon = dieuCayDef.Icon;
#if UNITY_EDITOR
                    if (pipeIcon == null)
                    {
                        pipeIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Weapons/Icon_W_PIPE.png");
                    }
#endif
                    relicPresenter.Rebind(dieuCayDef, pipeEffect, pipeIcon, "ĐIẾU CÀY");
                }
            }

            // 2. Tuyệt Kỹ Tướng Đạo Sĩ
            if (string.Equals(resolved.HeroId, Defense3DCatalog.HERO_DAO_SI, StringComparison.OrdinalIgnoreCase))
            {
                var daoSiSigDef = Resources.Load<DefenseSkillDefinitionSO>("Defense3D/Skill_DaoSi_Signature");
                if (daoSiSigDef != null && signaturePresenter != null && heroHealth != null)
                {
                    var lightningEffect = heroHealth.GetComponent<DefenseLightningSignatureEffect3D>();
                    if (lightningEffect == null)
                    {
                        lightningEffect = heroHealth.gameObject.AddComponent<DefenseLightningSignatureEffect3D>();
                    }
                    lightningEffect.ConfigureComponents(heroHealth, attack, motor);
                    if (session != null) lightningEffect.BindGameplay(session);

                    Sprite sigIcon = Resources.Load<Sprite>("Art/UI/HeroSkills/Icon_Skill_DaoSi_Signature");
#if UNITY_EDITOR
                    if (sigIcon == null)
                    {
                        sigIcon = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HeroSkills/Icon_Skill_DaoSi_Signature.png");
                    }
#endif
                    signaturePresenter.Rebind(daoSiSigDef, lightningEffect, sigIcon, "THIÊN LÔI");
                    if (visual != null)
                    {
                        visual.SetSignatureEffect(lightningEffect);
                    }
                }
            }
        }

        public static int ApplySanctuaryUpgrades(
            DefenseRunStats sessionStats,
            DefenseHeroHealth3D health,
            DefenseHeroMotor3D motor,
            PermanentUpgradeTreeData tree = null,
            MetaProgressionSaveData saveData = null)
        {
            if (saveData == null && ProjectZombie.Core.Save.GameManager.HasInstance)
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
                        float perLevel = node.statBonusPerLevel.baseDamageBonus > 0f ? node.statBonusPerLevel.baseDamageBonus * 0.1f : 0.05f;
                        totalDamageMult += perLevel * lvl;
                    }
                    else if (string.Equals(node.nodeId, "atk_speed", StringComparison.OrdinalIgnoreCase))
                    {
                        totalAttackSpeedMult += node.statBonusPerLevel.attackSpeedBonus * lvl;
                    }
                    else if (string.Equals(node.nodeId, "def_hp", StringComparison.OrdinalIgnoreCase))
                    {
                        totalMaxHpFlat += node.statBonusPerLevel.maxHealthBonus * lvl;
                    }
                    else if (string.Equals(node.nodeId, "def_speed", StringComparison.OrdinalIgnoreCase))
                    {
                        totalMoveSpeedFlat += node.statBonusPerLevel.moveSpeedBonus * lvl;
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
    }
}
