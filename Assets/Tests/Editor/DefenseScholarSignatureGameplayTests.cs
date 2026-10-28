using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using ProjectZombie.Features.Defense3D;

public sealed class DefenseScholarSignatureGameplayTests
{
    [Test]
    public void WindupDelaysDamage_ThenResolvesAllAcceptedTargetsAndRefundsOnce()
    {
        using (var f = new Fixture())
        {
            var front = f.AddVictim(true, Vector3.forward * 3); var back = f.AddVictim(false, Vector3.back * 3);
            int starts = 0, releases = 0, hits = 0, completed = 0;
            f.Skill.Feedback.Started += cue => starts++;
            f.Skill.Feedback.Released += cue => releases++;
            f.Skill.Feedback.HitConfirmed += cue => hits++;
            f.Skill.Feedback.Ended += cue => { if (cue.Completed) completed++; };
            Assert.IsTrue(f.Relic.Ability.TryActivate(12f, true));
            Assert.IsTrue(f.Cast());
            Assert.AreEqual(1, starts); Assert.AreEqual(0f, front.Damage); Assert.AreEqual(0, releases);
            f.Advance(.2f); Assert.AreEqual(0f, front.Damage);
            f.Advance(.2f);
            Assert.AreEqual(52.5f, front.Damage, .001f); Assert.AreEqual(0f, back.Damage);
            Assert.AreEqual(1, releases); Assert.AreEqual(1, hits); Assert.AreEqual(1, completed);
            float expected = (12f - .4f) * .8f;
            Assert.AreEqual(expected, f.Relic.Ability.RemainingCooldown, .001f);
            f.Update(); f.Update();
            Assert.AreEqual(expected, f.Relic.Ability.RemainingCooldown, .001f);
            Assert.Greater(f.Runtime.Ability.RemainingCooldown, 29f);
        }
    }
    [Test]
    public void EmptyAoEStillRefundsRelic_AndSignatureDamageUsesCurrentAttackModifier()
    {
        using (var f = new Fixture())
        {
            f.Relic.Ability.TryActivate(12f, true); Assert.IsTrue(f.Cast()); f.Advance(.4f);
            Assert.AreEqual((12f - .4f) * .8f, f.Relic.Ability.RemainingCooldown, .001f);
        }
        using (var f = new Fixture())
        {
            f.Session.Modifiers.TryApplyCard(new DefenseUpgradeCard("unit", "unit", "unit", DefenseUpgradeType.HeroAttackDamage, .5f));
            var victim = f.AddVictim(true, Vector3.forward);
            Assert.IsTrue(f.Cast()); f.Advance(.4f);
            Assert.AreEqual(52.5f * 1.5f, victim.Damage, .001f);
        }
    }
    [Test]
    public void CancelledWindupDoesNotDamageOrRefund_AndEndsExactlyOnce()
    {
        using (var f = new Fixture())
        {
            var victim = f.AddVictim(true, Vector3.forward); int ends = 0;
            f.Skill.Feedback.Ended += cue => { ends++; Assert.IsFalse(cue.Completed); f.Skill.CancelCasting(); };
            f.Relic.Ability.TryActivate(12f, true); Assert.IsTrue(f.Cast());
            f.Skill.CancelCasting(); f.Skill.CancelCasting(); f.Advance(.4f);
            Assert.AreEqual(0f, victim.Damage); Assert.AreEqual(1, ends); Assert.IsFalse(f.Skill.IsCasting);
            Assert.AreEqual(11.6f, f.Relic.Ability.RemainingCooldown, .001f);
            Assert.Greater(f.Runtime.Ability.RemainingCooldown, 0f);
        }
    }
    [TestCase("dash")]
    [TestCase("down")]
    [TestCase("pause")]
    [TestCase("restart")]
    public void InterruptionPreventsPendingRelease(string reason)
    {
        using (var f = new Fixture())
        {
            var victim = f.AddVictim(true, Vector3.forward); int released = 0;
            f.Skill.Feedback.Released += cue => released++;
            Assert.IsTrue(f.Cast());
            if (reason == "dash") f.Motor.BeginDash(f.Actor.transform.position + Vector3.forward, .2f, f.Actor.AddComponent<DefenseDashTraversal3D>());
            if (reason == "down") f.Health.TakeDamage(new DefenseDamageContext(99999f, sessionIdentity: f.Session, runGeneration: f.Session.RunGeneration));
            if (reason == "pause") { f.Session.Pause(); f.Session.Resume(); }
            if (reason == "restart") f.Session.Restart();
            f.Advance(.4f);
            Assert.AreEqual(0, released); Assert.AreEqual(0f, victim.Damage); Assert.IsFalse(f.Skill.IsCasting);
        }
    }
    [Test]
    public void CastCenterFollowsActor_AndHitPointIsCapturedBeforeTargetDespawn()
    {
        using (var f = new Fixture())
        {
            var victim = f.AddVictim(true, Vector3.right * 7f);
            victim.OnDamage = () => victim.TargetTransform.position = new Vector3(9999f, 0, 9999f);
            DefenseSignatureFeedback release = default; DefenseSignatureHitFeedback hit = default;
            f.Skill.Feedback.Released += cue => release = cue; f.Skill.Feedback.HitConfirmed += cue => hit = cue;
            Assert.IsTrue(f.Cast()); f.Actor.transform.position += Vector3.right * 3f;
            f.Advance(.4f);
            Assert.AreEqual(f.Origin.x + 3f, release.Position.X, .001f);
            Assert.AreEqual(f.Origin.x + 7f, hit.Hit.Position.X, .001f);
            Assert.AreEqual(52.5f, victim.Damage, .001f);
        }
    }
    [Test]
    public void VisualObserverFailureDoesNotBlockDamageOrRelicRefund()
    {
        using (var f = new Fixture())
        {
            var first = f.AddVictim(true, Vector3.forward); var second = f.AddVictim(true, Vector3.back);
            f.Skill.Feedback.Released += cue => throw new InvalidOperationException("Scholar visual unit failure");
            f.Relic.Ability.TryActivate(12f, true); Assert.IsTrue(f.Cast());
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Scholar visual unit failure"));
            f.Advance(.4f);
            Assert.AreEqual(52.5f, first.Damage, .001f); Assert.AreEqual(52.5f, second.Damage, .001f);
            Assert.AreEqual(9.28f, f.Relic.Ability.RemainingCooldown, .001f);
        }
    }
    [Test]
    public void RestartInsideDamageCannotHitOtherTargetsOrRefundNewRun()
    {
        using (var f = new Fixture())
        {
            var first = f.AddVictim(true, Vector3.forward); var second = f.AddVictim(true, Vector3.back);
            first.OnDamage = () => { f.Session.Restart(); f.Relic.Ability.TryActivate(12f, true); };
            Assert.IsTrue(f.Cast()); f.Advance(.4f);
            Assert.Greater(first.Damage, 0f); Assert.AreEqual(0f, second.Damage);
            Assert.AreEqual(12f, f.Relic.Ability.RemainingCooldown, .001f);
        }
    }
    [Test]
    public void DamageExceptionClosesCastAndAllowsNewCastAfterReset()
    {
        using (var f = new Fixture())
        {
            var victim = f.AddVictim(true, Vector3.forward);
            victim.OnDamage = () => throw new InvalidOperationException("Target damage unit failure");
            bool ended = false;
            f.Skill.Feedback.Ended += cue => { ended = true; Assert.IsFalse(cue.Completed); };
            Assert.IsTrue(f.Cast());
            var error = Assert.Throws<TargetInvocationException>(() => f.Advance(.4f));
            Assert.IsInstanceOf<InvalidOperationException>(error.InnerException);
            Assert.IsTrue(ended); Assert.IsFalse(f.Skill.IsCasting);
            f.Session.Restart(); Assert.IsTrue(f.Cast());
        }
    }

    [Test]
    public void ForeignRelicAbilityIsNotRefunded()
    {
        using (var other = new DefenseSession()) using (var f = new Fixture())
        {
            other.Start(); f.Relic.Bind(other); f.Relic.Ability.TryActivate(12f, true);
            Assert.IsTrue(f.Cast()); f.Advance(.4f);
            Assert.AreEqual(12f, f.Relic.Ability.RemainingCooldown, .001f);
        }
    }
    [Test]
    public void CastIdIncreasesAcrossRestart_AndRejectedCastEmitsNothing()
    {
        using (var f = new Fixture())
        {
            long first = 0, next = 0; int count = 0;
            f.Skill.Feedback.Started += cue => { count++; if (first == 0) first = cue.CastId; else next = cue.CastId; };
            Assert.IsTrue(f.Cast()); Assert.IsFalse(f.Cast()); Assert.AreEqual(1, count);
            f.Session.Restart(); Assert.IsTrue(f.Cast()); Assert.Greater(next, first);
        }
    }
    private sealed class Fixture : IDisposable
    {
        private readonly List<GameObject> _objects = new List<GameObject>();
        public readonly DefenseSession Session = new DefenseSession();
        public readonly GameObject Actor = new GameObject("ScholarSignatureUnitHero");
        public readonly Vector3 Origin = new Vector3(22000f, 1f, 0);
        public readonly DefenseHeroHealth3D Health;
        public readonly DefenseHeroMotor3D Motor;
        public readonly DefenseScholarSignatureEffect3D Skill;
        public readonly DefenseTestAbilitySource Relic;
        public readonly DefenseSkillCastRuntime Runtime;
        public Fixture()
        {
            Actor.transform.position = Origin; _objects.Add(Actor);
            Motor = Actor.AddComponent<DefenseHeroMotor3D>();
            var attack = Actor.AddComponent<DefenseHeroAutoAttack3D>();
            Health = Actor.AddComponent<DefenseHeroHealth3D>();
            Relic = Actor.AddComponent<DefenseTestAbilitySource>(); Relic.Bind(Session);
            Skill = Actor.AddComponent<DefenseScholarSignatureEffect3D>();
            Set(Skill, "_health", Health); Set(Skill, "_attack", attack); Set(Skill, "_motor", Motor); Set(Skill, "_relic", Relic);
            typeof(DefenseHeroMotor3D).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Motor, null);
            typeof(DefenseHeroHealth3D).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Health, null);
            Health.BindGameplay(Session); Motor.BindGameplay(Session); attack.BindGameplay(Session); Skill.BindGameplay(Session);
            var definition = new DefenseSkillDefinition(DefenseSkillKind.ScholarSignature, 6f, 6f, 52.5f, 30f, .38f, false, DefenseTargetMask.All);
            Skill.Configure(definition); Runtime = new DefenseSkillCastRuntime(Session, definition); Session.Start();
        }
        public bool Cast()
        {
            var p = Actor.transform.position; var origin = new DefensePoint3(p.x, p.y, p.z);
            DefenseAimResolver.TryResolve(origin, origin, 6f, out var aim);
            return Runtime.TryCast(new DefenseSkillCastRequest(aim, Session, Session.RunGeneration), origin, true, Skill);
        }
        public void Advance(float seconds) { Session.Tick(seconds, 100f, false); Update(); }
        public void Update() => typeof(DefenseScholarSignatureEffect3D).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Skill, null);
        public Victim AddVictim(bool accepts, Vector3 offset)
        {
            var go = new GameObject("ScholarSignatureUnitVictim"); _objects.Add(go); go.transform.position = Origin + offset;
            var victim = new Victim(go.transform, accepts); Session.TargetRegistry.Register(victim); return victim;
        }
        private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        public void Dispose()
        {
            Runtime.Dispose();
            foreach (var item in _objects) if (item != null) UnityEngine.Object.DestroyImmediate(item);
            Session.Dispose();
        }
    }
    private sealed class Victim : IDefenseEnemyTarget3D, IDefenseTargetPosition
    {
        private readonly bool _accepts;
        public float Damage;
        public Action OnDamage;
        public Victim(Transform target, bool accepts) { TargetTransform = target; _accepts = accepts; }
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
        { if (!_accepts) return false; Damage += context.Amount; OnDamage?.Invoke(); return true; }
    }
}
