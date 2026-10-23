using NUnit.Framework;
using ProjectZombie.Features.Defense3D;

public sealed class DefenseG1LifecycleTests
{
    [Test]
    public void ThreeRestarts_ResetCoreWithoutMultiplyingClockCallbacks()
    {
        var run = new DefenseRunLifecycle();
        int durationEvents = 0;
        run.Clock.DurationReached += () => durationEvents++;
        for (int i = 0; i < 3; i++)
        {
            Assert.IsTrue(run.Start());
            Assert.AreEqual(600f, run.Tick(600f, 100f, false));
            Assert.AreEqual(DefenseGamePhase.Overtime, run.State.Phase);
            run.Tick(1f, 100f, true);
            Assert.AreEqual(DefenseGamePhase.Win, run.State.Phase);
            run.ResetCore();
            Assert.AreEqual(DefenseGamePhase.Ready, run.State.Phase);
            Assert.AreEqual(0f, run.Clock.ElapsedSeconds);
            Assert.AreEqual(i + 1, run.RunGeneration);
        }
        Assert.AreEqual(3, durationEvents);
    }

    [Test]
    public void ClockAndOutcomeFreezeDuringModal_ExceptBaseLoss()
    {
        var run = new DefenseRunLifecycle();
        run.Start();
        run.Tick(590f, 100f, false);
        run.OpenLevelUp();
        Assert.AreEqual(0f, run.Tick(30f, 100f, true));
        Assert.AreEqual(590f, run.Clock.ElapsedSeconds);
        Assert.AreEqual(DefenseGamePhase.LevelUp, run.State.Phase);
        run.Resume();
        run.Tick(10f, 100f, true);
        Assert.AreEqual(DefenseGamePhase.Win, run.State.Phase);

        run.ResetCore();
        run.Start();
        run.Pause();
        run.Tick(10f, 0f, true);
        Assert.AreEqual(DefenseGamePhase.Loss, run.State.Phase);
        Assert.AreEqual(0f, run.Clock.ElapsedSeconds);
    }

    [Test]
    public void RestartResetsOvertimeResumeState()
    {
        var run = new DefenseRunLifecycle();
        run.Start();
        run.Tick(600f, 100f, false);
        run.Pause();
        Assert.AreEqual(DefenseGamePhase.Overtime, run.State.ResumePhase);
        run.ResetCore();
        Assert.AreEqual(DefenseGamePhase.Playing, run.State.ResumePhase);
        Assert.IsFalse(run.Resume());
        Assert.IsTrue(run.Start());
    }
}
