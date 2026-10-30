using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.Loadout;
using ProjectZombie.Features.Defense3D.UI;

namespace ProjectZombie.Tests.Editor
{
    [TestFixture]
    public class DefenseLoadoutCatalogAndAdapterTests
    {
        [Test]
        public void Catalog_CreateDefault_ContainsBaselineEntities()
        {
            var catalog = Defense3DCatalog.CreateDefault();

            Assert.IsTrue(catalog.Contains(Defense3DCatalog.HERO_THU_SINH));
            Assert.IsTrue(catalog.Contains(Defense3DCatalog.HERO_DAO_SI));
            Assert.IsTrue(catalog.Contains(Defense3DCatalog.HERO_THANH_DONG));
            Assert.IsTrue(catalog.Contains(Defense3DCatalog.HERO_AN_SI));
            Assert.IsTrue(catalog.Contains(Defense3DCatalog.RELIC_DEP_TO_ONG));
            Assert.IsTrue(catalog.Contains(Defense3DCatalog.MINION_DAN_BINH));
            Assert.IsTrue(catalog.Contains(Defense3DCatalog.MINION_PHU_TAP_DICH));
            Assert.IsTrue(catalog.Contains(Defense3DCatalog.MINION_THAY_LANG));
            Assert.IsTrue(catalog.Contains(Defense3DCatalog.TOWER_CUNG_TIEN));
        }

        [Test]
        public void Catalog_GetEntriesByType_FiltersCorrectly()
        {
            var catalog = Defense3DCatalog.CreateDefault();

            var heroes = catalog.GetEntriesByType(LoadoutItemType.Hero);
            var relics = catalog.GetEntriesByType(LoadoutItemType.Relic);
            var minions = catalog.GetEntriesByType(LoadoutItemType.Minion);
            var towers = catalog.GetEntriesByType(LoadoutItemType.Tower);

            Assert.AreEqual(4, heroes.Count);
            Assert.AreEqual(1, relics.Count);
            Assert.AreEqual(3, minions.Count);
            Assert.AreEqual(2, towers.Count);
        }

        [Test]
        public void Ownership_CreateDefault_OwnsAllFourBaselineHeroes()
        {
            var ownership = DefenseMetaOwnershipAdapter.CreateDefault();

            Assert.IsTrue(ownership.IsOwned(Defense3DCatalog.HERO_THU_SINH));
            Assert.IsTrue(ownership.IsOwned(Defense3DCatalog.HERO_DAO_SI));
            Assert.IsTrue(ownership.IsOwned(Defense3DCatalog.HERO_THANH_DONG));
            Assert.IsTrue(ownership.IsOwned(Defense3DCatalog.HERO_AN_SI));
            Assert.IsTrue(ownership.IsOwned(Defense3DCatalog.RELIC_DEP_TO_ONG));
            Assert.IsTrue(ownership.IsOwned(Defense3DCatalog.MINION_DAN_BINH));
            Assert.IsTrue(ownership.IsOwned(Defense3DCatalog.TOWER_CUNG_TIEN));
        }

        [Test]
        public void IdMapper_MapsAllFourHeroesCorrectly()
        {
            Assert.AreEqual(Defense3DCatalog.HERO_THU_SINH, DefenseLoadoutIdMapper.MapHeroId("C001_ThuSinh"));
            Assert.AreEqual(Defense3DCatalog.HERO_DAO_SI, DefenseLoadoutIdMapper.MapHeroId("C002_DaoSi"));
            Assert.AreEqual(Defense3DCatalog.HERO_THANH_DONG, DefenseLoadoutIdMapper.MapHeroId("C003_ThanhDong"));
            Assert.AreEqual(Defense3DCatalog.HERO_AN_SI, DefenseLoadoutIdMapper.MapHeroId("C004_AnSi"));
        }

        [Test]
        public void Resolver_TryGetMinionRole_MapsIdsCorrectly()
        {
            Assert.IsTrue(DefenseLoadoutResolver.TryGetMinionRole(Defense3DCatalog.MINION_DAN_BINH, out int roleDanBinh));
            Assert.AreEqual((int)DefenseMinionRole.Militia, roleDanBinh);

            Assert.IsTrue(DefenseLoadoutResolver.TryGetMinionRole(Defense3DCatalog.MINION_PHU_TAP_DICH, out int rolePhuTapDich));
            Assert.AreEqual((int)DefenseMinionRole.Gatherer, rolePhuTapDich);

            Assert.IsTrue(DefenseLoadoutResolver.TryGetMinionRole(Defense3DCatalog.MINION_THAY_LANG, out int roleThayLang));
            Assert.AreEqual((int)DefenseMinionRole.Healer, roleThayLang);

            Assert.IsFalse(DefenseLoadoutResolver.TryGetMinionRole("unknown_minion", out _));
        }

        [Test]
        public void Resolver_Resolve_NullSnapshot_ReturnsSafeDefault()
        {
            var resolved = DefenseLoadoutResolver.Resolve(null);

            Assert.IsNotNull(resolved);
            Assert.IsNotNull(resolved.Snapshot);
            Assert.AreEqual(Defense3DCatalog.HERO_THU_SINH, resolved.HeroId);
            Assert.AreEqual(Defense3DCatalog.RELIC_DEP_TO_ONG, resolved.RelicId);
            Assert.AreEqual(3, resolved.AllowedMinionRoles.Count);
        }

        [Test]
        public void Resolver_Resolve_InvalidSnapshot_FallbacksToDefault()
        {
            // Snapshot thiếu Tướng và Lính
            var invalidSnapshot = new DefenseLoadoutSnapshot(
                heroId: string.Empty,
                relicId: Defense3DCatalog.RELIC_DEP_TO_ONG,
                minionIds: new string[0],
                towerIds: new string[0]
            );

            var resolved = DefenseLoadoutResolver.Resolve(invalidSnapshot);

            Assert.IsNotNull(resolved);
            Assert.AreEqual(Defense3DCatalog.HERO_THU_SINH, resolved.HeroId);
            Assert.AreEqual(Defense3DCatalog.RELIC_DEP_TO_ONG, resolved.RelicId);
            Assert.AreEqual(3, resolved.AllowedMinionRoles.Count);
        }

        [Test]
        public void Resolver_Resolve_SpecificMinions_MapsOnlySelectedRoles()
        {
            // Chỉ chọn Dân Binh (0) và Thầy Lang (2), không chọn Phu Tạp Dịch (1)
            var snapshot = new DefenseLoadoutSnapshot(
                heroId: Defense3DCatalog.HERO_THU_SINH,
                relicId: Defense3DCatalog.RELIC_DEP_TO_ONG,
                minionIds: new[] { Defense3DCatalog.MINION_DAN_BINH, Defense3DCatalog.MINION_THAY_LANG },
                towerIds: new[] { Defense3DCatalog.TOWER_CUNG_TIEN }
            );

            var resolved = DefenseLoadoutResolver.Resolve(snapshot);

            Assert.AreEqual(2, resolved.AllowedMinionRoles.Count);
            CollectionAssert.Contains(resolved.AllowedMinionRoles, (int)DefenseMinionRole.Militia);
            CollectionAssert.Contains(resolved.AllowedMinionRoles, (int)DefenseMinionRole.Healer);
            CollectionAssert.DoesNotContain(resolved.AllowedMinionRoles, (int)DefenseMinionRole.Gatherer);
        }

        [Test]
        public void HeroKit_SetRelic_UpdatesDefinition()
        {
            var heroKit = ScriptableObject.CreateInstance<DefenseHeroKitSO>();
            Assert.IsNull(heroKit.Relic);

            var relicDef = ScriptableObject.CreateInstance<DefenseSkillDefinitionSO>();
            heroKit.SetRelic(relicDef);

            Assert.AreSame(relicDef, heroKit.Relic);
            Assert.AreSame(relicDef, heroKit.DefinitionFor(DefenseSkillKind.DepToOng));
        }

        [Test]
        public void VillageShop_AllowedRoles_FiltersRecruitmentCorrectly()
        {
            var shopGameObject = new GameObject("TestVillageShop");
            var shop = shopGameObject.AddComponent<DefenseVillageShop3D>();

            try
            {
                // Khi chưa cấu hình allowed roles: mặc định cho phép mọi role
                Assert.IsTrue(shop.IsRoleAllowed(0));
                Assert.IsTrue(shop.IsRoleAllowed(1));
                Assert.IsTrue(shop.IsRoleAllowed(2));

                // Cấu hình chỉ cho phép role 0 và 1
                shop.SetAllowedRoles(new[] { 0, 1 });
                Assert.IsTrue(shop.IsRoleAllowed(0));
                Assert.IsTrue(shop.IsRoleAllowed(1));
                Assert.IsFalse(shop.IsRoleAllowed(2));

                // Khôi phục null -> mọi role lại được phép
                shop.SetAllowedRoles(null);
                Assert.IsTrue(shop.IsRoleAllowed(2));
            }
            finally
            {
                Object.DestroyImmediate(shopGameObject);
            }
        }

        [Test]
        public void TowerBuilder_SetAllowedTowers_SetsIdsCorrectly()
        {
            var go = new GameObject("TestTowerBuilder");
            var builder = go.AddComponent<DefenseTowerBuildPrototype3D>();

            try
            {
                var resolved = DefenseLoadoutResolver.Resolve(null);
                DefenseLoadoutRuntimeAdapter.ApplyTowers(resolved, builder);

                Assert.AreEqual(1, builder.AllowedTowerIds.Count);
                Assert.AreEqual(Defense3DCatalog.TOWER_CUNG_TIEN, builder.AllowedTowerIds[0]);
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void ApplyToHeroKit_AutoLoadsRelicWhenNull()
        {
            var heroKit = ScriptableObject.CreateInstance<DefenseHeroKitSO>();
            var resolved = DefenseLoadoutResolver.Resolve(null);

            DefenseLoadoutRuntimeAdapter.ApplyToHeroKit(resolved, heroKit, null);

            Assert.IsNotNull(heroKit.Relic);
            Assert.AreEqual(DefenseSkillKind.DepToOng, heroKit.Relic.Capture().Kind);
        }

        [Test]
        public void DefensePreparationView_ComponentInstantiatesWithoutException()
        {
            var viewGo = new GameObject("TestDefensePreparationView");
            var view = viewGo.AddComponent<DefensePreparationView>();

            try
            {
                Assert.IsNotNull(view);
                Assert.AreEqual(ProjectZombie.Features.UI.MetaScreenType.DefensePreparation, view.ScreenType);
            }
            finally
            {
                Object.DestroyImmediate(viewGo);
            }
        }

        [Test]
        public void UIScreenFactory_HasScreen_ReturnsFalseWhenNotConfigured()
        {
            var registry = ScriptableObject.CreateInstance<ProjectZombie.Features.UI.UIRegistrySO>();
            try
            {
                var factory = new ProjectZombie.Features.UI.UIScreenFactory(registry, null);
                Assert.IsFalse(factory.HasScreen(ProjectZombie.Features.UI.MetaScreenType.DefensePreparation));
            }
            finally { Object.DestroyImmediate(registry); }
        }

        [Test]
        public void DefensePreparationUIGenerator_GeneratesValidPrefabAndRegistersInRegistry()
        {
            ProjectZombie.Editor.UI.DefensePreparationUIGenerator.GenerateDefensePreparationUI();
            ProjectZombie.Editor.UI.StageSelectUIGenerator.GenerateStageSelectUI();

            var prefab = Resources.Load<GameObject>("UI/DefensePreparationUI");
            Assert.IsNotNull(prefab, "Prefab UI/DefensePreparationUI phải tồn tại trong Assets/Resources/UI/");

            var view = prefab.GetComponent<DefensePreparationView>();
            Assert.IsNotNull(view, "Prefab phải gắn script DefensePreparationView");
            Assert.AreEqual(ProjectZombie.Features.UI.MetaScreenType.DefensePreparation, view.ScreenType);

            var registry = Resources.Load<ProjectZombie.Features.UI.UIRegistrySO>("UI/UIRegistry");
            Assert.IsNotNull(registry, "UIRegistry.asset phải tồn tại trong Resources/UI");
            bool found = registry.TryGetScreenEntry(ProjectZombie.Features.UI.MetaScreenType.DefensePreparation, out var screenItem);
            Assert.IsTrue(found, "DefensePreparation phải được đăng ký trong UIRegistry");
            Assert.IsNotNull(screenItem.screenPrefab, "DefensePreparation screenPrefab không được null trong UIRegistry");
        }

        [Test]
        public void MetaProgression_RelicStarAndSanctuaryTree_ApplyToRuntimeStats()
        {
            DefenseLoadoutRuntimeAdapter.EvaluateRelicStarBonuses(5, null, out float dmgBonus, out float cdRed, out float areaBonus);
            Assert.AreEqual(0.45f, dmgBonus, 0.001f);
            Assert.AreEqual(0.15f, cdRed, 0.001f);
            Assert.AreEqual(0.30f, areaBonus, 0.001f);

            var baseSkill = new DefenseSkillDefinition(DefenseSkillKind.DepToOng, 10f, 3f, 100f, 10f, 0.5f, false, DefenseTargetMask.All);
            var boostedSkill = baseSkill.WithBonuses(dmgBonus, cdRed, areaBonus);
            Assert.AreEqual(145f, boostedSkill.Damage, 0.01f);
            Assert.AreEqual(8.5f, boostedSkill.Cooldown, 0.01f);
            Assert.AreEqual(3.9f, boostedSkill.Radius, 0.01f);

            var saveData = new ProjectZombie.Features.MetaProgression.MetaProgressionSaveData();
            saveData.SetCharacterProgress("C001_ThuSinh", 0, 3);
            Assert.AreEqual(3, DefenseLoadoutRuntimeAdapter.GetEffectiveHeroStarLevel(Defense3DCatalog.HERO_THU_SINH, saveData));

            var tree = ScriptableObject.CreateInstance<ProjectZombie.Features.MetaProgression.PermanentUpgradeTreeData>();
            tree.nodes = new[]
            {
                new ProjectZombie.Features.MetaProgression.PermanentUpgradeNode { nodeId = "atk_power", maxLevel = 10 },
                new ProjectZombie.Features.MetaProgression.PermanentUpgradeNode { nodeId = "atk_speed", maxLevel = 5, statBonusPerLevel = new ProjectZombie.Features.Player.PlayerStatModifier { attackSpeedBonus = 0.04f } },
                new ProjectZombie.Features.MetaProgression.PermanentUpgradeNode { nodeId = "def_hp", maxLevel = 10, statBonusPerLevel = new ProjectZombie.Features.Player.PlayerStatModifier { maxHealthBonus = 15f } },
                new ProjectZombie.Features.MetaProgression.PermanentUpgradeNode { nodeId = "util_reroll", maxLevel = 3 }
            };
            saveData.SetUpgradeLevel(0, 4); // +20% damage
            saveData.SetUpgradeLevel(1, 2); // +8% attack speed
            saveData.SetUpgradeLevel(2, 3); // +45 Hero HP, +90 Base HP
            saveData.SetUpgradeLevel(3, 2); // +2 Rerolls

            var stats = new DefenseRunStats();
            var heroGo = new GameObject("TestHero");
            var health = heroGo.AddComponent<DefenseHeroHealth3D>();
            try
            {
                health.SetMaxHealth(200f);
                int rerolls = DefenseLoadoutRuntimeAdapter.ApplySanctuaryUpgrades(stats, health, null, tree, saveData);
                Assert.AreEqual(2, rerolls);
                Assert.AreEqual(245f, health.MaxHP, 0.01f);
                Assert.AreEqual(0.20f, stats.GetModifier(DefenseUpgradeType.HeroAttackDamage), 0.001f);
                Assert.AreEqual(0.08f, stats.GetModifier(DefenseUpgradeType.HeroAttackSpeed), 0.001f);
                Assert.AreEqual(90f, stats.GetModifier(DefenseUpgradeType.BaseMaxHp), 0.01f);
            }
            finally
            {
                Object.DestroyImmediate(heroGo);
                Object.DestroyImmediate(tree);
            }
        }
    }
}
