#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.UI;

/// <summary>Explicit idempotent migration of Sông Quê sockets, catalog and build menu.</summary>
public static class DefenseTowerArchitectureInstaller
{
    public const string CATALOG = "Assets/_Data/Defense3D/Towers/TowerCatalog.asset";
    public const string METADATA = "Assets/_Data/Defense3D/Towers/TowerLoadoutMetadata.asset";
    [MenuItem("Tools/ProjectZombie/Towers/Install Modular Towers")]
    public static void Install()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode first.");
        Directory.CreateDirectory("Assets/_Data/Defense3D/Towers/Attacks");
        Directory.CreateDirectory("Assets/_Data/Defense3D/Projectiles");
        var archer = ConfigureTower("SO_Tower_NoThan_Archer", "P_Tower_NoThan_Archer", "P_Proj_Arrow_Archer", "tower_cungtien", "Nỏ Thần", "Tên bám mục tiêu, sát thương đơn.");
        var ice = ConfigureTower("SO_Tower_BangHa_Ice", "P_Tower_BangHa_Ice", "P_Proj_IceShard_BangHa", "tower_bangha", "Băng Hà", "Đạn băng làm chậm kẻ địch.");
        var catalog = LoadOrCreate<DefenseTowerCatalogSO>(CATALOG);
        var retained = Enumerable.Range(0, catalog.Count).Select(catalog.Get).Where(x => x != null && x.Data != null && x.Data.Id != archer.Item1.Id && x.Data.Id != ice.Item1.Id).ToArray();
        var serializedCatalog = new SerializedObject(catalog);
        var entries = serializedCatalog.FindProperty("_entries");
        entries.arraySize = 2 + retained.Length;
        WriteEntry(entries.GetArrayElementAtIndex(0), archer.Item1, archer.Item2);
        WriteEntry(entries.GetArrayElementAtIndex(1), ice.Item1, ice.Item2);
        for (int i = 0; i < retained.Length; i++) WriteEntry(entries.GetArrayElementAtIndex(i + 2), retained[i].Data, retained[i].Prefab);
        serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
        catalog.Validate();
        SyncMetadata();

        var previous = SceneManager.GetActiveScene();
        var scene = SceneManager.GetSceneByPath(AppArchitectureInstaller.MAP);
        bool opened = !scene.IsValid() || !scene.isLoaded;
        if (opened) scene = EditorSceneManager.OpenScene(AppArchitectureInstaller.MAP, OpenSceneMode.Additive);
        string backup = "scratch/TowerArchitectureBackup/" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff");
        Directory.CreateDirectory(backup);
        if (!EditorSceneManager.SaveScene(scene, backup + "/MapSongQue3D_Prototype.unity", true)) throw new InvalidOperationException("Cannot snapshot scene.");
        var roots = scene.GetRootGameObjects();
        var builder = roots.SelectMany(x => x.GetComponentsInChildren<DefenseTowerBuildPrototype3D>(true)).Single();
        var info = roots.SelectMany(x => x.GetComponentsInChildren<DefenseSceneInfo3D>(true)).Single();
        var composition = roots.SelectMany(x => x.GetComponentsInChildren<DefenseSceneCompositionRoot>(true)).Single();
        var pool = roots.SelectMany(x => x.GetComponentsInChildren<DefenseProjectilePool3D>(true)).Single();
        var systems = roots.FirstOrDefault(x => x.name.Contains("SYSTEMS & CORE"));
        builder.transform.SetParent(systems != null ? systems.transform : null, true);
        builder.gameObject.name = "Defense Tower Builder";
        builder.gameObject.SetActive(true);
        foreach (var tower in roots.SelectMany(x => x.GetComponentsInChildren<DefenseTowerPrototype3D>(true)).ToArray())
            UnityEngine.Object.DestroyImmediate(tower.gameObject);
        foreach (var legacy in roots.Where(x => x != null).SelectMany(x => x.GetComponentsInChildren<DefenseTowerProjectileVfx3D>(true)).ToArray())
            UnityEngine.Object.DestroyImmediate(legacy.gameObject);
        var builderObject = new SerializedObject(builder);
        builderObject.FindProperty("_catalog").objectReferenceValue = catalog;
        builderObject.FindProperty("_projectiles").objectReferenceValue = pool;
        builderObject.FindProperty("sceneInfo").objectReferenceValue = info;
        builderObject.FindProperty("wallet").objectReferenceValue = info.HeroSpawn.GetComponent<DefenseWoodWallet3D>();
        var sockets = builderObject.FindProperty("_sockets");
        sockets.arraySize = info.TowerSocketCount;
        for (int i = 0; i < info.TowerSocketCount; i++)
        {
            var anchor = info.GetTowerSocket(i);
            if (!anchor.TryGetComponent<DefenseTowerSocket3D>(out var socket)) socket = anchor.gameObject.AddComponent<DefenseTowerSocket3D>();
            var socketObject = new SerializedObject(socket);
            if (string.IsNullOrEmpty(socket.SocketId)) socketObject.FindProperty("_socketId").stringValue = "songque-tower-" + (i + 1).ToString("00");
            socketObject.ApplyModifiedPropertiesWithoutUndo();
            sockets.GetArrayElementAtIndex(i).objectReferenceValue = socket;
        }
        builderObject.ApplyModifiedPropertiesWithoutUndo();
        var menu = roots.Where(x => x != null).SelectMany(x => x.GetComponentsInChildren<DefenseTowerBuildPresenter>(true)).FirstOrDefault();
        if (menu != null && menu.GetComponent<DefenseTowerBuildView>().Capacity != catalog.Count)
        {
            UnityEngine.Object.DestroyImmediate(menu.gameObject);
            menu = null;
        }
        if (menu == null) menu = CreateMenu(scene, builder, info.HeroSpawn.GetComponent<DefenseWoodWallet3D>(), catalog.Count);
        var uiRoot = roots.FirstOrDefault(x => x != null && x.name.Contains("USER INTERFACE"));
        if (uiRoot != null) menu.transform.SetParent(uiRoot.transform, false);
        var compositionObject = new SerializedObject(composition);
        PruneReferences(compositionObject.FindProperty("_sceneParticipants"));
        var participants = compositionObject.FindProperty("_sceneParticipants");
        bool included = false;
        for (int i = 0; i < participants.arraySize; i++) if (participants.GetArrayElementAtIndex(i).objectReferenceValue == menu) included = true;
        if (!included) { int index = participants.arraySize++; participants.GetArrayElementAtIndex(index).objectReferenceValue = menu; }
        var textActors = compositionObject.FindProperty("_combatTextActors");
        for (int i = textActors.arraySize - 1; i >= 0; i--)
            if (textActors.GetArrayElementAtIndex(i).FindPropertyRelative("Actor").objectReferenceValue == null) textActors.DeleteArrayElementAtIndex(i);
        compositionObject.ApplyModifiedPropertiesWithoutUndo();
        var friendly = roots.Where(x => x != null).SelectMany(x => x.GetComponentsInChildren<DefenseFriendlyTargets3D>(true)).Single();
        var friendlyObject = new SerializedObject(friendly);
        PruneReferences(friendlyObject.FindProperty("_supports"));
        friendlyObject.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        if (opened) EditorSceneManager.CloseScene(scene, true);
        if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
        AssetDatabase.SaveAssets();
        File.WriteAllText("scratch/TowerArchitectureInstall.txt", "Installed 2 tower types, independent sockets, build menu and per-map prewarm.");
    }
    [MenuItem("Tools/ProjectZombie/Towers/Sync Loadout Metadata")]
    public static void SyncMetadata()
    {
        var catalog = AssetDatabase.LoadAssetAtPath<DefenseTowerCatalogSO>(CATALOG);
        if (catalog == null) throw new InvalidOperationException("Install tower catalog first.");
        catalog.Validate();
        var metadata = LoadOrCreate<ProjectZombie.Features.Defense3D.Loadout.DefenseTowerLoadoutCatalogSO>(METADATA);
        var serialized = new SerializedObject(metadata);
        var entries = serialized.FindProperty("_entries");
        entries.arraySize = catalog.Count;
        for (int i = 0; i < catalog.Count; i++)
        {
            entries.GetArrayElementAtIndex(i).FindPropertyRelative("Id").stringValue = catalog.Get(i).Data.Id;
            entries.GetArrayElementAtIndex(i).FindPropertyRelative("Name").stringValue = catalog.Get(i).Data.TowerName;
        }
        serialized.ApplyModifiedPropertiesWithoutUndo();
        var bootstrap = PrefabUtility.LoadPrefabContents(AppArchitectureInstaller.PREFAB);
        try
        {
            var root = new SerializedObject(bootstrap.GetComponent<ProjectZombie.Core.Architecture.AppCompositionRoot>());
            root.FindProperty("_towerMetadata").objectReferenceValue = metadata;
            root.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(bootstrap, AppArchitectureInstaller.PREFAB);
        }
        finally { PrefabUtility.UnloadPrefabContents(bootstrap); }
        AssetDatabase.SaveAssets();
    }
    private static Tuple<SO_TowerData, DefenseTowerPrototype3D> ConfigureTower(string dataName, string prefabName, string projectileName, string id, string title, string description)
    {
        var data = AssetDatabase.LoadAssetAtPath<SO_TowerData>("Assets/_Data/Defense3D/Towers/" + dataName + ".asset");
        bool seedAttack = data.Attack == null;
        var projectile = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Defense3D/Projectiles/" + projectileName + ".prefab");
        var definition = data.Attack != null ? data.Attack.Projectile : LoadOrCreate<DefenseProjectileDefinitionSO>("Assets/_Data/Defense3D/Projectiles/" + projectileName + ".asset");
        if (definition == null) throw new InvalidOperationException("Existing attack has no projectile definition: " + data.Id);
        var definitionObject = new SerializedObject(definition);
        if (seedAttack)
        {
            definitionObject.FindProperty("_projectileId").stringValue = id + "-projectile";
            definitionObject.FindProperty("_visualPrefab").objectReferenceValue = projectile;
            definitionObject.FindProperty("_speed").floatValue = data.ProjectileSpeed;
            definitionObject.FindProperty("_arcHeight").floatValue = data.ProjectileArcHeight;
        }
        definitionObject.ApplyModifiedPropertiesWithoutUndo();
        var attack = data.Attack != null ? data.Attack : LoadOrCreate<DefenseTowerAttackProfileSO>("Assets/_Data/Defense3D/Towers/Attacks/" + id + ".asset");
        var attackObject = new SerializedObject(attack);
        if (seedAttack)
        {
            attackObject.FindProperty("_projectile").objectReferenceValue = definition;
            var effects = attackObject.FindProperty("_effects");
            effects.arraySize = data.AttackEffects.Count;
            for (int i = 0; i < effects.arraySize; i++) effects.GetArrayElementAtIndex(i).objectReferenceValue = data.AttackEffects[i];
        }
        attackObject.ApplyModifiedPropertiesWithoutUndo();
        var dataObject = new SerializedObject(data);
        dataObject.FindProperty("_id").stringValue = id;
        if (string.IsNullOrEmpty(data.TowerName)) dataObject.FindProperty("_towerName").stringValue = title;
        if (string.IsNullOrEmpty(data.Description)) dataObject.FindProperty("_description").stringValue = description;
        var levels = dataObject.FindProperty("_levels");
        for (int i = 0; i < levels.arraySize; i++)
            if (levels.GetArrayElementAtIndex(i).FindPropertyRelative("maxHealth").floatValue <= 0f)
                levels.GetArrayElementAtIndex(i).FindPropertyRelative("maxHealth").floatValue = 150f + i * 100f;
        dataObject.FindProperty("_attack").objectReferenceValue = attack;
        dataObject.ApplyModifiedPropertiesWithoutUndo();
        string prefabPath = "Assets/_Prefabs/Defense3D/Towers/" + prefabName + ".prefab";
        var root = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            var tower = root.GetComponent<DefenseTowerPrototype3D>();
            tower.Configure(data, null);
            if (root.GetComponentsInChildren<Renderer>(true).Length == 0) CreateBlockout(root.transform, id == "tower_bangha");
            if (!root.TryGetComponent<DefenseSupportTarget3D>(out var health)) health = root.AddComponent<DefenseSupportTarget3D>();
            var healthObject = new SerializedObject(health);
            healthObject.FindProperty("_isStructure").boolValue = true;
            healthObject.FindProperty("_maxHp").floatValue = 150f;
            healthObject.ApplyModifiedPropertiesWithoutUndo();
            if (!root.TryGetComponent<Collider>(out _)) { var collider = root.AddComponent<BoxCollider>(); collider.center = Vector3.up; collider.size = new Vector3(1, 2, 1); }
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
        return Tuple.Create(data, AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath).GetComponent<DefenseTowerPrototype3D>());
    }
    private static void CreateBlockout(Transform root, bool ice)
    {
        Directory.CreateDirectory("Assets/_Data/Defense3D/Towers/Materials");
        string path = "Assets/_Data/Defense3D/Towers/Materials/" + (ice ? "Ice" : "Wood") + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", ice ? new Color(.2f, .65f, .85f) : new Color(.42f, .27f, .12f));
            material.enableInstancing = true;
            AssetDatabase.CreateAsset(material, path);
        }
        var model = root.Find("Model_3D") ?? root;
        Part(model, "Base", PrimitiveType.Cylinder, new Vector3(0, .15f, 0), new Vector3(1.2f, .15f, 1.2f), material);
        Part(model, "Body", PrimitiveType.Cube, new Vector3(0, .8f, 0), new Vector3(.65f, 1.1f, .65f), material);
        var top = Part(model, ice ? "Crystal" : "Bow", PrimitiveType.Cube, new Vector3(0, 1.5f, 0),
            ice ? new Vector3(.6f, .8f, .6f) : new Vector3(1.3f, .15f, .35f), material);
        if (ice) top.localRotation = Quaternion.Euler(0, 45, 30);
    }
    private static Transform Part(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
    {
        var part = GameObject.CreatePrimitive(type);
        part.name = name;
        UnityEngine.Object.DestroyImmediate(part.GetComponent<Collider>());
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        part.GetComponent<Renderer>().sharedMaterial = material;
        return part.transform;
    }
    private static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null) { asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset, path); }
        return asset;
    }
    private static void WriteEntry(SerializedProperty property, SO_TowerData data, DefenseTowerPrototype3D prefab)
    {
        property.FindPropertyRelative("Data").objectReferenceValue = data;
        property.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
    }
    private static void PruneReferences(SerializedProperty array)
    {
        var retained = Enumerable.Range(0, array.arraySize).Select(i => array.GetArrayElementAtIndex(i).objectReferenceValue).Where(x => x != null).ToArray();
        array.arraySize = retained.Length;
        for (int i = 0; i < retained.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = retained[i];
    }
    private static DefenseTowerBuildPresenter CreateMenu(Scene scene, DefenseTowerBuildPrototype3D builder, DefenseWoodWallet3D wallet, int count)
    {
        var canvasObject = new GameObject("Tower Build Menu", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(DefenseTowerBuildView), typeof(DefenseTowerBuildPresenter));
        SceneManager.MoveGameObjectToScene(canvasObject, scene);
        var canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 150;
        var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080);
        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image)); panel.transform.SetParent(canvasObject.transform, false);
        var panelRect = panel.GetComponent<RectTransform>(); panelRect.sizeDelta = new Vector2(760, 220 + count * 180);
        panel.GetComponent<Image>().color = new Color(.035f, .09f, .1f, .98f);
        var heading = Label(panel.transform, "Title", new Vector2(0, panelRect.sizeDelta.y * .5f - 60), new Vector2(700, 70), 32);
        heading.text = "CHỌN TRỤ PHÒNG THỦ";
        var viewObject = new SerializedObject(canvasObject.GetComponent<DefenseTowerBuildView>());
        viewObject.FindProperty("_panel").objectReferenceValue = panel;
        viewObject.FindProperty("_message").objectReferenceValue = heading;
        var choices = viewObject.FindProperty("_choices"); var labels = viewObject.FindProperty("_labels"); choices.arraySize = labels.arraySize = count;
        for (int i = 0; i < count; i++)
        {
            var button = Button(panel.transform, "Tower " + i, new Vector2(0, panelRect.sizeDelta.y * .5f - 180 - i * 180), new Vector2(680, 155));
            var label = Label(button.transform, "Label", Vector2.zero, new Vector2(650, 145), 27);
            choices.GetArrayElementAtIndex(i).objectReferenceValue = button;
            labels.GetArrayElementAtIndex(i).objectReferenceValue = label;
        }
        var close = Button(panel.transform, "Close", new Vector2(0, -panelRect.sizeDelta.y * .5f + 50), new Vector2(260, 65));
        Label(close.transform, "Label", Vector2.zero, new Vector2(250, 60), 27).text = "ĐÓNG";
        viewObject.FindProperty("_close").objectReferenceValue = close;
        viewObject.ApplyModifiedPropertiesWithoutUndo();
        panel.SetActive(false);
        var presenter = canvasObject.GetComponent<DefenseTowerBuildPresenter>();
        var presenterObject = new SerializedObject(presenter);
        presenterObject.FindProperty("_builder").objectReferenceValue = builder;
        presenterObject.FindProperty("_wallet").objectReferenceValue = wallet;
        presenterObject.FindProperty("_view").objectReferenceValue = canvasObject.GetComponent<DefenseTowerBuildView>();
        presenterObject.ApplyModifiedPropertiesWithoutUndo();
        return presenter;
    }
    private static TextMeshProUGUI Label(Transform parent, string name, Vector2 position, Vector2 size, float fontSize)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)); obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>(); rect.anchoredPosition = position; rect.sizeDelta = size;
        var text = obj.GetComponent<TextMeshProUGUI>(); text.font = TMP_Settings.defaultFontAsset; text.fontSize = fontSize; text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
        return text;
    }
    private static Button Button(Transform parent, string name, Vector2 position, Vector2 size)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button)); obj.transform.SetParent(parent, false);
        var rect = obj.GetComponent<RectTransform>(); rect.anchoredPosition = position; rect.sizeDelta = size;
        obj.GetComponent<Image>().color = new Color(.11f, .25f, .25f);
        return obj.GetComponent<Button>();
    }
}
#endif
