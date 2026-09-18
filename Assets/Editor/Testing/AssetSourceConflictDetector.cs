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
        public class ConflictReport
        {
            public int TotalResourcesFiles { get; set; }
            public List<string> CriticalInsideResources { get; } = new List<string>();
            public List<string> LocalGroupDuplicates { get; } = new List<string>();
            public List<string> RemoteGroupDuplicates { get; } = new List<string>();
            public List<string> DuplicateResourcePaths { get; } = new List<string>();

            public int TotalDuplicates => LocalGroupDuplicates.Count + RemoteGroupDuplicates.Count;
            public bool HasConflicts => CriticalInsideResources.Count > 0 || TotalDuplicates > 0;
        }

        public static ConflictReport ScanConflicts()
        {
            var report = new ConflictReport();
            string resourcesPath = "Assets/Resources";
            if (!Directory.Exists(resourcesPath)) return report;

            string[] resourceFiles = Directory.GetFiles(resourcesPath, "*.*", SearchOption.AllDirectories);
            Dictionary<string, string> resourceAssetMap = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);

            foreach (var file in resourceFiles)
            {
                if (file.EndsWith(".meta")) continue;
                report.TotalResourcesFiles++;

                string assetName = Path.GetFileNameWithoutExtension(file);
                if (!resourceAssetMap.ContainsKey(assetName))
                {
                    resourceAssetMap[assetName] = file.Replace('\\', '/');
                }
            }

            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) return report;

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
                        report.CriticalInsideResources.Add($"[{group.name}] '{entry.address}' ({assetPath}) nằm trực tiếp trong Resources!");
                    }
                    else if (resourceAssetMap.TryGetValue(addressableName, out string matchedResPath))
                    {
                        string logMsg = $"[{group.name}] '{addressableName}' (Addressables: {assetPath} <=> Resources: {matchedResPath})";
                        if (isRemote)
                        {
                            report.RemoteGroupDuplicates.Add(logMsg);
                        }
                        else
                        {
                            report.LocalGroupDuplicates.Add(logMsg);
                        }

                        if (!report.DuplicateResourcePaths.Contains(matchedResPath))
                        {
                            report.DuplicateResourcePaths.Add(matchedResPath);
                        }
                    }
                }
            }

            return report;
        }

        [MenuItem("ProjectZombie/3. 🧪 Cheat & Testing/4. Detect Resources vs Addressables Conflicts", priority = 204)]
        public static void DetectConflicts()
        {
            Debug.Log("<color=#00E5FF>[AssetSourceConflictDetector]</color> Bắt đầu kiểm tra xung đột dữ liệu Resources vs Addressables...");
            var report = ScanConflicts();
            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;

            Debug.Log($"[AssetSourceConflictDetector] Đã quét {report.TotalResourcesFiles} files Resources và {(settings != null ? settings.groups.Count : 0)} Addressables groups.");

            if (report.CriticalInsideResources.Count > 0)
            {
                Debug.LogError($"<color=#FF0000>[NGHIÊM TRỌNG] {report.CriticalInsideResources.Count} asset nằm TRỰC TIẾP trong Resources nhưng vẫn gắn tag Addressable:</color>\n" + string.Join("\n", report.CriticalInsideResources));
            }

            if (report.LocalGroupDuplicates.Count > 0)
            {
                Debug.LogWarning($"<color=#FFAA00>[BẢN SAO LOCAL_PACKED] {report.LocalGroupDuplicates.Count} asset thuộc nhóm Local nhưng bị nhân bản sang Resources (Làm phình dung lượng APK gấp đôi):</color>\n" + string.Join("\n", report.LocalGroupDuplicates));
            }

            if (report.RemoteGroupDuplicates.Count > 0)
            {
                Debug.LogWarning($"<color=#00E5FF>[BẢN SAO REMOTE_DLC] {report.RemoteGroupDuplicates.Count} asset thuộc nhóm Remote DLC nhưng lại có bản sao trong Resources (Phá vỡ cơ chế tải DLC sau):</color>\n" + string.Join("\n", report.RemoteGroupDuplicates));
            }

            if (!report.HasConflicts)
            {
                Debug.Log("<color=#00FF88>[AssetSourceConflictDetector] HOÀN HẢO:</color> 0 xung đột giữa Addressables và Resources!");
            }
        }

        [MenuItem("ProjectZombie/3. 🧪 Cheat & Testing/4.1 Clean Duplicate Resources (Addressables-Backed)", priority = 205)]
        public static void CleanDuplicateResources()
        {
            CleanDuplicateResourcesInternal(interactive: true);
        }

        public static int CleanDuplicateResourcesSilently()
        {
            return CleanDuplicateResourcesInternal(interactive: false);
        }

        private static int CleanDuplicateResourcesInternal(bool interactive)
        {
            var report = ScanConflicts();
            var filesToDelete = report.DuplicateResourcePaths;

            if (filesToDelete.Count == 0)
            {
                if (interactive)
                {
                    EditorUtility.DisplayDialog("Dọn Dẹp Resources", "Không phát hiện file bản sao nào trong Resources cần dọn dẹp!", "OK");
                }
                return 0;
            }

            if (interactive)
            {
                bool confirm = EditorUtility.DisplayDialog(
                    "Xác Nhận Dọn Dẹp Bản Sao Resources",
                    $"Phát hiện {filesToDelete.Count} asset trong Assets/Resources đã có mặt trong Addressables Groups.\n\n" +
                    "Bạn có muốn chuyển các bản sao này vào Thùng Rác (Trash) để triệt tiêu nhân bản và giảm dung lượng APK không?",
                    "Đồng Ý Xóa Bản Sao",
                    "Hủy Bỏ");

                if (!confirm) return 0;
            }

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
            if (interactive)
            {
                EditorUtility.DisplayDialog("Hoàn Tất Dọn Dẹp", $"Đã dọn dẹp thành công {deletedCount} file bản sao trong Resources!\nChạy lại Audit để kiểm tra.", "Tuyệt vời");
            }

            return deletedCount;
        }
    }
}
#endif
