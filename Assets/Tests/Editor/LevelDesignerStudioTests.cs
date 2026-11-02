using System.Linq;
using NUnit.Framework;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

namespace ProjectZombie.Tests.LevelDesign
{
    [TestFixture]
    public sealed class LevelDesignerStudioTests
    {
        [Test]
        public void DifficultyCurveProfile_EvaluatesSpawnRateAndHpScalingCorrectly()
        {
            var profile = ScriptableObject.CreateInstance<StageDifficultyCurveProfileSO>();
            Assert.IsNotNull(profile);

            // Minute 0: Initial spawn rate
            float initialRate = profile.EvaluateSpawnRate(0f);
            Assert.GreaterOrEqual(initialRate, 10f);

            // Minute 6: Mid-to-late game rate should be substantially higher
            float midRate = profile.EvaluateSpawnRate(360f);
            Assert.Greater(midRate, initialRate);

            // HP Scaling at minute 0 should be 1.0x
            float initialHpScale = profile.EvaluateHpScaling(0f);
            Assert.AreEqual(1.0f, initialHpScale, 0.05f);

            // HP Scaling at minute 8 should be significantly higher (> 2.0x)
            float lateHpScale = profile.EvaluateHpScaling(510f);
            Assert.Greater(lateHpScale, 2.0f);

            // Wood income should increase over time
            float earlyWood = profile.EvaluateExpectedWood(60f);
            float lateWood = profile.EvaluateExpectedWood(480f);
            Assert.Greater(lateWood, earlyWood);
        }

        [Test]
        public void Milestone_IncreasesIntensityDuringHordeRush()
        {
            var profile = ScriptableObject.CreateInstance<StageDifficultyCurveProfileSO>();

            // Find Horde milestone around 120s
            var horde = profile.GetActiveMilestone(130f);
            Assert.IsNotNull(horde);
            Assert.AreEqual(StagePacingType.HordeRush, horde.pacingType);
            Assert.Greater(horde.intensityMultiplier, 1.0f);

            // Spawn rate at 130s should be amplified by the milestone intensity multiplier
            float baseRateWithoutMilestone = profile.SpawnRateCurve.Evaluate(130f);
            float evaluatedRate = profile.EvaluateSpawnRate(130f);
            Assert.AreEqual(baseRateWithoutMilestone * horde.intensityMultiplier, evaluatedRate, 0.1f);

            // Outside milestone (e.g. at 100s), active milestone should be null
            var normalTime = profile.GetActiveMilestone(100f);
            Assert.IsNull(normalTime);
        }

        [Test]
        public void Blueprint_WithDifficultyCurve_ValidatesAndCompilesCleanly()
        {
            var blueprint = ScriptableObject.CreateInstance<DefenseStageBlueprintSO>();
            blueprint.stageId = "STAGE_TEST";
            blueprint.stageName = "Ải Thử Nghiệm";
            blueprint.stageDurationSeconds = 600f;
            blueprint.bossSpawnTime = 510f;
            blueprint.bossHealthOverride = 1500f;

            // Setup boss & enemy definitions from existing assets
            var bossEnemy = UnityEditor.AssetDatabase.LoadAssetAtPath<DefenseEnemyDataSO>("Assets/Resources/Defense3D/Enemies/yaksha_boss.asset");
            var mobEnemy = UnityEditor.AssetDatabase.LoadAssetAtPath<DefenseEnemyDataSO>("Assets/Resources/Defense3D/Enemies/magiap.asset");
            blueprint.arenaBoss = bossEnemy;

            // Setup spawn zones
            blueprint.spawnZones = new[]
            {
                new DefenseSpawnZoneBlueprint
                {
                    zoneType = DefenseSpawnZoneType.EastBank,
                    origin = Vector3.zero,
                    points = new[] { new DefenseSpawnPointConfig { Offset = Vector3.zero, Lane = 1 } }
                },
                new DefenseSpawnZoneBlueprint
                {
                    zoneType = DefenseSpawnZoneType.ArenaEast,
                    origin = new Vector3(10f, 0f, 0f),
                    points = new[] { new DefenseSpawnPointConfig { Offset = Vector3.zero, Lane = 1 } }
                }
            };

            // Link curve
            var curve = ScriptableObject.CreateInstance<StageDifficultyCurveProfileSO>();
            blueprint.difficultyProfile = curve;

            // Setup wave
            blueprint.timelineWaves.Add(new DefenseStageWaveEntry
            {
                eventId = "wave-01",
                triggerTime = 15f,
                zone = DefenseSpawnZoneType.EastBank,
                enemy = mobEnemy ?? bossEnemy,
                count = 4
            });

            // Validate
            var report = DefenseStageValidator.Validate(blueprint);
            Assert.IsTrue(report.IsValid, string.Join("; ", report.Errors));

            // Compile into plan
            var plan = blueprint.Compile();
            Assert.IsNotNull(plan);
            Assert.AreEqual("STAGE_TEST", plan.ProfileVersion);
            Assert.AreEqual(2, plan.Events.Count); // 1 ordinary wave + 1 boss event
        }
    }
}
