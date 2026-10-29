using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

namespace ProjectZombie.Tests.Editor
{
    [TestFixture]
    public sealed class DefenseStageBlueprintTests
    {
        private DefenseEnemyDataSO _mockEnemy;
        private DefenseEnemyDataSO _mockBoss;

        [SetUp]
        public void SetUp()
        {
            _mockEnemy = ScriptableObject.CreateInstance<DefenseEnemyDataSO>();
            typeof(DefenseEnemyDataSO).GetField("enemyId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(_mockEnemy, "mock_enemy");

            _mockBoss = ScriptableObject.CreateInstance<DefenseEnemyDataSO>();
            typeof(DefenseEnemyDataSO).GetField("enemyId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(_mockBoss, "mock_boss");
        }

        [TearDown]
        public void TearDown()
        {
            if (_mockEnemy != null) UnityEngine.Object.DestroyImmediate(_mockEnemy);
            if (_mockBoss != null) UnityEngine.Object.DestroyImmediate(_mockBoss);
        }

        [Test]
        public void ZoneEnum_Extensions_MapsCorrectlyBothWays()
        {
            Assert.AreEqual("eastBank", DefenseSpawnZoneType.EastBank.ToZoneId());
            Assert.AreEqual("northCanopy", DefenseSpawnZoneType.NorthCanopy.ToZoneId());
            Assert.AreEqual("southBank", DefenseSpawnZoneType.SouthBank.ToZoneId());
            Assert.AreEqual("canalWater", DefenseSpawnZoneType.CanalWater.ToZoneId());
            Assert.AreEqual("arenaEast", DefenseSpawnZoneType.ArenaEast.ToZoneId());

            Assert.AreEqual(DefenseSpawnZoneType.EastBank, DefenseSpawnZoneExtensions.FromZoneId("eastBank"));
            Assert.AreEqual(DefenseSpawnZoneType.NorthCanopy, DefenseSpawnZoneExtensions.FromZoneId("northCanopy"));
            Assert.AreEqual(DefenseSpawnZoneType.SouthBank, DefenseSpawnZoneExtensions.FromZoneId("southBank"));
            Assert.AreEqual(DefenseSpawnZoneType.CanalWater, DefenseSpawnZoneExtensions.FromZoneId("canalWater"));
            Assert.AreEqual(DefenseSpawnZoneType.ArenaEast, DefenseSpawnZoneExtensions.FromZoneId("arenaEast"));
        }

        [Test]
        public void Compile_WithValidBlueprint_ProducesValidPlan()
        {
            var bp = ScriptableObject.CreateInstance<DefenseStageBlueprintSO>();
            bp.stageId = "TEST_STAGE";
            bp.arenaBoss = _mockBoss;
            bp.bossHealthOverride = 1500f;
            bp.bossSpawnTime = 500f;

            bp.spawnZones = new[]
            {
                new DefenseSpawnZoneBlueprint
                {
                    zoneType = DefenseSpawnZoneType.EastBank,
                    relativeToEastMarker = true,
                    origin = Vector3.zero,
                    points = new[] { new DefenseSpawnPointConfig { Offset = Vector3.zero, Lane = 1 } }
                }
            };

            bp.timelineWaves = new List<DefenseStageWaveEntry>
            {
                new DefenseStageWaveEntry
                {
                    eventId = "wave_01",
                    triggerTime = 10f,
                    enemy = _mockEnemy,
                    zone = DefenseSpawnZoneType.EastBank,
                    count = 3
                }
            };

            try
            {
                var plan = bp.Compile();
                Assert.IsNotNull(plan);
                Assert.AreEqual("TEST_STAGE", plan.ProfileVersion);
                Assert.AreEqual(2, plan.Events.Count); // 1 wave + 1 arena boss
                Assert.AreEqual(1500f, plan.BossHealth);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bp);
            }
        }

        [Test]
        public void Compile_MissingBoss_ThrowsException()
        {
            var bp = ScriptableObject.CreateInstance<DefenseStageBlueprintSO>();
            bp.arenaBoss = null;

            try
            {
                Assert.Throws<InvalidOperationException>(() => bp.Compile());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bp);
            }
        }

        [Test]
        public void Compile_DuplicateWaveEventId_ThrowsException()
        {
            var bp = ScriptableObject.CreateInstance<DefenseStageBlueprintSO>();
            bp.stageId = "TEST_STAGE";
            bp.arenaBoss = _mockBoss;

            bp.spawnZones = new[]
            {
                new DefenseSpawnZoneBlueprint
                {
                    zoneType = DefenseSpawnZoneType.EastBank,
                    points = new[] { new DefenseSpawnPointConfig { Offset = Vector3.zero, Lane = 1 } }
                }
            };

            bp.timelineWaves = new List<DefenseStageWaveEntry>
            {
                new DefenseStageWaveEntry { eventId = "dup_id", triggerTime = 10f, enemy = _mockEnemy, zone = DefenseSpawnZoneType.EastBank, count = 1 },
                new DefenseStageWaveEntry { eventId = "dup_id", triggerTime = 20f, enemy = _mockEnemy, zone = DefenseSpawnZoneType.EastBank, count = 1 }
            };

            try
            {
                Assert.Throws<InvalidOperationException>(() => bp.Compile());
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bp);
            }
        }

        [Test]
        public void Compile_SortsEventsChronologically_InPlan()
        {
            var bp = ScriptableObject.CreateInstance<DefenseStageBlueprintSO>();
            bp.stageId = "TEST_STAGE";
            bp.arenaBoss = _mockBoss;
            bp.bossSpawnTime = 300f;

            bp.spawnZones = new[]
            {
                new DefenseSpawnZoneBlueprint
                {
                    zoneType = DefenseSpawnZoneType.EastBank,
                    points = new[] { new DefenseSpawnPointConfig { Offset = Vector3.zero, Lane = 1 } }
                }
            };

            // Waves defined out of order
            bp.timelineWaves = new List<DefenseStageWaveEntry>
            {
                new DefenseStageWaveEntry { eventId = "wave_late", triggerTime = 120f, enemy = _mockEnemy, zone = DefenseSpawnZoneType.EastBank, count = 2 },
                new DefenseStageWaveEntry { eventId = "wave_early", triggerTime = 15f, enemy = _mockEnemy, zone = DefenseSpawnZoneType.EastBank, count = 1 }
            };

            try
            {
                var plan = bp.Compile();
                Assert.IsNotNull(plan);
                Assert.AreEqual(3, plan.Events.Count);
                Assert.AreEqual("wave_early", plan.Events[0].EventId);
                Assert.AreEqual(15f, plan.Events[0].TriggerTime);
                Assert.AreEqual("wave_late", plan.Events[1].EventId);
                Assert.AreEqual(120f, plan.Events[1].TriggerTime);
                Assert.AreEqual("arena-boss", plan.Events[2].EventId);
                Assert.AreEqual(300f, plan.Events[2].TriggerTime);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bp);
            }
        }

        [Test]
        public void Validator_DetectsEmptyStageId_ReturnsError()
        {
            var bp = ScriptableObject.CreateInstance<DefenseStageBlueprintSO>();
            bp.stageId = "";
            bp.arenaBoss = _mockBoss;

            try
            {
                var report = DefenseStageValidator.Validate(bp);
                Assert.IsFalse(report.IsValid);
                Assert.IsTrue(report.Errors.Exists(e => e.Contains("StageId")));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bp);
            }
        }

        [Test]
        public void Validator_DetectsUnmappedSpawnZone_ReturnsError()
        {
            var bp = ScriptableObject.CreateInstance<DefenseStageBlueprintSO>();
            bp.stageId = "TEST_STAGE";
            bp.arenaBoss = _mockBoss;
            bp.spawnZones = new[]
            {
                new DefenseSpawnZoneBlueprint
                {
                    zoneType = DefenseSpawnZoneType.EastBank,
                    points = new[] { new DefenseSpawnPointConfig { Offset = Vector3.zero, Lane = 1 } }
                }
            };
            bp.timelineWaves = new List<DefenseStageWaveEntry>
            {
                new DefenseStageWaveEntry
                {
                    eventId = "wave_water",
                    triggerTime = 30f,
                    enemy = _mockEnemy,
                    zone = DefenseSpawnZoneType.CanalWater, // Not in spawnZones!
                    count = 2
                }
            };

            try
            {
                var report = DefenseStageValidator.Validate(bp);
                Assert.IsFalse(report.IsValid);
                Assert.IsTrue(report.Errors.Exists(e => e.Contains("CanalWater")));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bp);
            }
        }

        [Test]
        public void Validator_DetectsExcessiveSingleWaveCount_ReturnsWarning()
        {
            var bp = ScriptableObject.CreateInstance<DefenseStageBlueprintSO>();
            bp.stageId = "TEST_STAGE";
            bp.arenaBoss = _mockBoss;
            bp.maxActiveEnemies = 20;
            bp.spawnZones = new[]
            {
                new DefenseSpawnZoneBlueprint
                {
                    zoneType = DefenseSpawnZoneType.EastBank,
                    points = new[] { new DefenseSpawnPointConfig { Offset = Vector3.zero, Lane = 1 } }
                }
            };
            bp.timelineWaves = new List<DefenseStageWaveEntry>
            {
                new DefenseStageWaveEntry
                {
                    eventId = "wave_huge",
                    triggerTime = 30f,
                    enemy = _mockEnemy,
                    zone = DefenseSpawnZoneType.EastBank,
                    count = 35 // > maxActiveEnemies!
                }
            };

            try
            {
                var report = DefenseStageValidator.Validate(bp);
                Assert.IsTrue(report.Warnings.Exists(w => w.Contains("vượt giới hạn an toàn")));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(bp);
            }
        }
    }
}
