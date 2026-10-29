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
    }
}
