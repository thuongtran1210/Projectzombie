using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

namespace ProjectZombie.EditorTools.Defense3D
{
    public static class DefenseBridgeSmokeTestInstaller
    {
        [MenuItem("TREBRAVE/Defense 3D/Add Bridge Enemy Test To Open Scene")]
        public static void AddTest()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[TREBRAVE] Stop Play Mode before adding the bridge test.");
                return;
            }

            var info = Object.FindObjectOfType<DefenseSceneInfo3D>();
            var defenseBase = Object.FindObjectOfType<DefenseBasePrototype3D>();
            if (info == null || defenseBase == null)
            {
                EditorUtility.DisplayDialog("TREBRAVE Defense 3D",
                    "Open MapSongQue3D_Prototype scene first. Missing scene markers or Main Base.", "OK");
                return;
            }

            var existing = Object.FindObjectOfType<DefenseEnemyBridgeSmokeTest3D>();
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                EditorUtility.DisplayDialog("TREBRAVE Defense 3D",
                    "Bridge test is already in the open scene. Press Play to test.", "OK");
                return;
            }

            var controller = new GameObject("[TREBRAVE] Enemy Bridge Smoke Test");
            Undo.RegisterCreatedObjectUndo(controller, "Add TREBRAVE bridge enemy test");
            controller.AddComponent<DefenseEnemyBridgeSmokeTest3D>();
            EditorSceneManager.MarkSceneDirty(controller.scene);
            Selection.activeGameObject = controller;
            Debug.Log("[TREBRAVE Defense3D] Bridge smoke test added to open scene. Save scene, then press Play.");
        }
    }
}
