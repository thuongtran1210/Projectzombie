using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Collectibles;

namespace Projectzombie.Editor.CollectiblesTools
{
    public static class CoinPrefabBuilder
    {
        [MenuItem("Tools/ProjectZombie/Create or Update Coin Prefab", priority = 2)]
        public static void CreateCoinPrefab()
        {
            string folderPath = "Assets/_Prefabs/Collectibles";
            if (!AssetDatabase.IsValidFolder("Assets/_Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "_Prefabs");
            }
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/_Prefabs", "Collectibles");
            }

            string prefabPath = $"{folderPath}/Coin_Drop.prefab";

            // Tạo GameObject tạm thời trong Scene
            GameObject coinObj = new GameObject("Coin_Drop");

            // 1. SpriteRenderer
            var sr = coinObj.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Collectibles";
            sr.sortingOrder = 1;

            string coinSpritePath = "Assets/Art/UI/Badges/Icon_CoTien_VongXuyen.png";
            Sprite coinSprite = AssetDatabase.LoadAssetAtPath<Sprite>(coinSpritePath);
            if (coinSprite != null)
            {
                sr.sprite = coinSprite;
            }

            // 2. CircleCollider2D (Trigger)
            var col = coinObj.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.4f;

            // 3. CoinDrop Component
            var coinDrop = coinObj.AddComponent<CoinDrop>();

            // 4. CoinPoolConfig Component
            var poolConfig = coinObj.AddComponent<CoinPoolConfig>();

            // Lưu thành Prefab
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(coinObj, prefabPath);
            Object.DestroyImmediate(coinObj);

            Debug.Log($"[CoinPrefabBuilder] Đã tạo thành công Coin Prefab tại: {prefabPath}");

            // Tự động gán vào CoinPoolManager trong Scene nếu có
            var poolMgr = Object.FindObjectOfType<CoinPoolManager>();
            if (poolMgr != null)
            {
                var so = new SerializedObject(poolMgr);
                var prop = so.FindProperty("defaultCoinPrefab");
                if (prop != null)
                {
                    prop.objectReferenceValue = savedPrefab;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(poolMgr);
                    Debug.Log("[CoinPrefabBuilder] Đã tự động gán defaultCoinPrefab cho CoinPoolManager trong Scene!");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
