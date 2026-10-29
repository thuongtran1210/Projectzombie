using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ProjectZombie.Features.Defense3D;

public sealed class DefenseBasicAttackGameplayTests
{
    [TestCase(true)]
    [TestCase(false)]
    public void HeroHit_EmitsSparkOnlyForAcceptedDamage_AndCapturesLethalPosition(bool accepted)
    {
        using (var fixture = new HeroFixture())
        {
            var victim = fixture.AddVictim(accepted);
            victim.MoveOnDamage = accepted;
            int swings = 0, hits = 0;
            DefenseHitFeedback captured = default;
            fixture.Attack.Feedback.SwingResolved += swing => swings++;
            fixture.Attack.Feedback.HitConfirmed += hit => { hits++; captured = hit; };
            Vector3 expected = victim.TargetTransform.position + Vector3.up * .7f;
            fixture.Resolve();
            Assert.AreEqual(1, swings);
            Assert.AreEqual(accepted ? 1 : 0, hits);
            Assert.AreEqual(accepted ? fixture.Attack.EffectiveDamage : 0f, victim.TotalDamage);
            if (accepted)
            {
                Assert.AreEqual(expected.x, captured.Position.X, .001f);
                Assert.AreEqual(expected.y, captured.Position.Y, .001f);
                Assert.AreEqual(expected.z, captured.Position.Z, .001f);
                Assert.AreNotEqual(expected, victim.TargetTransform.position);
            }
        }
    }
    [Test]
    public void LethalHeroHit_StillPublishesImpactAfterEnemyDisablesAndMoves()
    {
        using (var fixture = new HeroFixture())
        {
            var enemy = fixture.AddDyingEnemy();
            Vector3 expected = enemy.transform.position + Vector3.up * .7f;
            int hits = 0;
            DefenseHitFeedback captured = default;
            fixture.Attack.Feedback.HitConfirmed += hit => { hits++; captured = hit; };
            fixture.Resolve();
            Assert.IsTrue(enemy.IsDead);
            Assert.IsFalse(enemy.gameObject.activeSelf);
            Assert.AreEqual(1, hits);
            Assert.AreEqual(expected.x, captured.Position.X, .001f);
            Assert.AreEqual(expected.y, captured.Position.Y, .001f);
            Assert.AreEqual(expected.z, captured.Position.Z, .001f);
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public void EnemyBasicAttack_PublishesImpactOnlyWhenTargetAcceptsDamage(bool targetEnabled)
    {
        var actor = new GameObject("EnemyBasicFeedbackUnitActor");
        var targetObject = new GameObject("EnemyBasicFeedbackUnitTarget");
        using (var session = new DefenseSession())
        {
            try
            {
                actor.transform.position = new Vector3(22000, 0, 0);
                targetObject.transform.position = actor.transform.position + Vector3.forward * .25f;
                var attack = actor.AddComponent<DefenseEnemyAttack3D>();
                var enemy = actor.GetComponent<DefenseEnemy3D>();
                enemy.BindGameplay(session);
                enemy.Initialize(DefenseEnemyRegistry.GetStats(DefenseEnemyRegistry.ID_MAGIAP));
                attack.BindGameplay(session);
                var target = targetObject.AddComponent<DefenseDamageableComponent3D>();
                target.BindGameplay(session); target.enabled = targetEnabled;
                attack.Begin(target); session.Start();
                float before = target.CurrentHP;
                int swings = 0, hits = 0;
                attack.Feedback.SwingResolved += swing => swings++;
                attack.Feedback.HitConfirmed += hit => hits++;
                typeof(DefenseEnemyAttack3D).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(attack, null);
                Assert.AreEqual(1, swings);
                Assert.AreEqual(targetEnabled ? 1 : 0, hits);
                Assert.AreEqual(targetEnabled ? before - enemy.Stats.Damage : before, target.CurrentHP);
            }
            finally { UnityEngine.Object.DestroyImmediate(actor); UnityEngine.Object.DestroyImmediate(targetObject); }
        }
    }

    [Test]
    public void Miss_StillShowsSwingWithoutCreatingHitSpark()
    {
        using (var fixture = new HeroFixture())
        {
            var victim = fixture.AddVictim(true);
            victim.TargetTransform.position = fixture.Origin - Vector3.forward * 2f;
            int swings = 0, hits = 0;
            fixture.Attack.Feedback.SwingResolved += swing => swings++;
            fixture.Attack.Feedback.HitConfirmed += hit => hits++;
            fixture.Resolve();
            Assert.AreEqual(1, swings); Assert.AreEqual(0, hits); Assert.AreEqual(0f, victim.TotalDamage);
        }
    }
    [Test]
    public void ObserverFailure_DoesNotSkipOtherTargets()
    {
        using (var fixture = new HeroFixture())
        {
            var first = fixture.AddVictim(true); var second = fixture.AddVictim(true);
            fixture.Attack.Feedback.HitConfirmed += hit => throw new InvalidOperationException("Basic visual test failure");
            // Chỉ mong đợi log lỗi trình diễn; cả hai mục tiêu vẫn phải nhận damage.
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Basic visual test failure"));
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Basic visual test failure"));
            fixture.Resolve();
            Assert.Greater(first.TotalDamage, 0f); Assert.Greater(second.TotalDamage, 0f);
        }
    }
    [Test]
    public void CancelledWindup_DoesNotPublishSwingOrHit()
    {
        using (var fixture = new HeroFixture())
        {
            fixture.AddVictim(true);
            int cues = 0;
            fixture.Attack.Feedback.SwingResolved += swing => cues++;
            fixture.Attack.Feedback.HitConfirmed += hit => cues++;
            var p = fixture.Origin;
            Assert.IsTrue(DefenseAimResolver.TryResolve(new DefensePoint3(p.x, p.y, p.z),
                new DefensePoint3(p.x, p.y, p.z + 2f), 5f, out var aim));
            Assert.IsTrue(fixture.Attack.QueueAttack(aim));
            fixture.Attack.CancelPendingAttack();
            fixture.Session.Tick(.5f, 100f, false);
            typeof(DefenseHeroAutoAttack3D).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(fixture.Attack, null);
            Assert.AreEqual(0, cues);
        }
    }

    private sealed class HeroFixture : IDisposable
    {
        private readonly List<GameObject> _objects = new List<GameObject>();
        private readonly DefenseAttackTargetingProfileSO _profile;
        public readonly DefenseSession Session = new DefenseSession();
        public readonly DefenseHeroAutoAttack3D Attack;
        public readonly Vector3 Origin = new Vector3(22000, 0, 0);
        public HeroFixture()
        {
            var hero = new GameObject("BasicFeedbackUnitHero"); _objects.Add(hero); hero.transform.position = Origin;
            var health = hero.AddComponent<DefenseHeroHealth3D>(); health.BindGameplay(Session);
            var motor = hero.AddComponent<DefenseHeroMotor3D>(); motor.BindGameplay(Session);
            Attack = hero.AddComponent<DefenseHeroAutoAttack3D>();
            _profile = ScriptableObject.CreateInstance<DefenseAttackTargetingProfileSO>();
            // Không phụ thuộc collider của scene đang mở khi kiểm tra điều kiện chấp nhận damage.
            Set(_profile, "_lineOfSightMask", (LayerMask)0);
            Set(Attack, "_targeting", _profile);
            Attack.BindGameplay(Session);
            Session.Start();
        }
        public Victim AddVictim(bool accepts)
        {
            var target = new GameObject("BasicFeedbackUnitVictim"); _objects.Add(target);
            target.transform.position = Origin + Vector3.forward * 2f;
            var victim = new Victim(target.transform, accepts);
            Session.TargetRegistry.Register(victim);
            return victim;
        }
        public DefenseEnemy3D AddDyingEnemy()
        {
            var target = new GameObject("LethalBasicFeedbackUnitVictim"); _objects.Add(target);
            target.transform.position = Origin + Vector3.forward * 2f;
            var enemy = target.AddComponent<DefenseEnemy3D>();
            enemy.BindGameplay(Session);
            enemy.Initialize(new DefenseEnemyStats("unit", "unit", DefenseEnemyLocomotionType.GroundNavMesh, 1f, 0f, 1f, 0, 0));
            enemy.BindTargets(Session.TargetRegistry);
            enemy.Defeated += () => { target.SetActive(false); target.transform.position = new Vector3(9999, 0, 9999); };
            return enemy;
        }
        public void Resolve()
        {
            var cone = new DefenseConeSnapshot(new DefensePoint3(Origin.x, Origin.y, Origin.z), new DefensePoint3(0, 0, 1), 5f, 120f);
            typeof(DefenseHeroAutoAttack3D).GetMethod("Resolve", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Attack, new object[] { cone });
        }
        private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        public void Dispose()
        {
            foreach (var item in _objects) if (item != null) UnityEngine.Object.DestroyImmediate(item);
            Session.Dispose(); UnityEngine.Object.DestroyImmediate(_profile);
        }
    }
    private sealed class Victim : IDefenseEnemyTarget3D, IDefenseTargetPosition
    {
        private readonly bool _accepts;
        public float TotalDamage;
        public bool MoveOnDamage;
        public Victim(Transform target, bool accepts) { TargetTransform = target; _accepts = accepts; }
        public Transform TargetTransform { get; }
        public bool IsTargetable => true;
        public object Identity => this;
        public bool IsAvailable => true;
        public bool IsAirborne => false;
        public DefensePoint3 Position => new DefensePoint3(TargetTransform.position.x, TargetTransform.position.y, TargetTransform.position.z);
        public float CurrentHP => 1000f - TotalDamage;
        public float MaxHP => 1000f;
        public bool IsDead => false;
        public event Action<float, float> HealthChanged { add { } remove { } }
        public event Action<DefenseDamageContext> Damaged { add { } remove { } }
        public event Action Defeated { add { } remove { } }
        public bool TakeDamage(DefenseDamageContext context)
        {
            if (!_accepts) return false;
            TotalDamage += context.Amount;
            if (MoveOnDamage) TargetTransform.position = new Vector3(9999f, 0, 9999f);
            return true;
        }
    }
}
