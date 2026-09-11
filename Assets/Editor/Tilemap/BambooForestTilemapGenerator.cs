using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.Tilemaps;
using ProjectZombie.Features.Maps;

namespace Projectzombie.Editor.TilemapTools
{
    /// <summary>
    /// Công cụ sinh cấu trúc Prefab Map Rừng Trúc Âm Ty (Map_BambooForest.prefab) 
    /// theo chuẩn 2.5D Arena 4 tầng, tự động đăng ký Addressables Key 'Map_BambooForest'.
    /// </summary>
    public class BambooForestTilemapGenerator : EditorWindow
    {
        private const string PREFAB_DIR = "Assets/_Prefabs/Maps";
        private const string PREFAB_PATH = "Assets/_Prefabs/Maps/Map_BambooForest.prefab";
        private const string ADDRESS_KEY = "Map_BambooForest";

        [MenuItem("Tools/ProjectZombie/Maps/Generate Bamboo Forest Map Prefab (Stage 1)", priority = 210)]
        public static void GenerateBambooForestPrefab()
        {
            if (!Directory.Exists(PREFAB_DIR))
            {
                Directory.CreateDirectory(PREFAB_DIR);
                AssetDatabase.Refresh();
            }

            // 1. Tạo Root Grid Container
            GameObject rootObj = new GameObject(ADDRESS_KEY);
            Grid grid = rootObj.AddComponent<Grid>();
            grid.cellSize = new Vector3(1f, 1f, 0f);

            // 2. Tầng 0: BaseGround (Đất rừng u uất 64x64m)
            GameObject baseObj = new GameObject("Tilemap_BaseGround");
            baseObj.transform.SetParent(rootObj.transform, false);
            Tilemap baseTilemap = baseObj.AddComponent<Tilemap>();
            TilemapRenderer baseRenderer = baseObj.AddComponent<TilemapRenderer>();
            baseRenderer.sortingLayerName = "Background";
            baseRenderer.sortingOrder = -1;

            // 3. Tầng 1: Ground (Đường mòn đá rêu & đất nện 36x36m)
            GameObject groundObj = new GameObject("Tilemap_Ground");
            groundObj.transform.SetParent(rootObj.transform, false);
            Tilemap groundTilemap = groundObj.AddComponent<Tilemap>();
            TilemapRenderer groundRenderer = groundObj.AddComponent<TilemapRenderer>();
            groundRenderer.sortingLayerName = "Tilemap_Ground";
            groundRenderer.sortingOrder = 0;

            // 4. Tầng 2: Decals (Lá tre rụng, rêu phong, vũng nước)
            GameObject decalObj = new GameObject("Tilemap_Decals");
            decalObj.transform.SetParent(rootObj.transform, false);
            Tilemap decalTilemap = decalObj.AddComponent<Tilemap>();
            TilemapRenderer decalRenderer = decalObj.AddComponent<TilemapRenderer>();
            decalRenderer.sortingLayerName = "Tilemap_Decals";
            decalRenderer.sortingOrder = 1;

            // 5. Tầng 3: Obstacles (Rặng tre ma, vách đá có Collider)
            GameObject obstacleObj = new GameObject("Tilemap_Obstacles");
            obstacleObj.transform.SetParent(rootObj.transform, false);
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
            if (rb != null) rb.bodyType = RigidbodyType2D.Static;

            // 6. Nạp Tiles từ Atlas Sân Đình & Cổ Tự
            string texturePath = "Assets/Art/Tilemaps/SanDinhLangCo/Tileset_SanDinhLangCo.png";
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);

            Sprite earthBase = null, earthPebble = null, earthRoot = null;
            Sprite mossForest = null, mossGrass = null;
            Sprite stoneRoad = null, mossStone = null, wallBamboo = null;

            foreach (var asset in assets)
            {
                if (asset is Sprite s)
                {
                    if (s.name == "Tile_0_6") earthBase = s;
                    else if (s.name == "Tile_1_6") earthPebble = s;
                    else if (s.name == "Tile_2_6") earthRoot = s;
                    else if (s.name == "Tile_4_6") mossForest = s;
                    else if (s.name == "Tile_5_6") mossGrass = s;
                    else if (s.name == "Tile_1_0") mossStone = s;
                    else if (s.name == "Tile_0_0") stoneRoad = s;
                    else if (s.name == "Tile_0_2") wallBamboo = s;
                }
            }

            // Vẽ Tầng 0 (Đất rừng u ám 64x64m)
            if (earthBase != null)
            {
                Tile tEarth = CreateTile(earthBase);
                Tile tPebble = earthPebble != null ? CreateTile(earthPebble) : tEarth;
                Tile tRoot = earthRoot != null ? CreateTile(earthRoot) : tEarth;
                Tile tMoss = mossForest != null ? CreateTile(mossForest) : tEarth;
                Tile tGrass = mossGrass != null ? CreateTile(mossGrass) : tMoss;

                int baseRadius = 32;
                for (int x = -baseRadius; x <= baseRadius; x++)
                {
                    for (int y = -baseRadius; y <= baseRadius; y++)
                    {
                        int seed = (x * 17 + y * 31) % 12;
                        Tile picked = seed == 0 ? tPebble : (seed == 1 ? tRoot : (seed > 8 ? tGrass : tMoss));
                        baseTilemap.SetTile(new Vector3Int(x, y, 0), picked);
                    }
                }
            }

            // Vẽ Tầng 1 (Sàn di chuyển chính - Đường mòn rêu phong 36x36m)
            if (mossStone != null || stoneRoad != null)
            {
                Tile tRoad = CreateTile(stoneRoad != null ? stoneRoad : mossStone);
                Tile tMoss = CreateTile(mossStone != null ? mossStone : stoneRoad);

                int arenaRadius = 18;
                for (int x = -arenaRadius; x <= arenaRadius; x++)
                {
                    for (int y = -arenaRadius; y <= arenaRadius; y++)
                    {
                        int seed = (x * 13 + y * 29) % 8;
                        groundTilemap.SetTile(new Vector3Int(x, y, 0), seed == 0 ? tRoad : tMoss);
                    }
                }
            }

            // Vẽ Tầng 3: Rặng tre chướng ngại vật (4 góc và các cột tre cụm)
            if (wallBamboo != null)
            {
                Tile tBamboo = CreateTile(wallBamboo);
                tBamboo.colliderType = Tile.ColliderType.Grid;

                // Tạo vài cụm tre tự nhiên cản lối (tạo không gian combat chiến thuật)
                Vector2Int[] bambooClusters = new Vector2Int[]
                {
                    new Vector2Int(-10, 8), new Vector2Int(-9, 8), new Vector2Int(-10, 7),
                    new Vector2Int(10, -8), new Vector2Int(9, -8), new Vector2Int(10, -7),
                    new Vector2Int(-12, -10), new Vector2Int(12, 10)
                };

                foreach (var pos in bambooClusters)
                {
                    obstacleTilemap.SetTile(new Vector3Int(pos.x, pos.y, 0), tBamboo);
                }
            }

            // 7. Tạo 4 Bức Tường Ranh Giới (Map Boundaries)
            CreateMapBoundaryColliders(rootObj, 36f, 36f);

            // 8. Lưu thành Prefab
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(rootObj, PREFAB_PATH);
            Object.DestroyImmediate(rootObj);

            // 9. Đăng ký Addressable Key
            RegisterToAddressables(savedPrefab, ADDRESS_KEY);

            // 10. Cập nhật StageDefinitionSO của Stage 1
            UpdateStage1Definition();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = savedPrefab;
            Debug.Log($"<color=#00FF88>[BambooForestTilemapGenerator]</color> Đã tạo thành công Prefab Rừng Trúc tại '{PREFAB_PATH}' và gán Addressable Key '{ADDRESS_KEY}'!");
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

        private static Tile CreateTile(Sprite sprite)
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = sprite;
            return tile;
        }

        private static void RegisterToAddressables(GameObject prefab, string addressKey)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) return;

            string guid = AssetDatabase.AssetPathToGUID(PREFAB_PATH);
            var group = settings.FindGroup("Group_Core_Preload") ?? settings.DefaultGroup;

            var entry = settings.CreateOrMoveEntry(guid, group, false, false);
            entry.address = addressKey;
            entry.SetLabel("Map", true, true);

            EditorUtility.SetDirty(settings);
        }

        private static void UpdateStage1Definition()
        {
            string stagePath = "Assets/_Data/Levels/Stages/Stage_01_BambooForest.asset";
            var stage = AssetDatabase.LoadAssetAtPath<StageDefinitionSO>(stagePath);
            if (stage != null)
            {
                stage.mapPrefabAddress = ADDRESS_KEY;
                EditorUtility.SetDirty(stage);
            }

            string resStagePath = "Assets/Resources/Levels/Stage_01_BambooForest.asset";
            var resStage = AssetDatabase.LoadAssetAtPath<StageDefinitionSO>(resStagePath);
            if (resStage != null)
            {
                resStage.mapPrefabAddress = ADDRESS_KEY;
                EditorUtility.SetDirty(resStage);
            }
        }
    }
}
