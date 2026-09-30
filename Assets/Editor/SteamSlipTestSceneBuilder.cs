using ProjectZombie.Features.Elements;
using ProjectZombie.Features.Shared.VFX;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

namespace ProjectZombie.EditorTools
{
    public static class SteamSlipTestSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/SteamSlipTest.unity";

        [MenuItem("Tools/Project Zombie/Build Steam Slip Test Scene")]
        public static void Build()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                if (!EditorUtility.DisplayDialog("Rebuild Steam Slip Test Scene", "Replace the existing isolated test scene with the player/weapon/enemy spawn harness?", "Rebuild", "Cancel"))
                    return;
                AssetDatabase.DeleteAsset(ScenePath);
            }
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("Stop Play Mode before building the Steam Slip test scene.");
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                BuildCamera(scene);
                BuildLighting(scene);
                BuildVfxPool(scene);

                var playerSpawn = new GameObject("Player Spawn Point");
                SceneManager.MoveGameObjectToScene(playerSpawn, scene);
                playerSpawn.transform.position = new Vector3(0f, -1f, 0f);
                var enemySpawn = new GameObject("Enemy Spawn Point");
                SceneManager.MoveGameObjectToScene(enemySpawn, scene);
                enemySpawn.transform.position = new Vector3(0f, 0.8f, 0f);

                var harnessObject = new GameObject("Steam Slip Test Harness");
                SceneManager.MoveGameObjectToScene(harnessObject, scene);
                var harness = harnessObject.AddComponent<SteamSlipTestHarness>();
                var serializedHarness = new SerializedObject(harness);
                serializedHarness.FindProperty("playerSpawnPosition").vector3Value = playerSpawn.transform.position;
                serializedHarness.FindProperty("enemySpawnPosition").vector3Value = enemySpawn.transform.position;
                serializedHarness.ApplyModifiedPropertiesWithoutUndo();

                EditorSceneManager.SaveScene(scene, ScenePath);
                Debug.Log($"Created {ScenePath}. It is intentionally excluded from Build Settings.");
            }
            finally
            {
                if (scene.IsValid() && scene.isLoaded)
                    EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void BuildCamera(Scene scene)
        {
            var cameraObject = new GameObject("Test Camera", typeof(Camera), typeof(AudioListener));
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 4.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.1f, 0.14f);
        }

        private static void BuildLighting(Scene scene)
        {
            var lightObject = new GameObject("Global 2D Light");
            SceneManager.MoveGameObjectToScene(lightObject, scene);
            Light2D light = lightObject.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.color = Color.white;
            light.intensity = 1f;
        }

        private static void BuildVfxPool(Scene scene)
        {
            var poolObject = new GameObject("GlobalVFXPoolManager");
            SceneManager.MoveGameObjectToScene(poolObject, scene);
            poolObject.AddComponent<GlobalVFXPoolManager>();
        }

    }
}



