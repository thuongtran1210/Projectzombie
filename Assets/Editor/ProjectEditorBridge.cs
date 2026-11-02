#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Project-local, main-thread command bridge. No network or arbitrary code execution.</summary>
[InitializeOnLoad]
public static class ProjectEditorBridge
{
    private static readonly string ROOT = Path.GetFullPath("Library/ProjectEditorBridge");
    private static double _nextPoll;

    [Serializable]
    private sealed class Request
    {
        public string id;
        public string command;
    }

    [Serializable]
    private sealed class Reply
    {
        public string id;
        public bool ok;
        public string result;
        public string error;
    }

    static ProjectEditorBridge()
    {
        Directory.CreateDirectory(ROOT);
        EditorApplication.update += Poll;
        WriteAtomic(Path.Combine(ROOT, "ready.json"), JsonUtility.ToJson(new Reply
        {
            ok = true,
            result = Application.unityVersion + " | " + DateTime.UtcNow.ToString("O")
        }));
    }

    private static void Poll()
    {
        if (EditorApplication.timeSinceStartup < _nextPoll || EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            return;
        }
        _nextPoll = EditorApplication.timeSinceStartup + 0.5;
        string input = Path.Combine(ROOT, "request.json");
        if (!File.Exists(input))
        {
            return;
        }

        var reply = new Reply();
        try
        {
            var request = JsonUtility.FromJson<Request>(File.ReadAllText(input));
            File.Delete(input);
            if (request == null || !Guid.TryParseExact(request.id, "N", out _))
            {
                throw new ArgumentException("Request id must be a GUID in N format.");
            }
            reply.id = request.id;
            reply.result = Execute(request.command);
            reply.ok = true;
        }
        catch (Exception exception)
        {
            reply.error = exception.ToString();
        }
        WriteAtomic(Path.Combine(ROOT, "response.json"), JsonUtility.ToJson(reply, true));
    }

    private static string Execute(string command)
    {
        switch (command)
        {
            case "clear-validation-layout":
                return DefenseValidationSceneCleanup.ClearDrivenLayoutOnly();
            case "smoke-roster":
                DefenseMinionRosterPlayCheck.Start();
                return "Roster match check scheduled: scratch/MinionRosterMatchCheck.txt";
            case "migrate-roster":
                DefenseMinionRosterMigration.Migrate();
                return File.ReadAllText("scratch/MinionRosterMigration.txt");
            case "smoke-pipe":
                DefensePipeSmokePlayCheck.Start();
                return "Pipe match check scheduled: scratch/PipeSmokeMatchCheck.txt";
            case "build-pipe":
                DefensePipeSmokeBuilder.Build();
                return File.ReadAllText("scratch/PipeSmokeBuild.txt");
            case "preview-pipe":
                DefensePipeSmokePreview.Render();
                return Path.GetFullPath("scratch/PipeSmokePreview.png");
            case "status":
                return "Unity " + Application.unityVersion + "\nPlaying: " + EditorApplication.isPlaying
                    + "\nPaused: " + EditorApplication.isPaused + "\nScene: " + SceneManager.GetActiveScene().path
                    + "\nDirty: " + SceneManager.GetActiveScene().isDirty;
            case "migrate-skill-data":
                DefenseSkillDataMigration.Migrate();
                return File.ReadAllText("scratch/SkillDataMigration.txt");
            case "validate-skill-data":
                DefenseSkillDataMigration.Validate();
                return File.ReadAllText("scratch/SkillDataValidation.txt");
            case "test-skill-data":
                DefenseSkillDataTestRunner.Run();
                return File.ReadAllText("scratch/SkillDataTests.txt");
            case "build-soul-pickup":
                DefenseSoulPickupBuilder.Build();
                return File.ReadAllText("scratch/SoulPickupBuild.txt");
            case "validate-soul-pickup":
                DefenseSoulPickupValidation.Validate();
                return File.ReadAllText("scratch/SoulPickupValidation.txt");
            case "smoke-soul-pickup":
                DefenseSoulPickupSmoke.Start();
                return "Soul pickup Play Mode check scheduled: scratch/SoulPickupSmoke.txt";
            case "build-combat-hud":
                DefenseCombatHudBuilder.Build();
                return File.ReadAllText("scratch/CombatHudBuild.txt");
            case "smoke-combat-hud":
                DefenseCombatHudSmoke.Start();
                return "HUD Play Mode check scheduled: scratch/CombatHudSmoke.txt";
            case "audit-map-art":
                return DefenseMapArtUpgrade.Audit();
            case "build-map-art":
                DefenseMapArtUpgrade.Build();
                return File.ReadAllText("scratch/MapArtBuild.txt");
            case "preview-map-art":
                DefenseMapArtUpgrade.Preview();
                return "Map art previews refreshed; gameplay camera uses Forward renderer.";
            case "install-app-architecture":
                AppArchitectureInstaller.Install();
                return File.ReadAllText("scratch/AppArchitectureInstall.txt");
            case "install-stage01-blueprint":
                ProjectZombie.EditorTools.Defense3D.DefenseStageBlueprintInstaller.CreateOrUpdateStage01Blueprint();
                return "Stage 01 Blueprint & Difficulty Curve installed successfully.";
            case "validate-stage-blueprint":
                var bp = AssetDatabase.LoadAssetAtPath<ProjectZombie.Features.Defense3D.DefenseStageBlueprintSO>("Assets/Resources/Defense3D/Stages/Stage01_SongQue_Blueprint.asset");
                var report = ProjectZombie.Features.Defense3D.DefenseStageValidator.Validate(bp);
                return report.ToFormattedSummary();
            case "install-towers":
                DefenseTowerArchitectureInstaller.Install();
                return File.ReadAllText("scratch/TowerArchitectureInstall.txt");
            case "validate-towers":
                DefenseTowerValidation.Validate();
                return File.ReadAllText("scratch/TowerValidation.txt");
            case "build-tower-ordnance":
                DefenseTowerOrdnanceBuilder.Build();
                return File.ReadAllText("scratch/TowerOrdnanceBuild.txt");
            case "build-structure-art":
                DefenseStructureArtBuilder.Build();
                return File.ReadAllText("scratch/StructureArtBuild.txt");
            case "build-ice-laser":
                DefenseIceLaserBuilder.Build();
                return File.ReadAllText("scratch/IceLaserBuild.txt");
            case "build-loading-ui":
                AppLoadingPresentationBuilder.Build();
                return File.ReadAllText("scratch/LoadingBuild.txt");
            case "build-thunder":
                DefenseHeavenlyLightningBuilder.Build();
                return File.ReadAllText("scratch/ThunderBuild.txt");
            case "validate-map-collision":
                DefenseMapCollisionValidation.Validate();
                return File.ReadAllText("scratch/MapCollisionValidation.txt");
            case "smoke-map-collision":
                DefenseMapCollisionSmoke.Start();
                return "Map collision smoke scheduled: scratch/MapCollisionSmoke.txt";
            case "validate-thunder":
                DefenseThunderValidation.Validate();
                return File.ReadAllText("scratch/ThunderValidation.txt");
            case "smoke-thunder":
                DefenseThunderSmoke.Start();
                return "Thunder Play Mode validation scheduled: scratch/ThunderSmoke.txt";
            case "smoke-loading-ui":
                AppLoadingSmoke.Start();
                return "Loading Play Mode validation scheduled: scratch/LoadingSmoke.txt";
            case "smoke-towers":
                DefenseTowerSmoke.Start();
                return "Tower Play Mode smoke scheduled; report: scratch/TowerSmoke.txt";
            case "refresh":
                AssetDatabase.Refresh();
                return "Refresh requested.";
            case "save-scenes":
                UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
                return "Saved open scenes.";
            case "clean-tmp-submeshes":
                ProjectZombie.EditorTools.TMPVietnameseFontSetupTool.MigratePrefabsAndCleanSubMeshes();
                return "Migrated prefabs to Vietnamese font and cleaned TMP SubMeshes.";
            case "optimize-ui-raycasts":
                int optimizedCount = ProjectZombie.Editor.UI.UIRaycastOptimizer.OptimizeAllPrefabs(true);
                return "Optimized raycast targets on " + optimizedCount + " UI elements across all prefabs.";
            case "validate-app-architecture":
                AppArchitectureValidation.Validate();
                return File.ReadAllText("scratch/AppArchitectureValidation.txt");
            case "smoke-app-architecture":
                AppArchitectureSmoke.Start();
                return "Play Mode smoke scheduled; results: scratch/AppArchitectureSmoke.txt";
            case "smoke-bootstrap":
                AppArchitectureSmoke.StartFromBootstrap();
                return "Bootstrap Play Mode smoke scheduled; results: scratch/AppArchitectureSmoke.txt";
            case "smoke-direct-map":
                AppArchitectureSmoke.StartFromMap();
                return "Direct-map Play Mode smoke scheduled; results: scratch/AppArchitectureSmoke.txt";
            case "hierarchy":
                var rows = new List<string>();
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var scene = SceneManager.GetSceneAt(i);
                    if (!scene.isLoaded) continue;
                    rows.Add("Scene: " + scene.path);
                    foreach (var root in scene.GetRootGameObjects()) Collect(root.transform, "", rows);
                }
                return string.Join("\n", rows);
            case "capture-camera":
                return CaptureCamera();
            case "validate-card-progression":
                return CardProgressionValidation.Validate();
            case "preview-card-feedback":
                return ProjectZombie.Editor.UI.CardCodexVisualChecks.PreviewUpgradeFeedback();
            case "apply-courtyard":
                return ProjectZombie.Editor.UI.CourtyardDeploymentStyler.Apply();
            case "preview-courtyard":
                return ProjectZombie.Editor.UI.CourtyardDeploymentPreview.RenderAll();
            case "validate-courtyard":
                return ProjectZombie.Editor.UI.CourtyardDeploymentStyler.Validate();
            case "install-loadout-screen":
                return ProjectZombie.Editor.UI.LoadoutManagementScreenInstaller.Install();
            case "apply-clean-review":
                return ProjectZombie.Editor.UI.CleanBattleReviewStyler.Apply();
            case "validate-clean-review":
                return ProjectZombie.Editor.UI.CleanBattleReviewStyler.Validate();
            case "validate-enemy-health":
                if (EditorApplication.isPlaying) throw new InvalidOperationException("Validation requires Edit Mode.");
                DefenseEnemyHealthValidation.Validate();
                return File.ReadAllText("scratch/EnemyHealthValidation.txt") + "\n" + Path.GetFullPath("scratch/EnemyHealthPreview.png");
            default:
                throw new ArgumentException("Supported commands: status, hierarchy, capture-camera, validate-enemy-health");
        }
    }

    private static void Collect(Transform node, string parent, List<string> rows)
    {
        if (rows.Count >= 5000) return;
        string path = parent + "/" + node.name;
        rows.Add(path + " [active=" + node.gameObject.activeInHierarchy + "]");
        foreach (Transform child in node) Collect(child, path, rows);
    }

    private static string CaptureCamera()
    {
        var camera = Camera.main;
        if (camera == null) throw new InvalidOperationException("No active MainCamera.");
        const int WIDTH = 960;
        const int HEIGHT = 540;
        var target = new RenderTexture(WIDTH, HEIGHT, 24);
        var texture = new Texture2D(WIDTH, HEIGHT, TextureFormat.RGB24, false);
        var previousTarget = camera.targetTexture;
        var previousActive = RenderTexture.active;
        try
        {
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            texture.ReadPixels(new Rect(0, 0, WIDTH, HEIGHT), 0, 0);
            texture.Apply();
            string output = Path.Combine(ROOT, "camera.png");
            File.WriteAllBytes(output, texture.EncodeToPNG());
            return output;
        }
        finally
        {
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            UnityEngine.Object.DestroyImmediate(texture);
            UnityEngine.Object.DestroyImmediate(target);
        }
    }

    private static void WriteAtomic(string path, string contents)
    {
        string temporary = path + ".tmp";
        File.WriteAllText(temporary, contents);
        if (File.Exists(path)) File.Delete(path);
        File.Move(temporary, path);
    }
}
#endif
