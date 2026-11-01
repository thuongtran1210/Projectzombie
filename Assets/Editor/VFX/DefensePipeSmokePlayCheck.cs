#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ProjectZombie.Core.Architecture;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.Loadout;
using ProjectZombie.Features.Defense3D.UI;

/// <summary>Exercises the production loadout, HUD, aim view, cast owner, pause and pooled cleanup in a real match.</summary>
[InitializeOnLoad]
public static class DefensePipeSmokePlayCheck
{
    private const string KEY = "ProjectZombie.PipeSmokePlayCheck";
    static DefensePipeSmokePlayCheck()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(KEY, false))
                EditorApplication.delayCall += Run;
            if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(KEY + ".Restore", false))
            {
                string path = SessionState.GetString(KEY + ".Previous", "");
                EditorSceneManager.playModeStartScene = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                SessionState.SetBool(KEY + ".Restore", false);
            }
        };
    }

    [MenuItem("Tools/VFX Generator/Run Pipe Smoke Match Check")]
    public static void Start()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Requires Edit Mode.");
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save dirty scenes before running the match check.");
        var previous = EditorSceneManager.playModeStartScene;
        SessionState.SetString(KEY + ".Previous", previous != null ? AssetDatabase.GetAssetPath(previous) : "");
        SessionState.SetBool(KEY + ".Restore", true); SessionState.SetBool(KEY, true);
        EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(AppArchitectureInstaller.BOOT);
        Directory.CreateDirectory("scratch"); File.WriteAllText("scratch/PipeSmokeMatchCheck.txt", "Running...");
        EditorApplication.isPlaying = true;
    }

    private static async void Run()
    {
        try
        {
            double deadline = EditorApplication.timeSinceStartup + 90;
            while (AppCompositionRoot.Instance == null || AppCompositionRoot.Instance.Flow.IsNavigating)
            {
                Check(EditorApplication.timeSinceStartup < deadline, "Bootstrap timed out."); await Task.Yield();
            }
            var app = AppCompositionRoot.Instance;
            var loadout = new DefenseLoadoutSnapshot("hero_daosi", "relic_dieucay", new[] { "minion_danbinh" }, new[] { "tower_cungtien" });
            Check(await app.Flow.EnterMatchAsync(new MatchLaunchRequest(app.Stages.Stages.First(stage => stage.CanLaunchDefense), loadout)), "Enter pipe loadout match.");
            var composition = UnityEngine.Object.FindObjectOfType<DefenseSceneCompositionRoot>();
            Check(composition != null && composition.IsReady, "Match composition ready.");
            var presenter = UnityEngine.Object.FindObjectsOfType<DefenseSkillPresenter3D>().First(item => item.Effect is DefensePipeRelicEffect3D);
            var pipe = (DefensePipeRelicEffect3D)presenter.Effect;
            var data = new SerializedObject(presenter);
            var indicator = data.FindProperty("_indicator").objectReferenceValue as DefenseAimIndicator3D;
            var view = data.FindProperty("_view").objectReferenceValue as DefenseSkillButtonView;
            var runtime = (DefenseSkillCastRuntime)typeof(DefenseSkillPresenter3D).GetField("_runtime", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(presenter);
            var p = pipe.transform.position;
            var origin = new DefensePoint3(p.x, p.y, p.z);
            DefenseAimResolver.TryResolve(origin, new DefensePoint3(p.x + 3, p.y, p.z + 2), runtime.Definition.Range, out var aim);
            indicator.ShowArea(aim, runtime.Definition.Radius, runtime.Definition.Range, false);
            view.SetAimFeedback(true, false);
            await Capture("PipeSmokeAim.png");
            view.SetAimFeedback(true, true); indicator.ShowArea(aim, runtime.Definition.Radius, runtime.Definition.Range, true);
            await Capture("PipeSmokeCancel.png");
            view.SetAimFeedback(false, false); indicator.Hide();
            Check(runtime.TryCast(new DefenseSkillCastRequest(aim, composition.Gameplay.SessionIdentity, composition.Gameplay.RunGeneration), origin, true, pipe), "Bound cooldown owner accepted cast.");
            await Delay(.35f);
            await Capture("PipeSmokeImpact.png");
            Check(pipe.ActiveZoneCount == 1 && pipe.RemainingSmokeSeconds > 0, "Hazard and HUD lifetime active.");
            var service = UnityEngine.Object.FindObjectOfType<DefenseEffectService3D>();
            Check(service.ActiveCount > 0, "Session VFX service owns smoke.");
            float remaining = pipe.RemainingSmokeSeconds;
            Check(composition.Gameplay is DefenseSession, "Session type.");
            var session = (DefenseSession)composition.Gameplay;
            session.Pause(); await Delay(.4f);
            Check(pipe.ActiveZoneCount == 1 && Mathf.Abs(pipe.RemainingSmokeSeconds - remaining) < .1f, "Pause preserves smoke and lifetime.");
            session.Resume();
            await Delay(.7f); await Capture("PipeSmokeActive.png");
            while (pipe.ActiveZoneCount > 0)
            {
                Check(EditorApplication.timeSinceStartup < deadline, "Smoke expiry timed out."); await Task.Yield();
            }
            Check(pipe.RemainingSmokeSeconds == 0f, "Hazard lifetime cleared.");
            await Task.Yield();
            Check(await app.Flow.ReturnToHubAsync(), "Session cleanup and hub return.");
            File.WriteAllText("scratch/PipeSmokeMatchCheck.txt", "Passed: production DaoSi/DieuCay loadout; authored HUD and aim/cancel; real cooldown owner cast; pooled smoke; pause/resume; hazard expiry; hub return.\nInput gestures were visualized through the view; physical touch input and Android performance were not measured.");
        }
        catch (Exception exception)
        {
            File.WriteAllText("scratch/PipeSmokeMatchCheck.txt", "Failed: " + exception);
            Debug.LogException(exception);
        }
        finally
        {
            SessionState.SetBool(KEY, false);
            EditorApplication.isPlaying = false;
        }
    }

    private static async Task Delay(float seconds)
    {
        double until = EditorApplication.timeSinceStartup + seconds;
        while (EditorApplication.isPlaying && EditorApplication.timeSinceStartup < until) await Task.Yield();
    }

    private static async Task Capture(string name)
    {
        string path = Path.GetFullPath("scratch/" + name);
        if (File.Exists(path)) File.Delete(path);
        await Task.Yield(); ScreenCapture.CaptureScreenshot(path);
        double deadline = EditorApplication.timeSinceStartup + 8;
        while (!File.Exists(path))
        {
            Check(EditorApplication.timeSinceStartup < deadline, "Game View capture timed out: " + name); await Task.Yield();
        }
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
#endif
