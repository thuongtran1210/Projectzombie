using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using ProjectZombie.Features.Defense3D;

namespace ProjectZombie.EditorTools.Defense3D
{
    /// <summary>
    /// Bundles the scene-authored Hero_3D_Prototype into a self-contained, reusable Prefab:
    /// Assets/Resources/Defense3D/Hero_Scholar.prefab
    /// </summary>
    public static class DefenseHeroPrefabBuilder
    {
        private const string PREFAB_PATH = "Assets/Resources/Defense3D/Hero_Scholar.prefab";
        private const string SCENE_PATH = "Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity";

        [MenuItem("TREBRAVE/Defense 3D/Build and Export Hero Scholar Prefab")]
        public static void BuildPrefab()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Exit Play Mode before exporting Hero Prefab.");

            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != SCENE_PATH)
            {
                scene = EditorSceneManager.OpenScene(SCENE_PATH, OpenSceneMode.Single);
            }

            var hero = GameObject.Find("Hero_3D_Prototype");
            if (hero == null)
            {
                var health = UnityEngine.Object.FindObjectOfType<DefenseHeroHealth3D>();
                if (health != null) hero = health.gameObject;
            }

            if (hero == null) throw new InvalidOperationException("Cannot find Hero_3D_Prototype in the active scene.");

            Directory.CreateDirectory("Assets/Resources/Defense3D");

            // Ensure DefenseCombatSlashVfx3D exists on or as child of Hero for decoupled VFX
            var slashVfx = hero.GetComponentInChildren<DefenseCombatSlashVfx3D>(true);
            if (slashVfx == null)
            {
                var vfxGo = new GameObject("Hero_SlashVfx");
                vfxGo.transform.SetParent(hero.transform, false);
                slashVfx = vfxGo.AddComponent<DefenseCombatSlashVfx3D>();
            }

            // Wire slash VFX into attack effect
            var attackEffect = hero.GetComponent<DefenseHeroAttackEffect3D>();
            if (attackEffect != null)
            {
                var so = new SerializedObject(attackEffect);
                var slashProp = so.FindProperty("_slashVfx");
                if (slashProp != null && slashProp.objectReferenceValue == null)
                {
                    slashProp.objectReferenceValue = slashVfx;
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            // Save clean prefab asset
            PrefabUtility.SaveAsPrefabAssetAndConnect(hero, PREFAB_PATH, InteractionMode.AutomatedAction);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[TREBRAVE Defense3D] Successfully exported Hero Scholar Prefab to: " + PREFAB_PATH);
        }
    }
}
