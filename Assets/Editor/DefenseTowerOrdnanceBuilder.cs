#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectZombie.Features.Defense3D;

/// <summary>Updates only tower ordnance assets; never saves or migrates an open gameplay scene.</summary>
public static class DefenseTowerOrdnanceBuilder
{
    public const string FX = "Assets/_Data/Defense3D/Towers/Effects/";
    public const string PROJECTILES = "Assets/_Prefabs/Defense3D/Projectiles/";
    private const string DATA = "Assets/_Data/Defense3D/Projectiles/";
    [MenuItem("Tools/ProjectZombie/Towers/Build Tower Ordnance")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Requires Edit Mode.");
        Directory.CreateDirectory(FX); Directory.CreateDirectory(PROJECTILES); Directory.CreateDirectory(DATA);
        AssetDatabase.Refresh();
        var catalog = AssetDatabase.LoadAssetAtPath<DefenseTowerCatalogSO>(DefenseTowerArchitectureInstaller.CATALOG);
        var arrow = catalog.Find("tower_cungtien").Data;
        var ice = catalog.Find("tower_bangha").Data;
        var fire = catalog.Find("tower_hoalong")?.Data;
        if (fire == null) throw new InvalidOperationException("Hỏa Long is missing from the tower catalog.");
        var gold = new Color(1f, .72f, .2f, 1f);
        var cyan = new Color(.15f, .8f, 1f, 1f);
        var orange = new Color(1f, .24f, .025f, 1f);
        var arrowHit = Effect("ArrowHit", gold, 0, .22f);
        var iceHit = Effect("IceHit", cyan, 1, .4f);
        var fireHit = Effect("FireExplosion", orange, 2, .65f);
        var burnVisual = Effect("Burn", orange, 2, 3f, true);
        var arrowLaunch = Effect("ArrowLaunch", gold, 0, .12f);
        var iceLaunch = Effect("IceLaunch", cyan, 1, .18f);
        var fireLaunch = Effect("FireLaunch", orange, 2, .2f);
        Projectile(arrow.Attack.Projectile.VisualPrefab, gold, 0);
        Projectile(ice.Attack.Projectile.VisualPrefab, cyan, 1);
        string firePath = PROJECTILES + "P_Proj_Fireball_HoaLong.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(firePath) == null)
        {
            var copy = UnityEngine.Object.Instantiate(ice.Attack.Projectile.VisualPrefab);
            try { PrefabUtility.SaveAsPrefabAsset(copy, firePath); }
            finally { UnityEngine.Object.DestroyImmediate(copy); }
        }
        var firePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(firePath);
        Projectile(firePrefab, orange, 2);
        ConfigureDefinition(arrow.Attack.Projectile, "Mũi tên Nỏ Thần", arrowLaunch, arrowHit, null);
        ConfigureDefinition(ice.Attack.Projectile, "Tinh thể Băng Hà", iceLaunch, iceHit, null);
        var fireDefinition = Asset<DefenseProjectileDefinitionSO>(DATA + "P_Proj_Fireball_HoaLong.asset");
        var so = new SerializedObject(fireDefinition);
        so.FindProperty("_projectileId").stringValue = "tower_hoalong-projectile";
        so.FindProperty("_visualPrefab").objectReferenceValue = firePrefab;
        so.FindProperty("_trajectoryType").enumValueIndex = (int)DefenseTrajectoryType.BallisticGround;
        so.FindProperty("_payloadType").enumValueIndex = (int)DefensePayloadType.AoEExplosion;
        so.FindProperty("_aoeRadius").floatValue = 2f;
        so.FindProperty("_speed").floatValue = fire.ProjectileSpeed;
        so.FindProperty("_arcHeight").floatValue = 2f;
        so.ApplyModifiedPropertiesWithoutUndo();
        ConfigureDefinition(fireDefinition, "Hỏa cầu Hỏa Long", fireLaunch, fireHit, burnVisual);
        var burn = Asset<SO_BurnEffect>(FX + "SO_Effect_Burn_HoaLong.asset");
        so = new SerializedObject(burn);
        so.FindProperty("_triggerChance").floatValue = 1f;
        so.FindProperty("_duration").floatValue = 3f;
        so.FindProperty("_damagePerSecondRatio").floatValue = .2f;
        so.ApplyModifiedPropertiesWithoutUndo();
        so = new SerializedObject(fire.Attack);
        so.FindProperty("_projectile").objectReferenceValue = fireDefinition;
        var effects = so.FindProperty("_effects"); effects.arraySize = 1;
        effects.GetArrayElementAtIndex(0).objectReferenceValue = burn;
        so.ApplyModifiedPropertiesWithoutUndo();
        so = new SerializedObject(fire);
        so.FindProperty("_projectilePrefab").objectReferenceValue = firePrefab;
        so.ApplyModifiedPropertiesWithoutUndo();
        catalog.Validate();
        AssetDatabase.SaveAssets();
        Preview();
        Directory.CreateDirectory("scratch");
        File.WriteAllText("scratch/TowerOrdnanceBuild.txt", "Built arrow/ice/fire projectile visuals, launch/hit profiles, ballistic 2m fire explosion, 3s non-stacking burn at 20% hit damage per second.\nPreview: scratch/TowerOrdnancePreview.png");
    }
    private static T Asset<T>(string path) where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null) { asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset, path); }
        return asset;
    }
    private static Material Material(string name, Color color)
    {
        string path = FX + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Unlit")); AssetDatabase.CreateAsset(material, path); }
        material.SetColor("_BaseColor", color);
        material.SetFloat("_Surface", 1); material.SetFloat("_Blend", 0);
        material.SetFloat("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetFloat("_ZWrite", 0);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.renderQueue = 3000; material.enableInstancing = true;
        EditorUtility.SetDirty(material); return material;
    }
    private static Renderer Part(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
    {
        var obj = GameObject.CreatePrimitive(type); obj.name = name;
        UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
        obj.transform.SetParent(parent, false); obj.transform.localPosition = position; obj.transform.localScale = scale;
        var renderer = obj.GetComponent<Renderer>(); renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; renderer.receiveShadows = false;
        return renderer;
    }
    private static DefenseEffectProfileSO Effect(string name, Color color, int style, float duration, bool sustained = false)
    {
        var root = new GameObject(name);
        try
        {
            var material = Material("TowerImpact", Color.white);
            var flame = style == 2 ? FlameMesh() : null;
            var pieces = new System.Collections.Generic.List<Renderer>();
            int count = style == 0 ? 5 : style == 1 ? 7 : 9;
            for (int i = 0; i < count; i++)
            {
                float a = i * Mathf.PI * 2f / count;
                var direction = new Vector3(Mathf.Cos(a), style == 2 ? .4f + (i % 3) * .18f : .15f, Mathf.Sin(a));
                var piece = Part(root.transform, "Fragment " + i, style == 2 ? PrimitiveType.Sphere : PrimitiveType.Cube,
                    direction * (sustained ? .16f : style == 2 ? .35f : .5f),
                    style == 2 ? new Vector3(.28f, .5f, .28f) : new Vector3(.055f, .055f, style == 1 ? .32f : .2f), material);
                piece.transform.localRotation = Quaternion.LookRotation(direction);
                if (style == 2)
                {
                    piece.GetComponent<MeshFilter>().sharedMesh = flame;
                    piece.transform.localScale = sustained ? new Vector3(.25f, .55f, .25f) : new Vector3(.45f, .7f, .45f);
                    piece.transform.localRotation = Quaternion.Euler(Mathf.Sin(a) * 25f, i * 41f, -Mathf.Cos(a) * 25f);
                }
                pieces.Add(piece);
            }
            if (style == 2 && !sustained)
            {
                var ring = Part(root.transform, "Blast ring", PrimitiveType.Cube, Vector3.up * .02f, Vector3.one, material);
                ring.GetComponent<MeshFilter>().sharedMesh = RingMesh();
                pieces.Add(ring);
            }
            var visual = root.AddComponent<DefenseTowerImpactVisual3D>();
            var so = new SerializedObject(visual);
            var array = so.FindProperty("_pieces"); array.arraySize = pieces.Count;
            for (int i = 0; i < pieces.Count; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = pieces[i];
            so.FindProperty("_lingering").boolValue = sustained; so.ApplyModifiedPropertiesWithoutUndo();
            var instance = root.AddComponent<DefenseEffectInstance3D>();
            so = new SerializedObject(instance); array = so.FindProperty("_visuals"); array.arraySize = 1;
            array.GetArrayElementAtIndex(0).objectReferenceValue = visual; so.ApplyModifiedPropertiesWithoutUndo();
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, FX + name + ".prefab");
            var profile = Asset<DefenseEffectProfileSO>(FX + "FX_Tower" + name + ".asset");
            so = new SerializedObject(profile);
            so.FindProperty("_prefab").objectReferenceValue = prefab.GetComponent<DefenseEffectInstance3D>();
            so.FindProperty("_color").colorValue = color;
            so.FindProperty("_duration").floatValue = duration;
            so.FindProperty("_sustained").boolValue = sustained;
            so.FindProperty("_priority").intValue = style == 2 ? 2 : 1;
            so.ApplyModifiedPropertiesWithoutUndo(); return profile;
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }
    private static void ConfigureDefinition(DefenseProjectileDefinitionSO definition, string title,
        DefenseEffectProfileSO launch, DefenseEffectProfileSO impact, DefenseEffectProfileSO status)
    {
        var so = new SerializedObject(definition);
        so.FindProperty("_displayName").stringValue = title;
        so.FindProperty("_launchEffect").objectReferenceValue = launch;
        so.FindProperty("_impactEffect").objectReferenceValue = impact;
        so.FindProperty("_statusEffect").objectReferenceValue = status;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
    private static Mesh FlameMesh()
    {
        string path = FX + "Flame.asset";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (mesh != null) return mesh;
        mesh = new Mesh { name = "Tapered flame" };
        const int sides = 7;
        var vertices = new Vector3[sides * 4]; var triangles = new int[sides * 3 * 6];
        float[] radii = { .2f, .5f, .28f, .005f }; float[] heights = { 0f, .24f, .65f, 1.15f };
        for (int level = 0; level < 4; level++) for (int i = 0; i < sides; i++)
        {
            float a = (i + level * .15f) * Mathf.PI * 2f / sides;
            vertices[level * sides + i] = new Vector3(Mathf.Cos(a) * radii[level] + level * level * .025f, heights[level], Mathf.Sin(a) * radii[level]);
        }
        int t = 0;
        for (int level = 0; level < 3; level++) for (int i = 0; i < sides; i++)
        {
            int a = level * sides + i, b = level * sides + (i + 1) % sides, c = a + sides, d = b + sides;
            triangles[t++] = a; triangles[t++] = c; triangles[t++] = b;
            triangles[t++] = b; triangles[t++] = c; triangles[t++] = d;
        }
        mesh.vertices = vertices; mesh.triangles = triangles; mesh.RecalculateNormals(); mesh.RecalculateBounds(); AssetDatabase.CreateAsset(mesh, path); return mesh;
    }
    private static Mesh RingMesh()
    {
        string path = FX + "BlastRing.asset";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path); if (mesh != null) return mesh;
        mesh = new Mesh { name = "Blast ring" }; const int sides = 48;
        var vertices = new Vector3[sides * 2]; var triangles = new int[sides * 6];
        for (int i = 0; i < sides; i++)
        {
            float a = i * Mathf.PI * 2 / sides; var d = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
            vertices[i * 2] = d * .45f; vertices[i * 2 + 1] = d * .49f;
            int j = (i + 1) % sides * 2, t = i * 6;
            triangles[t] = i * 2; triangles[t + 1] = j; triangles[t + 2] = i * 2 + 1;
            triangles[t + 3] = i * 2 + 1; triangles[t + 4] = j; triangles[t + 5] = j + 1;
        }
        mesh.vertices = vertices; mesh.triangles = triangles; mesh.RecalculateNormals(); mesh.RecalculateBounds(); AssetDatabase.CreateAsset(mesh, path); return mesh;
    }
    private static Mesh CrystalMesh()
    {
        string path = FX + "IceCrystal.asset";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path); if (mesh != null) return mesh;
        mesh = new Mesh { name = "Ice shard" };
        mesh.vertices = new[] { new Vector3(0, 0, .4f), new Vector3(0, 0, -.3f), new Vector3(.13f, 0, 0), new Vector3(0, .13f, 0), new Vector3(-.13f, 0, 0), new Vector3(0, -.13f, 0) };
        mesh.triangles = new[] { 0, 2, 3, 0, 3, 4, 0, 4, 5, 0, 5, 2, 1, 3, 2, 1, 4, 3, 1, 5, 4, 1, 2, 5 };
        mesh.RecalculateNormals(); mesh.RecalculateBounds(); AssetDatabase.CreateAsset(mesh, path); return mesh;
    }
    private static void Projectile(GameObject prefab, Color color, int style)
    {
        string path = AssetDatabase.GetAssetPath(prefab);
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            // Preserve the actor root/core/GUID; replace only the visual owned by this builder.
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
            foreach (var particle in root.GetComponentsInChildren<ParticleSystem>(true))
            { particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); var emission = particle.emission; emission.enabled = false; }
            var old = root.transform.Find("OrdnanceVisual"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            var model = new GameObject("OrdnanceVisual"); model.transform.SetParent(root.transform, false);
            var material = Material("Projectile" + style, color);
            var body = Part(model.transform, "Core", style == 2 ? PrimitiveType.Sphere : PrimitiveType.Cube, Vector3.zero,
                style == 2 ? new Vector3(.32f, .32f, .48f) : new Vector3(style == 0 ? .06f : .14f, style == 0 ? .06f : .14f, .65f), material);
            if (style == 1) { body.GetComponent<MeshFilter>().sharedMesh = CrystalMesh(); body.transform.localScale = Vector3.one; }
            if (style == 2)
            {
                var tail = Part(model.transform, "Flame tail", PrimitiveType.Cube, Vector3.back * .15f, new Vector3(.32f, .65f, .32f), material);
                tail.GetComponent<MeshFilter>().sharedMesh = FlameMesh(); tail.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            }
            if (style == 0) Part(model.transform, "Arrowhead", PrimitiveType.Cube, Vector3.forward * .35f, new Vector3(.15f, .06f, .15f), material).transform.localRotation = Quaternion.Euler(0, 45, 0);
            var trail = model.AddComponent<TrailRenderer>(); trail.sharedMaterial = material; trail.time = .14f;
            trail.startWidth = style == 2 ? .25f : .06f; trail.endWidth = 0f; trail.minVertexDistance = .08f;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var gradient = new Gradient(); gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(color, 1) }, new[] { new GradientAlphaKey(.8f, 0), new GradientAlphaKey(0, 1) });
            trail.colorGradient = gradient;
            foreach (var previous in root.GetComponentsInChildren<ProjectileVisual3D>(true))
                if (previous.gameObject != root) UnityEngine.Object.DestroyImmediate(previous);
            var visual = root.GetComponent<ProjectileVisual3D>() ?? root.AddComponent<ProjectileVisual3D>();
            var so = new SerializedObject(visual);
            so.FindProperty("_trailRenderer").objectReferenceValue = trail;
            so.FindProperty("_particleSystem").objectReferenceValue = null;
            so.FindProperty("_meshModel").objectReferenceValue = model;
            so.ApplyModifiedPropertiesWithoutUndo();
            so = new SerializedObject(root.GetComponent<ProjectileCore3D>());
            so.FindProperty("_visual").objectReferenceValue = visual; so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
    [MenuItem("Tools/ProjectZombie/Towers/Preview Tower Ordnance")]
    public static void Preview()
    {
        var scene = EditorSceneManager.NewPreviewScene();
        var camObject = new GameObject("Ordnance Preview Camera"); SceneManager.MoveGameObjectToScene(camObject, scene);
        var camera = camObject.AddComponent<Camera>(); camera.scene = scene;
        camera.transform.SetPositionAndRotation(new Vector3(0, 6, -8), Quaternion.Euler(35, 0, 0));
        camera.orthographic = true; camera.orthographicSize = 2.8f;
        camera.backgroundColor = new Color(.09f, .12f, .15f); camera.clearFlags = CameraClearFlags.SolidColor;
        var lightObject = new GameObject("Preview key light"); SceneManager.MoveGameObjectToScene(lightObject, scene);
        var light = lightObject.AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.3f;
        light.transform.rotation = Quaternion.Euler(45, -30, 0);
        const int size = 320;
        var rt = new RenderTexture(size, size, 24); camera.targetTexture = rt;
        var sheet = new Texture2D(size * 3, size * 3, TextureFormat.RGB24, false);
        var tile = new Texture2D(size, size, TextureFormat.RGB24, false);
        var previous = RenderTexture.active;
        try
        {
            string[] names = { "ArrowHit", "IceHit", "FireExplosion" };
            for (int i = 0; i < 9; i++)
            {
                var profile = AssetDatabase.LoadAssetAtPath<DefenseEffectProfileSO>(FX + "FX_Tower" + names[i % 3] + ".asset");
                GameObject subject;
                DefenseEffectInstance3D instance = null;
                if (i < 3)
                {
                    string[] projectileNames = { "P_Proj_Arrow_Archer", "P_Proj_IceShard_BangHa", "P_Proj_Fireball_HoaLong" };
                    subject = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(PROJECTILES + projectileNames[i] + ".prefab"));
                    SceneManager.MoveGameObjectToScene(subject, scene);
                    subject.transform.rotation = Quaternion.Euler(-15, 55, 0);
                    camera.orthographicSize = .8f;
                    camera.transform.position = new Vector3(0, 6, -8);
                    subject.GetComponent<ProjectileVisual3D>().OnSpawn();
                }
                else
                {
                    subject = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Defense3D/DefenseEnemy_Magiap.prefab"));
                    SceneManager.MoveGameObjectToScene(subject, scene);
                    subject.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
                    subject.SetActive(true);
                    camera.orthographicSize = 2.8f;
                    camera.transform.position = new Vector3(0, 6.5f, -8);
                    instance = UnityEngine.Object.Instantiate(profile.Prefab); SceneManager.MoveGameObjectToScene(instance.gameObject, scene);
                    instance.Prepare(profile.Color, i % 3 == 2 ? 4f : 1.2f); instance.Begin(); instance.Render(i < 6 ? .2f : .65f);
                    instance.transform.position = i % 3 == 2 ? Vector3.zero : Vector3.up * .65f;
                }
                camera.Render(); RenderTexture.active = rt; tile.ReadPixels(new Rect(0, 0, size, size), 0, 0); tile.Apply();
                sheet.SetPixels(i % 3 * size, (2 - i / 3) * size, size, size, tile.GetPixels());
                if (instance != null) UnityEngine.Object.DestroyImmediate(instance.gameObject);
                UnityEngine.Object.DestroyImmediate(subject);
            }
            sheet.Apply(); Directory.CreateDirectory("scratch"); File.WriteAllBytes("scratch/TowerOrdnancePreview.png", sheet.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active = previous; camera.targetTexture = null; rt.Release();
            UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(sheet); UnityEngine.Object.DestroyImmediate(tile);
            EditorSceneManager.ClosePreviewScene(scene);
        }
    }
}
#endif
