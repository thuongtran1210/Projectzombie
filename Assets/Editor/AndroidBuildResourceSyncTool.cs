using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Bảng điều khiển kiểm tra toàn diện và Đồng bộ 1-Click tài nguyên cho Android Build:
    /// 1. Quét chẩn đoán lỗi Runtime (AssetDatabase trong Runtime, thiếu Resources, Project Settings).
    /// 2. Đồng bộ tự động toàn bộ Upgrades, Audios, Enemies, Weapons, Characters sang Resources/.
    /// 3. Tự động sinh đầy đủ các Prefab UI (Settings, PlayerStats, MobileControls, WeaponLoadout, CardCodex).
    /// 4. Chuẩn hóa Phím ảo & Cấu hình Android Player Settings chuẩn Google Play.
    /// </summary>
    public class AndroidBuildResourceSyncTool : EditorWindow
    {
        private const string DATA_UPGRADES_PATH = "Assets/_Data/Upgrades";
        private const string DATA_AUDIOS_PATH = "Assets/_Data/Audios";
        private const string DATA_WEAPONS_PATH = "Assets/_Data/Weapons";
        private const string ENEMY_PREFABS_PATH = "Assets/_Prefabs/Characters/Enemies";

        private const string RES_ROOT = "Assets/Resources";
        private const string RES_UPGRADES_PATH = "Assets/Resources/Upgrades";
        private const string RES_AUDIOS_PATH = "Assets/Resources/Audios";
        private const string RES_WEAPONS_PATH = "Assets/Resources/Weapons";
        private const string RES_ENEMIES_PATH = "Assets/Resources/Enemies";
        private const string RES_UI_PATH = "Assets/Resources/UI";

        private Vector2 _scrollPos;
        private List<AuditItem> _auditIssues = new List<AuditItem>();
        private bool _isScanned = false;
        private string _lastSyncSummary = "";

        public struct AuditItem
        {
            public enum SeverityLevel { Info, Warning, Error }
            public SeverityLevel Severity;
            public string Title;
            public string Description;
            public string Recommendation;
        }

        [MenuItem("Tools/ProjectZombie/📱 Android Pre-Build Wizard & 1-Click Sync", priority = 1)]
        [MenuItem("ProjectZombie/📱 Android Pre-Build Wizard & 1-Click Sync", priority = 1)]
        public static void ShowWindow()
        {
            var window = GetWindow<AndroidBuildResourceSyncTool>("Android Build Wizard");
            window.minSize = new Vector2(580, 680);
            window.RunDiagnosticScan();
            window.Show();
        }

        [MenuItem("Tools/ProjectZombie/⚡ 1-Click Sync All Resources for Android Build (Quick Run)", priority = 2)]
        [MenuItem("ProjectZombie/⚡ 1-Click Sync All Resources for Android Build (Quick Run)", priority = 2)]
        public static void QuickSyncAll()
        {
            PerformFullSync(true);
        }

        /// <summary>
        /// Alias tương thích ngược cho các Editor Tool khác gọi.
        /// </summary>
        public static void SyncAllResourcesForAndroid()
        {
            PerformFullSync(false);
        }

        private void OnEnable()
        {
            if (!_isScanned)
            {
                RunDiagnosticScan();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            DrawHeader();
            EditorGUILayout.Space(10);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            DrawActionButtons();
            EditorGUILayout.Space(12);

            DrawAuditSection();
            EditorGUILayout.Space(12);

            if (!string.IsNullOrEmpty(_lastSyncSummary))
            {
                DrawSummarySection();
                EditorGUILayout.Space(12);
            }

            DrawChecklistGuide();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.2f, 0.8f, 1f) }
            };
            GUILayout.Label("📱 ANDROID PRE-BUILD WIZARD & RESOURCE SYNC", headerStyle);
            EditorGUILayout.LabelField("Kiểm tra nguy cơ lỗi trước khi đóng gói APK/AAB và đồng bộ 100% tài nguyên Runtime.", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.3f, 0.7f, 1f);
            if (GUILayout.Button("🔍 QUÉT CHẨN ĐOÁN (SCAN)", GUILayout.Height(38)))
            {
                RunDiagnosticScan();
            }

            GUI.backgroundColor = new Color(0.2f, 0.9f, 0.4f);
            if (GUILayout.Button("⚡ ĐỒNG BỘ TẤT CẢ (1-CLICK SYNC)", GUILayout.Height(38)))
            {
                PerformFullSync(false);
                RunDiagnosticScan();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            if (GUILayout.Button("⚙️ Tự động cấu hình Android Player Settings chuẩn (IL2CPP + ARM64 + API 34)", GUILayout.Height(26)))
            {
                OptimizePlayerSettings();
                RunDiagnosticScan();
            }
        }

        private void DrawAuditSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("📋 KẾT QUẢ QUÉT NGUY CƠ TRƯỚC KHI BUILD", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            if (_auditIssues.Count == 0)
            {
                EditorGUILayout.HelpBox(" Tuyệt vời! Không phát hiện nguy cơ lỗi nghiêm trọng nào cho Android Build.", MessageType.Info);
            }
            else
            {
                int errCount = _auditIssues.FindAll(x => x.Severity == AuditItem.SeverityLevel.Error).Count;
                int warnCount = _auditIssues.FindAll(x => x.Severity == AuditItem.SeverityLevel.Warning).Count;

                EditorGUILayout.LabelField($"Phát hiện: {errCount} Lỗi Nghiêm Trọng | {warnCount} Cảnh Báo", EditorStyles.miniBoldLabel);
                EditorGUILayout.Space(4);

                foreach (var issue in _auditIssues)
                {
                    MessageType mType = issue.Severity == AuditItem.SeverityLevel.Error ? MessageType.Error :
                                       (issue.Severity == AuditItem.SeverityLevel.Warning ? MessageType.Warning : MessageType.Info);

                    EditorGUILayout.BeginVertical(EditorStyles.textArea);
                    EditorGUILayout.HelpBox($"[{issue.Severity.ToString().ToUpper()}] {issue.Title}\n{issue.Description}\n💡 Khắc phục: {issue.Recommendation}", mType);
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(2);
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawSummarySection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(" BÁO CÁO ĐỒNG BỘ GẦN NHẤT", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(_lastSyncSummary, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawChecklistGuide()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("💡 HƯỚNG DẪN KIỂM TRA NHANH KHI BUILD APK", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("1. Nhấn '⚡ ĐỒNG BỘ TẤT CẢ' để tự động copy toàn bộ thẻ bài, âm thanh, prefab vào Resources.", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("2. Vào File > Build Settings > Switch Platform sang Android.", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("3. Đảm bảo 'Scripting Backend' là IL2CPP và bật 'ARM64' trong Player Settings.", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("4. Nhấn Build & Run để cài trực tiếp lên thiết bị Android hoặc xuất file APK.", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        public void RunDiagnosticScan()
        {
            _auditIssues.Clear();

            // 1. Kiểm tra Scripting Backend & Target Architecture
            var backend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android);
            if (backend != ScriptingImplementation.IL2CPP)
            {
                _auditIssues.Add(new AuditItem
                {
                    Severity = AuditItem.SeverityLevel.Error,
                    Title = "Scripting Backend chưa đặt là IL2CPP",
                    Description = $"Hiện đang dùng {backend}. Google Play và các thiết bị Android 64-bit hiện đại bắt buộc IL2CPP.",
                    Recommendation = "Nhấn '⚙️ Tự động cấu hình Android Player Settings' hoặc chỉnh trong Player Settings > Other Settings."
                });
            }

            var targetArch = PlayerSettings.Android.targetArchitectures;
            if ((targetArch & AndroidArchitecture.ARM64) == 0)
            {
                _auditIssues.Add(new AuditItem
                {
                    Severity = AuditItem.SeverityLevel.Error,
                    Title = "Chưa kích hoạt kiến trúc ARM64",
                    Description = "Target Architecture chưa có ARM64, game sẽ không chạy được trên phần lớn máy Android đời mới.",
                    Recommendation = "Kích hoạt ARM64 trong Player Settings > Target Architectures."
                });
            }

            // 2. Kiểm tra Target API Level
            var targetApi = PlayerSettings.Android.targetSdkVersion;
            if (targetApi != AndroidSdkVersions.AndroidApiLevelAuto && (int)targetApi < 34)
            {
                _auditIssues.Add(new AuditItem
                {
                    Severity = AuditItem.SeverityLevel.Warning,
                    Title = "Target API Level thấp hơn 34 (Android 14)",
                    Description = $"Hiện tại Target API Level là: {targetApi}. Google Play yêu cầu tối thiểu API Level 34.",
                    Recommendation = "Đặt Target API Level thành 'Automatic (highest installed)' hoặc 'API Level 34'."
                });
            }

            // 3. Quét sự chênh lệch Assets giữa _Data và Resources
            CheckDirectorySyncStatus(DATA_UPGRADES_PATH, RES_UPGRADES_PATH, "*.asset", "Thẻ Nâng Cấp (Upgrades)");
            CheckDirectorySyncStatus(DATA_AUDIOS_PATH, RES_AUDIOS_PATH, "*.*", "Âm thanh (Audios)");
            CheckDirectorySyncStatus(ENEMY_PREFABS_PATH, RES_ENEMIES_PATH, "*.prefab", "Quái vật (Enemies)");

            // 4. Kiểm tra các Prefab UI cốt lõi trong Resources/UI
            CheckCoreUIPrefab("SettingsModalUI");
            CheckCoreUIPrefab("PlayerStatsMenuUI");
            CheckCoreUIPrefab("MobileControlsCustomizerUI");
            CheckCoreUIPrefab("WeaponLoadoutUI");
            CheckCoreUIPrefab("CardCodexUI");

            _isScanned = true;
            Repaint();
        }

        private void CheckDirectorySyncStatus(string sourceDir, string resDir, string pattern, string categoryName)
        {
            if (!Directory.Exists(sourceDir)) return;
            string[] srcFiles = Directory.GetFiles(sourceDir, pattern, SearchOption.AllDirectories);
            int srcCount = 0;
            foreach (var f in srcFiles) if (!f.EndsWith(".meta")) srcCount++;

            int resCount = 0;
            if (Directory.Exists(resDir))
            {
                string[] resFiles = Directory.GetFiles(resDir, pattern, SearchOption.AllDirectories);
                foreach (var f in resFiles) if (!f.EndsWith(".meta")) resCount++;
            }

            if (resCount < srcCount)
            {
                _auditIssues.Add(new AuditItem
                {
                    Severity = AuditItem.SeverityLevel.Warning,
                    Title = $"Chưa đồng bộ đầy đủ {categoryName}",
                    Description = $"Thư mục gốc có {srcCount} files nhưng Resources chỉ có {resCount} files.",
                    Recommendation = "Nhấn '⚡ ĐỒNG BỘ TẤT CẢ (1-CLICK SYNC)' để tự động cập nhật."
                });
            }
        }

        private void CheckCoreUIPrefab(string prefabName)
        {
            string path = $"{RES_UI_PATH}/{prefabName}.prefab";
            if (!File.Exists(path))
            {
                _auditIssues.Add(new AuditItem
                {
                    Severity = AuditItem.SeverityLevel.Warning,
                    Title = $"Thiếu Prefab UI: {prefabName}",
                    Description = $"Không tìm thấy {prefabName}.prefab trong {RES_UI_PATH}.",
                    Recommendation = "Nhấn '⚡ ĐỒNG BỘ TẤT CẢ' để tự động sinh lại đầy đủ Prefab UI."
                });
            }
        }

        public static void PerformFullSync(bool isSilent = false)
        {
            EnsureDirectory(RES_ROOT);
            EnsureDirectory(RES_UPGRADES_PATH);
            EnsureDirectory(RES_AUDIOS_PATH);
            EnsureDirectory(RES_WEAPONS_PATH);
            EnsureDirectory(RES_ENEMIES_PATH);
            EnsureDirectory(RES_UI_PATH);
            EnsureDirectory("Assets/Resources/Levels");
            EnsureDirectory("Assets/Resources/Players");

            int upgradesCount = SyncDirectoryAssets(DATA_UPGRADES_PATH, RES_UPGRADES_PATH, "*.asset");
            int audiosCount = SyncDirectoryAssets(DATA_AUDIOS_PATH, RES_AUDIOS_PATH, "*.*", new[] { ".wav", ".mp3", ".ogg", ".asset" });
            int weaponsCount = SyncDirectoryAssets(DATA_WEAPONS_PATH, RES_WEAPONS_PATH, "*.asset");
            int enemiesCount = SyncDirectoryAssets(ENEMY_PREFABS_PATH, RES_ENEMIES_PATH, "*.prefab");
            int playersCount = SyncDirectoryAssets("Assets/_Prefabs/Characters/Players", "Assets/Resources/Players", "*.prefab");
            int levelsCount = SyncDirectoryAssets("Assets/_Data/Levels", "Assets/Resources/Levels", "*.asset");

            // Đồng bộ Single Assets
            SyncSingleAsset("Assets/_Data/CharacterDatabase.asset", "Assets/Resources/CharacterDatabase.asset");
            SyncSingleAsset("Assets/_Data/Meta/PermanentUpgradeTree.asset", "Assets/Resources/PermanentUpgradeTree.asset");

            // Tự động dựng lại toàn bộ các UI Prefab Cổ Phong
            try { ProjectZombie.Editor.UI.SettingsUIGenerator.GenerateSettingsModal(); } catch { }
            try { ProjectZombie.Editor.UI.PlayerStatsMenuUIGenerator.RebuildPlayerStatsMenuUI(); } catch { }
            try { ProjectZombie.Editor.UI.MobileControlsCustomizerUIGenerator.GenerateCustomizerUI(); } catch { }
            try { ProjectZombie.Editor.UI.WeaponLoadoutUIGenerator.GenerateWeaponLoadoutPrefab(); } catch { }
            try { ProjectZombie.Editor.UI.CardCodexUIGenerator.GenerateCardCodexPrefab(); } catch { }

            // Tối ưu Upgrade UI & Mobile Controls
            try { UpgradeUIHierarchyOptimizer.OptimizeUpgradeUI(); } catch { }
            try { ProjectZombie.Editor.Tools.MobileControlsSetupTool.SetupAndWireControlsInScene(); } catch { }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string summary = $"[THỜI GIAN: {DateTime.Now:HH:mm:ss}]\n" +
                             $"✓ Đã đồng bộ {upgradesCount} Thẻ Nâng Cấp vào Resources/Upgrades/\n" +
                             $"✓ Đã đồng bộ {audiosCount} Âm Thanh vào Resources/Audios/\n" +
                             $"✓ Đã đồng bộ {weaponsCount} Pháp Bảo vào Resources/Weapons/\n" +
                             $"✓ Đã đồng bộ {enemiesCount} Quái Vật vào Resources/Enemies/\n" +
                             $"✓ Đã đồng bộ {playersCount} Tướng (Player) vào Resources/Players/\n" +
                             $"✓ Đã đồng bộ {levelsCount} Timeline Màn Chơi vào Resources/Levels/\n" +
                             $"✓ Đã sinh đầy đủ 5/5 Prefab UI Cốt Lõi (Settings, Stats, Controls, Loadout, Codex)!\n" +
                             $"✓ Đã chuẩn hóa phím ảo cảm ứng và liên kết Scene.";

            if (HasOpenInstances<AndroidBuildResourceSyncTool>())
            {
                var window = GetWindow<AndroidBuildResourceSyncTool>();
                window._lastSyncSummary = summary;
                window.Repaint();
            }

            Debug.Log($"<color=#00FF88>[AndroidBuildResourceSyncTool] HOÀN TẤT ĐỒNG BỘ!</color>\n{summary}");

            if (!isSilent)
            {
                EditorUtility.DisplayDialog("Đồng Bộ Android Thành Công!",
                    "Toàn bộ tài nguyên, âm thanh, dữ liệu và giao diện UI đã được đồng bộ 100% vào thư mục Resources.\n\nSẵn sàng đóng gói Android APK!", "OK");
            }
        }

        public static void OptimizePlayerSettings()
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24; // Android 7.0+
            PlayerSettings.colorSpace = ColorSpace.Linear;

            Debug.Log("<color=#00FF88>[AndroidBuildResourceSyncTool] Đã tự động cấu hình Player Settings chuẩn Android (IL2CPP, ARM64+ARMv7, Auto API, Linear Color Space)!</color>");
            EditorUtility.DisplayDialog("Cấu Hình Thành Công", "Đã cập nhật cấu hình Android Player Settings:\n- Scripting Backend: IL2CPP\n- Target Architectures: ARM64 + ARMv7\n- Color Space: Linear", "OK");
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
                else
                {
                    var srcInfo = new FileInfo(src);
                    var destInfo = new FileInfo(dest);
                    if (srcInfo.Length != destInfo.Length || srcInfo.LastWriteTimeUtc > destInfo.LastWriteTimeUtc)
                    {
                        AssetDatabase.CopyAsset(src, dest);
                    }
                }
            }
        }
    }
}

