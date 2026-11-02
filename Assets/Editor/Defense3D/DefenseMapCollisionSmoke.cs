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
using UnityEngine.Rendering.Universal;

[InitializeOnLoad]
public static class DefenseMapCollisionSmoke
{
    private const string Key = "ProjectZombie.MapCollisionSmoke";
    static DefenseMapCollisionSmoke()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key,false))
                EditorApplication.delayCall += Run;
            if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Key+".Restore",false))
            {
                string previous = SessionState.GetString(Key+".Previous","");
                EditorSceneManager.playModeStartScene = string.IsNullOrEmpty(previous) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(previous);
                SessionState.SetBool(Key+".Restore",false);
            }
        };
    }
    [MenuItem("Tools/ProjectZombie/Run Map Collision Smoke")]
    public static void Start()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Requires Edit Mode.");
        for(int i=0;i<SceneManager.sceneCount;i++) if(SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save scene edits before validation.");
        var previous=EditorSceneManager.playModeStartScene;
        SessionState.SetString(Key+".Previous",previous!=null?AssetDatabase.GetAssetPath(previous):"");
        SessionState.SetBool(Key+".Restore",true); SessionState.SetBool(Key,true);
        EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(AppArchitectureInstaller.BOOT);
        Directory.CreateDirectory("scratch"); File.WriteAllText("scratch/MapCollisionSmoke.txt","Running...");
        EditorApplication.isPlaying=true;
    }
    private sealed class Commands : IDefenseHeroCommands
    {
        public DefensePoint3 Movement {get;set;}
        public bool ConsumeBuild()=>false;
    }
    private static async void Run()
    {
        double deadline=EditorApplication.timeSinceStartup+90;
        try
        {
            while(AppCompositionRoot.Instance==null || AppCompositionRoot.Instance.Flow.IsNavigating) { CheckTime(deadline); await Task.Yield(); }
            var app=AppCompositionRoot.Instance;
            Check(await app.Flow.EnterMatchAsync(new MatchLaunchRequest(app.Stages.Stages.First(x=>x.CanLaunchDefense),DefenseLoadoutSnapshot.CreateDefault())),"enter gameplay");
            var motor=UnityEngine.Object.FindObjectOfType<DefenseHeroMotor3D>();
            var body=motor.GetComponent<Rigidbody>(); var capsule=motor.GetComponent<CapsuleCollider>();
            var commands=new Commands(); motor.BindCommands(commands);
            var ground=new DefenseHeroGroundSafety3D(capsule,LayerMask.GetMask("Ground"));
            var art=GameObject.Find("SongQue_ArtUpgrade");
            if(art!=null)
            {
                Check(art.GetComponentsInChildren<Collider>().Length==0,"art does not introduce collision obstacles");
                Check(Camera.main.GetUniversalAdditionalCameraData().scriptableRenderer is UniversalRenderer,"gameplay uses the 3D Forward renderer");
                await Wait(.8f,deadline);
                ScreenCapture.CaptureScreenshot(Path.GetFullPath("scratch/MapArtGameplay.png"));
                await Wait(.1f,deadline);
            }
            float clearance=Mathf.Max(capsule.height*Mathf.Abs(capsule.transform.lossyScale.y)*.5f,capsule.radius)-capsule.transform.TransformVector(capsule.center).y;
            // Hold real motor movement into each river bank and the four outer map edges.
            Vector3[] starts={new Vector3(3.5f,0,5),new Vector3(9,0,5),new Vector3(-29,0,5),new Vector3(29,0,5),new Vector3(-20,0,19),new Vector3(-20,0,-19)};
            Vector3[] directions={Vector3.right,Vector3.left,Vector3.left,Vector3.right,Vector3.forward,Vector3.back};
            for(int i=0;i<starts.Length;i++)
            {
                Place(body,starts[i],clearance); commands.Movement=default; await Wait(.15f,deadline);
                commands.Movement=Point(directions[i]); await Wait(.65f,deadline); commands.Movement=default;
                Check(ground.HasSupport(body.position),"edge "+i+" retains capsule support at "+body.position);
                Check(body.position.y>-.5f,"edge "+i+" does not fall");
            }
            foreach(float z in new[]{-10.5f,-.5f,10.5f})
            {
                Place(body,new Vector3(3.5f,0,z),clearance); commands.Movement=default; await Wait(.15f,deadline);
                commands.Movement=Point(Vector3.right); await Wait(1.25f,deadline); commands.Movement=default;
                Check(body.position.x>8.3f && ground.HasSupport(body.position),"bridge crossing at "+z+": "+body.position);
                Place(body,new Vector3(6.25f,0,z),clearance); await Wait(.15f,deadline);
                commands.Movement=Point(Vector3.forward); await Wait(.6f,deadline); commands.Movement=default;
                Check(ground.HasSupport(body.position),"bridge side retains support at "+z);
            }
            Place(body,new Vector3(-20,0,5),clearance); await Wait(.2f,deadline);
            Vector3 safe=body.position; body.position=new Vector3(6.25f,-4,5); body.velocity=Vector3.zero;
            await Wait(.4f,deadline);
            Check(Vector3.Distance(body.position,safe)<.5f,"fall recovery returns to previous safe ground");
            await app.Flow.ReturnToHubAsync();
            File.WriteAllText("scratch/MapCollisionSmoke.txt","Passed: real hero motor blocks both river banks and four outer edges; crosses all three bridges; stops at bridge sides; recovers forced fall; returns to hub.");
        }
        catch(Exception error) { File.WriteAllText("scratch/MapCollisionSmoke.txt","Failed: "+error); Debug.LogException(error); }
        finally { SessionState.SetBool(Key,false); EditorApplication.isPlaying=false; }
    }
    private static void Place(Rigidbody body,Vector3 point,float clearance)
    {
        Check(Physics.Raycast(point+Vector3.up*3,Vector3.down,out var hit,6,LayerMask.GetMask("Ground"),QueryTriggerInteraction.Ignore),"fixture ground exists");
        body.position=new Vector3(point.x,hit.point.y+clearance+.01f,point.z); body.velocity=Vector3.zero; Physics.SyncTransforms();
    }
    private static DefensePoint3 Point(Vector3 v)=>new DefensePoint3(v.x,0,v.z);
    private static async Task Wait(float seconds,double deadline)
    {float end=Time.time+seconds;while(Time.time<end){CheckTime(deadline);await Task.Yield();}}
    private static void CheckTime(double deadline){if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Collision smoke timed out.");}
    private static void Check(bool value,string message){if(!value)throw new InvalidOperationException(message);}
}
#endif
