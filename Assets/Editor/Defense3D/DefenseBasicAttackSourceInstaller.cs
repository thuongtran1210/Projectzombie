using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.UI;

namespace ProjectZombie.EditorTools.Defense3D
{
    /// <summary>Captures visible map surfaces for indicators and applies the source basic attack timing.</summary>
    public static class DefenseBasicAttackSourceInstaller
    {
        private const string REQUEST = "Temp/ApplyBasicAttackSource.request";
        private const string SCENE = "Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity";
        private static readonly string[] SURFACES =
        {
            "Ground_WestBank", "Grass_WestBank", "Slope_WestBank", "Ground_EastBank", "Slope_EastBank",
            "Courtyard_", "BridgeStep_", "Path_", "Village_PathStone_", "ArenaFloor", "HutFloor",
            "West_Bank_Ground", "East_Bank_Ground", "Bridge_01_Ground", "Bridge_02_Ground", "Bridge_03_Ground"
        };
        [InitializeOnLoadMethod]
        private static void Queue() { if (File.Exists(REQUEST)) EditorApplication.update += Process; }
        private static void Process()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            EditorApplication.update -= Process;
            if (!File.Exists(REQUEST)) return;
            File.Delete(REQUEST);
            try { Apply(); }
            catch (Exception exception) { File.WriteAllText("Logs/basic-attack-source-install.txt", "FAILED: " + exception); Debug.LogException(exception); }
        }

        [MenuItem("TREBRAVE/Defense 3D/Apply Source Basic Attack and Rebake Indicator Ground")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before updating source attack bindings.");
            var active = SceneManager.GetActiveScene();
            if (active.path != SCENE && active.isDirty) throw new InvalidOperationException("Save the current scene before rebaking ground indicators.");
            var scene = active.path == SCENE ? active : EditorSceneManager.OpenScene(SCENE, OpenSceneMode.Single);
            var roots = scene.GetRootGameObjects();
            var triangles = new List<Vector3>();
            int surfaces = 0;
            foreach (var meshFilter in roots.SelectMany(root => root.GetComponentsInChildren<MeshFilter>(true)))
            {
                if (!meshFilter.gameObject.activeInHierarchy || meshFilter.name.EndsWith("_outline", StringComparison.Ordinal) ||
                    !SURFACES.Any(prefix => meshFilter.name.StartsWith(prefix, StringComparison.Ordinal))) continue;
                if (!meshFilter.TryGetComponent<Renderer>(out var renderer) || !renderer.enabled || meshFilter.sharedMesh == null) continue;
                var vertices = meshFilter.sharedMesh.vertices;
                var indices = meshFilter.sharedMesh.triangles;
                var matrix = meshFilter.transform.localToWorldMatrix;
                for (int i = 0; i + 2 < indices.Length; i += 3)
                {
                    Vector3 a = matrix.MultiplyPoint3x4(vertices[indices[i]]), b = matrix.MultiplyPoint3x4(vertices[indices[i + 1]]),
                        c = matrix.MultiplyPoint3x4(vertices[indices[i + 2]]);
                    Vector3 normal = Vector3.Cross(b - a, c - a);
                    if (normal.sqrMagnitude < .000001f || Mathf.Abs(normal.normalized.y) < .64f) continue;
                    triangles.Add(a); triangles.Add(b); triangles.Add(c);
                }
                surfaces++;
            }
            if (triangles.Count == 0) throw new InvalidOperationException("No visible bank/path/deck terrain found; refusing an empty surface bake.");
            var surface = roots.SelectMany(root => root.GetComponentsInChildren<DefenseIndicatorSurface3D>(true)).SingleOrDefault();
            if (surface == null) surface = new GameObject("[DEFENSE INDICATOR SURFACE]").AddComponent<DefenseIndicatorSurface3D>();
            var data = new SerializedObject(surface);
            var array = data.FindProperty("_triangles"); array.arraySize = triangles.Count;
            for (int i = 0; i < triangles.Count; i++) array.GetArrayElementAtIndex(i).vector3Value = triangles[i];
            data.ApplyModifiedPropertiesWithoutUndo();
            foreach (var view in roots.SelectMany(root => root.GetComponentsInChildren<DefenseAimIndicator3D>(true)))
            {
                var viewData = new SerializedObject(view); viewData.FindProperty("_surface").objectReferenceValue = surface;
                viewData.ApplyModifiedPropertiesWithoutUndo(); view.Hide();
            }
            foreach (var foot in roots.SelectMany(root => root.GetComponentsInChildren<DefenseHeroFootIndicator3D>(true)))
            {
                var footData = new SerializedObject(foot); footData.FindProperty("_surface").objectReferenceValue = surface;
                footData.ApplyModifiedPropertiesWithoutUndo();
                var visual = footData.FindProperty("_visual").objectReferenceValue as Transform;
                if (visual != null)
                    foreach (var renderer in visual.GetComponentsInChildren<MeshRenderer>(true))
                    {
                        var material = renderer.sharedMaterial;
                        if (material == null || !material.HasProperty("_Shape")) continue;
                        float shape = material.GetFloat("_Shape");
                        material.SetColor("_BaseColor", shape == 2f ? new Color(.333f, .902f, .98f, .18f) :
                            new Color(.502f, 1f, .784f, shape == 1f ? .75f : .95f));
                        EditorUtility.SetDirty(material);
                    }
            }
            foreach (var status in roots.SelectMany(root => root.GetComponentsInChildren<DefenseHeroStatusPresenter3D>(true)))
            {
                var statusData = new SerializedObject(status);
                var footprint = statusData.FindProperty("_attackArea").objectReferenceValue as DefenseAimIndicator3D;
                if (footprint != null) { footprint.Hide(); footprint.gameObject.SetActive(false); }
            }
            foreach (var attack in roots.SelectMany(root => root.GetComponentsInChildren<DefenseHeroAutoAttack3D>(true)))
            {
                var attackData = new SerializedObject(attack);
                attackData.FindProperty("_coneAngle").floatValue = DefenseBasicAttackRules.ConeAngle;
                attackData.FindProperty("_autoAttackEnabled").boolValue = false;
                attackData.FindProperty("attackCooldown").floatValue = DefenseBasicAttackRules.RepeatInterval;
                attackData.ApplyModifiedPropertiesWithoutUndo();
            }
            foreach (var effect in roots.SelectMany(root => root.GetComponentsInChildren<DefenseHeroAttackEffect3D>(true)))
            {
                var effectData = new SerializedObject(effect);
                var view = effectData.FindProperty("_impactView").objectReferenceValue as DefenseAimIndicator3D;
                if (view == null) continue;
                var viewData = new SerializedObject(view);
                var renderer = viewData.FindProperty("_sector").objectReferenceValue as Renderer;
                if (renderer == null) continue;
                renderer.gameObject.name = "BasicAttackSlash";
                renderer.transform.position = surface.Place(effect.transform.position, .75f);
                renderer.enabled = false;
                PrefabUtility.RecordPrefabInstancePropertyModifications(renderer.gameObject);
                PrefabUtility.RecordPrefabInstancePropertyModifications(renderer.transform);
                PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
            }
            var definition = AssetDatabase.LoadAssetAtPath<DefenseSkillDefinitionSO>("Assets/Resources/Defense3D/Skill_BasicAttack.asset");
            var definitionData = new SerializedObject(definition);
            definitionData.FindProperty("_cooldown").floatValue = DefenseBasicAttackRules.RepeatInterval;
            definitionData.FindProperty("_duration").floatValue = DefenseBasicAttackRules.Windup;
            definitionData.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            File.WriteAllText("Logs/basic-attack-source-install.txt", "APPLIED: source press/hold basic attack, .18s hit/.65s repeat/.85s pose, transient waist slash, no idle fan, " + surfaces + " visible ground meshes (" + triangles.Count / 3 + " triangles). Gameplay colliders unchanged; no Play Mode/GUI QA.\n");
        }
    }
}
