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
using ProjectZombie.Features.Defense3D.Loadout;
using ProjectZombie.Features.Defense3D.UI;
using Object = UnityEngine.Object;

/// <summary>Migrates legacy role-indexed scene data into a shared catalog and reusable ID-based UI templates.</summary>
public static class DefenseMinionRosterMigration
{
    public const string FOLDER = "Assets/Features/Defense3D/Minions/Assets/";
    public const string CATALOG = FOLDER + "MinionCatalog.asset";
    private const string RECRUIT = FOLDER + "MinionRecruitRow.prefab";
    private const string COMMAND = FOLDER + "MinionCommandRow.prefab";

    [MenuItem("Tools/ProjectZombie/Minions/Migrate Squad Roster")]
    public static void Migrate()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Requires Edit Mode.");
        Directory.CreateDirectory(FOLDER); Directory.CreateDirectory("scratch"); AssetDatabase.Refresh();
        var catalog = BuildCatalog();
        Scene previous = SceneManager.GetActiveScene();
        foreach (string path in new[] { "Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity", "Assets/Features/CombatLab/Scenes/DefenseBaseline.unity" })
        {
            Scene scene = SceneManager.GetSceneByPath(path);
            bool alreadyLoaded = scene.IsValid() && scene.isLoaded;
            if (alreadyLoaded && scene.isDirty) throw new InvalidOperationException("Save this scene before migration: " + path);
            if (!alreadyLoaded) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                string backup = "scratch/RosterBefore_" + Path.GetFileName(path);
                if (!File.Exists(backup)) File.Copy(path, backup);
                foreach (var shop in scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<DefenseVillageShop3D>(true)))
                    Configure(shop, catalog);
                EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            }
            finally { if (!alreadyLoaded) EditorSceneManager.CloseScene(scene, true); }
        }
        if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
        AssetDatabase.SaveAssets();
        File.WriteAllText("scratch/MinionRosterMigration.txt", "Migrated production and CombatLab scenes: shared ID catalog, 4 definitions, dynamic recruit/order row prefabs, staged actor prewarm. Existing combat stats and role enum retained.");
    }

    private static DefenseMinionCatalogSO BuildCatalog()
    {
        // These IDs translate existing content once. Runtime has no list of built-in unit types.
        string art = "Assets/Art/Minions/Vongxuyen/";
        string[] ids = { Defense3DCatalog.MINION_DAN_BINH, Defense3DCatalog.MINION_PHU_TAP_DICH, Defense3DCatalog.MINION_THAY_LANG, Defense3DCatalog.MINION_XA_THU };
        string[] files = { "danbinh", "phutapdich", "thaylang", "xathu" };
        string[] names = { "Dân Binh", "Phu Tạp Dịch", "Thầy Lang", "Xạ Thủ" };
        var definitions = new DefenseMinionDefinitionSO[ids.Length];
        for (int i = 0; i < ids.Length; i++)
        {
            var definition = AssetDatabase.LoadAssetAtPath<DefenseMinionDefinitionSO>(i < 3
                ? art + "Definitions/" + files[i] + ".asset" : "Assets/Resources/Defense3D/Minion_XaThu.asset");
            if (definition == null) throw new InvalidOperationException("Missing source definition: " + ids[i]);
            string prefabPath = i < 3 ? art + "Prefabs/" + files[i] + "Minion.prefab" : FOLDER + "XaThuMinion.prefab";
            if (i == 3 && AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
            {
                var clone = PrefabUtility.LoadPrefabContents(art + "Prefabs/danbinhMinion.prefab");
                try
                {
                    clone.name = "XaThuMinion";
                    var unit = new SerializedObject(clone.GetComponent<DefenseMinion3D>());
                    unit.FindProperty("_definition").objectReferenceValue = definition; unit.ApplyModifiedPropertiesWithoutUndo();
                    PrefabUtility.SaveAsPrefabAsset(clone, prefabPath);
                }
                finally { PrefabUtility.UnloadPrefabContents(clone); }
            }
            var data = new SerializedObject(definition);
            data.FindProperty("_id").stringValue = ids[i];
            data.FindProperty("_shortName").stringValue = names[i];
            data.FindProperty("_icon").objectReferenceValue = DefenseLoadoutRuntimeAdapter.ResolveItemIcon(ids[i]);
            data.FindProperty("_prefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath).GetComponent<DefenseMinion3D>();
            // Match the existing source pool budget without resetting designers' later capacity edits.
            if (data.FindProperty("_capacity").intValue <= 0) data.FindProperty("_capacity").intValue = 12;
            if (i == 1) SetLabels(data, "Gom gần nhà", "Theo nhặt gỗ", "Gom toàn map");
            else if (i == 2) SetLabels(data, "Trạm y tế", "Hộ vệ chủ tướng", "Tiếp tế tiền tuyến");
            data.ApplyModifiedPropertiesWithoutUndo(); definitions[i] = definition;
        }
        var catalog = AssetDatabase.LoadAssetAtPath<DefenseMinionCatalogSO>(CATALOG);
        if (catalog == null) { catalog = ScriptableObject.CreateInstance<DefenseMinionCatalogSO>(); AssetDatabase.CreateAsset(catalog, CATALOG); }
        var catalogData = new SerializedObject(catalog);
        var entries = catalogData.FindProperty("_definitions");
        // Preserve additional registered types when migration is run again.
        var existing = Enumerable.Range(0, entries.arraySize).Select(index => entries.GetArrayElementAtIndex(index).objectReferenceValue as DefenseMinionDefinitionSO)
            .Where(item => item != null && !definitions.Contains(item)).ToArray();
        var all = definitions.Concat(existing).ToArray(); entries.arraySize = all.Length;
        for (int i = 0; i < all.Length; i++) entries.GetArrayElementAtIndex(i).objectReferenceValue = all[i];
        catalogData.ApplyModifiedPropertiesWithoutUndo();
        return catalog;
    }

    /// <summary>Keeps the legacy full installer compatible while its content authoring is gradually retired.</summary>
    public static void UpgradeShop(DefenseVillageShop3D shop)
    {
        Directory.CreateDirectory(FOLDER); AssetDatabase.Refresh();
        Configure(shop, BuildCatalog());
    }

    private static void SetLabels(SerializedObject data, string defend, string follow, string advance)
    {
        data.FindProperty("_defendLabel").stringValue = defend;
        data.FindProperty("_followLabel").stringValue = follow;
        data.FindProperty("_advanceLabel").stringValue = advance;
    }

    /// <summary>Can also migrate a freshly rebuilt legacy shop without rebuilding the rest of its scene.</summary>
    public static void Configure(DefenseVillageShop3D shop, DefenseMinionCatalogSO catalog)
    {
        var shopData = new SerializedObject(shop);
        shopData.FindProperty("_minionCatalog").objectReferenceValue = catalog;
        var slots = shopData.FindProperty("_slots");
        var actors = Enumerable.Range(0, slots.arraySize).Select(index => slots.GetArrayElementAtIndex(index).objectReferenceValue as DefenseMinion3D)
            .Where(item => item != null).ToArray();
        var friends = new SerializedObject(shopData.FindProperty("_friends").objectReferenceValue);
        var supports = friends.FindProperty("_supports");
        var remaining = Enumerable.Range(0, supports.arraySize).Select(index => supports.GetArrayElementAtIndex(index).objectReferenceValue)
            .Where(item => item != null && !actors.Any(actor => actor.Health == item)).ToArray();
        supports.arraySize = remaining.Length;
        for (int i = 0; i < remaining.Length; i++) supports.GetArrayElementAtIndex(i).objectReferenceValue = remaining[i];
        friends.ApplyModifiedPropertiesWithoutUndo();
        slots.arraySize = 0; shopData.FindProperty("_catalog").arraySize = 0; shopData.ApplyModifiedPropertiesWithoutUndo();
        foreach (var actor in actors) Object.DestroyImmediate(actor.gameObject);
        ConfigureView(shop.GetComponentInChildren<DefenseVillageShopView>(true));
    }

    private static void ConfigureView(DefenseVillageShopView view)
    {
        var data = new SerializedObject(view);
        var oldRecruit = data.FindProperty("_recruit");
        if (oldRecruit.arraySize > 0)
        {
            var recruit = (Button)oldRecruit.GetArrayElementAtIndex(0).objectReferenceValue;
            var group = (GameObject)data.FindProperty("_commandGroups").GetArrayElementAtIndex(0).objectReferenceValue;
            BuildTemplate(recruit.gameObject, data, false);
            BuildTemplate(group, data, true);
        }
        if (AssetDatabase.LoadAssetAtPath<GameObject>(RECRUIT) == null || AssetDatabase.LoadAssetAtPath<GameObject>(COMMAND) == null)
            throw new InvalidOperationException("Missing roster UI templates; migrate the authored production shop first.");
        NormalizeRecruitTemplate();
        var panel = (GameObject)data.FindProperty("_panel").objectReferenceValue;
        var dock = (GameObject)data.FindProperty("_commandDock").objectReferenceValue;
        foreach (var layout in dock.GetComponents<LayoutGroup>()) Object.DestroyImmediate(layout);
        foreach (var fitter in dock.GetComponents<ContentSizeFitter>()) Object.DestroyImmediate(fitter);
        var obsolete = new System.Collections.Generic.HashSet<GameObject>();
        foreach (string field in new[] { "_recruit", "_commandGroups" })
        {
            var array = data.FindProperty(field);
            for (int i = 0; i < array.arraySize; i++)
            {
                Object value = array.GetArrayElementAtIndex(i).objectReferenceValue;
                if (value is Component component) obsolete.Add(component.gameObject);
                else if (value is GameObject gameObject) obsolete.Add(gameObject);
            }
        }
        foreach (var node in obsolete) if (node != null) Object.DestroyImmediate(node);
        foreach (string field in new[] { "_recruit", "_orders", "_recruitLabels", "_orderLabels", "_commandGroups", "_orderIcons", "_recruitIcons", "_orderCounts" })
            data.FindProperty(field).arraySize = 0;
        Rect((RectTransform)panel.transform, new Vector2(0, -16), new Vector2(420, 440), new Vector2(.5f, 1));
        var status = (TMP_Text)data.FindProperty("_status").objectReferenceValue;
        Rect(status.rectTransform, new Vector2(0, 12), new Vector2(390, 32), new Vector2(.5f, 0));
        status.rectTransform.pivot = new Vector2(.5f, 0);
        Rect((RectTransform)dock.transform, new Vector2(0, 16), new Vector2(520, 160), new Vector2(.5f, 0));
        ((RectTransform)dock.transform).pivot = new Vector2(.5f, 0);
        var recruitContent = Scroll(panel.transform, "RosterRecruitScroll", false, new Vector2(0, -88), new Vector2(396, 296));
        var commandContent = Scroll(dock.transform, "RosterCommandScroll", true, Vector2.zero, new Vector2(520, 160));
        data.FindProperty("_recruitContent").objectReferenceValue = recruitContent;
        data.FindProperty("_commandContent").objectReferenceValue = commandContent;
        data.FindProperty("_recruitTemplate").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(RECRUIT).GetComponent<DefenseMinionSquadRowView>();
        data.FindProperty("_commandTemplate").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(COMMAND).GetComponent<DefenseMinionSquadRowView>();
        data.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void BuildTemplate(GameObject source, SerializedObject viewData, bool command)
    {
        var clone = Object.Instantiate(source); clone.SetActive(false);
        try
        {
            clone.name = command ? "MinionCommandRow" : "MinionRecruitRow";
            foreach (var button in clone.GetComponentsInChildren<Button>(true)) button.onClick = new Button.ButtonClickedEvent();
            var row = clone.AddComponent<DefenseMinionSquadRowView>();
            var data = new SerializedObject(row);
            string[] targets = command ? new[] { "_orders", "_orderLabels", "_orderIcons", "_orderCounts" }
                : new[] { "_recruit", "_recruitLabels", "_recruitIcons" };
            string[] fields = command ? new[] { "_button", "_label", "_icon", "_count" } : new[] { "_button", "_label", "_icon" };
            for (int i = 0; i < targets.Length; i++)
            {
                var original = (Component)viewData.FindProperty(targets[i]).GetArrayElementAtIndex(0).objectReferenceValue;
                string path = AnimationUtility.CalculateTransformPath(original.transform, source.transform);
                Transform node = string.IsNullOrEmpty(path) ? clone.transform : clone.transform.Find(path);
                data.FindProperty(fields[i]).objectReferenceValue = node.GetComponent(original.GetType());
            }
            if (command) data.FindProperty("_name").objectReferenceValue = clone.transform.Find("RoleName").GetComponent<TMP_Text>();
            data.ApplyModifiedPropertiesWithoutUndo();
            var element = clone.GetComponent<LayoutElement>() ?? clone.AddComponent<LayoutElement>();
            element.preferredWidth = command ? 120 : 396; element.preferredHeight = command ? 160 : 72;
            Rect((RectTransform)clone.transform, Vector2.zero, new Vector2(element.preferredWidth, element.preferredHeight), new Vector2(.5f, 1));
            if (command)
            {
                var name = clone.transform.Find("RoleName").GetComponent<TMP_Text>(); name.fontSize = 16;
                name.fontSizeMax = 16; name.fontSizeMin = 12;
                Rect(name.rectTransform, new Vector2(0, -104), new Vector2(120, 26), new Vector2(.5f, 1));
                var label = (TMP_Text)data.FindProperty("_label").objectReferenceValue; label.fontSize = 14;
                label.fontSizeMax = 14; label.fontSizeMin = 12;
                Rect(label.rectTransform, new Vector2(0, -132), new Vector2(120, 28), new Vector2(.5f, 1));
            }
            PrefabUtility.SaveAsPrefabAsset(clone, command ? COMMAND : RECRUIT);
        }
        finally { Object.DestroyImmediate(clone); }
    }

    private static void NormalizeRecruitTemplate()
    {
        var root = PrefabUtility.LoadPrefabContents(RECRUIT);
        try
        {
            root.GetComponent<LayoutElement>().preferredHeight = 56f;
            var rect = (RectTransform)root.transform; rect.sizeDelta = new Vector2(rect.sizeDelta.x, 56f);
            var data = new SerializedObject(root.GetComponent<DefenseMinionSquadRowView>());
            var label = (TMP_Text)data.FindProperty("_label").objectReferenceValue;
            label.fontSize = label.fontSizeMax = 20f; label.fontSizeMin = 16f;
            label.rectTransform.anchoredPosition = new Vector2(78f, -2f);
            label.rectTransform.sizeDelta = new Vector2(270f, 52f);
            var icon = (Image)data.FindProperty("_icon").objectReferenceValue;
            icon.rectTransform.anchoredPosition = new Vector2(12f, -4f);
            icon.rectTransform.sizeDelta = Vector2.one * 48f;
            PrefabUtility.SaveAsPrefabAsset(root, RECRUIT);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static RectTransform Scroll(Transform parent, string name, bool horizontal, Vector2 position, Vector2 size)
    {
        var previous = parent.Find(name); if (previous != null) Object.DestroyImmediate(previous.gameObject);
        var root = new GameObject(name, typeof(RectTransform), typeof(ScrollRect)).GetComponent<RectTransform>(); root.SetParent(parent, false);
        Rect(root, position, size, new Vector2(.5f, 1));
        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D)).GetComponent<RectTransform>();
        viewport.SetParent(root, false); viewport.anchorMin = Vector2.zero; viewport.anchorMax = Vector2.one; viewport.offsetMin = viewport.offsetMax = Vector2.zero;
        viewport.GetComponent<Image>().color = new Color(0, 0, 0, .001f);
        var content = new GameObject("Content", typeof(RectTransform)).GetComponent<RectTransform>(); content.SetParent(viewport, false);
        content.pivot = new Vector2(0, 1); content.anchorMin = content.anchorMax = new Vector2(0, 1); content.sizeDelta = size;
        LayoutGroup layout = horizontal ? (LayoutGroup)content.gameObject.AddComponent<HorizontalLayoutGroup>() : content.gameObject.AddComponent<VerticalLayoutGroup>();
        var group = (HorizontalOrVerticalLayoutGroup)layout; group.spacing = 8;
        group.childControlHeight = group.childControlWidth = true; group.childForceExpandHeight = group.childForceExpandWidth = false;
        var fit = content.gameObject.AddComponent<ContentSizeFitter>();
        fit.horizontalFit = horizontal ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
        fit.verticalFit = horizontal ? ContentSizeFitter.FitMode.Unconstrained : ContentSizeFitter.FitMode.PreferredSize;
        var scroll = root.GetComponent<ScrollRect>(); scroll.content = content; scroll.viewport = viewport;
        scroll.horizontal = horizontal; scroll.vertical = !horizontal; scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 24;
        return content;
    }

    private static void Rect(RectTransform rect, Vector2 position, Vector2 size, Vector2 anchor)
    {
        rect.anchorMin = rect.anchorMax = anchor; rect.pivot = new Vector2(.5f, 1); rect.anchoredPosition = position; rect.sizeDelta = size;
    }
}
#endif
