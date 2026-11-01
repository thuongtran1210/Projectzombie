using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

public sealed class DefenseLightningSignatureGameplayTests
{
    [Test]
    public void WindupDelaysDamage_ThenResolvesHit_AndEmitsAllFeedbackEvents()
    {
        using (var f = new Fixture())
        {
            var victim = f.AddVictim(true, Vector3.forward * 3f);
            int starts = 0, releases = 0, hits = 0, completed = 0;

            f.Skill.Feedback.Started += cue => starts++;
            f.Skill.Feedback.Released += cue => releases++;
            f.Skill.Feedback.HitConfirmed += cue => hits++;
            f.Skill.Feedback.Ended += cue => { if (cue.Completed) completed++; };

            Assert.IsTrue(f.Cast());
            Assert.AreEqual(1, starts);
            Assert.AreEqual(0f, victim.Damage);
            Assert.AreEqual(0, releases);
            Assert.IsTrue(f.Skill.IsCasting);

            // Tiến triển 0.1s (chưa hết 0.25s windup)
            f.Advance(0.1f);
            Assert.AreEqual(0f, victim.Damage);
            Assert.AreEqual(0, releases);

            // Tiến triển thêm 0.2s (đã vượt 0.25s windup)
            f.Advance(0.2f);
            Assert.AreEqual(95f, victim.Damage, 0.001f);
            Assert.AreEqual(1, releases);
            Assert.AreEqual(1, hits);
            Assert.AreEqual(1, completed);
            Assert.IsFalse(f.Skill.IsCasting);

            // Kiểm tra trạng thái khống chế Stun & Slow
            Assert.IsTrue(victim.HasStatus(DefenseStatusKind.Stun));
            Assert.IsTrue(victim.HasStatus(DefenseStatusKind.Slow));
        }
    }

    [Test]
    public void DamageScalesWithHeroAttackDamage()
    {
        using (var f = new Fixture())
        {
            // Thiết lập Attack Modifier tăng 50% damage
            f.Session.Modifiers.TryApplyCard(new DefenseUpgradeCard("unit", "unit", "unit", DefenseUpgradeType.HeroAttackDamage, 0.5f));

            var victim = f.AddVictim(true, Vector3.forward * 2f);
            Assert.IsTrue(f.Cast());

            f.Advance(0.3f);

            // 95 base damage * 1.5 = 142.5f
            Assert.AreEqual(95f * 1.5f, victim.Damage, 0.001f);
        }
    }

    [Test]
    public void HeroDownDuringWindup_CancelsCasting_AndDoesNoDamage()
    {
        using (var f = new Fixture())
        {
            var victim = f.AddVictim(true, Vector3.forward * 2f);
            int ended = 0;
            f.Skill.Feedback.Ended += cue =>
            {
                ended++;
                Assert.IsFalse(cue.Completed);
            };

            Assert.IsTrue(f.Cast());
            Assert.IsTrue(f.Skill.IsCasting);

            // Giả lập Tướng bị hạ gục
            f.Health.TakeDamage(new DefenseDamageContext(99999f, sessionIdentity: f.Session, runGeneration: f.Session.RunGeneration));
            Assert.IsTrue(f.Health.IsDown);

            f.Advance(0.3f);

            Assert.AreEqual(1, ended);
            Assert.IsFalse(f.Skill.IsCasting);
            Assert.AreEqual(0f, victim.Damage);
        }
    }

    [Test]
    public void SessionRestart_CancelsCastingCleanly()
    {
        using (var f = new Fixture())
        {
            var victim = f.AddVictim(true, Vector3.forward * 2f);
            Assert.IsTrue(f.Cast());
            Assert.IsTrue(f.Skill.IsCasting);

            // Restart session
            f.Session.Restart();
            f.Update();

            Assert.IsFalse(f.Skill.IsCasting);
            f.Advance(0.5f);
            Assert.AreEqual(0f, victim.Damage);
        }
    }

    private sealed class Fixture : IDisposable
    {
        private readonly List<GameObject> _objects = new List<GameObject>();
        public readonly DefenseSession Session = new DefenseSession();
        public readonly GameObject Actor = new GameObject("DaoSiLightningUnitHero");
        public readonly Vector3 Origin = new Vector3(45000f, 1f, 0);
        public readonly DefenseHeroHealth3D Health;
        public readonly DefenseHeroMotor3D Motor;
        public readonly DefenseHeroAutoAttack3D Attack;
        public readonly DefenseLightningSignatureEffect3D Skill;
        public readonly DefenseSkillCastRuntime Runtime;

        public Fixture()
        {
            Actor.transform.position = Origin;
            _objects.Add(Actor);

            Motor = Actor.AddComponent<DefenseHeroMotor3D>();
            Attack = Actor.AddComponent<DefenseHeroAutoAttack3D>();
            Health = Actor.AddComponent<DefenseHeroHealth3D>();
            Skill = Actor.AddComponent<DefenseLightningSignatureEffect3D>();

            Set(Skill, "_health", Health);
            Set(Skill, "_attack", Attack);
            Set(Skill, "_motor", Motor);

            typeof(DefenseHeroMotor3D).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Motor, null);
            typeof(DefenseHeroHealth3D).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Health, null);

            Health.BindGameplay(Session);
            Motor.BindGameplay(Session);
            Attack.BindGameplay(Session);
            Skill.BindGameplay(Session);

            var definition = new DefenseSkillDefinition(
                DefenseSkillKind.Signature,
                range: 10f,
                radius: 4.5f,
                damage: 95f,
                cooldown: 18f,
                duration: 0.25f,
                autoTargetOnTap: true,
                targetMask: DefenseTargetMask.All);

            Skill.Configure(definition);
            Skill.StunDuration = 1.5f;
            Skill.ParalysisSlowStrength = 0.4f;

            Runtime = new DefenseSkillCastRuntime(Session, definition);
            Session.Start();
        }

        public bool Cast()
        {
            var p = Actor.transform.position;
            var origin = new DefensePoint3(p.x, p.y, p.z);
            DefenseAimResolver.TryResolve(origin, origin, 10f, out var aim);
            return Runtime.TryCast(new DefenseSkillCastRequest(aim, Session, Session.RunGeneration), origin, true, Skill);
        }

        public void Advance(float seconds)
        {
            Session.Tick(seconds, 100f, false);
            Update();
        }

        public void Update() => typeof(DefenseLightningSignatureEffect3D)
            .GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(Skill, null);

        public TestVictim AddVictim(bool accepts, Vector3 offset)
        {
            var go = new GameObject("DaoSiLightningUnitVictim");
            _objects.Add(go);
            go.transform.position = Origin + offset;
            var victim = new TestVictim(go.transform, accepts);
            Session.TargetRegistry.Register(victim);
            return victim;
        }

        private static void Set(object target, string field, object value) =>
            target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);

        public void Dispose()
        {
            Runtime.Dispose();
            foreach (var item in _objects)
            {
                if (item != null) UnityEngine.Object.DestroyImmediate(item);
            }
            Session.Dispose();
        }
    }

    private sealed class TestVictim : IDefenseEnemyTarget3D, IDefenseTargetPosition, IDefenseStatusReceiver
    {
        private readonly bool _accepts;
        public float Damage;
        public readonly List<DefenseStatusRequest> Statuses = new List<DefenseStatusRequest>();

        public TestVictim(Transform target, bool accepts)
        {
            TargetTransform = target;
            _accepts = accepts;
        }

        public Transform TargetTransform { get; }
        public bool IsTargetable => true;
        public object Identity => this;
        public bool IsAvailable => true;
        public bool IsAirborne => false;
        public DefensePoint3 Position => new DefensePoint3(TargetTransform.position.x, TargetTransform.position.y, TargetTransform.position.z);
        public float CurrentHP => 1000f - Damage;
        public float MaxHP => 1000f;
        public bool IsDead => false;

        public event Action<float, float> HealthChanged { add { } remove { } }
        public event Action<DefenseDamageContext> Damaged { add { } remove { } }
        public event Action Defeated { add { } remove { } }

        public bool TakeDamage(DefenseDamageContext context)
        {
            if (!_accepts) return false;
            Damage += context.Amount;
            return true;
        }

        public bool TryApplyStatus(DefenseStatusRequest request)
        {
            Statuses.Add(request);
            return true;
        }

        public bool HasStatus(DefenseStatusKind kind)
        {
            for (int i = 0; i < Statuses.Count; i++)
            {
                if (Statuses[i].Kind == kind) return true;
            }
            return false;
        }
    }
}

