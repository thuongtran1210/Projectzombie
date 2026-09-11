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

            // Sinh 4 bản đồ chi tiết (bao gồm cả Map_SanDinhLangCo mặc định)
            BuildSanDinhLangCoMap(spriteDict);
            BuildBambooForestMap(spriteDict);
            BuildAncientCitadelMap(spriteDict);
            BuildCinnabarSwampMap(spriteDict);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=#00FF88>[RemoteStagesPrefabGenerator] HOÀN TẤT NÂNG CẤP MỸ THUẬT TOÀN BỘ BẢN ĐỒ TILEMAP 2.5D!</color>");
            EditorUtility.DisplayDialog("Nâng Cấp Tilemap Thành Công", "Đã kiến tạo lại toàn bộ bản đồ (Sân Đình Làng Cổ, Rừng Trúc, Cổ Thành, Đầm Lầy) với đầy đủ thảm cỏ xanh, hồ sen ngọc bích, tường thành đá và ranh giới chiến đấu!", "OK");
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

        private static Tile CreateTile(Sprite sprite)
        {
            if (sprite == null) return null;
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            tile.color = Color.white; // Luôn giữ nguyên vẹn 100% màu sắc gốc của Texture Atlas
            return tile;
        }

        #region --- BẢN ĐỒ MẶC ĐỊNH: SÂN ĐÌNH LÀNG CỔ (SAN DINH LANG CO) ---
        private static void BuildSanDinhLangCoMap(Dictionary<string, Sprite> sprites)
        {
            string mapName = "Map_SanDinhLangCo";
            var (root, ground, decals, obstacles) = CreateMapHierarchy(mapName);

            var tGrassOuter = CreateTile(GetSprite(sprites, "Tile_4_7")); // Thảm cỏ xanh sẫm nền ngoài
            var tBrickCourtyard = CreateTile(GetSprite(sprites, "Tile_0_0")); // Sân gạch nung Bát Tràng ở giữa
            var tBrickPattern = CreateTile(GetSprite(sprites, "Tile_1_0")); // Gạch nung rêu phong điểm xuyết
            var tWaterPond = CreateTile(GetSprite(sprites, "Tile_4_0")); // Nước ao sen xanh ngọc
            var tLotusDecal = CreateTile(GetSprite(sprites, "Tile_5_0")); // Hoa sen & lá sen
            var tWallStone = CreateTile(GetSprite(sprites, "Tile_0_2")); // Tường thành đá cổ
            var tPillar = CreateTile(GetSprite(sprites, "Tile_2_2")); // Trụ đá phong ấn

            int halfSize = 24;
            for (int x = -halfSize; x <= halfSize; x++)
            {
                for (int y = -halfSize; y <= halfSize; y++)
                {
                    // Vùng ngoài là thảm cỏ xanh đêm
                    ground.SetTile(new Vector3Int(x, y, 0), tGrassOuter);

                    // Sân Đình Gạch Cổ Bát Tràng ở trung tâm (-10 đến 10)
                    if (Mathf.Abs(x) <= 10 && Mathf.Abs(y) <= 10)
                    {
                        var brickTile = ((x + y) % 3 == 0) ? tBrickPattern : tBrickCourtyard;
                        ground.SetTile(new Vector3Int(x, y, 0), brickTile);
                    }

                    // 2 Ao sen xanh ngọc bích ở 2 góc sân
                    float distTopLeft = Vector2.Distance(new Vector2(x, y), new Vector2(-15, 15));
                    float distBottomRight = Vector2.Distance(new Vector2(x, y), new Vector2(15, -15));
                    if (distTopLeft < 4.5f || distBottomRight < 4.5f)
                    {
                        ground.SetTile(new Vector3Int(x, y, 0), tWaterPond);
                        if ((x * 3 + y * 7) % 2 == 0)
                        {
                            decals.SetTile(new Vector3Int(x, y, 0), tLotusDecal);
                        }
                    }
                }
            }

            // Tường thành đá phòng thủ ngắt quãng bảo vệ sân đình
            for (int x = -11; x <= 11; x++)
            {
                if (Mathf.Abs(x) >= 4 && Mathf.Abs(x) <= 9)
                {
                    obstacles.SetTile(new Vector3Int(x, 11, 0), tWallStone);
                    obstacles.SetTile(new Vector3Int(x, -11, 0), tWallStone);
                }
            }
            for (int y = -11; y <= 11; y++)
            {
                if (Mathf.Abs(y) >= 4 && Mathf.Abs(y) <= 9)
                {
                    obstacles.SetTile(new Vector3Int(11, y, 0), tWallStone);
                    obstacles.SetTile(new Vector3Int(-11, y, 0), tWallStone);
                }
            }

            // 4 Trụ đá trấn yểm 4 góc sân đình
            obstacles.SetTile(new Vector3Int(-7, 7, 0), tPillar);
            obstacles.SetTile(new Vector3Int(7, 7, 0), tPillar);
            obstacles.SetTile(new Vector3Int(-7, -7, 0), tPillar);
            obstacles.SetTile(new Vector3Int(7, -7, 0), tPillar);

            CreateMapBoundaryColliders(root, 48f, 48f);
            SaveAndRegisterPrefab(root, mapName, "Group_Core_Preload");
        }
        #endregion

        #region --- ẢI 1: RỪNG TRÚC ÂM TY (BAMBOO FOREST) ---
        private static void BuildBambooForestMap(Dictionary<string, Sprite> sprites)
        {
            string mapName = "Map_BambooForest";
            var (root, ground, decals, obstacles) = CreateMapHierarchy(mapName);

            // Nền đất cỏ sẫm tự nhiên (Hàng 6 & 7: y=64 và y=0 trong Atlas)
            var tGrassDark1 = CreateTile(GetSprite(sprites, "Tile_4_7")); // Cỏ xanh sẫm 1
            var tGrassDark2 = CreateTile(GetSprite(sprites, "Tile_5_7")); // Cỏ xanh sẫm 2
            var tDirtGround = CreateTile(GetSprite(sprites, "Tile_0_7")); // Đất bùn nâu
            var tGravelPath = CreateTile(GetSprite(sprites, "Tile_1_7")); // Lối mòn đất sỏi
            var tDecalFlora = CreateTile(GetSprite(sprites, "Tile_6_7")); // Cỏ hoa dại điểm xuyết
            var tObstacleBushes = CreateTile(GetSprite(sprites, "Tile_1_6")); // Bụi cây rậm / đá rêu
            var tStonePillar = CreateTile(GetSprite(sprites, "Tile_2_2")); // Trụ đá phong ấn cổ

            int halfSize = 22;
            for (int x = -halfSize; x <= halfSize; x++)
            {
                for (int y = -halfSize; y <= halfSize; y++)
                {
                    // Đan xen nền cỏ xanh sẫm tự nhiên
                    var baseGrass = ((x + y) % 2 == 0) ? tGrassDark1 : tGrassDark2;
                    ground.SetTile(new Vector3Int(x, y, 0), baseGrass);

                    // Con đường mòn đất sỏi uốn lượn hình chữ thập xuyên qua rừng trúc
                    if (Mathf.Abs(x) <= 2 || Mathf.Abs(y) <= 2)
                    {
                        ground.SetTile(new Vector3Int(x, y, 0), tGravelPath);
                    }
                    else if ((x * 7 + y * 13) % 9 == 0)
                    {
                        // Những khoảnh đất nâu loang lổ tự nhiên
                        ground.SetTile(new Vector3Int(x, y, 0), tDirtGround);
                    }
                    // Rải hoa cỏ dại lên lớp Decal
                    else if ((x * 3 + y * 11) % 5 == 0)
                    {
                        decals.SetTile(new Vector3Int(x, y, 0), tDecalFlora);
                    }
                }
            }

            // Chướng ngại vật: Bụi cây rậm và cụm đá phong ấn ở 4 góc và rải rác
            int[,] rockClusters = new int[,]
            {
                { -10, 8 }, { -9, 8 }, { -10, 7 },
                { 10, 8 }, { 9, 8 }, { 10, 7 },
                { -10, -8 }, { -9, -8 }, { -10, -7 },
                { 10, -8 }, { 9, -8 }, { 10, -7 },
                { -6, -6 }, { 6, 6 }, { -6, 6 }, { 6, -6 }
            };

            for (int i = 0; i < rockClusters.GetLength(0); i++)
            {
                int rx = rockClusters[i, 0];
                int ry = rockClusters[i, 1];
                obstacles.SetTile(new Vector3Int(rx, ry, 0), tObstacleBushes);
            }

            // 4 Trụ đá cổ trấn giữ
            obstacles.SetTile(new Vector3Int(-8, 0, 0), tStonePillar);
            obstacles.SetTile(new Vector3Int(8, 0, 0), tStonePillar);
            obstacles.SetTile(new Vector3Int(0, -8, 0), tStonePillar);
            obstacles.SetTile(new Vector3Int(0, 8, 0), tStonePillar);

            CreateMapBoundaryColliders(root, 44f, 44f);
            SaveAndRegisterPrefab(root, mapName, "Group_DLC_Stages_Remote");
        }
        #endregion

        #region --- ẢI 2: CỔ THÀNH ĐÔNG SƠN (ANCIENT CITADEL) ---
        private static void BuildAncientCitadelMap(Dictionary<string, Sprite> sprites)
        {
            string mapName = "Map_AncientCitadel";
            var (root, ground, decals, obstacles) = CreateMapHierarchy(mapName);

            // Phối cảnh: Vùng ngoài là nền cỏ xanh sẫm + bờ bao đá, ở giữa là Sân Đình Gạch Đỏ Bát Tràng tráng lệ
            var tGrassOuter = CreateTile(GetSprite(sprites, "Tile_4_7")); // Cỏ xanh sẫm nền ngoài
            var tBrickCourtyard = CreateTile(GetSprite(sprites, "Tile_0_0")); // Sân gạch nung Bát Tràng
            var tBrickPattern = CreateTile(GetSprite(sprites, "Tile_1_0")); // Gạch nung có hoa văn điểm nhấn
            var tWallStone = CreateTile(GetSprite(sprites, "Tile_0_2")); // Tường thành đá
            var tPillar = CreateTile(GetSprite(sprites, "Tile_2_2")); // Bia đá / trụ đá cổ
            var tDecalMoss = CreateTile(GetSprite(sprites, "Tile_6_7")); // Rêu xanh viền chân thành

            int halfSize = 24;
            for (int x = -halfSize; x <= halfSize; x++)
            {
                for (int y = -halfSize; y <= halfSize; y++)
                {
                    // Mặc định bên ngoài là thảm cỏ thành quách
                    ground.SetTile(new Vector3Int(x, y, 0), tGrassOuter);

                    // Sân Đình Gạch Cổ ở trung tâm thành (Khu vực -12 đến 12)
                    if (Mathf.Abs(x) <= 12 && Mathf.Abs(y) <= 12)
                    {
                        var brickTile = ((x + y) % 3 == 0) ? tBrickPattern : tBrickCourtyard;
                        ground.SetTile(new Vector3Int(x, y, 0), brickTile);

                        // Viền rêu mốc quanh mép sân gạch
                        if (Mathf.Abs(x) == 12 || Mathf.Abs(y) == 12)
                        {
                            decals.SetTile(new Vector3Int(x, y, 0), tDecalMoss);
                        }
                    }
                }
            }

            // Tường thành đá phòng thủ ngắt quãng bao bọc quanh sân đình
            for (int x = -13; x <= 13; x++)
            {
                if (Mathf.Abs(x) >= 4 && Mathf.Abs(x) <= 10)
                {
                    obstacles.SetTile(new Vector3Int(x, 13, 0), tWallStone);
                    obstacles.SetTile(new Vector3Int(x, -13, 0), tWallStone);
                }
            }
            for (int y = -13; y <= 13; y++)
            {
                if (Mathf.Abs(y) >= 4 && Mathf.Abs(y) <= 10)
                {
                    obstacles.SetTile(new Vector3Int(13, y, 0), tWallStone);
                    obstacles.SetTile(new Vector3Int(-13, y, 0), tWallStone);
                }
            }

            // 4 Trụ đá phong ấn trấn yểm 4 góc quảng trường
            obstacles.SetTile(new Vector3Int(-8, 8, 0), tPillar);
            obstacles.SetTile(new Vector3Int(8, 8, 0), tPillar);
            obstacles.SetTile(new Vector3Int(-8, -8, 0), tPillar);
            obstacles.SetTile(new Vector3Int(8, -8, 0), tPillar);

            CreateMapBoundaryColliders(root, 48f, 48f);
            SaveAndRegisterPrefab(root, mapName, "Group_DLC_Stages_Remote");
        }
        #endregion

        #region --- ẢI 3: ĐẦM LẦY THẦN SA (CINNABAR SWAMP) ---
        private static void BuildCinnabarSwampMap(Dictionary<string, Sprite> sprites)
        {
            string mapName = "Map_CinnabarSwamp";
            var (root, ground, decals, obstacles) = CreateMapHierarchy(mapName);

            // Nền đầm lầy: Đất bùn nâu sẫm + ao nước xanh ngọc sâu + lá sen hồng
            var tMudGround = CreateTile(GetSprite(sprites, "Tile_0_7")); // Đất bùn đầm lầy
            var tWaterPond = CreateTile(GetSprite(sprites, "Tile_4_0")); // Nước ao xanh ngọc (y=448, x=256)
            var tLotusDecal = CreateTile(GetSprite(sprites, "Tile_5_0")); // Lá sen hồng trên mặt nước (y=448, x=320)
            var tGrassPatch = CreateTile(GetSprite(sprites, "Tile_4_7")); // Thảm cỏ rêu quanh bờ ao
            var tObstacleRock = CreateTile(GetSprite(sprites, "Tile_0_2")); // Bãi đá trơn trượt

            int halfSize = 24;
            for (int x = -halfSize; x <= halfSize; x++)
            {
                for (int y = -halfSize; y <= halfSize; y++)
                {
                    // Nền đầm lầy đất bùn ẩm ướt
                    ground.SetTile(new Vector3Int(x, y, 0), tMudGround);

                    // 4 Vùng ao sen tự nhiên ở 4 góc và 1 hồ sen nhỏ ở góc dưới
                    float distTopLeft = Vector2.Distance(new Vector2(x, y), new Vector2(-11, 11));
                    float distBottomRight = Vector2.Distance(new Vector2(x, y), new Vector2(11, -11));
                    float distTopRight = Vector2.Distance(new Vector2(x, y), new Vector2(12, 12));

                    if (distTopLeft < 5.5f || distBottomRight < 5.5f || distTopRight < 4.5f)
                    {
                        // Mặt hồ nước ngọc bích
                        ground.SetTile(new Vector3Int(x, y, 0), tWaterPond);

                        // Thả lá sen và hoa sen bồng bềnh
                        if ((x * 3 + y * 7) % 2 == 0)
                        {
                            decals.SetTile(new Vector3Int(x, y, 0), tLotusDecal);
                        }
                    }
                    else if (distTopLeft < 7.0f || distBottomRight < 7.0f || distTopRight < 6.0f)
                    {
                        // Bờ cỏ rêu bao bọc quanh hồ nước
                        ground.SetTile(new Vector3Int(x, y, 0), tGrassPatch);
                    }
                }
            }

            // Chướng ngại vật: Bãi đá cổ bao quanh mép hồ đầm lầy
            Vector2Int[] rockPositions = new Vector2Int[]
            {
                new(-14, 11), new(-13, 12), new(-12, 14), new(-9, 14),
                new(14, -11), new(13, -12), new(12, -14), new(9, -14),
                new(-5, 0), new(5, 0), new(0, -5), new(0, 5)
            };

            foreach (var pos in rockPositions)
            {
                obstacles.SetTile(new Vector3Int(pos.x, pos.y, 0), tObstacleRock);
            }

            CreateMapBoundaryColliders(root, 48f, 48f);
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

