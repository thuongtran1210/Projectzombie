#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace ProjectZombie.Editor.UI
{
    /// <summary>
    /// Tool tự động sao lưu (Backup) toàn bộ ảnh UI hiện tại và khởi tạo/cấu hình Sprite Atlases theo nhóm chức năng.
    /// Chuẩn hóa cho Unity 2D URP, tối ưu Draw Call và quản lý bộ nhớ.
    /// </summary>
    public class SpriteAtlasAndBackupTool : EditorWindow
    {
        private const string UI_ROOT_FOLDER = "Assets/Art/UI";
        private const string BACKUP_ROOT_FOLDER = "UI_Backups"; // Lưu ngoài thư mục Assets để Unity không import và không bị GUID conflict
        private const string ATLAS_TARGET_FOLDER = "Assets/Art/UI/Atlases";

        private Vector2 scrollPos;
        private bool includeSubFoldersInBackup = true;
        private bool overwriteExistingAtlases = true;
        private int atlasPadding = 4;
        private bool enableTightPacking = false; // Luôn false cho UI 9-slice để không bị lỗi stretch viền

        [MenuItem("Tools/ProjectZombie/UI/Sprite Atlas & Backup Manager", priority = 1)]
        [MenuItem("ProjectZombie/UI/Sprite Atlas & Backup Manager", priority = 1)]
        [MenuItem("Tools/Project Zombie/UI/Sprite Atlas & Backup Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<SpriteAtlasAndBackupTool>("UI Atlas & Backup");
            window.minSize = new Vector2(500, 480);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("📦 UI SPRITE ATLAS & BACKUP MANAGER", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Tool này thực hiện 2 chức năng chính:\n" +
                "1. Tự động sao lưu (Backup) toàn bộ ảnh UI hiện tại vào thư mục có Timestamp (an toàn 100%).\n" +
                "2. Tự động tạo và cấu hình 4 nhóm Sprite Atlas tối ưu Draw Call theo chuẩn URP UI.",
                MessageType.Info);

            EditorGUILayout.Space(10);
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // --- SECTION 1: BACKUP SETTINGS ---
            EditorGUILayout.LabelField("1. Cấu Hình Sao Lưu (Backup)", EditorStyles.boldLabel);
            EditorGUILayout.TextField("Thư Mục UI Gốc:", UI_ROOT_FOLDER);
            EditorGUILayout.TextField("Thư Mục Backup:", BACKUP_ROOT_FOLDER);
            includeSubFoldersInBackup = EditorGUILayout.Toggle("Bao gồm thư mục con:", includeSubFoldersInBackup);

            EditorGUILayout.Space(5);
            if (GUILayout.Button("🛡️ BẬT SAO LƯU UI NGAY (Backup All UI)", GUILayout.Height(35)))
            {
                ExecuteBackup();
            }

            EditorGUILayout.Space(15);

            // --- SECTION 2: SPRITE ATLAS GENERATION ---
            EditorGUILayout.LabelField("2. Cấu Hình Sinh Sprite Atlas", EditorStyles.boldLabel);
            EditorGUILayout.TextField("Thư Mục Lưu Atlas:", ATLAS_TARGET_FOLDER);
            atlasPadding = EditorGUILayout.IntSlider("Atlas Padding (px):", atlasPadding, 2, 16);
            enableTightPacking = EditorGUILayout.Toggle(new GUIContent("Tight Packing (Khuyên tắt cho 9-Slice):", "Tắt Tight Packing để tránh lỗi xé hình viền UI 9-slice"), enableTightPacking);
            overwriteExistingAtlases = EditorGUILayout.Toggle("Ghi đè Atlas nếu đã tồn tại:", overwriteExistingAtlases);

            EditorGUILayout.Space(5);
            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.4f);
            if (GUILayout.Button("⚡ TẠO & PACK 4 NHÓM SPRITE ATLAS", GUILayout.Height(40)))
            {
                FixSourceTexturesFormat();
                ExecuteGenerateSpriteAtlases();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(5);
            if (GUILayout.Button("🔧 Chuẩn Hóa Định Dạng Ảnh Gốc (Fix Uncompressed Format)", GUILayout.Height(30)))
            {
                FixSourceTexturesFormat();
            }

            EditorGUILayout.Space(10);
            if (GUILayout.Button("🔄 THỰC HIỆN TẤT CẢ (Backup + Fix Format + Tạo Atlas)", GUILayout.Height(35)))
            {
                if (ExecuteBackup())
                {
                    FixSourceTexturesFormat();
                    ExecuteGenerateSpriteAtlases();
                }
            }

            EditorGUILayout.Space(15);

            // --- SECTION 3: PERFORMANCE OPTIMIZATION ---
            EditorGUILayout.LabelField("3. Tối Ưu Hiệu Năng UI (Performance)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Tắt Raycast Target trên tất cả UI tĩnh (Ảnh nền, Khung viền, Text) để giải phóng CPU khi vuốt chạm joystick/cảm ứng.", MessageType.None);
            
            if (GUILayout.Button("⚡ TẮT RAYCAST TARGET UI TĨNH (Active Scene)", GUILayout.Height(35)))
            {
                UIRaycastOptimizer.OptimizeRaycastInActiveScene();
            }
            if (GUILayout.Button("⚡ Tắt Raycast Target Toàn Bộ UI Prefabs", GUILayout.Height(30)))
            {
                UIRaycastOptimizer.OptimizeRaycastInAllUIPrefabs();
            }

            EditorGUILayout.EndScrollView();
        }

        #region TEXTURE FORMAT FIXER LOGIC
        /// <summary>
        /// Chuẩn hóa các texture UI gốc về định dạng Uncompressed (RGBA 32 bit) để khi Unity đóng gói vào SpriteAtlas
        /// không bị nén 2 lần (Double Compression) gây mờ hoặc vỡ pixel.
        /// </summary>
        public static void FixSourceTexturesFormat()
        {
            if (!Directory.Exists(UI_ROOT_FOLDER)) return;

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { UI_ROOT_FOLDER });
            int modifiedCount = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.EndsWith(".spriteatlas", StringComparison.OrdinalIgnoreCase)) continue;

                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer != null)
                    {
                        bool isDirty = false;

                        // Chuyển Compression về None để ảnh gốc giữ nguyên 100% chi tiết
                        if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                        {
                            importer.textureCompression = TextureImporterCompression.Uncompressed;
                            isDirty = true;
                        }

                        // Đảm bảo là Sprite (2D and UI)
                        if (importer.textureType != TextureImporterType.Sprite)
                        {
                            importer.textureType = TextureImporterType.Sprite;
                            isDirty = true;
                        }

                        if (importer.mipmapEnabled)
                        {
                            importer.mipmapEnabled = false;
                            isDirty = true;
                        }

                        if (isDirty)
                        {
                            importer.SaveAndReimport();
                            modifiedCount++;
                        }
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            Debug.Log($"<color=#00FF00>[UI Format Fixer]</color> Đã chuẩn hóa {modifiedCount} ảnh UI về chuẩn Uncompressed (RGBA 32 bit).");
        }
        #endregion

        #region BACKUP LOGIC
        /// <summary>
        /// Sao lưu thư mục Assets/Art/UI sang Assets/Art/UI_Backup/Backup_yyyyMMdd_HHmmss
        /// </summary>
        public static bool ExecuteBackup()
        {
            if (!Directory.Exists(UI_ROOT_FOLDER))
            {
                EditorUtility.DisplayDialog("Lỗi", $"Không tìm thấy thư mục nguồn: {UI_ROOT_FOLDER}", "OK");
                return false;
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupPath = Path.Combine(BACKUP_ROOT_FOLDER, $"Backup_{timestamp}").Replace("\\", "/");

            try
            {
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }

                CopyDirectoryRecursive(UI_ROOT_FOLDER, backupPath);

                AssetDatabase.Refresh();
                Debug.Log($"<color=#00FF00>[UI Backup]</color> Đã sao lưu thành công toàn bộ ảnh UI sang: <b>{backupPath}</b>");
                EditorUtility.DisplayDialog("Thành Công", $"Đã sao lưu toàn bộ UI vào:\n{backupPath}", "OK");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UI Backup Error]: {ex.Message}");
                EditorUtility.DisplayDialog("Lỗi Backup", ex.Message, "OK");
                return false;
            }
        }

        private static void CopyDirectoryRecursive(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            // Copy files
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                // Bỏ qua các file .spriteatlas hoặc file tạm nếu có
                if (fileName.EndsWith(".spriteatlas", StringComparison.OrdinalIgnoreCase)) continue;

                string destFile = Path.Combine(targetDir, fileName);
                File.Copy(file, destFile, true);
            }

            // Copy sub-directories
            foreach (var subDir in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(subDir);
                // Bỏ qua thư mục Atlases hoặc Backup nếu nằm lồng bên trong
                if (dirName.Equals("Atlases", StringComparison.OrdinalIgnoreCase) ||
                    dirName.Equals("UI_Backup", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string destSubDir = Path.Combine(targetDir, dirName);
                CopyDirectoryRecursive(subDir, destSubDir);
            }
        }
        #endregion

        #region SPRITE ATLAS GENERATION LOGIC
        /// <summary>
        /// Tạo và cấu hình 4 nhóm Sprite Atlas theo phân tích chuyên dụng
        /// </summary>
        public void ExecuteGenerateSpriteAtlases()
        {
            if (!Directory.Exists(ATLAS_TARGET_FOLDER))
            {
                Directory.CreateDirectory(ATLAS_TARGET_FOLDER);
                AssetDatabase.Refresh();
            }

            int createdCount = 0;

            // 1. Nhóm Ingame HUD
            createdCount += CreateOrUpdateAtlas(
                "Atlas_UI_HUD_Ingame",
                1024,
                new string[] { "Assets/Art/UI/HUD" },
                new string[] { } // Blacklist nếu có
            );

            // 2. Nhóm Upgrade Icons
            createdCount += CreateOrUpdateAtlas(
                "Atlas_UI_Upgrade_Icons",
                512,
                new string[] { "Assets/Art/UI/UpgradeIcons" },
                new string[] { }
            );

            // 3. Nhóm Vọng Xuyên Hub (Loại trừ Background lớn)
            createdCount += CreateOrUpdateAtlas(
                "Atlas_UI_VongXuyen_Hub",
                1024,
                new string[] { "Assets/Art/UI/VongXuyen" },
                new string[] { "BG_VongXuyen_Forest_Hub.png" } // Không đưa BG 2K vào Atlas
            );

            // 4. Nhóm Controls & Skills Combat
            createdCount += CreateOrUpdateAtlas(
                "Atlas_UI_Controls_Combat",
                512,
                new string[] { "Assets/Art/UI/Joystick", "Assets/Art/UI/Skills" },
                new string[] { }
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Kích hoạt pack tất cả atlases
            SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget);

            Debug.Log($"<color=#00FF00>[Sprite Atlas Manager]</color> Đã tạo và cấu hình thành công {createdCount} Sprite Atlas tại <b>{ATLAS_TARGET_FOLDER}</b>!");
            EditorUtility.DisplayDialog("Thành Công", $"Đã hoàn tất tạo và Pack {createdCount} Sprite Atlas trong thư mục:\n{ATLAS_TARGET_FOLDER}", "OK");
        }

        private int CreateOrUpdateAtlas(string atlasName, int maxTextureSize, string[] sourceFolderPaths, string[] blacklistedFileNames)
        {
            string atlasAssetPath = $"{ATLAS_TARGET_FOLDER}/{atlasName}.spriteatlas";

            SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasAssetPath);
            if (atlas == null)
            {
                atlas = new SpriteAtlas();
                AssetDatabase.CreateAsset(atlas, atlasAssetPath);
            }
            else if (!overwriteExistingAtlases)
            {
                Debug.Log($"[Sprite Atlas] Bỏ qua {atlasName} vì đã tồn tại.");
                return 0;
            }

            // Thiết lập Packing Settings
            SpriteAtlasPackingSettings packingSettings = new SpriteAtlasPackingSettings
            {
                blockOffset = 1,
                enableRotation = false, // Luôn false cho UI sprites
                enableTightPacking = enableTightPacking, // False để giữ biên 9-slice nguyên vẹn
                padding = atlasPadding
            };
            atlas.SetPackingSettings(packingSettings);

            // Thiết lập Texture Settings
            SpriteAtlasTextureSettings textureSettings = new SpriteAtlasTextureSettings
            {
                readable = false,
                generateMipMaps = false, // UI 2D thường không cần MipMaps
                sRGB = true,
                filterMode = FilterMode.Bilinear
            };
            atlas.SetTextureSettings(textureSettings);

            // Thiết lập Platform Settings (Default)
            TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings
            {
                maxTextureSize = maxTextureSize,
                format = TextureImporterFormat.Automatic,
                textureCompression = TextureImporterCompression.Compressed,
                crunchedCompression = true,
                compressionQuality = 80
            };
            atlas.SetPlatformSettings(platformSettings);

            // Thu thập Sprites/Folders hợp lệ
            List<UnityEngine.Object> packables = new List<UnityEngine.Object>();
            HashSet<string> blacklist = new HashSet<string>(blacklistedFileNames, StringComparer.OrdinalIgnoreCase);

            foreach (var folder in sourceFolderPaths)
            {
                if (!Directory.Exists(folder)) continue;

                // Nếu không có blacklist, có thể add cả folder Object
                if (blacklistedFileNames.Length == 0)
                {
                    var folderObject = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folder);
                    if (folderObject != null)
                    {
                        packables.Add(folderObject);
                        continue;
                    }
                }

                // Nếu có file blacklist hoặc muốn add chi tiết từng sprite
                string[] files = Directory.GetFiles(folder, "*.png", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    string fileName = Path.GetFileName(file);
                    if (blacklist.Contains(fileName))
                    {
                        Debug.Log($"<color=#FFFF00>[Atlas Skip]</color> Loại trừ {fileName} khỏi {atlasName}");
                        continue;
                    }

                    string unityPath = file.Replace("\\", "/");
                    Sprite spriteAsset = AssetDatabase.LoadAssetAtPath<Sprite>(unityPath);
                    if (spriteAsset != null)
                    {
                        packables.Add(spriteAsset);
                    }
                }
            }

            // Gán danh sách Packables vào Atlas
            atlas.Remove(atlas.GetPackables());
            atlas.Add(packables.ToArray());

            EditorUtility.SetDirty(atlas);
            return 1;
        }
        #endregion
    }
}
#endif
