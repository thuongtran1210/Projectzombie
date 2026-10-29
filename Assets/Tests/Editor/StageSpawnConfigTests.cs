using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ProjectZombie.Features.Defense3D;
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.Tests.Editor
{
    public sealed class StageSpawnConfigTests
    {
        private static StageSpawnConfig Production => Resources.Load<StageSpawnConfig>(StageSpawnConfig.DefaultResource);

        [Test]
        public void ProductionProfile_Preserves34Events214SlotsAndIndependentBossAt510()
        {
            var plan = Production.Compile();
            var jobs = new List<DefenseSpawnJob>();
            new DefenseStageSpawnScheduler(plan.Events).EnqueueDue(600f, jobs.Add);
            var ordinary = jobs.Where(j => j.Kind == DefenseSpawnKind.Enemy).ToList();
            Assert.That(plan.Events.Count(e => e.Kind == DefenseSpawnKind.Enemy), Is.EqualTo(34));
            Assert.That(ordinary.Count, Is.EqualTo(214));
            Assert.That(ordinary.Count(j => j.ActualSpawnType == "phicuong"), Is.EqualTo(20));
            Assert.That(ordinary.Count(j => j.ActualSpawnType == "magiap"), Is.EqualTo(108));
            var miniboss = ordinary.Where(j => j.EventId == "benque-07").Single();
            Assert.That(miniboss.ScheduledTime, Is.EqualTo(108f));
            Assert.That(miniboss.ActualSpawnType, Is.EqualTo("magiap_boss"));
            var boss = jobs.Single(j => j.Kind == DefenseSpawnKind.ArenaBoss);
            Assert.That(boss.ScheduledTime, Is.EqualTo(510f));
            Assert.That(boss.ActualSpawnType, Is.EqualTo("yaksha_boss"));
        }

        [Test]
        public void Scheduler_CatchesUpWithoutDuplicates_AndResetReplaysSameJobs()
        {
            var plan = Production.Compile();
            var expected = new List<DefenseSpawnJob>();
            new DefenseStageSpawnScheduler(plan.Events).EnqueueDue(600f, expected.Add);
            var scheduler = new DefenseStageSpawnScheduler(plan.Events);
            var actual = new List<DefenseSpawnJob>();
            foreach (float time in new[] { 0f, 0f, 13f, 120f, 120f, 509f, 510f, 600f, 700f })
                scheduler.EnqueueDue(time, actual.Add);
            CollectionAssert.AreEqual(expected, actual);
            scheduler.Reset();
            actual.Clear();
            scheduler.EnqueueDue(600f, actual.Add);
            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void FastProfile_UsesSeparateTimes_WithoutChangingProductionOrMatchClock()
        {
            var production = Production.Compile();
            var fast = Resources.Load<StageSpawnConfig>("Defense3D/Stages/Stage01_FastSpawn").Compile();
            Assert.That(fast.Events.Single(e => e.Kind == DefenseSpawnKind.ArenaBoss).TriggerTime, Is.EqualTo(10f));
            Assert.That(production.Events.Single(e => e.Kind == DefenseSpawnKind.ArenaBoss).TriggerTime, Is.EqualTo(510f));
            Assert.That(fast.Events.Last().TriggerTime, Is.EqualTo(29.7f).Within(0.001f));
            Assert.That(new DefenseG1RuntimeAdapter().Run.Clock.DurationSeconds, Is.EqualTo(600f));
        }

        [TestCase("EventId", "benque-02")]
        [TestCase("ZoneId", "missing-zone")]
        public void Compile_RejectsDuplicateEventOrUndefinedZone(string field, string invalidValue)
        {
            var config = Object.Instantiate(Production);
            try
            {
                var serialized = new SerializedObject(config);
                serialized.FindProperty("events").GetArrayElementAtIndex(0).FindPropertyRelative(field).stringValue = invalidValue;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assert.Throws<System.InvalidOperationException>(() => config.Compile());
            }
            finally { Object.DestroyImmediate(config); }
        }

        [TestCase(-1f)]
        [TestCase(600f)]
        [TestCase(float.NaN)]
        public void Compile_RejectsInvalidEventTime(float time)
        {
            var config = Object.Instantiate(Production);
            try
            {
                var serialized = new SerializedObject(config);
                serialized.FindProperty("events").GetArrayElementAtIndex(0).FindPropertyRelative("TriggerTime").floatValue = time;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assert.Throws<System.InvalidOperationException>(() => config.Compile());
            }
            finally { Object.DestroyImmediate(config); }
        }

        [Test]
        public void Snapshot_IsIsolatedFromLaterAuthoringChanges_AndResolvesDistinctZones()
        {
            var config = Object.Instantiate(Production);
            try
            {
                var snapshot = config.Compile();
                var marker = new Vector3(25f, 0.1f, 0f);
                var job = new DefenseSpawnJob("test", 0, "magiap", "magiap", "eastBank", 0f);
                var east = snapshot.Resolve(job, marker);
                var serialized = new SerializedObject(config);
                serialized.FindProperty("zones").GetArrayElementAtIndex(0).FindPropertyRelative("Origin").vector3Value = Vector3.one * 500f;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Assert.That(snapshot.Resolve(job, marker).Position, Is.EqualTo(east.Position));
                job.Zone = "southBank";
                Assert.That(snapshot.Resolve(job, marker).Position, Is.Not.EqualTo(east.Position));
                job.Zone = "northCanopy";
                Assert.That(snapshot.Resolve(job, marker).Lane, Is.EqualTo(2));
            }
            finally { Object.DestroyImmediate(config); }
        }

        [Test]
        public void Planner_UsesConfiguredLimits_ReleasesCapacity_AndDoesNotStarveOtherZones()
        {
            var planner = new DefenseSpawnPlanner(1, 3, 1, 2f);
            var blocked = new DefenseSpawnJob("blocked", 0, "magiap", "magiap", "blocked", 0f);
            var safe = new DefenseSpawnJob("safe", 0, "magiap", "magiap", "safe", 0f);
            planner.EnqueueJob(blocked, 0f);
            planner.EnqueueJob(safe, 0f);
            Assert.That(planner.ProcessSpawns(0f, j => j.Zone == "safe"), Is.EqualTo(1));
            Assert.That(planner.CurrentActiveCount, Is.EqualTo(1));
            planner.EnqueueJob(safe, 0f);
            Assert.That(planner.ProcessSpawns(0.1f), Is.Zero);
            planner.NotifyEnemyDefeatedOrRemoved();
            Assert.That(planner.ProcessSpawns(0.2f, j => j.Zone == "safe"), Is.EqualTo(1));
            Assert.That(planner.ProcessSpawns(3f), Is.Zero);
            Assert.That(planner.PendingCount, Is.Zero);
        }
    }
}
