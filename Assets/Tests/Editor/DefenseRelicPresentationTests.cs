using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ProjectZombie.Features.Defense3D;

public sealed class DefenseRelicPresentationTests
{
    [Test]
    public void AcceptedCast_StillDamagesAndCommitsCooldownWhenVisualObserverThrows()
    {
        var hero = new GameObject("RelicUnitTestHero");
        var victimObject = new GameObject("RelicUnitTestTarget");
        using (var session = new DefenseSession())
        {
            try
            {
                var health = hero.AddComponent<DefenseHeroHealth3D>();
                health.BindGameplay(session);
                var effect = hero.AddComponent<DefenseRelicSkillEffect3D>();
                typeof(DefenseRelicSkillEffect3D).GetField("_health", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(effect, health);
                typeof(DefenseRelicSkillEffect3D).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(effect, null);
                effect.BindGameplay(session);
                var definition = new DefenseSkillDefinition(DefenseSkillKind.DepToOng, 10f, 3.5f, 180f, 12f, .5f, false, DefenseTargetMask.Ground);
                effect.Configure(definition);
                var victim = new Victim(victimObject.transform);
                session.TargetRegistry.Register(victim);
                session.Start();
                int notifications = 0;
                effect.ImpactOccurred += impact => throw new InvalidOperationException("Visual adapter test failure");
                effect.ImpactOccurred += impact => notifications++;
                Assert.IsTrue(DefenseAimResolver.TryResolve(default, default, 10f, out var aim));
                using (var runtime = new DefenseSkillCastRuntime(session, definition))
                {
                    // Lỗi hình ảnh được ghi log nhưng không được chặn damage hoặc callback hình ảnh còn lại.
                    LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Visual adapter test failure"));
                    Assert.IsTrue(runtime.TryCast(new DefenseSkillCastRequest(aim, session, session.RunGeneration), default, true, effect));
                    Assert.AreEqual(180f, victim.TotalDamage);
                    Assert.AreEqual(1, notifications);
                    Assert.Greater(runtime.Ability.RemainingCooldown, 0f);
                    Assert.IsFalse(runtime.TryCast(new DefenseSkillCastRequest(aim, session, session.RunGeneration), default, true, effect));
                    Assert.AreEqual(1, notifications);
                }
            }
            finally { UnityEngine.Object.DestroyImmediate(hero); UnityEngine.Object.DestroyImmediate(victimObject); }
        }
    }
    private sealed class Victim : IDefenseEnemyTarget3D, IDefenseTargetPosition
    {
        public float TotalDamage;
        public Victim(Transform target) { TargetTransform = target; }
        public Transform TargetTransform { get; }
        public bool IsTargetable => true;
        public object Identity => this;
        public bool IsAvailable => true;
        public bool IsAirborne => false;
        public DefensePoint3 Position => default;
        public float CurrentHP => 1000f - TotalDamage;
        public float MaxHP => 1000f;
        public bool IsDead => false;
        public event Action<float, float> HealthChanged { add { } remove { } }
        public event Action<DefenseDamageContext> Damaged { add { } remove { } }
        public event Action Defeated { add { } remove { } }
        public bool TakeDamage(DefenseDamageContext context) { TotalDamage += context.Amount; return true; }
    }
}
