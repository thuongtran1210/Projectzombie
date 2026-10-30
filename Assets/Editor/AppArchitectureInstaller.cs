#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ProjectZombie.Core.Architecture;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Maps;
using ProjectZombie.Features.UI;

/// <summary>Idempotent project migration. Snapshots loaded scenes before preserving and migrating their current state.</summary>
public static class AppArchitectureInstaller
{
    public const string HUB = "Assets/Scenes/MainMenu.unity";
    public const string MAP = "Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity";
    public const string BOOT = "Assets/Scenes/Bootstrap.unity";
    public const string PREFAB = "Assets/Core/Architecture/Assets/AppBootstrap.prefab";
    public const string RULES = "Assets/_Data/Levels/Rules/Stage01_Production.asset";
    private const string MAP_DATA = "Assets/_Data/Levels/Map_SongQue.asset";
    private const string STAGE = "Assets/_Data/Levels/Stages/Stage_04_SongQue_Defense3D.asset";
    private const string DATABASE = "Assets/_Data/Levels/WorldStageDatabase.asset";

    [MenuItem("Tools/ProjectZombie/Architecture/Install Bootstrap and Multi Map Flow")]
    public static void Install()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");
        string backup = "scratch/ArchitectureBackup/" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff");
        Directory.CreateDirectory(backup);
        foreach (var path in new[] { HUB, MAP, BOOT })
        {
            var loaded = SceneManager.GetSceneByPath(path);
            if (loaded.IsValid() && loaded.isLoaded)
            {
                if (!EditorSceneManager.SaveScene(loaded, backup + "/" + Path.GetFileName(path), true))
                    throw new InvalidOperationException("Could not snapshot loaded scene: " + path);
            }
            else if (File.Exists(path)) File.Copy(path, backup + "/" + Path.GetFileName(path));
        }
        var previous = SceneManager.GetActiveScene();
        Directory.CreateDirectory("Assets/Core/Architecture/Assets");
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) throw new InvalidOperationException("Addressables settings are missing.");
        var group = settings.FindGroup("App_Scenes_Local") ?? settings.CreateGroup("App_Scenes_Local", false, false, true, null,
            typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));
        var bundle = group.GetSchema<BundledAssetGroupSchema>();
        bundle.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackSeparately;
        bundle.BuildPath.SetVariableByName(settings, "Local.BuildPath");
        bundle.LoadPath.SetVariableByName(settings, "Local.LoadPath");
        foreach (var path in new[] { HUB, MAP })
        {
            var entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(path), group);
            entry.address = Path.GetFileNameWithoutExtension(path);
        }
        EditorUtility.SetDirty(settings);

        var mapData = AssetDatabase.LoadAssetAtPath<MapDefinitionSO>(MAP_DATA);
        if (mapData == null)
        {
            mapData = ScriptableObject.CreateInstance<MapDefinitionSO>();
            AssetDatabase.CreateAsset(mapData, MAP_DATA);
        }
        var mapObject = new SerializedObject(mapData);
        mapObject.FindProperty("_mapId").stringValue = "songque";
        mapObject.FindProperty("_scene.m_AssetGUID").stringValue = AssetDatabase.AssetPathToGUID(MAP);
        mapObject.ApplyModifiedPropertiesWithoutUndo();
        var stage = AssetDatabase.LoadAssetAtPath<StageDefinitionSO>(STAGE);
        Directory.CreateDirectory("Assets/_Data/Levels/Rules");
        AssetDatabase.Refresh();
        const string OLD_RULES = "Assets/Resources/Defense3D/Stages/Stage01_Production.asset";
        if (AssetDatabase.LoadAssetAtPath<StageSpawnConfig>(RULES) == null && File.Exists(OLD_RULES))
        {
            string error = AssetDatabase.MoveAsset(OLD_RULES, RULES);
            if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
        }
        var spawnConfig = AssetDatabase.LoadAssetAtPath<StageSpawnConfig>(RULES);
        if (stage == null || spawnConfig == null) throw new InvalidOperationException("Production stage assets missing.");
        var stageObject = new SerializedObject(stage);
        stageObject.FindProperty("_map").objectReferenceValue = mapData;
        var rulesEntry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(spawnConfig)), group);
        rulesEntry.address = "StageRules_SongQue";
        stageObject.FindProperty("_spawnRules.m_AssetGUID").stringValue = rulesEntry.guid;
        stageObject.ApplyModifiedPropertiesWithoutUndo();

        var bootstrap = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        var app = new GameObject("AppCompositionRoot");
        SceneManager.MoveGameObjectToScene(app, bootstrap);
        var composition = app.AddComponent<AppCompositionRoot>();
        var input = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        input.transform.SetParent(app.transform, false);
        var canvasObject = new GameObject("LoadingCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup), typeof(AppLoadingView));
        canvasObject.transform.SetParent(app.transform, false);
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        var backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
        backdrop.transform.SetParent(canvasObject.transform, false);
        Stretch(backdrop.GetComponent<RectTransform>());
        backdrop.GetComponent<Image>().color = new Color(.025f, .045f, .05f, 1f);
        var status = new GameObject("Status", typeof(RectTransform), typeof(TextMeshProUGUI));
        status.transform.SetParent(canvasObject.transform, false);
        Stretch(status.GetComponent<RectTransform>());
        var text = status.GetComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = 36;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        var dismissObject = new GameObject("Dismiss", typeof(RectTransform), typeof(Image), typeof(Button));
        dismissObject.transform.SetParent(canvasObject.transform, false);
        var dismissRect = dismissObject.GetComponent<RectTransform>();
        dismissRect.anchorMin = dismissRect.anchorMax = new Vector2(.5f, .25f);
        dismissRect.sizeDelta = new Vector2(280, 90);
        dismissObject.GetComponent<Image>().color = new Color(.15f, .35f, .32f);
        var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(dismissObject.transform, false);
        Stretch(labelObject.GetComponent<RectTransform>());
        var label = labelObject.GetComponent<TextMeshProUGUI>();
        label.font = text.font;
        label.fontSize = 32;
        label.text = "Đóng";
        label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
        dismissObject.SetActive(false);
        var view = canvasObject.GetComponent<AppLoadingView>();
        var viewObject = new SerializedObject(view);
        viewObject.FindProperty("_group").objectReferenceValue = canvasObject.GetComponent<CanvasGroup>();
        viewObject.FindProperty("_status").objectReferenceValue = text;
        viewObject.FindProperty("_dismiss").objectReferenceValue = dismissObject.GetComponent<Button>();
        viewObject.ApplyModifiedPropertiesWithoutUndo();
        view.Hide();
        var audioObject = new GameObject("Audio");
        audioObject.transform.SetParent(app.transform, false);
        var audio = audioObject.AddComponent<global::Core.Audio.AudioManager>();
        var audioSettings = new SerializedObject(audio);
        audioSettings.FindProperty("_masterMixer").objectReferenceValue = AssetDatabase.LoadAssetAtPath<UnityEngine.Audio.AudioMixer>("Assets/_Data/Audios/MasterMixer.mixer");
        audioSettings.ApplyModifiedPropertiesWithoutUndo();
        var appObject = new SerializedObject(composition);
        appObject.FindProperty("_hubScene.m_AssetGUID").stringValue = AssetDatabase.AssetPathToGUID(HUB);
        appObject.FindProperty("_stages").objectReferenceValue = AssetDatabase.LoadAssetAtPath<WorldStageDatabaseSO>(DATABASE);
        appObject.FindProperty("_loading").objectReferenceValue = view;
        appObject.FindProperty("_towerMetadata").objectReferenceValue = AssetDatabase.LoadAssetAtPath<ProjectZombie.Features.Defense3D.Loadout.DefenseTowerLoadoutCatalogSO>(DefenseTowerArchitectureInstaller.METADATA);
        appObject.ApplyModifiedPropertiesWithoutUndo();
        var bootstrapPrefab = PrefabUtility.SaveAsPrefabAsset(app, PREFAB);
        UnityEngine.Object.DestroyImmediate(app);
        PrefabUtility.InstantiatePrefab(bootstrapPrefab, bootstrap);
        EditorSceneManager.SaveScene(bootstrap, BOOT);
        EditorSceneManager.CloseScene(bootstrap, true);

        MigrateContent(HUB, stage);
        MigrateContent(MAP, stage);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(BOOT, true) };
        AssetDatabase.SaveAssets();
        if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
        Directory.CreateDirectory("scratch");
        File.WriteAllText("scratch/AppArchitectureInstall.txt", "Bootstrap, local Addressable content scenes, stage route and scene-local managers installed.");
        Debug.Log("[AppArchitecture] Migration complete.");
    }

    private static void MigrateContent(string path, StageDefinitionSO stage)
    {
        var scene = SceneManager.GetSceneByPath(path);
        bool opened = !scene.IsValid() || !scene.isLoaded;
        if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            foreach (var system in root.GetComponentsInChildren<EventSystem>(true)) UnityEngine.Object.DestroyImmediate(system.gameObject);
            if (root == null) continue;
            foreach (var transition in root.GetComponentsInChildren<MetaSceneTransitionController>(true))
            {
                var ui = roots.Where(r => r != null).SelectMany(r => r.GetComponentsInChildren<MetaUIManager>(true)).FirstOrDefault();
                var serialized = new SerializedObject(transition);
                serialized.FindProperty("_metaUIManager").objectReferenceValue = ui;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                transition.gameObject.name = "MainMenu Navigation";
            }
            foreach (var component in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (component is ProjectZombie.Features.Shared.GameStateManager
                    || component is ProjectZombie.Core.Save.GameManager
                    || component is ProjectZombie.Features.MetaProgression.MetaCurrencyManager
                    || component is ProjectZombie.Features.MetaProgression.RelicInventoryManager
                    || component is ProjectZombie.Features.MetaProgression.Gacha.RelicGachaManager
                    || component is ProjectZombie.Features.MetaProgression.CharacterProgressionManager)
                    UnityEngine.Object.DestroyImmediate(component);
                if (component is DefenseSceneCompositionRoot composition)
                {
                    var serialized = new SerializedObject(composition);
                    serialized.FindProperty("_directPlayStage").objectReferenceValue = stage;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
                if (component is DefenseRun6003D run)
                {
                    var serialized = new SerializedObject(run);
                    serialized.FindProperty("stageSpawnConfig").objectReferenceValue = null;
                    serialized.FindProperty("stageBlueprint").objectReferenceValue = null;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
            }
            if (path == HUB)
            {
                foreach (var group in root.GetComponentsInChildren<CanvasGroup>(true))
                    if (group.name == "FadeTransitionOverlay") UnityEngine.Object.DestroyImmediate(group.gameObject);
            }
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        if (opened) EditorSceneManager.CloseScene(scene, true);
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
}
#endif
