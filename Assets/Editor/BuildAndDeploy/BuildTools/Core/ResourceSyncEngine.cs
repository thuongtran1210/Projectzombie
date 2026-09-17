using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.EditorTools.BuildSync
{
    public static class ResourceSyncEngine
    {
        public static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// So sánh 2 file chuẩn xác: Xử lý cả file nhị phân lẫn file Text/YAML của Unity (bỏ qua khác biệt CRLF/LF do import).
        /// </summary>
        public static bool AreFilesEqual(string file1, string file2)
        {
            if (!File.Exists(file1) || !File.Exists(file2)) return false;

            var f1 = new FileInfo(file1);
            var f2 = new FileInfo(file2);

            // 1. Cùng kích thước và thời gian sửa đổi gần nhau -> Khớp 100%
            if (f1.Length == f2.Length && Math.Abs((f1.LastWriteTimeUtc - f2.LastWriteTimeUtc).TotalSeconds) < 2)
            {
                return true;
            }

            string ext = Path.GetExtension(file1).ToLower();
            bool isTextOrYaml = ext == ".prefab" || ext == ".asset" || ext == ".mat" || ext == ".json" || ext == ".txt" || ext == ".unity";

            // 2. File nhị phân (ảnh, âm thanh, mixer...)
            if (!isTextOrYaml)
            {
                if (f1.Length != f2.Length) return false;
                return CompareBinaryFiles(file1, file2);
            }

            // 3. File text / YAML Unity: So sánh nội dung chuẩn hóa line endings và bỏ qua SortKey ngẫu nhiên của Photon Fusion
            try
            {
                string text1 = NormalizeYamlText(File.ReadAllText(file1));
                string text2 = NormalizeYamlText(File.ReadAllText(file2));
                return string.Equals(text1, text2, StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }

        private static string NormalizeYamlText(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;
            string normalized = raw.Replace("\r\n", "\n").TrimEnd();
            // Bỏ qua trường SortKey được sinh ngẫu nhiên/riêng biệt bởi Photon Fusion NetworkObject Weaver
            return System.Text.RegularExpressions.Regex.Replace(normalized, @"SortKey:\s*\d+", "SortKey: 0");
        }

        private static bool CompareBinaryFiles(string path1, string path2)
        {
            const int bufferSize = 8192;
            byte[] buffer1 = new byte[bufferSize];
            byte[] buffer2 = new byte[bufferSize];

            using (var stream1 = File.OpenRead(path1))
            using (var stream2 = File.OpenRead(path2))
            {
                int bytesRead1, bytesRead2;
                while ((bytesRead1 = stream1.Read(buffer1, 0, bufferSize)) > 0)
                {
                    bytesRead2 = stream2.Read(buffer2, 0, bufferSize);
                    if (bytesRead1 != bytesRead2) return false;
                    for (int i = 0; i < bytesRead1; i++)
                    {
                        if (buffer1[i] != buffer2[i]) return false;
                    }
                }
            }
            return true;
        }

        public static int SyncDirectory(SyncRule rule)
        {
            if (rule.IsAddressableManaged)
            {
                Debug.Log($"<color=#00E5FF>[ResourceSyncEngine]</color> Bỏ qua đồng bộ '{rule.Name}' vào Resources vì đã chuyển sang quản lý bằng Addressables.");
                return 0;
            }

            if (!Directory.Exists(rule.SourcePath)) return 0;
            EnsureDirectory(rule.TargetPath);

            string[] files = Directory.GetFiles(rule.SourcePath, rule.SearchPattern ?? "*.*", SearchOption.AllDirectories);
            int count = 0;

            foreach (string file in files)
            {
                if (file.EndsWith(".meta")) continue;

                if (rule.AllowedExtensions != null)
                {
                    string ext = Path.GetExtension(file).ToLower();
                    bool allowed = false;
                    foreach (var validExt in rule.AllowedExtensions)
                    {
                        if (ext == validExt)
                        {
                            allowed = true;
                            break;
                        }
                    }
                    if (!allowed) continue;
                }

                string relPath = file.Substring(rule.SourcePath.Length).TrimStart('/', '\\');
                string destFile = Path.Combine(rule.TargetPath, relPath).Replace('\\', '/');
                string srcFile = file.Replace('\\', '/');

                EnsureDirectory(Path.GetDirectoryName(destFile));

                if (File.Exists(destFile))
                {
                    if (AreFilesEqual(srcFile, destFile))
                    {
                        count++;
                        continue;
                    }

                    var srcInfo = new FileInfo(srcFile);
                    var destInfo = new FileInfo(destFile);

                    // Nếu file đích mới hơn hẳn 5 giây (người dùng sửa trực tiếp trong Resources) -> copy ngược lại
                    if (destInfo.LastWriteTimeUtc > srcInfo.LastWriteTimeUtc.AddSeconds(5))
                    {
                        File.Copy(destFile, srcFile, true);
                        File.SetLastWriteTimeUtc(srcFile, destInfo.LastWriteTimeUtc);
                        AssetDatabase.ImportAsset(srcFile, ImportAssetOptions.ForceUpdate);
                        count++;
                        continue;
                    }
                }

                File.Copy(srcFile, destFile, true);
                var srcTime = File.GetLastWriteTimeUtc(srcFile);
                File.SetLastWriteTimeUtc(destFile, srcTime);
                AssetDatabase.ImportAsset(destFile, ImportAssetOptions.ForceUpdate);
                File.SetLastWriteTimeUtc(destFile, srcTime);
                count++;
            }

            return count;
        }

        public static void SyncSingleAsset(SyncRule rule)
        {
            string src = rule.SourcePath;
            string dest = rule.TargetPath;

            if (!File.Exists(src) && !File.Exists(dest))
            {
                try
                {
                    rule.FallbackGenerator?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[ResourceSyncEngine] Không thể khởi tạo tự động {rule.Name}: {e.Message}");
                }
                return;
            }

            if (File.Exists(src))
            {
                EnsureDirectory(Path.GetDirectoryName(dest));
                if (!File.Exists(dest))
                {
                    File.Copy(src, dest, true);
                    var srcTime = File.GetLastWriteTimeUtc(src);
                    File.SetLastWriteTimeUtc(dest, srcTime);
                    AssetDatabase.ImportAsset(dest, ImportAssetOptions.ForceUpdate);
                    File.SetLastWriteTimeUtc(dest, srcTime);
                }
                else
                {
                    if (AreFilesEqual(src, dest)) return;

                    var srcInfo = new FileInfo(src);
                    var destInfo = new FileInfo(dest);

                    if (destInfo.LastWriteTimeUtc > srcInfo.LastWriteTimeUtc.AddSeconds(5))
                    {
                        File.Copy(dest, src, true);
                        File.SetLastWriteTimeUtc(src, destInfo.LastWriteTimeUtc);
                        AssetDatabase.ImportAsset(src, ImportAssetOptions.ForceUpdate);
                        return;
                    }

                    File.Copy(src, dest, true);
                    var srcTime = File.GetLastWriteTimeUtc(src);
                    File.SetLastWriteTimeUtc(dest, srcTime);
                    AssetDatabase.ImportAsset(dest, ImportAssetOptions.ForceUpdate);
                    File.SetLastWriteTimeUtc(dest, srcTime);
                }
            }
            else if (File.Exists(dest))
            {
                EnsureDirectory(Path.GetDirectoryName(src));
                File.Copy(dest, src, true);
                var destTime = File.GetLastWriteTimeUtc(dest);
                File.SetLastWriteTimeUtc(src, destTime);
                AssetDatabase.ImportAsset(src, ImportAssetOptions.ForceUpdate);
                File.SetLastWriteTimeUtc(src, destTime);
            }
        }

        public static void SyncUIPrefab(SyncRule rule)
        {
            string resPath = rule.TargetPath;
            string masterPath = rule.SourcePath;

            EnsureDirectory(Path.GetDirectoryName(resPath));
            EnsureDirectory(Path.GetDirectoryName(masterPath));

            if (!File.Exists(resPath) && !File.Exists(masterPath))
            {
                try
                {
                    rule.FallbackGenerator?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[ResourceSyncEngine] Không thể sinh fallback cho {rule.Name}: {e.Message}");
                }
                return;
            }

            if (File.Exists(masterPath))
            {
                if (!File.Exists(resPath) || !AreFilesEqual(masterPath, resPath))
                {
                    File.Copy(masterPath, resPath, true);
                    var masterTime = File.GetLastWriteTimeUtc(masterPath);
                    File.SetLastWriteTimeUtc(resPath, masterTime);
                    AssetDatabase.ImportAsset(resPath, ImportAssetOptions.ForceUpdate);
                    File.SetLastWriteTimeUtc(resPath, masterTime);
                }
            }
            else if (File.Exists(resPath))
            {
                File.Copy(resPath, masterPath, true);
                var resTime = File.GetLastWriteTimeUtc(resPath);
                File.SetLastWriteTimeUtc(masterPath, resTime);
                AssetDatabase.ImportAsset(masterPath, ImportAssetOptions.ForceUpdate);
                File.SetLastWriteTimeUtc(masterPath, resTime);
            }
        }

        public static string PerformFullSync()
        {
            EnsureDirectory("Assets/Resources");

            var sb = new StringBuilder();
            sb.AppendLine($"[THỜI GIAN: {DateTime.Now:HH:mm:ss}]");

            foreach (var rule in SyncRegistry.DirectoryRules)
            {
                int c = SyncDirectory(rule);
                sb.AppendLine($"✓ Đã đồng bộ {c} files vào {rule.TargetPath}");
            }

            foreach (var rule in SyncRegistry.SingleAssetRules)
            {
                SyncSingleAsset(rule);
            }
            sb.AppendLine("✓ Đã đồng bộ các Single Assets cốt lõi (CharacterDatabase, UpgradeTree)");

            foreach (var rule in SyncRegistry.UIPrefabRules)
            {
                SyncUIPrefab(rule);
            }
            sb.AppendLine($"✓ Đã bảo toàn & đồng bộ đầy đủ {SyncRegistry.UIPrefabRules.Count} Prefab UI cốt lõi!");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return sb.ToString();
        }

        public static string GetSyncSummary() => PerformFullSync();
    }
}
