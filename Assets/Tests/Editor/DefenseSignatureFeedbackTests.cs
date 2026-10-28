using System;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

public sealed class DefenseSignatureFeedbackTests
{
    private const string Folder = "Assets/Features/Defense3D/Effects/Assets/";
    private sealed class Fixture : IDisposable
    {
        public readonly DefenseSession Session = new DefenseSession();
        public readonly DefenseSignatureFeedbackEvents Events = new DefenseSignatureFeedbackEvents();
        public readonly DefenseDashFeedbackTests.Sink Effects = new DefenseDashFeedbackTests.Sink();
        public readonly GameObject Owner = new GameObject("SignatureAdapterUnitOwner");
        public readonly DefenseSignatureEffectSetSO Style = AssetDatabase.LoadAssetAtPath<DefenseSignatureEffectSetSO>(Folder + "FXSet_ScholarSignature.asset");
        public readonly DefenseSignatureFeedbackAdapter Adapter;
        public bool CanPresent = true;
        public Fixture()
        {
            Session.Start();
            Adapter = new DefenseSignatureFeedbackAdapter(Events, Session, Effects, Style, Owner.transform, Owner.transform, () => CanPresent);
        }
        public DefenseSignatureFeedback Cue(long id = 1, bool completed = false) => new DefenseSignatureFeedback(id,
            new DefensePoint3(2, 0, 3), 6f, Session, Session.RunGeneration, completed);
        public DefenseSignatureHitFeedback Hit(long id = 1) => new DefenseSignatureHitFeedback(id,
            new DefenseHitFeedback(new DefensePoint3(4, 1, 5), new DefensePoint3(0, 0, 1), 52.5f, Session, Session.RunGeneration));
        public void Dispose() { Adapter.Dispose(); Session.Dispose(); UnityEngine.Object.DestroyImmediate(Owner); }
    }
    [Test]
    public void StagesUseDistinctProfiles_AndCompletionDoesNotReplayRelease()
    {
        using (var f = new Fixture())
        {
            f.Events.PublishStarted(f.Cue());
            Assert.AreEqual(1, f.Effects.ActiveCount); Assert.AreSame(f.Style.Cast, f.Effects.Requests[0].Profile);
            Assert.AreSame(f.Owner.transform, f.Effects.Requests[0].Anchor); Assert.AreEqual(6f, f.Effects.Requests[0].Radius);
            f.Events.PublishReleased(f.Cue()); f.Events.PublishHit(f.Hit());
            Assert.AreEqual(2, f.Effects.ActiveCount);
            Assert.AreSame(f.Style.Release, f.Effects.Requests[1].Profile); Assert.AreSame(f.Style.Hit, f.Effects.Requests[2].Profile);
            Assert.AreEqual(new Vector3(4, 1, 5), f.Effects.Requests[2].Position); Assert.IsNull(f.Effects.Requests[2].Anchor);
            f.Events.PublishEnded(f.Cue(completed: true)); f.Events.PublishEnded(f.Cue(completed: true));
            f.Events.PublishReleased(f.Cue()); f.Events.PublishHit(f.Hit());
            Assert.AreEqual(3, f.Effects.Requests.Count);
            f.Adapter.Clear(); Assert.AreEqual(0, f.Effects.ActiveCount);
        }
    }
    [Test]
    public void CancelStopsOnlySignatureHandles()
    {
        using (var f = new Fixture())
        {
            f.Effects.Play(f.Style.Hit, Vector3.zero);
            f.Events.PublishStarted(f.Cue()); f.Events.PublishReleased(f.Cue()); f.Events.PublishHit(f.Hit());
            f.CanPresent = false; f.Events.PublishEnded(f.Cue());
            Assert.AreEqual(1, f.Effects.ActiveCount); Assert.AreEqual(0, f.Effects.StopOwnerCalls);
        }
    }
    [Test]
    public void SparkBudgetLimitsVisualsWithoutChangingEventStream()
    {
        using (var f = new Fixture())
        {
            int observedHits = 0; f.Events.HitConfirmed += cue => observedHits++;
            f.Events.PublishStarted(f.Cue()); f.Events.PublishReleased(f.Cue());
            for (int i = 0; i < f.Style.MaxHitEffects + 10; i++) f.Events.PublishHit(f.Hit());
            Assert.AreEqual(f.Style.MaxHitEffects + 10, observedHits);
            Assert.AreEqual(f.Style.MaxHitEffects + 2, f.Effects.Requests.Count);
            Assert.AreEqual(f.Style.MaxHitEffects + 1, f.Effects.ActiveCount);
        }
    }
    [Test]
    public void OldCastAndRunCannotAffectNewCast()
    {
        using (var f = new Fixture())
        {
            var old = f.Cue(); f.Events.PublishStarted(old); f.Session.Restart();
            f.Events.PublishStarted(old); Assert.AreEqual(0, f.Effects.ActiveCount);
            f.Events.PublishStarted(f.Cue(2));
            f.Events.PublishEnded(old); f.Events.PublishReleased(old); f.Events.PublishHit(f.Hit(1));
            Assert.AreEqual(1, f.Effects.ActiveCount); Assert.AreEqual(2, f.Effects.Requests.Count);
            f.Session.Pause(); Assert.AreEqual(0, f.Effects.ActiveCount);
        }
    }
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public void CancelDuringPlayReturnsLateHandleInsteadOfLeaking(int stage)
    {
        using (var f = new Fixture())
        {
            var profile = stage == 0 ? f.Style.Cast : stage == 1 ? f.Style.Release : f.Style.Hit;
            f.Effects.OnPlay = request => { if (request.Profile == profile) f.Events.PublishEnded(f.Cue()); };
            f.Events.PublishStarted(f.Cue());
            if (stage > 0) f.Events.PublishReleased(f.Cue());
            if (stage > 1) f.Events.PublishHit(f.Hit());
            Assert.AreEqual(0, f.Effects.ActiveCount);
        }
    }
    [Test]
    public void NewCastDuringOldReleaseRejectsLateWaveHandle()
    {
        using (var f = new Fixture())
        {
            f.Events.PublishStarted(f.Cue());
            f.Effects.OnPlay = request => { if (request.Profile == f.Style.Release) f.Events.PublishStarted(f.Cue(2)); };
            f.Events.PublishReleased(f.Cue()); f.Events.PublishEnded(f.Cue(completed: true));
            Assert.AreEqual(1, f.Effects.ActiveCount);
            Assert.AreSame(f.Style.Cast, f.Effects.Requests[f.Effects.Requests.Count - 1].Profile);
        }
    }
    [Test]
    public void ForeignOrPrematureEventsAreIgnored_AndDisposeUnsubscribes()
    {
        using (var f = new Fixture())
        {
            f.Events.PublishReleased(f.Cue()); f.Events.PublishHit(f.Hit());
            f.Events.PublishStarted(new DefenseSignatureFeedback(1, default, 6f, new object(), f.Session.RunGeneration));
            Assert.AreEqual(0, f.Effects.Requests.Count);
            f.Events.PublishStarted(f.Cue()); f.Adapter.Dispose(); f.Adapter.Dispose();
            f.Events.PublishStarted(f.Cue(2)); f.Events.PublishReleased(f.Cue(2)); f.Events.PublishHit(f.Hit(2));
            Assert.AreEqual(0, f.Effects.ActiveCount); Assert.AreEqual(1, f.Effects.Requests.Count);
        }
    }
}
