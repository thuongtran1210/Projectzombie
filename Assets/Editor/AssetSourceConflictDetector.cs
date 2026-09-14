#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Công cụ rà soát xung đột nguồn dữ liệu giữa thư mục Resources và Addressables.
    /// Giúp phát hiện asset bị duplicate, phình to APK và lệch version.
    /// </summary>
    public static class AssetSourceConflictDetector
    {
        [MenuItem("Tools/ProjectZombie/Audit/Detect Resources vs Addressables Conflicts", priority = 101)]
        public static void DetectConflicts()
        {
            Debug.Log("<color=#00E5FF>[AssetSourceConflictDetector]</color> Bắt đầu kiểm tra xung đột dữ liệu Resources vs Addressables...");

            // 1. Quét tất cả asset trong Resources
            string resourcesPath = "Assets/Resources";
            if (!Directory.Exists(resourcesPath))
            {
                Debug.LogWarning("[AssetSourceConflictDetector] Thư mục Assets/Resources không tồn tại.");
                return;
            }

            string[] resourceFiles = Directory.GetFiles(resourcesPath, "*.*", SearchOption.AllDirectories);
            HashSet<string> resourceAssetNames = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            Dictionary<string, string> resourceAssetMap = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);

            int totalResourcesFiles = 0;
            foreach (var file in resourceFiles)
            {
                if (file.EndsWith(".meta")) continue;
                totalResourcesFiles++;

                string assetName = Path.GetFileNameWithoutExtension(file);
                if (!resourceAssetNames.Contains(assetName))
                {
                    resourceAssetNames.Add(assetName);
                    resourceAssetMap[assetName] = file.Replace('\\', '/');
                }
            }

            // 2. Quét các asset trong Addressables Settings nếu có
            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogWarning("[AssetSourceConflictDetector] Không tìm thấy AddressableAssetSettings. Vui lòng đảm bảo Addressables đã được cấu hình.");
                return;
            }

            List<string> conflicts = new List<string>();

            foreach (var group in settings.groups)
            {
                if (group == null) continue;

                foreach (var entry in group.entries)
                {
                    if (entry == null) continue;

                    string assetPath = entry.AssetPath;
                    string addressableName = Path.GetFileNameWithoutExtension(assetPath);

                    // Kiểm tra xem asset Addressable có nằm trực tiếp trong Resources không (LỖI NẶNG)
                    if (assetPath.StartsWith("Assets/Resources/", System.StringComparison.OrdinalIgnoreCase))
                    {
                        conflicts.Add($"[TRỰC TIẾP TRONG RESOURCES] Asset '{entry.address}' ({assetPath}) vừa là Addressable vừa nằm trong Resources! Gây nhân đôi bundle.");
                    }
                    // Kiểm tra xem có asset trùng tên giữa Addressables và Resources không (LỆCH VERSION)
                    else if (resourceAssetMap.TryGetValue(addressableName, out string matchedResPath))
                    {
                        conflicts.Add($"[TRÙNG TÊN DUPLICATE] '{addressableName}' xuất hiện ở cả Addressables ({assetPath}) và Resources ({matchedResPath})!");
                    }
                }
            }

            Debug.Log($"[AssetSourceConflictDetector] Quét {totalResourcesFiles} files trong Resources và {settings.groups.Count} groups Addressables.");

            if (conflicts.Count > 0)
            {
                Debug.LogError($"<color=#FF4444>[AssetSourceConflictDetector] PHÁT HIỆN {conflicts.Count} XUNG ĐỘT NGUỒN DỮ LIỆU:</color>\n" + string.Join("\n", conflicts));
            }
            else
            {
                Debug.Log("<color=#00FF88>[AssetSourceConflictDetector] HOÀN TẤT:</color> Không phát hiện xung đột trực tiếp nào giữa Resources và Addressables!");
            }
        }
    }
}
#endif
