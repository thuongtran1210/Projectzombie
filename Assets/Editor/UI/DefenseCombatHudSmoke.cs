#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ProjectZombie.Core.Architecture;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.Loadout;
using ProjectZombie.Features.Defense3D.UI;

[InitializeOnLoad]
public static class DefenseCombatHudSmoke
{
    private const string Key="ProjectZombie.CombatHudSmoke";
    static DefenseCombatHudSmoke()
    {
        EditorApplication.playModeStateChanged+=state=>
        {
            if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Key,false))EditorApplication.delayCall+=Run;
            if(state==PlayModeStateChange.EnteredEditMode&&SessionState.GetBool(Key+".Restore",false))
            {
                string old=SessionState.GetString(Key+".Previous","");EditorSceneManager.playModeStartScene=string.IsNullOrEmpty(old)?null:AssetDatabase.LoadAssetAtPath<SceneAsset>(old);
                SessionState.SetBool(Key+".Restore",false);
            }
        };
    }
    [MenuItem("Tools/ProjectZombie/UI/Run Compact HUD Smoke")]
    public static void Start()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Requires Edit Mode.");
        for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Save scene edits first.");
        var old=EditorSceneManager.playModeStartScene;SessionState.SetString(Key+".Previous",old!=null?AssetDatabase.GetAssetPath(old):"");
        SessionState.SetBool(Key+".Restore",true);SessionState.SetBool(Key,true);
        EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(AppArchitectureInstaller.BOOT);
        Directory.CreateDirectory("scratch");File.WriteAllText("scratch/CombatHudSmoke.txt","Running...");EditorApplication.isPlaying=true;
    }
    private static async void Run()
    {
        double deadline=EditorApplication.timeSinceStartup+90;
        try
        {
            while(AppCompositionRoot.Instance==null||AppCompositionRoot.Instance.Flow.IsNavigating){TimeLimit(deadline);await Task.Yield();}
            var app=AppCompositionRoot.Instance;var stage=app.Stages.Stages.First(x=>x.CanLaunchDefense);
            Check(await app.Flow.EnterMatchAsync(new MatchLaunchRequest(stage,DefenseLoadoutSnapshot.CreateDefault())),"enter real gameplay");
            await Wait(.8f,deadline);ReviewLayout();
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("scratch/CombatHudGameplay.png"));await Wait(.15f,deadline);
            var presenters=UnityEngine.Object.FindObjectsOfType<DefenseSkillPresenter3D>();
            var signature=presenters.First(x=>x.Effect is DefenseScholarSignatureEffect3D);
            Check(signature.TryQuickCast(),"scholar signature remains usable");await Wait(.1f,deadline);
            var sigData=new SerializedObject(signature);var view=(DefenseSkillButtonView)sigData.FindProperty("_view").objectReferenceValue;
            var viewData=new SerializedObject(view);var fill=(Image)viewData.FindProperty("_cooldownFill").objectReferenceValue;
            Check(fill.fillAmount>0f,"radial cooldown is rendered");
            Check(view.GetComponentsInChildren<TMPro.TMP_Text>().Any(x=>int.TryParse(x.text,out _)),"numeric cooldown remains readable");
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("scratch/CombatHudCooldown.png"));await Wait(.15f,deadline);
            await app.Flow.ReturnToHubAsync();
            var defaults=DefenseLoadoutSnapshot.CreateDefault();
            var loadout=new DefenseLoadoutSnapshot(Defense3DCatalog.HERO_DAO_SI,Defense3DCatalog.RELIC_DIEU_CAY,defaults.MinionIds,defaults.TowerIds);
            Check(await app.Flow.EnterMatchAsync(new MatchLaunchRequest(stage,loadout)),"alternate hero/relic loadout enters gameplay");
            await Wait(.6f,deadline);ReviewLayout();
            var icons=UnityEngine.Object.FindObjectsOfType<DefenseSkillButtonView>().Select(v=>(Image)new SerializedObject(v).FindProperty("_iconImage").objectReferenceValue).ToArray();
            Check(icons.Any(x=>x.sprite.name=="Glyph_Thunder")&&icons.Any(x=>x.sprite.name=="Glyph_Pipe"),"dynamic identities use matching HUD glyphs");
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("scratch/CombatHudAlternate.png"));await Wait(.15f,deadline);
            var lightning=UnityEngine.Object.FindObjectsOfType<DefenseSkillPresenter3D>().First(x=>x.Effect is DefenseLightningSignatureEffect3D);
            Check(lightning.TryQuickCast(),"lightning remains usable");await Wait(.4f,deadline);
            await app.Flow.ReturnToHubAsync();
            File.WriteAllText("scratch/CombatHudSmoke.txt","Passed: real gameplay HUD; four action hit targets >=96 at 1080 height; no action overlap; raycast routes correct; compact squad hit targets; numeric radial cooldown; scholar/lightning casting; dynamic thunder/pipe glyphs; return to hub.\nCaptures: CombatHudGameplay.png, CombatHudCooldown.png, CombatHudAlternate.png");
        }
        catch(Exception error){File.WriteAllText("scratch/CombatHudSmoke.txt","Failed: "+error);Debug.LogException(error);}
        finally{SessionState.SetBool(Key,false);EditorApplication.isPlaying=false;}
    }
    private static void ReviewLayout()
    {
        Canvas.ForceUpdateCanvases();var views=UnityEngine.Object.FindObjectsOfType<DefenseSkillButtonView>();Check(views.Length==4,"four skill controls");
        var rects=new List<Rect>();
        foreach(var view in views)
        {
            var presenter=view.GetComponent<DefenseSkillPresenter3D>();var source=presenter.DefinitionSource;
            var icon=(Image)new SerializedObject(view).FindProperty("_iconImage").objectReferenceValue;
            Check(source!=null&&icon.sprite==source.Presentation.Icon,"HUD reads the resolved definition icon");
            Check(presenter.ResolvedDefinition!=null&&presenter.ResolvedDefinition.Kind==presenter.Kind,"runtime snapshot matches the selected slot");
            var rect=ScreenRect((RectTransform)view.transform);rects.Add(rect);
            Check(rect.xMin>=Screen.safeArea.xMin-1&&rect.xMax<=Screen.safeArea.xMax+1&&rect.yMin>=Screen.safeArea.yMin-1,"action within safe area: "+view.name);
            Check(rect.height*1080/Screen.height>=95,"action touch target: "+view.name);
            Check(view.GetComponent<Image>().color.a<.01f,"hit surface stays visually transparent: "+view.name);
            var pointer=new PointerEventData(EventSystem.current){position=rect.center};var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(pointer,hits);
            Check(hits.Count>0&&hits[0].gameObject==view.gameObject,"unblocked action raycast: "+view.name+" first="+(hits.Count>0?hits[0].gameObject.name:"none"));
        }
        for(int a=0;a<rects.Count;a++)for(int b=a+1;b<rects.Count;b++)Check(!rects[a].Overlaps(rects[b]),"action hit regions do not overlap");
        var rows=UnityEngine.Object.FindObjectsOfType<DefenseMinionSquadRowView>().Where(x=>x.name.Contains("Command")).ToArray();Check(rows.Length>=3,"squad rows visible");
        foreach(var row in rows)
        {
            var button=row.GetComponentInChildren<Button>();Check(ScreenRect((RectTransform)button.transform).height*1080/Screen.height>=95,"squad touch target");
            var image=row.GetComponentsInChildren<Image>().First(x=>x.name=="CommandFace");Check(image.rectTransform.rect.width==48,"compact squad art");
        }
    }
    private static Rect ScreenRect(RectTransform rect)
    {var corners=new Vector3[4];rect.GetWorldCorners(corners);var min=RectTransformUtility.WorldToScreenPoint(null,corners[0]);var max=RectTransformUtility.WorldToScreenPoint(null,corners[2]);return Rect.MinMaxRect(min.x,min.y,max.x,max.y);}
    private static async Task Wait(float duration,double deadline){float end=Time.time+duration;while(Time.time<end){TimeLimit(deadline);await Task.Yield();}}
    private static void TimeLimit(double deadline){if(EditorApplication.timeSinceStartup>deadline)throw new TimeoutException("HUD smoke timeout.");}
    private static void Check(bool value,string text){if(!value)throw new InvalidOperationException(text);}
}
#endif
