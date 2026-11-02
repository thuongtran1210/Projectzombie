using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.Loadout;

public sealed class DefenseHeroCatalogTests
{
        private DefenseHeroCatalogSO _catalog;
        private DefenseHeroDefinitionSO _heroThuSinh;
        private DefenseHeroDefinitionSO _heroDaoSi;

        [SetUp]
        public void SetUp()
        {
            _catalog = ScriptableObject.CreateInstance<DefenseHeroCatalogSO>();

            _heroThuSinh = ScriptableObject.CreateInstance<DefenseHeroDefinitionSO>();
            SetHeroField(_heroThuSinh, "_heroId", DefenseHeroIds.THU_SINH);
            SetHeroField(_heroThuSinh, "_displayName", "Thư Sinh");

            _heroDaoSi = ScriptableObject.CreateInstance<DefenseHeroDefinitionSO>();
            SetHeroField(_heroDaoSi, "_heroId", DefenseHeroIds.DAO_SI);
            SetHeroField(_heroDaoSi, "_displayName", "Đạo Sĩ");

            _catalog.SetHeroes(new List<DefenseHeroDefinitionSO> { _heroThuSinh, _heroDaoSi }, _heroThuSinh);
        }

        [TearDown]
        public void TearDown()
        {
            if (_catalog != null) Object.DestroyImmediate(_catalog);
            if (_heroThuSinh != null) Object.DestroyImmediate(_heroThuSinh);
            if (_heroDaoSi != null) Object.DestroyImmediate(_heroDaoSi);
        }

        private static void SetHeroField(DefenseHeroDefinitionSO hero, string fieldName, object value)
        {
            var field = typeof(DefenseHeroDefinitionSO).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(hero, value);
        }

        [Test]
        public void GetHeroById_ExactCanonicalId_ReturnsTargetHero()
        {
            var result = _catalog.GetHeroById(DefenseHeroIds.THU_SINH);
            Assert.AreSame(_heroThuSinh, result);

            var resultDaoSi = _catalog.GetHeroById(DefenseHeroIds.DAO_SI);
            Assert.AreSame(_heroDaoSi, resultDaoSi);
        }

        [Test]
        public void GetHeroById_LegacyAliases_ResolvesToCanonicalHero()
        {
            // Legacy alias HERO_THUSHINH
            var resultLegacy = _catalog.GetHeroById("HERO_THUSHINH");
            Assert.AreSame(_heroThuSinh, resultLegacy);

            // Legacy alias C001
            var resultC001 = _catalog.GetHeroById("C001");
            Assert.AreSame(_heroThuSinh, resultC001);

            // Legacy alias HERO_DAOSI
            var resultLegacyDaoSi = _catalog.GetHeroById("HERO_DAOSI");
            Assert.AreSame(_heroDaoSi, resultLegacyDaoSi);
        }

        [Test]
        public void GetHeroById_UnknownOrEmptyId_FallsBackToDefaultHero()
        {
            var nullResult = _catalog.GetHeroById(null);
            Assert.AreSame(_heroThuSinh, nullResult);

            var emptyResult = _catalog.GetHeroById("   ");
            Assert.AreSame(_heroThuSinh, emptyResult);

            var unknownResult = _catalog.GetHeroById("non_existing_hero_xyz");
            Assert.AreSame(_heroThuSinh, unknownResult);
        }

        [Test]
        public void DefenseHeroIds_IsCanonicalHeroId_ValidatesCorrectly()
        {
            Assert.IsTrue(DefenseHeroIds.IsCanonicalHeroId(DefenseHeroIds.THU_SINH));
            Assert.IsTrue(DefenseHeroIds.IsCanonicalHeroId(DefenseHeroIds.DAO_SI));
            Assert.IsTrue(DefenseHeroIds.IsCanonicalHeroId(DefenseHeroIds.THANH_DONG));
            Assert.IsTrue(DefenseHeroIds.IsCanonicalHeroId(DefenseHeroIds.AN_SI));

            Assert.IsFalse(DefenseHeroIds.IsCanonicalHeroId("HERO_THUSHINH"));
            Assert.IsFalse(DefenseHeroIds.IsCanonicalHeroId("unknown"));
            Assert.IsFalse(DefenseHeroIds.IsCanonicalHeroId(null));
        }

        [Test]
        public void Definition_ApplyStatsToHero_SyncsHealthAndMotorOnly()
        {
            var def = ScriptableObject.CreateInstance<DefenseHeroDefinitionSO>();
            var go = new GameObject("TestHeroStats");
            var health = go.AddComponent<DefenseHeroHealth3D>();
            var motor = go.AddComponent<DefenseHeroMotor3D>();

            def.ApplyStatsToHero(health, motor);

            Assert.AreEqual(def.BaseMaxHp, health.MaxHp);

            Object.DestroyImmediate(go);
            Object.DestroyImmediate(def);
        }
    }
}

