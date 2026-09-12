using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ProjectZombie.EditorTools.BuildSync;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Giao diện điều khiển Android Pre-Build Wizard & 1-Click Sync.
    /// Tách bạch hoàn toàn phần hiển thị GUI với Core logic (ResourceSyncEngine, AndroidAuditEngine).
    /// </summary>
    public class AndroidBuildResourceSyncTool : EditorWindow
    {
        private Vector2 _scrollPos;
        [SerializeField] private List<AuditItem> _auditIssues = new List<AuditItem>();
        [SerializeField] private bool _isScanned = false;
        [SerializeField] private string _lastSyncSummary = "";

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
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("⚙️ Cấu hình Player Settings (IL2CPP + ARM64)", GUILayout.Height(28)))
            {
                AndroidAuditEngine.OptimizePlayerSettings();
                RunDiagnosticScan();
            }

            GUI.backgroundColor = new Color(1f, 0.75f, 0.2f);
            if (GUILayout.Button("📦 TỐI ƯU DUNG LƯỢNG APK (Nén ASTC & Vorbis)", GUILayout.Height(28)))
            {
                AndroidAuditEngine.OptimizeTexturesAndAudios();
                RunDiagnosticScan();
            }

            GUI.backgroundColor = new Color(0.6f, 0.4f, 1f);
            if (GUILayout.Button("🌐 Đóng Gói Addressables Bundles (CDN)", GUILayout.Height(28)))
            {
                ProjectZombie.Editor.AddressablesTools.AddressableGroupsSetupTool.BuildAddressablesBundles();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
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

                for (int i = 0; i < _auditIssues.Count; i++)
                {
                    var issue = _auditIssues[i];
                    MessageType mType = issue.Severity == AuditItem.SeverityLevel.Error ? MessageType.Error :
                                       (issue.Severity == AuditItem.SeverityLevel.Warning ? MessageType.Warning : MessageType.Info);

                    EditorGUILayout.BeginVertical(EditorStyles.textArea);
                    EditorGUILayout.HelpBox($"[{issue.Severity.ToString().ToUpper()}] {issue.Title}\n{issue.Description}\n💡 Khắc phục: {issue.Recommendation}", mType);

                    if (issue.ActionType != AuditItem.FixActionType.None)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUI.backgroundColor = issue.Severity == AuditItem.SeverityLevel.Error ? new Color(1f, 0.6f, 0.6f) : new Color(1f, 0.85f, 0.4f);
                        string btnText = string.IsNullOrEmpty(issue.FixButtonText) ? "🛠️ Khắc Phục Mục Này Ngay" : issue.FixButtonText;
                        if (GUILayout.Button(btnText, GUILayout.Height(24), GUILayout.MinWidth(180)))
                        {
                            ExecuteIndividualFix(issue);
                        }
                        GUI.backgroundColor = Color.white;
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(4);
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
            _auditIssues = AndroidAuditEngine.RunAudit();
            _isScanned = true;
            Repaint();

            if (_auditIssues.Count == 0)
            {
                ShowNotification(new GUIContent("✓ Quét hoàn tất: 0 Lỗi, 0 Cảnh Báo!"));
            }
            else
            {
                int errCount = _auditIssues.FindAll(x => x.Severity == AuditItem.SeverityLevel.Error).Count;
                int warnCount = _auditIssues.FindAll(x => x.Severity == AuditItem.SeverityLevel.Warning).Count;
                ShowNotification(new GUIContent($"Quét xong: {errCount} Lỗi, {warnCount} Cảnh Báo"));
            }
        }

        public static void PerformFullSync(bool isSilent = false)
        {
            string summary = ResourceSyncEngine.PerformFullSync();

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
                    "Toàn bộ tài nguyên, âm thanh, dữ liệu và giao diện UI đã được đồng bộ 100% vào thư mục Resources.\n\nMọi thay đổi tùy chỉnh của bạn trên Editor đã được bảo toàn nguyên vẹn!", "OK");
            }
        }

        private void ExecuteIndividualFix(AuditItem issue)
        {
            switch (issue.ActionType)
            {
                case AuditItem.FixActionType.OptimizePlayerSettings:
                    AndroidAuditEngine.OptimizePlayerSettings();
                    break;

                case AuditItem.FixActionType.SyncDirectory:
                    int count = ResourceSyncEngine.SyncDirectory(issue.Rule);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    ShowNotification(new GUIContent($"✓ Đã đồng bộ {count} files vào {issue.Rule.TargetPath}"));
                    break;

                case AuditItem.FixActionType.SyncSingleAsset:
                    ResourceSyncEngine.SyncSingleAsset(issue.Rule);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    ShowNotification(new GUIContent($"✓ Đã đồng bộ {issue.Rule.Name}!"));
                    break;

                case AuditItem.FixActionType.SyncUIPrefab:
                    ResourceSyncEngine.SyncUIPrefab(issue.Rule);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    ShowNotification(new GUIContent($"✓ Đã cập nhật UI Prefab {issue.Rule.Name}!"));
                    break;
            }

            RunDiagnosticScan();
        }
    }
}
