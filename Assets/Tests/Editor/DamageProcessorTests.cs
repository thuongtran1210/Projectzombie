using NUnit.Framework;
using UnityEngine;
using ProjectZombie.Features.Combat;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Elements;
using ProjectZombie.Features.Weapons;
using System.Reflection;

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

        [TestCase(ElementType.Kim, ElementType.Moc)]
        [TestCase(ElementType.Moc, ElementType.Tho)]
        [TestCase(ElementType.Thuy, ElementType.Hoa)]
        [TestCase(ElementType.Hoa, ElementType.Kim)]
        [TestCase(ElementType.Tho, ElementType.Thuy)]
        public void ElementDamage_AppliesCounterExactlyOnce(ElementType attack, ElementType defense)
        {
            _healthSystem.SetMaxHealth(1000f);
            _healthSystem.CurrentElement = defense;
            _healthSystem.TakeDamage(new DamageData(100f, element: attack));
            Assert.That(_healthSystem.CurrentHealth, Is.EqualTo(870f).Within(0.001f));
            var resolved = DamageUtility.CalculateHitDamage(100f, false, attack, defense);
            _healthSystem.TakeDamage(resolved);
            Assert.That(_healthSystem.CurrentHealth, Is.EqualTo(740f).Within(0.001f));
        }

        [Test]
        public void ExplosionContext_PreservesCriticalElementSourceAndResolvesEachTarget()
        {
            _healthSystem.SetMaxHealth(1000f);
            DamageData observed = default;
            _healthSystem.OnDamageTaken += damage => observed = damage;
            var context = new DamageContext(null, 200f, ElementType.Hoa, true, _targetObj);
            _healthSystem.CurrentElement = ElementType.Kim;
            _healthSystem.TakeDamage(context);
            Assert.That(observed.Amount, Is.EqualTo(260f).Within(0.001f));
            Assert.That(observed.IsCritical && observed.IsCounter);
            Assert.That(observed.Element, Is.EqualTo(ElementType.Hoa));
            Assert.That(observed.SourceWeapon, Is.SameAs(_targetObj));
            _healthSystem.CurrentElement = ElementType.Thuy;
            _healthSystem.TakeDamage(context);
            Assert.That(observed.Amount, Is.EqualTo(200f).Within(0.001f));
            Assert.That(observed.IsCounter, Is.False);
        }

        [TestCase(ElementType.Kim, ElementType.Tho)]
        [TestCase(ElementType.Moc, ElementType.Thuy)]
        [TestCase(ElementType.Thuy, ElementType.Kim)]
        [TestCase(ElementType.Hoa, ElementType.Moc)]
        [TestCase(ElementType.Tho, ElementType.Hoa)]
        public void ScholarAutoElement_IsGenerativeParent(ElementType relic, ElementType expected)
        {
            Assert.That(ElementSynergyRules.GetGenerativeParent(relic), Is.EqualTo(expected));
            Assert.That(ElementSynergyRules.IsElementGenerative(expected, relic));
        }
    }

    public sealed class ElementTestSynergyReceiver : MonoBehaviour, IElementSynergyReceiver
    {
        public int RewardApplyCount { get; private set; }
        public int RewardResetCount { get; private set; }
        public void ApplyElementSynergyReward() => RewardApplyCount++;
        public void ResetElementSynergyReward() => RewardResetCount++;
    }

    public sealed class ElementTestRelic : WeaponBase
    {
        protected override void PerformAttack() { }
    }

    public class ElementCycleRegressionTests
    {
        private GameObject _managerObject;
        private GameObject _ownerA;
        private GameObject _ownerB;
        private ElementCycleManager _manager;
        private ElementTestRelic _relicA;
        private ElementTestRelic _relicB;
        private ElementTestSynergyReceiver _receiverA;
        private ElementTestSynergyReceiver _receiverB;
        private int _procs;

        [SetUp]
        public void SetUp()
        {
            _managerObject = new GameObject("Element test manager");
            _manager = _managerObject.AddComponent<ElementCycleManager>();
            typeof(ElementCycleManager).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_manager, null);
            _ownerA = new GameObject("Owner A");
            _receiverA = _ownerA.AddComponent<ElementTestSynergyReceiver>();
            _ownerB = new GameObject("Owner B");
            _receiverB = _ownerB.AddComponent<ElementTestSynergyReceiver>();

            var relicAObject = new GameObject("Relic A");
            relicAObject.transform.SetParent(_ownerA.transform);
            _relicA = relicAObject.AddComponent<ElementTestRelic>();
            _relicA.element = ElementType.Thuy;
            _relicA.isPrimaryActiveWeapon = false;
            _relicA.activeCooldown = 10f;
            SetCooldown(_relicA, 5f);

            var relicBObject = new GameObject("Relic B");
            relicBObject.transform.SetParent(_ownerB.transform);
            _relicB = relicBObject.AddComponent<ElementTestRelic>();
            _relicB.element = ElementType.Thuy;
            _relicB.isPrimaryActiveWeapon = false;
            _relicB.activeCooldown = 10f;
            SetCooldown(_relicB, 5f);

            _procs = 0;
            _manager.OnElementSynergyTriggered += (_, __, ___) => _procs++;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_managerObject);
            Object.DestroyImmediate(_ownerA);
            Object.DestroyImmediate(_ownerB);
        }

        private void SetCooldown(WeaponBase relic, float remaining)
        {
            typeof(WeaponBase).GetField("_lastRelicSkillCastTime", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(relic, Time.time - relic.activeCooldown + remaining);
            typeof(WeaponBase).GetField("_currentRelicPhase", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(relic, WeaponBase.RelicCastPhase.Cooldown);
        }

        [Test]
        public void Synergy_InvokesReceiverReward_AndResonates()
        {
            _manager.RegisterHit(ElementType.Kim, null, _ownerA, ElementHitSource.HeroBasicAttack, ElementSynergyRules.NextAttackId());
            _manager.RegisterHit(ElementType.Thuy, _relicA, _ownerA, ElementHitSource.Relic, ElementSynergyRules.NextAttackId());
            Assert.That(_procs, Is.EqualTo(1));
            Assert.That(_receiverA.RewardApplyCount, Is.EqualTo(1));
        }

        [Test]
        public void Players_DoNotShareHitsOrProcCooldown()
        {
            _manager.RegisterHit(ElementType.Kim, null, _ownerA, ElementHitSource.HeroBasicAttack, ElementSynergyRules.NextAttackId());
            _manager.RegisterHit(ElementType.Thuy, _relicB, _ownerB, ElementHitSource.Relic, ElementSynergyRules.NextAttackId());
            Assert.That(_procs, Is.Zero);

            _manager.RegisterHit(ElementType.Thuy, _relicA, _ownerA, ElementHitSource.Relic, ElementSynergyRules.NextAttackId());
            Assert.That(_procs, Is.EqualTo(1));
            Assert.That(_receiverA.RewardApplyCount, Is.EqualTo(1));
            Assert.That(_receiverB.RewardApplyCount, Is.Zero);

            // Owner B primes and procs independently
            _manager.RegisterHit(ElementType.Moc, null, _ownerB, ElementHitSource.HeroBasicAttack, ElementSynergyRules.NextAttackId());
            _relicB.element = ElementType.Hoa;
            _manager.RegisterHit(ElementType.Hoa, _relicB, _ownerB, ElementHitSource.Relic, ElementSynergyRules.NextAttackId());
            Assert.That(_procs, Is.EqualTo(2));
            Assert.That(_receiverB.RewardApplyCount, Is.EqualTo(1));
        }

        [Test]
        public void VirtualHit_IsConsumedOnceAndDoesNotChangeOtherPlayer()
        {
            _manager.PushVirtualElementHit(ElementType.Kim, _ownerA);
            _manager.RegisterHit(ElementType.Thuy, _relicB, _ownerB, ElementHitSource.Relic, ElementSynergyRules.NextAttackId());
            Assert.That(_procs, Is.Zero);

            _manager.RegisterHit(ElementType.Thuy, _relicA, _ownerA, ElementHitSource.Relic, ElementSynergyRules.NextAttackId());
            _manager.RegisterHit(ElementType.Thuy, _relicA, _ownerA, ElementHitSource.Relic, ElementSynergyRules.NextAttackId());
            Assert.That(_procs, Is.EqualTo(1));
            Assert.That(_receiverA.RewardApplyCount, Is.EqualTo(1));
        }

        [Test]
        public void MultiTargetAttack_RegistersOnlyFirstSuccessfulHit()
        {
            var target = new GameObject("Target");
            var health = target.AddComponent<HealthSystem>();
            health.SetMaxHealth(1000f);
            var record = ElementAttackRecord.Acquire(_ownerA);
            try
            {
                var hit = new DamageData(10f, element: ElementType.Kim) 
                { 
                    AttackRecord = record, 
                    Owner = _ownerA, 
                    HitSource = ElementHitSource.HeroBasicAttack, 
                    AttackId = record.AttackId 
                };
                health.CustomDamageInterceptor = (_, __) => true;
                health.TakeDamage(hit);
                _manager.RegisterHit(ElementType.Thuy, _relicA, _ownerA, ElementHitSource.Relic, ElementSynergyRules.NextAttackId());
                Assert.That(_procs, Is.Zero);

                health.CustomDamageInterceptor = null;
                health.TakeDamage(hit);
                _manager.RegisterHit(ElementType.Thuy, _relicA, _ownerA, ElementHitSource.Relic, ElementSynergyRules.NextAttackId());
                Assert.That(_procs, Is.EqualTo(1));

                // Repeated hit with same record does not register another lead
                health.TakeDamage(hit);
                _relicA.element = ElementType.Moc;
                _manager.RegisterHit(ElementType.Moc, _relicA, _ownerA, ElementHitSource.Relic, ElementSynergyRules.NextAttackId());
                Assert.That(_procs, Is.EqualTo(1));
            }
            finally
            {
                record.Release();
                Object.DestroyImmediate(target);
            }
        }

        [Test]
        public void RecastWindow_IsNotShortenedByCooldownReduction()
        {
            _relicA.hasRecastPhase = true;
            typeof(WeaponBase).GetField("_currentRelicPhase", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(_relicA, WeaponBase.RelicCastPhase.RecastReady);
            _relicA.ReduceRelicSkillCooldown(0.2f);
            Assert.That(_relicA.CurrentRelicPhase, Is.EqualTo(WeaponBase.RelicCastPhase.RecastReady));
            Assert.That(_relicA.RelicRemainingCooldown, Is.EqualTo(5f).Within(0.001f));
        }

        [Test]
        public void ScholarManualSelection_IsPreservedByExecution()
        {
            var skill = new ProjectZombie.Features.Player.Skills.ThuSinhSignatureSkill();
            ElementType selected = ElementType.None;
            skill.ExecuteWithElement(_ownerA, ElementType.Hoa, element => selected = element);
            Assert.That(selected, Is.EqualTo(ElementType.Hoa));

            _relicA.element = ElementType.Tho;
            _manager.RegisterHit(ElementType.Tho, _relicA, _ownerA, ElementHitSource.Relic, ElementSynergyRules.NextAttackId());
            Assert.That(_procs, Is.EqualTo(1));
            _manager.RegisterHit(ElementType.Tho, _relicA, _ownerA, ElementHitSource.Relic, ElementSynergyRules.NextAttackId());
            Assert.That(_procs, Is.EqualTo(1));
        }

        [Test]
        public void CharacterCombat_ApplyElementSynergyReward_AppliesCooldownReductionAndResonance()
        {
            _ownerA.SetActive(false);
            var stats = _ownerA.AddComponent<ProjectZombie.Features.Player.PlayerStats>();
            var combat = _ownerA.AddComponent<ProjectZombie.Features.Player.CharacterCombat>();
            typeof(ProjectZombie.Features.Player.CharacterCombat).GetField("_playerStats", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(combat, stats);
            combat.ApplyElementSynergyReward();
            Assert.That(combat.ResonanceRemainingDuration, Is.GreaterThan(0f));
        }

        [Test]
        public void PooledAttackRecord_DoesNotKeepPreviousOwnerOrHit()
        {
            var first = ElementAttackRecord.Acquire(_ownerA);
            first.RegisterSuccessfulHit(ElementType.Kim);
            first.Release();
            var second = ElementAttackRecord.Acquire(_ownerB);
            try
            {
                second.RegisterSuccessfulHit(ElementType.Kim);
                _manager.RegisterHit(ElementType.Thuy, _relicB, _ownerB, ElementHitSource.Relic, ElementSynergyRules.NextAttackId());
                Assert.That(_procs, Is.EqualTo(1));
            }
            finally { second.Release(); }
        }

        [Test]
        public void BasicAttack_UsesOwnerElementAndSkillOverride()
        {
            _ownerA.SetActive(false);
            var stats = _ownerA.AddComponent<ProjectZombie.Features.Player.PlayerStats>();
            var combat = _ownerA.AddComponent<ProjectZombie.Features.Player.CharacterCombat>();
            typeof(ProjectZombie.Features.Player.CharacterCombat).GetField("_playerStats", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(combat, stats);
            combat.SetAttackConfig(new ProjectZombie.Features.Player.CharacterAttackConfig { element = ElementType.None });
            stats.SetBaseElement(ElementType.Kim);
            Assert.That(combat.AttackElement, Is.EqualTo(ElementType.Kim));
            stats.SetElementOverride(ElementType.Hoa);
            Assert.That(combat.AttackElement, Is.EqualTo(ElementType.Hoa));
            stats.SetElementOverride(ElementType.None);
            Assert.That(combat.AttackElement, Is.EqualTo(ElementType.Kim));
        }

        [Test]
        public void ScholarAutoSelection_UsesActiveRelicCooldown()
        {
            _ownerA.SetActive(false);
            var inventory = _ownerA.AddComponent<WeaponManager>();
            var weapons = (System.Collections.Generic.List<WeaponBase>)typeof(WeaponManager)
                .GetField("_activeWeapons", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(inventory);
            weapons.Add(_relicA);
            var other = new GameObject("Other relic");
            other.transform.SetParent(_ownerA.transform);
            var fireRelic = other.AddComponent<ElementTestRelic>();
            fireRelic.element = ElementType.Hoa;
            fireRelic.activeCooldown = 20f;
            typeof(WeaponBase).GetField("_lastRelicSkillCastTime", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(fireRelic, Time.time - 12f);
            weapons.Add(fireRelic);
            var skill = new ProjectZombie.Features.Player.Skills.ThuSinhSignatureSkill();
            Assert.That(skill.GetAutoSelectFallbackElement(_ownerA), Is.EqualTo(ElementType.Moc));
        }
    }
}
