#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ProjectZombie.Core.Architecture;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.Loadout;
using ProjectZombie.Features.Defense3D.UI;

/// <summary>Exercises the real socket/build UI, two tower types, projectile gate and reset in Play Mode.</summary>
[InitializeOnLoad]
public static class DefenseTowerSmoke
{
    private const string KEY = "ProjectZombie.TowerSmoke";
    static DefenseTowerSmoke()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(KEY, false)) EditorApplication.delayCall += Run;
            if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(KEY + ".Restore", false))
            {
                string previous = SessionState.GetString(KEY + ".Previous", "");
                EditorSceneManager.playModeStartScene = string.IsNullOrEmpty(previous) ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(previous);
                SessionState.SetBool(KEY + ".Restore", false);
            }
        };
    }
    [MenuItem("Tools/ProjectZombie/Towers/Run Tower Smoke")]
    public static void Start()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Requires Edit Mode.");
        var previous = EditorSceneManager.playModeStartScene;
        SessionState.SetString(KEY + ".Previous", previous != null ? AssetDatabase.GetAssetPath(previous) : "");
        SessionState.SetBool(KEY + ".Restore", true);
        SessionState.SetBool(KEY, true);
        EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(AppArchitectureInstaller.BOOT);
        Directory.CreateDirectory("scratch");
        File.WriteAllText("scratch/TowerSmoke.txt", "Running...");
        EditorApplication.isPlaying = true;
    }
    private static async void Run()
    {
        try
        {
            double deadline = EditorApplication.timeSinceStartup + 90;
            while (AppCompositionRoot.Instance == null || AppCompositionRoot.Instance.Flow.IsNavigating) await Task.Yield();
            var app = AppCompositionRoot.Instance;
            var defaults = DefenseLoadoutSnapshot.CreateDefault();
            var loadout = new DefenseLoadoutSnapshot(defaults.HeroId, defaults.RelicId, defaults.MinionIds,
                new[] { Defense3DCatalog.TOWER_CUNG_TIEN, Defense3DCatalog.TOWER_BANG_HA });
            var stage = app.Stages.Stages.First(x => x.CanLaunchDefense);
            Check(await app.Flow.EnterMatchAsync(new MatchLaunchRequest(stage, loadout)), "enter map with both tower types");
            DefenseSceneCompositionRoot composition;
            while ((composition = UnityEngine.Object.FindObjectOfType<DefenseSceneCompositionRoot>()) == null || !composition.IsReady)
            {
                if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Map failed to initialize.");
                await Task.Yield();
            }
            var session = (DefenseSession)composition.Gameplay;
            var builder = UnityEngine.Object.FindObjectOfType<DefenseTowerBuildPrototype3D>();
            var info = UnityEngine.Object.FindObjectOfType<DefenseSceneInfo3D>();
            var wallet = info.HeroSpawn.GetComponent<DefenseWoodWallet3D>();
            var pool = UnityEngine.Object.FindObjectOfType<DefenseProjectilePool3D>();
            var menu = UnityEngine.Object.FindObjectOfType<DefenseTowerBuildView>();
            int towerCount = UnityEngine.Object.FindObjectsOfType<DefenseTowerPrototype3D>(true).Length;
            int projectileCount = UnityEngine.Object.FindObjectsOfType<ProjectileCore3D>(true).Length;
            int socket = builder.SocketCount - 1;
            info.HeroSpawn.position = builder.GetSocket(socket).position;
            builder.SetAllowedTowers(new[] { Defense3DCatalog.TOWER_CUNG_TIEN });
            int before = wallet.Wood;
            Check(!builder.TryBuild(socket, Defense3DCatalog.TOWER_BANG_HA) && wallet.Wood == before, "loadout rejects ice without spending");
            wallet.TrySpend(wallet.Wood);
            Check(!builder.TryBuild(socket, Defense3DCatalog.TOWER_CUNG_TIEN) && wallet.Wood == 0, "insufficient wood is atomic");
            wallet.CollectWood(1000);
            builder.RequestBuildNearest();
            Check(menu.IsOpen, "build prompt opens menu");
            var button = (UnityEngine.UI.Button)new SerializedObject(menu).FindProperty("_choices").GetArrayElementAtIndex(0).objectReferenceValue;
            button.onClick.Invoke();
            var archer = builder.GetTower(socket);
            Check(archer != null && archer.TowerData.Id == Defense3DCatalog.TOWER_CUNG_TIEN && !menu.IsOpen, "menu builds archer");
            before = wallet.Wood;
            Check(!builder.TryBuild(socket, Defense3DCatalog.TOWER_CUNG_TIEN) && wallet.Wood == before, "occupied socket rejects duplicate");
            Check(builder.TryUpgradeNearest(), "upgrade uses selected tower stats");
            Check(archer.Level == 2, "upgrade reaches level 2");
            archer.GetComponent<DefenseSupportTarget3D>().TakeDamage(new DefenseDamageContext(99999, sessionIdentity: session.SessionIdentity, runGeneration: session.RunGeneration));
            Check(builder.GetTower(socket) == null, "destruction frees socket");
            builder.SetAllowedTowers(new[] { Defense3DCatalog.TOWER_CUNG_TIEN, Defense3DCatalog.TOWER_BANG_HA });
            builder.RequestBuildNearest();
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("scratch/TowerBuildMenu.png"));
            for (int frame = 0; frame < 3; frame++) await Task.Yield();
            info.HeroSpawn.position = builder.GetSocket(socket).position;
            Check(builder.TryBuild(socket, Defense3DCatalog.TOWER_BANG_HA), "same socket builds ice");
            var ice = builder.GetTower(socket);
            var targetObject = new GameObject("Tower smoke target");
            targetObject.transform.position = ice.transform.position + Vector3.right * 2;
            var target = targetObject.AddComponent<TowerSmokeTarget>();
            Check(ice.TryFire(target, ice.TowerData.Attack), "ice launches configured projectile");
            var projectile = UnityEngine.Object.FindObjectsOfType<ProjectileCore3D>().First(x => x.IsInFlight);
            session.Pause();
            float progress = projectile.Progress;
            projectile.Tick(100);
            Check(projectile.Progress == progress && target.Damage == 0, "projectile pauses with session");
            session.Resume();
            projectile.Tick(100);
            Check(target.Damage > 0 && target.Slows == 1, "ice hit applies damage and slow once");
            for (int i = 0; i < pool.CapacityPerType; i++) Check(ice.TryFire(target, ice.TowerData.Attack), "fill projectile budget");
            float damage = target.Damage;
            Check(!ice.TryFire(target, ice.TowerData.Attack) && target.Damage == damage, "exhausted pool does not become instant damage");
            Check(UnityEngine.Object.FindObjectsOfType<ProjectileCore3D>(true).Length == projectileCount, "combat does not create projectiles");
            Check(UnityEngine.Object.FindObjectsOfType<DefenseTowerPrototype3D>(true).Length == towerCount, "combat does not create towers");
            session.Restart();
            for (int i = 0; i < builder.SocketCount; i++) Check(builder.GetTower(i) == null, "restart clears socket " + i);
            Check(pool.ActiveCount == 0 && !menu.IsOpen, "restart recalls projectiles and closes menu");
            UnityEngine.Object.Destroy(targetObject);
            Check(await AppCompositionRoot.Instance.Flow.ReturnToHubAsync(), "return to hub");
            File.WriteAllText("scratch/TowerSmoke.txt", "Passed: loadout/wood/occupancy guards; UI builds archer; upgrade; destruction; same socket builds ice; pause; one slow per hit; bounded pools; restart cleanup; return to hub.");
        }
        catch (Exception exception)
        {
            File.WriteAllText("scratch/TowerSmoke.txt", "Failed: " + exception);
            Debug.LogError("[Tower Smoke] " + exception);
        }
        finally
        {
            SessionState.SetBool(KEY, false);
            EditorApplication.isPlaying = false;
        }
    }
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
#endif
