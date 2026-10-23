using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

namespace ProjectZombie.EditorTools.Defense3D
{
    public static class DefenseRun600Installer
    {
        [MenuItem("TREBRAVE/Defense 3D/Install 600s Defense Run In Open Scene")]
        public static void Install()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[TREBRAVE Defense3D] Stop Play Mode before installing run.");
                return;
            }
            var info = Object.FindObjectOfType<DefenseSceneInfo3D>();
            var defenseBase = Object.FindObjectOfType<DefenseBasePrototype3D>();
            if (info == null || defenseBase == null || info.EastEnemySpawn == null)
            {
                EditorUtility.DisplayDialog("TREBRAVE Defense 3D",
                    "Open existing MapSongQue3D_Prototype scene and enable its gameplay root.", "OK");
                return;
            }
            if (info.HeroSpawn != null && info.HeroSpawn.GetComponent<DefenseHeroHealth3D>() == null)
            {
                Undo.AddComponent<DefenseHeroHealth3D>(info.HeroSpawn.gameObject);
                EditorSceneManager.MarkSceneDirty(info.HeroSpawn.gameObject.scene);
            }
            var current = Object.FindObjectOfType<DefenseRun6003D>();
            if (current != null)
            {
                Selection.activeGameObject = current.gameObject;
                Debug.Log("[TREBRAVE Defense3D] 600s run is already installed.");
                return;
            }
            foreach (var test in Object.FindObjectsOfType<DefenseWaveSlice3D>())
            {
                Undo.RecordObject(test, "Disable old 3-wave test");
                test.enabled = false;
                EditorUtility.SetDirty(test);
            }
            foreach (var test in Object.FindObjectsOfType<DefenseEnemyBridgeSmokeTest3D>())
            {
                Undo.RecordObject(test, "Disable old bridge smoke test");
                test.enabled = false;
                EditorUtility.SetDirty(test);
            }
            var controller = new GameObject("[TREBRAVE] 600s Defense Run");
            Undo.RegisterCreatedObjectUndo(controller, "Install 600s Defense Run");
            controller.AddComponent<DefenseRun6003D>();
            EditorSceneManager.MarkSceneDirty(controller.scene);
            Selection.activeGameObject = controller;
            Debug.Log("[TREBRAVE Defense3D] 600s Defense Run installed; older test spawners disabled. Save scene and Play.");
        }
    }
}
