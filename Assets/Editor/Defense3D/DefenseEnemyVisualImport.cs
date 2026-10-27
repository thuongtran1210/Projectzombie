using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

namespace ProjectZombie.EditorTools.Defense3D
{
    /// <summary>Imports the six EnemyFactory visuals without rebuilding gameplay prefabs or scene data.</summary>
    public static class DefenseEnemyVisualImport
    {
        private const string ART = "Assets/Art/Enemies/Vongxuyen";
        private const string REQUEST = "Temp/ImportEnemyVisuals.request";
        private const string YAKSHA_REQUEST = "Temp/ImportYakshaVisual.request";
        private static readonly string[] IDS = { "magiap", "thuyquai", "huyetdoi", "quaidu", "phicuong", "magiap_boss" };
        private static readonly string[] PREFABS = { "Magiap", "ThuyQuai", "HuyetDoi", "QuaiDu", "PhiCuong", "MagiapBoss" };

        [InitializeOnLoadMethod]
        private static void QueueRequestedImport()
        {
            if (File.Exists(REQUEST) || File.Exists(YAKSHA_REQUEST)) EditorApplication.update += ProcessRequest;
        }

        private static void ProcessRequest()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            EditorApplication.update -= ProcessRequest;
            bool yaksha = File.Exists(YAKSHA_REQUEST);
            if (!yaksha && !File.Exists(REQUEST)) return;
            File.Delete(yaksha ? YAKSHA_REQUEST : REQUEST);
            try { if (yaksha) ImportYaksha(); else Import(); }
            catch (Exception exception)
            {
                File.WriteAllText(yaksha ? "Logs/yaksha-visual-import-result.txt" : "Logs/enemy-visual-import-result.txt", "FAILED: " + exception);
                Debug.LogException(exception);
            }
        }

        [MenuItem("TREBRAVE/Defense 3D/Import Vongxuyen Yaksha Visual")]
        public static void ImportYaksha()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before importing Yaksha.");
            Directory.CreateDirectory(ART + "/Materials");
            Directory.CreateDirectory(ART + "/Prefabs");
            AssetDatabase.Refresh();
            var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Art/Characters/ThuSinh/ScholarToon.shader");
            if (shader == null) throw new InvalidOperationException("Missing shared toon shader.");
            string modelPath = ART + "/yaksha_boss.glb";
            AssetDatabase.ImportAsset(modelPath, ImportAssetOptions.ForceSynchronousImport);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null) throw new InvalidOperationException("Yaksha GLB did not import.");
            string visualPath = BuildVisual(model, shader, "yaksha_boss");
            Attach(visualPath, "yaksha_boss", "Assets/Resources/Defense3D/DefenseYaksha3D.prefab");
            AssetDatabase.SaveAssets();
            File.WriteAllText("Logs/yaksha-visual-import-result.txt", "IMPORTED: yaksha_boss visual and source pose bindings attached; gameplay components and warning ring preserved. No Play Mode or visual QA performed.\n");
        }

        [MenuItem("TREBRAVE/Defense 3D/Import Vongxuyen Enemy Visuals")]
        public static void Import()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before importing enemy visuals.");
            Directory.CreateDirectory(ART + "/Materials");
            Directory.CreateDirectory(ART + "/Prefabs");
            AssetDatabase.Refresh();
            var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/Art/Characters/ThuSinh/ScholarToon.shader");
            if (shader == null) throw new InvalidOperationException("Missing shared toon shader.");
            for (int i = 0; i < IDS.Length; i++)
            {
                string modelPath = ART + "/" + IDS[i] + ".glb";
                AssetDatabase.ImportAsset(modelPath, ImportAssetOptions.ForceSynchronousImport);
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                if (model == null) throw new InvalidOperationException("Model import failed: " + IDS[i]);
                string visualPath = BuildVisual(model, shader, IDS[i]);
                Attach(visualPath, IDS[i], "Assets/Resources/Defense3D/DefenseEnemy_" + PREFABS[i] + ".prefab");
            }
            AssetDatabase.SaveAssets();
            File.WriteAllText("Logs/enemy-visual-import-result.txt", "IMPORTED: " + string.Join(", ", IDS) + ". Visuals and source locomotion attached to the existing prefabs; no Play Mode or visual QA performed.\n");
            Debug.Log("[TREBRAVE] Six Vongxuyen enemy visuals imported.");
        }

        internal static string BuildVisual(GameObject model, Shader shader, string id, string artDirectory = ART)
        {
            var visual = UnityEngine.Object.Instantiate(model);
            try
            {
                visual.name = "VongxuyenVisual";
                var materials = new Dictionary<Material, Material>();
                foreach (var renderer in visual.GetComponentsInChildren<MeshRenderer>(true))
                {
                    var mapped = renderer.sharedMaterials;
                    for (int i = 0; i < mapped.Length; i++)
                    {
                        var original = mapped[i];
                        if (!materials.TryGetValue(original, out var material))
                        {
                            string path = artDirectory + "/Materials/" + original.name + ".mat";
                            material = AssetDatabase.LoadAssetAtPath<Material>(path);
                            if (material == null)
                            {
                                material = new Material(shader);
                                AssetDatabase.CreateAsset(material, path);
                            }
                            material.shader = shader;
                            material.SetColor("_BaseColor", original.HasProperty("_BaseColor") ? original.GetColor("_BaseColor") : original.color);
                            material.SetFloat("_Cull", original.name.EndsWith("_2", StringComparison.Ordinal) ? 0f : 2f);
                            EditorUtility.SetDirty(material);
                            materials.Add(original, material);
                        }
                        mapped[i] = material;
                    }
                    renderer.sharedMaterials = mapped;
                }
                string pathToPrefab = artDirectory + "/Prefabs/" + id + "Visual.prefab";
                PrefabUtility.SaveAsPrefabAsset(visual, pathToPrefab);
                return pathToPrefab;
            }
            finally { UnityEngine.Object.DestroyImmediate(visual); }
        }

        private static void Attach(string visualPath, string id, string prefabPath)
        {
            var actor = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                Bounds targetBounds = GetAuthoredLocalBounds(actor);
                Transform oldVisual = actor.transform.Find("VongxuyenVisual");
                if (oldVisual != null) UnityEngine.Object.DestroyImmediate(oldVisual.gameObject);
                var visual = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(visualPath), actor.transform);
                visual.name = "VongxuyenVisual";
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
                visual.transform.localScale = Vector3.one;
                var renderers = visual.GetComponentsInChildren<Renderer>();
                // Mesh bounds transformed to actor-local space avoid changing the authored actor scale.
                var bounds = new Bounds();
                bool hasBounds = false;
                foreach (var renderer in renderers)
                {
                    var filter = renderer.GetComponent<MeshFilter>();
                    var local = filter.sharedMesh.bounds;
                    var matrix = actor.transform.worldToLocalMatrix * filter.transform.localToWorldMatrix;
                    for (int corner = 0; corner < 8; corner++)
                    {
                        Vector3 point = local.center + Vector3.Scale(local.extents,
                            new Vector3((corner & 1) == 0 ? -1 : 1, (corner & 2) == 0 ? -1 : 1, (corner & 4) == 0 ? -1 : 1));
                        point = matrix.MultiplyPoint3x4(point);
                        if (!hasBounds) { bounds = new Bounds(point, Vector3.zero); hasBounds = true; }
                        else bounds.Encapsulate(point);
                    }
                }
                float scale = targetBounds.size.y / Mathf.Max(0.01f, bounds.size.y);
                visual.transform.localScale = Vector3.one * scale;
                visual.transform.localPosition = targetBounds.center - Vector3.up * (bounds.center.y * scale);
                foreach (Transform child in visual.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = actor.layer;
                actor.GetComponent<MeshRenderer>().enabled = false;
                var motion = actor.GetComponent<DefenseEnemyVisualMotion3D>();
                if (motion == null) motion = actor.AddComponent<DefenseEnemyVisualMotion3D>();
                var data = new SerializedObject(motion);
                data.FindProperty("_enemy").objectReferenceValue = actor.GetComponent<DefenseEnemy3D>();
                var transforms = visual.GetComponentsInChildren<Transform>(true);
                data.FindProperty("_body").objectReferenceValue = transforms.Single(item => item.name == "EnemyBody");
                data.FindProperty("_bossCombat").objectReferenceValue = id == "yaksha_boss" ? actor.GetComponent<DefenseYakshaArenaCombat3D>() : null;
                data.FindProperty("_weapon").objectReferenceValue = id == "yaksha_boss" ? transforms.Single(item => item.name == "Yaksha_WeaponPivot") : null;
                data.FindProperty("_leftWing").objectReferenceValue = transforms.FirstOrDefault(item => item.name == (id == "phicuong" ? "PC_WingL" : "Bat_LeatherWingL"));
                data.FindProperty("_rightWing").objectReferenceValue = transforms.FirstOrDefault(item => item.name == (id == "phicuong" ? "PC_WingR" : "Bat_LeatherWingR"));
                SetMotion(data, id);
                data.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(actor, prefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(actor); }
        }

        private static Bounds GetAuthoredLocalBounds(GameObject actor)
        {
            if (actor.TryGetComponent<CapsuleCollider>(out var capsule) && capsule.enabled)
            {
                var size = Vector3.one * (capsule.radius * 2f);
                size[capsule.direction] = Mathf.Max(capsule.height, capsule.radius * 2f);
                return new Bounds(capsule.center, size);
            }
            if (actor.TryGetComponent<SphereCollider>(out var sphere) && sphere.enabled)
                return new Bounds(sphere.center, Vector3.one * (sphere.radius * 2f));
            if (actor.TryGetComponent<BoxCollider>(out var box) && box.enabled)
                return new Bounds(box.center, box.size);

            // Ground/water/landing prefabs can use logical targeting without a physical collider.
            // Their original primitive mesh defines the visual envelope; keep it in actor-local space.
            if (actor.TryGetComponent<MeshFilter>(out var mesh) && mesh.sharedMesh != null && mesh.sharedMesh.bounds.size.y > 0f)
                return mesh.sharedMesh.bounds;
            if (actor.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent) && agent.height > 0f)
                return new Bounds(Vector3.zero, new Vector3(agent.radius * 2f, agent.height, agent.radius * 2f));
            throw new InvalidOperationException("Enemy has no authored collider, mesh bounds or navigation dimensions: " + actor.name);
        }

        private static void SetMotion(SerializedObject data, string id)
        {
            float bob = 0f, frequency = 0f, sway = 0f, swayFrequency = 0f, wing = 0f, wingFrequency = 0f;
            bool absolute = true;
            Vector3 axis = Vector3.forward;
            switch (id)
            {
                case "magiap": bob = .28f; frequency = 6f; sway = .06f; swayFrequency = 6f; break;
                case "thuyquai": bob = .08f; frequency = 8f; sway = .18f; swayFrequency = 9.6f; axis = Vector3.up; break;
                case "huyetdoi": bob = .14f; frequency = 12f; absolute = false; wing = .65f; wingFrequency = 22f; break;
                case "quaidu": bob = .18f; frequency = 11.2f; sway = .08f; swayFrequency = 11.2f; break;
                case "phicuong": bob = .16f; frequency = 9.6f; absolute = false; sway = .08f; swayFrequency = 7.2f; wing = .55f; wingFrequency = 16f; break;
                case "yaksha_boss": bob = .12f; frequency = 6.5f; sway = .03f; swayFrequency = 6.5f; break;
            }
            data.FindProperty("_bobAmplitude").floatValue = bob;
            data.FindProperty("_bobFrequency").floatValue = frequency;
            data.FindProperty("_absoluteBob").boolValue = absolute;
            data.FindProperty("_swayAxis").vector3Value = axis;
            // glTFast reflects X when converting Three.js to Unity, reversing Y/Z rotations.
            data.FindProperty("_swayAmplitude").floatValue = -sway * Mathf.Rad2Deg;
            data.FindProperty("_swayFrequency").floatValue = swayFrequency;
            data.FindProperty("_wingAmplitude").floatValue = -wing * Mathf.Rad2Deg;
            data.FindProperty("_wingFrequency").floatValue = wingFrequency;
        }
    }
}
