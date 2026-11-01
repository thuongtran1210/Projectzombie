#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using ProjectZombie.Core.Architecture;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.Loadout;
using ProjectZombie.Features.Defense3D.UI;

/// <summary>Runs a four-type production roster through real UI events, recruitment, independent orders and restart.</summary>
[InitializeOnLoad]
public static class DefenseMinionRosterPlayCheck
{
    private const string KEY = "ProjectZombie.MinionRosterPlayCheck";
    static DefenseMinionRosterPlayCheck()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(KEY, false)) EditorApplication.delayCall += Run;
            if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(KEY + ".Restore", false))
            {
                string path = SessionState.GetString(KEY + ".Previous", "");
                EditorSceneManager.playModeStartScene = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                SessionState.SetBool(KEY + ".Restore", false);
            }
        };
    }

    [MenuItem("Tools/ProjectZombie/Minions/Run Roster Match Check")]
    public static void Start()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Requires Edit Mode.");
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save dirty scenes before the match check.");
        var previous = EditorSceneManager.playModeStartScene;
        SessionState.SetString(KEY + ".Previous", previous != null ? AssetDatabase.GetAssetPath(previous) : "");
        SessionState.SetBool(KEY + ".Restore", true); SessionState.SetBool(KEY, true);
        EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(AppArchitectureInstaller.BOOT);
        Directory.CreateDirectory("scratch"); File.WriteAllText("scratch/MinionRosterMatchCheck.txt", "Running...");
        EditorApplication.isPlaying = true;
    }

    private static async void Run()
    {
        try
        {
            double deadline = EditorApplication.timeSinceStartup + 120;
            while (AppCompositionRoot.Instance == null || AppCompositionRoot.Instance.Flow.IsNavigating)
            {
                Check(EditorApplication.timeSinceStartup < deadline, "Bootstrap timeout."); await Task.Yield();
            }
            var app = AppCompositionRoot.Instance;
            string[] ids = { Defense3DCatalog.MINION_DAN_BINH, Defense3DCatalog.MINION_PHU_TAP_DICH, Defense3DCatalog.MINION_THAY_LANG, Defense3DCatalog.MINION_XA_THU };
            var loadout = new DefenseLoadoutSnapshot(Defense3DCatalog.HERO_DAO_SI, Defense3DCatalog.RELIC_DIEU_CAY, ids, new[] { Defense3DCatalog.TOWER_CUNG_TIEN });
            Check(await app.Flow.EnterMatchAsync(new MatchLaunchRequest(app.Stages.Stages.First(stage => stage.CanLaunchDefense), loadout)), "Enter production match.");
            var composition = UnityEngine.Object.FindObjectOfType<DefenseSceneCompositionRoot>();
            Check(composition != null && composition.IsReady, "Composition ready.");
            var shop = UnityEngine.Object.FindObjectOfType<DefenseVillageShop3D>();
            var view = UnityEngine.Object.FindObjectOfType<DefenseVillageShopView>();
            var session = (DefenseSession)composition.Gameplay;
            Check(shop.Roster.Squads.Count == ids.Length, "All four equipped types survive loadout resolution.");
            await Task.Yield(); await Task.Yield();
            Check(view.SquadRowCount == ids.Length, "Four recruit and command rows authored dynamically.");
            Vector3 position = shop.RecruitmentPosition + Vector3.right * .3f;
            shop.Hero.transform.position = position;
            if (shop.Hero.TryGetComponent<Rigidbody>(out var body)) body.position = position;
            shop.Wallet.CollectWood(200);
            await Task.Yield(); await Task.Yield();
            Check(shop.CanRecruit, "Hero can recruit near the hut.");
            view.ToggleRecruitment();
            await Capture("MinionRosterRecruitment.png");
            var recruits = Rows(view, "_recruitRows");
            var commands = Rows(view, "_commandRows");
            int initialActors = UnityEngine.Object.FindObjectsOfType<DefenseMinion3D>(true).Length;
            foreach (string id in ids)
            {
                int wood = shop.Wallet.Wood;
                recruits[id].GetComponent<Button>().onClick.Invoke();
                Check(shop.Count(id) == 1, "UI recruits its exact type: " + id);
                Check(shop.Roster.TryGetSquad(id, out var squad) && shop.Wallet.Wood == wood - squad.Definition.WoodCost, "One successful payment: " + id);
            }
            commands[Defense3DCatalog.MINION_XA_THU].GetComponentInChildren<Button>().onClick.Invoke();
            Check(shop.Order(Defense3DCatalog.MINION_XA_THU) == DefenseMinionOrder.Follow, "Archer order changes.");
            Check(shop.Order(Defense3DCatalog.MINION_DAN_BINH) == DefenseMinionOrder.Defend, "Militia order remains independent.");
            var archer = UnityEngine.Object.FindObjectsOfType<DefenseMinion3D>().Single(unit => unit.Definition.Id == Defense3DCatalog.MINION_XA_THU);
            Check(archer.Health.TakeDamage(new DefenseDamageContext(100000f, shop.Hero, archer.transform.position,
                runGeneration: session.RunGeneration, sessionIdentity: session.SessionIdentity)), "Archer receives lethal damage.");
            Check(shop.Count(Defense3DCatalog.MINION_XA_THU) == 0 && shop.Count(Defense3DCatalog.MINION_DAN_BINH) == 1, "Defeated actor returns only to its own squad pool.");
            recruits[Defense3DCatalog.MINION_XA_THU].GetComponent<Button>().onClick.Invoke();
            Check(shop.Count(Defense3DCatalog.MINION_XA_THU) == 1, "Returned actor can be recruited again.");
            Check(UnityEngine.Object.FindObjectsOfType<DefenseMinion3D>(true).Length == initialActors, "Defeat/recruit cycle does not construct new actors.");
            await Capture("MinionRosterCommands.png");
            session.Pause(); int pausedWood = shop.Wallet.Wood;
            recruits[ids[0]].GetComponent<Button>().onClick.Invoke();
            Check(shop.Wallet.Wood == pausedWood && shop.Count(ids[0]) == 1, "Paused clicks do not purchase.");
            session.Resume();
            int generation = session.RunGeneration; session.Restart();
            await Task.Yield(); await Task.Yield();
            Check(session.RunGeneration > generation, "Restart advances generation.");
            foreach (string id in ids) Check(shop.Count(id) == 0 && shop.Order(id) == DefenseMinionOrder.Defend, "Restart clears actors/orders: " + id);
            Check(view.SquadRowCount == ids.Length, "Restart keeps one row pair per type.");
            Check(UnityEngine.Object.FindObjectsOfType<DefenseMinion3D>(true).Length == initialActors, "Actor pool is reused across restart.");
            Check(await app.Flow.ReturnToHubAsync(), "Return to hub.");
            Check(UnityEngine.Object.FindObjectsOfType<DefenseMinion3D>(true).Length == 0, "Session scene unload releases all actors.");
            File.WriteAllText("scratch/MinionRosterMatchCheck.txt", "Passed: four equipped types; four recruit/command rows; real Button events recruit all four types with exact wood charges; independent archer order; defeated archer returned/recruited through native ObjectPool without creating actors; paused click blocked; restart resets counts/orders and reuses actor pool; hub return releases actors. No rewards saved. Physical touch input and Android performance not measured.");
        }
        catch (Exception exception) { File.WriteAllText("scratch/MinionRosterMatchCheck.txt", "Failed: " + exception); Debug.LogException(exception); }
        finally { SessionState.SetBool(KEY, false); EditorApplication.isPlaying = false; }
    }

    private static Dictionary<string, DefenseMinionSquadRowView> Rows(DefenseVillageShopView view, string field)
        => (Dictionary<string, DefenseMinionSquadRowView>)typeof(DefenseVillageShopView).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(view);
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static async Task Capture(string filename)
    {
        await Task.Yield(); string path = Path.GetFullPath("scratch/" + filename);
        if (File.Exists(path)) File.Delete(path); ScreenCapture.CaptureScreenshot(path);
        double deadline = EditorApplication.timeSinceStartup + 8;
        while (!File.Exists(path)) { Check(EditorApplication.timeSinceStartup < deadline, "Capture timeout."); await Task.Yield(); }
    }
}
#endif
