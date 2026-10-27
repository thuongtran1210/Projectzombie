using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

namespace ProjectZombie.EditorTools.Defense3D
{
    /// <summary>Author the visual inside the Yaksha prefab before entering Play Mode.</summary>
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

            var prefab = Selection.activeObject as GameObject;
            if (prefab == null || !PrefabUtility.IsPartOfPrefabAsset(prefab))
            {
                EditorUtility.DisplayDialog("TREBRAVE Yaksha",
                    "Select an imported character prefab or prefab asset in the Project window, then run this menu again. Do not select the GLB map or a scene instance.", "OK");
                return;
            }

            const string bossPath = "Assets/Resources/Defense3D/DefenseYaksha3D.prefab";
            if (AssetDatabase.GetAssetPath(prefab) == bossPath)
            {
                Debug.LogError("Select a visual model, not the Yaksha gameplay prefab itself.");
                return;
            }
            if (!EditorUtility.DisplayDialog("TREBRAVE Yaksha",
                "Save '" + prefab.name + "' as a visual child inside the Yaksha prefab? You can edit its transform in Prefab Mode.", "Assign", "Cancel"))
                return;
            var root = PrefabUtility.LoadPrefabContents(bossPath);
            try
            {
                var oldVisual = root.transform.Find("Yaksha_3D_Visual");
                if (oldVisual != null) Object.DestroyImmediate(oldVisual.gameObject);
                var visual = (GameObject)PrefabUtility.InstantiatePrefab(prefab, root.scene);
                visual.name = "Yaksha_3D_Visual";
                visual.transform.SetParent(root.transform, false);
                foreach (var collider in visual.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
                var placeholder = root.GetComponent<Renderer>();
                if (placeholder != null) placeholder.enabled = false;
                PrefabUtility.SaveAsPrefabAsset(root, bossPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(bossPath);
            Debug.Log("[TREBRAVE Defense3D] Yaksha visual saved in prefab. Adjust it in Prefab Mode.");
        }

        [MenuItem("TREBRAVE/Defense 3D/Assign Selected Model As Yaksha Visual", true)]
        private static bool ValidateAssign() => !EditorApplication.isPlaying;
    }
}
