using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Player;

namespace ProjectZombie.EditorTools.Defense3D
{
    /// <summary>
    /// Generates an isolated, primitive-art Defense3D prototype. Does not overwrite
    /// SampleScene, Legacy Survival objects, or existing stage Addressable assets.
    /// </summary>
    public static class DefenseMapSongQueSceneGenerator
    {
        public const string SceneFolder = "Assets/Scenes/Defense3D";
        public const string ScenePath = SceneFolder + "/MapSongQue3D_Prototype.unity";

        [MenuItem("TREBRAVE/Defense 3D/Build & Open Map Song Que Prototype")]
        public static void BuildAndOpen()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[TREBRAVE] Exit Play Mode before generating Defense 3D.");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            EnsureFolder();
            // Avoid overwriting manually edited prototype scenes without consent.
            if (System.IO.File.Exists(ScenePath) &&
                !EditorUtility.DisplayDialog("TREBRAVE Defense 3D",
                    "MapSongQue3D_Prototype already exists. Rebuild and overwrite this prototype scene?",
                    "Rebuild", "Cancel"))
                return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var world = new GameObject("[TREBRAVE] Map Song Que Defense 3D");
            var sceneInfo = world.AddComponent<DefenseSceneInfo3D>();

            int groundLayer = LayerMask.NameToLayer("Ground");
            int waterLayer = LayerMask.NameToLayer("Water");

            // X spans about -30..30, Z -20..20. River occupies X 4.45..8.05.
            // Both banks are collider floors; bridges provide the only solid crossings.
            var west = Box("West_Bank_Ground", new Vector3(-12.775f, -0.35f, 0f),
                new Vector3(34.45f, 0.7f, 40f), new Color(0.24f, 0.42f, 0.25f), world.transform, groundLayer);
            var east = Box("East_Bank_Ground", new Vector3(19.025f, -0.35f, 0f),
                new Vector3(21.95f, 0.7f, 40f), new Color(0.32f, 0.43f, 0.25f), world.transform, groundLayer);
            Box("Song_Que_Water_Visual", new Vector3(6.25f, -0.52f, 0f),
                new Vector3(3.6f, 0.12f, 40f), new Color(0.07f, 0.32f, 0.48f), world.transform, waterLayer,
                includeCollider: false);

            Transform[] bridgeMarkers = new Transform[3];
            float[] bridgeZ = { -10.5f, -0.5f, 10.5f };
            for (int i = 0; i < bridgeMarkers.Length; i++)
            {
                var bridge = Box("Bridge_" + (i + 1).ToString("00") + "_Ground",
                    new Vector3(6.25f, -0.12f, bridgeZ[i]), new Vector3(4.4f, 0.24f, 2.8f),
                    new Color(0.48f, 0.31f, 0.19f), world.transform, groundLayer);
                bridgeMarkers[i] = bridge.transform;
            }

            Transform baseMarker = Box("Main_Base_West", new Vector3(-12f, 1.3f, 0f),
                new Vector3(3.2f, 2.6f, 3.2f), new Color(0.7f, 0.53f, 0.25f), world.transform, -1).transform;
            baseMarker.gameObject.AddComponent<DefenseBasePrototype3D>();

            var hero = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            hero.name = "Hero_3D_Prototype";
            hero.SetActive(false);
            hero.transform.position = new Vector3(-18f, 1.2f, 0f);
            hero.tag = "Player";
            var playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0) hero.layer = playerLayer;
            Paint(hero, new Color(0.15f, 0.75f, 0.85f));
            Rigidbody heroBody = hero.AddComponent<Rigidbody>();
            heroBody.useGravity = true;
            heroBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            hero.AddComponent<DefenseHeroMotor3D>();
            hero.SetActive(true);

            // Arena marker is informational: enemy AI/waves are not present in this PR.
            var eastEnemySpawn = new GameObject("EnemySpawn_East_Arena");
            eastEnemySpawn.transform.SetParent(world.transform);
            eastEnemySpawn.transform.position = new Vector3(25f, 0.1f, 0f);
            var arena = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            arena.name = "East_Arena_Visual";
            arena.transform.SetParent(world.transform);
            arena.transform.position = new Vector3(25f, 0.03f, 0f);
            arena.transform.localScale = new Vector3(3.3f, 0.04f, 3.3f);
            Paint(arena, new Color(0.43f, 0.19f, 0.23f));
            UnityEngine.Object.DestroyImmediate(arena.GetComponent<Collider>());

            Transform[] sockets = new Transform[10];
            Vector3[] locations =
            {
                new Vector3(-24f, 0.15f, -14f), new Vector3(-19f, 0.15f, -7f),
                new Vector3(-19f, 0.15f, 7f), new Vector3(-24f, 0.15f, 14f),
                new Vector3(-8f, 0.15f, -15f), new Vector3(-3f, 0.15f, -9f),
                new Vector3(-3f, 0.15f, 9f), new Vector3(-8f, 0.15f, 15f),
                new Vector3(0f, 0.15f, -2f), new Vector3(0f, 0.15f, 2f)
            };
            for (int i = 0; i < sockets.Length; i++)
            {
                var socket = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                socket.name = "TowerSocket_" + (i + 1).ToString("00") + "_VisualOnly";
                socket.transform.SetParent(world.transform);
                socket.transform.position = locations[i];
                socket.transform.localScale = new Vector3(0.7f, 0.12f, 0.7f);
                Paint(socket, new Color(0.78f, 0.65f, 0.28f));
                UnityEngine.Object.DestroyImmediate(socket.GetComponent<Collider>());
                sockets[i] = socket.transform;
            }

            sceneInfo.Configure(hero.transform, baseMarker, eastEnemySpawn.transform, bridgeMarkers, sockets);

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.shadows = LightShadows.Soft;
            light.intensity = 1.15f;
            lightGo.transform.rotation = Quaternion.Euler(52f, -30f, 0f);

            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            var camera = cameraGo.AddComponent<Camera>();
            camera.fieldOfView = 50f;
            cameraGo.AddComponent<AudioListener>();
            cameraGo.transform.position = hero.transform.position + new Vector3(0f, 18f, -13f);
            cameraGo.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
            var follow = cameraGo.AddComponent<Camera3DFollower>();
            follow.target = hero.transform;
            follow.offset = new Vector3(0f, 18f, -13f);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            // Register separately, never replace the Legacy SampleScene entry.
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            bool alreadyListed = false;
            foreach (var entry in scenes)
                if (entry.path == ScenePath) alreadyListed = true;
            if (!alreadyListed)
            {
                scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }
            Debug.Log("[TREBRAVE] Generated " + ScenePath +
                ". Run this scene to test Defense3D. Legacy Survival SampleScene is unchanged.");
        }

        [MenuItem("TREBRAVE/Defense 3D/Open Existing Defense Map")]
        public static void OpenExisting()
        {
            if (EditorApplication.isPlaying) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!System.IO.File.Exists(ScenePath))
            {
                Debug.LogWarning("[TREBRAVE] Defense3D prototype has not been generated. Select Build & Open first.");
                return;
            }
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(SceneFolder))
                AssetDatabase.CreateFolder("Assets/Scenes", "Defense3D");
        }

        private static GameObject Box(string name, Vector3 position, Vector3 scale, Color color,
            Transform parent, int layer, bool includeCollider = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = scale;
            if (layer >= 0) go.layer = layer;
            Paint(go, color);
            if (!includeCollider) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        private static void Paint(GameObject obj, Color color)
        {
            var renderer = obj.GetComponent<MeshRenderer>();
            if (renderer == null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            if (shader == null) return;
            var material = new Material(shader);
            material.color = color;
            renderer.sharedMaterial = material;
        }
    }
}
