using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

public sealed class DefensePipeRelicEffect3DTests
{
    [Test]
    public void TryPrepare_ReturnsTrue_WhenConfiguredAndGameplayActive()
    {
        using (var f = new Fixture())
        {
            var origin = new DefensePoint3(f.Origin.x, f.Origin.y, f.Origin.z);
            DefenseAimResolver.TryResolve(origin, origin, 9f, out var aim);

            bool prepared = f.Skill.TryPrepare(aim, out var resolved);
            Assert.IsTrue(prepared);
            Assert.AreEqual(aim.Target, resolved.Target);
        }
    }

    [Test]
    public void Execute_TriggersInitialBurstDamage_AndAppliesStunAndSlow()
    {
        using (var f = new Fixture())
        {
            var victim = f.AddVictim(true, Vector3.forward * 2f);

            Assert.IsTrue(f.Cast());

            // Definition.Damage = 60, InitialBurstRatio = 0.5 => Burst Damage = 30
            Assert.AreEqual(30f, victim.Damage, 0.001f);
            Assert.AreEqual(2, victim.Statuses.Count);
            // Sặc ho (Stun) đợt đầu
            Assert.AreEqual(DefenseStatusKind.Stun, victim.Statuses[0].Kind);
            Assert.AreEqual(f.Skill.CoughStunDuration, victim.Statuses[0].Duration, 0.001f);
            Assert.AreEqual(1, f.Skill.ActiveZoneCount);
        }
    }

    [Test]
    public void Update_TickingSmokeZone_InflictsDamageOverTime_AndRefreshesSlow()
    {
        using (var f = new Fixture())
        {
            var victim = f.AddVictim(true, Vector3.forward * 2f);

            Assert.IsTrue(f.Cast());
            Assert.AreEqual(30f, victim.Damage, 0.001f);

            // Tiến triển 0.5s -> Kích hoạt tick DoT đầu tiên (Definition.Damage * 0.15 = 9 damage)
            f.Advance(0.55f);

            Assert.AreEqual(39f, victim.Damage, 0.001f);
            Assert.IsTrue(victim.HasStatus(DefenseStatusKind.Slow));

            // Tiến triển thêm 0.5s -> Kích hoạt tick DoT thứ hai (thêm 9 damage = 48)
            f.Advance(0.5f);
            Assert.AreEqual(48f, victim.Damage, 0.001f);
        }
    }

    [Test]
    public void VictimEnteringZoneAfterCast_TakesDoTAndSlow()
    {
        using (var f = new Fixture())
        {
            // Cast khi chưa có victim
            Assert.IsTrue(f.Cast());
            Assert.AreEqual(1, f.Skill.ActiveZoneCount);

            // Cho quái chạy vào tâm khói sau khi chiêu đã nổ 0.2s
            f.Advance(0.2f);
            var lateVictim = f.AddVictim(true, Vector3.forward * 1.5f);
            Assert.AreEqual(0f, lateVictim.Damage);

            // Sau 0.5s (tổng 0.7s), tick DoT xảy ra
            f.Advance(0.35f);
            Assert.AreEqual(9f, lateVictim.Damage, 0.001f);
            Assert.IsTrue(lateVictim.HasStatus(DefenseStatusKind.Slow));
        }
    }

    [Test]
    public void SmokeZone_ExpiresAfterDuration_AndCleansUp()
    {
        using (var f = new Fixture())
        {
            var victim = f.AddVictim(true, Vector3.forward * 2f);
            Assert.IsTrue(f.Cast());
            Assert.AreEqual(1, f.Skill.ActiveZoneCount);

            // Tiến triển vượt quá thời gian tồn tại của khói (Duration = 4.5s)
            f.Advance(5.0f);

            Assert.AreEqual(0, f.Skill.ActiveZoneCount);
            float damageAtExpiry = victim.Damage;

            // Tiến triển tiếp -> không còn gây thêm sát thương
            f.Advance(1.0f);
            Assert.AreEqual(damageAtExpiry, victim.Damage);
        }
    }

    [Test]
    public void SessionRestart_CancelsActiveSmokeZones()
    {
        using (var f = new Fixture())
        {
            var victim = f.AddVictim(true, Vector3.forward * 2f);
            Assert.IsTrue(f.Cast());
            Assert.AreEqual(1, f.Skill.ActiveZoneCount);

            // Restart session
            f.Session.Restart();
            f.Update();

            Assert.AreEqual(0, f.Skill.ActiveZoneCount);
            float damageBefore = victim.Damage;

            f.Advance(1.0f);
            Assert.AreEqual(damageBefore, victim.Damage);
        }
    }

    [Test]
    public void PausePreservesSmokeAndRemainingLifetime()
    {
        using (var f = new Fixture())
        {
            Assert.IsTrue(f.Cast());
            f.Advance(1f);
            float remaining = f.Skill.RemainingSmokeSeconds;
            Assert.IsTrue(f.Session.Pause());
            f.Advance(2f);
            Assert.AreEqual(1, f.Skill.ActiveZoneCount);
            Assert.AreEqual(remaining, f.Skill.RemainingSmokeSeconds, .001f);
            Assert.IsTrue(f.Session.Resume());
            f.Advance(.5f);
            Assert.AreEqual(remaining - .5f, f.Skill.RemainingSmokeSeconds, .001f);
        }
    }

    [Test]
    public void DisableClearsSmokeAndHudLifetime()
    {
        using (var f = new Fixture())
        {
            Assert.IsTrue(f.Cast());
            f.Actor.SetActive(false);
            // EditMode fixtures do not dispatch runtime MonoBehaviour lifecycle callbacks.
            typeof(DefensePipeRelicEffect3D).GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(f.Skill, null);
            Assert.AreEqual(0, f.Skill.ActiveZoneCount);
            Assert.AreEqual(0f, f.Skill.RemainingSmokeSeconds);
        }
    }

    private sealed class Fixture : IDisposable
    {
        private readonly List<GameObject> _objects = new List<GameObject>();
        public readonly DefenseSession Session = new DefenseSession();
        public readonly GameObject Actor = new GameObject("DieuCayUnitHero");
        public readonly Vector3 Origin = new Vector3(31000f, 1f, 0);
        public readonly DefensePipeRelicEffect3D Skill;
        public readonly DefenseSkillCastRuntime Runtime;

        public Fixture()
        {
            Actor.transform.position = Origin;
            _objects.Add(Actor);

            Skill = Actor.AddComponent<DefensePipeRelicEffect3D>();
            Skill.BindGameplay(Session);

            var definition = new DefenseSkillDefinition(
                DefenseSkillKind.Relic,
                range: 9f,
                radius: 4f,
                damage: 60f,
                cooldown: 16f,
                duration: 4.5f,
                autoTargetOnTap: true,
                targetMask: DefenseTargetMask.All);

            Skill.Configure(definition);
            Skill.InitialBurstRatio = 0.5f; // 30 damage
            Skill.TickDamageRatio = 0.15f;   // 9 damage per tick
            Skill.TickInterval = 0.5f;
            Skill.CoughStunDuration = 0.6f;
            Skill.SlowStrength = 0.6f;

            Runtime = new DefenseSkillCastRuntime(Session, definition);
            Session.Start();
        }

        public bool Cast()
        {
            var p = Actor.transform.position;
            var origin = new DefensePoint3(p.x, p.y, p.z);
            DefenseAimResolver.TryResolve(origin, origin, 9f, out var aim);
            return Runtime.TryCast(new DefenseSkillCastRequest(aim, Session, Session.RunGeneration), origin, true, Skill);
        }

        public void Advance(float seconds)
        {
            Session.Tick(seconds, 100f, false);
            Update();
        }

        public void Update() => typeof(DefensePipeRelicEffect3D)
            .GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(Skill, null);

        public TestVictim AddVictim(bool accepts, Vector3 offset)
        {
            var go = new GameObject("DieuCayUnitVictim");
            _objects.Add(go);
            go.transform.position = Origin + offset;
            var victim = new TestVictim(go.transform, accepts);
            Session.TargetRegistry.Register(victim);
            return victim;
        }

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

