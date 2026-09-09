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

            // 3. Quét sự chênh lệch Assets giữa _Data/_Prefabs và Resources
            CheckDirectorySyncStatus(DATA_UPGRADES_PATH, RES_UPGRADES_PATH, "*.asset", "Thẻ Nâng Cấp (Upgrades)");
            CheckDirectorySyncStatus(DATA_AUDIOS_PATH, RES_AUDIOS_PATH, "*.*", "Âm thanh (Audios)", new[] { ".wav", ".mp3", ".ogg", ".asset", ".mixer" });
            CheckDirectorySyncStatus(DATA_WEAPONS_PATH, RES_WEAPONS_PATH, "*.asset", "Pháp Bảo (Weapons)");
            CheckDirectorySyncStatus(ENEMY_PREFABS_PATH, RES_ENEMIES_PATH, "*.prefab", "Quái vật (Enemies)");
            CheckDirectorySyncStatus("Assets/_Prefabs/Characters/Players", "Assets/Resources/Players", "*.prefab", "Tướng (Players)");
            CheckDirectorySyncStatus("Assets/_Data/Levels", "Assets/Resources/Levels", "*.asset", "Timeline Màn Chơi (Levels)");
            CheckDirectorySyncStatus("Assets/_Prefabs/UI", RES_UI_PATH, "*.prefab", "Giao Diện (UI Prefabs)");

            // 4. Kiểm tra các Single Assets cốt lõi cho Runtime
            CheckSingleAssetRuntime("Assets/Resources/CharacterDatabase.asset", "CharacterDatabase (Dữ liệu Tướng)", "Assets/_Data/CharacterDatabase.asset");
            CheckSingleAssetRuntime("Assets/Resources/PermanentUpgradeTree.asset", "PermanentUpgradeTree (Cây Nâng Cấp Vĩnh Viễn)", "Assets/_Data/Meta/PermanentUpgradeTree.asset");

            // 5. Kiểm tra các Prefab UI cốt lõi cần thiết trong Runtime
            CheckCoreUIPrefabRuntime("SettingsModalUI");
            CheckCoreUIPrefabRuntime("PlayerStatsMenuUI");
            CheckCoreUIPrefabRuntime("MobileControlsCustomizerUI");
            CheckCoreUIPrefabRuntime("WeaponLoadoutUI");
            CheckCoreUIPrefabRuntime("CardCodexUI");

            _isScanned = true;
            Repaint();
        }

        private void CheckDirectorySyncStatus(string sourceDir, string resDir, string pattern, string categoryName, string[] allowedExtensions = null)
        {
            if (!Directory.Exists(sourceDir)) return;
            string[] srcFiles = Directory.GetFiles(sourceDir, pattern, SearchOption.AllDirectories);
            int srcCount = 0;
            foreach (var f in srcFiles)
            {
                if (f.EndsWith(".meta")) continue;
                if (allowedExtensions != null)
                {
                    string ext = Path.GetExtension(f).ToLower();
                    bool allowed = false;
                    foreach (var ve in allowedExtensions)
                    {
                        if (ext == ve) { allowed = true; break; }
                    }
                    if (!allowed) continue;
                }
                srcCount++;
            }

            int resCount = 0;
            if (Directory.Exists(resDir))
            {
                string[] resFiles = Directory.GetFiles(resDir, pattern, SearchOption.AllDirectories);
                foreach (var f in resFiles)
                {
                    if (f.EndsWith(".meta")) continue;
                    if (allowedExtensions != null)
                    {
                        string ext = Path.GetExtension(f).ToLower();
                        bool allowed = false;
                        foreach (var ve in allowedExtensions)
                        {
                            if (ext == ve) { allowed = true; break; }
                        }
                        if (!allowed) continue;
                    }
                    resCount++;
                }
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

        private void CheckSingleAssetRuntime(string resPath, string assetName, string srcPath)
        {
            if (!File.Exists(resPath) && !File.Exists(srcPath))
            {
                _auditIssues.Add(new AuditItem
                {
                    Severity = AuditItem.SeverityLevel.Warning,
                    Title = $"Thiếu Asset: {assetName}",
                    Description = $"Không tìm thấy asset tại {srcPath} hoặc {resPath}.",
                    Recommendation = "Nhấn '⚡ ĐỒNG BỘ TẤT CẢ' để tự động cập nhật."
                });
            }
            else if (!File.Exists(resPath) && File.Exists(srcPath))
            {
                _auditIssues.Add(new AuditItem
                {
                    Severity = AuditItem.SeverityLevel.Warning,
                    Title = $"Chưa đồng bộ Runtime: {assetName}",
                    Description = $"Asset đã có tại nguồn ({srcPath}) nhưng chưa được copy vào thư mục Resources/ để load khi chạy build Android.",
                    Recommendation = "Nhấn '⚡ ĐỒNG BỘ TẤT CẢ (1-CLICK SYNC)' để tự động copy vào Resources."
                });
            }
        }

        private void CheckCoreUIPrefabRuntime(string prefabName)
        {
            string resPath = $"{RES_UI_PATH}/{prefabName}.prefab";
            string masterPath = $"Assets/_Prefabs/UI/{prefabName}.prefab";

            if (!File.Exists(resPath) && !File.Exists(masterPath))
            {
                _auditIssues.Add(new AuditItem
                {
                    Severity = AuditItem.SeverityLevel.Warning,
                    Title = $"Thiếu Prefab UI: {prefabName}",
                    Description = $"Không tìm thấy {prefabName}.prefab trong Assets/_Prefabs/UI hoặc Resources/UI.",
                    Recommendation = "Nhấn '⚡ ĐỒNG BỘ TẤT CẢ' để tự động copy/sinh Prefab UI."
                });
            }
            else if (!File.Exists(resPath) && File.Exists(masterPath))
            {
                _auditIssues.Add(new AuditItem
                {
                    Severity = AuditItem.SeverityLevel.Warning,
                    Title = $"Chưa đồng bộ Runtime UI: {prefabName}",
                    Description = $"Prefab đã có tại Assets/_Prefabs/UI/{prefabName}.prefab nhưng chưa được đồng bộ vào Resources/UI/ để load khi chạy game.",
                    Recommendation = "Nhấn '⚡ ĐỒNG BỘ TẤT CẢ' để copy ngay vào Resources/UI/."
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
            int audiosCount = SyncDirectoryAssets(DATA_AUDIOS_PATH, RES_AUDIOS_PATH, "*.*", new[] { ".wav", ".mp3", ".ogg", ".asset", ".mixer" });
            int weaponsCount = SyncDirectoryAssets(DATA_WEAPONS_PATH, RES_WEAPONS_PATH, "*.asset");
            int enemiesCount = SyncDirectoryAssets(ENEMY_PREFABS_PATH, RES_ENEMIES_PATH, "*.prefab");
            int playersCount = SyncDirectoryAssets("Assets/_Prefabs/Characters/Players", "Assets/Resources/Players", "*.prefab");
            int levelsCount = SyncDirectoryAssets("Assets/_Data/Levels", "Assets/Resources/Levels", "*.asset");

            // Đồng bộ Single Assets
            SyncSingleAsset("Assets/_Data/CharacterDatabase.asset", "Assets/Resources/CharacterDatabase.asset");
            SyncSingleAsset("Assets/_Data/Meta/PermanentUpgradeTree.asset", "Assets/Resources/PermanentUpgradeTree.asset");

            // 1. Đồng bộ Prefab UI từ Assets/_Prefabs/UI sang Assets/Resources/UI (Bảo toàn 100% chỉnh sửa của người dùng)
            int uiCount = SyncDirectoryAssets("Assets/_Prefabs/UI", RES_UI_PATH, "*.prefab");

            // 2. Chỉ sinh fallback nếu Prefab hoàn toàn chưa tồn tại ở cả 2 nơi (KHÔNG ghi đè prefab người dùng đã sửa)
            EnsureFallbackUIPrefab("SettingsModalUI", () => ProjectZombie.Editor.UI.SettingsUIGenerator.GenerateSettingsModal());
            EnsureFallbackUIPrefab("PlayerStatsMenuUI", () => ProjectZombie.Editor.UI.PlayerStatsMenuUIGenerator.RebuildPlayerStatsMenuUI());
            EnsureFallbackUIPrefab("MobileControlsCustomizerUI", () => ProjectZombie.Editor.UI.MobileControlsCustomizerUIGenerator.GenerateCustomizerUI());
            EnsureFallbackUIPrefab("WeaponLoadoutUI", () => ProjectZombie.Editor.UI.WeaponLoadoutUIGenerator.GenerateWeaponLoadoutPrefab());
            EnsureFallbackUIPrefab("CardCodexUI", () => ProjectZombie.Editor.UI.CardCodexUIGenerator.GenerateCardCodexPrefab());

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string summary = $"[THỜI GIAN: {DateTime.Now:HH:mm:ss}]\n" +
                             $"✓ Đã đồng bộ {upgradesCount} Thẻ Nâng Cấp vào Resources/Upgrades/\n" +
                             $"✓ Đã đồng bộ {audiosCount} Âm Thanh vào Resources/Audios/\n" +
                             $"✓ Đã đồng bộ {weaponsCount} Pháp Bảo vào Resources/Weapons/\n" +
                             $"✓ Đã đồng bộ {enemiesCount} Quái Vật vào Resources/Enemies/\n" +
                             $"✓ Đã đồng bộ {playersCount} Tướng (Player) vào Resources/Players/\n" +
                             $"✓ Đã đồng bộ {levelsCount} Timeline Màn Chơi vào Resources/Levels/\n" +
                             $"✓ Đã bảo toàn & đồng bộ {uiCount} Prefab UI tùy chỉnh vào Resources/UI/!";

            if (HasOpenInstances<AndroidBuildResourceSyncTool>())
            {
                var window = GetWindow<AndroidBuildResourceSyncTool>();
                window._lastSyncSummary = summary;
                window.Repaint();
            }

            Debug.Log($"<color=#00FF88>[AndroidBuildResourceSyncTool] HOÀN TẤT ĐỒNG BỘ (BẢO TOÀN CUSTOM EDITS)!</color>\n{summary}");

            if (!isSilent)
            {
                EditorUtility.DisplayDialog("Đồng Bộ Android Thành Công!",
                    "Toàn bộ tài nguyên, âm thanh, dữ liệu và giao diện UI đã được đồng bộ 100% vào thư mục Resources.\n\nMọi thay đổi tùy chỉnh của bạn trên Editor đã được bảo toàn nguyên vẹn!", "OK");
            }
        }

        private static void EnsureFallbackUIPrefab(string prefabName, Action generateAction)
        {
            string resPath = $"{RES_UI_PATH}/{prefabName}.prefab";
            string masterPath = $"Assets/_Prefabs/UI/{prefabName}.prefab";

            if (!File.Exists(resPath) && !File.Exists(masterPath))
            {
                try
                {
                    generateAction?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[AndroidBuildResourceSyncTool] Không thể sinh fallback cho {prefabName}: {e.Message}");
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
            EnsureDirectory(targetDir);

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

                    // Nếu cùng kích thước và thời gian sửa đổi gần như nhau (trong 2s), bỏ qua
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

        private static void SyncSingleAsset(string src, string dest)
        {
            if (File.Exists(src))
            {
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
                File.Copy(dest, src, true);
                AssetDatabase.ImportAsset(src, ImportAssetOptions.ForceUpdate);
            }
        }
    }
}

