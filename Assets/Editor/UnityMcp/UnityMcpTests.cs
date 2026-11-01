#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.Editor.UnityMcp
{
    /// <summary>Re-registers Test Framework callbacks on reload and persists asynchronous test results.</summary>
    internal sealed class UnityMcpTests : IErrorCallbacks
    {
        private static TestRunnerApi _api;
        internal static bool IsRunning => SessionState.GetBool("ProjectZombie.UnityMcp.TestsRunning", false);

        internal static void Initialize()
        {
            _api = ScriptableObject.CreateInstance<TestRunnerApi>();
            _api.RegisterCallbacks(new UnityMcpTests());
        }

        internal static void Start(JObject args)
        {
            string mode = (string)args["mode"] ?? "EditMode";
            if (mode != "EditMode" && mode != "PlayMode") throw new ArgumentException("Invalid test mode.");
            var filter = new Filter { testMode = mode == "EditMode" ? TestMode.EditMode : TestMode.PlayMode,
                testNames = (args["test_names"] as JArray)?.ToObject<string[]>(),
                assemblyNames = (args["assembly_names"] as JArray)?.ToObject<string[]>() };
            _api.Execute(new ExecutionSettings(filter));
        }

        /// <summary>Callbacks are registered globally; only runs requested through this bridge are recorded.</summary>
        public void RunStarted(ITestAdaptor testsToRun)
        {
            SessionState.SetBool("ProjectZombie.UnityMcp.TestsRunning", true);
        }
        /// <summary>Individual progress is summarized by the final result tree.</summary>
        public void TestStarted(ITestAdaptor test) { }
        /// <summary>Individual progress is summarized by the final result tree.</summary>
        public void TestFinished(ITestResultAdaptor result) { }

        /// <summary>Writes a complete NUnit XML report and a compact JSON result.</summary>
        public void RunFinished(ITestResultAdaptor result)
        {
            SessionState.SetBool("ProjectZombie.UnityMcp.TestsRunning", false);
            if (string.IsNullOrEmpty(UnityMcpBridge.ActiveTestJob)) return;
            string output = Path.Combine(UnityMcpBridge.ROOT, "artifacts", UnityMcpBridge.ActiveTestJob + ".xml");
            try
            {
                File.WriteAllText(output, result.ToXml().OuterXml);
                UnityMcpBridge.FinishTests(Summarize(result, output));
            }
            catch (Exception exception)
            {
                UnityMcpBridge.FinishTests(null, exception.ToString());
            }
        }

        private static JObject Summarize(ITestResultAdaptor result, string path)
        {
            return new JObject { ["resultState"] = result.ResultState, ["passed"] = result.PassCount,
                ["failed"] = result.FailCount, ["skipped"] = result.SkipCount, ["durationSeconds"] = result.Duration,
                ["reportPath"] = path, ["message"] = result.Message,
                ["failures"] = new JArray(Failures(result).Take(100)) };
        }

        private static System.Collections.Generic.IEnumerable<JObject> Failures(ITestResultAdaptor result)
        {
            if (!result.HasChildren && result.ResultState.StartsWith("Failed", StringComparison.Ordinal))
                yield return new JObject { ["test"] = result.FullName, ["message"] = result.Message,
                    ["stackTrace"] = result.StackTrace };
            if (!result.HasChildren) yield break;
            foreach (ITestResultAdaptor child in result.Children)
                foreach (JObject failure in Failures(child)) yield return failure;
        }

        /// <summary>Captures prebuild and compilation errors reported by Test Framework.</summary>
        public void OnError(string message)
        {
            SessionState.SetBool("ProjectZombie.UnityMcp.TestsRunning", false);
            UnityMcpBridge.FinishTests(null, message);
        }
    }
}
#endif
