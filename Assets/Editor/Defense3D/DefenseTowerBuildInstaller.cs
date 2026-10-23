using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

namespace ProjectZombie.EditorTools.Defense3D
{
    public static class DefenseTowerBuildInstaller
    {
        [MenuItem("TREBRAVE/Defense 3D/Install Tower Build Test In Open Scene")]
        public static void Install()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[TREBRAVE Defense3D] Stop Play Mode before installing towers.");
                return;
            }
            var info = Object.FindObjectOfType<DefenseSceneInfo3D>();
            if (info == null || info.HeroSpawn == null || info.TowerSocketCount != 10)
            {
                EditorUtility.DisplayDialog("TREBRAVE Defense 3D",
                    "Open your existing Defense scene and enable its gameplay root (10 tower sockets required).", "OK");
                return;
            }
            var existing = Object.FindObjectOfType<DefenseTowerBuildPrototype3D>();
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                Debug.Log("[TREBRAVE Defense3D] Tower builder already installed.");
                return;
            }
            var controller = new GameObject("[TREBRAVE] Tower Build Test");
            Undo.RegisterCreatedObjectUndo(controller, "Install tower building test");
            controller.AddComponent<DefenseTowerBuildPrototype3D>();
            EditorSceneManager.MarkSceneDirty(controller.scene);
            Selection.activeGameObject = controller;
            Debug.Log("[TREBRAVE Defense3D] Tower builder installed. Save, Play, move near a socket and press B.");
        }
    }
}
