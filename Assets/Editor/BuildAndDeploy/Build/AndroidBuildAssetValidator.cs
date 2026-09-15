#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ProjectZombie.Editor.Build
{
    /// <summary>
    /// Bộ công cụ kiểm tra tự động trước khi đóng gói APK Android.
    /// Kế thừa IPreprocessBuildWithReport để ngăn chặn Build nếu thiếu tài nguyên hoặc vi phạm quy chuẩn nạp Asset.
    /// </summary>
    public class AndroidBuildAssetValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        [MenuItem("ProjectZombie/4. 🤖 Android Build & Deploy/3. Pre-Build Asset Validator", false, 304)]
        public static bool ValidateAllAssetsMenu()
        {
            return RunFullValidation(isBuildPipeline: false);
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.Android)
            {
                Debug.Log("<color=#00FF88>[AndroidBuildAssetValidator]</color> Tự động đồng bộ toàn bộ Resources (UI/Upgrades/Audios/Weapons) trước khi Build Android...");
                ProjectZombie.EditorTools.BuildSync.ResourceSyncEngine.PerformFullSync();

                Debug.Log("<color=#00FF88>[AndroidBuildAssetValidator]</color> Tự động kiểm tra & bổ sung URP VFX Shaders vào Graphics Settings...");
                ProjectZombie.Editor.VFX.AlwaysIncludedShadersSetupTool.AddRequiredShaders();

                Debug.Log("<color=#00FF88>[AndroidBuildAssetValidator]</color> Bắt đầu kiểm tra tính hợp lệ của Asset trước khi Build Android...");
                bool passed = RunFullValidation(isBuildPipeline: true);
                if (!passed)
                {
                    throw new BuildFailedException("[AndroidBuildAssetValidator] Phát hiện lỗi liên quan đến Addressables hoặc AssetDatabase Runtime. Build Android bị chặn.");
                }
            }
        }

        public static bool RunFullValidation(bool isBuildPipeline)
        {
            List<string> errors = new List<string>();
            List<string> warnings = new List<string>();

            // 1. Kiểm tra AddressableAssetSettings
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                errors.Add("❌ [Addressables] Không tìm thấy AddressableAssetSettingsDefaultObject trong dự án!");
            }
            else
            {
                var addressSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var exactAddressSet = new HashSet<string>();
                int checkedEntries = 0;

                foreach (AddressableAssetGroup group in settings.groups)
                {
                    if (group == null) continue;
                    foreach (AddressableAssetEntry entry in group.entries)
                    {
                        if (entry == null) continue;
                        checkedEntries++;

                        if (string.IsNullOrEmpty(entry.address))
                        {
                            errors.Add($"❌ [Addressables] Entry tại đường dẫn '{entry.AssetPath}' có Address bị null/rỗng trong Group '{group.Name}'.");
                            continue;
                        }

                        // Kiểm tra trùng lặp address (Case-insensitive check cho Android)
                        if (addressSet.Contains(entry.address))
                        {
                            if (!exactAddressSet.Contains(entry.address))
                            {
                                errors.Add($"❌ [Addressables] Địa chỉ '{entry.address}' trùng lặp nhưng khác hoa/thường! Android filesystem sẽ bị lỗi.");
                            }
                            else
                            {
                                warnings.Add($"⚠️ [Addressables] Địa chỉ '{entry.address}' bị gán trùng lặp cho nhiều asset.");
                            }
                        }
                        else
                        {
                            addressSet.Add(entry.address);
                            exactAddressSet.Add(entry.address);
                        }

                        // Kiểm tra file target có tồn tại không (bỏ qua các system built-in entries như EditorSceneList và Resources)
                        if (entry.TargetAsset == null && !entry.IsFolder && !IsSystemBuiltInEntry(entry))
                        {
                            errors.Add($"❌ [Addressables] Address '{entry.address}' chỉ tới file bị mất/gãy (Missing Asset) tại '{entry.AssetPath}'.");
                        }
                    }
                }
                Debug.Log($"✅ [Addressables Audit] Đã kiểm tra {checkedEntries} Addressable Entries thành công.");
            }

            // 2. Scan C# Runtime Code về AssetDatabase không cô lập
            AuditRuntimeAssetDatabaseUsages(errors);

            // 3. Tổng hợp báo cáo
            Debug.Log("=================================================================");
            if (errors.Count > 0)
            {
                string combinedReport = $"❌ [AndroidBuildAssetValidator] KIỂM TRA THẤT BẠI: Phát hiện {errors.Count} LỖI nghiêm trọng và {warnings.Count} CẢNH BÁO.\n\n" +
                                        "--- CHI TIẾT LỖI (DETAILS) ---\n" +
                                        string.Join("\n", errors);

                if (warnings.Count > 0)
                {
                    combinedReport += "\n\n--- CẢNH BÁO (WARNINGS) ---\n" + string.Join("\n", warnings);
                }

                Debug.LogError(combinedReport);
                return false;
            }
            else
            {
                string successMsg = $"<color=#00FF88>✅ [AndroidBuildAssetValidator] KIỂM TRA THÀNH CÔNG! Đạt tiêu chuẩn đóng gói Android APK ({warnings.Count} cảnh báo).</color>";
                if (warnings.Count > 0)
                {
                    successMsg += "\n" + string.Join("\n", warnings);
                }
                Debug.Log(successMsg);
                return true;
            }
        }

        private static void AuditRuntimeAssetDatabaseUsages(List<string> errors)
        {
            string assetsFolder = Application.dataPath;
            string[] csFiles = Directory.GetFiles(assetsFolder, "*.cs", SearchOption.AllDirectories);

            foreach (string filePath in csFiles)
            {
                string normalizedPath = filePath.Replace("\\", "/");

                // Bỏ qua các tệp nằm trong thư mục Editor hoặc ThirdParty (DOTween, Plugins...)
                if (normalizedPath.Contains("/Editor/") || normalizedPath.Contains("/Plugins/"))
                {
                    continue;
                }

                string content = File.ReadAllText(filePath);

                if (content.Contains("AssetDatabase."))
                {
                    string relativePath = "Assets" + normalizedPath.Substring(assetsFolder.Length);
                    ScanFileForUnprotectedAssetDatabase(content, relativePath, errors);
                }
            }
        }

        private class PreprocessorFrame
        {
            public bool IsEditorBranch;
        }

        private static void ScanFileForUnprotectedAssetDatabase(string content, string relativePath, List<string> errors)
        {
            string[] lines = content.Split('\n');
            var stack = new Stack<PreprocessorFrame>();

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                if (line.StartsWith("#if ") || line.StartsWith("#if\t"))
                {
                    bool isEditor = IsEditorCondition(line);
                    bool parentIsEditor = stack.Count > 0 && stack.Peek().IsEditorBranch;
                    stack.Push(new PreprocessorFrame { IsEditorBranch = isEditor || parentIsEditor });
                }
                else if (line.StartsWith("#elif ") || line.StartsWith("#elif\t"))
                {
                    if (stack.Count > 0) stack.Pop();
                    bool isEditor = IsEditorCondition(line);
                    bool parentIsEditor = stack.Count > 0 && stack.Peek().IsEditorBranch;
                    stack.Push(new PreprocessorFrame { IsEditorBranch = isEditor || parentIsEditor });
                }
                else if (line.StartsWith("#else"))
                {
                    if (stack.Count > 0)
                    {
                        var top = stack.Pop();
                        bool parentIsEditor = stack.Count > 0 && stack.Peek().IsEditorBranch;
                        // Nếu #if là !UNITY_EDITOR thì #else chính là UNITY_EDITOR branch
                        bool isElseEditor = (!top.IsEditorBranch) || parentIsEditor;
                        stack.Push(new PreprocessorFrame { IsEditorBranch = isElseEditor });
                    }
                }
                else if (line.StartsWith("#endif"))
                {
                    if (stack.Count > 0) stack.Pop();
                }

                // Kiểm tra cuộc gọi AssetDatabase
                if (line.Contains("AssetDatabase.") && !line.StartsWith("//") && !line.StartsWith("/*"))
                {
                    bool isProtected = stack.Count > 0 && stack.Peek().IsEditorBranch;
                    if (!isProtected)
                    {
                        errors.Add($"❌ [Code Audit] File Runtime [{relativePath} (Dòng {i + 1})] gọi direct 'AssetDatabase' mà không được bọc bởi #if UNITY_EDITOR!");
                    }
                }
            }
        }

        private static bool IsSystemBuiltInEntry(AddressableAssetEntry entry)
        {
            if (entry == null) return true;
            if (entry.parentGroup != null && entry.parentGroup.Name.IndexOf("Built In", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (entry.AssetPath == "Scenes In Build" || entry.AssetPath.Contains("*/Resources/") || entry.address == "EditorSceneList" || entry.address == "Resources") return true;
            return false;
        }

        private static bool IsEditorCondition(string line)
        {
            // Kiểm tra xem directive có phải UNITY_EDITOR (và không phải !UNITY_EDITOR)
            if (!line.Contains("UNITY_EDITOR")) return false;
            
            // Xử lý !UNITY_EDITOR
            int idx = line.IndexOf("UNITY_EDITOR");
            if (idx > 0 && line[idx - 1] == '!')
            {
                return false;
            }
            return true;
        }
    }
}
#endif
