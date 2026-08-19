using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Projectzombie.Editor.VFXTools
{
    public class SortingLayerFixerTool : EditorWindow
    {
        [MenuItem("ProjectZombie/Fix All Prefabs Sorting Layers")]
        public static void ShowWindow()
        {
            GetWindow<SortingLayerFixerTool>("Sorting Layer Fixer");
        }

        private void OnGUI()
        {
            GUILayout.Label("Batch Sorting Layer Validator & Auto-Fixer", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Công cụ này sẽ quét toàn bộ Prefabs trong Assets/_Prefabs (ExpGem, VFX, Projectiles, Skills) " +
                "và tự động chuẩn hóa Sorting Layer về đúng chuẩn kiến trúc:\n" +
                "- ExpGem / Collectibles -> 'Collectibles' (ID: 2785782173)\n" +
                "- VFX / Slash / Particle -> 'Skill' (ID: 1207572677)\n" +
                "- Projectiles / Bullets -> 'Projectiles' (ID: 201581293)",
                MessageType.Info
            );

            if (GUILayout.Button("FIX ALL PREFABS SORTING LAYERS NOW", GUILayout.Height(45)))
            {
                FixAllSortingLayers();
            }
        }

        public static void FixAllSortingLayers()
        {
            int fixedCount = 0;
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/_Prefabs", "Assets/VFX", "Assets/Features" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;

                bool modified = false;

                // 1. Collectibles (ExpGem, Chests, Pickups)
                if (path.Contains("ExpGem") || path.Contains("Collectible") || path.Contains("Chest"))
                {
                    var renderers = prefab.GetComponentsInChildren<Renderer>(true);
                    foreach (var r in renderers)
                    {
                        if (r.sortingLayerName != "Collectibles")
                        {
                            r.sortingLayerName = "Collectibles";
                            modified = true;
                        }
                    }
                }

                // 2. Skill Effects & VFX
                if (path.Contains("VFX") || path.Contains("Skill") || path.Contains("PS_") || path.Contains("Impact") || path.Contains("Slash"))
                {
                    var renderers = prefab.GetComponentsInChildren<Renderer>(true);
                    foreach (var r in renderers)
                    {
                        // Nếu là decal mặt đất thì để Tilemap_Decals, còn lại là Skill
                        if (r.name.Contains("Decal") || r.name.Contains("Ground"))
                        {
                            if (r.sortingLayerName != "Tilemap_Decals")
                            {
                                r.sortingLayerName = "Tilemap_Decals";
                                modified = true;
                            }
                        }
                        else if (r.sortingLayerName != "Skill")
                        {
                            r.sortingLayerName = "Skill";
                            modified = true;
                        }
                    }
                }

                // 3. Projectiles
                if (path.Contains("Projectile") || path.Contains("Proj_"))
                {
                    var renderers = prefab.GetComponentsInChildren<Renderer>(true);
                    foreach (var r in renderers)
                    {
                        if (r.sortingLayerName != "Projectiles" && r.sortingLayerName != "Skill")
                        {
                            r.sortingLayerName = "Projectiles";
                            modified = true;
                        }
                    }
                }

                if (modified)
                {
                    EditorUtility.SetDirty(prefab);
                    fixedCount++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SortingLayerFixer] ĐÃ SỬA THÀNH CÔNG {fixedCount} PREFABS VỀ ĐÚNG SORTING LAYER!");
        }
    }
}
