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
                new[] { Defense3DCatalog.TOWER_CUNG_TIEN, Defense3DCatalog.TOWER_BANG_HA, "tower_hoalong" });
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
            var laserTargetVisual = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Defense3D/DefenseEnemy_Magiap.prefab"), targetObject.transform);
            laserTargetVisual.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false; laserTargetVisual.transform.localPosition = Vector3.zero; laserTargetVisual.SetActive(true);
            var target = targetObject.AddComponent<TowerSmokeTarget>();
            int beforeIceProjectiles = pool.ActiveCount;
            session.Pause();
            Check(!ice.TryFire(target, ice.TowerData.Attack) && target.Damage == 0, "laser cannot deal damage while paused");
            session.Resume();
            var laserCamera = (Camera)new SerializedObject(composition).FindProperty("_combatTextCamera").objectReferenceValue;
            for (int level = 1; level <= 3; level++)
            {
                if (level > 1) Check(builder.TryUpgradeNearest(), "upgrade ice laser tier");
                float previousDamage = target.Damage; int previousSlows = target.Slows;
                Check(ice.TryFire(target, ice.TowerData.Attack), "ice fires real laser pulse");
                Check(Mathf.Abs(target.Damage - previousDamage - ice.EffectiveDamage * level) < .001f && target.Slows == previousSlows + 1, "laser applies per-beam damage and one slow per target");
                var activeBeams = UnityEngine.Object.FindObjectsOfType<DefenseTowerLaserVisual3D>();
                Check(activeBeams.Length == level, "level matches actual visible laser count");
                session.Pause();
                foreach (var beam in activeBeams) Check(beam.GetComponentInParent<DefenseEffectInstance3D>().gameObject.activeInHierarchy, "pause retains frozen beam pulse");
                session.Resume();
                CaptureStructures(laserCamera, ice.transform.position + Vector3.right, "scratch/IceLaserLevel" + level + ".png", 3.5f);
            }
            Check(pool.ActiveCount == beforeIceProjectiles, "ice laser never acquires projectile instances");
            Check(UnityEngine.Object.FindObjectsOfType<ProjectileCore3D>(true).Length == projectileCount, "combat does not create projectiles");
            Check(UnityEngine.Object.FindObjectsOfType<DefenseTowerPrototype3D>(true).Length == towerCount, "combat does not create towers");
            builder.SetAllowedTowers(new[] { Defense3DCatalog.TOWER_CUNG_TIEN, Defense3DCatalog.TOWER_BANG_HA, "tower_hoalong" });
            int fireSocket = socket - 1;
            info.HeroSpawn.position = builder.GetSocket(fireSocket).position;
            Check(builder.TryBuild(fireSocket, "tower_hoalong"), "build fire tower from catalog");
            var fire = builder.GetTower(fireSocket);
            var fireTargetObject = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Defense3D/DefenseEnemy_Magiap.prefab"));
            fireTargetObject.name = "Tower smoke burn target";
            fireTargetObject.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
            fireTargetObject.transform.position = fire.transform.position + Vector3.right * 1.5f;
            var fireTarget = fireTargetObject.GetComponent<DefenseEnemy3D>();
            fireTarget.BindGameplay(session);
            fireTarget.Initialize(new DefenseEnemyStats("test", "test", DefenseEnemyLocomotionType.GroundNavMesh, 1000, 1, 1, 0, 0));
            fireTarget.BindTargets(session.TargetRegistry);
            fireTargetObject.SetActive(true);
            Check(fire.TryFire(fireTarget, fire.TowerData.Attack), "fire launches separate shell");
            var fireProjectile = UnityEngine.Object.FindObjectsOfType<ProjectileCore3D>().First(x => x.IsInFlight && x.name.StartsWith("P_Proj_Fireball_HoaLong"));
            session.Pause(); fireProjectile.Tick(100);
            Check(fireTarget.CurrentHP == 1000, "fire explosion pauses with session");
            session.Resume(); fireProjectile.Tick(100);
            Check(fireTarget.CurrentHP < 1000 && fireTarget.Status.IsBurning, "fire explosion applies burn to real actor");
            float beforeBurn = fireTarget.CurrentHP;
            session.Tick(.5f, 1000, false);
            typeof(DefenseEnemyStatus3D).GetMethod("Update", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(fireTarget.Status, null);
            Check(fireTarget.CurrentHP < beforeBurn, "real actor receives timed burn damage");
            int archerSocket = socket - 2;
            info.HeroSpawn.position = builder.GetSocket(archerSocket).position;
            Check(builder.TryBuild(archerSocket, Defense3DCatalog.TOWER_CUNG_TIEN), "rebuild archer for structure presentation");
            Check(builder.TryUpgradeNearest() && builder.TryUpgradeNearest(), "activate prewarmed tier-three model");
            var masterArcher = builder.GetTower(archerSocket);
            targetObject.transform.position = masterArcher.transform.position + Vector3.right * 2;
            for (int i = 0; i < pool.CapacityPerType; i++) Check(masterArcher.TryFire(target, masterArcher.TowerData.Attack), "fill arrow projectile budget");
            float damage = target.Damage;
            Check(!masterArcher.TryFire(target, masterArcher.TowerData.Attack) && target.Damage == damage, "exhausted arrow pool does not become instant damage");
            foreach (var built in new[] { masterArcher, ice, fire })
            {
                var hp = built.GetComponent<DefenseSupportTarget3D>();
                Check(hp.TakeDamage(new DefenseDamageContext(hp.MaxHP * .3f)), "structure accepts real damage");
                var bar = built.transform.Find("StructureHealthBar").GetComponent<MeshRenderer>();
                var properties = new MaterialPropertyBlock(); bar.GetPropertyBlock(properties);
                Check(Mathf.Abs(properties.GetFloat("_Ratio") - .7f) < .001f, "structure bar follows real HP after build/upgrade");
            }
            var cannons = UnityEngine.Object.FindObjectsOfType<DefenseGhostCannon3D>();
            Check(cannons.Length == 3, "three authored enemy launchers remain wired");
            var hostile = cannons[0];
            Check(hostile.TakeDamage(new DefenseDamageContext(hostile.MaxHP * .45f)), "enemy launcher receives damage");
            var hostileData = new SerializedObject(hostile);
            var hostileModel = (GameObject)hostileData.FindProperty("_modelRoot").objectReferenceValue;
            var sceneCamera = (Camera)new SerializedObject(composition).FindProperty("_combatTextCamera").objectReferenceValue;
            CaptureStructures(sceneCamera, (ice.transform.position + fire.transform.position + masterArcher.transform.position) / 3f, "scratch/StructureCombat.png", 9f);
            CaptureStructures(sceneCamera, hostile.transform.position + Vector3.up * .2f, "scratch/EnemyLauncherCombat.png", 3.5f);
            Check(hostile.TakeDamage(new DefenseDamageContext(99999)), "enemy launcher can be destroyed");
            Check(!hostileModel.activeSelf && !hostile.IsTargetable, "destroyed launcher hides all model meshes and leaves targeting");
            Check(!hostile.transform.Find("StructureHealthBar").GetComponent<MeshRenderer>().enabled, "destroyed launcher hides health bar");
            ScreenCapture.CaptureScreenshot(Path.GetFullPath("scratch/TowerFireCombat.png"));
            for (int frame = 0; frame < 3; frame++) await Task.Yield();
            session.Restart();
            for (int i = 0; i < builder.SocketCount; i++) Check(builder.GetTower(i) == null, "restart clears socket " + i);
            Check(pool.ActiveCount == 0 && !menu.IsOpen, "restart recalls projectiles and closes menu");
            Check(!fireTarget.Status.IsBurning, "restart clears burn");
            var effectService = UnityEngine.Object.FindObjectOfType<DefenseEffectService3D>();
            Check(effectService.ActiveCount == 0, "restart clears tower effects including attached burn");
            Check(hostileModel.activeSelf && hostile.CurrentHP == hostile.MaxHP && hostile.IsTargetable, "restart restores enemy launcher model, health and target registry");
            UnityEngine.Object.Destroy(fireTargetObject);
            UnityEngine.Object.Destroy(targetObject);
            Check(await AppCompositionRoot.Instance.Flow.ReturnToHubAsync(), "return to hub");
            File.WriteAllText("scratch/TowerSmoke.txt", "Passed: loadout/wood/occupancy guards; UI builds archer; upgrade; destruction/rebuild; ice laser 1/2/3 actual beam counts, per-beam damage, one slow per target, pause retention and no projectile acquisition; bounded arrow pool; fire explosion/burn; tier-three model; real tower health bars; enemy launcher damage/destruction/model hiding; restart restores launcher and clears towers/projectiles/status/VFX; return to hub. Captures: scratch/IceLaserLevel1.png, scratch/IceLaserLevel2.png, scratch/IceLaserLevel3.png, scratch/StructureCombat.png, scratch/EnemyLauncherCombat.png");
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
    private static void CaptureStructures(Camera camera, Vector3 focus, string path, float size)
    {
        var position = camera.transform.position; var rotation = camera.transform.rotation;
            bool oldOrthographic = camera.orthographic;
        float oldSize = camera.orthographicSize; var oldTarget = camera.targetTexture; var oldActive = RenderTexture.active;
        var target = new RenderTexture(1280, 800, 24); var texture = new Texture2D(1280, 800, TextureFormat.RGB24, false);
        try
        {
            camera.transform.position = focus + new Vector3(5,8,-10); camera.transform.LookAt(focus + Vector3.up);
            camera.orthographic = true; camera.orthographicSize = size; camera.targetTexture = target;
            foreach (var presenter in UnityEngine.Object.FindObjectsOfType<DefenseStructureHealthPresenter3D>())
                typeof(DefenseStructureHealthPresenter3D).GetMethod("LateUpdate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(presenter, null);
            camera.Render(); RenderTexture.active = target;
            texture.ReadPixels(new Rect(0,0,1280,800),0,0); texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG());
        }
        finally
        {
            camera.transform.SetPositionAndRotation(position, rotation); camera.orthographicSize = oldSize; camera.targetTexture = oldTarget;
            camera.orthographic = oldOrthographic;
            RenderTexture.active = oldActive; target.Release(); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(texture);
        }
    }
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
#endif
