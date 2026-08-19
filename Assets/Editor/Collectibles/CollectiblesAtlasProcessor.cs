using UnityEditor;
using UnityEngine;

namespace Projectzombie.Editor.CollectiblesTools
{
    public class CollectiblesAtlasProcessor : EditorWindow
    {
        [MenuItem("ProjectZombie/Collectibles/Setup Gems & Chests Sprites")]
        public static void SetupCollectibles()
        {
            string texturePath = "Assets/Art/Collectibles/Collectibles_Atlas.png";
            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogError($"[Collectibles] Không tìm thấy ảnh tại {texturePath}");
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 64;
            importer.filterMode = FilterMode.Bilinear;
            importer.alphaIsTransparency = true;
            importer.sRGBTexture = true;

            // Atlas 512x256 (Origin bottom-left)
            // Height = 256
            // Row 0 (Top in image = Y 192..256 in Unity coords): 5 Gems of size 64x64
            // Row 1 (Bottom in image = Y 64..192 in Unity coords): 2 Chests of size 128x128

            SpriteMetaData[] metaData = new SpriteMetaData[]
            {
                new SpriteMetaData
                {
                    name = "Gem_Tier1_Cyan",
                    rect = new Rect(0, 192, 64, 64),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                },
                new SpriteMetaData
                {
                    name = "Gem_Tier2_Emerald",
                    rect = new Rect(64, 192, 64, 64),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                },
                new SpriteMetaData
                {
                    name = "Gem_Tier3_Purple",
                    rect = new Rect(128, 192, 64, 64),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                },
                new SpriteMetaData
                {
                    name = "Gem_Tier4_Gold",
                    rect = new Rect(192, 192, 64, 64),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                },
                new SpriteMetaData
                {
                    name = "Gem_Template_White",
                    rect = new Rect(256, 192, 64, 64),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                },
                new SpriteMetaData
                {
                    name = "Chest_Normal_Wood",
                    rect = new Rect(0, 64, 128, 128),
                    alignment = (int)SpriteAlignment.BottomCenter,
                    pivot = new Vector2(0.5f, 0.15f)
                },
                new SpriteMetaData
                {
                    name = "Chest_Boss_UMinh",
                    rect = new Rect(128, 64, 128, 128),
                    alignment = (int)SpriteAlignment.BottomCenter,
                    pivot = new Vector2(0.5f, 0.15f)
                }
            };

            importer.spritesheet = metaData;
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
            AssetDatabase.Refresh();

            // Auto-assign to Prefabs
            AssignSpritesToPrefabs();
            Debug.Log("[Collectibles] Đã cắt Sprite và cập nhật hình ảnh Gem & Rương Boss U Minh thành công!");
        }

        private static void AssignSpritesToPrefabs()
        {
            string texturePath = "Assets/Art/Collectibles/Collectibles_Atlas.png";
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            Sprite whiteGem = null;
            Sprite normalChest = null;
            Sprite bossChest = null;

            foreach (var a in assets)
            {
                if (a is Sprite s)
                {
                    if (s.name == "Gem_Template_White") whiteGem = s;
                    if (s.name == "Chest_Normal_Wood") normalChest = s;
                    if (s.name == "Chest_Boss_UMinh") bossChest = s;
                }
            }

            // 1. Update ExpGem.prefab
            string gemPrefabPath = "Assets/_Prefabs/ExpGem.prefab";
            GameObject gemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(gemPrefabPath);
            if (gemPrefab != null && whiteGem != null)
            {
                var sr = gemPrefab.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = whiteGem;
                    sr.sortingLayerName = "Collectibles";
                    sr.color = Color.white;
                    EditorUtility.SetDirty(gemPrefab);
                }
            }

            // 2. Update Chest_UMinh.prefab (Rương Boss)
            string bossChestPrefabPath = "Assets/_Prefabs/Chests/Chest_UMinh.prefab";
            GameObject bossChestPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(bossChestPrefabPath);
            if (bossChestPrefab != null && bossChest != null)
            {
                var sr = bossChestPrefab.GetComponentInChildren<SpriteRenderer>(true);
                if (sr != null)
                {
                    sr.sprite = bossChest;
                    sr.sortingLayerName = "Collectibles";
                    sr.color = Color.white;
                    EditorUtility.SetDirty(bossChestPrefab);
                }
            }

            // 3. Update Chest_DauThai.prefab (Rương Thường)
            string dauThaiChestPrefabPath = "Assets/_Prefabs/Chests/Chest_DauThai.prefab";
            GameObject dauThaiChestPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(dauThaiChestPrefabPath);
            if (dauThaiChestPrefab != null && normalChest != null)
            {
                var sr = dauThaiChestPrefab.GetComponentInChildren<SpriteRenderer>(true);
                if (sr != null)
                {
                    sr.sprite = normalChest;
                    sr.sortingLayerName = "Collectibles";
                    sr.color = Color.white;
                    EditorUtility.SetDirty(dauThaiChestPrefab);
                }
            }

            AssetDatabase.SaveAssets();
        }
    }
}
