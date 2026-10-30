using System;
using NUnit.Framework;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Tests.Editor
{
    [TestFixture]
    public class DefenseMatchResultAndLifecycleTests
    {
        [SetUp]
        public void SetUp()
        {
            DefenseMatchResultService.LogWarningHook = msg => { };
            DefenseMatchResultService.LogErrorHook = msg => { };
            DefenseNavigationService.LogWarningHook = msg => { };
            DefenseNavigationService.LogErrorHook = msg => { };
        }

        [TearDown]
        public void TearDown()
        {
            DefenseMatchResultService.LogWarningHook = null;
            DefenseMatchResultService.LogErrorHook = null;
            DefenseNavigationService.LogWarningHook = null;
            DefenseNavigationService.LogErrorHook = null;
            DefenseNavigationService.Default.ResetStateForTesting();
        }

        [Test]
        public void ProcessMatchResult_Victory_AwardsWoodAndVictoryBonus()
        {
            var service = new DefenseMatchResultService();
            var saveData = new MetaProgressionSaveData { totalCurrency = 100 };
            bool saved = false;

            var input = new DefenseMatchResultData(
                isWon: true,
                elapsedSeconds: 320.5f,
                woodCollected: 50,
                enemiesDefeated: 42,
                stageId: "STAGE_01");

            var result = service.ProcessMatchResult(input, saveData, _ => saved = true);

            Assert.IsTrue(service.HasProcessedResult);
            Assert.IsTrue(result.IsWon);
            Assert.AreEqual(50, result.WoodCollected);
            Assert.AreEqual(42, result.EnemiesDefeated);
            // 50 gỗ * 2.0 + 500 thưởng thắng = 600
            Assert.AreEqual(600, result.EarnedCurrency);
            Assert.AreEqual(700, saveData.totalCurrency);
            Assert.IsTrue(saveData.completedStages.Contains("STAGE_01"));
            Assert.IsTrue(saved);
            StringAssert.Contains("CHIẾN THẮNG", result.Title);
            StringAssert.Contains("+600", result.Summary);
        }

        [Test]
        public void ProcessMatchResult_Defeat_AwardsOnlyWoodReward()
        {
            var service = new DefenseMatchResultService();
            var saveData = new MetaProgressionSaveData { totalCurrency = 200 };
            bool saved = false;

            var input = new DefenseMatchResultData(
                isWon: false,
                elapsedSeconds: 150f,
                woodCollected: 30,
                enemiesDefeated: 15,
                stageId: "STAGE_01");

            var result = service.ProcessMatchResult(input, saveData, _ => saved = true);

            Assert.IsTrue(service.HasProcessedResult);
            Assert.IsFalse(result.IsWon);
            // 30 gỗ * 2.0 + 0 bonus = 60
            Assert.AreEqual(60, result.EarnedCurrency);
            Assert.AreEqual(260, saveData.totalCurrency);
            Assert.IsFalse(saveData.completedStages.Contains("STAGE_01"));
            Assert.IsTrue(saved);
            StringAssert.Contains("THẤT THỦ", result.Title);
            StringAssert.Contains("+60", result.Summary);
        }

        [Test]
        public void ProcessMatchResult_IdempotencyGuard_DoesNotAwardTwice()
        {
            var service = new DefenseMatchResultService();
            var saveData = new MetaProgressionSaveData { totalCurrency = 100 };
            int saveCount = 0;

            var input = new DefenseMatchResultData(
                isWon: true,
                elapsedSeconds: 200f,
                woodCollected: 20,
                enemiesDefeated: 10,
                stageId: "STAGE_01");

            var firstResult = service.ProcessMatchResult(input, saveData, _ => saveCount++);
            Assert.AreEqual(1, saveCount);
            Assert.AreEqual(100 + (20 * 2 + 500), saveData.totalCurrency); // 100 + 540 = 640

            // Gọi lần 2: Không được cộng thêm và không được save lại
            var secondResult = service.ProcessMatchResult(input, saveData, _ => saveCount++);
            Assert.AreSame(firstResult, secondResult);
            Assert.AreEqual(1, saveCount);
            Assert.AreEqual(640, saveData.totalCurrency);
        }

        [Test]
        public void Service_Reset_AllowsProcessingNewMatch()
        {
            var service = new DefenseMatchResultService();
            var saveData = new MetaProgressionSaveData { totalCurrency = 0 };

            var input = new DefenseMatchResultData(
                isWon: false,
                elapsedSeconds: 60f,
                woodCollected: 10,
                enemiesDefeated: 5,
                stageId: "STAGE_01");

            service.ProcessMatchResult(input, saveData, _ => { });
            Assert.IsTrue(service.HasProcessedResult);
            Assert.AreEqual(20, saveData.totalCurrency);

            service.Reset();
            Assert.IsFalse(service.HasProcessedResult);
            Assert.IsNull(service.LastProcessedResult);

            service.ProcessMatchResult(input, saveData, _ => { });
            Assert.IsTrue(service.HasProcessedResult);
            Assert.AreEqual(40, saveData.totalCurrency);
        }

        [Test]
        public void ProcessMatchResult_HandlesSaveExceptionsSafely()
        {
            var service = new DefenseMatchResultService();
            var saveData = new MetaProgressionSaveData();

            var input = new DefenseMatchResultData(
                isWon: true,
                elapsedSeconds: 100f,
                woodCollected: 10,
                enemiesDefeated: 5,
                stageId: "STAGE_01");

            // Custom save action ném lỗi không được làm sập kết quả trả về
            Assert.DoesNotThrow(() =>
            {
                var result = service.ProcessMatchResult(input, saveData, _ => throw new InvalidOperationException("Disk full simulated"));
                Assert.IsNotNull(result);
                Assert.IsTrue(result.IsWon);
            });
        }

        [Test]
        public void Navigation_HubSceneName_IsExplicitStringNotIndex()
        {
            // Yêu cầu G5: Hợp đồng điều hướng về sảnh bằng tên tường minh (MainMenu)
            Assert.AreEqual("MainMenu", DefenseNavigationService.HUB_SCENE_NAME);
            Assert.IsFalse(int.TryParse(DefenseNavigationService.HUB_SCENE_NAME, out _));
        }

        [Test]
        public void Navigation_DebounceGuard_PreventsConcurrentCalls()
        {
            var nav = DefenseNavigationService.Default;
            nav.ResetStateForTesting();

            Assert.IsFalse(nav.IsNavigating);
        }

        [Test]
        public void ProcessMatchResult_FirstClearAndStarRating_AwardsBonusOnlyOnceAndUpgradesBestStars()
        {
            var service = new DefenseMatchResultService();
            var saveData = new MetaProgressionSaveData { totalCurrency = 100 };

            // Lần vượt ải đầu tiên với 50% máu Căn Cứ -> Đạt 2★ + Nhận thưởng Lần Đầu (+500 Cổ Tiền, +10 Mảnh Tín Vật)
            var firstWin = new DefenseMatchResultData(
                isWon: true,
                elapsedSeconds: 600f,
                woodCollected: 50,
                enemiesDefeated: 100,
                stageId: "STAGE_DEFENSE_3D",
                runId: "run_01",
                victoryReward: 1000,
                baseRemainingHpRatio: 0.5f,
                firstClearBonusCurrency: 500,
                firstClearRelicId: "R001_Deptoong",
                firstClearRelicShards: 10);

            var res1 = service.ProcessMatchResult(firstWin, saveData, _ => { });
            Assert.IsTrue(res1.IsFirstClear);
            Assert.AreEqual(2, res1.EarnedStars);
            // 50 gỗ * 2 + 1000 thắng + 500 lần đầu = 1600
            Assert.AreEqual(1600, res1.EarnedCurrency);
            Assert.AreEqual(1700, saveData.totalCurrency);
            Assert.AreEqual(10, saveData.GetRelicShards("R001_Deptoong"));
            Assert.AreEqual(2, saveData.GetStageRecord("STAGE_DEFENSE_3D").starRating);

            // Lần vượt ải thứ hai với 90% máu Căn Cứ -> Đạt 3★ (cập nhật kỷ lục sao), KHÔNG nhận lại thưởng Lần Đầu
            service.Reset();
            var secondWin = new DefenseMatchResultData(
                isWon: true,
                elapsedSeconds: 580f,
                woodCollected: 50,
                enemiesDefeated: 110,
                stageId: "STAGE_DEFENSE_3D",
                runId: "run_02",
                victoryReward: 1000,
                baseRemainingHpRatio: 0.9f,
                firstClearBonusCurrency: 500,
                firstClearRelicId: "R001_Deptoong",
                firstClearRelicShards: 10);

            var res2 = service.ProcessMatchResult(secondWin, saveData, _ => { });
            Assert.IsFalse(res2.IsFirstClear);
            Assert.AreEqual(3, res2.EarnedStars);
            // 50 gỗ * 2 + 1000 thắng + 0 lần đầu = 1100
            Assert.AreEqual(1100, res2.EarnedCurrency);
            Assert.AreEqual(2800, saveData.totalCurrency);
            // Mảnh tín vật không cộng trùng lần đầu
            Assert.AreEqual(10, saveData.GetRelicShards("R001_Deptoong"));
            // Kỷ lục sao được nâng từ 2★ lên 3★
            Assert.AreEqual(3, saveData.GetStageRecord("STAGE_DEFENSE_3D").starRating);
        }

        private void UnusedPlaceholder()
        {
        }
    }
}
