using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

public static class DefenseG7QaTemporary
{
    private const string StatePath = "Temp/defense-g7-qa-state";
    private static TestRunnerApi api;

    private static readonly string[] TestsToRun = new[]
    {
        "DefenseG7MatchUITests",
        "DefenseG2CombatAndEnemyTests",
        "ProjectZombie.Tests.Editor.DefenseEnemyPrefab3DTests",
        "ProjectZombie.Tests.Editor.StageSpawnConfigTests",
        "DefenseG1StateClockTests",
        "DefenseG4DefenseAndHeroTests",
        "DefenseG5RogueliteTests",
        "DefenseG6BossAndResultTests"
    };

    [MenuItem("TREBRAVE/Defense 3D/QA/Run G7 Tests Manually")]
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
        if (!File.Exists(StatePath)) return;

        string state = File.ReadAllText(StatePath).Trim();
        if (state != "0:ready") return;

        File.WriteAllText(StatePath, "0:running");

        // QA must not regenerate authored prefabs or modify scene configuration.
        api.Execute(new ExecutionSettings(new Filter { testMode = TestMode.EditMode, testNames = TestsToRun }));
    }

    private sealed class Results : ICallbacks
    {
        public void RunStarted(ITestAdaptor testsToRun) { }

        public void RunFinished(ITestResultAdaptor result)
        {
            if (!Directory.Exists("Logs/TraversalQA"))
                Directory.CreateDirectory("Logs/TraversalQA");

            File.WriteAllText("Logs/TraversalQA/defense-g7-qa.xml", result.ToXml().OuterXml);
            File.WriteAllText(StatePath, "done");
            Debug.Log("<color=#00FF88>[TREBRAVE Defense3D]</color> Gate G7 QA Run Finished! Result: " + result.TestStatus);
        }

        public void TestStarted(ITestAdaptor test) { }
        public void TestFinished(ITestResultAdaptor result) { }
    }
}
