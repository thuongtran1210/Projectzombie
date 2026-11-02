#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ProjectZombie.Core.Architecture;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.Loadout;

[InitializeOnLoad]
public static class AppLoadingSmoke
{
    private const string Key="ProjectZombie.LoadingSmoke";
    static AppLoadingSmoke()
    {
        EditorApplication.playModeStateChanged+=state=>
        {
            if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Key,false))EditorApplication.delayCall+=Run;
            if(state==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key+".Restore",false))
            {var previous=SessionState.GetString(Key+".Previous","");EditorSceneManager.playModeStartScene=string.IsNullOrEmpty(previous)?null:AssetDatabase.LoadAssetAtPath<SceneAsset>(previous);SessionState.SetBool(Key+".Restore",false);}
        };
    }
    [MenuItem("Tools/ProjectZombie/Architecture/Run Loading UI Smoke")]
    public static void Start()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Requires Edit Mode.");
        for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Save scene edits before running Play Mode validation.");
        var previous=EditorSceneManager.playModeStartScene;SessionState.SetString(Key+".Previous",previous!=null?AssetDatabase.GetAssetPath(previous):"");
        SessionState.SetBool(Key+".Restore",true);SessionState.SetBool(Key,true);EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(AppArchitectureInstaller.BOOT);
        Directory.CreateDirectory("scratch");File.WriteAllText("scratch/LoadingSmoke.txt","Running...");EditorApplication.isPlaying=true;
    }
    private static async void Run()
    {
        try
        {
            double deadline=EditorApplication.timeSinceStartup+90;
            while(AppCompositionRoot.Instance==null||AppCompositionRoot.Instance.Flow.IsNavigating){Timeout(deadline);await Task.Yield();}
            var app=AppCompositionRoot.Instance;var view=app.GetComponentInChildren<AppLoadingView>(true);var samples=new List<float>();var trace=new List<string>();bool premature=false;
            Action<float,string> observe=(value,message)=>
            {
                if(samples.Count>0&&value+0.0001f<samples[samples.Count-1])throw new InvalidOperationException("Loading progress moved backwards.");
                samples.Add(value);trace.Add(value.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+" | "+message);
                if(value>=1f){var composition=UnityEngine.Object.FindObjectOfType<DefenseSceneCompositionRoot>();if(composition==null||!composition.IsReady)premature=true;}
            };
            view.ProgressChanged+=observe;
            bool captured=false;
            try
            {
                var stage=app.Stages.Stages.First(x=>x.CanLaunchDefense);
                var transition=app.Flow.EnterMatchAsync(new MatchLaunchRequest(stage,DefenseLoadoutSnapshot.CreateDefault()));
                while(!transition.IsCompleted)
                {
                    Timeout(deadline);
                    Check(view.DisplayedProgress<=view.Progress+.001f,"display never runs ahead of work");
                    if(!captured&&view.IsVisible&&view.DisplayedProgress>=.3f&&view.Progress<.98f)
                    {
                        Check(view.GetComponent<CanvasGroup>().blocksRaycasts,"cover blocks input during loading");
                        ScreenCapture.CaptureScreenshot(Path.GetFullPath("scratch/LoadingTransition.png"));captured=true;
                    }
                    await Task.Yield();
                }
                Check(await transition,"menu enters match");Check(!premature,"100% requires ready gameplay");Check(samples.Count>5&&samples[0]==0&&samples[samples.Count-1]==1,"real progress spans start through readiness");
                Check(captured,"captured live loading cover");Check(!view.IsVisible&&!view.GetComponent<CanvasGroup>().blocksRaycasts,"ready match releases cover and input");
            }
            finally{view.ProgressChanged-=observe;File.WriteAllLines("scratch/LoadingProgress.txt",trace);}
            for(int i=0;i<3;i++)await Task.Yield();
            Check(await app.Flow.ReturnToHubAsync(),"return navigation resets and completes loading");
            view.ShowError("Không thể mở ải. Đã trở về sảnh, bạn có thể thử lại.");
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("scratch/LoadingError.png"));for(int i=0;i<3;i++)await Task.Yield();
            var button=(Button)new SerializedObject(view).FindProperty("_dismiss").objectReferenceValue;
            Check(button.gameObject.activeSelf,"recovered error offers dismissal");button.onClick.Invoke();Check(!view.IsVisible&&!view.GetComponent<CanvasGroup>().blocksRaycasts,"error dismissal releases input");
            File.WriteAllText("scratch/LoadingSmoke.txt","Passed: live menu-to-match progress, monotonic work samples, display never ahead, input cover, no premature 100%, ready-match dismissal, return to hub, simulated recovered-error dismissal.\nCaptures: scratch/LoadingTransition.png, scratch/LoadingError.png\nTrace: scratch/LoadingProgress.txt");
        }
        catch(Exception exception){File.WriteAllText("scratch/LoadingSmoke.txt","Failed: "+exception);Debug.LogError(exception);}
        finally{SessionState.SetBool(Key,false);EditorApplication.isPlaying=false;}
    }
    private static void Timeout(double deadline){if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Loading validation timed out.");}
    private static void Check(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
}
#endif
