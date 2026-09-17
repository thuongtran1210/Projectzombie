#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Fusion;
using ProjectZombie.Features.Multiplayer.Core;
using ProjectZombie.Features.Combat.Coop;
using ProjectZombie.Features.Player.Mechanics;

namespace ProjectZombie.Editor.MultiplayerTools
{
    /// <summary>
    /// Tool tự động gắn và cấu hình các thành phần Photon Fusion cho Player Prefabs.
    /// Đảm bảo tính an toàn (Non-destructive), hỗ trợ cả chế độ Solo lẫn Multiplayer Co-op.
    /// </summary>
    public static class SetupPhotonPlayerPrefabsTool
    {
        private static readonly string[] PLAYER_PREFAB_PATHS = new string[]
        {
            "Assets/_Prefabs/Characters/Players/Dao Si.prefab",
            "Assets/_Prefabs/Characters/Players/An Si.prefab",
            "Assets/_Prefabs/Characters/Players/Thanh Dong.prefab",
            "Assets/_Prefabs/Characters/Players/Thu Sinh.prefab"
        };

        [MenuItem("ProjectZombie/3. 🌐 Multiplayer/1. ⚡ Gắn Photon Components vào Player Prefabs", priority = 1)]
        public static void SetupPlayerPrefabs()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("[SetupPhotonPlayerPrefabsTool] Vui lòng tắt Play Mode trước khi cấu hình Prefabs!");
                return;
            }

            int configuredCount = 0;

            foreach (var path in PLAYER_PREFAB_PATHS)
            {
                if (!File.Exists(path))
                {
                    Debug.LogWarning($"[SetupPhotonPlayerPrefabsTool] Không tìm thấy: {path}");
                    continue;
                }

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

                    // 3. Gắn NetworkInputBridge
                    if (!root.TryGetComponent<NetworkInputBridge>(out var inputBridge))
                    {
                        inputBridge = root.AddComponent<NetworkInputBridge>();
                    }

                    // 4. Gắn NetworkPlayerCharacter
                    if (!root.TryGetComponent<NetworkPlayerCharacter>(out var netChar))
                    {
                        netChar = root.AddComponent<NetworkPlayerCharacter>();
                    }

                    // 5. Gắn Coop Mechanics (Gục ngã & Chia sẻ kinh nghiệm)
                    if (!root.TryGetComponent<CoopDownedMechanic>(out var downedMech))
                    {
                        downedMech = root.AddComponent<CoopDownedMechanic>();
                    }

                    if (!root.TryGetComponent<CoopTeamExperience>(out var teamExp))
                    {
                        teamExp = root.AddComponent<CoopTeamExperience>();
                    }

                    PrefabUtility.SaveAsPrefabAsset(root, path);
                    configuredCount++;
                    Debug.Log($"<color=#00FF88>[SetupPhotonPlayerPrefabsTool]</color> Đã cấu hình thành công: {Path.GetFileName(path)}");
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=#00FF88>[SetupPhotonPlayerPrefabsTool] HOÀN TẤT!</color> Đã cập nhật thành công {configuredCount}/{PLAYER_PREFAB_PATHS.Length} Player Prefabs sẵn sàng cho Photon Fusion!");
        }
    }
}
#endif
