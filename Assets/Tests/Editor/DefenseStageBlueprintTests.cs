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
    }
}
