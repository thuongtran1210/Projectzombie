using System.IO;
using NUnit.Framework;
using UnityEngine;
using ProjectZombie.Features.MetaProgression;
using ProjectZombie.Features.Defense3D.Loadout;

namespace ProjectZombie.Tests.Editor
{
    [TestFixture]
    public class DefenseLoadoutSaveAndMetaAdapterTests
    {
        private string _testDir;

        [SetUp]
        public void SetUp()
        {
            _testDir = Path.Combine(Application.temporaryCachePath, "DefenseLoadoutTest_" + System.Guid.NewGuid().ToString("N"));
            if (!Directory.Exists(_testDir))
            {
                Directory.CreateDirectory(_testDir);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testDir))
            {
                try { Directory.Delete(_testDir, true); } catch { }
            }
        }

        [Test]
        public void IdMapper_LegacyHeroAliases_MapToThuSinh()
        {
            Assert.AreEqual(Defense3DCatalog.HERO_THU_SINH, DefenseLoadoutIdMapper.MapHeroId("C001_ThuSinh"));
            Assert.AreEqual(Defense3DCatalog.HERO_THU_SINH, DefenseLoadoutIdMapper.MapHeroId("C001"));
            Assert.AreEqual(Defense3DCatalog.HERO_THU_SINH, DefenseLoadoutIdMapper.MapHeroId("thusinh"));
            Assert.AreEqual(Defense3DCatalog.HERO_THU_SINH, DefenseLoadoutIdMapper.MapHeroId("ThuSinh"));
            Assert.AreEqual(Defense3DCatalog.HERO_THU_SINH, DefenseLoadoutIdMapper.MapHeroId("hero_thusinh"));
        }

        [Test]
        public void IdMapper_UnknownHero_FallbacksToThuSinh()
        {
            Assert.AreEqual(Defense3DCatalog.HERO_THU_SINH, DefenseLoadoutIdMapper.MapHeroId(null));
            Assert.AreEqual(Defense3DCatalog.HERO_THU_SINH, DefenseLoadoutIdMapper.MapHeroId(""));
            Assert.AreEqual(Defense3DCatalog.HERO_THU_SINH, DefenseLoadoutIdMapper.MapHeroId("unknown_hero_xyz"));
        }

        [Test]
        public void IdMapper_LegacyRelicAliases_MapToDepToOng()
        {
            Assert.AreEqual(Defense3DCatalog.RELIC_DEP_TO_ONG, DefenseLoadoutIdMapper.MapRelicId("R001_Deptoong"));
            Assert.AreEqual(Defense3DCatalog.RELIC_DEP_TO_ONG, DefenseLoadoutIdMapper.MapRelicId("R001"));
            Assert.AreEqual(Defense3DCatalog.RELIC_DEP_TO_ONG, DefenseLoadoutIdMapper.MapRelicId("deptoong"));
            Assert.AreEqual(Defense3DCatalog.RELIC_DEP_TO_ONG, DefenseLoadoutIdMapper.MapRelicId("relic_deptoong"));
        }

        [Test]
        public void IdMapper_EmptyMinionsAndTowers_FillsDefaults()
        {
            var minions = DefenseLoadoutIdMapper.MapMinionIds(new string[0]);
            Assert.AreEqual(3, minions.Count);
            CollectionAssert.Contains(minions, Defense3DCatalog.MINION_DAN_BINH);
            CollectionAssert.Contains(minions, Defense3DCatalog.MINION_PHU_TAP_DICH);
            CollectionAssert.Contains(minions, Defense3DCatalog.MINION_THAY_LANG);

            var towers = DefenseLoadoutIdMapper.MapTowerIds(null);
            Assert.AreEqual(1, towers.Count);
            Assert.AreEqual(Defense3DCatalog.TOWER_CUNG_TIEN, towers[0]);
        }

        [Test]
        public void OwnershipAdapter_BaselineEntitiesAlwaysOwned()
        {
            var adapter = DefenseMetaOwnershipAdapter.CreateDefault();

            Assert.IsTrue(adapter.IsOwned(Defense3DCatalog.HERO_THU_SINH));
            Assert.IsTrue(adapter.IsOwned(Defense3DCatalog.RELIC_DEP_TO_ONG));
            Assert.IsTrue(adapter.IsOwned(Defense3DCatalog.MINION_DAN_BINH));
            Assert.IsTrue(adapter.IsOwned(Defense3DCatalog.MINION_PHU_TAP_DICH));
            Assert.IsTrue(adapter.IsOwned(Defense3DCatalog.MINION_THAY_LANG));
            Assert.IsTrue(adapter.IsOwned(Defense3DCatalog.TOWER_CUNG_TIEN));
            Assert.IsFalse(adapter.IsOwned("unregistered_secret_item"));
        }

        [Test]
        public void OwnershipAdapter_ReadsLegacySaveData()
        {
            var saveData = new MetaProgressionSaveData();
            saveData.unlockedCharacters = new[] { "C001_ThuSinh" };
            saveData.relicProgressList.Add(new RelicProgressEntry
            {
                relicId = "R001_Deptoong",
                shardCount = 10,
                starLevel = 1
            });

            var adapter = new DefenseMetaOwnershipAdapter(saveData);

            Assert.IsTrue(adapter.IsOwned(Defense3DCatalog.HERO_THU_SINH));
            Assert.IsTrue(adapter.IsOwned(Defense3DCatalog.RELIC_DEP_TO_ONG));
        }

        [Test]
        public void Repository_FreshInstall_CreatesDefaultDto()
        {
            var repo = new DefenseLoadoutRepository(_testDir, "fresh_loadout.json");
            var dto = repo.Load();

            Assert.IsNotNull(dto);
            Assert.AreEqual(DefenseLoadoutRepository.CURRENT_VERSION, dto.Version);
            Assert.AreEqual(Defense3DCatalog.HERO_THU_SINH, dto.HeroId);
            Assert.AreEqual(Defense3DCatalog.RELIC_DEP_TO_ONG, dto.RelicId);
            Assert.AreEqual(3, dto.MinionIds.Count);
            Assert.AreEqual(1, dto.TowerIds.Count);
            Assert.IsTrue(File.Exists(repo.SaveFilePath));
        }

        [Test]
        public void Repository_RoundTrip_PreservesData()
        {
            var repo = new DefenseLoadoutRepository(_testDir, "roundtrip.json");
            var original = new LoadoutSaveDto(
                Defense3DCatalog.HERO_THU_SINH,
                Defense3DCatalog.RELIC_DEP_TO_ONG,
                new[] { Defense3DCatalog.MINION_DAN_BINH, Defense3DCatalog.MINION_THAY_LANG },
                new[] { Defense3DCatalog.TOWER_CUNG_TIEN },
                DefenseLoadoutRepository.CURRENT_VERSION);

            repo.Save(original);
            var loaded = repo.Load();

            Assert.IsNotNull(loaded);
            Assert.AreEqual(original.HeroId, loaded.HeroId);
            Assert.AreEqual(original.RelicId, loaded.RelicId);
            Assert.AreEqual(2, loaded.MinionIds.Count);
            Assert.AreEqual(1, loaded.TowerIds.Count);
        }

        [Test]
        public void Repository_FutureVersion_IsNotOverwritten()
        {
            var repo = new DefenseLoadoutRepository(_testDir, "future_version.json");
            string futureJson = "{\"Version\": 99, \"HeroId\": \"future_hero\", \"RelicId\": \"future_relic\"}";
            File.WriteAllText(repo.SaveFilePath, futureJson);

            // Cố tình ghi đè bằng save v1
            var v1Dto = DefenseLoadoutRepository.CreateDefaultDto();
            repo.Save(v1Dto);

            // Kiểm tra tệp vẫn giữ nguyên nội dung phiên bản 99
            string currentJson = File.ReadAllText(repo.SaveFilePath);
            StringAssert.Contains("99", currentJson);
            StringAssert.Contains("future_hero", currentJson);
        }

        [Test]
        public void Repository_CorruptJson_RecoversSafely()
        {
            var repo = new DefenseLoadoutRepository(_testDir, "corrupt.json");
            File.WriteAllText(repo.SaveFilePath, "{ invalid json corrupt content !!! }");

            var dto = repo.Load();

            Assert.IsNotNull(dto);
            Assert.AreEqual(Defense3DCatalog.HERO_THU_SINH, dto.HeroId);
            Assert.AreEqual(3, dto.MinionIds.Count);
        }
    }
}
