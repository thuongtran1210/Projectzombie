using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectZombie.Features.Defense3D;

public sealed class OwnedPoolTests
{
    private sealed class Item { public bool Destroyed; public int Resets; }
    private static OwnedPool<Item> Create(int maxIdle = 4) => new OwnedPool<Item>(
        () => new Item(), item => item.Resets++, item => item.Destroyed = true, maxIdle, item => item != null && !item.Destroyed);

    [Test]
    public void Return_RejectsDuplicateAndOldTokenAfterReuse()
    {
        using (var pool = Create())
        {
            var first = pool.Acquire();
            Assert.IsTrue(pool.TryReturn(first));
            Assert.IsFalse(pool.TryReturn(first));
            var second = pool.Acquire();
            Assert.AreSame(first.Item, second.Item);
            Assert.AreNotEqual(first.Token, second.Token);
            Assert.IsFalse(pool.TryReturn(first));
            Assert.AreEqual(1, pool.ActiveCount);
            Assert.IsTrue(pool.TryReturn(second));
            Assert.AreEqual(2, second.Item.Resets);
        }
    }
    [Test]
    public void Return_RejectsOtherPoolWithoutTouchingItsLease()
    {
        using (var first = Create()) using (var second = Create())
        {
            var lease = first.Acquire();
            Assert.IsFalse(second.TryReturn(lease));
            Assert.AreEqual(1, first.ActiveCount);
            Assert.AreEqual(0, lease.Item.Resets);
        }
    }
    [Test]
    public void Clear_InvalidatesActiveLeasesAndAllowsNextRun()
    {
        using (var pool = Create())
        {
            var previous = pool.Acquire();
            pool.ClearActive(); pool.ClearActive();
            Assert.AreEqual(0, pool.ActiveCount);
            Assert.AreEqual(1, previous.Item.Resets);
            var current = pool.Acquire();
            Assert.IsFalse(pool.TryReturn(previous));
            Assert.IsTrue(pool.TryReturn(current));
        }
    }
    [Test]
    public void Dispose_ResetsActiveDestroysAllAndRejectsNewAcquisition()
    {
        var pool = Create();
        var first = pool.Acquire(); var second = pool.Acquire();
        pool.TryReturn(first);
        pool.Dispose(); pool.Dispose();
        Assert.IsTrue(first.Item.Destroyed); Assert.IsTrue(second.Item.Destroyed);
        Assert.AreEqual(1, second.Item.Resets);
        Assert.AreEqual(0, pool.ActiveCount); Assert.AreEqual(0, pool.InactiveCount);
        Assert.IsFalse(pool.TryReturn(second));
        Assert.Throws<ObjectDisposedException>(() => pool.Acquire());
    }
    [Test]
    public void RetainedCapacity_DoesNotDiscardActiveGameplayObjects()
    {
        using (var pool = Create(1))
        {
            var first = pool.Acquire(); var second = pool.Acquire();
            Assert.AreEqual(2, pool.ActiveCount);
            pool.TryReturn(first); pool.TryReturn(second);
            Assert.AreEqual(1, pool.InactiveCount);
            Assert.IsTrue(second.Item.Destroyed);
        }
    }
    [Test]
    public void Cleanup_ClosesLeaseBeforeCallbackAndBlocksReentrantAcquire()
    {
        OwnedPool<Item> pool = null;
        PoolLease<Item> lease = default;
        bool duplicateAccepted = true;
        pool = new OwnedPool<Item>(() => new Item(), item =>
        {
            duplicateAccepted = pool.TryReturn(lease);
            Assert.Throws<InvalidOperationException>(() => pool.Acquire());
        }, item => item.Destroyed = true);
        lease = pool.Acquire();
        pool.ClearActive();
        Assert.IsFalse(duplicateAccepted);
        pool.Dispose();
    }
    [Test]
    public void ResetFailure_DestroysFailedItemAndDoesNotKeepItActive()
    {
        var pool = new OwnedPool<Item>(() => new Item(), item => throw new InvalidOperationException(), item => item.Destroyed = true);
        var lease = pool.Acquire();
        Assert.Throws<InvalidOperationException>(() => pool.TryReturn(lease));
        Assert.IsTrue(lease.Item.Destroyed);
        Assert.AreEqual(0, pool.ActiveCount); Assert.AreEqual(0, pool.InactiveCount);
        pool.Dispose();
    }
    [Test]
    public void Cleanup_AttemptsEveryItemEvenIfOneResetFails()
    {
        var failed = new Item();
        int created = 0;
        var pool = new OwnedPool<Item>(() => created++ == 0 ? failed : new Item(),
            item => { if (ReferenceEquals(item, failed)) throw new InvalidOperationException(); item.Resets++; }, item => item.Destroyed = true);
        pool.Acquire(); var good = pool.Acquire();
        Assert.Throws<AggregateException>(() => pool.ClearActive());
        Assert.AreEqual(0, pool.ActiveCount);
        Assert.IsTrue(failed.Destroyed); Assert.AreEqual(1, good.Item.Resets);
        pool.Dispose();
    }
    [Test]
    public void Prewarm_IsBoundedAndDoesNotCreateActiveLeases()
    {
        using (var pool = Create(2))
        {
            pool.Prewarm(20); pool.Prewarm(2);
            Assert.AreEqual(2, pool.InactiveCount);
            Assert.AreEqual(0, pool.ActiveCount);
        }
    }
    [Test]
    public void Acquire_SkipsExternallyDestroyedIdleItems()
    {
        using (var pool = Create())
        {
            var first = pool.Acquire(); pool.TryReturn(first); first.Item.Destroyed = true;
            var second = pool.Acquire();
            Assert.AreNotSame(first.Item, second.Item);
        }
    }
    [Test]
    public void Dispose_AttemptsAllIdleDestructionsAfterOneFailure()
    {
        int attempts = 0;
        var pool = new OwnedPool<Item>(() => new Item(), item => item.Resets++, item =>
        {
            attempts++;
            if (attempts == 1) throw new InvalidOperationException();
            item.Destroyed = true;
        });
        pool.Prewarm(2);
        Assert.Throws<AggregateException>(() => pool.Dispose());
        Assert.AreEqual(2, attempts);
        Assert.AreEqual(0, pool.InactiveCount);
        Assert.DoesNotThrow(() => pool.Dispose());
    }

    [TestCase(0f)]
    [TestCase(-1f)]
    [TestCase(float.NaN)]
    public void EffectLifetime_RejectsInvalidDuration(float duration)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DefenseEffectLifetime(0f, duration, false));
    }

    [Test]
    public void TimedEffect_UsesGameplayClock_AndSustainedRequiresExplicitStop()
    {
        var timed = new DefenseEffectLifetime(10f, .5f, false);
        Assert.IsFalse(timed.IsExpired(10.4f));
        Assert.AreEqual(.5f, timed.Progress(10.25f), .001f);
        Assert.IsTrue(timed.IsExpired(10.5f));
        Assert.AreEqual(1f, timed.Progress(11f));
        var sustained = new DefenseEffectLifetime(10f, .5f, true);
        Assert.IsFalse(sustained.IsExpired(1000f));
        Assert.AreEqual(0f, sustained.Progress(1000f));
    }
}
