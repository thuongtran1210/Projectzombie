using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Projectzombie.Editor.TilemapTools
{
    /// <summary>
    /// Tool tự động tạo lập và nâng cấp hệ thống Tilemap 2.5D Cổ Phong Đông Sơn cho 3 Ải:
    /// - Ải 1: Map_BambooForest (Rừng Trúc Âm Ty - Nền đất cỏ, lối mòn gạch, rêu xanh & bụi trúc)
    /// - Ải 2: Map_AncientCitadel (Cổ Thành Đông Sơn - Sân gạch nung Bát Tràng, quảng trường bia đá, tường rêu cổ)
    /// - Ải 3: Map_CinnabarSwamp (Đầm Lầy Thần Sa - Đầm lầy u ám, hồ sen tím độc, bậc đá rêu & chướng ngại vật)
    /// </summary>
    public static class RemoteStagesPrefabGenerator
    {
        private const string PREFAB_DIR = "Assets/_Prefabs/Maps";
        private const string ATLAS_PATH = "Assets/Art/Tilemaps/SanDinhLangCo/Tileset_SanDinhLangCo.png";

        [MenuItem("Tools/ProjectZombie/Maps/🎨 Nâng Cấp Tuyệt Đẹp 3 Bản Đồ (Tilemap Visual Upgrade)", priority = 210)]
        [MenuItem("ProjectZombie/Maps/🎨 Nâng Cấp Tuyệt Đẹp 3 Bản Đồ (Tilemap Visual Upgrade)", priority = 210)]
        public static void GenerateAllDetailedMaps()
        {
            if (!Directory.Exists(PREFAB_DIR))
            {
                Directory.CreateDirectory(PREFAB_DIR);
                AssetDatabase.Refresh();
            }

            // Nạp toàn bộ Sprite từ Tileset Atlas
            var spriteDict = LoadTilesetSprites(ATLAS_PATH);
            if (spriteDict.Count == 0)
            {
                Debug.LogError($"[RemoteStagesPrefabGenerator] Không thể nạp sprites từ: {ATLAS_PATH}");
                return;
            }

            // Sinh 3 bản đồ chi tiết
            BuildBambooForestMap(spriteDict);
            BuildAncientCitadelMap(spriteDict);
            BuildCinnabarSwampMap(spriteDict);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=#00FF88>[RemoteStagesPrefabGenerator] HOÀN TẤT NÂNG CẤP MỸ THUẬT 3 BẢN ĐỒ TILEMAP 2.5D!</color>");
            EditorUtility.DisplayDialog("Nâng Cấp Tilemap Thành Công", "Đã kiến tạo lại 3 bản đồ (Rừng Trúc, Cổ Thành, Đầm Lầy) với đầy đủ 4 tầng Sorting Layer, hoa văn gạch, ao sen, tường rêu và ranh giới chiến đấu!", "OK");
        }

        private static Dictionary<string, Sprite> LoadTilesetSprites(string path)
        {
            var dict = new Dictionary<string, Sprite>();
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in assets)
            {
                if (a is Sprite s)
                {
                    dict[s.name] = s;
                }
            }
            return dict;
        }

        private static Sprite GetSprite(Dictionary<string, Sprite> dict, string name, string fallback = "Tile_0_0")
        {
            if (dict.TryGetValue(name, out var sprite)) return sprite;
            if (dict.TryGetValue(fallback, out var fbSprite)) return fbSprite;
            using var it = dict.Values.GetEnumerator();
            return it.MoveNext() ? it.Current : null;
        }

        private static Tile CreateTile(Sprite sprite, Color? color = null)
        {
            if (sprite == null) return null;
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            if (color.HasValue) tile.color = color.Value;
            return tile;
        }

        #region --- ẢI 1: RỪNG TRÚC ÂM TY (BAMBOO FOREST) ---
        private static void BuildBambooForestMap(Dictionary<string, Sprite> sprites)
        {
            string mapName = "Map_BambooForest";
            var (root, ground, decals, obstacles) = CreateMapHierarchy(mapName);

            var tGrass = CreateTile(GetSprite(sprites, "Tile_0_0"), new Color(0.35f, 0.52f, 0.32f, 1f));
            var tPath = CreateTile(GetSprite(sprites, "Tile_1_0"), new Color(0.55f, 0.48f, 0.38f, 1f));
            var tMoss = CreateTile(GetSprite(sprites, "Tile_2_0"), new Color(0.25f, 0.45f, 0.25f, 0.85f));
            var tRock = CreateTile(GetSprite(sprites, "Tile_0_2"), new Color(0.40f, 0.45f, 0.38f, 1f));

            int halfSize = 24;
            // 1. Nền cỏ rậm rạp
            for (int x = -halfSize; x <= halfSize; x++)
            {
                for (int y = -halfSize; y <= halfSize; y++)
                {
                    ground.SetTile(new Vector3Int(x, y, 0), tGrass);

                    // Con đường mòn đất ở giữa hình chữ thập
                    if (Mathf.Abs(x) <= 2 || Mathf.Abs(y) <= 2)
                    {
                        ground.SetTile(new Vector3Int(x, y, 0), tPath);
                    }
                    // Các vệt rêu ngẫu nhiên
                    else if ((x * 7 + y * 13) % 5 == 0)
                    {
                        decals.SetTile(new Vector3Int(x, y, 0), tMoss);
                    }
                }
            }

            // 2. Chướng ngại vật: Các bụi đá & rặng tre cổ quanh 4 góc
            int[,] rockClusters = new int[,]
            {
                { -12, 10 }, { -11, 10 }, { -12, 9 },
                { 12, 10 }, { 11, 10 }, { 12, 9 },
                { -12, -10 }, { -11, -10 }, { -12, -9 },
                { 12, -10 }, { 11, -10 }, { 12, -9 },
                { -6, -6 }, { 6, 6 }, { -6, 6 }, { 6, -6 }
            };

            for (int i = 0; i < rockClusters.GetLength(0); i++)
            {
                int rx = rockClusters[i, 0];
                int ry = rockClusters[i, 1];
                obstacles.SetTile(new Vector3Int(rx, ry, 0), tRock);
            }

            CreateMapBoundaryColliders(root, 48f, 48f);
            SaveAndRegisterPrefab(root, mapName, "Group_DLC_Stages_Remote");
        }
        #endregion

        #region --- ẢI 2: CỔ THÀNH ĐÔNG SƠN (ANCIENT CITADEL) ---
        private static void BuildAncientCitadelMap(Dictionary<string, Sprite> sprites)
        {
            string mapName = "Map_AncientCitadel";
            var (root, ground, decals, obstacles) = CreateMapHierarchy(mapName);

            var tBrick = CreateTile(GetSprite(sprites, "Tile_0_0"), new Color(0.72f, 0.38f, 0.28f, 1f)); // Gạch nung Bát Tràng
            var tCourtyard = CreateTile(GetSprite(sprites, "Tile_1_0"), new Color(0.85f, 0.65f, 0.42f, 1f)); // Sân đại điện
            var tCrackDecal = CreateTile(GetSprite(sprites, "Tile_3_0"), new Color(0.35f, 0.20f, 0.15f, 0.75f)); // Vết nứt thời gian
            var tCitadelWall = CreateTile(GetSprite(sprites, "Tile_0_2"), new Color(0.45f, 0.38f, 0.32f, 1f)); // Tường thành đá cổ
            var tJadeStatue = CreateTile(GetSprite(sprites, "Tile_1_2"), new Color(0.30f, 0.55f, 0.45f, 1f)); // Trụ đá phong ấn

            int halfSize = 25;
            for (int x = -halfSize; x <= halfSize; x++)
            {
                for (int y = -halfSize; y <= halfSize; y++)
                {
                    // Nền gạch ngoài
                    ground.SetTile(new Vector3Int(x, y, 0), tBrick);

                    // Đại điện trung tâm (Hình vuông 14x14)
                    if (Mathf.Abs(x) <= 7 && Mathf.Abs(y) <= 7)
                    {
                        ground.SetTile(new Vector3Int(x, y, 0), tCourtyard);

                        // Hoa văn nứt gạch cổ kính
                        if ((x * 3 + y * 5) % 4 == 0)
                        {
                            decals.SetTile(new Vector3Int(x, y, 0), tCrackDecal);
                        }
                    }
                }
            }

            // Tường thành ngắt quãng phòng thủ (Citadel Walls)
            for (int x = -16; x <= 16; x++)
            {
                if (Mathf.Abs(x) > 3 && Mathf.Abs(x) < 14)
                {
                    obstacles.SetTile(new Vector3Int(x, 14, 0), tCitadelWall);
                    obstacles.SetTile(new Vector3Int(x, -14, 0), tCitadelWall);
                }
            }
            for (int y = -14; y <= 14; y++)
            {
                if (Mathf.Abs(y) > 3 && Mathf.Abs(y) < 12)
                {
                    obstacles.SetTile(new Vector3Int(16, y, 0), tCitadelWall);
                    obstacles.SetTile(new Vector3Int(-16, y, 0), tCitadelWall);
                }
            }

            // 4 Trụ đá phong ấn bảo vệ 4 góc
            obstacles.SetTile(new Vector3Int(-7, 7, 0), tJadeStatue);
            obstacles.SetTile(new Vector3Int(7, 7, 0), tJadeStatue);
            obstacles.SetTile(new Vector3Int(-7, -7, 0), tJadeStatue);
            obstacles.SetTile(new Vector3Int(7, -7, 0), tJadeStatue);

            CreateMapBoundaryColliders(root, 50f, 50f);
            SaveAndRegisterPrefab(root, mapName, "Group_DLC_Stages_Remote");
        }
        #endregion

        #region --- ẢI 3: ĐẦM LẦY THẦN SA (CINNABAR SWAMP) ---
        private static void BuildCinnabarSwampMap(Dictionary<string, Sprite> sprites)
        {
            string mapName = "Map_CinnabarSwamp";
            var (root, ground, decals, obstacles) = CreateMapHierarchy(mapName);

            var tMud = CreateTile(GetSprite(sprites, "Tile_0_0"), new Color(0.22f, 0.28f, 0.20f, 1f)); // Bùn lầy u ám
            var tWater = CreateTile(GetSprite(sprites, "Tile_1_0"), new Color(0.12f, 0.35f, 0.38f, 1f)); // Ao nước ngọc độc
            var tLotusDecal = CreateTile(GetSprite(sprites, "Tile_2_0"), new Color(0.85f, 0.40f, 0.55f, 0.90f)); // Lá sen & hoa sen hồng
            var tSwampRock = CreateTile(GetSprite(sprites, "Tile_0_2"), new Color(0.20f, 0.30f, 0.24f, 1f)); // Đá rêu ngập nước

            int halfSize = 25;
            for (int x = -halfSize; x <= halfSize; x++)
            {
                for (int y = -halfSize; y <= halfSize; y++)
                {
                    // Nền bùn lầy
                    ground.SetTile(new Vector3Int(x, y, 0), tMud);

                    // 4 Vùng ao sen tự nhiên ở 4 góc
                    float distTopLeft = Vector2.Distance(new Vector2(x, y), new Vector2(-12, 12));
                    float distBottomRight = Vector2.Distance(new Vector2(x, y), new Vector2(12, -12));
                    float distCenterPond = Vector2.Distance(new Vector2(x, y), Vector2.zero);

                    if (distTopLeft < 5.5f || distBottomRight < 5.5f || (distCenterPond > 7f && distCenterPond < 10f && (x + y) % 3 == 0))
                    {
                        ground.SetTile(new Vector3Int(x, y, 0), tWater);
                        if ((x * 2 + y * 7) % 3 == 0)
                        {
                            decals.SetTile(new Vector3Int(x, y, 0), tLotusDecal);
                        }
                    }
                }
            }

            // Chướng ngại vật: Bãi đá rêu phong bao quanh bờ hồ
            Vector2Int[] rockPositions = new Vector2Int[]
            {
                new(-16, 12), new(-15, 13), new(-14, 15), new(-10, 16),
                new(16, -12), new(15, -13), new(14, -15), new(10, -16),
                new(-5, 0), new(5, 0), new(0, -5), new(0, 5),
                new(-8, -8), new(8, 8)
            };

            foreach (var pos in rockPositions)
            {
                obstacles.SetTile(new Vector3Int(pos.x, pos.y, 0), tSwampRock);
            }

            CreateMapBoundaryColliders(root, 50f, 50f);
            SaveAndRegisterPrefab(root, mapName, "Group_DLC_Stages_Remote");
        }
        #endregion

        #region --- HELPER BUILD HIERARCHY & BOUNDARIES ---
        private static (GameObject root, Tilemap ground, Tilemap decals, Tilemap obstacles) CreateMapHierarchy(string mapName)
        {
            GameObject rootObj = new GameObject(mapName);
            Grid grid = rootObj.AddComponent<Grid>();
            grid.cellSize = new Vector3(1f, 1f, 0f);

            // Tầng 0: Background
            GameObject bgObj = new GameObject("Tilemap_BaseGround");
            bgObj.transform.SetParent(rootObj.transform, false);
            var bgTilemap = bgObj.AddComponent<Tilemap>();
            var bgRend = bgObj.AddComponent<TilemapRenderer>();
            bgRend.sortingLayerName = "Background";
            bgRend.sortingOrder = -1;

            // Tầng 1: Ground
            GameObject groundObj = new GameObject("Tilemap_Ground");
            groundObj.transform.SetParent(rootObj.transform, false);
            var groundTilemap = groundObj.AddComponent<Tilemap>();
            var groundRend = groundObj.AddComponent<TilemapRenderer>();
            groundRend.sortingLayerName = "Tilemap_Ground";
            groundRend.sortingOrder = 0;

            // Tầng 2: Decals
            GameObject decalObj = new GameObject("Tilemap_Decals");
            decalObj.transform.SetParent(rootObj.transform, false);
            var decalTilemap = decalObj.AddComponent<Tilemap>();
            var decalRend = decalObj.AddComponent<TilemapRenderer>();
            decalRend.sortingLayerName = "Tilemap_Decals";
            decalRend.sortingOrder = 1;

            // Tầng 3: Obstacles
            GameObject obstacleObj = new GameObject("Tilemap_Obstacles");
            obstacleObj.transform.SetParent(rootObj.transform, false);
            int obstacleLayer = LayerMask.NameToLayer("Obstacle");
            if (obstacleLayer != -1) obstacleObj.layer = obstacleLayer;

            var obstacleTilemap = obstacleObj.AddComponent<Tilemap>();
            var obstacleRend = obstacleObj.AddComponent<TilemapRenderer>();
            obstacleRend.sortingLayerName = "Entities";
            obstacleRend.sortingOrder = 0;

            var tilemapCollider = obstacleObj.AddComponent<TilemapCollider2D>();
            tilemapCollider.usedByComposite = true;
            var compositeCollider = obstacleObj.AddComponent<CompositeCollider2D>();
            compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
            var rb = obstacleObj.GetComponent<Rigidbody2D>();
            if (rb != null) rb.bodyType = RigidbodyType2D.Static;

            return (rootObj, groundTilemap, decalTilemap, obstacleTilemap);
        }

        private static void CreateMapBoundaryColliders(GameObject parent, float width, float height)
        {
            GameObject boundariesRoot = new GameObject("Map_Boundaries");
            boundariesRoot.transform.SetParent(parent.transform, false);
            int obstacleLayer = LayerMask.NameToLayer("Obstacle");
            if (obstacleLayer != -1) boundariesRoot.layer = obstacleLayer;

            float halfW = width * 0.5f;
            float halfH = height * 0.5f;
            float thickness = 2.0f;

            AddBoundaryBox(boundariesRoot, "Wall_Top", new Vector2(0f, halfH + thickness * 0.5f), new Vector2(width + thickness * 2f, thickness));
            AddBoundaryBox(boundariesRoot, "Wall_Bottom", new Vector2(0f, -halfH - thickness * 0.5f), new Vector2(width + thickness * 2f, thickness));
            AddBoundaryBox(boundariesRoot, "Wall_Left", new Vector2(-halfW - thickness * 0.5f, 0f), new Vector2(thickness, height));
            AddBoundaryBox(boundariesRoot, "Wall_Right", new Vector2(halfW + thickness * 0.5f, 0f), new Vector2(thickness, height));
        }

        private static void AddBoundaryBox(GameObject parent, string name, Vector2 offset, Vector2 size)
        {
            GameObject wall = new GameObject(name);
            wall.transform.SetParent(parent.transform, false);
            wall.transform.localPosition = offset;
            wall.layer = parent.layer;

            var box = wall.AddComponent<BoxCollider2D>();
            box.size = size;
            var rb = wall.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
        }

        private static void SaveAndRegisterPrefab(GameObject rootObj, string addressKey, string groupName)
        {
            string prefabPath = $"{PREFAB_DIR}/{addressKey}.prefab";
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(rootObj, prefabPath);
            Object.DestroyImmediate(rootObj);

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings != null)
            {
                string guid = AssetDatabase.AssetPathToGUID(prefabPath);
                var group = settings.FindGroup(groupName) ?? settings.DefaultGroup;
                var entry = settings.CreateOrMoveEntry(guid, group, false, false);
                entry.address = addressKey;
                entry.SetLabel("Map", true, true);
                EditorUtility.SetDirty(settings);
            }
        }
        #endregion
    }
}

