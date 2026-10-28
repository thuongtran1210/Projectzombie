using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectZombie.EditorTools.Migration
{
    /// <summary>
    /// Công cụ Editor hỗ trợ G6 Batch 1: Tự động dọn dẹp các GameObject con 2D cũ dưới --- GAME MANAGER --- trong SampleScene.
    /// Đảm bảo tính an toàn: Sử dụng Undo API của Unity Editor và yêu cầu xác nhận trước khi thực hiện.
    /// </summary>
    public static class CleanLegacySampleSceneTool
    {
        private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";

        // Danh sách tên các GameObject con 2D cần gỡ bỏ khỏi --- GAME MANAGER ---
        private static readonly HashSet<string> Legacy2DChildNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Player",
            "EnemyManager",
            "SpawnManager",
            "EnemyPoolManager",
            "ProjectilePoolManager",
            "VFXPoolManager",
            "DamageTextManager",
            "FloatingJoystick",
            "GameplayHUD",
            "ExperienceManager",
            "LevelUpManager",
            "GameOverManager",
            "SoundManager",
            "CameraManager",
            "GlobalLight2D",
            "MapManager",
            "Grid",
            "Tilemap"
        };

        [MenuItem("Tools/Project Zombie/3D Migration/G6: Dọn Dẹp 2D Trong SampleScene")]
        public static void CleanSampleScene2DRoots()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Cảnh báo", "Vui lòng tắt Play Mode trước khi thực hiện dọn dẹp Scene.", "Đồng ý");
                return;
            }

            // Mở scene SampleScene nếu chưa mở
            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene.path != SampleScenePath)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                currentScene = EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
            }

            // Tìm GameObject --- GAME MANAGER ---
            GameObject gameManagerRoot = null;
            foreach (var root in currentScene.GetRootGameObjects())
            {
                if (root.name.Trim().Equals("--- GAME MANAGER ---", StringComparison.OrdinalIgnoreCase))
                {
                    gameManagerRoot = root;
                    break;
                }
            }

            if (gameManagerRoot == null)
            {
                EditorUtility.DisplayDialog("Thông báo", "Không tìm thấy GameObject '--- GAME MANAGER ---' trong SampleScene.", "Đóng");
                return;
            }

            // Rà soát các con 2D
            var childrenToRemove = new List<GameObject>();
            for (int i = 0; i < gameManagerRoot.transform.childCount; i++)
            {
                var child = gameManagerRoot.transform.GetChild(i).gameObject;
                if (Legacy2DChildNames.Contains(child.name.Trim()) || child.name.IndexOf("2D", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    childrenToRemove.Add(child);
                }
            }

            if (childrenToRemove.Count == 0)
            {
                EditorUtility.DisplayDialog("Thông báo", "Cây phân cấp '--- GAME MANAGER ---' đã sạch sẽ, không còn đối tượng 2D cũ nào.", "Đóng");
                return;
            }

            string listNames = string.Join("\n• ", childrenToRemove.ConvertAll(c => c.name));
            bool confirmed = EditorUtility.DisplayDialog(
                "Xác nhận Dọn Dẹp G6 Batch 1",
                $"Tìm thấy {childrenToRemove.Count} đối tượng 2D dưới '--- GAME MANAGER ---':\n• {listNames}\n\nBạn có chắc chắn muốn gỡ bỏ để biến SampleScene thành Sảnh Menu chuyên trách không?",
                "Tiến Hành Gỡ Bỏ",
                "Hủy");

            if (!confirmed) return;

            Undo.SetCurrentGroupName("G6: Dọn Dẹp 2D SampleScene");
            int undoGroup = Undo.GetCurrentGroup();

            int removedCount = 0;
            foreach (var child in childrenToRemove)
            {
                if (child != null)
                {
                    Undo.DestroyObjectImmediate(child);
                    removedCount++;
                }
            }

            Undo.CollapseUndoOperations(undoGroup);
            EditorSceneManager.MarkSceneDirty(currentScene);
            EditorSceneManager.SaveScene(currentScene);

            Debug.Log($"<color=#00FF00>[G6 Migration]</color> Đã dọn dẹp thành công {removedCount} đối tượng 2D cũ khỏi SampleScene!");
            EditorUtility.DisplayDialog("Hoàn tất", $"Đã gỡ bỏ thành công {removedCount} đối tượng 2D cũ khỏi SampleScene và lưu Scene an toàn.", "Đóng");
        }
    }
}
