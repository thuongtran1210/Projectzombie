using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

public sealed class DefenseDashFeedbackTests
{
    private const string Folder = "Assets/Features/Defense3D/Effects/Assets/";
    private sealed class Fixture : IDisposable
    {
        public readonly DefenseSession Session = new DefenseSession();
        public readonly DefenseDashFeedbackEvents Events = new DefenseDashFeedbackEvents();
        public readonly Sink Effects = new Sink();
        public readonly DefenseDashEffectSetSO Style = AssetDatabase.LoadAssetAtPath<DefenseDashEffectSetSO>(Folder + "FXSet_HeroDash.asset");
        public readonly GameObject Owner = new GameObject("DashAdapterUnitOwner");
        public readonly DefenseDashFeedbackAdapter Adapter;
        public bool CanPresent = true;
        public Fixture()
        {
            Session.Start();
            Adapter = new DefenseDashFeedbackAdapter(Events, Session, Effects, Style, Owner.transform, Owner.transform, () => CanPresent);
        }
        public DefenseDashFeedback Cue(long id, bool completed = false, int? generation = null) =>
            new DefenseDashFeedback(id, new DefensePoint3(3, 0, 4), new DefensePoint3(0, 0, 1), Session, generation ?? Session.RunGeneration, completed);
        public void Dispose() { Adapter.Dispose(); Session.Dispose(); UnityEngine.Object.DestroyImmediate(Owner); }
    }
    [Test]
    public void StartedPlaysBurstAndAttachedTrail_CompletedStopsThemAndPlaysEndOnce()
    {
        using (var f = new Fixture())
        {
            f.Events.PublishStarted(f.Cue(1));
            Assert.AreEqual(2, f.Effects.Requests.Count); Assert.AreEqual(2, f.Effects.ActiveCount);
            Assert.AreSame(f.Style.Start, f.Effects.Requests[0].Profile);
            Assert.AreSame(f.Style.Trail, f.Effects.Requests[1].Profile);
            Assert.AreSame(f.Owner.transform, f.Effects.Requests[1].Anchor);
            f.Events.PublishEnded(f.Cue(1, true)); f.Events.PublishEnded(f.Cue(1, true));
            Assert.AreEqual(3, f.Effects.Requests.Count); Assert.AreEqual(1, f.Effects.ActiveCount);
            Assert.AreSame(f.Style.End, f.Effects.Requests[2].Profile);
            Assert.AreEqual(new Vector3(3, f.Style.Lift, 4), f.Effects.Requests[2].Position);
        }
    }
    [Test]
    public void CancelledOrDownedDash_StopsOwnHandlesWithoutEndBurst()
    {
        using (var f = new Fixture())
        {
            f.Events.PublishStarted(f.Cue(1));
            f.CanPresent = false;
            f.Events.PublishEnded(f.Cue(1));
            Assert.AreEqual(0, f.Effects.ActiveCount); Assert.AreEqual(2, f.Effects.Requests.Count);
            Assert.AreEqual(0, f.Effects.StopOwnerCalls);
        }
    }
    [Test]
    public void PreviousEndCannotStopNewDash_AndDuplicateStartIsIgnored()
    {
        using (var f = new Fixture())
        {
            f.Events.PublishStarted(f.Cue(1)); f.Events.PublishEnded(f.Cue(1));
            f.Events.PublishStarted(f.Cue(2)); f.Events.PublishStarted(f.Cue(2));
            f.Events.PublishEnded(f.Cue(1, true)); f.Events.PublishStarted(f.Cue(1));
            Assert.AreEqual(2, f.Effects.ActiveCount); Assert.AreEqual(4, f.Effects.Requests.Count);
        }
    }
    [Test]
    public void PauseAndRestartClearTrail_AndRejectOldScope()
    {
        using (var f = new Fixture())
        {
            var old = f.Cue(1);
            f.Events.PublishStarted(old); f.Session.Pause();
            Assert.AreEqual(0, f.Effects.ActiveCount);
            f.Events.PublishEnded(f.Cue(1, true));
            Assert.AreEqual(2, f.Effects.Requests.Count);
            f.Session.Resume(); f.Session.Restart();
            f.Events.PublishStarted(old);
            Assert.AreEqual(0, f.Effects.ActiveCount);
            f.Events.PublishStarted(f.Cue(2));
            f.Events.PublishEnded(new DefenseDashFeedback(2, default, default, new object(), f.Session.RunGeneration, true));
            Assert.AreEqual(2, f.Effects.ActiveCount);
        }
    }
    [TestCase(false)]
    [TestCase(true)]
    public void CancelDuringPlay_DoesNotLeakHandleReturnedAfterCancellation(bool cancelOnTrail)
    {
        using (var f = new Fixture())
        {
            f.Effects.OnPlay = request =>
            {
                if (request.Profile == (cancelOnTrail ? f.Style.Trail : f.Style.Start)) f.Events.PublishEnded(f.Cue(1));
            };
            f.Events.PublishStarted(f.Cue(1));
            Assert.AreEqual(0, f.Effects.ActiveCount);
            Assert.AreEqual(cancelOnTrail ? 2 : 1, f.Effects.Requests.Count);
        }
    }
    [Test]
    public void NewDashDuringEndBurst_DoesNotKeepOldBurstOrStopNewTrail()
    {
        using (var f = new Fixture())
        {
            f.Events.PublishStarted(f.Cue(1));
            f.Effects.OnPlay = request => { if (request.Profile == f.Style.End) f.Events.PublishStarted(f.Cue(2)); };
            f.Events.PublishEnded(f.Cue(1, true));
            Assert.AreEqual(2, f.Effects.ActiveCount);
            f.Events.PublishEnded(f.Cue(1)); Assert.AreEqual(2, f.Effects.ActiveCount);
            f.Events.PublishEnded(f.Cue(2)); Assert.AreEqual(0, f.Effects.ActiveCount);
        }
    }
    [Test]
    public void DisposeIsIdempotentAndUnsubscribesBothEvents()
    {
        using (var f = new Fixture())
        {
            f.Events.PublishStarted(f.Cue(1)); f.Adapter.Dispose(); f.Adapter.Dispose();
            f.Events.PublishStarted(f.Cue(2)); f.Events.PublishEnded(f.Cue(2, true));
            Assert.AreEqual(0, f.Effects.ActiveCount); Assert.AreEqual(2, f.Effects.Requests.Count);
        }
    }
    [Test]
    public void TrailResetClearsGeometryEmissionAndTintBeforeReuse()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "VFX_DashTrail_VongXuyen.prefab");
        var copy = UnityEngine.Object.Instantiate(prefab);
        try
        {
            var instance = copy.GetComponent<DefenseEffectInstance3D>(); var trail = copy.GetComponentInChildren<TrailRenderer>();
            Color original = trail.startColor;
            instance.Prepare(Color.red, 1f); instance.Begin();
            trail.AddPosition(Vector3.zero); trail.AddPosition(Vector3.right);
            Assert.Greater(trail.positionCount, 0);
            instance.ResetPresentation();
            Assert.AreEqual(0, trail.positionCount); Assert.IsFalse(trail.emitting); Assert.IsFalse(trail.enabled);
            Assert.AreEqual(original, trail.startColor);
            copy.transform.position = new Vector3(9999, 0, 9999);
            instance.Prepare(Color.green, .5f);
            Assert.AreEqual(0, trail.positionCount); Assert.AreEqual(Color.green, trail.startColor);
        }
        finally { UnityEngine.Object.DestroyImmediate(copy); }
    }

    internal sealed class Sink : IDefenseEffectService
    {
        private static readonly ConstructorInfo HandleConstructor = typeof(DefenseEffectHandle).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(object), typeof(long) }, null);
        private static readonly FieldInfo TokenField = typeof(DefenseEffectHandle).GetField("Token", BindingFlags.Instance | BindingFlags.NonPublic);
        private readonly HashSet<long> _active = new HashSet<long>();
        private long _next;
        public readonly List<DefenseEffectRequest> Requests = new List<DefenseEffectRequest>();
        public Action<DefenseEffectRequest> OnPlay;
        public int StopOwnerCalls;
        public int ActiveCount => _active.Count;
        public DefenseEffectHandle Play(in DefenseEffectRequest request)
        {
            long token = ++_next; _active.Add(token); Requests.Add(request);
            var handle = (DefenseEffectHandle)HandleConstructor.Invoke(new object[] { this, token });
            OnPlay?.Invoke(request);
            return handle;
        }
        public DefenseEffectHandle Play(DefenseEffectProfileSO profile, Vector3 position, float radius = 1f, Component owner = null, Transform anchor = null)
            => Play(new DefenseEffectRequest(profile, position, radius, owner, anchor));
        public bool Stop(DefenseEffectHandle handle) => _active.Remove((long)TokenField.GetValue(handle));
        public void StopOwner(Component owner) { StopOwnerCalls++; }
    }
}
