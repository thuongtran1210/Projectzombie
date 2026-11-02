#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectZombie.Core.Architecture;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.Loadout;

[InitializeOnLoad]
public static class DefenseSoulPickupSmoke
{
    private const string Key="ProjectZombie.SoulPickupSmoke";
    static DefenseSoulPickupSmoke()
    {
        EditorApplication.playModeStateChanged+=state=>
        {
            if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Key,false))EditorApplication.delayCall+=Run;
            if(state==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key+".Restore",false))
            {
                string path=SessionState.GetString(Key+".Previous","");EditorSceneManager.playModeStartScene=string.IsNullOrEmpty(path)?null:AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                SessionState.SetBool(Key+".Restore",false);
            }
        };
    }
    [MenuItem("Tools/ProjectZombie/VFX/Run Soul Pickup Smoke")]
    public static void Start()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Requires Edit Mode.");
        for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Save scene edits first.");
        var old=EditorSceneManager.playModeStartScene;SessionState.SetString(Key+".Previous",old!=null?AssetDatabase.GetAssetPath(old):"");
        SessionState.SetBool(Key+".Restore",true);SessionState.SetBool(Key,true);
        EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(AppArchitectureInstaller.BOOT);
        Directory.CreateDirectory("scratch");File.WriteAllText("scratch/SoulPickupSmoke.txt","Running...");EditorApplication.isPlaying=true;
    }
    private static async void Run()
    {
        double deadline=EditorApplication.timeSinceStartup+90;
        try
        {
            while(AppCompositionRoot.Instance==null||AppCompositionRoot.Instance.Flow.IsNavigating){Limit(deadline);await Task.Yield();}
            var app=AppCompositionRoot.Instance;
            Check(await app.Flow.EnterMatchAsync(new MatchLaunchRequest(app.Stages.Stages.First(x=>x.CanLaunchDefense),DefenseLoadoutSnapshot.CreateDefault())),"enter gameplay");
            var composition=UnityEngine.Object.FindObjectOfType<DefenseSceneCompositionRoot>();var session=(DefenseSession)composition.Gameplay;
            var hero=UnityEngine.Object.FindObjectOfType<DefenseHeroHealth3D>();var run=UnityEngine.Object.FindObjectOfType<DefenseRun6003D>();
            var wallet=UnityEngine.Object.FindObjectOfType<DefenseWoodWallet3D>();await Wait(.2f,deadline);
            int before=wallet.Wood;
            var position=hero.transform.position;position.y=.3f;
            var a=DefenseWoodDrop3D.Spawn(position+Vector3.right*2.8f,10,hero.transform,wallet,session,null,run.WoodDrops);
            var b=DefenseWoodDrop3D.Spawn(position+new Vector3(4.8f,0,1),20,hero.transform,wallet,session,null,run.WoodDrops);
            var c=DefenseWoodDrop3D.Spawn(position+new Vector3(5.8f,0,-1),30,hero.transform,wallet,session,null,run.WoodDrops);
            await Wait(.2f,deadline);
            Check(wallet.Wood==before&&a.gameObject.activeSelf,"pickup delay preserves wallet");
            Check(a.GetComponent<MeshRenderer>().enabled&&a.GetComponent<MeshRenderer>().sharedMaterial.shader.name=="ProjectZombie/Defense3D/SoulPickup","new soul renderer active");
            Check(a.GetComponentsInChildren<Light>().Length==0,"no per-pickup realtime light");
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("scratch/SoulPickupGameplay.png"));await Wait(.1f,deadline);
            session.Pause();await Task.Yield();await Task.Yield();Vector3 paused=a.transform.position;
            var block=new MaterialPropertyBlock();a.GetComponent<Renderer>().GetPropertyBlock(block);float clock=block.GetFloat("_AnimTime");
            double pauseEnd=EditorApplication.timeSinceStartup+.35;
            while(EditorApplication.timeSinceStartup<pauseEnd){Limit(deadline);await Task.Yield();}
            a.GetComponent<Renderer>().GetPropertyBlock(block);
            Check(a.transform.position==paused&&Mathf.Abs(block.GetFloat("_AnimTime")-clock)<.04f,"pause freezes movement and soul animation");
            session.Resume();await Wait(1.6f,deadline);
            Check(!a.gameObject.activeSelf&&wallet.Wood==before+10,"magnet collects exact original wood value once");
            Check(b.gameObject.activeSelf&&c.gameObject.activeSelf,"distant souls stay collectible");
            session.Restart();await Task.Yield();await Task.Yield();
            Check(UnityEngine.Object.FindObjectsOfType<DefenseWoodDrop3D>().Length==0,"restart clears active souls");
            Check(await app.Flow.ReturnToHubAsync(),"return to hub");
            File.WriteAllText("scratch/SoulPickupSmoke.txt","Passed: production drop pool in real map; soul renderer; pickup delay; pause freezes visual/magnet; exact +10 wood collection; distant drops persist; restart cleanup; return to hub. Capture: scratch/SoulPickupGameplay.png");
        }
        catch(Exception error){File.WriteAllText("scratch/SoulPickupSmoke.txt","Failed: "+error);Debug.LogException(error);}
        finally{SessionState.SetBool(Key,false);EditorApplication.isPlaying=false;}
    }
    private static async Task Wait(float duration,double deadline){float end=Time.time+duration;while(Time.time<end){Limit(deadline);await Task.Yield();}}
    private static void Limit(double deadline){if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Soul pickup smoke timeout.");}
    private static void Check(bool value,string text){if(!value)throw new InvalidOperationException(text);}
}
#endif
