#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Fusion;
using ProjectZombie.Features.Multiplayer.Core;

namespace ProjectZombie.Editor.MultiplayerTools
{
    /// <summary>
    /// Tool tự động cấu hình các thành phần Photon Fusion cho toàn bộ Enemy và Boss Prefabs.
    /// Đảm bảo tính an toàn (Non-destructive), hỗ trợ cơ chế Host-Authoritative.
    /// </summary>
    public static class SetupPhotonEnemyPrefabsTool
    {
        private const string ENEMY_PREFABS_FOLDER = "Assets/_Prefabs/Characters/Enemies";

        [MenuItem("ProjectZombie/3. 🌐 Multiplayer/2. ⚡ Gắn Photon Components vào Enemy Prefabs", priority = 2)]
        public static void SetupEnemyPrefabs()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("[SetupPhotonEnemyPrefabsTool] Vui lòng tắt Play Mode trước khi cấu hình Prefabs!");
                return;
            }

            if (!Directory.Exists(ENEMY_PREFABS_FOLDER))
            {
                Debug.LogWarning($"[SetupPhotonEnemyPrefabsTool] Không tìm thấy thư mục: {ENEMY_PREFABS_FOLDER}");
                return;
            }

            string[] prefabFiles = Directory.GetFiles(ENEMY_PREFABS_FOLDER, "*.prefab", SearchOption.AllDirectories);
            int configuredCount = 0;

            foreach (var path in prefabFiles)
            {
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                if (root == null) continue;

                try
                {
                    // 1. Gắn NetworkObject
                    if (!root.TryGetComponent<NetworkObject>(out var netObj))
                    {
                        netObj = root.AddComponent<NetworkObject>();
                    }

                    // 2. Gắn NetworkTransform để tự động nội suy tọa độ qua mạng
                    if (!root.TryGetComponent<NetworkTransform>(out var netTransform))
                    {
                        netTransform = root.AddComponent<NetworkTransform>();
                    }

                    // 3. Gắn NetworkEnemySync
                    if (!root.TryGetComponent<NetworkEnemySync>(out var enemySync))
                    {
                        enemySync = root.AddComponent<NetworkEnemySync>();
                    }

                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    configuredCount++;
                    Debug.Log($"<color=#00FF88>[SetupPhotonEnemyPrefabsTool]</color> Đã cấu hình thành công: {Path.GetFileName(path)}");
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=#00FF88>[SetupPhotonEnemyPrefabsTool] HOÀN TẤT!</color> Đã cập nhật thành công {configuredCount} Enemy/Boss Prefabs sẵn sàng cho Host-Authoritative Multiplayer!");
        }
    }
}
#endif
