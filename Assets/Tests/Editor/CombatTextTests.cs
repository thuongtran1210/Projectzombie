using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.UI.DamageText;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.UI;

public sealed class CombatTextTests
{
    [Test]
    public void Leases_LimitActiveAndPrioritizePlayerOverCriticalOverNormal()
    {
        var returned = new List<object>();
        using (var leases = new CombatTextLeases<object>(2, () => new object(), returned.Add))
        {
            Assert.IsTrue(leases.TryAcquire(0, out var normal, out _));
            Assert.IsTrue(leases.TryAcquire(1, out var crit, out _));
            Assert.IsFalse(leases.TryAcquire(0, out _, out _));
            Assert.IsTrue(leases.TryAcquire(2, out _, out _));
            Assert.AreEqual(2, leases.Count);
            CollectionAssert.AreEqual(new[] { normal }, returned);
            Assert.IsTrue(leases.TryAcquire(2, out _, out _));
            Assert.AreSame(crit, returned[1]);
            Assert.IsFalse(leases.TryAcquire(2, out _, out _));
        }
    }
    [Test]
    public void Leases_IgnoreDuplicateAndStaleCompletionsAfterReuse()
    {
        var item = new object();
        int releases = 0;
        using (var leases = new CombatTextLeases<object>(1, () => item, _ => releases++))
        {
            leases.TryAcquire(0, out _, out long first);
            Assert.IsTrue(leases.Complete(item, first));
            Assert.IsFalse(leases.Complete(item, first));
            leases.TryAcquire(0, out _, out long second);
            Assert.IsFalse(leases.Complete(item, first));
            Assert.AreEqual(1, leases.Count);
            Assert.IsTrue(leases.Complete(item, second));
            Assert.AreEqual(2, releases);
        }
    }
    [Test]
    public void Leases_ClearInvalidatesOldLeaseAndAllowsNewRun_DisposeRejectsAcquisition()
    {
        var item = new object();
        int releases = 0;
        var leases = new CombatTextLeases<object>(1, () => item, _ => releases++);
        leases.TryAcquire(0, out _, out long old);
        leases.Clear(); leases.Clear();
        Assert.AreEqual(1, releases);
        Assert.IsTrue(leases.TryAcquire(0, out _, out _));
        Assert.IsFalse(leases.Complete(item, old));
        leases.Dispose(); leases.Dispose();
        Assert.AreEqual(2, releases);
        Assert.AreEqual(0, leases.Count);
        Assert.IsFalse(leases.TryAcquire(2, out _, out _));
    }
    [Test]
    public void Leases_RejectReentrantAcquireWhileClearing()
    {
        CombatTextLeases<object> leases = null;
        bool acquired = true;
        leases = new CombatTextLeases<object>(1, () => new object(), releasedItem => acquired = leases.TryAcquire(0, out _, out _));
        leases.TryAcquire(0, out _, out _);
        leases.Clear();
        Assert.IsFalse(acquired);
        Assert.AreEqual(0, leases.Count);
        leases.Dispose();
    }
    [Test]
    public void DamageReportMappingPreservesFlagsAndFormat()
    {
        var position = new Vector3(3, 4, 5);
        var request = CombatTextRequest.FromDamage(new DamageReport(42, true, ElementType.Hoa, position, true, true));
        Assert.AreEqual(position, request.Position);
        Assert.AreEqual(CombatTextTarget.Player, request.Target);
        Assert.AreEqual(ElementType.Hoa, request.Element);
        Assert.AreEqual(2, request.Priority);
        Assert.IsTrue(request.IsValid);
        Assert.AreEqual("<b>* 42!</b>", DamageTextManager.Format(request));
    }
    [TestCase(0f)] [TestCase(-1f)] [TestCase(float.NaN)] [TestCase(float.PositiveInfinity)]
    public void DamageRequestRejectsInvalidAmount(float value)
    {
        Assert.IsFalse(new CombatTextRequest(value, Vector3.zero).IsValid);
    }
    [Test]
    public void Requests_ValidatePositionAndCustomText_FormatHealAndBlock()
    {
        Assert.IsFalse(new CombatTextRequest(1, new Vector3(float.NaN, 0, 0)).IsValid);
        Assert.IsFalse(new CombatTextRequest(0, Vector3.zero, kind: CombatTextKind.Custom).IsValid);
        var heal = new CombatTextRequest(12, Vector3.zero, kind: CombatTextKind.Heal);
        var block = new CombatTextRequest(0, Vector3.zero, kind: CombatTextKind.Block);
        Assert.IsTrue(block.IsValid);
        Assert.AreEqual("+12", DamageTextManager.Format(heal));
        Assert.AreEqual("BLOCK", DamageTextManager.Format(block));
    }
    [Test]
    public void Styles_PreserveLegacyPriorityAndResolveHeal()
    {
        var style = ScriptableObject.CreateInstance<DamageTextStyleConfig>();
        try
        {
            Assert.AreEqual(style.PlayerDamageColor, style.ResolveColor(new CombatTextRequest(1, Vector3.zero, CombatTextTarget.Player, true, true)));
            Assert.AreEqual(style.CounterColor, style.ResolveColor(new CombatTextRequest(1, Vector3.zero, isCritical: true, isCounter: true)));
            Assert.AreEqual(style.HealColor, style.ResolveColor(new CombatTextRequest(1, Vector3.zero, kind: CombatTextKind.Heal)));
        }
        finally { UnityEngine.Object.DestroyImmediate(style); }
    }
    [Test]
    public void DefenseAdapter_FiltersForeignAndOldRunEvents_UnsubscribesOnDispose()
    {
        using (var session = new DefenseSession())
        {
            session.Start();
            var source = new DamageSource();
            var sink = new Sink();
            var adapter = new DefenseCombatTextAdapter(source, session, sink, () => Vector3.up, CombatTextTarget.Structure);
            var valid = new DefenseDamageContext(20, isCritical: true, runGeneration: session.RunGeneration, sessionIdentity: session);
            source.Raise(valid);
            Assert.AreEqual(1, sink.Count);
            Assert.AreEqual(CombatTextTarget.Structure, sink.Last.Target);
            Assert.AreEqual(Vector3.up, sink.Last.Position);
            Assert.IsTrue(sink.Last.IsCritical);
            source.Raise(new DefenseDamageContext(20, sessionIdentity: new object()));
            source.Raise(new DefenseDamageContext(20, runGeneration: session.RunGeneration + 1));
            session.Pause(); source.Raise(valid); session.Resume();
            Assert.AreEqual(1, sink.Count);
            session.Restart();
            source.Raise(valid);
            Assert.AreEqual(1, sink.Count);
            source.Raise(new DefenseDamageContext(20, runGeneration: session.RunGeneration, sessionIdentity: session));
            Assert.AreEqual(2, sink.Count);
            adapter.Dispose(); adapter.Dispose(); source.Raise(valid);
            Assert.AreEqual(2, sink.Count);
            Assert.AreEqual(0, source.Subscribers);
        }
    }
    private sealed class Sink : ICombatTextService
    {
        public int Count;
        public CombatTextRequest Last;
        public bool Show(in CombatTextRequest request) { Count++; Last = request; return true; }
    }
    private sealed class DamageSource : IDefenseDamageable3D
    {
        private Action<DefenseDamageContext> _damaged;
        public int Subscribers => _damaged == null ? 0 : _damaged.GetInvocationList().Length;
        public float CurrentHP => 100;
        public float MaxHP => 100;
        public bool IsDead => false;
        public event Action<float, float> HealthChanged { add { } remove { } }
        public event Action Defeated { add { } remove { } }
        public event Action<DefenseDamageContext> Damaged { add { _damaged += value; } remove { _damaged -= value; } }
        public void Raise(DefenseDamageContext context) { _damaged?.Invoke(context); }
        public bool TakeDamage(DefenseDamageContext context) { Raise(context); return true; }
    }
}
