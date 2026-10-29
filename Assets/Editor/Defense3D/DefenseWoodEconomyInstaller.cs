using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

namespace ProjectZombie.EditorTools.Defense3D
{
    public static class DefenseWoodEconomyInstaller
    {
        [MenuItem("TREBRAVE/Defense 3D/Install WoodDrop Economy In Open Scene")]
        public static void Install()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[TREBRAVE Defense3D] Exit Play Mode to install Wood economy.");
                return;
            }

            var info = Object.FindObjectOfType<DefenseSceneInfo3D>();
            if (info == null || info.HeroSpawn == null)
            {
                EditorUtility.DisplayDialog("TREBRAVE Defense 3D",
                    "Open your existing Defense scene and enable the gameplay root first.", "OK");
                return;
            }

            var hero = info.HeroSpawn.gameObject;
            if (hero.GetComponent<DefenseWoodWallet3D>() == null)
                Undo.AddComponent<DefenseWoodWallet3D>(hero);
            EditorSceneManager.MarkSceneDirty(hero.scene);
            Selection.activeGameObject = hero;
            Debug.Log("[TREBRAVE Defense3D] Wood wallet installed on Hero (0 starting Wood). Kill enemies and approach drops to collect them.");
        }
    }
}