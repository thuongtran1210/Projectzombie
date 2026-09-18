#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using ProjectZombie.Editor.Testing;
using ProjectZombie.EditorTools.BuildSync;
using ProjectZombie.Editor.Build;
using ProjectZombie.Editor.AddressablesTools;

namespace ProjectZombie.Editor.BuildAndDeploy
{
    /// <summary>
    /// Bảng Điều Khiển Trực Quan Quy Trình Build Android & Addressables (Workflow Dashboard).
    /// Giúp lập trình viên:
    /// 1. Quan sát trạng thái thực tế mọi cấu hình (Platform, Addressables PlayMode, Sync Delta, IL2CPP).
    /// 2. Chuyển đổi nhanh 1-Click giữa chế độ "Test trên Editor" và "Sẵn sàng Build Android".
    /// 3. Thực hiện từng bước có kiểm soát (Kiểm toán -> Đồng bộ -> Build Bundles -> Build APK).
    /// 4. Giữ toàn quyền chủ động, hiển thị log minh bạch, không thao tác ngầm.
    /// </summary>
    public class AndroidAddressablesDashboardWindow : EditorWindow
    {
        private Vector2 _scrollPos;
        private PrefabIntegrityAuditor.AuditReport _auditReport;
        private bool _isAuditing = false;
        private string _lastOperationStatus = "Sẵn sàng.";
        private MessageType _statusMessageType = MessageType.Info;

        [MenuItem("ProjectZombie/🤖 Android & Addressables Dashboard", priority = 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<AndroidAddressablesDashboardWindow>("Android & Addressables Control Panel");
            window.minSize = new Vector2(520, 680);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshAudit();
        }

        private void OnFocus()
        {
            RefreshAudit();
        }

        private void RefreshAudit()
        {
            _isAuditing = true;
            try
            {
                _auditReport = PrefabIntegrityAuditor.AuditPlayerPrefabs();
            }
            finally
            {
                _isAuditing = false;
            }
            Repaint();
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            DrawHeader();
            EditorGUILayout.Space(8);

            DrawStatusSection();
            EditorGUILayout.Space(10);

            DrawQuickModeSwitcherSection();
            EditorGUILayout.Space(10);

            DrawWorkflowChecklistSection();
            EditorGUILayout.Space(10);

            DrawAuditReportSection();
            EditorGUILayout.Space(10);

            DrawFooter();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.2f, 0.85f, 1f) }
            };
            EditorGUILayout.LabelField("🎮 BẢNG ĐIỀU KHIỂN BUILD ANDROID & ADDRESSABLES", headerStyle);
            EditorGUILayout.LabelField("Quan sát trực quan, kiểm soát chuyển đổi chế độ và loại bỏ lỗi xử lý chéo", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawStatusSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("📊 TRẠNG THÁI HỆ THỐNG HIỆN TẠI", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            // 1. Target Platform
            bool isAndroid = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("• Nền Tảng Đang Chọn:", GUILayout.Width(180));
            GUI.color = isAndroid ? new Color(0.4f, 1f, 0.4f) : new Color(1f, 0.8f, 0.3f);
            EditorGUILayout.LabelField(isAndroid ? "✅ Android" : $"⚠️ {EditorUserBuildSettings.activeBuildTarget} (Chưa phải Android)", EditorStyles.boldLabel);
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();

            // 2. Addressables Play Mode
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            string currentPlayMode = "Không tìm thấy Addressables";
            bool isAssetDatabaseMode = false;
            bool isExistingBuildMode = false;

            if (settings != null)
            {
                int idx = settings.ActivePlayModeDataBuilderIndex;
                if (idx >= 0 && idx < settings.DataBuilders.Count)
                {
                    currentPlayMode = settings.DataBuilders[idx].name;
                    isAssetDatabaseMode = currentPlayMode.Contains("AssetDatabase");
                    isExistingBuildMode = currentPlayMode.Contains("ExistingBuild");
                }
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("• Chế Độ Addressables PlayMode:", GUILayout.Width(180));
            if (isAssetDatabaseMode)
            {
                GUI.color = new Color(0.3f, 0.85f, 1f); // Xanh dương
                EditorGUILayout.LabelField("💻 Use Asset Database (Test Editor Nhanh)", EditorStyles.boldLabel);
            }
            else if (isExistingBuildMode)
            {
                GUI.color = new Color(1f, 0.9f, 0.3f); // Vàng
                EditorGUILayout.LabelField("📦 Use Existing Build (Dùng Cho APK)", EditorStyles.boldLabel);
            }
            else
            {
                EditorGUILayout.LabelField(currentPlayMode, EditorStyles.boldLabel);
            }
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();

            // 3. Scripting Backend & Architecture
            var targetGroup = BuildTargetGroup.Android;
            var backend = PlayerSettings.GetScriptingBackend(targetGroup);
            var arch = PlayerSettings.Android.targetArchitectures;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("• Scripting Backend / CPU:", GUILayout.Width(180));
            bool isIL2CPP = backend == ScriptingImplementation.IL2CPP;
            bool hasARM64 = (arch & AndroidArchitecture.ARM64) != 0;
            EditorGUILayout.LabelField($"{(isIL2CPP ? "IL2CPP" : "Mono")} | {(hasARM64 ? "ARM64 (Chuẩn Google Play)" : "ARMv7")}");
            EditorGUILayout.EndHorizontal();

            // 4. Kiểm toán toàn vẹn
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("• Trạng Thái Player Prefabs:", GUILayout.Width(180));
            if (_auditReport != null)
            {
                if (_auditReport.IsHealthy && _auditReport.Warnings.Count == 0)
                {
                    GUI.color = new Color(0.4f, 1f, 0.4f);
                    EditorGUILayout.LabelField("✅ Toàn Vẹn & Khớp 100% với Resources", EditorStyles.boldLabel);
                }
                else if (_auditReport.IsHealthy)
                {
                    GUI.color = new Color(1f, 0.85f, 0.2f);
                    EditorGUILayout.LabelField($"⚠️ Có {_auditReport.Warnings.Count} Cảnh Báo Cần Đồng Bộ", EditorStyles.boldLabel);
                }
                else
                {
                    GUI.color = new Color(1f, 0.35f, 0.35f);
                    EditorGUILayout.LabelField($"❌ Có {_auditReport.Errors.Count} Lỗi Nghiêm Trọng!", EditorStyles.boldLabel);
                }
                GUI.color = Color.white;
            }
            else
            {
                EditorGUILayout.LabelField("Đang kiểm tra...");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawQuickModeSwitcherSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("⚡ CHUYỂN ĐỔI CHẾ ĐỘ NHANH (1-CLICK TOGGLE)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Chọn một trong hai chế độ dưới đây. Dashboard sẽ tự động thiết lập chính xác các thông số và thông báo ngay trên màn hình:", MessageType.None);
            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();

            // NÚT 1: Chế độ Editor Test
            GUI.backgroundColor = new Color(0.3f, 0.8f, 1f);
            if (GUILayout.Button("💻 CHUYỂN SANG TEST TRÊN EDITOR\n(Asset Database / Chạy Ngay)", GUILayout.Height(44)))
            {
                SwitchToEditorTestMode();
            }

            // NÚT 2: Chế độ Android Build
            GUI.backgroundColor = new Color(0.35f, 1f, 0.5f);
            if (GUILayout.Button("🤖 CHUYỂN SANG SẴN SÀNG BUILD APK\n(Existing Build / IL2CPP)", GUILayout.Height(44)))
            {
                SwitchToAndroidBuildMode();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawWorkflowChecklistSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("📋 QUY TRÌNH ĐÓNG GÓI CÓ KIỂM SOÁT (TỪNG BƯỚC)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Bạn có thể bấm chạy độc lập từng bước hoặc kiểm tra trạng thái trước khi đóng gói APK:", MessageType.None);
            EditorGUILayout.Space(4);

            // BƯỚC 1: Kiểm toán & Đồng bộ Resources
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("1. Đồng Bộ Tài Nguyên Gốc sang Resources (SSOT)", GUILayout.Width(300));
            if (GUILayout.Button("Chạy Đồng Bộ Ngay", GUILayout.Height(26)))
            {
                ExecuteStep1_SyncResources();
            }
            EditorGUILayout.EndHorizontal();

            // BƯỚC 2: Kiểm tra Asset & Shaders Validator
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("2. Kiểm Tra Tài Nguyên & Shaders Validator", GUILayout.Width(300));
            if (GUILayout.Button("Kiểm Tra (Validate)", GUILayout.Height(26)))
            {
                ExecuteStep2_ValidateAssets();
            }
            EditorGUILayout.EndHorizontal();

            // BƯỚC 3: Đóng gói Addressables Bundles
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("3. Đóng Gói Addressables Content Bundles", GUILayout.Width(300));
            if (GUILayout.Button("Build Bundles", GUILayout.Height(26)))
            {
                ExecuteStep3_BuildAddressables();
            }
            EditorGUILayout.EndHorizontal();

            // BƯỚC 4: Xuất APK
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("4. Xuất File Cài Đặt Android APK", GUILayout.Width(300));
            GUI.backgroundColor = new Color(0.4f, 1f, 0.4f);
            if (GUILayout.Button("Bắt Đầu Build APK", GUILayout.Height(26)))
            {
                ExecuteStep4_BuildAPK();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawAuditReportSection()
        {
            if (_auditReport == null) return;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("🔍 CHI TIẾT KIỂM TOÁN TÀI NGUYÊN (PREFAB INTEGRITY)", EditorStyles.boldLabel);
            if (GUILayout.Button("Quét Lại", GUILayout.Width(75)))
            {
                RefreshAudit();
            }
            if (!_auditReport.IsHealthy || _auditReport.Warnings.Count > 0)
            {
                GUI.backgroundColor = new Color(0.3f, 1f, 0.6f);
                if (GUILayout.Button("Tự Sửa Lỗi (Fix All)", GUILayout.Width(120)))
                {
                    PrefabIntegrityAuditor.FixAllIssues();
                    RefreshAudit();
                    _lastOperationStatus = "Đã sửa thành công và đồng bộ toàn bộ Player Prefabs!";
                    _statusMessageType = MessageType.Info;
                }
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);

            if (_auditReport.Errors.Count > 0)
            {
                foreach (var err in _auditReport.Errors)
                {
                    EditorGUILayout.HelpBox(err, MessageType.Error);
                }
            }

            if (_auditReport.Warnings.Count > 0)
            {
                foreach (var warn in _auditReport.Warnings)
                {
                    EditorGUILayout.HelpBox(warn, MessageType.Warning);
                }
            }

            if (_auditReport.IsHealthy && _auditReport.Warnings.Count == 0)
            {
                EditorGUILayout.HelpBox("Tất cả Player Prefabs đều có đầy đủ CharacterCombat, Network Components và khớp 100% giữa _Prefabs và Resources.", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawFooter()
        {
            if (!string.IsNullOrEmpty(_lastOperationStatus))
            {
                EditorGUILayout.HelpBox(_lastOperationStatus, _statusMessageType);
            }
        }

        // =========================================================================
        // CÁC HÀM THỰC THI LOGIC CỦA DASHBOARD
        // =========================================================================

        private void SwitchToEditorTestMode()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
            {
                for (int i = 0; i < settings.DataBuilders.Count; i++)
                {
                    if (settings.DataBuilders[i].name.Contains("AssetDatabase"))
                    {
                        settings.ActivePlayModeDataBuilderIndex = i;
                        EditorUtility.SetDirty(settings);
                        AssetDatabase.SaveAssets();
                        break;
                    }
                }
            }

            _lastOperationStatus = "Đã chuyển Addressables sang 'Use Asset Database (fastest)'. Bạn có thể bấm PLAY trên Editor để test ngay lập tức!";
            _statusMessageType = MessageType.Info;
            RefreshAudit();
        }

        private void SwitchToAndroidBuildMode()
        {
            // 1. Chuyển Switch Target sang Android nếu chưa phải
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }

            // 2. Chuyển Addressables sang Use Existing Build
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
            {
                for (int i = 0; i < settings.DataBuilders.Count; i++)
                {
                    if (settings.DataBuilders[i].name.Contains("ExistingBuild"))
                    {
                        settings.ActivePlayModeDataBuilderIndex = i;
                        EditorUtility.SetDirty(settings);
                        AssetDatabase.SaveAssets();
                        break;
                    }
                }
            }

            _lastOperationStatus = "Đã chuyển cấu hình sang Android & 'Use Existing Build'. Sẵn sàng cho quy trình đóng gói APK!";
            _statusMessageType = MessageType.Info;
            RefreshAudit();
        }

        private void ExecuteStep1_SyncResources()
        {
            ResourceSyncEngine.PerformFullSync();
            RefreshAudit();
            _lastOperationStatus = "Đã hoàn tất đồng bộ toàn bộ tài nguyên Master sang Resources.";
            _statusMessageType = MessageType.Info;
        }

        private void ExecuteStep2_ValidateAssets()
        {
            bool passed = AndroidBuildAssetValidator.RunFullValidation(isBuildPipeline: false);
            _lastOperationStatus = passed ? "Kiểm tra hoàn tất: Tài nguyên hợp lệ 100%!" : "Phát hiện vấn đề! Xem chi tiết trong cửa sổ Console.";
            _statusMessageType = passed ? MessageType.Info : MessageType.Error;
        }

        private void ExecuteStep3_BuildAddressables()
        {
            AddressableGroupsSetupTool.BuildAddressablesBundles();
            _lastOperationStatus = "Đã hoàn tất đóng gói Addressables Content Bundles vào ServerData/Android/.";
            _statusMessageType = MessageType.Info;
        }

        private void ExecuteStep4_BuildAPK()
        {
            string exportPath = EditorUtility.SaveFilePanel("Chọn Nơi Lưu File APK", "Builds/Android", $"ProjectZombie_{DateTime.Now:yyyyMMdd_HHmm}.apk", "apk");
            if (string.IsNullOrEmpty(exportPath)) return;

            string buildDir = Path.GetDirectoryName(exportPath);
            if (!Directory.Exists(buildDir)) Directory.CreateDirectory(buildDir);

            // Bắt đầu Build
            string[] scenes = new string[] { "Assets/Scenes/SampleScene.unity" };
            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = exportPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(buildOptions);
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                _lastOperationStatus = $"Build APK THÀNH CÔNG! File tại: {exportPath}";
                _statusMessageType = MessageType.Info;

                // Hỏi người dùng có muốn hoàn trả Addressables về Editor Test không
                bool restore = EditorUtility.DisplayDialog(
                    "Build APK Thành Công!",
                    $"Đã xuất file APK thành công ({report.summary.totalSize / (1024 * 1024)} MB).\n\nBạn có muốn tự động hoàn trả chế độ Addressables về 'Use Asset Database' để tiếp tục test trên Unity Editor không?",
                    "Có, hoàn trả ngay",
                    "Không, giữ nguyên"
                );

                if (restore)
                {
                    SwitchToEditorTestMode();
                }
            }
            else
            {
                _lastOperationStatus = $"Build APK THẤT BẠI: {report.summary.result}. Xem log Console để biết chi tiết.";
                _statusMessageType = MessageType.Error;
            }
        }
    }
}
#endif
