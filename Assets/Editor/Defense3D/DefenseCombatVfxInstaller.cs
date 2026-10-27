using System;
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
    public static class DefenseCombatVfxInstaller
    {
        private const string REQUEST = "Temp/ApplyDefenseCombatVfx.request";
        private const string FOLDER = "Assets/VFX/Defense3D";
        private const string SCENE = "Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity";
        [InitializeOnLoadMethod] private static void Queue() { if (File.Exists(REQUEST)) EditorApplication.update += Process; }
        private static void Process()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;
            EditorApplication.update -= Process;
            if (!File.Exists(REQUEST)) return;
            File.Delete(REQUEST);
            try { Apply(); }
            catch (Exception e) { File.WriteAllText("Logs/defense-combat-vfx-install.txt", "FAILED: " + e); Debug.LogException(e); }
        }
        [MenuItem("TREBRAVE/Defense 3D/Apply Combat VFX")]
        public static void Apply()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before installing VFX.");
            var active = SceneManager.GetActiveScene();
            if (active.path != SCENE && active.isDirty) throw new InvalidOperationException("Save current scene first.");
            var scene = active.path == SCENE ? active : EditorSceneManager.OpenScene(SCENE);
            Directory.CreateDirectory(FOLDER); AssetDatabase.Refresh();
            var profile = AssetDatabase.LoadAssetAtPath<DefenseCombatVfxProfileSO>(FOLDER + "/CombatProfile.asset");
            if (profile == null) { profile = ScriptableObject.CreateInstance<DefenseCombatVfxProfileSO>(); AssetDatabase.CreateAsset(profile, FOLDER + "/CombatProfile.asset"); }
            var lineMaterial = Material("ArrowUnlit", Shader.Find("ProjectZombie/Defense3D/CombatLine"));
            var slashMaterial = Material("JadeSlash", Shader.Find("ProjectZombie/Defense3D/CombatSlash"));
            var roots = scene.GetRootGameObjects();
            var director = roots.SelectMany(r => r.GetComponentsInChildren<DefenseTowerProjectileVfx3D>(true)).SingleOrDefault();
            if (director == null) director = new GameObject("[DEFENSE COMBAT VFX]").AddComponent<DefenseTowerProjectileVfx3D>();
            var data = new SerializedObject(director);
            data.FindProperty("_profile").objectReferenceValue = profile;
            Populate(data.FindProperty("_arrows"), director.transform, "Arrow", 32, 5, .04f, profile.arrowColor, lineMaterial);
            Populate(data.FindProperty("_trails"), director.transform, "Trail", 32, 2, .07f, new Color(1f,.8f,.4f,.35f), lineMaterial);
            Populate(data.FindProperty("_impacts"), director.transform, "Impact", 24, 24, .04f, profile.impactColor, lineMaterial);
            data.ApplyModifiedPropertiesWithoutUndo();
            foreach (var tower in roots.SelectMany(r => r.GetComponentsInChildren<DefenseTowerPrototype3D>(true)))
            { var so = new SerializedObject(tower); so.FindProperty("_projectileVfx").objectReferenceValue = director; so.ApplyModifiedPropertiesWithoutUndo(); }
            foreach (var effect in roots.SelectMany(r => r.GetComponentsInChildren<DefenseHeroAttackEffect3D>(true)))
            {
                var so = new SerializedObject(effect);
                var view = so.FindProperty("_impactView").objectReferenceValue as DefenseAimIndicator3D;
                if (view == null) continue;
                var renderer = new SerializedObject(view).FindProperty("_sector").objectReferenceValue as Renderer;
                if (renderer != null) { renderer.sharedMaterial = slashMaterial; renderer.enabled = false; EditorUtility.SetDirty(renderer); }
            }
            var composition = roots.SelectMany(r => r.GetComponentsInChildren<DefenseSceneCompositionRoot>(true)).Single();
            var compositionData = new SerializedObject(composition);
            var participants = compositionData.FindProperty("_sceneParticipants");
            bool found = false;
            for (int i = 0; i < participants.arraySize; i++) if (participants.GetArrayElementAtIndex(i).objectReferenceValue == director) found = true;
            if (!found) { int i = participants.arraySize; participants.arraySize++; participants.GetArrayElementAtIndex(i).objectReferenceValue = director; }
            compositionData.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            File.WriteAllText("Logs/defense-combat-vfx-install.txt", "APPLIED: jade layered slash, 32 pooled arrow/trail slots, 24 independent hit bursts; scoped .22s flight. No Play Mode or GUI tests.\n");
        }
        private static Material Material(string name, Shader shader)
        {
            if (shader == null) throw new InvalidOperationException("Missing combat shader: " + name);
            string path = FOLDER + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null) { material = new Material(shader); AssetDatabase.CreateAsset(material,path); }
            material.shader = shader;
            if (material.HasProperty("_Shape")) material.SetFloat("_Shape",0f);
            EditorUtility.SetDirty(material); return material;
        }
        private static void Populate(SerializedProperty array, Transform parent, string prefix, int count, int points, float width, Color color, Material material)
        {
            array.arraySize = count;
            for (int i = 0; i < count; i++)
            {
                var line = array.GetArrayElementAtIndex(i).objectReferenceValue as LineRenderer;
                if (line == null) { var go = new GameObject(prefix + "_" + i.ToString("00")); go.transform.SetParent(parent,false); line = go.AddComponent<LineRenderer>(); }
                line.useWorldSpace = true; line.positionCount = points; line.sharedMaterial = material;
                line.widthMultiplier = width; line.startColor = color; line.endColor = prefix == "Trail" ? color : color;
                line.widthCurve = prefix == "Trail" ? AnimationCurve.Linear(0,0,1,1) : AnimationCurve.Linear(0,1,1,1);
                if (prefix == "Trail") line.startColor = new Color(color.r,color.g,color.b,0f);
                line.numCapVertices = 2; line.numCornerVertices = 2; line.enabled = false;
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; line.receiveShadows = false;
                array.GetArrayElementAtIndex(i).objectReferenceValue = line;
            }
        }
    }
}
