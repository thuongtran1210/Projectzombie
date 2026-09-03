using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Projectzombie.Editor.TilemapTools
{
    public class SanDinhTilemapBuilder : EditorWindow
    {
        [MenuItem("ProjectZombie/Tilemap/Build Sân Đình Làng Cổ Map")]
        public static void ShowWindow()
        {
            var window = GetWindow<SanDinhTilemapBuilder>("Sân Đình Map Builder");
            window.minSize = new Vector2(400, 320);
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("🏛️ Sân Đình Làng Cổ — 2.5D Arena Generator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Công cụ tự động hóa 4 tầng:\n" +
                "1. Auto Slice Tileset Atlas 64 ô (Đất nện, Gạch Bát Tràng, Ao sen, Tường rêu).\n" +
                "2. Dựng Grid 5 tầng: Tilemap_BaseGround (64x64m), Tilemap_Ground (36x36m), Tilemap_Decals, Tilemap_Obstacles và 4 Bức Tường Ranh Giới (Map Boundaries).",
                MessageType.Info);

            GUILayout.Space(10);
            if (GUILayout.Button("1. Configure Tileset Texture Meta (Auto Slice 64 Tiles)", GUILayout.Height(38)))
            {
                ConfigureTilesetTexture();
            }

            GUILayout.Space(6);
            GUI.backgroundColor = new Color(0.4f, 0.9f, 0.5f);
            if (GUILayout.Button("2. Build Complete 36x36 Arena in Scene (1-Click)", GUILayout.Height(45)))
            {
                BuildCompleteArenaInScene();
            }
            GUI.backgroundColor = Color.white;
        }

        public static void ConfigureTilesetTexture()
        {
            string texturePath = "Assets/Art/Tilemaps/SanDinhLangCo/Tileset_SanDinhLangCo.png";
            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError($"[TilemapBuilder] Không tìm thấy Texture tại {texturePath}");
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 64;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.sRGBTexture = true;

            int tileSize = 64;
            int cols = 8;
            int rows = 8;

            SpriteMetaData[] metaData = new SpriteMetaData[cols * rows];
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int index = r * cols + c;
                    metaData[index] = new SpriteMetaData
                    {
                        name = $"Tile_{c}_{r}",
                        rect = new Rect(c * tileSize, (rows - 1 - r) * tileSize, tileSize, tileSize),
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f)
                    };
                }
            }

#pragma warning disable CS0618
            importer.spritesheet = metaData;
#pragma warning restore CS0618
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
            AssetDatabase.Refresh();

            Debug.Log($"[TilemapBuilder] Đã cắt thành công {cols * rows} sprites (64x64) cho Tileset_SanDinhLangCo.png!");
        }

        public static void BuildCompleteArenaInScene()
        {
            // 0. Cấu hình Slice trước nếu cần
            ConfigureTilesetTexture();

            // 1. Dọn dẹp map cũ nếu có
            GameObject oldGrid = GameObject.Find("Environment_SanDinhLangCo");
            if (oldGrid != null)
            {
                Undo.DestroyObjectImmediate(oldGrid);
            }

            // 2. Tạo Root Grid
            GameObject gridObj = new GameObject("Environment_SanDinhLangCo");
            Grid grid = gridObj.AddComponent<Grid>();
            grid.cellSize = new Vector3(1f, 1f, 0f);

            // 3. Tạo Tầng 0: Tilemap_BaseGround (Phủ rộng 64x64m — Triệt tiêu 100% khoảng trống Unity)
            GameObject baseObj = new GameObject("Tilemap_BaseGround");
            baseObj.transform.SetParent(gridObj.transform);
            Tilemap baseTilemap = baseObj.AddComponent<Tilemap>();
            TilemapRenderer baseRenderer = baseObj.AddComponent<TilemapRenderer>();
            baseRenderer.sortingLayerName = "Background";
            baseRenderer.sortingOrder = -1;

            // 4. Tạo Tầng 1: Tilemap_Ground (Sân Đình Gạch Bát Tràng 36x36m)
            GameObject groundObj = new GameObject("Tilemap_Ground");
            groundObj.transform.SetParent(gridObj.transform);
            Tilemap groundTilemap = groundObj.AddComponent<Tilemap>();
            TilemapRenderer groundRenderer = groundObj.AddComponent<TilemapRenderer>();
            groundRenderer.sortingLayerName = "Tilemap_Ground";
            groundRenderer.sortingOrder = 0;

            // 5. Tạo Tầng 2: Tilemap_Decals (Hoa Sen, Rêu Phong, Thảm Cói)
            GameObject decalObj = new GameObject("Tilemap_Decals");
            decalObj.transform.SetParent(gridObj.transform);
            Tilemap decalTilemap = decalObj.AddComponent<Tilemap>();
            TilemapRenderer decalRenderer = decalObj.AddComponent<TilemapRenderer>();
            decalRenderer.sortingLayerName = "Tilemap_Decals";
            decalRenderer.sortingOrder = 1;

            // 6. Tạo Tầng 3: Tilemap_Obstacles (Tường Rêu, Bờ Kè Ao Sen, Đỉnh Đồng có Collider)
            GameObject obstacleObj = new GameObject("Tilemap_Obstacles");
            obstacleObj.transform.SetParent(gridObj.transform);
            int obstacleLayer = LayerMask.NameToLayer("Obstacle");
            if (obstacleLayer != -1) obstacleObj.layer = obstacleLayer;

            Tilemap obstacleTilemap = obstacleObj.AddComponent<Tilemap>();
            TilemapRenderer obstacleRenderer = obstacleObj.AddComponent<TilemapRenderer>();
            obstacleRenderer.sortingLayerName = "Entities";
            obstacleRenderer.sortingOrder = 0;

            TilemapCollider2D tilemapCollider = obstacleObj.AddComponent<TilemapCollider2D>();
            tilemapCollider.usedByComposite = true;

            CompositeCollider2D compositeCollider = obstacleObj.AddComponent<CompositeCollider2D>();
            compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;

            Rigidbody2D rb = obstacleObj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Static;
            }

            // 7. Load Sprites từ Atlas
            string texturePath = "Assets/Art/Tilemaps/SanDinhLangCo/Tileset_SanDinhLangCo.png";
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);

            Sprite brickClean = null, brickMoss = null, brickCracked = null, brickWet = null;
            Sprite earthBase = null, earthPebble = null, earthRoot = null;
            Sprite mossForest = null, mossGrass = null;
            Sprite waterTile = null, lotusTile = null, stoneKerb = null, wallTile = null;
            Sprite matTile = null, bronzeUrnTop = null, bronzeUrnBase = null;

            foreach (var asset in assets)
            {
                if (asset is Sprite s)
                {
                    if (s.name == "Tile_0_0") brickClean = s;
                    else if (s.name == "Tile_1_0") brickMoss = s;
                    else if (s.name == "Tile_2_0") brickCracked = s;
                    else if (s.name == "Tile_3_0") brickWet = s;
                    else if (s.name == "Tile_4_0") waterTile = s;
                    else if (s.name == "Tile_5_0") lotusTile = s;
                    else if (s.name == "Tile_6_0") stoneKerb = s;
                    else if (s.name == "Tile_0_2") wallTile = s;
                    else if (s.name == "Tile_0_4") matTile = s;
                    else if (s.name == "Tile_4_2") bronzeUrnTop = s;
                    else if (s.name == "Tile_4_3") bronzeUrnBase = s;
                    else if (s.name == "Tile_0_6") earthBase = s;
                    else if (s.name == "Tile_1_6") earthPebble = s;
                    else if (s.name == "Tile_2_6") earthRoot = s;
                    else if (s.name == "Tile_4_6") mossForest = s;
                    else if (s.name == "Tile_5_6") mossGrass = s;
                }
            }

            // --- VẼ TẦNG 0: NỀN ĐẤT NỆN HOÀNG THỔ & RỪNG TRÚC (64x64m) ---
            if (earthBase != null)
            {
                Tile tEarth = CreateTile(earthBase);
                Tile tPebble = earthPebble != null ? CreateTile(earthPebble) : tEarth;
                Tile tRoot = earthRoot != null ? CreateTile(earthRoot) : tEarth;
                Tile tMoss = mossForest != null ? CreateTile(mossForest) : tEarth;
                Tile tGrass = mossGrass != null ? CreateTile(mossGrass) : tMoss;

                int baseRadius = 32; // 64x64m
                for (int x = -baseRadius; x <= baseRadius; x++)
                {
                    for (int y = -baseRadius; y <= baseRadius; y++)
                    {
                        int distFromCenter = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                        if (distFromCenter > 20)
                        {
                            // Vùng viền ngoài xa: Thảm Rêu & Cỏ Rừng Đêm
                            int seed = (x * 13 + y * 29) % 10;
                            baseTilemap.SetTile(new Vector3Int(x, y, 0), seed == 0 ? tGrass : tMoss);
                        }
                        else
                        {
                            // Vùng đệm sát sàn đình: Đất Nện Hoàng Thổ
                            int seed = (x * 17 + y * 31) % 12;
                            Tile picked = seed == 0 ? tPebble : (seed == 1 ? tRoot : tEarth);
                            baseTilemap.SetTile(new Vector3Int(x, y, 0), picked);
                        }
                    }
                }
            }

            // --- VẼ TẦNG 1: SÂN ĐÌNH GẠCH BÁT TRÀNG (36x36m) ---
            if (brickClean != null)
            {
                Tile tClean = CreateTile(brickClean);
                Tile tMoss = brickMoss != null ? CreateTile(brickMoss) : tClean;
                Tile tCracked = brickCracked != null ? CreateTile(brickCracked) : tClean;
                Tile tWet = brickWet != null ? CreateTile(brickWet) : tClean;

                int arenaRadius = 18; // 36x36m
                for (int x = -arenaRadius; x <= arenaRadius; x++)
                {
                    for (int y = -arenaRadius; y <= arenaRadius; y++)
                    {
                        int seed = (x * 19 + y * 23) % 16;
                        Tile picked = tClean;
                        if (seed == 0) picked = tMoss;
                        else if (seed == 1) picked = tCracked;
                        else if (seed == 2) picked = tWet;

                        groundTilemap.SetTile(new Vector3Int(x, y, 0), picked);
                    }
                }
            }

            // --- VẼ TẦNG 2 & 3: TƯỜNG RÊU & AO SEN GÓC ĐÌNH ---
            if (wallTile != null)
            {
                Tile tWall = CreateTile(wallTile);
                tWall.colliderType = Tile.ColliderType.Grid;

                int arenaRadius = 18;
                // Đắp tường bao góc sân đình cổ
                for (int x = -arenaRadius; x <= -arenaRadius + 6; x++)
                {
                    obstacleTilemap.SetTile(new Vector3Int(x, arenaRadius, 0), tWall);
                    obstacleTilemap.SetTile(new Vector3Int(x, -arenaRadius, 0), tWall);
                }
                for (int x = arenaRadius - 6; x <= arenaRadius; x++)
                {
                    obstacleTilemap.SetTile(new Vector3Int(x, arenaRadius, 0), tWall);
                    obstacleTilemap.SetTile(new Vector3Int(x, -arenaRadius, 0), tWall);
                }
            }

            // Đỉnh đồng trầm hương ở trung tâm góc trên
            if (bronzeUrnBase != null && bronzeUrnTop != null)
            {
                Tile tUrnBase = CreateTile(bronzeUrnBase);
                tUrnBase.colliderType = Tile.ColliderType.Grid;
                Tile tUrnTop = CreateTile(bronzeUrnTop);

                obstacleTilemap.SetTile(new Vector3Int(0, 10, 0), tUrnBase);
                decalTilemap.SetTile(new Vector3Int(0, 11, 0), tUrnTop);
            }

            // --- TẠO 4 BỨC TƯỜNG RANH GIỚI (Map Boundaries) ---
            CreateMapBoundaryColliders(gridObj, 36f, 36f);

            // --- ĐỒNG BỘ CAMERA CLEAR COLOR SANG NỀN NÂU ĐẤT CỔ ---
            Camera cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.08f, 0.06f, 0.05f, 1f);
            }

            Selection.activeGameObject = gridObj;
            Undo.RegisterCreatedObjectUndo(gridObj, "Build Complete Arena");
            Debug.Log("✅ [TilemapBuilder] Đã khôi phục thành công Đấu Trường Sân Đình Làng Cổ V1!");
        }

        private static void CreateMapBoundaryColliders(GameObject parent, float width, float height)
        {
            GameObject boundariesRoot = new GameObject("Map_Boundaries");
            boundariesRoot.transform.SetParent(parent.transform);
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
            wall.transform.SetParent(parent.transform);
            wall.transform.localPosition = offset;
            wall.layer = parent.layer;

            var box = wall.AddComponent<BoxCollider2D>();
            box.size = size;
            var rb = wall.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
        }

        private static Tile CreateTile(Sprite sprite)
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            return tile;
        }
    }
}
