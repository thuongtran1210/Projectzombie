using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectZombie.Features.Defense3D;

public sealed class DefenseG1RuntimeAdapterTests
{
    [Test]
    public void Restart_UsesReverseCleanup_ForwardReset_ThreeCycles()
    {
        var order = new List<string>();
        using (var adapter = new DefenseG1RuntimeAdapter())
        {
            adapter.RegisterOwner(() => order.Add("cleanupA"), () => order.Add("resetA"));
            adapter.RegisterOwner(() => order.Add("cleanupB"), () => order.Add("resetB"));
            for (int i = 0; i < 3; i++)
            {
                Assert.IsTrue(adapter.Start());
                adapter.Tick(10f, 100f, false);
                adapter.Restart();
                Assert.AreEqual(i + 1, adapter.Run.RunGeneration);
                Assert.AreEqual(0f, adapter.Run.Clock.ElapsedSeconds);
                Assert.AreEqual(DefenseGamePhase.Ready, adapter.Run.State.Phase);
            }
        }
        CollectionAssert.AreEqual(new[] {
            "cleanupB", "cleanupA", "resetA", "resetB",
            "cleanupB", "cleanupA", "resetA", "resetB",
            "cleanupB", "cleanupA", "resetA", "resetB"
        }, order);
    }

    [Test]
    public void FailedCleanup_PreventsCoreResetAndGenerationAdvance()
    {
        using (var adapter = new DefenseG1RuntimeAdapter())
        {
            adapter.RegisterOwner(() => { throw new InvalidOperationException("cleanup failed"); }, () => {});
            adapter.Start();
            adapter.Tick(12f, 100f, false);
            Assert.Throws<InvalidOperationException>(() => adapter.Restart());
            Assert.AreEqual(0, adapter.Run.RunGeneration);
            Assert.AreEqual(12f, adapter.Run.Clock.ElapsedSeconds);
        }
    }

    [Test]
    public void Dispose_MakesAdapterUnusable_WithoutInvokingSceneCleanup()
    {
        var adapter = new DefenseG1RuntimeAdapter();
        int calls = 0;
        adapter.RegisterOwner(() => calls++, () => calls++);
        adapter.Dispose();
        adapter.Dispose();
        Assert.AreEqual(0, calls);
        Assert.Throws<ObjectDisposedException>(() => adapter.Start());
        Assert.Throws<ObjectDisposedException>(() => adapter.Restart());
    }
}
