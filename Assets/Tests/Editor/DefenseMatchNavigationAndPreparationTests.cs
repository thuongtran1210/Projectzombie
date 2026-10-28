using System.Collections.Generic;
using NUnit.Framework;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.Loadout;
using ProjectZombie.Features.Defense3D.UI;

namespace ProjectZombie.Tests.Editor
{
    [TestFixture]
    public class DefenseMatchNavigationAndPreparationTests
    {
        [TearDown]
        public void TearDown()
        {
            DefenseMatchNavigation.ResetStateForTesting();
        }

        [Test]
        public void Navigation_TargetScene_IsExplicitStringNameNotIndex()
        {
            // Yêu cầu G4: Tuyệt đối không điều hướng bằng build index
            Assert.AreEqual("MapSongQue3D_Prototype", DefenseMatchNavigation.TARGET_SCENE_NAME);
            Assert.IsFalse(int.TryParse(DefenseMatchNavigation.TARGET_SCENE_NAME, out _));
        }

        [Test]
        public void SessionContext_ConsumeSnapshot_ReturnsDefaultWhenEmpty()
        {
            DefenseSessionContext.Clear();
            var snapshot = DefenseSessionContext.ConsumeSnapshot();

            Assert.IsNotNull(snapshot);
            Assert.AreEqual(Defense3DCatalog.HERO_THU_SINH, snapshot.HeroId);
            Assert.AreEqual(Defense3DCatalog.RELIC_DEP_TO_ONG, snapshot.RelicId);
            Assert.AreEqual(3, snapshot.MinionIds.Count);
        }

        [Test]
        public void SessionContext_ConsumeSnapshot_ClearsAfterRead()
        {
            var custom = new DefenseLoadoutSnapshot(
                Defense3DCatalog.HERO_THU_SINH,
                Defense3DCatalog.RELIC_DEP_TO_ONG,
                new[] { Defense3DCatalog.MINION_DAN_BINH },
                new[] { Defense3DCatalog.TOWER_CUNG_TIEN });

            DefenseSessionContext.PendingSnapshot = custom;
            var consumed = DefenseSessionContext.ConsumeSnapshot();

            Assert.AreSame(custom, consumed);
            Assert.IsNull(DefenseSessionContext.PendingSnapshot);
        }

        [Test]
        public void Presenter_InitialState_CanStartBattleWithDefaults()
        {
            var catalog = Defense3DCatalog.CreateDefault();
            var ownership = DefenseMetaOwnershipAdapter.CreateDefault();
            var repo = new FakeRepository(DefenseLoadoutRepository.CreateDefaultDto());

            using (var presenter = new DefensePreparationPresenter(catalog, ownership, repo))
            {
                Assert.IsTrue(presenter.CanStartBattle);
                Assert.AreEqual(Defense3DCatalog.HERO_THU_SINH, presenter.Selection.HeroId);
                Assert.AreEqual(Defense3DCatalog.RELIC_DEP_TO_ONG, presenter.Selection.RelicId);
                Assert.AreEqual(3, presenter.Selection.MinionIds.Count);
            }
        }

        [Test]
        public void Presenter_RemovingAllMinions_DisablesStartBattle()
        {
            var catalog = Defense3DCatalog.CreateDefault();
            var ownership = DefenseMetaOwnershipAdapter.CreateDefault();
            var repo = new FakeRepository(DefenseLoadoutRepository.CreateDefaultDto());

            using (var presenter = new DefensePreparationPresenter(catalog, ownership, repo))
            {
                Assert.IsTrue(presenter.CanStartBattle);

                // Gỡ toàn bộ lính
                presenter.ToggleMinion(Defense3DCatalog.MINION_DAN_BINH);
                presenter.ToggleMinion(Defense3DCatalog.MINION_PHU_TAP_DICH);
                presenter.ToggleMinion(Defense3DCatalog.MINION_THAY_LANG);

                // Không đủ số lượng lính tối thiểu (1)
                Assert.IsFalse(presenter.CanStartBattle);
                StringAssert.Contains("Số loại lính", presenter.LastValidationMessage);

                // Thêm lại 1 lính
                presenter.ToggleMinion(Defense3DCatalog.MINION_DAN_BINH);
                Assert.IsTrue(presenter.CanStartBattle);
            }
        }

        [Test]
        public void Presenter_ValidationEvent_FiresOnSelectionChange()
        {
            var catalog = Defense3DCatalog.CreateDefault();
            var ownership = DefenseMetaOwnershipAdapter.CreateDefault();
            var repo = new FakeRepository(DefenseLoadoutRepository.CreateDefaultDto());

            using (var presenter = new DefensePreparationPresenter(catalog, ownership, repo))
            {
                int eventCount = 0;
                bool lastState = false;
                presenter.OnValidationStateChanged += (canStart, msg) =>
                {
                    eventCount++;
                    lastState = canStart;
                };

                // Bỏ relic (bắt buộc theo default rules)
                presenter.SelectRelic("");

                Assert.Greater(eventCount, 0);
                Assert.IsFalse(lastState);
                Assert.IsFalse(presenter.CanStartBattle);
            }
        }

        [Test]
        public void Presenter_Dispose_UnsubscribesCleanly()
        {
            var catalog = Defense3DCatalog.CreateDefault();
            var ownership = DefenseMetaOwnershipAdapter.CreateDefault();
            var repo = new FakeRepository(DefenseLoadoutRepository.CreateDefaultDto());

            var presenter = new DefensePreparationPresenter(catalog, ownership, repo);
            int eventCount = 0;
            presenter.OnValidationStateChanged += (canStart, msg) => eventCount++;

            presenter.Dispose();

            // Thay đổi sau dispose không kích hoạt event
            presenter.Selection.SetHero("hero_unknown");
            Assert.AreEqual(0, eventCount);
        }

        private sealed class FakeRepository : ILoadoutRepository
        {
            private LoadoutSaveDto _saved;
            public FakeRepository(LoadoutSaveDto initial) => _saved = initial;
            public LoadoutSaveDto Load() => _saved;
            public void Save(LoadoutSaveDto dto) => _saved = dto;
        }
    }
}
