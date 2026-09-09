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

        public static int SyncDirectory(SyncRule rule)
        {
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

                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(rule.TargetPath, fileName).Replace('\\', '/');
                string srcFile = file.Replace('\\', '/');

                if (File.Exists(destFile))
                {
                    var srcInfo = new FileInfo(srcFile);
                    var destInfo = new FileInfo(destFile);

                    // Bỏ qua nếu cùng kích thước và thời gian sửa đổi gần như nhau (trong 2s)
                    if (srcInfo.Length == destInfo.Length && Math.Abs((srcInfo.LastWriteTimeUtc - destInfo.LastWriteTimeUtc).TotalSeconds) < 2)
                    {
                        count++;
                        continue;
                    }

                    // Bảo vệ thay đổi của người dùng: Nếu file đích (Resources) mới hơn file nguồn (_Data), đồng bộ ngược lại!
                    if (destInfo.LastWriteTimeUtc > srcInfo.LastWriteTimeUtc.AddSeconds(2))
                    {
                        File.Copy(destFile, srcFile, true);
                        AssetDatabase.ImportAsset(srcFile, ImportAssetOptions.ForceUpdate);
                        count++;
                        continue;
                    }
                }

                File.Copy(srcFile, destFile, true);
                AssetDatabase.ImportAsset(destFile, ImportAssetOptions.ForceUpdate);
                count++;
            }

            return count;
        }

        public static void SyncSingleAsset(SyncRule rule)
        {
            string src = rule.SourcePath;
            string dest = rule.TargetPath;

            if (File.Exists(src))
            {
                EnsureDirectory(Path.GetDirectoryName(dest));
                if (!File.Exists(dest))
                {
                    File.Copy(src, dest, true);
                    AssetDatabase.ImportAsset(dest, ImportAssetOptions.ForceUpdate);
                }
                else
                {
                    var srcInfo = new FileInfo(src);
                    var destInfo = new FileInfo(dest);

                    if (srcInfo.Length == destInfo.Length && Math.Abs((srcInfo.LastWriteTimeUtc - destInfo.LastWriteTimeUtc).TotalSeconds) < 2)
                    {
                        return;
                    }

                    if (destInfo.LastWriteTimeUtc > srcInfo.LastWriteTimeUtc.AddSeconds(2))
                    {
                        File.Copy(dest, src, true);
                        AssetDatabase.ImportAsset(src, ImportAssetOptions.ForceUpdate);
                        return;
                    }

                    File.Copy(src, dest, true);
                    AssetDatabase.ImportAsset(dest, ImportAssetOptions.ForceUpdate);
                }
            }
            else if (File.Exists(dest))
            {
                EnsureDirectory(Path.GetDirectoryName(src));
                File.Copy(dest, src, true);
                AssetDatabase.ImportAsset(src, ImportAssetOptions.ForceUpdate);
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
            }
            else if (File.Exists(masterPath) && !File.Exists(resPath))
            {
                File.Copy(masterPath, resPath, true);
                AssetDatabase.ImportAsset(resPath, ImportAssetOptions.ForceUpdate);
            }
            else if (!File.Exists(masterPath) && File.Exists(resPath))
            {
                File.Copy(resPath, masterPath, true);
                AssetDatabase.ImportAsset(masterPath, ImportAssetOptions.ForceUpdate);
            }
            else if (File.Exists(masterPath) && File.Exists(resPath))
            {
                var masterInfo = new FileInfo(masterPath);
                var resInfo = new FileInfo(resPath);

                if (resInfo.LastWriteTimeUtc > masterInfo.LastWriteTimeUtc.AddSeconds(2))
                {
                    File.Copy(resPath, masterPath, true);
                    AssetDatabase.ImportAsset(masterPath, ImportAssetOptions.ForceUpdate);
                }
                else if (masterInfo.LastWriteTimeUtc > resInfo.LastWriteTimeUtc.AddSeconds(2))
                {
                    File.Copy(masterPath, resPath, true);
                    AssetDatabase.ImportAsset(resPath, ImportAssetOptions.ForceUpdate);
                }
            }
        }

        public static string PerformFullSync()
        {
            EnsureDirectory("Assets/Resources");

            var sb = new StringBuilder();
            sb.AppendLine($"[THỜI GIAN: {DateTime.Now:HH:mm:ss}]");

            foreach (var rule in SyncRegistry.DirectoryRules)
            {
                int count = SyncDirectory(rule);
                sb.AppendLine($"✓ Đã đồng bộ {count} files vào {rule.TargetPath}");
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
    }
}
