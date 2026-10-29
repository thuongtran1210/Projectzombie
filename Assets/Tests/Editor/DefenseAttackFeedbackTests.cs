using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

public sealed class DefenseAttackFeedbackTests
{
    private const string Folder = "Assets/Features/Defense3D/Effects/Assets/";
    private static DefenseBasicAttackEffectSetSO Style => AssetDatabase.LoadAssetAtPath<DefenseBasicAttackEffectSetSO>(Folder + "FXSet_HeroBasicAttack.asset");
    private static DefenseConeSnapshot Cone => new DefenseConeSnapshot(default, new DefensePoint3(0, 0, 1), 5f, 120f);

    [Test]
    public void Events_ContinueAfterThrowingObserverAndThrowingLogger()
    {
        int calls = 0;
        var events = new DefenseAttackFeedbackEvents(error => throw new Exception("Logger failure"));
        events.HitConfirmed += hit => throw new Exception("Visual failure");
        events.HitConfirmed += hit => calls++;
        Assert.DoesNotThrow(() => events.PublishHit(default));
        Assert.AreEqual(1, calls);
    }
    [Test]
    public void Events_KeepStableDispatchWhenSubscriptionsChange()
    {
        var events = new DefenseAttackFeedbackEvents();
        int laterCalls = 0;
        Action<DefenseSwingFeedback> later = swing => laterCalls++;
        events.SwingResolved += swing => events.SwingResolved -= later;
        events.SwingResolved += later;
        events.PublishSwing(default); events.PublishSwing(default);
        Assert.AreEqual(1, laterCalls);
    }
    [Test]
    public void Adapter_MapsSwingAndHitToDifferentProfilesWithoutFollowingVictim()
    {
        using (var session = new DefenseSession())
        {
            session.Start();
            var events = new DefenseAttackFeedbackEvents(); var sink = new Sink();
            using (var adapter = new DefenseBasicAttackFeedbackAdapter(events, session, sink, Style))
            {
                events.PublishSwing(new DefenseSwingFeedback(Cone, session, session.RunGeneration));
                var point = new DefensePoint3(2, .9f, 3);
                events.PublishHit(new DefenseHitFeedback(point, Cone.Forward, 35, session, session.RunGeneration));
                Assert.AreEqual(2, sink.Requests.Count);
                Assert.AreSame(Style.Swing, sink.Requests[0].Profile);
                Assert.AreEqual(5f, sink.Requests[0].Radius);
                Assert.AreEqual(Style.SwingHeight, sink.Requests[0].Position.y, .001f);
                Assert.AreSame(Style.Hit, sink.Requests[1].Profile);
                Assert.AreEqual(new Vector3(2, .9f, 3), sink.Requests[1].Position);
                Assert.IsNull(sink.Requests[1].Anchor);
            }
        }
    }
    [Test]
    public void Adapter_FiltersPauseForeignSessionOldRunAndRetiredActor()
    {
        using (var session = new DefenseSession())
        {
            session.Start();
            var events = new DefenseAttackFeedbackEvents(); var sink = new Sink();
            bool actorCurrent = true;
            using (var adapter = new DefenseBasicAttackFeedbackAdapter(events, session, sink, Style, isCurrent: () => actorCurrent))
            {
                var old = new DefenseSwingFeedback(Cone, session, session.RunGeneration);
                events.PublishSwing(new DefenseSwingFeedback(Cone, new object(), session.RunGeneration));
                session.Pause(); events.PublishSwing(old); session.Resume();
                actorCurrent = false; events.PublishSwing(old); actorCurrent = true;
                session.Restart(); events.PublishSwing(old);
                Assert.IsEmpty(sink.Requests);
                events.PublishSwing(new DefenseSwingFeedback(Cone, session, session.RunGeneration));
                Assert.AreEqual(1, sink.Requests.Count);
            }
        }
    }
    [Test]
    public void Adapter_DisposePreventsDuplicateEffectsAfterRebind()
    {
        using (var session = new DefenseSession())
        {
            session.Start();
            var events = new DefenseAttackFeedbackEvents(); var sink = new Sink();
            var previous = new DefenseBasicAttackFeedbackAdapter(events, session, sink, Style);
            previous.Dispose(); previous.Dispose();
            using (var current = new DefenseBasicAttackFeedbackAdapter(events, session, sink, Style))
                events.PublishSwing(new DefenseSwingFeedback(Cone, session, session.RunGeneration));
            events.PublishSwing(new DefenseSwingFeedback(Cone, session, session.RunGeneration));
            Assert.AreEqual(1, sink.Requests.Count);
        }
    }
    [Test]
    public void Adapter_DropsInvalidGeometryAndNonpositiveDamage()
    {
        using (var session = new DefenseSession())
        {
            session.Start();
            var events = new DefenseAttackFeedbackEvents(); var sink = new Sink();
            using (var adapter = new DefenseBasicAttackFeedbackAdapter(events, session, sink, Style))
            {
                events.PublishSwing(new DefenseSwingFeedback(new DefenseConeSnapshot(default, default, 5, 120), session, session.RunGeneration));
                events.PublishHit(new DefenseHitFeedback(default, default, 0, session, session.RunGeneration));
                events.PublishHit(new DefenseHitFeedback(new DefensePoint3(float.NaN, 0, 0), default, 1, session, session.RunGeneration));
                Assert.IsEmpty(sink.Requests);
            }
        }
    }
    [Test]
    public void VisualModules_AreConfiguredAndResetWithoutLeakingRendererState()
    {
        foreach (string name in new[] { "BasicSlash", "BasicHit" })
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "VFX_" + name + "_VongXuyen.prefab");
            var copy = UnityEngine.Object.Instantiate(prefab);
            try
            {
                var instance = copy.GetComponent<DefenseEffectInstance3D>();
                var renderer = copy.GetComponentInChildren<MeshRenderer>();
                var originalRotation = renderer.transform.localRotation;
                instance.Prepare(Color.yellow, 2f); instance.Render(.75f);
                Assert.IsTrue(renderer.enabled);
                instance.ResetPresentation();
                Assert.IsFalse(renderer.enabled); Assert.IsFalse(copy.activeSelf);
                Assert.Less(Quaternion.Angle(originalRotation, renderer.transform.localRotation), .001f);
                var block = new MaterialPropertyBlock(); renderer.GetPropertyBlock(block);
                Assert.IsTrue(block.isEmpty);
            }
            finally { UnityEngine.Object.DestroyImmediate(copy); }
        }
    }
    private sealed class Sink : IDefenseEffectService
    {
        public readonly List<DefenseEffectRequest> Requests = new List<DefenseEffectRequest>();
        public DefenseEffectHandle Play(in DefenseEffectRequest request) { Requests.Add(request); return default; }
        public DefenseEffectHandle Play(DefenseEffectProfileSO profile, Vector3 position, float radius = 1f, Component owner = null, Transform anchor = null)
            => Play(new DefenseEffectRequest(profile, position, radius, owner, anchor));
        public bool Stop(DefenseEffectHandle handle) => false;
        public void StopOwner(Component owner) { }
    }
}
