#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectZombie.Core.Architecture;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.Loadout;
using ProjectZombie.Features.Defense3D.UI;

[InitializeOnLoad]
public static class DefenseThunderSmoke
{
    private const string KEY="ProjectZombie.ThunderSmoke";
    static DefenseThunderSmoke()
    {
        EditorApplication.playModeStateChanged+=state=>
        {
            if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(KEY,false))EditorApplication.delayCall+=Run;
            if(state==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(KEY+".Restore",false))
            {string previous=SessionState.GetString(KEY+".Previous","");EditorSceneManager.playModeStartScene=string.IsNullOrEmpty(previous)?null:AssetDatabase.LoadAssetAtPath<SceneAsset>(previous);SessionState.SetBool(KEY+".Restore",false);}
        };
    }
    [MenuItem("Tools/VFX Generator/Run Heavenly Lightning Smoke")]
    public static void Start()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Requires Edit Mode.");
        for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Save scene edits before Play validation.");
        var previous=EditorSceneManager.playModeStartScene;SessionState.SetString(KEY+".Previous",previous!=null?AssetDatabase.GetAssetPath(previous):"");SessionState.SetBool(KEY+".Restore",true);SessionState.SetBool(KEY,true);
        EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(AppArchitectureInstaller.BOOT);Directory.CreateDirectory("scratch");File.WriteAllText("scratch/ThunderSmoke.txt","Running...");EditorApplication.isPlaying=true;
    }
    private static async void Run()
    {
        try
        {
            double deadline=EditorApplication.timeSinceStartup+90;
            while(AppCompositionRoot.Instance==null||AppCompositionRoot.Instance.Flow.IsNavigating){Timeout(deadline);await Task.Yield();}
            var app=AppCompositionRoot.Instance;var defaults=DefenseLoadoutSnapshot.CreateDefault();
            var loadout=new DefenseLoadoutSnapshot(Defense3DCatalog.HERO_DAO_SI,defaults.RelicId,defaults.MinionIds,defaults.TowerIds);
            Check(await app.Flow.EnterMatchAsync(new MatchLaunchRequest(app.Stages.Stages.First(x=>x.CanLaunchDefense),loadout)),"Đạo Sĩ enters the real map");
            var composition=UnityEngine.Object.FindObjectOfType<DefenseSceneCompositionRoot>();Check(composition!=null&&composition.IsReady,"map composition is ready");
            var session=(DefenseSession)composition.Gameplay;
            var button=UnityEngine.Object.FindObjectsOfType<DefenseSkillPresenter3D>().First(x=>x.Effect is DefenseLightningSignatureEffect3D);
            var skill=(DefenseLightningSignatureEffect3D)button.Effect;var hero=skill.GetComponent<DefenseHeroHealth3D>();var motor=skill.GetComponent<DefenseHeroMotor3D>();
            Check(skill.GetComponent<DefenseLightningSignaturePresenter3D>().IsBound,"lightning presentation is bound");
            var attack=new SerializedObject(skill.GetComponent<DefenseHeroAutoAttack3D>());attack.FindProperty("_autoAttackEnabled").boolValue=false;attack.ApplyModifiedPropertiesWithoutUndo();
            for(int i=0;i<10;i++){Timeout(deadline);await Task.Yield();}
            // Let the authored Rigidbody settle, then place targets around its actual position.
            var origin=hero.transform.position;origin.y=.15f;
            var a=Enemy(session,origin+Vector3.right*2);var b=Enemy(session,origin+new Vector3(3,0,1));var outside=Enemy(session,origin+Vector3.right*9);
            int released=0,hits=0;bool captured=false;
            Vector3 aimed=Vector3.zero;string ended="";
            skill.Feedback.Started+=cue=>aimed=new Vector3(cue.Position.X,cue.Position.Y,cue.Position.Z);
            skill.Feedback.Ended+=cue=>ended=cue.Completed?"completed":"cancelled";
            skill.Feedback.Released+=cue=>{released++;if(!captured){ScreenCapture.CaptureScreenshot(Path.GetFullPath("scratch/ThunderCombat.png"));captured=true;}};
            skill.Feedback.HitConfirmed+=cue=>hits++;
            Check(button.TryQuickCast(),"real signature button casts lightning");
            Check(skill.IsCasting&&a.CurrentHP==1000,"windup precedes damage");
            Check(UnityEngine.Object.FindObjectsOfType<DefenseHeavenlyLightningVisual3D>().Length>0,"targeted cast seal is visible");
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("scratch/ThunderWindup.png"));
            while(skill.IsCasting){Timeout(deadline);await Task.Yield();}
            Check(released==1&&hits>=2,"one release and confirmed area hits: released="+released+", hits="+hits+", ended="+ended+", phase="+session.Phase+", hero="+hero.transform.position+", aim="+aimed+", a="+a.transform.position+"/"+a.CurrentHP+", b="+b.transform.position+"/"+b.CurrentHP);
            Check(a.CurrentHP<1000&&b.CurrentHP<1000&&outside.CurrentHP==1000,"damage stays within strike radius");
            Check(a.Status.BlocksActions&&a.Status.MovementMultiplier<1,"stun and slow remain active");
            for(int i=0;i<3;i++)await Task.Yield();
            session.Pause();Check(UnityEngine.Object.FindObjectsOfType<DefenseHeavenlyLightningVisual3D>().Length==0,"pause clears owned lightning handles");session.Resume();
            session.Restart();int before=released;float hp=a.CurrentHP;
            for(int i=0;i<2;i++)await Task.Yield();
            Check(button.TryQuickCast(),"signature can cast again after restart");
            session.Pause();Check(!skill.IsCasting,"pause interrupts windup immediately");session.Resume();
            session.Tick(.5f,1000,false);for(int i=0;i<3;i++)await Task.Yield();
            Check(released==before&&a.CurrentHP==hp,"interrupted windup cannot release stale damage");
            session.Restart();for(int i=0;i<2;i++)await Task.Yield();Check(button.TryQuickCast(),"new cast after interruption");
            hero.TakeDamage(new DefenseDamageContext(99999,sessionIdentity:session.SessionIdentity,runGeneration:session.RunGeneration));
            Check(!skill.IsCasting&&UnityEngine.Object.FindObjectsOfType<DefenseHeavenlyLightningVisual3D>().Length==0,"hero death clears windup and effects");
            session.Restart();Check(UnityEngine.Object.FindObjectsOfType<DefenseHeavenlyLightningVisual3D>().Length==0,"restart leaves no old effects");
            UnityEngine.Object.Destroy(a.gameObject);UnityEngine.Object.Destroy(b.gameObject);UnityEngine.Object.Destroy(outside.gameObject);
            Check(await app.Flow.ReturnToHubAsync(),"return to hub");
            File.WriteAllText("scratch/ThunderSmoke.txt","Passed: Đạo Sĩ enters gameplay; real skill-button cast; targeted windup; one pooled release; AoE damage/confirmed hits; preserved stun/slow; pause/death interruption; restart cleanup; return to hub. Captures: scratch/ThunderWindup.png, scratch/ThunderCombat.png");
        }
        catch(Exception exception){File.WriteAllText("scratch/ThunderSmoke.txt","Failed: "+exception);Debug.LogError(exception);}
        finally{SessionState.SetBool(KEY,false);EditorApplication.isPlaying=false;}
    }
    private static DefenseEnemy3D Enemy(DefenseSession session,Vector3 position)
    {
        var obj=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Defense3D/DefenseEnemy_Magiap.prefab"));obj.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled=false;obj.transform.position=position;
        var enemy=obj.GetComponent<DefenseEnemy3D>();enemy.BindGameplay(session);enemy.Initialize(new DefenseEnemyStats("thunder-test","thunder-test",DefenseEnemyLocomotionType.GroundNavMesh,1000,1,1,0,0));enemy.BindTargets(session.TargetRegistry);obj.SetActive(true);return enemy;
    }
    private static void Timeout(double deadline){if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("Thunder smoke timed out.");}
    private static void Check(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
}
#endif
