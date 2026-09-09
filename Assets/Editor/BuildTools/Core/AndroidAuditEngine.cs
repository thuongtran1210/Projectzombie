using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.EditorTools.BuildSync
{
    public static class AndroidAuditEngine
    {
        public static List<AuditItem> RunAudit()
        {
            AssetDatabase.Refresh();
            var issues = new List<AuditItem>();

            // 1. Kiểm tra Scripting Backend
            var backend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android);
            if (backend != ScriptingImplementation.IL2CPP)
            {
                issues.Add(new AuditItem
                {
                    Severity = AuditItem.SeverityLevel.Error,
                    Title = "Scripting Backend chưa đặt là IL2CPP",
                    Description = $"Hiện đang dùng {backend}. Google Play và Android 64-bit hiện đại bắt buộc IL2CPP.",
                    Recommendation = "Chuyển Scripting Backend sang IL2CPP.",
                    ActionType = AuditItem.FixActionType.OptimizePlayerSettings,
                    FixButtonText = "⚙️ Cấu Hình IL2CPP Ngay"
                });
            }

            // 2. Kiểm tra ARM64
            var targetArch = PlayerSettings.Android.targetArchitectures;
            if ((targetArch & AndroidArchitecture.ARM64) == 0)
            {
                issues.Add(new AuditItem
                {
                    Severity = AuditItem.SeverityLevel.Error,
                    Title = "Chưa kích hoạt kiến trúc ARM64",
                    Description = "Target Architecture chưa có ARM64, game sẽ không chạy được trên phần lớn máy Android đời mới.",
                    Recommendation = "Kích hoạt ARM64 trong Player Settings.",
                    ActionType = AuditItem.FixActionType.OptimizePlayerSettings,
                    FixButtonText = "⚙️ Kích Hoạt ARM64 Ngay"
                });
            }

            // 3. Kiểm tra Target API Level
            var targetApi = PlayerSettings.Android.targetSdkVersion;
            if (targetApi != AndroidSdkVersions.AndroidApiLevelAuto && (int)targetApi < 34)
            {
                issues.Add(new AuditItem
                {
                    Severity = AuditItem.SeverityLevel.Warning,
                    Title = "Target API Level thấp hơn 34 (Android 14)",
                    Description = $"Hiện tại Target API Level là: {targetApi}. Google Play yêu cầu tối thiểu API Level 34.",
                    Recommendation = "Đặt Target API Level thành 'Automatic (highest installed)' hoặc 34.",
                    ActionType = AuditItem.FixActionType.OptimizePlayerSettings,
                    FixButtonText = "⚙️ Cập Nhật API Level 34"
                });
            }

            // 4. Quét đồng bộ các thư mục
            foreach (var rule in SyncRegistry.DirectoryRules)
            {
                AuditDirectory(rule, issues);
            }

            // 5. Quét Single Assets
            foreach (var rule in SyncRegistry.SingleAssetRules)
            {
                AuditSingleAsset(rule, issues);
            }

            // 6. Quét UI Prefabs
            foreach (var rule in SyncRegistry.UIPrefabRules)
            {
                AuditUIPrefab(rule, issues);
            }

            return issues;
        }

        private static void AuditDirectory(SyncRule rule, List<AuditItem> issues)
        {
            if (!Directory.Exists(rule.SourcePath)) return;
            string[] srcFiles = Directory.GetFiles(rule.SourcePath, rule.SearchPattern ?? "*.*", SearchOption.AllDirectories);
            int srcCount = 0;
            foreach (var f in srcFiles)
            {
                if (f.EndsWith(".meta")) continue;
                if (rule.AllowedExtensions != null)
                {
                    string ext = Path.GetExtension(f).ToLower();
                    bool allowed = false;
                    foreach (var ve in rule.AllowedExtensions)
                    {
                        if (ext == ve) { allowed = true; break; }
                    }
                    if (!allowed) continue;
                }
                srcCount++;
            }

            int resCount = 0;
            if (Directory.Exists(rule.TargetPath))
            {
                string[] resFiles = Directory.GetFiles(rule.TargetPath, rule.SearchPattern ?? "*.*", SearchOption.AllDirectories);
                foreach (var f in resFiles)
                {
                    if (f.EndsWith(".meta")) continue;
                    if (rule.AllowedExtensions != null)
                    {
                        string ext = Path.GetExtension(f).ToLower();
                        bool allowed = false;
                        foreach (var ve in rule.AllowedExtensions)
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
                issues.Add(new AuditItem
                {
                    Severity = AuditItem.SeverityLevel.Warning,
                    Title = $"Chưa đồng bộ đầy đủ {rule.Name}",
                    Description = $"Thư mục gốc có {srcCount} files nhưng Resources chỉ có {resCount} files.",
                    Recommendation = $"Nhấn nút bên dưới để đồng bộ riêng {rule.Name} vào Resources.",
                    ActionType = AuditItem.FixActionType.SyncDirectory,
                    Rule = rule,
                    FixButtonText = $"⚡ Đồng Bộ Riêng {rule.Name}"
                });
            }
        }

        private static void AuditSingleAsset(SyncRule rule, List<AuditItem> issues)
        {
            bool hasSrc = File.Exists(rule.SourcePath);
            bool hasTarget = File.Exists(rule.TargetPath);

            if (!hasSrc && !hasTarget)
            {
                issues.Add(new AuditItem
                {
                    Severity = AuditItem.SeverityLevel.Warning,
                    Title = $"Thiếu Asset: {rule.Name}",
                    Description = $"Không tìm thấy asset tại {rule.SourcePath} hoặc {rule.TargetPath}.",
                    Recommendation = "Kiểm tra lại asset trong project.",
                    ActionType = AuditItem.FixActionType.None
                });
            }
            else if (hasSrc && !hasTarget)
            {
                issues.Add(new AuditItem
                {
                    Severity = AuditItem.SeverityLevel.Warning,
                    Title = $"Chưa đồng bộ Runtime: {rule.Name}",
                    Description = $"Asset đã có tại nguồn ({rule.SourcePath}) nhưng chưa được copy vào Resources/.",
                    Recommendation = $"Nhấn nút bên dưới để copy riêng {rule.Name} vào Resources/.",
                    ActionType = AuditItem.FixActionType.SyncSingleAsset,
                    Rule = rule,
                    FixButtonText = $"⚡ Đồng Bộ {rule.Name}"
                });
            }
        }

        private static void AuditUIPrefab(SyncRule rule, List<AuditItem> issues)
        {
            bool hasMaster = File.Exists(rule.SourcePath);
            bool hasRes = File.Exists(rule.TargetPath);

            if (!hasMaster && !hasRes)
            {
                issues.Add(new AuditItem
                {
                    Severity = AuditItem.SeverityLevel.Warning,
                    Title = $"Thiếu Prefab UI: {rule.Name}",
                    Description = $"Không tìm thấy {rule.Name}.prefab trong Assets/_Prefabs/UI hoặc Resources/UI.",
                    Recommendation = "Nhấn nút bên dưới để tự động tạo mới Prefab UI này.",
                    ActionType = AuditItem.FixActionType.SyncUIPrefab,
                    Rule = rule,
                    FixButtonText = $"🛠️ Sinh Prefab {rule.Name}"
                });
            }
            else if (hasMaster && !hasRes)
            {
                issues.Add(new AuditItem
                {
                    Severity = AuditItem.SeverityLevel.Warning,
                    Title = $"Chưa đồng bộ Runtime UI: {rule.Name}",
                    Description = $"Prefab đã có tại Assets/_Prefabs/UI/{rule.Name}.prefab nhưng chưa được đồng bộ vào Resources/UI/.",
                    Recommendation = $"Nhấn nút bên dưới để copy {rule.Name} vào Resources/UI/.",
                    ActionType = AuditItem.FixActionType.SyncUIPrefab,
                    Rule = rule,
                    FixButtonText = $"⚡ Đồng Bộ {rule.Name}"
                });
            }
        }

        public static void OptimizePlayerSettings()
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.colorSpace = ColorSpace.Linear;

            Debug.Log("<color=#00FF88>[AndroidAuditEngine] Đã tự động cấu hình Player Settings chuẩn Android (IL2CPP, ARM64+ARMv7, Auto API, Linear)!</color>");
            EditorUtility.DisplayDialog("Cấu Hình Thành Công", "Đã cập nhật cấu hình Android Player Settings:\n- Scripting Backend: IL2CPP\n- Target Architectures: ARM64 + ARMv7\n- Color Space: Linear", "OK");
        }
    }
}
