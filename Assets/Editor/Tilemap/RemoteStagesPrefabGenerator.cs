using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Projectzombie.Editor.TilemapTools
{
    /// <summary>
    /// Tool tự động tạo Prefab placeholder cho các Ải DLC từ xa (Stage 2: Cổ Thành, Stage 3: Đầm Lầy)
    /// và đăng ký Addressables Key vào nhóm Group_DLC_Stages_Remote.
    /// </summary>
    public static class RemoteStagesPrefabGenerator
    {
        private const string PREFAB_DIR = "Assets/_Prefabs/Maps";

        [MenuItem("Tools/ProjectZombie/Maps/Generate Remote DLC Map Prefabs (Stage 2 & 3)", priority = 215)]
        public static void GenerateAllDlcMapPrefabs()
        {
            if (!Directory.Exists(PREFAB_DIR))
            {
                Directory.CreateDirectory(PREFAB_DIR);
                AssetDatabase.Refresh();
            }

            GenerateSingleMapPrefab("Map_AncientCitadel", "Group_DLC_Stages_Remote", new Color(0.12f, 0.08f, 0.06f, 1f));
            GenerateSingleMapPrefab("Map_CinnabarSwamp", "Group_DLC_Stages_Remote", new Color(0.06f, 0.12f, 0.08f, 1f));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=#00FF88>[RemoteStagesPrefabGenerator]</color> Đã tạo thành công Prefab Map_AncientCitadel và Map_CinnabarSwamp cho DLC!");
        }

        private static void GenerateSingleMapPrefab(string addressKey, string groupName, Color bgColor)
        {
            string prefabPath = $"{PREFAB_DIR}/{addressKey}.prefab";

            GameObject rootObj = new GameObject(addressKey);
            Grid grid = rootObj.AddComponent<Grid>();
            grid.cellSize = new Vector3(1f, 1f, 0f);

            // Tầng 0: BaseGround
            GameObject baseObj = new GameObject("Tilemap_BaseGround");
            baseObj.transform.SetParent(rootObj.transform, false);
            Tilemap baseTilemap = baseObj.AddComponent<Tilemap>();
            TilemapRenderer baseRenderer = baseObj.AddComponent<TilemapRenderer>();
            baseRenderer.sortingLayerName = "Background";
            baseRenderer.sortingOrder = -1;

            // Tầng 1: Ground
            GameObject groundObj = new GameObject("Tilemap_Ground");
            groundObj.transform.SetParent(rootObj.transform, false);
            Tilemap groundTilemap = groundObj.AddComponent<Tilemap>();
            TilemapRenderer groundRenderer = groundObj.AddComponent<TilemapRenderer>();
            groundRenderer.sortingLayerName = "Tilemap_Ground";
            groundRenderer.sortingOrder = 0;

            // Tầng 2: Decals
            GameObject decalObj = new GameObject("Tilemap_Decals");
            decalObj.transform.SetParent(rootObj.transform, false);
            Tilemap decalTilemap = decalObj.AddComponent<Tilemap>();
            TilemapRenderer decalRenderer = decalObj.AddComponent<TilemapRenderer>();
            decalRenderer.sortingLayerName = "Tilemap_Decals";
            decalRenderer.sortingOrder = 1;

            // Tầng 3: Obstacles
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

            // Nạp Tiles từ Atlas
            string texturePath = "Assets/Art/Tilemaps/SanDinhLangCo/Tileset_SanDinhLangCo.png";
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            Sprite groundSprite = null;
            Sprite wallSprite = null;

            foreach (var asset in assets)
            {
                if (asset is Sprite s)
                {
                    if (s.name == "Tile_0_0") groundSprite = s;
                    else if (s.name == "Tile_0_2") wallSprite = s;
                }
            }

            if (groundSprite != null)
            {
                Tile tGround = ScriptableObject.CreateInstance<Tile>();
                tGround.sprite = groundSprite;

                int radius = 18;
                for (int x = -radius; x <= radius; x++)
                {
                    for (int y = -radius; y <= radius; y++)
                    {
                        groundTilemap.SetTile(new Vector3Int(x, y, 0), tGround);
                    }
                }
            }

            // Boundaries
            CreateMapBoundaryColliders(rootObj, 36f, 36f);

            // Lưu Prefab
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(rootObj, prefabPath);
            Object.DestroyImmediate(rootObj);

            // Đăng ký Addressable
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
    }
}
