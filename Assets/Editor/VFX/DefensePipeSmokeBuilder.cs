#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.UI;
using Object = UnityEngine.Object;

/// <summary>Rebuilds only the pipe's authored VFX and adds optional area/HUD views without replacing other skills.</summary>
public static class DefensePipeSmokeBuilder
{
    public const string FOLDER = "Assets/Features/Defense3D/Effects/Assets/PipeSmoke/";
    public const string PROFILE = FOLDER + "FX_PipeSmoke.asset";

    [MenuItem("Tools/VFX Generator/Build Pipe Smoke Presentation")]
    public static void Build()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Requires Edit Mode.");
        Directory.CreateDirectory(FOLDER);
        AssetDatabase.Refresh();
        Shader shader = Shader.Find("ProjectZombie/Defense3D/PipeSmoke");
        if (shader == null) throw new InvalidOperationException("Import PipeSmoke shader before building.");
        var quadObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Mesh quad = quadObject.GetComponent<MeshFilter>().sharedMesh;
        Object.DestroyImmediate(quadObject);
        var root = new GameObject("VFX_PipeSmoke_VongXuyen");
        try
        {
            var instance = root.AddComponent<DefenseEffectInstance3D>();
            var visual = root.AddComponent<DefensePipeSmokeVisual3D>();
            var serialized = new SerializedObject(visual);
            foreach (string phase in new[] { "Ground", "Smoke", "Embers" })
            {
                int index = phase == "Ground" ? 0 : phase == "Smoke" ? 1 : 2;
                var material = Asset<Material>(FOLDER + "MAT_Pipe" + phase + ".mat", () => new Material(shader));
                material.shader = shader;
                material.SetFloat("_Phase", index);
                material.enableInstancing = true;
                EditorUtility.SetDirty(material);
                Mesh mesh = index == 0 ? quad : BuildCards(index == 2);
                var renderer = Renderer(phase, root.transform, mesh, material);
                if (index == 0) renderer.transform.localRotation = Quaternion.Euler(90, 0, 0);
                serialized.FindProperty(index == 0 ? "_ground" : index == 1 ? "_smoke" : "_embers").objectReferenceValue = renderer;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var effectData = new SerializedObject(instance);
            effectData.FindProperty("_visuals").arraySize = 1;
            effectData.FindProperty("_visuals").GetArrayElementAtIndex(0).objectReferenceValue = visual;
            effectData.ApplyModifiedPropertiesWithoutUndo();
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, FOLDER + "VFX_PipeSmoke.prefab");
            var definition = AssetDatabase.LoadAssetAtPath<DefenseSkillDefinitionSO>("Assets/Resources/Defense3D/Skill_DieuCay.asset");
            var profile = Asset<DefenseEffectProfileSO>(PROFILE, () => ScriptableObject.CreateInstance<DefenseEffectProfileSO>());
            var data = new SerializedObject(profile);
            data.FindProperty("_prefab").objectReferenceValue = prefab.GetComponent<DefenseEffectInstance3D>();
            data.FindProperty("_duration").floatValue = definition.Capture().Duration;
            data.FindProperty("_sustained").boolValue = true;
            data.FindProperty("_animateSustained").boolValue = true;
            data.FindProperty("_priority").intValue = 6;
            data.FindProperty("_scale").floatValue = 1f;
            data.ApplyModifiedPropertiesWithoutUndo();
            var skillData = new SerializedObject(definition);
            skillData.FindProperty("_presentationProfile").objectReferenceValue = profile;
            skillData.FindProperty("_icon").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Weapons/Icon_W_PIPE.png");
            skillData.ApplyModifiedPropertiesWithoutUndo();
            BuildIndicator(quad);
            BuildHud();
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("scratch");
            File.WriteAllText("scratch/PipeSmokeBuild.txt", "Pipe smoke assets, area preview and optional skill HUD authored. Gameplay values preserved.\n" + PROFILE);
        }
        finally { Object.DestroyImmediate(root); }
    }

    private static T Asset<T>(string path, Func<T> create) where T : Object
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null) return asset;
        asset = create();
        asset.name = Path.GetFileNameWithoutExtension(path);
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static MeshRenderer Renderer(string name, Transform parent, Mesh mesh, Material material)
    {
        var node = new GameObject(name);
        node.transform.SetParent(parent, false);
        node.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = node.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.enabled = false;
        return renderer;
    }

    private static Mesh BuildCards(bool embers)
    {
        int count = embers ? 16 : 12;
        var vertices = new List<Vector3>();
        var uv = new List<Vector2>();
        var seeds = new List<Vector2>();
        var triangles = new List<int>();
        Quaternion plane = Quaternion.Euler(45, 0, 0);
        for (int i = 0; i < count; i++)
        {
            float angle = i * 2.399963f;
            float radius = embers ? 2.8f : (i < 8 ? 2.25f : .85f);
            Vector3 center = new Vector3(Mathf.Cos(angle) * radius, embers ? .3f : .45f + (i % 3) * .26f, Mathf.Sin(angle) * radius);
            float width = embers ? .07f : 2.3f;
            float height = embers ? .17f : 1.55f;
            int start = vertices.Count;
            foreach (Vector2 corner in new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) })
            {
                vertices.Add(center + plane * new Vector3((corner.x - .5f) * width, (corner.y - .5f) * height, 0));
                uv.Add(corner);
                seeds.Add(new Vector2(i / (float)count, 0));
            }
            triangles.AddRange(new[] { start, start + 2, start + 1, start + 1, start + 2, start + 3 });
        }
        string path = FOLDER + (embers ? "Mesh_PipeEmbers.asset" : "Mesh_PipeClouds.asset");
        var mesh = Asset<Mesh>(path, () => new Mesh());
        mesh.Clear(); mesh.SetVertices(vertices); mesh.SetUVs(0, uv); mesh.SetUVs(1, seeds); mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        // Vertex shader raises embers; include their travel in CPU-side culling bounds.
        mesh.bounds = new Bounds(new Vector3(0, 1, 0), new Vector3(10, 6, 10));
        EditorUtility.SetDirty(mesh);
        return mesh;
    }

    private static void BuildIndicator(Mesh quad)
    {
        const string path = "Assets/Resources/Defense3D/DefenseAimIndicator3D.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var data = new SerializedObject(root.GetComponent<DefenseAimIndicator3D>());
            foreach (string name in new[] { "PipeAreaFill", "PipeAreaCenter" })
            {
                var previous = root.transform.Find(name);
                if (previous != null) Object.DestroyImmediate(previous.gameObject);
                var material = Asset<Material>(FOLDER + "MAT_" + name + ".mat", () => new Material(Shader.Find("ProjectZombie/Defense3D/AimOverlay")));
                material.SetFloat("_Shape", name.EndsWith("Fill") ? 2 : 1);
                var renderer = Renderer(name, root.transform, quad, material);
                data.FindProperty(name.EndsWith("Fill") ? "_areaFill" : "_areaCenter").objectReferenceValue = renderer;
                EditorUtility.SetDirty(material);
            }
            data.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static void BuildHud()
    {
        string texturePath = FOLDER + "UI_PipeDurationRing.png";
        var texture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        for (int y = 0; y < 128; y++)
            for (int x = 0; x < 128; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(63.5f, 63.5f));
                float alpha = Mathf.Clamp01(62f - d) * Mathf.Clamp01(d - 56f);
                texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        texture.Apply();
        File.WriteAllBytes(texturePath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(texturePath);
        var importer = (TextureImporter)AssetImporter.GetAtPath(texturePath);
        importer.textureType = TextureImporterType.Sprite; importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true; importer.SaveAndReimport();
        var ringSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
        const string path = "Assets/Resources/Defense3D/DefenseSkillControlsCanvas.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            Transform button = root.transform.Find("DepToOngButton");
            var view = button.GetComponent<DefenseSkillButtonView>();
            var data = new SerializedObject(view);
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/GameFont_Vietnamese_SD.asset");
            button.GetComponent<Image>().color = new Color(.07f, .14f, .12f, 1f);
            var identity = button.Find("Label").GetComponent<TMP_Text>();
            identity.fontSize = 22;
            identity.rectTransform.anchoredPosition = new Vector2(0, -93);
            identity.rectTransform.sizeDelta = new Vector2(190, 28);
            foreach (string name in new[] { "PipeStatus", "PipeDuration", "PipeAimBorder", "PipeFrame" })
            {
                var previous = button.Find(name);
                if (previous != null) Object.DestroyImmediate(previous.gameObject);
            }
            var status = new GameObject("PipeStatus", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            status.transform.SetParent(button, false);
            var text = status.GetComponent<TextMeshProUGUI>();
            text.font = font; text.fontSize = 15; text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false; text.text = "";
            var rect = text.rectTransform; rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.anchoredPosition = new Vector2(0, -122); rect.sizeDelta = new Vector2(190, 22);
            data.FindProperty("_stateLabel").objectReferenceValue = text;
            foreach (string name in new[] { "PipeFrame", "PipeDuration", "PipeAimBorder" })
            {
                var node = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                node.transform.SetParent(button, false);
                var image = node.GetComponent<Image>(); image.sprite = ringSprite;
                image.raycastTarget = false; image.color = new Color(1, .76f, .3f, 1);
                image.rectTransform.anchorMin = image.rectTransform.anchorMax = new Vector2(.5f, .5f);
                image.rectTransform.sizeDelta = Vector2.one * (name == "PipeDuration" ? 154 : name == "PipeFrame" ? 144 : 166);
                if (name == "PipeDuration")
                {
                    image.type = Image.Type.Filled; image.fillMethod = Image.FillMethod.Radial360;
                    image.fillOrigin = 2; image.fillClockwise = false; image.enabled = false;
                    data.FindProperty("_durationRing").objectReferenceValue = image;
                }
                else if (name == "PipeAimBorder")
                {
                    data.FindProperty("_aimBorder").objectReferenceValue = image;
                    node.SetActive(false);
                }
                else image.color = new Color(.7f, .52f, .23f, .9f);
            }
            data.FindProperty("_nameLabel").objectReferenceValue = button.Find("Label").GetComponent<TMP_Text>();
            data.FindProperty("_iconImage").objectReferenceValue = button.Find("Icon").GetComponent<Image>();
            data.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
}
#endif
