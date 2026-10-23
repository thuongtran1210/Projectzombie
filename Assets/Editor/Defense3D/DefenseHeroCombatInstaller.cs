using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

namespace ProjectZombie.EditorTools.Defense3D
{
    public static class DefenseHeroCombatInstaller
    {
        [MenuItem("TREBRAVE/Defense 3D/Install Hero Basic Attack In Open Scene")]
        public static void Install()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[TREBRAVE Defense3D] Stop Play Mode before installing Hero Combat.");
                return;
            }
            var info = Object.FindObjectOfType<DefenseSceneInfo3D>();
            if (info == null || info.HeroSpawn == null)
            {
                EditorUtility.DisplayDialog("TREBRAVE Defense 3D",
                    "Open the Defense 3D scene and enable its gameplay root first.", "OK");
                return;
            }
            var hero = info.HeroSpawn.gameObject;
            var component = hero.GetComponent<DefenseHeroAutoAttack3D>();
            if (component != null)
            {
                Selection.activeGameObject = hero;
                Debug.Log("[TREBRAVE Defense3D] Hero Basic Attack is already installed.");
                return;
            }
            Undo.AddComponent<DefenseHeroAutoAttack3D>(hero);
            EditorSceneManager.MarkSceneDirty(hero.scene);
            Selection.activeGameObject = hero;
            Debug.Log("[TREBRAVE Defense3D] Installed Hero Basic Attack on " + hero.name +
                ". Save scene and press Play.");
        }
    }
}
