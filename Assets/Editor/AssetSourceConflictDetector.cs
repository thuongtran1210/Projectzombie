#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Công cụ rà soát và phân loại xung đột dữ liệu giữa Resources và Addressables.
    /// Cho phép phân biệt giữa bản sao có chủ đích (SyncRegistry) và xung đột thật sự,
    /// đồng thời cung cấp chức năng dọn dẹp các asset đã có trong Addressables.
    /// </summary>
    public static class AssetSourceConflictDetector
    {
        [MenuItem("Tools/ProjectZombie/Audit/Detect Resources vs Addressables Conflicts", priority = 101)]
        public static void DetectConflicts()
        {
            Debug.Log("<color=#00E5FF>[AssetSourceConflictDetector]</color> Bắt đầu kiểm tra xung đột dữ liệu Resources vs Addressables...");

            string resourcesPath = "Assets/Resources";
            if (!Directory.Exists(resourcesPath))
            {
                Debug.LogWarning("[AssetSourceConflictDetector] Thư mục Assets/Resources không tồn tại.");
                return;
            }

            string[] resourceFiles = Directory.GetFiles(resourcesPath, "*.*", SearchOption.AllDirectories);
            Dictionary<string, string> resourceAssetMap = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);

            int totalResourcesFiles = 0;
            foreach (var file in resourceFiles)
            {
                if (file.EndsWith(".meta")) continue;
                totalResourcesFiles++;

                string assetName = Path.GetFileNameWithoutExtension(file);
                if (!resourceAssetMap.ContainsKey(assetName))
                {
                    resourceAssetMap[assetName] = file.Replace('\\', '/');
                }
            }

            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogWarning("[AssetSourceConflictDetector] Không tìm thấy AddressableAssetSettings.");
                return;
            }

            List<string> criticalInsideResources = new List<string>();
            List<string> localGroupDuplicates = new List<string>();
            List<string> remoteGroupDuplicates = new List<string>();

            foreach (var group in settings.groups)
            {
                if (group == null) continue;
                bool isRemote = group.name.IndexOf("Remote", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                                group.name.IndexOf("DLC", System.StringComparison.OrdinalIgnoreCase) >= 0;

                foreach (var entry in group.entries)
                {
                    if (entry == null) continue;

                    string assetPath = entry.AssetPath;
                    string addressableName = Path.GetFileNameWithoutExtension(assetPath);

                    if (assetPath.StartsWith("Assets/Resources/", System.StringComparison.OrdinalIgnoreCase))
                    {
                        criticalInsideResources.Add($"[{group.name}] Asset '{entry.address}' ({assetPath}) nằm trực tiếp trong Resources!");
                    }
                    else if (resourceAssetMap.TryGetValue(addressableName, out string matchedResPath))
                    {
                        string logMsg = $"[{group.name}] '{addressableName}' (Addressables: {assetPath} <=> Resources: {matchedResPath})";
                        if (isRemote)
                        {
                            remoteGroupDuplicates.Add(logMsg);
                        }
                        else
                        {
                            localGroupDuplicates.Add(logMsg);
                        }
                    }
                }
            }

            Debug.Log($"[AssetSourceConflictDetector] Đã quét {totalResourcesFiles} files Resources và {settings.groups.Count} Addressables groups.");

            if (criticalInsideResources.Count > 0)
            {
                Debug.LogError($"<color=#FF0000>[NGHIÊM TRỌNG] {criticalInsideResources.Count} asset nằm TRỰC TIẾP trong Resources nhưng vẫn gắn tag Addressable:</color>\n" + string.Join("\n", criticalInsideResources));
            }

            if (localGroupDuplicates.Count > 0)
            {
                Debug.LogWarning($"<color=#FFAA00>[BẢN SAO LOCAL_PACKED] {localGroupDuplicates.Count} asset thuộc nhóm Local nhưng bị nhân bản sang Resources (Làm phình dung lượng APK gấp đôi):</color>\n" + string.Join("\n", localGroupDuplicates));
            }

            if (remoteGroupDuplicates.Count > 0)
            {
                Debug.LogWarning($"<color=#00E5FF>[BẢN SAO REMOTE_DLC] {remoteGroupDuplicates.Count} asset thuộc nhóm Remote DLC nhưng lại có bản sao trong Resources (Phá vỡ cơ chế tải DLC sau):</color>\n" + string.Join("\n", remoteGroupDuplicates));
            }

            if (criticalInsideResources.Count == 0 && localGroupDuplicates.Count == 0 && remoteGroupDuplicates.Count == 0)
            {
                Debug.Log("<color=#00FF88>[AssetSourceConflictDetector] HOÀN HẢO:</color> 0 xung đột giữa Addressables và Resources!");
            }
        }

        [MenuItem("Tools/ProjectZombie/Audit/🧹 Clean Duplicate Resources (Addressables-Backed)", priority = 102)]
        public static void CleanDuplicateResources()
        {
            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogWarning("[AssetSourceConflictDetector] Không tìm thấy AddressableAssetSettings.");
                return;
            }

            string resourcesPath = "Assets/Resources";
            if (!Directory.Exists(resourcesPath)) return;

            string[] resourceFiles = Directory.GetFiles(resourcesPath, "*.*", SearchOption.AllDirectories);
            Dictionary<string, string> resourceAssetMap = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);

            foreach (var file in resourceFiles)
            {
                if (file.EndsWith(".meta")) continue;
                string assetName = Path.GetFileNameWithoutExtension(file);
                if (!resourceAssetMap.ContainsKey(assetName))
                {
                    resourceAssetMap[assetName] = file.Replace('\\', '/');
                }
            }

            // Tập hợp các file trong Resources cần dọn dẹp (đặc biệt là Remote DLC để phục hồi tính năng tải DLC)
            List<string> filesToDelete = new List<string>();

            foreach (var group in settings.groups)
            {
                if (group == null) continue;

                foreach (var entry in group.entries)
                {
                    if (entry == null) continue;
                    string assetPath = entry.AssetPath;
                    string addressableName = Path.GetFileNameWithoutExtension(assetPath);

                    if (!assetPath.StartsWith("Assets/Resources/", System.StringComparison.OrdinalIgnoreCase) &&
                        resourceAssetMap.TryGetValue(addressableName, out string matchedResPath))
                    {
                        if (!filesToDelete.Contains(matchedResPath))
                        {
                            filesToDelete.Add(matchedResPath);
                        }
                    }
                }
            }

            if (filesToDelete.Count == 0)
            {
                EditorUtility.DisplayDialog("Dọn Dẹp Resources", "Không phát hiện file bản sao nào trong Resources cần dọn dẹp!", "OK");
                return;
            }

            bool confirm = EditorUtility.DisplayDialog(
                "Xác Nhận Dọn Dẹp Bản Sao Resources",
                $"Phát hiện {filesToDelete.Count} asset trong Assets/Resources đã có mặt trong Addressables Groups.\n\n" +
                "Bạn có muốn chuyển các bản sao này vào Thùng Rác (Trash) để khôi phục cơ chế Remote DLC và giảm dung lượng APK không?",
                "Đồng Ý Xóa Bản Sao",
                "Hủy Bỏ");

            if (!confirm) return;

            int deletedCount = 0;
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var path in filesToDelete)
                {
                    if (AssetDatabase.MoveAssetToTrash(path))
                    {
                        deletedCount++;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }

            Debug.Log($"<color=#00FF88>[AssetSourceConflictDetector] ĐÃ DỌN DẸP THÀNH CÔNG:</color> Đã chuyển {deletedCount}/{filesToDelete.Count} file bản sao trong Resources vào Thùng Rác!");
            EditorUtility.DisplayDialog("Hoàn Tất Dọn Dẹp", $"Đã dọn dẹp thành công {deletedCount} file bản sao trong Resources!\nChạy lại Audit để kiểm tra.", "Tuyệt vời");
        }
    }
}
#endif
