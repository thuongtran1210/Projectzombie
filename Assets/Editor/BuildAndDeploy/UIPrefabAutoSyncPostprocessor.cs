#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.EditorTools.BuildSync
{
    /// <summary>
    /// Tự động đồng bộ Prefab từ Assets/_Prefabs/UI sang Assets/Resources/UI mỗi khi người dùng chỉnh sửa và lưu Prefab.
    /// Giúp bạn hoàn toàn thoải mái chỉnh sửa Prefab tại Assets/_Prefabs/UI mà bản Build Android luôn tự động cập nhật.
    /// </summary>
    public class UIPrefabAutoSyncPostprocessor : AssetPostprocessor
    {
        private static bool _isSyncing = false;

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (_isSyncing) return;

            bool hasSyncedAny = false;

            foreach (string path in importedAssets)
            {
                if (path.StartsWith("Assets/_Prefabs/UI", System.StringComparison.OrdinalIgnoreCase) &&
                    path.EndsWith(".prefab", System.StringComparison.OrdinalIgnoreCase))
                {
                    string relPath = path.Substring("Assets/_Prefabs/UI".Length).TrimStart('/', '\\');
                    string targetPath = Path.Combine("Assets/Resources/UI", relPath).Replace('\\', '/');

                    try
                    {
                        _isSyncing = true;
                        string targetDir = Path.GetDirectoryName(targetPath);
                        if (!Directory.Exists(targetDir))
                        {
                            Directory.CreateDirectory(targetDir);
                        }

                        // So sánh dung lượng & thời gian để tránh copy dư thừa
                        if (File.Exists(targetPath))
                        {
                            var srcInfo = new FileInfo(path);
                            var dstInfo = new FileInfo(targetPath);
                            if (srcInfo.Length == dstInfo.Length && 
                                System.Math.Abs((srcInfo.LastWriteTimeUtc - dstInfo.LastWriteTimeUtc).TotalSeconds) < 2)
                            {
                                continue;
                            }
                        }

                        File.Copy(path, targetPath, true);
                        hasSyncedAny = true;
                        Debug.Log($"<color=#00FF88>[Auto-Sync UI]</color> Đã tự động cập nhật Prefab '{Path.GetFileName(path)}' vào Assets/Resources/UI phục vụ Android Build.");
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[Auto-Sync UI] Không thể đồng bộ {path}: {ex.Message}");
                    }
                    finally
                    {
                        _isSyncing = false;
                    }
                }
            }

            if (hasSyncedAny)
            {
                // Refresh asset database để Unity nhận diện file Resources mới
                EditorApplication.delayCall += () =>
                {
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                };
            }
        }
    }
}
#endif
