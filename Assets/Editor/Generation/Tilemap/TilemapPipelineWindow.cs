using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Projectzombie.Editor.TilemapTools
{
    /// <summary>Import a fixed-grid atlas and build a review prefab without touching existing maps.</summary>
    public sealed class TilemapPipelineWindow : EditorWindow
    {
        private Texture2D atlas;
        private string outputName = "Map_Generated_Draft";
        private int tileSize = 64;
        private int groundTileIndex = 0;
        private int variationTileIndex = 1;
        private int mapRadius = 18;
        private int seed = 2026;
        private bool configureAndroidCompression = true;

        [MenuItem("ProjectZombie/1. 🛠️ Editor & Content/Maps/Tilemap Pipeline MVP", priority = 49)]
        private static void Open() => GetWindow<TilemapPipelineWindow>("Tilemap Pipeline");

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Tilemap Pipeline — Atlas to Prefab", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Chọn atlas dạng lưới đều. MVP này kiểm tra kích thước, cấu hình Sprite slicing, tạo Tile assets và sinh prefab nháp Ground/Decals/Obstacles. Ảnh AI cần được duyệt bằng mắt trước khi dùng chính thức.", MessageType.Info);
            atlas = (Texture2D)EditorGUILayout.ObjectField("Atlas", atlas, typeof(Texture2D), false);
            outputName = EditorGUILayout.TextField("Tên đầu ra", outputName);
            tileSize = EditorGUILayout.IntPopup("Kích thước ô (px)", tileSize, new[] { "64", "32" }, new[] { 64, 32 });
            groundTileIndex = EditorGUILayout.IntField("Index tile nền", groundTileIndex);
            variationTileIndex = EditorGUILayout.IntField("Index biến thể", variationTileIndex);
            mapRadius = EditorGUILayout.IntSlider("Bán kính map (ô)", mapRadius, 4, 48);
            seed = EditorGUILayout.IntField("Seed", seed);
            configureAndroidCompression = EditorGUILayout.Toggle("Cấu hình nén Android", configureAndroidCompression);

            if (atlas != null)
            {
                int cols = atlas.width / Mathf.Max(1, tileSize);
                int rows = atlas.height / Mathf.Max(1, tileSize);
                EditorGUILayout.LabelField($"Kích thước: {atlas.width}×{atlas.height} px — {cols}×{rows} ô ({cols * rows} sprites)");
                if (atlas.width % tileSize != 0 || atlas.height % tileSize != 0)
                    EditorGUILayout.HelpBox("Kích thước atlas không chia hết cho tile size. Hãy crop/resize atlas trước khi import; MVP không tự bóp méo ảnh.", MessageType.Error);
            }

            GUILayout.Space(8);
            using (new EditorGUI.DisabledScope(atlas == null || !IsAtlasValid()))
            {
                if (GUILayout.Button("1. Configure, Slice & Create Tile Assets", GUILayout.Height(32))) ConfigureAndCreateTiles();
                if (GUILayout.Button("2. Build Draft Map Prefab", GUILayout.Height(32))) BuildDraftPrefab();
            }
            EditorGUILayout.HelpBox("Thứ tự sprite dùng trong MVP là row-major từ góc trên trái. Tile nền/biến thể mặc định là index 0 và 1.", MessageType.None);
        }

        private bool IsAtlasValid() => atlas != null && tileSize > 0 && atlas.width % tileSize == 0 && atlas.height % tileSize == 0;

        private string AtlasPath => atlas == null ? null : AssetDatabase.GetAssetPath(atlas);
        private string TileDirectory => $"Assets/Art/Tilemaps/Generated/{SafeName(outputName)}/Tiles";
        private string PrefabPath => $"Assets/_Prefabs/Maps/{SafeName(outputName)}.prefab";

        private void ConfigureAndCreateTiles()
        {
            if (!IsAtlasValid()) return;
            string path = AtlasPath;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) { Debug.LogError($"Không thể cấu hình TextureImporter: {path}"); return; }

            int cols = atlas.width / tileSize;
            int rows = atlas.height / tileSize;
            var rects = new SpriteMetaData[cols * rows];
            for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
            {
                int index = y * cols + x;
                rects[index] = new SpriteMetaData
                {
                    name = $"Tile_{index:000}_{x}_{y}",
                    rect = new Rect(x * tileSize, (rows - 1 - y) * tileSize, tileSize, tileSize),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(.5f, .5f)
                };
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = tileSize;
            importer.filterMode = FilterMode.Point;
            importer.alphaIsTransparency = true;
            importer.sRGBTexture = true;
            importer.textureCompression = TextureImporterCompression.Compressed;
            if (configureAndroidCompression)
            {
                var android = importer.GetPlatformTextureSettings("Android");
                android.name = "Android";
                android.overridden = true;
                android.format = TextureImporterFormat.ASTC_6x6;
                importer.SetPlatformTextureSettings(android);
            }
#pragma warning disable CS0618
            importer.spritesheet = rects;
#pragma warning restore CS0618
            importer.SaveAndReimport();

            Directory.CreateDirectory(TileDirectory);
            AssetDatabase.Refresh();
            var loaded = AssetDatabase.LoadAllAssetsAtPath(path);
            int created = 0;
            foreach (var item in loaded)
            {
                if (!(item is Sprite sprite)) continue;
                string tilePath = $"{TileDirectory}/{SafeName(sprite.name)}.asset";
                var tile = AssetDatabase.LoadAssetAtPath<Tile>(tilePath);
                if (tile == null)
                {
                    tile = ScriptableObject.CreateInstance<Tile>();
                    AssetDatabase.CreateAsset(tile, tilePath);
                }
                tile.sprite = sprite;
                tile.colliderType = Tile.ColliderType.None;
                EditorUtility.SetDirty(tile);
                created++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[TilemapPipeline] Imported {created} sprites from {path}; tile assets at {TileDirectory}.");
        }

        private void BuildDraftPrefab()
        {
            if (!IsAtlasValid()) return;
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null && !EditorUtility.DisplayDialog("Prefab đã tồn tại", $"Ghi đè {PrefabPath}?", "Ghi đè", "Hủy")) return;
            ConfigureAndCreateTiles();

            var root = new GameObject(SafeName(outputName));
            try
            {
                var grid = root.AddComponent<Grid>();
                grid.cellSize = Vector3.one;
                var ground = CreateLayer(root.transform, "Tilemap_Ground", "Tilemap_Ground", 0);
                CreateLayer(root.transform, "Tilemap_Decals", "Tilemap_Decals", 1);
                var obstacle = CreateLayer(root.transform, "Tilemap_Obstacles", "Entities", 0);
                int unityLayer = LayerMask.NameToLayer("Obstacle");
                if (unityLayer >= 0) obstacle.gameObject.layer = unityLayer;
                var tileCollider = obstacle.gameObject.AddComponent<TilemapCollider2D>();
                tileCollider.usedByComposite = true;
                var rb = obstacle.gameObject.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Static;
                obstacle.gameObject.AddComponent<CompositeCollider2D>().geometryType = CompositeCollider2D.GeometryType.Polygons;

                var baseTile = LoadTile(groundTileIndex);
                var variation = LoadTile(variationTileIndex) ?? baseTile;
                if (baseTile == null) throw new InvalidOperationException($"Không tìm thấy tile index {groundTileIndex}. Hãy chạy bước 1 trước.");
                var rng = new System.Random(seed);
                for (int x = -mapRadius; x <= mapRadius; x++)
                for (int y = -mapRadius; y <= mapRadius; y++)
                    ground.SetTile(new Vector3Int(x, y, 0), variation != null && rng.Next(8) == 0 ? variation : baseTile);

                string dir = Path.GetDirectoryName(PrefabPath).Replace('\\', '/');
                Directory.CreateDirectory(dir);
                var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                AssetDatabase.SaveAssets();
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
                Debug.Log($"[TilemapPipeline] Draft map saved: {PrefabPath}. Seed={seed}, radius={mapRadius}.");
            }
            catch (Exception e) { Debug.LogException(e); }
            finally { DestroyImmediate(root); }
        }

        private Tilemap CreateLayer(Transform parent, string name, string sortingLayer, int order)
        {
            var go = new GameObject(name, typeof(Tilemap), typeof(TilemapRenderer));
            go.transform.SetParent(parent, false);
            var renderer = go.GetComponent<TilemapRenderer>();
            renderer.sortingLayerName = sortingLayer;
            renderer.sortingOrder = order;
            return go.GetComponent<Tilemap>();
        }

        private Tile LoadTile(int index)
        {
            var files = Directory.Exists(TileDirectory) ? Directory.GetFiles(TileDirectory, "*.asset") : Array.Empty<string>();
            Array.Sort(files, StringComparer.Ordinal);
            if (index < 0 || index >= files.Length) return null;
            return AssetDatabase.LoadAssetAtPath<Tile>(files[index].Replace('\\', '/'));
        }

        private static string SafeName(string value)
        {
            foreach (char c in Path.GetInvalidFileNameChars()) value = value.Replace(c, '_');
            return string.IsNullOrWhiteSpace(value) ? "Map_Generated_Draft" : value.Trim().Replace(' ', '_');
        }
    }
}
