using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

namespace ProjectZombie.EditorTools.Defense3D
{
    /// <summary>Builds a reusable Scholar visual from the exported source and attaches it to Defense3D.</summary>
    public static class DefenseScholarImport
    {
        private const string ART = "Assets/Art/Characters/ThuSinh";
        private const string MODEL = ART + "/ThuSinh_Vongxuyen.glb";
        private const string PREFAB = ART + "/ThuSinhVisual.prefab";
        private const string SCENE = "Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity";
        private static readonly string[] POSES = { "Idle", "Run", "Dash", "Attack", "Skill", "Die" };

        [MenuItem("TREBRAVE/Defense 3D/Import Vongxuyen Scholar")]
        public static void Import()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before importing Scholar.");
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory(ART + "/Materials");
            Directory.CreateDirectory(ART + "/Animations");
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(MODEL, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(MODEL);
            if (model == null) throw new InvalidOperationException("glTFast did not import the Scholar model.");
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(ART + "/ScholarToon.shader");
            if (shader == null || ShaderUtil.ShaderHasError(shader)) throw new InvalidOperationException("Scholar shader has import errors.");
            var controller = BuildController();
            BuildPrefab(model, shader, controller);
            AttachToScene();
            AssetDatabase.SaveAssets();
            ValidateScene();
            File.WriteAllText("Logs/scholar-import-validation.txt", "PASS: imported model, 6 animation states, materials, prefab, scene binding, capsule renderer hidden; gameplay physics preserved.\n");
            Debug.Log("[TREBRAVE Scholar] Import and scene validation PASS.");
        }

        private static AnimatorController BuildController()
        {
            string path = ART + "/Animations/ThuSinh.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null) controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            var stateMachine = controller.layers[0].stateMachine;
            var imported = AssetDatabase.LoadAllAssetsAtPath(MODEL).OfType<AnimationClip>().ToArray();
            foreach (string pose in POSES)
            {
                var source = imported.SingleOrDefault(clip => clip.name == pose);
                if (source == null) throw new InvalidOperationException("Missing imported animation: " + pose);
                string clipPath = ART + "/Animations/" + pose + ".anim";
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if (clip == null)
                {
                    clip = UnityEngine.Object.Instantiate(source);
                    AssetDatabase.CreateAsset(clip, clipPath);
                }
                else EditorUtility.CopySerialized(source, clip);
                clip.name = pose;
                clip.legacy = false;
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = pose == "Idle" || pose == "Run";
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                EditorUtility.SetDirty(clip);
                var state = stateMachine.states.Select(child => child.state).SingleOrDefault(item => item.name == pose);
                if (state == null) state = stateMachine.AddState(pose);
                state.motion = clip;
                state.writeDefaultValues = true;
                if (pose == "Idle") stateMachine.defaultState = state;
            }
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void BuildPrefab(GameObject model, Shader shader, AnimatorController controller)
        {
            var visual = UnityEngine.Object.Instantiate(model);
            try
            {
                visual.name = "ThuSinhVisual";
                var materials = new Dictionary<Material, Material>();
                foreach (var renderer in visual.GetComponentsInChildren<MeshRenderer>(true))
                {
                    var mapped = renderer.sharedMaterials;
                    for (int i = 0; i < mapped.Length; i++)
                    {
                        var original = mapped[i];
                        if (!materials.TryGetValue(original, out var converted))
                        {
                            string path = ART + "/Materials/" + original.name + ".mat";
                            converted = AssetDatabase.LoadAssetAtPath<Material>(path);
                            if (converted == null)
                            {
                                converted = new Material(shader);
                                AssetDatabase.CreateAsset(converted, path);
                            }
                            converted.shader = shader;
                            Color color = original.HasProperty("_BaseColor") ? original.GetColor("_BaseColor") : original.color;
                            converted.SetColor("_BaseColor", color);
                            EditorUtility.SetDirty(converted);
                            materials.Add(original, converted);
                        }
                        mapped[i] = converted;
                    }
                    renderer.sharedMaterials = mapped;
                }
                var animator = visual.GetComponentInChildren<Animator>(true);
                if (animator == null) animator = visual.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                PrefabUtility.SaveAsPrefabAsset(visual, PREFAB);
            }
            finally { UnityEngine.Object.DestroyImmediate(visual); }
        }

        private static void AttachToScene()
        {
            var scene = EditorSceneManager.OpenScene(SCENE, OpenSceneMode.Single);
            var health = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<DefenseHeroHealth3D>(true)).Single();
            var hero = health.gameObject;
            var composition = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<DefenseSceneCompositionRoot>(true)).Single();
            Transform previous = hero.transform.Find("ThuSinhVisual");
            if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PREFAB), hero.transform);
            visual.name = "ThuSinhVisual";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;
            var renderers = visual.GetComponentsInChildren<Renderer>();
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
            var foot = renderers.First(renderer => renderer.name == "L_Leg").bounds.min.y;
            var capsule = hero.GetComponent<CapsuleCollider>();
            float scale = capsule.height / (bounds.max.y - foot);
            visual.transform.localScale = Vector3.one * scale;
            visual.transform.localPosition = Vector3.up * (capsule.center.y - capsule.height * 0.5f - (foot - hero.transform.position.y) * scale);
            foreach (Transform child in visual.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = hero.layer;
            hero.GetComponent<MeshRenderer>().enabled = false;
            var driver = hero.GetComponent<DefenseScholarVisual3D>();
            if (driver == null) driver = hero.AddComponent<DefenseScholarVisual3D>();
            var driverData = new SerializedObject(driver);
            Set(driverData, "_animator", visual.GetComponentInChildren<Animator>(true));
            Set(driverData, "_body", hero.GetComponent<Rigidbody>());
            Set(driverData, "_health", health);
            Set(driverData, "_motor", hero.GetComponent<DefenseHeroMotor3D>());
            Set(driverData, "_attack", hero.GetComponent<DefenseHeroAutoAttack3D>());
            Set(driverData, "_signature", hero.GetComponent<DefenseScholarSignatureEffect3D>());
            var compositionData = new SerializedObject(composition);
            Set(driverData, "_kit", compositionData.FindProperty("_heroKit").objectReferenceValue);
            driverData.ApplyModifiedPropertiesWithoutUndo();
            var participants = compositionData.FindProperty("_sceneParticipants");
            bool registered = false;
            for (int i = 0; i < participants.arraySize; i++) registered |= participants.GetArrayElementAtIndex(i).objectReferenceValue == driver;
            if (!registered)
            {
                participants.InsertArrayElementAtIndex(participants.arraySize);
                participants.GetArrayElementAtIndex(participants.arraySize - 1).objectReferenceValue = driver;
                compositionData.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void Set(SerializedObject data, string field, UnityEngine.Object value)
        {
            if (value == null) throw new InvalidOperationException("Missing Scholar binding: " + field);
            data.FindProperty(field).objectReferenceValue = value;
        }

        private static void ValidateScene()
        {
            var driver = UnityEngine.Object.FindObjectOfType<DefenseScholarVisual3D>();
            if (driver == null) throw new InvalidOperationException("Scene has no Scholar visual driver.");
            if (driver.GetComponent<MeshRenderer>().enabled) throw new InvalidOperationException("Capsule art still visible.");
            var animator = driver.GetComponentInChildren<Animator>();
            if (animator == null || animator.applyRootMotion || animator.runtimeAnimatorController.animationClips.Length != 6)
                throw new InvalidOperationException("Scholar Animator binding failed.");
            foreach (var renderer in driver.GetComponentsInChildren<MeshRenderer>())
            {
                if (renderer == driver.GetComponent<MeshRenderer>()) continue;
                if (renderer.sharedMaterials.Any(material => material == null || material.shader.name != "TREBRAVE/Defense3D/ScholarToon"))
                    throw new InvalidOperationException("Missing Scholar material: " + renderer.name);
            }
            if (driver.GetComponent<CapsuleCollider>() == null || driver.GetComponent<Rigidbody>() == null)
                throw new InvalidOperationException("Hero physics were removed.");
        }
    }
}
