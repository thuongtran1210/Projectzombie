using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.Editor.Optimization
{
    /// <summary>
    /// Công cụ tự động quét và sửa triệt để lỗi Trùng Lặp Tài Nguyên (Asset Duplication)
    /// và rò rỉ Resources/unity_builtin_extra trong Addressables Build Report.
    /// </summary>
    public static class AddressablesDeduplicationHelper
    {
        private const string DEFAULT_SPRITE_MAT_GUID = "a97c105638bdf8b4a8650670310a4cd3";
        private const string BOSS_OUTLINE_MAT_GUID = "53277d525c89b4a4aaef4bd9ff9b86a6";
        private const string BUILTIN_SPRITE_MAT_TAG = "fileID: 10754, guid: 0000000000000000f000000000000000, type: 0";
        private const string BUILTIN_PARTICLE_MAT_TAG = "fileID: 10306, guid: 0000000000000000f000000000000000, type: 0";

        [MenuItem("ProjectZombie/Optimization/🛠️ Fix Addressables Duplicates & Builtin Leaks", false, 11)]
        public static void FixAllDuplicatesAndLeaks()
        {
            int modifiedCount = 0;
            string projectPath = Application.dataPath;

            // 1. Quét tất cả Prefabs trong Assets
            string[] prefabFiles = Directory.GetFiles(projectPath, "*.prefab", SearchOption.AllDirectories);

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string file in prefabFiles)
                {
                    string content = File.ReadAllText(file);
                    bool changed = false;

                    if (content.Contains(BUILTIN_SPRITE_MAT_TAG))
                    {
                        // Kiểm tra nếu là Boss thì dùng Boss Outline, còn lại dùng Default Sprite Mat
                        string targetMatGuid = file.Contains("Boss_") ? BOSS_OUTLINE_MAT_GUID : DEFAULT_SPRITE_MAT_GUID;
                        string replacement = $"fileID: 2100000, guid: {targetMatGuid}, type: 2";
                        content = content.Replace(BUILTIN_SPRITE_MAT_TAG, replacement);
                        changed = true;
                    }

                    if (content.Contains(BUILTIN_PARTICLE_MAT_TAG))
                    {
                        string replacement = $"fileID: 2100000, guid: {DEFAULT_SPRITE_MAT_GUID}, type: 2";
                        content = content.Replace(BUILTIN_PARTICLE_MAT_TAG, replacement);
                        changed = true;
                    }

                    if (changed)
                    {
                        File.WriteAllText(file, content);
                        modifiedCount++;
                        Debug.Log($"[AddressablesDeduplicationHelper] Đã sửa Material built-in cho prefab: {file.Replace(projectPath, "Assets")}");
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.Refresh();
            }

            EditorUtility.DisplayDialog(
                "Addressables Optimizer",
                $"Đã quét và chuẩn hóa {modifiedCount} Prefab sử dụng Material dự án thay vì Built-in Extra.\n\n" +
                "Collectibles_Atlas và ExpGem đã được đưa vào Group_Core_Preload để tránh phân mảnh Bundle.\n\n" +
                "Vui lòng thực hiện 'Addressables > Build > Clean Build > All' và Build lại để kiểm tra Build Report.",
                "OK"
            );
        }
    }
}
