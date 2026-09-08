using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Tool 1-Click tự động đồng bộ và đóng gói toàn bộ Resource cốt lõi cho Android Build:
    /// 1. Đồng bộ UpgradeData (Thẻ Nâng Cấp khi lên cấp) vào Assets/Resources/Upgrades/
    /// 2. Đồng bộ Audio Clips (BGM, SFX chiến đấu, UI) vào Assets/Resources/Audios/
    /// 3. Đồng bộ Enemy Prefabs (Quái vật Màn 1) vào Assets/Resources/Enemies/
    /// 4. Đồng bộ Character & Permanent Upgrade Database vào Assets/Resources/
    /// </summary>
    public static class AndroidBuildResourceSyncTool
    {
        private const string DATA_UPGRADES_PATH = "Assets/_Data/Upgrades";
        private const string DATA_AUDIOS_PATH = "Assets/_Data/Audios";
        private const string ENEMY_PREFABS_PATH = "Assets/_Prefabs/Characters/Enemies";

        private const string RES_ROOT = "Assets/Resources";
        private const string RES_UPGRADES_PATH = "Assets/Resources/Upgrades";
        private const string RES_AUDIOS_PATH = "Assets/Resources/Audios";
        private const string RES_ENEMIES_PATH = "Assets/Resources/Enemies";

        [MenuItem("Tools/ProjectZombie/⚡ 1-Click Sync All Resources for Android Build", priority = 2)]
        [MenuItem("ProjectZombie/⚡ 1-Click Sync All Resources for Android Build", priority = 2)]
        public static void SyncAllResourcesForAndroid()
        {
            EnsureDirectory(RES_ROOT);
            EnsureDirectory(RES_UPGRADES_PATH);
            EnsureDirectory(RES_AUDIOS_PATH);
            EnsureDirectory(RES_ENEMIES_PATH);

            int upgradesCount = SyncDirectoryAssets(DATA_UPGRADES_PATH, RES_UPGRADES_PATH, "*.asset");
            int audiosCount = SyncDirectoryAssets(DATA_AUDIOS_PATH, RES_AUDIOS_PATH, "*.*", new[] { ".wav", ".mp3", ".ogg", ".asset" });
            int enemiesCount = SyncDirectoryAssets(ENEMY_PREFABS_PATH, RES_ENEMIES_PATH, "*.prefab");

            // Đồng bộ CharacterDatabase và PermanentUpgradeTree nếu cần
            SyncSingleAsset("Assets/_Data/CharacterDatabase.asset", "Assets/Resources/CharacterDatabase.asset");
            SyncSingleAsset("Assets/_Data/MetaProgression/PermanentUpgradeTree.asset", "Assets/Resources/PermanentUpgradeTree.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=#00FF88>[AndroidBuildResourceSyncTool] HOÀN TẤT ĐỒNG BỘ CHO ANDROID!</color>\n" +
                      $"- Thẻ Nâng Cấp: {upgradesCount} assets vào {RES_UPGRADES_PATH}\n" +
                      $"- Âm Thanh (BGM/SFX): {audiosCount} assets vào {RES_AUDIOS_PATH}\n" +
                      $"- Quái Vật: {enemiesCount} prefabs vào {RES_ENEMIES_PATH}\n" +
                      $"Bạn có thể an tâm Build APK / AAB sang thiết bị Android thực tế!");
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static int SyncDirectoryAssets(string sourceDir, string targetDir, string searchPattern, string[] allowedExtensions = null)
        {
            if (!Directory.Exists(sourceDir)) return 0;

            string[] files = Directory.GetFiles(sourceDir, searchPattern, SearchOption.AllDirectories);
            int count = 0;

            foreach (string file in files)
            {
                if (file.EndsWith(".meta")) continue;

                if (allowedExtensions != null)
                {
                    string ext = Path.GetExtension(file).ToLower();
                    bool allowed = false;
                    foreach (var validExt in allowedExtensions)
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
                string destFile = Path.Combine(targetDir, fileName).Replace('\\', '/');
                string srcFile = file.Replace('\\', '/');

                if (File.Exists(destFile))
                {
                    // So sánh byte size để cập nhật nếu có thay đổi
                    var srcInfo = new FileInfo(srcFile);
                    var destInfo = new FileInfo(destFile);
                    if (srcInfo.Length == destInfo.Length && srcInfo.LastWriteTimeUtc <= destInfo.LastWriteTimeUtc)
                    {
                        count++;
                        continue;
                    }
                }

                AssetDatabase.CopyAsset(srcFile, destFile);
                count++;
            }

            return count;
        }

        private static void SyncSingleAsset(string src, string dest)
        {
            if (File.Exists(src))
            {
                if (!File.Exists(dest))
                {
                    AssetDatabase.CopyAsset(src, dest);
                }
            }
        }
    }
}
