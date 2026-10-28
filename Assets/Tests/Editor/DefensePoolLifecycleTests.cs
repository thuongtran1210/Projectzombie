using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

public sealed class DefensePoolLifecycleTests
{
    [Test]
    public void Pickup_StaleCompletionCannotReturnReusedDrop_AndDisposeDestroysActive()
    {
        var source = new GameObject("PickupPoolTestPrefab");
        source.SetActive(false);
        source.AddComponent<DefenseWoodDrop3D>();
        var root = new GameObject("PickupPoolTestRoot");
        var pool = new DefenseWoodDropPool(source, root.transform);
        try
        {
            var first = pool.Get();
            var callbackField = typeof(DefenseWoodDrop3D).GetField("_poolRelease", BindingFlags.Instance | BindingFlags.NonPublic);
            var oldCompletion = (Action<DefenseWoodDrop3D>)callbackField.GetValue(first);
            pool.Release(first); pool.Release(first);
            var second = pool.Get();
            Assert.AreSame(first, second);
            oldCompletion(first);
            Assert.AreEqual(1, pool.ActiveDrops.Count);
            Assert.IsTrue(second.gameObject.activeSelf);
            pool.Dispose();
            Assert.IsTrue(second == null);
            Assert.Throws<ObjectDisposedException>(() => pool.Get());
        }
        finally
        {
            pool.Dispose();
            UnityEngine.Object.DestroyImmediate(source); UnityEngine.Object.DestroyImmediate(root);
        }
    }
    [Test]
    public void EnemyLease_CannotBeReturnedToAnotherSessionPoolOrReleasedTwice()
    {
        var source = new GameObject("EnemyPoolTestPrefab");
        source.SetActive(false);
        var rootA = new GameObject("EnemyPoolA"); var rootB = new GameObject("EnemyPoolB");
        var a = new DefenseEnemyPoolManager(rootA.transform); var b = new DefenseEnemyPoolManager(rootB.transform);
        try
        {
            var first = a.Acquire(source, rootA.transform);
            Assert.IsFalse(b.TryRelease(first));
            Assert.IsTrue(a.TryRelease(first));
            Assert.IsFalse(a.TryRelease(first));
            var second = a.Acquire(source, rootA.transform);
            Assert.AreSame(first.Item, second.Item);
            Assert.IsFalse(a.TryRelease(first));
            Assert.IsTrue(a.TryRelease(second));
            a.FlushReturns();
            Assert.IsTrue(second.Item.transform.IsChildOf(rootA.transform));
            Assert.IsFalse(second.Item.gameObject.activeSelf);
        }
        finally
        {
            a.Dispose(); b.Dispose();
            UnityEngine.Object.DestroyImmediate(source);
            UnityEngine.Object.DestroyImmediate(rootA); UnityEngine.Object.DestroyImmediate(rootB);
        }
    }
    [Test]
    public void PickupSpawn_BindsSession_AndResetRemovesSessionSubscription()
    {
        var source = new GameObject("PickupLifecyclePrefab"); source.SetActive(false);
        source.AddComponent<DefenseWoodDrop3D>();
        var storage = new GameObject("PickupLifecycleStorage"); storage.SetActive(false);
        var active = new GameObject("PickupLifecycleActive");
        using (var session = new DefenseSession())
        {
            var pool = new DefenseWoodDropPool(source, storage.transform, gameplay: session);
            try
            {
                session.Start();
                var drop = pool.Spawn(Vector3.zero, 10, null, null, session, active.transform);
                Assert.IsTrue(drop.isActiveAndEnabled);
                Assert.AreSame(active.transform, drop.transform.parent);
                var context = typeof(DefenseGameplayBehaviour).GetProperty("Gameplay", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.AreSame(session, context.GetValue(drop));
                Assert.IsFalse(drop.CanBeGathered(session)); // Đang trong thời gian chờ nhặt.
                pool.Release(drop);
                Assert.IsFalse(drop.gameObject.activeSelf);
                Assert.IsNull(context.GetValue(drop));
                Assert.IsFalse(drop.CanBeGathered(session));
                session.Restart();
                Assert.AreEqual(0, pool.ActiveDrops.Count);
                Assert.DoesNotThrow(() => pool.Release(drop));
            }
            finally { pool.Dispose(); }
        }
        UnityEngine.Object.DestroyImmediate(source); UnityEngine.Object.DestroyImmediate(storage); UnityEngine.Object.DestroyImmediate(active);
    }
}
