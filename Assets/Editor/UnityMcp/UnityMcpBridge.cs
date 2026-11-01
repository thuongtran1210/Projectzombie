#if UNITY_EDITOR
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ProjectZombie.Editor.UnityMcp
{
    /// <summary>Editor-only, main-thread dispatcher for durable MCP jobs; independent of the legacy bridge.</summary>
    [InitializeOnLoad]
    internal static class UnityMcpBridge
    {
        internal static readonly string ROOT = Path.GetFullPath("Library/UnityMcpBridge");
        private const int LOG_CAPACITY = 500;
        private const string TEST_JOB_KEY = "ProjectZombie.UnityMcp.TestJob";
        private static readonly string EDITOR_SESSION = GetEditorSession();
        private static readonly ConcurrentQueue<JObject> LOGS = new ConcurrentQueue<JObject>();
        private static readonly List<JObject> COMPILER_MESSAGES = new List<JObject>();
        private static double _nextPoll;
        private static double _nextHeartbeat;

        private static string GetEditorSession()
        {
            const string key = "ProjectZombie.UnityMcp.EditorSession";
            string value = SessionState.GetString(key, "");
            if (!string.IsNullOrEmpty(value)) return value;
            value = Guid.NewGuid().ToString("N");
            SessionState.SetString(key, value);
            return value;
        }

        static UnityMcpBridge()
        {
            foreach (string folder in new[] { "requests", "jobs", "artifacts" })
            {
                Directory.CreateDirectory(Path.Combine(ROOT, folder));
            }
            Application.logMessageReceivedThreaded += OnLog;
            CompilationPipeline.compilationStarted += context => COMPILER_MESSAGES.Clear();
            CompilationPipeline.assemblyCompilationFinished += (assembly, messages) =>
            {
                foreach (var message in messages)
                {
                    COMPILER_MESSAGES.Add(new JObject { ["assembly"] = assembly, ["message"] = message.message,
                        ["file"] = message.file, ["line"] = message.line, ["column"] = message.column,
                        ["type"] = message.type.ToString() });
                }
                Write(Path.Combine(ROOT, "compiler.json"), new JArray(COMPILER_MESSAGES));
            };
            EditorApplication.update += Poll;
            EditorApplication.quitting += () => WriteHeartbeat(false);
            AssemblyReloadEvents.beforeAssemblyReload += () => WriteHeartbeat(false);
            UnityMcpTests.Initialize();
            WriteHeartbeat(true);
        }

        private static void OnLog(string message, string stackTrace, LogType type)
        {
            LOGS.Enqueue(new JObject { ["utc"] = DateTime.UtcNow.ToString("O"), ["type"] = type.ToString(),
                ["message"] = message, ["stackTrace"] = stackTrace });
            while (LOGS.Count > LOG_CAPACITY) LOGS.TryDequeue(out _);
        }

        private static void WriteHeartbeat(bool available)
        {
            Write(Path.Combine(ROOT, "heartbeat.json"), new JObject { ["utc"] = DateTime.UtcNow.ToString("O"),
                ["available"] = available, ["project"] = Path.GetFullPath("."), ["unityVersion"] = Application.unityVersion,
                ["compiling"] = EditorApplication.isCompiling, ["updating"] = EditorApplication.isUpdating,
                ["playing"] = EditorApplication.isPlaying });
        }

        private static void Poll()
        {
            try
            {
                if (EditorApplication.timeSinceStartup >= _nextHeartbeat)
                {
                    _nextHeartbeat = EditorApplication.timeSinceStartup + 1;
                    WriteHeartbeat(true);
                }
                if (EditorApplication.timeSinceStartup < _nextPoll) return;
                _nextPoll = EditorApplication.timeSinceStartup + 0.2;
                ResumeJobs();
                if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
                string path = Directory.GetFiles(Path.Combine(ROOT, "requests"), "*.json")
                    .OrderBy(File.GetLastWriteTimeUtc).FirstOrDefault();
                if (path == null) return;
                Process(path);
            }
            catch (Exception exception)
            {
                // Disk failures must not flood the Console every Editor update.
                _nextPoll = EditorApplication.timeSinceStartup + 5;
                Debug.LogException(exception);
            }
        }

        private static void Process(string path)
        {
            string id = Path.GetFileNameWithoutExtension(path);
            if (!Guid.TryParseExact(id, "N", out _))
            {
                File.Delete(path);
                return;
            }
            JObject job = ReadJob(id);
            if (job != null && (string)job["state"] != "queued")
            {
                File.Delete(path);
                return;
            }
            try
            {
                JObject request = JObject.Parse(File.ReadAllText(path));
                if ((string)request["id"] != id) throw new ArgumentException("Request ID mismatch.");
                string command = (string)request["command"];
                job = new JObject { ["id"] = id, ["command"] = command, ["state"] = "running",
                    ["editorSession"] = EDITOR_SESSION,
                    ["createdUtc"] = request["createdUtc"], ["startedUtc"] = DateTime.UtcNow.ToString("O") };
                if (DateTime.Parse((string)request["expiresUtc"], CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal) < DateTime.UtcNow)
                {
                    job["state"] = "expired";
                    job["error"] = "Request expired before execution; no action performed.";
                    SaveJob(job);
                    File.Delete(path);
                    return;
                }
                SaveJob(job);
                File.Delete(path);
                JObject arguments = request["arguments"] as JObject ?? new JObject();
                JToken result = Execute(command, arguments, job);
                if (result != null) Complete(id, result);
            }
            catch (Exception exception)
            {
                if (job != null && (string)job["command"] == "tests_run") SessionState.EraseString(TEST_JOB_KEY);
                if (job == null) job = new JObject { ["id"] = id };
                job["state"] = "failed";
                job["error"] = exception.ToString();
                job["finishedUtc"] = DateTime.UtcNow.ToString("O");
                SaveJob(job);
                if (File.Exists(path)) File.Delete(path);
            }
        }

        private static JToken Execute(string command, JObject args, JObject job)
        {
            switch (command)
            {
                case "editor_status":
                    return new JObject { ["unityVersion"] = Application.unityVersion,
                        ["playing"] = EditorApplication.isPlaying, ["paused"] = EditorApplication.isPaused,
                        ["transitioning"] = EditorApplication.isPlayingOrWillChangePlaymode != EditorApplication.isPlaying,
                        ["scenes"] = new JArray(Scenes().Select(scene => new JObject { ["path"] = scene.path,
                            ["name"] = scene.name, ["dirty"] = scene.isDirty })),
                        ["compilerDiagnostics"] = File.Exists(Path.Combine(ROOT, "compiler.json"))
                            ? JArray.Parse(File.ReadAllText(Path.Combine(ROOT, "compiler.json"))) : new JArray(),
                        ["consoleScope"] = "Messages since this bridge domain loaded; compiler diagnostics persist separately." };
                case "console_read":
                    string severity = (string)args["severity"] ?? "all";
                    var logs = LOGS.ToArray().Where(row => severity == "all" ||
                        (severity == "error" && new[] { "Error", "Exception", "Assert" }.Contains((string)row["type"])) ||
                        (severity == "warning" && (string)row["type"] == "Warning") ||
                        (severity == "log" && (string)row["type"] == "Log")).ToArray();
                    return new JObject { ["entries"] = new JArray(logs.Skip(Math.Max(0, logs.Length - Limit(args, 500)))),
                        ["scope"] = "Current domain only; bounded to 500 messages; Console is not cleared." };
                case "hierarchy_query": return UnityMcpObjects.Hierarchy(args);
                case "component_read": return UnityMcpObjects.Read(args);
                case "serialized_property_set":
                    RequireIdle();
                    return UnityMcpObjects.Set(args);
                case "asset_find": return UnityMcpObjects.FindAssets(args);
                case "capture_game_view":
                    RequireIdle();
                    if (!EditorApplication.isPlaying || EditorApplication.isPaused)
                        throw new InvalidOperationException("Capture requires running, unpaused Play Mode and a visible Game View.");
                    string output = Path.Combine(ROOT, "artifacts", (string)job["id"] + ".png");
                    job["capturePath"] = output;
                    job["deadlineUtc"] = DateTime.UtcNow.AddSeconds(20).ToString("O");
                    SaveJob(job);
                    ScreenCapture.CaptureScreenshot(output);
                    return null;
                case "playmode_set":
                    RequireIdle();
                    bool playing = args.Value<bool>("playing");
                    if (playing == EditorApplication.isPlaying && playing == EditorApplication.isPlayingOrWillChangePlaymode)
                        return new JObject { ["playing"] = playing };
                    if (playing) RequireCleanScenes();
                    job["targetPlaying"] = playing;
                    job["deadlineUtc"] = DateTime.UtcNow.AddSeconds(90).ToString("O");
                    SaveJob(job);
                    EditorApplication.isPlaying = playing;
                    return null;
                case "tests_run":
                    RequireIdle();
                    RequireCleanScenes();
                    if (EditorApplication.isPlayingOrWillChangePlaymode)
                        throw new InvalidOperationException("Tests require Edit Mode.");
                    SessionState.SetString(TEST_JOB_KEY, (string)job["id"]);
                    job["deadlineUtc"] = DateTime.UtcNow.AddMinutes(15).ToString("O");
                    SaveJob(job);
                    UnityMcpTests.Start(args);
                    return null;
                default: throw new ArgumentException("Unknown MCP command: " + command);
            }
        }

        private static void ResumeJobs()
        {
            foreach (string path in Directory.GetFiles(Path.Combine(ROOT, "jobs"), "*.json"))
            {
                JObject job = JObject.Parse(File.ReadAllText(path));
                if ((string)job["state"] != "running") continue;
                string id = (string)job["id"];
                string command = (string)job["command"];
                if ((string)job["editorSession"] != EDITOR_SESSION)
                {
                    job["state"] = "interrupted";
                    job["error"] = "Editor restarted. Inspect state before retrying; effects may have occurred.";
                    SaveJob(job);
                    continue;
                }
                if (command == "playmode_set" && job["targetPlaying"] != null &&
                    EditorApplication.isPlaying == (bool)job["targetPlaying"] &&
                    EditorApplication.isPlayingOrWillChangePlaymode == EditorApplication.isPlaying)
                {
                    Complete(id, new JObject { ["playing"] = EditorApplication.isPlaying });
                    continue;
                }
                if (command == "capture_game_view" && File.Exists((string)job["capturePath"]))
                {
                    var texture = new Texture2D(2, 2);
                    try
                    {
                        if (texture.LoadImage(File.ReadAllBytes((string)job["capturePath"])))
                        {
                            Complete(id, new JObject { ["path"] = job["capturePath"], ["width"] = texture.width,
                                ["height"] = texture.height, ["includesOverlayUI"] = true });
                            continue;
                        }
                    }
                    finally { Object.DestroyImmediate(texture); }
                }
                if (job["deadlineUtc"] != null && DateTime.Parse((string)job["deadlineUtc"],
                        CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal) > DateTime.UtcNow)
                {
                    if (command != "tests_run" || SessionState.GetString(TEST_JOB_KEY, "") == id) continue;
                }
                job["state"] = "interrupted";
                job["error"] = "Editor restarted, execution deadline elapsed, or a domain reload interrupted work. Inspect state before retrying; effects may have occurred.";
                SaveJob(job);
            }
        }

        internal static void RequireIdle()
        {
            if (!string.IsNullOrEmpty(ActiveTestJob) || UnityMcpTests.IsRunning)
                throw new InvalidOperationException("A test run is active. Wait for completion or stop it in Unity Test Runner.");
            // The current job has no deadline until execution starts; other asynchronous jobs do.
            foreach (string path in Directory.GetFiles(Path.Combine(ROOT, "jobs"), "*.json"))
            {
                JObject job = JObject.Parse(File.ReadAllText(path));
                if ((string)job["state"] == "running" && job["deadlineUtc"] != null)
                    throw new InvalidOperationException("Another asynchronous job is running: " + job["id"]);
            }
        }

        private static void RequireCleanScenes()
        {
            if (Scenes().Any(scene => scene.isDirty || string.IsNullOrEmpty(scene.path)))
                throw new InvalidOperationException("Save dirty/untitled scenes in Unity before changing mode or running tests.");
        }

        internal static IEnumerable<Scene> Scenes()
        {
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                if (scene.isLoaded) yield return scene;
            }
        }

        internal static int Limit(JObject args, int max)
        {
            return Mathf.Clamp(args.Value<int?>("limit") ?? 100, 1, max);
        }

        internal static string ActiveTestJob => SessionState.GetString(TEST_JOB_KEY, "");

        internal static void FinishTests(JToken result, string error = null)
        {
            string id = ActiveTestJob;
            if (string.IsNullOrEmpty(id)) return;
            JObject job = ReadJob(id);
            if (job != null && (string)job["state"] == "running")
            {
                job["state"] = error == null ? "succeeded" : "failed";
                job["result"] = result;
                job["error"] = error;
                job["finishedUtc"] = DateTime.UtcNow.ToString("O");
                SaveJob(job);
            }
            SessionState.EraseString(TEST_JOB_KEY);
        }

        internal static JObject ReadJob(string id)
        {
            string path = Path.Combine(ROOT, "jobs", id + ".json");
            return File.Exists(path) ? JObject.Parse(File.ReadAllText(path)) : null;
        }

        private static void Complete(string id, JToken result)
        {
            JObject job = ReadJob(id);
            job["state"] = "succeeded";
            job["result"] = result;
            job["finishedUtc"] = DateTime.UtcNow.ToString("O");
            SaveJob(job);
        }

        internal static void SaveJob(JObject job)
        {
            Write(Path.Combine(ROOT, "jobs", (string)job["id"] + ".json"), job);
        }

        internal static void Write(string path, JToken value)
        {
            string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            File.WriteAllText(temporary, value.ToString(Formatting.None));
            if (File.Exists(path)) File.Replace(temporary, path, null);
            else File.Move(temporary, path);
        }
    }
}
#endif
