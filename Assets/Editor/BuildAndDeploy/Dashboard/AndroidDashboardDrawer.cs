#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

namespace ProjectZombie.Editor.BuildAndDeploy.Dashboard
{
    /// <summary>
    /// Chuyên trách hiển thị toàn bộ giao diện GUI của Android & Addressables Dashboard.
    /// Tách biệt hoàn toàn khỏi logic nghiệp vụ theo nguyên lý Single Responsibility.
    /// </summary>
    public class AndroidDashboardDrawer
    {
        private readonly AddressablesWorkflowService _service;

        public AndroidDashboardDrawer(AddressablesWorkflowService service)
        {
            _service = service;
        }

        public void DrawHeader()
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

        public void DrawStatusSection()
        {
            // Xác định chế độ hiện tại
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            string currentPlayMode = "Không tìm thấy Addressables";
            bool isAssetDatabaseMode = false;
            bool isExistingBuildMode = false;

            if (settings != null)
            {
                int idx = settings.ActivePlayModeDataBuilderIndex;
                if (idx >= 0 && idx < settings.DataBuilders.Count)
                {
                    string builderName = settings.DataBuilders[idx].name;
                    // Trong Addressables:
                    // 1. BuildScriptFastMode / AssetDatabase = "Use Asset Database (fastest)"
                    // 2. BuildScriptVirtualMode = "Simulate Groups (advanced)"
                    // 3. BuildScriptPackedPlayMode / ExistingBuild = "Use Existing Build (requires built groups)"
                    isAssetDatabaseMode = builderName.Contains("AssetDatabase") || builderName.Contains("FastMode") || builderName.Contains("VirtualMode");
                    isExistingBuildMode = builderName.Contains("ExistingBuild") || builderName.Contains("PackedPlayMode");

                    if (isAssetDatabaseMode)
                    {
                        currentPlayMode = builderName.Contains("FastMode") 
                            ? "Use Asset Database (Fast Mode)" 
                            : (builderName.Contains("VirtualMode") ? "Simulate Groups (Virtual Mode)" : "Use Asset Database");
                    }
                    else if (isExistingBuildMode)
                    {
                        currentPlayMode = "Use Existing Build";
                    }
                    else
                    {
                        currentPlayMode = builderName;
                    }
                }
            }

            bool isAndroid = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
            bool isReadyForAPK = isAndroid && isExistingBuildMode;
            bool isReadyForEditor = isAssetDatabaseMode;

            // 1. BANNER TRỰC QUAN MÔI TRƯỜNG HIỆN TẠI (CURRENT ACTIVE ENVIRONMENT BADGE)
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            if (isReadyForEditor)
            {
                GUI.color = new Color(0.2f, 0.9f, 1f);
                EditorGUILayout.LabelField($"💻 ĐANG Ở CHẾ ĐỘ: TEST TRÊN UNITY EDITOR ({currentPlayMode.ToUpper()})", EditorStyles.boldLabel);
                GUI.color = Color.white;
                EditorGUILayout.LabelField("• Load trực tiếp từ AssetDatabase (nhanh nhất), không cần build bundle trước mỗi lần Play, hỗ trợ hot-reload script & data.", EditorStyles.miniLabel);
            }
            else if (isReadyForAPK)
            {
                GUI.color = new Color(0.35f, 1f, 0.4f);
                EditorGUILayout.LabelField("🤖 ĐANG Ở CHẾ ĐỘ: SẴN SÀNG BUILD ANDROID APK (PRODUCTION)", EditorStyles.boldLabel);
                GUI.color = Color.white;
                EditorGUILayout.LabelField("• Target: Android | Addressables: Existing Build | Yêu cầu Bundles mới nhất trước khi build APK.", EditorStyles.miniLabel);
            }
            else
            {
                GUI.color = new Color(1f, 0.85f, 0.2f);
                EditorGUILayout.LabelField("⚠️ ĐANG Ở CHẾ ĐỘ TRUNG GIAN / CHƯA ĐỒNG BỘ CẤU HÌNH!", EditorStyles.boldLabel);
                GUI.color = Color.white;
                EditorGUILayout.LabelField($"• Platform: {EditorUserBuildSettings.activeBuildTarget} | PlayMode: {currentPlayMode}. Hãy bấm 1 trong 2 nút bên dưới để đồng bộ.", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(4);

            // 2. BẢNG THÔNG SỐ KỸ THUẬT CHI TIẾT
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("📊 THÔNG SỐ HỆ THỐNG CHI TIẾT", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);

            // 1. Target Platform
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("• Nền Tảng Đang Chọn:", GUILayout.Width(200));
            GUI.color = isAndroid ? new Color(0.4f, 1f, 0.4f) : new Color(1f, 0.8f, 0.3f);
            EditorGUILayout.LabelField(isAndroid ? "✅ Android" : $"⚠️ {EditorUserBuildSettings.activeBuildTarget} (Chưa phải Android)", EditorStyles.boldLabel);
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();

            // 2. Addressables Play Mode
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("• Addressables PlayMode:", GUILayout.Width(200));
            if (isAssetDatabaseMode)
            {
                GUI.color = new Color(0.3f, 0.85f, 1f);
                EditorGUILayout.LabelField("💻 Use Asset Database (Test Editor Nhanh)", EditorStyles.boldLabel);
            }
            else if (isExistingBuildMode)
            {
                GUI.color = new Color(1f, 0.9f, 0.3f);
                EditorGUILayout.LabelField("📦 Use Existing Build (Dùng Cho APK)", EditorStyles.boldLabel);
            }
            else
            {
                EditorGUILayout.LabelField(currentPlayMode, EditorStyles.boldLabel);
            }
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();

            // 3. Scripting Backend & Architecture
            var backend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android);
            var arch = PlayerSettings.Android.targetArchitectures;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("• Scripting Backend / CPU:", GUILayout.Width(200));
            bool isIL2CPP = backend == ScriptingImplementation.IL2CPP;
            bool hasARM64 = (arch & AndroidArchitecture.ARM64) != 0;
            EditorGUILayout.LabelField($"{(isIL2CPP ? "IL2CPP" : "Mono")} | {(hasARM64 ? "ARM64 (Chuẩn Google Play)" : "ARMv7")}");
            EditorGUILayout.EndHorizontal();

            // 4. Kiểm toán toàn vẹn
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("• Trạng Thái Player Prefabs:", GUILayout.Width(200));
            if (_service.IsAuditing)
            {
                EditorGUILayout.LabelField("⏳ Đang quét kiểm toán...");
            }
            else if (_service.AuditReport != null)
            {
                if (_service.AuditReport.IsHealthy && _service.AuditReport.Warnings.Count == 0)
                {
                    GUI.color = new Color(0.4f, 1f, 0.4f);
                    EditorGUILayout.LabelField("✅ Toàn Vẹn & Khớp 100% với Resources", EditorStyles.boldLabel);
                }
                else if (_service.AuditReport.IsHealthy)
                {
                    GUI.color = new Color(1f, 0.85f, 0.2f);
                    EditorGUILayout.LabelField($"⚠️ Có {_service.AuditReport.Warnings.Count} Cảnh Báo Cần Đồng Bộ", EditorStyles.boldLabel);
                }
                else
                {
                    GUI.color = new Color(1f, 0.35f, 0.35f);
                    EditorGUILayout.LabelField($"❌ Có {_service.AuditReport.Errors.Count} Lỗi Nghiêm Trọng!", EditorStyles.boldLabel);
                }
                GUI.color = Color.white;
            }
            else
            {
                EditorGUILayout.LabelField("Chưa quét kiểm toán.");
            }
            EditorGUILayout.EndHorizontal();

            // 5. Trạng thái Bundles Mới/Cũ
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("• Addressables Content Bundles:", GUILayout.Width(200));
            if (_service.IsBundleOutdated)
            {
                GUI.color = new Color(1f, 0.4f, 0.2f);
                string reason = _service.LastBundleBuildTime == DateTime.MinValue 
                    ? "⚠️ Chưa Build Lần Nào" 
                    : $"⚠️ Có {_service.ModifiedAssetsCount} Asset Vừa Sửa (Lạc Hậu!)";
                EditorGUILayout.LabelField(reason, EditorStyles.boldLabel);
            }
            else
            {
                GUI.color = new Color(0.4f, 1f, 0.4f);
                string buildTimeStr = _service.LastBundleBuildTime != DateTime.MinValue 
                    ? _service.LastBundleBuildTime.ToString("HH:mm dd/MM") 
                    : "Mới";
                EditorGUILayout.LabelField($"✅ Mới Nhất (Đã Build lúc {buildTimeStr})", EditorStyles.boldLabel);
            }
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();

            // 6. Xung đột Bản Sao Trùng Lặp Resources vs Addressables
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("• Xung Đột Resources / Addressables:", GUILayout.Width(200));
            if (_service.ConflictReport != null && _service.ConflictReport.HasConflicts)
            {
                GUI.color = new Color(1f, 0.4f, 0.2f);
                string namesPreview = string.Join(", ", _service.ConflictReport.DuplicateResourcePaths);
                if (namesPreview.Length > 45) namesPreview = namesPreview.Substring(0, 42) + "...";
                string labelText = $"⚠️ Có {_service.ConflictReport.TotalDuplicates} Bản Sao ({namesPreview})";

                EditorGUILayout.LabelField(new GUIContent(labelText, string.Join("\n", _service.ConflictReport.DuplicateResourcePaths)), EditorStyles.boldLabel);
                GUI.color = Color.white;
                GUI.backgroundColor = new Color(1f, 0.8f, 0.3f);
                if (GUILayout.Button("🧹 Dọn Dẹp Bản Sao", GUILayout.Width(130), GUILayout.Height(19)))
                {
                    _service.CleanDuplicateResources();
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUI.color = new Color(0.4f, 1f, 0.4f);
                EditorGUILayout.LabelField("✅ Hoàn Hảo (0 Bản Sao Trùng Lặp)", EditorStyles.boldLabel);
                GUI.color = Color.white;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        public void DrawQuickModeSwitcherSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("⚡ CHUYỂN ĐỔI CHẾ ĐỘ NHANH (1-CLICK TOGGLE)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Chọn một trong hai chế độ dưới đây. Dashboard sẽ tự động thiết lập chính xác các thông số:", MessageType.None);
            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.3f, 0.8f, 1f);
            if (GUILayout.Button("💻 CHUYỂN SANG TEST TRÊN EDITOR\n(Asset Database / Chạy Ngay)", GUILayout.Height(44)))
            {
                _service.SwitchToEditorTestMode();
            }

            GUI.backgroundColor = new Color(0.35f, 1f, 0.5f);
            if (GUILayout.Button("🤖 CHUYỂN SANG SẴN SÀNG BUILD APK\n(Existing Build / IL2CPP)", GUILayout.Height(44)))
            {
                _service.SwitchToAndroidBuildMode();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        public void DrawWorkflowChecklistSection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("📋 QUY TRÌNH ĐÓNG GÓI CÓ KIỂM SOÁT (TỪNG BƯỚC)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Bạn có thể bấm chạy độc lập từng bước hoặc kiểm tra trạng thái trước khi đóng gói APK:", MessageType.None);
            EditorGUILayout.Space(4);

            // Bước 1
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("1. Chuẩn Hóa Nhóm Addressables (Local & DLC Groups)", GUILayout.Width(360));
            if (GUILayout.Button("Kiểm Tra & Chuẩn Hóa", GUILayout.Height(26)))
            {
                ProjectZombie.Editor.AddressablesTools.AddressableGroupsSetupTool.SetupStandardGroups();
                _service.RefreshAll();
            }
            EditorGUILayout.EndHorizontal();

            // Bước 2
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("2. Kiểm Tra Tài Nguyên & Shaders Validator", GUILayout.Width(360));
            if (GUILayout.Button("Kiểm Tra (Validate)", GUILayout.Height(26)))
            {
                _service.ExecuteStep2_ValidateAssets();
            }
            EditorGUILayout.EndHorizontal();

            // Bước 3
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            string step3Label = _service.IsBundleOutdated 
                ? $"3. Đóng Gói Bundles (⚠️ Có {_service.ModifiedAssetsCount} Asset Cần Build Lại!)" 
                : "3. Đóng Gói Addressables Content Bundles (✅ Mới Nhất)";
            EditorGUILayout.LabelField(step3Label, GUILayout.Width(360));
            if (_service.IsBundleOutdated) GUI.backgroundColor = new Color(1f, 0.6f, 0.2f);
            if (GUILayout.Button(_service.IsBundleOutdated ? "⚠️ Build Bundles Ngay" : "Build Bundles", GUILayout.Height(26)))
            {
                _service.ExecuteStep3_BuildAddressables();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            // Bước 4
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField("4. Xuất File Cài Đặt Android APK", GUILayout.Width(360));
            GUI.backgroundColor = new Color(0.4f, 1f, 0.4f);
            if (GUILayout.Button("Bắt Đầu Build APK", GUILayout.Height(26)))
            {
                _service.ExecuteStep4_BuildAPK();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        public void DrawAuditReportSection()
        {
            if (_service.AuditReport == null) return;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("🔍 CHI TIẾT KIỂM TOÁN TÀI NGUYÊN (PREFAB INTEGRITY)", EditorStyles.boldLabel);
            if (GUILayout.Button("Quét Lại", GUILayout.Width(75)))
            {
                _service.RefreshAll();
            }
            if (!_service.AuditReport.IsHealthy || _service.AuditReport.Warnings.Count > 0)
            {
                GUI.backgroundColor = new Color(0.3f, 1f, 0.6f);
                if (GUILayout.Button("Tự Sửa Lỗi (Fix All)", GUILayout.Width(120)))
                {
                    _service.FixAuditIssues();
                }
                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);

            if (_service.AuditReport.Errors.Count > 0)
            {
                foreach (var err in _service.AuditReport.Errors)
                {
                    EditorGUILayout.HelpBox(err, MessageType.Error);
                }
            }

            if (_service.AuditReport.Warnings.Count > 0)
            {
                foreach (var warn in _service.AuditReport.Warnings)
                {
                    EditorGUILayout.HelpBox(warn, MessageType.Warning);
                }
            }

            if (_service.AuditReport.IsHealthy && _service.AuditReport.Warnings.Count == 0)
            {
                EditorGUILayout.HelpBox("Tất cả Player Prefabs đều có đầy đủ CharacterCombat, Network Components và khớp 100% giữa _Prefabs và Resources.", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        public void DrawFooter()
        {
            if (!string.IsNullOrEmpty(_service.LastOperationStatus))
            {
                EditorGUILayout.HelpBox(_service.LastOperationStatus, _service.StatusMessageType);
            }
        }
    }
}
#endif
