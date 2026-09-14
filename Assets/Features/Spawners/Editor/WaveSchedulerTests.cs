using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ProjectZombie.Features.Spawners;
using ProjectZombie.Features.Spawners.Core;

namespace ProjectZombie.Features.Spawners.Tests
{
    [TestFixture]
    public class WaveSchedulerTests
    {
        private LevelTimelineConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<LevelTimelineConfig>();
            _config.maxLevelDuration = 100f;
            _config.events = new List<TimelineEvent>
            {
                new TimelineEvent { eventName = "Wave 1", timestampSeconds = 0f },
                new TimelineEvent { eventName = "Wave 2", timestampSeconds = 30f },
                new TimelineEvent { eventName = "Boss Spawn", timestampSeconds = 60f }
            };
        }

        [Test]
        public void Test_MatchProgress_CalculatesCorrectly()
        {
            var scheduler = new WaveScheduler();
            scheduler.Initialize(_config);
            scheduler.StartMatch();

            Assert.AreEqual(0f, scheduler.MatchProgress, 0.001f);

            scheduler.Tick(50f, out bool isEnded);
            Assert.IsFalse(isEnded);
            Assert.AreEqual(0.5f, scheduler.MatchProgress, 0.001f);

            scheduler.Tick(50f, out isEnded);
            Assert.IsTrue(isEnded);
            Assert.AreEqual(1.0f, scheduler.MatchProgress, 0.001f);
        }

        [Test]
        public void Test_EventsTrigger_AtCorrectTimestamps()
        {
            var scheduler = new WaveScheduler();
            scheduler.Initialize(_config);
            scheduler.StartMatch();

            // Tại 0s, Wave 1 được kích hoạt ngay lập tức
            var eventsAt0 = scheduler.Tick(0f, out _);
            Assert.AreEqual(1, eventsAt0.Count);
            Assert.AreEqual("Wave 1", eventsAt0[0].eventName);

            // Tick 20s (tổng thời gian 20s) - chưa có event mới
            var eventsAt20 = scheduler.Tick(20f, out _);
            Assert.AreEqual(0, eventsAt20.Count);

            // Tick 15s (tổng thời gian 35s) - Wave 2 đến hạn
            var eventsAt35 = scheduler.Tick(15f, out _);
            Assert.AreEqual(1, eventsAt35.Count);
            Assert.AreEqual("Wave 2", eventsAt35[0].eventName);
        }
    }
}
