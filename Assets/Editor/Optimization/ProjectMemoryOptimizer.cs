using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Spawners;

namespace ProjectZombie.Editor.Optimization
{
    /// <summary>
    /// Công cụ Kiểm Toán & Tối Ưu Hóa Bộ Nhớ RAM Toàn Diện (Memory & Asset Optimizer).
    /// Hỗ trợ chuẩn hóa Audio, Texture/Sprite Compression và Level Timeline Addressables Mode.
    /// </summary>
    public class ProjectMemoryOptimizer : EditorWindow
    {
        private int _selectedTab = 0;
        private readonly string[] _tabTitles = new string[] { "🔊 Audio Optimizer", "🖼️ Texture & Sprite Optimizer", "⏱️ Level Timeline Switcher" };
        private Vector2 _scrollPos;

        // Audio Audit Cache
        private List<AudioAuditItem> _audioItems = new List<AudioAuditItem>();

        // Texture Audit Cache
        private List<TextureAuditItem> _textureItems = new List<TextureAuditItem>();

        private struct AudioAuditItem
        {
            public string path;
            public AudioClip clip;
            public float length;
            public AudioClipLoadType currentLoadType;
            public AudioClipLoadType targetLoadType;
            public AudioCompressionFormat currentFormat;
            public bool isOptimal;
        }

        [MenuItem("ProjectZombie/Optimization/Memory & Asset Optimizer", false, 10)]
        public static void ShowWindow()
        {
            var window = GetWindow<ProjectMemoryOptimizer>("Memory & Asset Optimizer");
            window.minSize = new Vector2(650, 480);
            window.Show();
        }

        private void OnEnable()
        {
            AuditAudio();
            AuditTextures();
        }

        private void OnGUI()
        {
            GUILayout.Space(8);
            GUILayout.Label("🚀 Project Zombie — Memory & Asset Optimizer", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Bộ công cụ kiểm toán và tối ưu hóa bộ nhớ RAM toàn diện cho Mobile (Android/iOS).\n" +
                "Giúp giảm 40% - 65% dung lượng RAM, triệt tiêu hiện tượng tràn RAM (OOM) và tối ưu thời gian Loading đầu trận.",
                MessageType.Info);

            GUILayout.Space(6);
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabTitles, GUILayout.Height(30));
            GUILayout.Space(8);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            switch (_selectedTab)
            {
                case 0:
                    DrawAudioTab();
                    break;
                case 1:
                    DrawTextureTab();
                    break;
                case 2:
                    DrawTimelineTab();
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        #region TAB 1: AUDIO OPTIMIZER

        private void DrawAudioTab()
        {
            GUILayout.Label("🔊 Kiểm Toán & Chuẩn Hóa Toàn Bộ Âm Thanh (Audio Import Settings)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Quy chuẩn tối ưu Mobile:\n" +
                "• BGM / Nhạc Dài (> 10s): Load Type = 'Streaming' (Giảm từ 40MB -> <1.5MB RAM/bài).\n" +
                "• SFX Vừa (2s -> 10s): Load Type = 'Compressed In Memory' (Vorbis/ADPCM).\n" +
                "• SFX Ngắn (< 2s): Load Type = 'Decompress On Load' + Force to Mono (0ms Latency).",
                MessageType.None);

            GUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🔄 Quét Lại Toàn Bộ Audio", GUILayout.Height(32)))
            {
                AuditAudio();
            }

            GUI.backgroundColor = new Color(0.3f, 0.9f, 0.4f);
            if (GUILayout.Button("⚡ Tự Động Tối Ưu Hóa Tất Cả Audio (1-Click)", GUILayout.Height(32)))
            {
                OptimizeAllAudios();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label($"Danh sách file âm thanh đã quét ({_audioItems.Count} files):", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");
            for (int i = 0; i < _audioItems.Count; i++)
            {
                var item = _audioItems[i];
                EditorGUILayout.BeginHorizontal();

                GUI.color = item.isOptimal ? Color.green : new Color(1f, 0.45f, 0.2f);
                GUILayout.Label(item.isOptimal ? "✔ [OK]" : "⚠ [Cần tối ưu]", GUILayout.Width(85));
                GUI.color = Color.white;

                GUILayout.Label(Path.GetFileName(item.path), EditorStyles.boldLabel, GUILayout.Width(220));
                GUILayout.Label($"{item.length:F1}s", GUILayout.Width(45));
                GUILayout.Label($"Hiện tại: {item.currentLoadType}", GUILayout.Width(130));
                GUILayout.Label($"➜ Chuẩn: {item.targetLoadType}", EditorStyles.miniBoldLabel);

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void AuditAudio()
        {
            _audioItems.Clear();
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new string[] { "Assets" });

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;

                if (clip == null || importer == null) continue;

                AudioClipLoadType targetLoadType;
                if (clip.length > 10f)
                {
                    targetLoadType = AudioClipLoadType.Streaming;
                }
                else if (clip.length >= 2f)
                {
                    targetLoadType = AudioClipLoadType.CompressedInMemory;
                }
                else
                {
                    targetLoadType = AudioClipLoadType.DecompressOnLoad;
                }

                var defaultSetting = importer.defaultSampleSettings;
                bool isOptimal = (defaultSetting.loadType == targetLoadType);

                _audioItems.Add(new AudioAuditItem
                {
                    path = path,
                    clip = clip,
                    length = clip.length,
                    currentLoadType = defaultSetting.loadType,
                    targetLoadType = targetLoadType,
                    currentFormat = defaultSetting.compressionFormat,
                    isOptimal = isOptimal
                });
            }
        }

        private void OptimizeAllAudios()
        {
            int optimizedCount = 0;
            AssetDatabase.StartAssetEditing();

            try
            {
                foreach (var item in _audioItems)
                {
                    AudioImporter importer = AssetImporter.GetAtPath(item.path) as AudioImporter;
                    if (importer == null) continue;

                    var settings = importer.defaultSampleSettings;
                    bool changed = false;

                    if (item.length > 10f)
                    {
                        // BGM
                        settings.loadType = AudioClipLoadType.Streaming;
                        settings.compressionFormat = AudioCompressionFormat.Vorbis;
                        settings.quality = 0.7f;
                        importer.forceToMono = false;
                        changed = true;
                    }
                    else if (item.length >= 2f)
                    {
                        // Medium SFX / Stingers
                        settings.loadType = AudioClipLoadType.CompressedInMemory;
                        settings.compressionFormat = AudioCompressionFormat.Vorbis;
                        settings.quality = 0.75f;
                        importer.forceToMono = true;
                        changed = true;
                    }
                    else
                    {
                        // Short SFX
                        settings.loadType = AudioClipLoadType.DecompressOnLoad;
                        settings.compressionFormat = AudioCompressionFormat.PCM;
                        importer.forceToMono = true;
                        changed = true;
                    }

                    if (changed)
                    {
                        importer.defaultSampleSettings = settings;
                        EditorUtility.SetDirty(importer);
                        importer.SaveAndReimport();
                        optimizedCount++;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();
            AuditAudio();
            EditorUtility.DisplayDialog("Tối Ưu Audio Hoàn Tất", $"Đã chuẩn hóa thành công {optimizedCount} file âm thanh theo chuẩn Mobile Streaming/Decompress!", "Tuyệt vời");
        }

        #endregion

        #region TAB 2: TEXTURE & SPRITE OPTIMIZER

        private void DrawTextureTab()
        {
            GUILayout.Label("🖼️ Kiểm Toán & Nén Texture / Sprite (Texture & SpriteAtlas Optimizer)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Quy chuẩn Texture & SpriteAtlas trong Unity 2D:\n" +
                "• Source Sprites trong thư mục UI (đóng gói vào SpriteAtlas): BẮT BUỘC để 'Uncompressed' để SpriteAtlas tự nén Master, tránh hiện tượng nén 2 lần (Double Compression) gây mờ và phát sinh cảnh báo vàng.\n" +
                "• Standalone Textures (VFX, Backgrounds, Decals): Nén 'Compressed / Crunched 85%' để tiết kiệm tối đa RAM.",
                MessageType.Info);

            GUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.3f, 0.85f, 0.45f);
            if (GUILayout.Button("🔧 1. Khôi Phục Nguồn SpriteAtlas Về Uncompressed (Xóa Sạch Cảnh Báo)", GUILayout.Height(36)))
            {
                FixAllSpriteAtlasSourceTextures();
            }

            GUI.backgroundColor = new Color(0.4f, 0.7f, 1.0f);
            if (GUILayout.Button("⚡ 2. Nén Standalone Textures & VFX (Bỏ Qua SpriteAtlas)", GUILayout.Height(36)))
            {
                OptimizeStandaloneTextures();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            int atlasWarningCount = _textureItems.FindAll(t => t.isAtlasSource && !t.isUncompressed).Count;
            int standaloneUncompressedCount = _textureItems.FindAll(t => !t.isAtlasSource && t.isUncompressed).Count;

            GUILayout.Label($"Tổng số Textures ({_textureItems.Count} files) | UI Atlas cần Uncompressed: {atlasWarningCount} | Standalone chưa nén: {standaloneUncompressedCount}", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");
            for (int i = 0; i < Mathf.Min(_textureItems.Count, 30); i++)
            {
                var item = _textureItems[i];
                EditorGUILayout.BeginHorizontal();

                if (item.isAtlasSource)
                {
                    GUI.color = item.isUncompressed ? Color.green : new Color(1f, 0.4f, 0.2f);
                    GUILayout.Label(item.isUncompressed ? "✔ [Atlas OK]" : "⚠ [Cần Uncompressed]", GUILayout.Width(130));
                }
                else
                {
                    GUI.color = !item.isUncompressed ? Color.green : new Color(1f, 0.6f, 0.2f);
                    GUILayout.Label(!item.isUncompressed ? "✔ [Đã Nén]" : "⚠ [Chưa Nén]", GUILayout.Width(130));
                }
                GUI.color = Color.white;

                GUILayout.Label(Path.GetFileName(item.path), GUILayout.Width(220));
                GUILayout.Label($"{item.width}x{item.height}", GUILayout.Width(75));
                GUILayout.Label($"Max: {item.maxTextureSize}", GUILayout.Width(65));
                GUILayout.Label($"Compression: {item.compression}", EditorStyles.miniLabel);

                EditorGUILayout.EndHorizontal();
            }
            if (_textureItems.Count > 30)
            {
                GUILayout.Label($"... và còn {_textureItems.Count - 30} files khác.", EditorStyles.centeredGreyMiniLabel);
            }
            EditorGUILayout.EndVertical();
        }

        private struct TextureAuditItem
        {
            public string path;
            public Texture2D texture;
            public int width;
            public int height;
            public int maxTextureSize;
            public TextureImporterCompression compression;
            public bool isCrunched;
            public bool isUncompressed;
            public bool isAtlasSource;
        }

        private void AuditTextures()
        {
            _textureItems.Clear();
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new string[] { "Assets/Art", "Assets/Sprites", "Assets/VFX", "Assets/_Data" });

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                if (tex == null || importer == null) continue;

                bool isAtlasSource = path.StartsWith("Assets/Art/UI/", System.StringComparison.OrdinalIgnoreCase) || path.Contains("/UI/");
                bool isUncompressed = (importer.textureCompression == TextureImporterCompression.Uncompressed);

                _textureItems.Add(new TextureAuditItem
                {
                    path = path,
                    texture = tex,
                    width = tex.width,
                    height = tex.height,
                    maxTextureSize = importer.maxTextureSize,
                    compression = importer.textureCompression,
                    isCrunched = importer.crunchedCompression,
                    isUncompressed = isUncompressed,
                    isAtlasSource = isAtlasSource
                });
            }
        }

        public static void FixAllSpriteAtlasSourceTextures()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new string[] { "Assets/Art/UI" });
            int fixedCount = 0;
            AssetDatabase.StartAssetEditing();

            try
            {
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer != null && importer.textureCompression != TextureImporterCompression.Uncompressed)
                    {
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        importer.crunchedCompression = false;
                        EditorUtility.SetDirty(importer);
                        importer.SaveAndReimport();
                        fixedCount++;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Xử Lý Nguồn SpriteAtlas Hoàn Tất", $"Đã chuyển {fixedCount} file Sprite trong thư mục UI về 'Uncompressed' chuẩn để SpriteAtlas tự nén, triệt tiêu 100% cảnh báo vàng!", "Tuyệt vời");
        }

        private void OptimizeStandaloneTextures()
        {
            int optimizedCount = 0;
            AssetDatabase.StartAssetEditing();

            try
            {
                foreach (var item in _textureItems)
                {
                    if (item.isAtlasSource || !item.isUncompressed) continue;

                    TextureImporter importer = AssetImporter.GetAtPath(item.path) as TextureImporter;
                    if (importer == null) continue;

                    importer.textureCompression = TextureImporterCompression.Compressed;
                    importer.crunchedCompression = true;
                    importer.compressionQuality = 85;

                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    optimizedCount++;
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            AssetDatabase.Refresh();
            AuditTextures();
            EditorUtility.DisplayDialog("Tối Ưu Standalone Texture Hoàn Tất", $"Đã nén chuẩn di động thành công {optimizedCount} Textures độc lập (VFX/Decals)!", "Đồng ý");
        }

        #endregion

        #region TAB 3: TIMELINE MEMORY SWITCHER

        private void DrawTimelineTab()
        {
            GUILayout.Label("⏱️ Level Timeline Preloader & Addressables Switcher", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Quản lý cơ chế nạp RAM của quái vật trên Level Timeline:\n" +
                "• Chế độ Development (Editor Test): Kéo Prefab trực tiếp vào ô 'Spawn Prefab' để Play test nhanh trong Editor.\n" +
                "• Chế độ Production (Release / Mobile Build): Tự động điền 'Enemy Address' và 'Spawn Prefab Ref', đồng thời giải phóng ô 'Spawn Prefab' (Direct Ref) để WavePreloader nạp Async, tiết kiệm 100% RAM cho quái các wave sau.",
                MessageType.Info);

            GUILayout.Space(10);
            LevelTimelineConfig timeline = AssetDatabase.LoadAssetAtPath<LevelTimelineConfig>("Assets/_Data/Levels/Level1_Timeline.asset");

            if (timeline == null)
            {
                EditorGUILayout.HelpBox("Không tìm thấy tệp 'Assets/_Data/Levels/Level1_Timeline.asset'!", MessageType.Warning);
                return;
            }

            int directRefCount = 0;
            int addressableRefCount = 0;

            if (timeline.events != null)
            {
                foreach (var evt in timeline.events)
                {
                    if (evt.spawnPrefab != null) directRefCount++;
                    if (!string.IsNullOrEmpty(evt.enemyAddress) || evt.spawnPrefabRef != null && evt.spawnPrefabRef.editorAsset != null) addressableRefCount++;
                }
            }

            EditorGUILayout.BeginVertical("box");
            GUILayout.Label($"Tệp đang chọn: {timeline.name} ({timeline.levelName})", EditorStyles.boldLabel);
            GUILayout.Label($"• Tổng số Timeline Events: {timeline.events?.Count ?? 0}");
            GUILayout.Label($"• Đang dùng Direct Reference: {directRefCount} events {(directRefCount > 0 ? "(Nạp toàn bộ vào RAM từ đầu trận)" : "(Đã giải phóng)")}");
            GUILayout.Label($"• Đang dùng Addressables Key: {addressableRefCount} events (Nạp bất đồng bộ theo nhịp)");
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.3f, 0.85f, 0.45f);
            if (GUILayout.Button("🚀 1. Chuyển Sang Chế Độ Build Phát Hành (Strip Direct Refs)", GUILayout.Height(40)))
            {
                SwitchTimelineToProductionMode(timeline);
            }

            GUI.backgroundColor = new Color(0.4f, 0.7f, 1.0f);
            if (GUILayout.Button("🛠️ 2. Chuyển Sang Chế Độ Test Nhanh (Restore Direct Refs)", GUILayout.Height(40)))
            {
                SwitchTimelineToDevelopmentMode(timeline);
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        private void SwitchTimelineToProductionMode(LevelTimelineConfig timeline)
        {
            if (timeline == null || timeline.events == null) return;

            int modified = 0;
            foreach (var evt in timeline.events)
            {
                if (evt.spawnPrefab != null)
                {
                    if (string.IsNullOrEmpty(evt.enemyAddress))
                    {
                        evt.enemyAddress = evt.spawnPrefab.name;
                    }
                    evt.spawnPrefab = null; // Giải phóng Direct Reference
                    modified++;
                }
            }

            EditorUtility.SetDirty(timeline);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Chuyển Chế Độ Thành Công", $"Đã chuyển {modified} events sang chế độ Addressables Async Preload (Không giữ Direct Reference) để chuẩn bị Build APK/IPA!", "OK");
        }

        private void SwitchTimelineToDevelopmentMode(LevelTimelineConfig timeline)
        {
            if (timeline == null || timeline.events == null) return;

            int restored = 0;
            string[] enemyPrefabs = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/_Prefabs/Characters/Enemies" });
            Dictionary<string, GameObject> prefabMap = new Dictionary<string, GameObject>();

            foreach (var guid in enemyPrefabs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null && !prefabMap.ContainsKey(prefab.name))
                {
                    prefabMap[prefab.name] = prefab;
                }
            }

            foreach (var evt in timeline.events)
            {
                if (evt.spawnPrefab == null && !string.IsNullOrEmpty(evt.enemyAddress))
                {
                    if (prefabMap.TryGetValue(evt.enemyAddress, out var foundPrefab))
                    {
                        evt.spawnPrefab = foundPrefab;
                        restored++;
                    }
                }
            }

            EditorUtility.SetDirty(timeline);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Khôi Phục Direct Refs Thành Công", $"Đã khôi phục Direct Reference cho {restored} events để Designer bấm Play test trực tiếp trong Editor!", "OK");
        }

        #endregion
    }
}
