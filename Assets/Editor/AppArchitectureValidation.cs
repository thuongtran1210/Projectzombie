#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using NUnit.Framework.Api;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Execution;
using NUnit.Framework.Internal.Filters;
using UnityEditor;
using UnityEngine;

/// <summary>Runs migration, transition, session and presentation regressions on Unity's main thread.</summary>
public static class AppArchitectureValidation
{
    [MenuItem("Tools/ProjectZombie/Architecture/Validate App Flow")]
    public static void Validate()
    {
        if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Requires Edit Mode.");
        var runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());
        runner.Load(typeof(ProjectZombie.Tests.Editor.AppFlowTests).Assembly, new Dictionary<string, object>());
        var filter = new OrFilter();
        foreach (string name in new[] { "ProjectZombie.Tests.Editor.AppFlowTests", "ProjectZombie.Tests.Editor.DefenseMatchNavigationAndPreparationTests",
            "ProjectZombie.Tests.Editor.MainHubReadinessTests",
            "ProjectZombie.Tests.Editor.DefenseLoadoutCatalogAndAdapterTests",
            "ProjectZombie.Tests.Editor.DefenseLoadoutSaveAndMetaAdapterTests",
            "ProjectZombie.Tests.Editor.DefenseMatchResultAndLifecycleTests", "DefenseG6BossAndResultTests",
            "ProjectZombie.Tests.Editor.StageSpawnConfigTests", "ProjectZombie.Tests.Editor.DefenseSessionBalancingTests",
            "DefenseEnemyHealthTests", "DefenseSpawnWarningTests", "DefenseEffectAuthoringTests", "OwnedPoolTests" })
            filter.Add(new FullNameFilter(name));
        var work = WorkItem.CreateWorkItem(runner.LoadedTest, filter);
        work.InitializeContext(new TestExecutionContext { Dispatcher = new MainThreadDispatcher(), IsSingleThreaded = true });
        work.Execute();
        Directory.CreateDirectory("scratch");
        File.WriteAllText("scratch/AppArchitectureValidation.xml", work.Result.ToXml(true).OuterXml);
        File.WriteAllText("scratch/AppArchitectureValidation.txt", work.Result.ResultState + "\nPassed: " + work.Result.PassCount + "\nFailed: " + work.Result.FailCount);
        Debug.Log("[AppArchitecture] " + work.Result.ResultState + " | passed=" + work.Result.PassCount + " failed=" + work.Result.FailCount);
    }
    private sealed class MainThreadDispatcher : IWorkItemDispatcher
    {
        public void Dispatch(WorkItem work) => work.Execute();
        public void CancelRun(bool force) { }
    }
}
#endif
