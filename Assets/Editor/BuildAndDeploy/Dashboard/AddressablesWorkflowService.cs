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

namespace ProjectZombie.Editor.BuildAndDeploy.Dashboard
{
    /// <summary>
    /// Service chuyên trách toàn bộ logic nghiệp vụ của quy trình Build Android & Addressables.
    /// Độc lập hoàn toàn với giao diện GUI (SRP), hỗ trợ cả Editor GUI lẫn CI/CD Command line.
    /// </summary>
    public class AddressablesWorkflowService
    {
        public PrefabIntegrityAuditor.AuditReport AuditReport { get; private set; }
        public ProjectZombie.EditorTools.AssetSourceConflictDetector.ConflictReport ConflictReport { get; private set; }
        public bool IsAuditing { get; private set; }
        public bool IsBundleOutdated { get; private set; }
        public DateTime LastBundleBuildTime { get; private set; } = DateTime.MinValue;
        public int ModifiedAssetsCount { get; private set; }
        public IReadOnlyList<string> ModifiedAssetNames => _modifiedAssetNames;
        public string LastOperationStatus { get; private set; } = "Sẵn sàng.";
        public MessageType StatusMessageType { get; private set; } = MessageType.Info;

        private readonly List<string> _modifiedAssetNames = new List<string>();

        public void RefreshAll()
        {
            IsAuditing = true;
            try
            {
                AuditReport = PrefabIntegrityAuditor.AuditPlayerPrefabs();
                ConflictReport = ProjectZombie.EditorTools.AssetSourceConflictDetector.ScanConflicts();
                CheckBundleOutdatedState();
            }
            finally
            {
                IsAuditing = false;
            }
        }

        public void CheckBundleOutdatedState()
        {
            _modifiedAssetNames.Clear();
            ModifiedAssetsCount = 0;
            IsBundleOutdated = false;
            LastBundleBuildTime = DateTime.MinValue;

            string catalogPath = "ServerData/Android/catalog.json";
            if (!File.Exists(catalogPath) && Directory.Exists("ServerData"))
            {
                var files = Directory.GetFiles("ServerData", "*catalog*.json", SearchOption.AllDirectories);
                if (files.Length > 0) catalogPath = files[0];
            }

            if (File.Exists(catalogPath))
            {
                LastBundleBuildTime = File.GetLastWriteTime(catalogPath);
            }

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) return;

            if (LastBundleBuildTime == DateTime.MinValue)
            {
                IsBundleOutdated = true;
                ModifiedAssetsCount = 1;
                _modifiedAssetNames.Add("Chưa từng đóng gói Addressables Content Bundles!");
                return;
            }

            foreach (var group in settings.groups)
            {
                if (group == null || group.ReadOnly) continue;

                foreach (var entry in group.entries)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.AssetPath)) continue;

                    string fullPath = entry.AssetPath;
                    if (File.Exists(fullPath))
                    {
                        DateTime assetTime = File.GetLastWriteTime(fullPath);
                        if (assetTime > LastBundleBuildTime)
                        {
                            ModifiedAssetsCount++;
                            if (_modifiedAssetNames.Count < 5)
                            {
                                _modifiedAssetNames.Add(Path.GetFileName(fullPath));
                            }
                        }
                    }
                }
            }

            IsBundleOutdated = ModifiedAssetsCount > 0;
        }

        public void SwitchToEditorTestMode()
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

            SetStatus("Đã chuyển Addressables sang 'Use Asset Database (fastest)'. Bấm PLAY trên Editor để test ngay!", MessageType.Info);
            RefreshAll();
        }

        public void SwitchToAndroidBuildMode()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }

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

            SetStatus("Đã chuyển cấu hình sang Android & 'Use Existing Build'. Sẵn sàng đóng gói APK!", MessageType.Info);
            RefreshAll();
        }

        public void ExecuteStep1_SyncResources()
        {
            ResourceSyncEngine.PerformFullSync();
            RefreshAll();
            SetStatus("Đã hoàn tất đồng bộ toàn bộ tài nguyên Master sang Resources.", MessageType.Info);
        }

        public void ExecuteStep2_ValidateAssets()
        {
            bool passed = AndroidBuildAssetValidator.RunFullValidation(isBuildPipeline: false);
            SetStatus(passed ? "Kiểm tra hoàn tất: Tài nguyên hợp lệ 100%!" : "Phát hiện vấn đề! Xem chi tiết trong cửa sổ Console.", passed ? MessageType.Info : MessageType.Error);
        }

        public void ExecuteStep3_BuildAddressables()
        {
            AddressableGroupsSetupTool.BuildAddressablesBundles();
            RefreshAll();
            SetStatus("Đã hoàn tất đóng gói Addressables Content Bundles vào ServerData/Android/.", MessageType.Info);
        }

        public void ExecuteStep4_BuildAPK()
        {
            CheckBundleOutdatedState();
            if (IsBundleOutdated)
            {
                string modifiedList = string.Join("\n • ", _modifiedAssetNames);
                int choice = EditorUtility.DisplayDialogComplex(
                    "⚠️ CẢNH BÁO: TÀI NGUYÊN ĐÃ BỊ THAY ĐỔI!",
                    $"Phát hiện có {ModifiedAssetsCount} tài nguyên đã chỉnh sửa nhưng CHƯA ĐƯỢC ĐÓNG GÓI LẠI VÀO BUNDLES:\n\n • {modifiedList}\n\nNếu tiếp tục Build APK ngay, bản cài đặt Android sẽ CHỨA DỮ LIỆU CŨ!\n\nBạn có muốn tự động Build Bundles trước rồi mới tiếp tục Build APK không?",
                    "1. Tự Động Build Lại Bundles Rồi Build APK (Khuyên dùng)",
                    "2. Hủy Bỏ Build APK",
                    "3. Vẫn Tiếp Tục Bỏ Qua Cảnh Báo"
                );

                if (choice == 1)
                {
                    SetStatus("Đã hủy Build APK để chuẩn bị lại tài nguyên.", MessageType.Warning);
                    return;
                }
                else if (choice == 0)
                {
                    Debug.Log("<color=#FFAA00>[Dashboard]</color> Tự động đóng gói lại Addressables Content Bundles trước khi Build APK...");
                    ExecuteStep3_BuildAddressables();
                }
            }

            string exportPath = EditorUtility.SaveFilePanel("Chọn Nơi Lưu File APK", "Builds/Android", $"ProjectZombie_{DateTime.Now:yyyyMMdd_HHmm}.apk", "apk");
            if (string.IsNullOrEmpty(exportPath)) return;

            string buildDir = Path.GetDirectoryName(exportPath);
            if (!Directory.Exists(buildDir)) Directory.CreateDirectory(buildDir);

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
                SetStatus($"Build APK THÀNH CÔNG! File tại: {exportPath}", MessageType.Info);

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
                SetStatus($"Build APK THẤT BẠI: {report.summary.result}. Xem log Console để biết chi tiết.", MessageType.Error);
            }
        }

        public void FixAuditIssues()
        {
            PrefabIntegrityAuditor.FixAllIssues();
            RefreshAll();
            SetStatus("Đã sửa thành công và đồng bộ toàn bộ Player Prefabs!", MessageType.Info);
        }

        public void CleanDuplicateResources()
        {
            ProjectZombie.EditorTools.AssetSourceConflictDetector.CleanDuplicateResources();
            RefreshAll();
            SetStatus("Đã hoàn tất rà soát & dọn dẹp các tài nguyên trùng lặp giữa Resources và Addressables.", MessageType.Info);
        }

        private void SetStatus(string message, MessageType type)
        {
            LastOperationStatus = message;
            StatusMessageType = type;
        }
    }
}
#endif
