using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

namespace ProjectZombie.EditorTools.Defense3D
{
    public static class DefenseWaveSliceInstaller
    {
        [MenuItem("TREBRAVE/Defense 3D/Install 3-Wave Defense Test In Open Scene")]
        public static void Install()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[TREBRAVE Defense3D] Exit Play Mode to install the wave test.");
                return;
            }
            var sceneInfo = Object.FindObjectOfType<DefenseSceneInfo3D>();
            var defenseBase = Object.FindObjectOfType<DefenseBasePrototype3D>();
            if (sceneInfo == null || defenseBase == null)
            {
                EditorUtility.DisplayDialog("TREBRAVE Defense 3D",
                    "Open your saved MapSongQue3D_Prototype and enable its gameplay root first.", "OK");
                return;
            }
            var existing = Object.FindObjectOfType<DefenseWaveSlice3D>();
            if (existing != null)
            {
                Selection.activeObject = existing.gameObject;
                Debug.Log("[TREBRAVE Defense3D] Wave test already installed.");
                return;
            }
            var smokeTest = Object.FindObjectOfType<DefenseEnemyBridgeSmokeTest3D>();
            if (smokeTest != null)
            {
                Undo.RecordObject(smokeTest, "Disable old bridge smoke test");
                smokeTest.enabled = false;
                EditorUtility.SetDirty(smokeTest);
            }
            var go = new GameObject("[TREBRAVE] Three Wave Defense Test");
            Undo.RegisterCreatedObjectUndo(go, "Install 3-wave Defense test");
            go.AddComponent<DefenseWaveSlice3D>();
            EditorSceneManager.MarkSceneDirty(go.scene);
            Selection.activeGameObject = go;
            Debug.Log("[TREBRAVE Defense3D] Installed 3-wave test. Old bridge smoke test disabled. Save, then Play.");
        }
    }
}
