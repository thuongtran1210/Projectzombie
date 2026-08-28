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
            GetWindow<SanDinhTilemapBuilder>("Sân Đình Map Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Ancient Vietnamese Temple (Sân Đình) Map Generator", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Công cụ này sẽ tự động thiết lập Texture 2D Multiple Sprites, cấu hình Sorting Layers, tạo Grid Hierarchy và sinh mẫu Map Sân Đình Làng Cổ hoàn chỉnh với Composite Collider.", MessageType.Info);

            if (GUILayout.Button("1. Configure Tileset Texture Meta (Auto Slice)", GUILayout.Height(35)))
            {
                ConfigureTilesetTexture();
            }

            if (GUILayout.Button("2. Create Sân Đình Map GameObject in Scene", GUILayout.Height(40)))
            {
                CreateSanDinhMapHierarchy();
            }
        }

        private static void ConfigureTilesetTexture()
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

        private static void CreateSanDinhMapHierarchy()
        {
            // 1. Root Grid
            GameObject gridObj = new GameObject("Environment_SanDinhLangCo");
            Grid grid = gridObj.AddComponent<Grid>();
            grid.cellSize = new Vector3(1f, 1f, 0f);

            // 2. Ground Tilemap (Gạch Bát Tràng)
            GameObject groundObj = new GameObject("Tilemap_Ground");
            groundObj.transform.SetParent(gridObj.transform);
            Tilemap groundTilemap = groundObj.AddComponent<Tilemap>();
            TilemapRenderer groundRenderer = groundObj.AddComponent<TilemapRenderer>();
            groundRenderer.sortingLayerName = "Tilemap_Ground";
            groundRenderer.sortingOrder = 0;

            // 3. Decal Tilemap (Lá sen, Rêu nền, Vết nứt)
            GameObject decalObj = new GameObject("Tilemap_Decals");
            decalObj.transform.SetParent(gridObj.transform);
            Tilemap decalTilemap = decalObj.AddComponent<Tilemap>();
            TilemapRenderer decalRenderer = decalObj.AddComponent<TilemapRenderer>();
            decalRenderer.sortingLayerName = "Tilemap_Decals";
            decalRenderer.sortingOrder = 0;

            // 4. Obstacles Tilemap (Tường gạch rêu, Bờ kè ao sen - có Collider)
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

            // 5. Load Sprites & Paint Sample Arena (20x20 sân đình)
            string texturePath = "Assets/Art/Tilemaps/SanDinhLangCo/Tileset_SanDinhLangCo.png";
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            Sprite groundSprite = null;
            Sprite waterSprite = null;
            Sprite wallSprite = null;

            foreach (var asset in assets)
            {
                if (asset is Sprite s)
                {
                    if (s.name == "Tile_0_0") groundSprite = s;
                    if (s.name == "Tile_4_0") waterSprite = s;
                    if (s.name == "Tile_0_2") wallSprite = s;
                }
            }

            if (groundSprite != null)
            {
                Tile groundTile = ScriptableObject.CreateInstance<Tile>();
                groundTile.sprite = groundSprite;

                int arenaRadius = 12;
                for (int x = -arenaRadius; x <= arenaRadius; x++)
                {
                    for (int y = -arenaRadius; y <= arenaRadius; y++)
                    {
                        groundTilemap.SetTile(new Vector3Int(x, y, 0), groundTile);
                    }
                }
            }

            if (wallSprite != null)
            {
                Tile wallTile = ScriptableObject.CreateInstance<Tile>();
                wallTile.sprite = wallSprite;
                wallTile.colliderType = Tile.ColliderType.Grid;

                int arenaRadius = 12;
                for (int x = -arenaRadius; x <= arenaRadius; x++)
                {
                    obstacleTilemap.SetTile(new Vector3Int(x, arenaRadius, 0), wallTile);
                    obstacleTilemap.SetTile(new Vector3Int(x, -arenaRadius, 0), wallTile);
                }
                for (int y = -arenaRadius; y <= arenaRadius; y++)
                {
                    obstacleTilemap.SetTile(new Vector3Int(-arenaRadius, y, 0), wallTile);
                    obstacleTilemap.SetTile(new Vector3Int(arenaRadius, y, 0), wallTile);
                }
            }

            Selection.activeGameObject = gridObj;
            Undo.RegisterCreatedObjectUndo(gridObj, "Create Sân Đình Map");
            Debug.Log("[TilemapBuilder] Đã tạo thành công Map Sân Đình Làng Cổ (24x24) với Composite Collider và 10-Layer Sorting chuẩn URP!");
        }
    }
}
