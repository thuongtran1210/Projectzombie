using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

namespace ProjectZombie.EditorTools.Defense3D
{
    /// <summary>Assign an imported Yaksha model prefab without modifying map or NavMesh.</summary>
    public static class DefenseYakshaVisualInstaller
    {
        [MenuItem("TREBRAVE/Defense 3D/Assign Selected Model As Yaksha Visual")]
        private static void Assign()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("TREBRAVE Yaksha", "Exit Play Mode before assigning a model.", "OK");
                return;
            }

            var run = Object.FindObjectOfType<DefenseRun6003D>();
            if (run == null)
            {
                EditorUtility.DisplayDialog("TREBRAVE Yaksha",
                    "Open your existing MapSongQue3D scene and ensure the 600s Defense Run component is active.", "OK");
                return;
            }

            var prefab = Selection.activeObject as GameObject;
            if (prefab == null || !PrefabUtility.IsPartOfPrefabAsset(prefab))
            {
                EditorUtility.DisplayDialog("TREBRAVE Yaksha",
                    "Select an imported character prefab or prefab asset in the Project window, then run this menu again. Do not select the GLB map or a scene instance.", "OK");
                return;
            }

            if (!EditorUtility.DisplayDialog("TREBRAVE Yaksha",
                "Assign prefab '" + prefab.name + "' as the Yaksha visual? The existing 600s controller, HP, NavMesh and map remain unchanged.", "Assign", "Cancel"))
                return;

            var serialized = new SerializedObject(run);
            var visual = serialized.FindProperty("yakshaVisualPrefab");
            if (visual == null)
            {
                Debug.LogError("[TREBRAVE Defense3D] yakshaVisualPrefab property not found.");
                return;
            }
            Undo.RecordObject(run, "Assign Yaksha visual prefab");
            visual.objectReferenceValue = prefab;
            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(run);
            EditorSceneManager.MarkSceneDirty(run.gameObject.scene);
            Selection.activeGameObject = run.gameObject;
            Debug.Log("[TREBRAVE Defense3D] Yaksha visual prefab assigned: " + prefab.name +
                ". Save scene. Boss visual is instantiated at runtime at 510s.");
        }

        [MenuItem("TREBRAVE/Defense 3D/Assign Selected Model As Yaksha Visual", true)]
        private static bool ValidateAssign() => !EditorApplication.isPlaying;
    }
}
