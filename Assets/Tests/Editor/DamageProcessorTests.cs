using NUnit.Framework;
using UnityEngine;
using ProjectZombie.Features.Combat;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Tests.Editor
{
    [TestFixture]
    public class DamageProcessorTests
    {
        private GameObject _targetObj;
        private HealthSystem _healthSystem;
        private IDamageProcessor _damageProcessor;

        [SetUp]
        public void SetUp()
        {
            _targetObj = new GameObject("Target_Dummy");
            _healthSystem = _targetObj.AddComponent<HealthSystem>();
            _healthSystem.SetMaxHealth(100f, fillCurrentHealth: true);
            _damageProcessor = new LocalDamageProcessor();
        }

        [TearDown]
        public void TearDown()
        {
            if (_targetObj != null) Object.DestroyImmediate(_targetObj);
        }

        [Test]
        public void LocalDamageProcessor_AppliesDamageAccurately()
        {
            float initialHealth = _healthSystem.CurrentHealth;
            _damageProcessor.ApplyDamage(_healthSystem, 30f);

            Assert.AreEqual(70f, _healthSystem.CurrentHealth, 0.001f);
            Assert.IsTrue(_healthSystem.IsAlive);
        }

        [Test]
        public void LocalDamageProcessor_WhenDamageExceedsMaxHealth_TriggersDeath()
        {
            bool diedEventFired = false;
            _healthSystem.OnDied += () => diedEventFired = true;

            _damageProcessor.ApplyDamage(_healthSystem, 150f);

            Assert.AreEqual(0f, _healthSystem.CurrentHealth, 0.001f);
            Assert.IsFalse(_healthSystem.IsAlive);
            Assert.IsTrue(diedEventFired);
        }

        [Test]
        public void LocalDamageProcessor_WhenTargetNull_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _damageProcessor.ApplyDamage(null, 50f));
        }
    }
}
