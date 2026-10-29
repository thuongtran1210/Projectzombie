using NUnit.Framework;
using ProjectZombie.Features.Defense3D;

public sealed class DefenseG1StateClockTests
{
    [Test]
    public void StartPauseLevelUpResume_ObeysTransitions()
    {
        var state = new DefenseGameState();
        Assert.IsFalse(state.TryResume());
        Assert.IsTrue(state.TryStart());
        Assert.IsFalse(state.TryStart());
        Assert.IsTrue(state.TryPause());
        Assert.IsFalse(state.TryOpenLevelUp());
        Assert.IsTrue(state.TryResume());
        Assert.AreEqual(DefenseGamePhase.Playing, state.Phase);
        Assert.IsTrue(state.TryOpenLevelUp());
        Assert.IsTrue(state.TryResume());
        Assert.AreEqual(DefenseGamePhase.Playing, state.Phase);
    }

    [Test]
    public void SameTickBaseDestroyed_BeatsYakshaDefeat()
    {
        var state = new DefenseGameState();
        state.TryStart();
        state.Resolve(0f, true, 600f);
        Assert.AreEqual(DefenseGamePhase.Loss, state.Phase);
        state.Resolve(100f, true, 700f);
        Assert.AreEqual(DefenseGamePhase.Loss, state.Phase);
    }

    [Test]
    public void YakshaDefeatBefore600_DoesNotWin_ThenOvertimeOrWin()
    {
        var state = new DefenseGameState();
        state.TryStart();
        state.Resolve(100f, true, 599f);
        Assert.AreEqual(DefenseGamePhase.Playing, state.Phase);
        state.Resolve(100f, false, 600f);
        Assert.AreEqual(DefenseGamePhase.Overtime, state.Phase);
        state.Resolve(100f, true, 610f);
        Assert.AreEqual(DefenseGamePhase.Win, state.Phase);
    }

    [Test]
    public void ClockOnlyTicksInActivePhases_AndDurationEventOnce()
    {
        var state = new DefenseGameState();
        var clock = new DefenseGameplayClock();
        int events = 0;
        clock.DurationReached += () => events++;
        Assert.AreEqual(0f, clock.Tick(3f, state));
        state.TryStart();
        Assert.AreEqual(599f, clock.Tick(599f, state));
        state.TryPause();
        Assert.AreEqual(0f, clock.Tick(10f, state));
        state.TryResume();
        state.TryOpenLevelUp();
        Assert.AreEqual(0f, clock.Tick(2f, state));
        state.TryResume();
        Assert.AreEqual(1f, clock.Tick(1f, state));
        Assert.AreEqual(1, events);
        clock.Tick(30f, state);
        Assert.AreEqual(1, events);
        state.Resolve(100f, true, clock.ElapsedSeconds);
        Assert.AreEqual(DefenseGamePhase.Win, state.Phase);
        Assert.AreEqual(0f, clock.Tick(1f, state));
        clock.Reset();
        state.Reset();
        state.TryStart();
        clock.Tick(600f, state);
        Assert.AreEqual(2, events);
    }

    [Test]
    public void ClockRejectsInvalidDelta()
    {
        var clock = new DefenseGameplayClock();
        var state = new DefenseGameState();
        Assert.Throws<System.ArgumentOutOfRangeException>(() => clock.Tick(-1f, state));
        Assert.Throws<System.ArgumentOutOfRangeException>(() => clock.Tick(float.NaN, state));
    }
}
