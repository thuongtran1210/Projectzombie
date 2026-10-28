using System;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

public sealed class DefenseDashMotorTests
{
    private sealed class MotorFixture : IDisposable
    {
        public readonly GameObject Actor = new GameObject("DashMotorUnitActor");
        public readonly DefenseSession Session = new DefenseSession();
        public readonly DefenseHeroMotor3D Motor;
        public readonly DefenseDashTraversal3D Traversal;
        public MotorFixture()
        {
            Actor.transform.position = new Vector3(22000, 1, 0);
            Motor = Actor.AddComponent<DefenseHeroMotor3D>();
            typeof(DefenseHeroMotor3D).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(Motor, null);
            Traversal = Actor.AddComponent<DefenseDashTraversal3D>();
            Motor.BindGameplay(Session); Session.Start();
        }
        public void Begin(float duration = .2f) => Motor.BeginDash(Actor.transform.position + Vector3.forward * 3f, duration, Traversal);
        public void Dispose() { UnityEngine.Object.DestroyImmediate(Actor); Session.Dispose(); }
    }
    [Test]
    public void InvalidBeginIsSilent_AndRepeatedCancelEndsAcceptedDashOnce()
    {
        using (var f = new MotorFixture())
        {
            int starts = 0, ends = 0;
            f.Motor.DashFeedback.Started += feedback => starts++;
            f.Motor.DashFeedback.Ended += feedback => { ends++; Assert.IsFalse(feedback.Completed); f.Motor.CancelDash(); };
            f.Begin(0f); Assert.AreEqual(0, starts);
            f.Begin(); f.Begin();
            Assert.IsTrue(f.Motor.IsDashing); Assert.AreEqual(1, starts);
            f.Motor.CancelDash(); f.Motor.CancelDash();
            Assert.IsFalse(f.Motor.IsDashing); Assert.AreEqual(1, ends);
        }
    }
    [Test]
    public void VisualObserverFailureDoesNotCancelAcceptedDash()
    {
        using (var f = new MotorFixture())
        {
            f.Motor.DashFeedback.Started += feedback => throw new InvalidOperationException("Dash observer unit failure");
            UnityEngine.TestTools.LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("Dash observer unit failure"));
            f.Begin();
            Assert.IsTrue(f.Motor.IsDashing);
            f.Motor.CancelDash();
            Assert.IsFalse(f.Motor.IsDashing);
        }
    }

    [Test]
    public void CompletionUsesApprovedPoint_EvenBeforeRigidbodyAppliesMove()
    {
        using (var f = new MotorFixture())
        {
            DefenseDashFeedback ended = default;
            f.Motor.DashFeedback.Ended += feedback => ended = feedback;
            f.Begin();
            var approved = f.Actor.transform.position + Vector3.forward * 2f;
            // Kiểm tra hợp đồng kết thúc; không mô phỏng vật lý hoặc chạy Play Mode.
            typeof(DefenseHeroMotor3D).GetMethod("FinishDash", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(f.Motor, new object[] { true, approved });
            Assert.IsTrue(ended.Completed); Assert.IsFalse(f.Motor.IsDashing);
            Assert.AreEqual(approved.z, ended.Position.Z, .001f);
            Assert.AreNotEqual(f.Actor.GetComponent<Rigidbody>().position.z, ended.Position.Z);
        }
    }
    [Test]
    public void FailedTraversalCancelsWithoutCompletedEvent()
    {
        using (var f = new MotorFixture())
        {
            DefenseDashFeedback ended = default;
            int count = 0;
            f.Motor.DashFeedback.Ended += feedback => { ended = feedback; count++; };
            f.Begin(); f.Session.Tick(.1f, 100f, false);
            // Traversal chưa được gán collider/mask nên từ chối trước khi truy vấn Physics/NavMesh.
            typeof(DefenseHeroMotor3D).GetMethod("FixedUpdate", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(f.Motor, null);
            Assert.AreEqual(1, count); Assert.IsFalse(ended.Completed); Assert.IsFalse(f.Motor.IsDashing);
        }
    }
    [Test]
    public void PauseCancelsAndRestartNeverReusesDashId()
    {
        using (var f = new MotorFixture())
        {
            long previous = 0, next = 0; int ends = 0;
            f.Motor.DashFeedback.Started += feedback => { if (previous == 0) previous = feedback.DashId; else next = feedback.DashId; };
            f.Motor.DashFeedback.Ended += feedback => { ends++; Assert.IsFalse(feedback.Completed); };
            f.Begin(); f.Session.Pause(); Assert.AreEqual(1, ends);
            f.Session.Resume(); f.Session.Restart(); f.Begin();
            Assert.Greater(next, previous);
        }
    }
    [Test]
    public void DownedHeroStopsDashHandlesImmediately()
    {
        using (var f = new MotorFixture())
        {
            var health = f.Actor.AddComponent<DefenseHeroHealth3D>();
            typeof(DefenseHeroHealth3D).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(health, null);
            health.BindGameplay(f.Session);
            var anchor = new GameObject("DashUnitGround"); anchor.transform.SetParent(f.Actor.transform, false);
            var presenter = f.Actor.AddComponent<DefenseHeroDashPresenter3D>();
            Set(presenter, "_motor", f.Motor); Set(presenter, "_health", health); Set(presenter, "_groundAnchor", anchor.transform);
            Set(presenter, "_style", AssetDatabase.LoadAssetAtPath<DefenseDashEffectSetSO>("Assets/Features/Defense3D/Effects/Assets/FXSet_HeroDash.asset"));
            var sink = new DefenseDashFeedbackTests.Sink();
            presenter.Bind(f.Session, sink);
            f.Begin(); Assert.AreEqual(2, sink.ActiveCount);
            health.TakeDamage(new DefenseDamageContext(99999f, sessionIdentity: f.Session, runGeneration: f.Session.RunGeneration));
            Assert.IsTrue(health.IsDown); Assert.IsFalse(f.Motor.IsDashing);
            Assert.AreEqual(0, sink.ActiveCount); Assert.AreEqual(2, sink.Requests.Count);
            presenter.Unbind();
        }
    }
    private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
}
