#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using ProjectZombie.Core.Architecture;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.Loadout;
using ProjectZombie.Features.UI;

/// <summary>Bounded Play Mode navigation regression. Does not finish a run or award/save rewards.</summary>
[InitializeOnLoad]
public static class AppArchitectureSmoke
{
    private const string KEY = "ProjectZombie.AppFlowSmoke";
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CaptureCurrencyWarnings()
    {
        if (!SessionState.GetBool(KEY, false)) return;
        SessionState.SetInt(KEY + ".CurrencyWarnings", 0);
        Application.logMessageReceived += CountCurrencyWarning;
    }
    private static void CountCurrencyWarning(string message, string trace, LogType type)
    {
        if (type == LogType.Warning && message.Contains("PersistentSingleton<MetaCurrencyManager>"))
            SessionState.SetInt(KEY + ".CurrencyWarnings", SessionState.GetInt(KEY + ".CurrencyWarnings", 0) + 1);
    }
    static AppArchitectureSmoke()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(KEY, false))
                EditorApplication.delayCall += Run;
            if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(KEY + ".Restore", false))
            {
                string previous = SessionState.GetString(KEY + ".PreviousScene", "");
                UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene = string.IsNullOrEmpty(previous) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(previous);
                SessionState.SetBool(KEY + ".Restore", false);
            }
        };
    }

    [MenuItem("Tools/ProjectZombie/Architecture/Run Navigation Smoke")]
    public static void Start()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Requires Edit Mode.");
        SessionState.SetBool(KEY, true);
        Directory.CreateDirectory("scratch");
        File.WriteAllText("scratch/AppArchitectureSmoke.txt", "Running...");
        EditorApplication.isPlaying = true;
    }

        public static void StartFromBootstrap()
        {
            StartFromScene(AppArchitectureInstaller.BOOT);
        }

        public static void StartFromMap()
        {
            StartFromScene(AppArchitectureInstaller.MAP);
        }

        private static void StartFromScene(string path)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Requires Edit Mode.");
            var previous = UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene;
            SessionState.SetString(KEY + ".PreviousScene", previous != null ? AssetDatabase.GetAssetPath(previous) : "");
            SessionState.SetBool(KEY + ".Restore", true);
            UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            Start();
        }

    private static async void Run()
    {
        try
        {
            var deadline = EditorApplication.timeSinceStartup + 90;
            while (AppCompositionRoot.Instance == null)
            {
                if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Bootstrap did not initialize.");
                await Task.Yield();
            }
            var app = AppCompositionRoot.Instance;
            while (app.Flow.IsNavigating) await Task.Yield();
            var directMap = UnityEngine.Object.FindObjectOfType<DefenseSceneCompositionRoot>();
            if (directMap != null)
            {
                while (!directMap.IsReady)
                {
                    if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Direct map initialization failed.");
                    await Task.Yield();
                }
                Check(await app.Flow.ReturnToHubAsync(), "direct map returns to hub");
            }
            await Task.Yield();
            Check(UnityEngine.Object.FindObjectsOfType<AppCompositionRoot>().Length == 1, "one app root");
            Check(UnityEngine.Object.FindObjectsOfType<EventSystem>().Length == 1, "one EventSystem");
            var hub = UnityEngine.Object.FindObjectOfType<MainHubPresenter>();
            Check(hub != null, "hub loaded");
            var view = hub.GetComponent<MainHubView>();
            var button = new SerializedObject(view).FindProperty("_startRunButton").objectReferenceValue as UnityEngine.UI.Button;
            Check(button != null, "start button wired");
            button.onClick.Invoke();
            await Task.Yield();
            Check(UnityEngine.Object.FindObjectsOfType<ProjectZombie.Features.UI.StageSelect.StageSelectUIPresenter>().Any(x => x.isActiveAndEnabled), "start opens stage selection");
            var stage = app.Stages.Stages.First(x => x.CanLaunchDefense);
            for (int attempt = 0; attempt < 2; attempt++)
            {
                app.Flow.SelectStage(stage);
                var request = new MatchLaunchRequest(stage, DefenseLoadoutSnapshot.CreateDefault());
                Check(await app.Flow.EnterMatchAsync(request), "enter map " + attempt + ": " + app.Flow.LastError);
                var composition = UnityEngine.Object.FindObjectOfType<DefenseSceneCompositionRoot>();
                Check(composition != null && composition.IsReady, "session ready");
                Check(UnityEngine.Object.FindObjectsOfType<EventSystem>().Length == 1, "one EventSystem in map");
                Check(UnityEngine.Object.FindObjectsOfType<ProjectZombie.Core.Save.GameManager>().Length == 1, "one save owner");
                var run = UnityEngine.Object.FindObjectOfType<DefenseRun6003D>();
                Check(run.TotalEventsCount == 35, "production spawn plan retained");
                if (attempt == 0)
                {
                    int generation = composition.Gameplay.RunGeneration;
                    string runId = app.Flow.ActiveMatch.RunId;
                    run.RestartRun();
                    Check(composition.Gameplay.RunGeneration > generation, "restart advances lease generation");
                    Check(runId != app.Flow.ActiveMatch.RunId, "restart creates new attempt identity");
                }
                Check(await app.Flow.ReturnToHubAsync(), "return to hub");
                Check(UnityEngine.Object.FindObjectOfType<DefenseRun6003D>() == null, "map objects released");
                Check(UnityEngine.Object.FindObjectOfType<MainHubPresenter>() != null, "hub restored");
            }
            Check(SessionState.GetInt(KEY + ".CurrencyWarnings", 0) == 0, "currency service accessed before bootstrap readiness");
            File.WriteAllText("scratch/AppArchitectureSmoke.txt", "Passed: button -> stage selection; 2 map entries; restart; 2 hub returns; one app/save/EventSystem owner; 35 production events; zero early currency warnings.");
        }
        catch (Exception exception)
        {
            File.WriteAllText("scratch/AppArchitectureSmoke.txt", "Failed: " + exception);
            Debug.LogError("[AppFlow Smoke] " + exception);
        }
        finally
        {
            Application.logMessageReceived -= CountCurrencyWarning;
            SessionState.SetBool(KEY, false);
            EditorApplication.isPlaying = false;
        }
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
#endif
