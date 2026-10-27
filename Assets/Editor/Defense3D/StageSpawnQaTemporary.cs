using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

public static class StageSpawnQaTemporary
{
    private const string StatePath = "Temp/stage-spawn-qa-state";
    private static TestRunnerApi api;
    private static readonly string[][] Stages = {
        new[] { "ProjectZombie.Tests.Editor.StageSpawnConfigTests",
            "ProjectZombie.Tests.Editor.DefenseEnemyFoundationTests", "ProjectZombie.Tests.Editor.DefenseWoodDrop3DTests",
            "ProjectZombie.Tests.Editor.DefenseEnemyPrefab3DTests", "ProjectZombie.Tests.Editor.Migration3DTraversalAndTargetingTests",
            "DefenseG1StateClockTests", "DefenseG2CombatAndEnemyTests", "DefenseG3StageAndSpawnTests",
            "DefenseG4DefenseAndHeroTests", "DefenseG5RogueliteTests", "DefenseG6BossAndResultTests" },
        new[] { "ProjectZombie.Tests.Editor.DefenseRuntimeIntegrationTests.EnemyFoundation_RuntimeScenariosComplete" }
    };
    [MenuItem("TREBRAVE/Defense 3D/QA/Run Stage Spawn Tests Manually")]
    private static void Run()
    {
        api = ScriptableObject.CreateInstance<TestRunnerApi>();
        api.RegisterCallbacks(new Results());
        File.WriteAllText(StatePath, "0:ready");
        EditorApplication.update += Launch;
        Launch();
    }
    private static void Launch()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
        var state = File.ReadAllText(StatePath).Trim().Split(':');
        int stage = int.Parse(state[0]);
        if (stage >= Stages.Length || state[1] != "ready") return;
        File.WriteAllText(StatePath, stage + ":running");
        api.Execute(new ExecutionSettings(new Filter { testMode = TestMode.EditMode, testNames = Stages[stage] }));
    }
    private sealed class Results : ICallbacks
    {
        public void RunStarted(ITestAdaptor testsToRun) { }
        public void RunFinished(ITestResultAdaptor result)
        {
            int stage = int.Parse(File.ReadAllText(StatePath).Split(':')[0]);
            File.WriteAllText("Logs/TraversalQA/stage-spawn-stage" + stage + ".xml", result.ToXml().OuterXml);
            File.WriteAllText(StatePath, (stage + 1) + ":ready");
            EditorApplication.delayCall += Launch;
        }
        public void TestStarted(ITestAdaptor test) { }
        public void TestFinished(ITestResultAdaptor result) { }
    }
}
