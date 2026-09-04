#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.Editor.Tools
{
    /// <summary>
    /// Tool tự động tìm các Prefabs có sẵn (Player & Enemies) và thêm/cập nhật GameObject Shadow.
    /// Giữ nguyên 100% component và cấu hình hiện có của prefab.
    /// Menu: ProjectZombie > Characters > Setup Character Shadows (All Existing Prefabs)
    /// </summary>
    public static class CharacterShadowSetupTool
    {
        private const string PlayerFolderPath = "Assets/_Prefabs/Characters/Players";
        private const string EnemyFolderPath = "Assets/_Prefabs/Characters/Enemies";

        [MenuItem("ProjectZombie/Characters/Setup Character Shadows (All Existing Prefabs)")]
        public static void SetupAllCharacterShadows()
        {
            Sprite shadowSprite = ShadowSpriteGenerator.GetOrCreateShadowSprite();
            if (shadowSprite == null)
            {
                Debug.LogError("[CharacterShadowSetupTool] ❌ Không tìm thấy hoặc không thể tạo Shadow Sprite!");
                return;
            }

            int count = 0;
            count += ProcessFolder(PlayerFolderPath, shadowSprite, isPlayer: true);
            count += ProcessFolder(EnemyFolderPath, shadowSprite, isPlayer: false);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[CharacterShadowSetupTool] ✅ Đã cập nhật thành công Shadow cho {count} Character Prefabs có sẵn!");
        }

        private static int ProcessFolder(string folderPath, Sprite shadowSprite, bool isPlayer)
        {
            if (!Directory.Exists(folderPath))
            {
                Debug.LogWarning($"[CharacterShadowSetupTool] Thư mục không tồn tại: {folderPath}");
                return 0;
            }

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
            int processedCount = 0;

            foreach (string guid in prefabGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (ApplyShadowToPrefab(assetPath, shadowSprite, isPlayer))
                {
                    processedCount++;
                }
            }

            return processedCount;
        }

        public static bool ApplyShadowToPrefab(string prefabPath, Sprite shadowSprite, bool isPlayer)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            if (root == null)
            {
                Debug.LogError($"[CharacterShadowSetupTool] Không thể mở Prefab: {prefabPath}");
                return false;
            }

            try
            {
                string prefabName = Path.GetFileNameWithoutExtension(prefabPath);

                // 1. Tìm hoặc tạo Child Shadow
                Transform shadowTransform = root.transform.Find("Shadow");
                GameObject shadowObj;
                if (shadowTransform == null)
                {
                    shadowObj = new GameObject("Shadow");
                    shadowObj.transform.SetParent(root.transform, false);
                }
                else
                {
                    shadowObj = shadowTransform.gameObject;
                }

                // Đặt vị trí Shadow dưới chân (Sibling index đầu tiên để gọn hierarchy)
                shadowObj.transform.SetSiblingIndex(0);

                // 2. Tính toán Offset và Scale tương ứng với kích thước nhân vật
                Vector3 shadowOffset = new Vector3(0f, -0.4f, 0f);
                Vector3 shadowScale = new Vector3(0.85f, 0.35f, 1f);

                if (!isPlayer)
                {
                    // Tùy biến scale theo từng loại quái/boss
                    if (prefabName.Contains("Boss") || prefabName.Contains("DiemVuong") || prefabName.Contains("NguuDau"))
                    {
                        shadowOffset = new Vector3(0f, -0.8f, 0f);
                        shadowScale = new Vector3(2.2f, 0.9f, 1f);
                    }
                    else if (prefabName.Contains("QUYNHAPTRANG"))
                    {
                        shadowOffset = new Vector3(0f, -0.55f, 0f);
                        shadowScale = new Vector3(1.3f, 0.5f, 1f);
                    }
                    else if (prefabName.Contains("MATROI") || prefabName.Contains("HOALYTINH"))
                    {
                        shadowOffset = new Vector3(0f, -0.35f, 0f);
                        shadowScale = new Vector3(0.65f, 0.28f, 1f);
                    }
                    else
                    {
                        // Quái thường (Ma Giáp, Ma Da...)
                        shadowOffset = new Vector3(0f, -0.42f, 0f);
                        shadowScale = new Vector3(0.85f, 0.35f, 1f);
                    }
                }

                shadowObj.transform.localPosition = shadowOffset;
                shadowObj.transform.localRotation = Quaternion.identity;
                shadowObj.transform.localScale = shadowScale;

                // 3. Setup SpriteRenderer
                SpriteRenderer sr = shadowObj.GetComponent<SpriteRenderer>();
                if (sr == null)
                {
                    sr = shadowObj.AddComponent<SpriteRenderer>();
                }

                sr.sprite = shadowSprite;
                // Màu đen với độ trong suốt 45%
                sr.color = new Color(0f, 0f, 0f, 0.45f);

                // Tìm SpriteRenderer của Visual để đồng bộ Sorting Layer
                Transform visualTransform = root.transform.Find("Visual");
                if (visualTransform != null && visualTransform.TryGetComponent<SpriteRenderer>(out var visualSr))
                {
                    sr.sortingLayerID = visualSr.sortingLayerID;
                    sr.sortingLayerName = visualSr.sortingLayerName;
                    // Đặt order nhỏ hơn Visual để luôn nằm dưới chân
                    sr.sortingOrder = visualSr.sortingOrder - 10;
                }
                else
                {
                    sr.sortingOrder = -10;
                }

                // 4. Lưu lại Prefab Asset mà không làm mất các component khác
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Debug.Log($"[CharacterShadowSetupTool] ✔ Đã gắn/cập nhật Shadow cho: {prefabName}");
                return true;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
#endif
