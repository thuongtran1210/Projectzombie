using NUnit.Framework;
using ProjectZombie.Features.Defense3D;

/// <summary>Two-press relic timing must not depend on VFX callbacks or pointer repeats.</summary>
public sealed class DefenseRelicCastStateTests
{
    [Test]
    public void ThrowCannotRecastBeforeImpact()
    {
        var state = new DefenseRelicCastState();
        Assert.IsTrue(state.Begin(0f, .2f, 4f));
        Assert.IsFalse(state.CanRecast(.1f));
        Assert.IsFalse(state.Begin(.1f, .2f, 4f));
        Assert.IsFalse(state.Advance(.1f));
        Assert.IsTrue(state.Advance(.2f));
        Assert.IsTrue(state.CanRecast(.2f));
        Assert.IsFalse(state.Advance(.3f));
    }
    [Test]
    public void SecondPressConsumesAnchorExactlyOnce()
    {
        var state = new DefenseRelicCastState();
        state.Begin(0f, .2f, 4f);
        state.Advance(.2f);
        Assert.IsTrue(state.Consume(1f));
        Assert.IsFalse(state.Consume(1f));
        Assert.AreEqual(DefenseRelicPhase.Ready, state.Phase);
    }
    [Test]
    public void ExpiryRejectsRecastEvenBeforeNextUpdate()
    {
        var state = new DefenseRelicCastState();
        state.Begin(0f, 1f, 4f);
        state.Advance(1f);
        Assert.IsFalse(state.CanRecast(5f));
        Assert.IsFalse(state.Consume(5f));
        state.Advance(5f);
        Assert.AreEqual(DefenseRelicPhase.Ready, state.Phase);
    }
    [Test]
    public void LateTickDoesNotExtendWindowOrDuplicateImpact()
    {
        var state = new DefenseRelicCastState();
        state.Begin(0f, 1f, 4f);
        Assert.IsTrue(state.Advance(6f));
        Assert.IsFalse(state.CanRecast(6f));
        Assert.IsFalse(state.Advance(7f));
    }
    [Test]
    public void CleanupCancelsFlightAndAnchor()
    {
        var state = new DefenseRelicCastState();
        state.Begin(0f, .2f, 4f);
        state.Reset();
        Assert.IsFalse(state.Advance(1f));
        Assert.IsTrue(state.Begin(1f, .2f, 4f));
        state.Advance(1.2f);
        state.Reset();
        Assert.IsFalse(state.Consume(2f));
    }
    [Test]
    public void InvalidTimingIsRejected()
    {
        var state = new DefenseRelicCastState();
        Assert.IsFalse(state.Begin(float.NaN, .2f, 4f));
        Assert.IsFalse(state.Begin(0f, 0f, 4f));
        Assert.IsFalse(state.Begin(0f, .2f, -1f));
        Assert.AreEqual(DefenseRelicPhase.Ready, state.Phase);
    }
}
