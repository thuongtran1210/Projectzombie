#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using NUnit.Framework.Api;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Execution;
using NUnit.Framework.Internal.Filters;
using UnityEditor;
using UnityEngine;

/// <summary>Deterministic projectile and tower/loadout regression suite.</summary>
public static class DefenseTowerValidation
{
    [MenuItem("Tools/ProjectZombie/Towers/Validate Modular Towers")]
    public static void Validate()
    {
        if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Requires Edit Mode.");
        var runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());
        runner.Load(typeof(ProjectZombie.Tests.Editor.DefenseModularTowerTests).Assembly, new Dictionary<string, object>());
        var filter = new OrFilter();
        foreach (string name in new[] { "ProjectZombie.Tests.Editor.DefenseModularTowerTests", "ProjectZombie.Tests.Editor.DefenseProjectileCoreAndPoolTests",
            "ProjectZombie.Tests.Editor.DefenseLoadoutCoreTests", "ProjectZombie.Tests.Editor.DefenseLoadoutCatalogAndAdapterTests",
            "ProjectZombie.Tests.Editor.DefenseLoadoutSaveAndMetaAdapterTests", "DefenseG4DefenseAndHeroTests" })
            filter.Add(new FullNameFilter(name));
        var safeFilter = new AndFilter();
        safeFilter.Add(filter);
        safeFilter.Add(new NotFilter(new FullNameFilter("ProjectZombie.Tests.Editor.DefenseLoadoutCatalogAndAdapterTests.DefensePreparationUIGenerator_GeneratesValidPrefabAndRegistersInRegistry")));
        var work = WorkItem.CreateWorkItem(runner.LoadedTest, safeFilter);
        work.InitializeContext(new TestExecutionContext { Dispatcher = new MainThreadDispatcher(), IsSingleThreaded = true });
        work.Execute();
        Directory.CreateDirectory("scratch");
        File.WriteAllText("scratch/TowerValidation.xml", work.Result.ToXml(true).OuterXml);
        File.WriteAllText("scratch/TowerValidation.txt", work.Result.ResultState + "\nPassed: " + work.Result.PassCount + "\nFailed: " + work.Result.FailCount);
        Debug.Log("[Tower Validation] " + work.Result.ResultState + " " + work.Result.PassCount + " passed / " + work.Result.FailCount + " failed");
    }
    private sealed class MainThreadDispatcher : IWorkItemDispatcher
    {
        public void Dispatch(WorkItem work) => work.Execute();
        public void CancelRun(bool force) { }
    }
}
#endif
