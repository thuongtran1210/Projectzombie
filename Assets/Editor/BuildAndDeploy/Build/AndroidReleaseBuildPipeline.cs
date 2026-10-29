#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using ProjectZombie.EditorTools.BuildSync;
using ProjectZombie.Editor.VFX;

namespace ProjectZombie.Editor.Build
{
    /// <summary>
    /// Pipeline tự động hóa Build Android (APK / AAB) chuẩn hóa cho dự án Projectzombie.
    /// Tự động kiểm tra PlayerSettings, đồng bộ tài nguyên, và xuất gói cài đặt 1-Click.
    /// </summary>
    public static class AndroidReleaseBuildPipeline
    {
        private const string BUILD_OUTPUT_FOLDER = "Builds/Android";

        [MenuItem("ProjectZombie/4. 🤖 Android Build & Deploy/1. Kiểm Tra & Tối Ưu Cấu Hình Android (PlayerSettings)", false, 301)]
        public static void ValidateAndConfigureAndroidSettings()
        {
            Debug.Log("<color=#00FF88>[AndroidPipeline]</color> Bắt đầu kiểm tra và chuẩn hóa cấu hình PlayerSettings cho Android...");

            // 1. Scripting Backend: Bắt buộc IL2CPP
            var currentBackend = PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android);
            if (currentBackend != ScriptingImplementation.IL2CPP)
            {
                PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
                Debug.Log("<color=#00FF88>[AndroidPipeline]</color> Đã chuyển Scripting Backend sang IL2CPP.");
            }

            // 2. Kiến trúc CPU: Bắt buộc ARM64 (chuẩn Google Play) kèm ARMv7
            var targetArch = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
            if (PlayerSettings.Android.targetArchitectures != targetArch)
            {
                PlayerSettings.Android.targetArchitectures = targetArch;
                Debug.Log("<color=#00FF88>[AndroidPipeline]</color> Đã cấu hình Target Architectures: ARM64 + ARMv7.");
            }

            // 3. Target SDK: Chuẩn Google Play tối thiểu API 34 (Android 14)
            if (PlayerSettings.Android.targetSdkVersion < (AndroidSdkVersions)34)
            {
                PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)34;
                Debug.Log("<color=#00FF88>[AndroidPipeline]</color> Đã nâng Target SDK lên API 34 (Android 14).");
            }

            // 4. Min SDK: API 24 (Android 7.0 Nougat) để tương thích 98% thiết bị thị trường
            if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel24)
            {
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
                Debug.Log("<color=#00FF88>[AndroidPipeline]</color> Đã đặt Min SDK = API 24.");
            }

            // 5. Đảm bảo Incremental IL2CPP Build được bật để tăng tốc độ biên dịch
            PlayerSettings.SetIncrementalIl2CppBuild(NamedBuildTarget.Android, true);

            // 6. Đảm bảo cảnh quan ngang (Landscape)
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

            // 7. Đồng bộ Resources & Shaders
            ResourceSyncEngine.PerformFullSync();
            AlwaysIncludedShadersSetupTool.AddRequiredShaders();

            EditorUtility.DisplayDialog("Cấu Hình Android Thành Công",
                "Đã chuẩn hóa toàn bộ thiết lập Android:\n\n" +
                "• Scripting Backend: IL2CPP\n" +
                "• Kiến trúc: ARM64 + ARMv7 (Google Play Ready)\n" +
                "• Target SDK: API 34 (Android 14)\n" +
                "• Min SDK: API 24 (Android 7.0)\n" +
                "• Định hướng: Landscape Left & Right\n" +
                "• Đã đồng bộ tài nguyên và Shaders.",
                "Đồng Ý");
        }

        [MenuItem("ProjectZombie/4. 🤖 Android Build & Deploy/2. Xuất APK Test (Development Build)", false, 302)]
        public static void BuildDevelopmentAPK()
        {
            BuildAndroidInternal(isReleaseAab: false, isDevelopment: true);
        }

        [MenuItem("ProjectZombie/4. 🤖 Android Build & Deploy/4. Xuất Google Play App Bundle (.aab)", false, 305)]
        public static void BuildReleaseAAB()
        {
            BuildAndroidInternal(isReleaseAab: true, isDevelopment: false);
        }

        private static void BuildAndroidInternal(bool isReleaseAab, bool isDevelopment)
        {
            // Kiểm tra và chuẩn hóa cấu hình trước khi build
            ValidateAndConfigureAndroidSettings();

            if (!Directory.Exists(BUILD_OUTPUT_FOLDER))
            {
                Directory.CreateDirectory(BUILD_OUTPUT_FOLDER);
            }

            string extension = isReleaseAab ? "aab" : "apk";
            string modeName = isDevelopment ? "Dev" : "Release";
            string defaultFileName = $"ProjectZombie_{modeName}_{DateTime.Now:yyyyMMdd_HHmm}.{extension}";
            string exportPath = Path.Combine(BUILD_OUTPUT_FOLDER, defaultFileName);

            // Thu thập các Scene hợp lệ
            var scenesList = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled && !string.IsNullOrEmpty(scene.path) && File.Exists(scene.path))
                {
                    scenesList.Add(scene.path);
                }
            }

            if (scenesList.Count == 0)
            {
                scenesList.Add("Assets/Scenes/MainMenu.unity");
                scenesList.Add("Assets/Scenes/Defense3D/MapSongQue3D_Prototype.unity");
            }

            EditorUserBuildSettings.buildAppBundle = isReleaseAab;

            BuildOptions options = BuildOptions.None;
            if (isDevelopment)
            {
                options |= BuildOptions.Development | BuildOptions.AllowDebugging;
            }

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = scenesList.ToArray(),
                locationPathName = exportPath,
                target = BuildTarget.Android,
                options = options
            };

            Debug.Log($"<color=#00E5FF>[AndroidPipeline]</color> Bắt đầu xuất {extension.ToUpper()} ({modeName}) tới: {exportPath}...");
            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

            if (report.summary.result == BuildResult.Succeeded)
            {
                long sizeMB = (long)(report.summary.totalSize / (1024 * 1024));
                Debug.Log($"<color=#00FF88>[AndroidPipeline]</color> Build {extension.ToUpper()} THÀNH CÔNG! Dung lượng: {sizeMB} MB tại: {exportPath}");
                EditorUtility.RevealInFinder(exportPath);
                EditorUtility.DisplayDialog("Build Thành Công",
                    $"Đã xuất file {extension.ToUpper()} thành công!\n\n" +
                    $"• Đường dẫn: {exportPath}\n" +
                    $"• Dung lượng: {sizeMB} MB\n" +
                    $"• Tổng số Scene: {scenesList.Count}",
                    "Tuyệt Vời");
            }
            else
            {
                Debug.LogError($"<color=#FF4444>[AndroidPipeline]</color> Build {extension.ToUpper()} THẤT BẠI: {report.summary.result}. Tổng số lỗi: {report.summary.totalErrors}");
                EditorUtility.DisplayDialog("Build Thất Bại",
                    $"Quá trình build gặp lỗi ({report.summary.totalErrors} errors).\nVui lòng kiểm tra Console log để xem chi tiết.",
                    "Đóng");
            }
        }
    }
}
#endif
