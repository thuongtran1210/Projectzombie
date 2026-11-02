#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using ProjectZombie.Features.Defense3D;

public static class DefenseIceLaserBuilder
{
    public const string PROFILE = "Assets/_Data/Defense3D/Towers/Effects/FX_TowerIceLaser.asset";
    [MenuItem("Tools/ProjectZombie/Towers/Build Ice Laser")]
    public static void Build()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Requires Edit Mode.");
        string folder = "Assets/_Data/Defense3D/Towers/Effects/";
        var material = AssetDatabase.LoadAssetAtPath<Material>(folder + "IceLaser.mat");
        if (material == null) { material = new Material(Shader.Find("ProjectZombie/Defense3D/TowerLaser")); AssetDatabase.CreateAsset(material, folder + "IceLaser.mat"); }
        material.shader = Shader.Find("ProjectZombie/Defense3D/TowerLaser");
        material.SetColor("_BaseColor", Color.white); material.renderQueue = 3000; EditorUtility.SetDirty(material);
        var root = new GameObject("IceLaser");
        try
        {
            var visual = root.AddComponent<DefenseTowerLaserVisual3D>();
            var so = new SerializedObject(visual);
            foreach (string name in new[] { "_core", "_halo" })
            {
                var obj = new GameObject(name); obj.transform.SetParent(root.transform, false);
                var line = obj.AddComponent<LineRenderer>(); line.sharedMaterial = material; line.useWorldSpace = false;
                line.positionCount = 2; line.widthCurve = AnimationCurve.Linear(0,1,1,1); line.numCapVertices = 3;
                line.shadowCastingMode = ShadowCastingMode.Off; line.receiveShadows = false;
                so.FindProperty(name).objectReferenceValue = line;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            var glowMaterial = AssetDatabase.LoadAssetAtPath<Material>(folder + "IceLaserGlow.mat");
            if (glowMaterial == null) { glowMaterial = new Material(Shader.Find("ProjectZombie/Defense3D/IceLaserGlow")); AssetDatabase.CreateAsset(glowMaterial, folder + "IceLaserGlow.mat"); }
            glowMaterial.shader = Shader.Find("ProjectZombie/Defense3D/IceLaserGlow"); EditorUtility.SetDirty(glowMaterial);
            so = new SerializedObject(visual);
            foreach (string name in new[] { "_startGlow", "_endGlow" })
            {
                var obj = GameObject.CreatePrimitive(PrimitiveType.Quad); UnityEngine.Object.DestroyImmediate(obj.GetComponent<Collider>());
                obj.name = name; obj.transform.SetParent(root.transform, false); obj.transform.localScale = Vector3.one * (name == "_startGlow" ? .7f : 1.1f);
                var renderer = obj.GetComponent<MeshRenderer>(); renderer.sharedMaterial = glowMaterial;
                renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
                so.FindProperty(name).objectReferenceValue = renderer;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            var instance = root.AddComponent<DefenseEffectInstance3D>(); so = new SerializedObject(instance);
            var array = so.FindProperty("_visuals"); array.arraySize = 1; array.GetArrayElementAtIndex(0).objectReferenceValue = visual; so.ApplyModifiedPropertiesWithoutUndo();
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, folder + "IceLaser.prefab");
            var profile = AssetDatabase.LoadAssetAtPath<DefenseEffectProfileSO>(PROFILE);
            if (profile == null) { profile = ScriptableObject.CreateInstance<DefenseEffectProfileSO>(); AssetDatabase.CreateAsset(profile, PROFILE); }
            so = new SerializedObject(profile); so.FindProperty("_prefab").objectReferenceValue = prefab.GetComponent<DefenseEffectInstance3D>();
            so.FindProperty("_duration").floatValue = .24f; so.FindProperty("_color").colorValue = new Color(.1f,.8f,1f,1);
            so.FindProperty("_priority").intValue = 4; so.FindProperty("_sustained").boolValue = false; so.ApplyModifiedPropertiesWithoutUndo();
            var catalog = AssetDatabase.LoadAssetAtPath<DefenseTowerCatalogSO>(DefenseTowerArchitectureInstaller.CATALOG);
            var ice = catalog.Find("tower_bangha").Data;
            so = new SerializedObject(ice.Attack); so.FindProperty("_mode").enumValueIndex = (int)DefenseTowerAttackMode.Laser;
            so.FindProperty("_beamEffect").objectReferenceValue = profile; so.ApplyModifiedPropertiesWithoutUndo();
            so = new SerializedObject(ice); so.FindProperty("_description").stringValue = "Laser băng làm chậm. Cấp 1/2/3 bắn 1/2/3 tia, ưu tiên mục tiêu khác nhau."; so.ApplyModifiedPropertiesWithoutUndo();
            catalog.Validate(); AssetDatabase.SaveAssets(); Preview();
            File.WriteAllText("scratch/IceLaserBuild.txt", "Băng Hà now uses instant laser pulses: 1/2/3 beams by level, distinct targets first, remaining beams focus primary; slow once per target. Preview: scratch/IceLaserPreview.png");
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }
    [MenuItem("Tools/ProjectZombie/Towers/Preview Ice Laser")]
    public static void Preview()
    {
        var scene = EditorSceneManager.NewPreviewScene(); var cameraObject = new GameObject("Laser preview"); SceneManager.MoveGameObjectToScene(cameraObject,scene);
        var camera = cameraObject.AddComponent<Camera>(); camera.scene = scene; camera.orthographic = true; camera.orthographicSize = 3.8f;
        camera.transform.position = new Vector3(6,8,-9); camera.transform.LookAt(new Vector3(1,1,1));
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.1f,.15f,.18f);
        var rt = new RenderTexture(640,480,24); camera.targetTexture = rt; var sheet = new Texture2D(1920,480,TextureFormat.RGB24,false); var tile = new Texture2D(640,480,TextureFormat.RGB24,false); var previous = RenderTexture.active;
        try
        {
            var data = AssetDatabase.LoadAssetAtPath<DefenseTowerCatalogSO>(DefenseTowerArchitectureInstaller.CATALOG).Find("tower_bangha").Data;
            var profile = data.Attack.BeamEffect;
            for (int level=1;level<=3;level++)
            {
                var group = new GameObject("Preview tier " + level); SceneManager.MoveGameObjectToScene(group,scene);
                var tower = UnityEngine.Object.Instantiate(data.GetLevelStats(level).visualModelPrefab,group.transform);
                Vector3 origin = tower.transform.Find("Turret/Muzzle").position;
                for (int i=0;i<level;i++)
                {
                    Vector3 endpoint = new Vector3(3, .65f, (i - (level - 1)*.5f)*1.7f);
                    var enemy = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Defense3D/DefenseEnemy_Magiap.prefab"),group.transform);
                    enemy.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false; enemy.transform.position = endpoint - Vector3.up*.65f; enemy.SetActive(true);
                    var beam = UnityEngine.Object.Instantiate(profile.Prefab,group.transform);
                    Vector3 direction=endpoint-origin; beam.Prepare(profile.Color,direction.magnitude); beam.transform.SetPositionAndRotation(origin,Quaternion.LookRotation(direction)); beam.Begin(); beam.Render(.2f);
                }
                camera.Render(); RenderTexture.active=rt; tile.ReadPixels(new Rect(0,0,640,480),0,0);tile.Apply();sheet.SetPixels((level-1)*640,0,640,480,tile.GetPixels());UnityEngine.Object.DestroyImmediate(group);
            }
            sheet.Apply();Directory.CreateDirectory("scratch");File.WriteAllBytes("scratch/IceLaserPreview.png",sheet.EncodeToPNG());
        }
        finally { RenderTexture.active=previous;camera.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tile);UnityEngine.Object.DestroyImmediate(sheet);EditorSceneManager.ClosePreviewScene(scene); }
    }
}
#endif
