#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.UI.HUD;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Tool tự động Slice 5 icon phụ kiện Timeline HUD Kit (Ngọc Chim Lạc, Trụ Đuốc Phase, Song Kiếm, Đầu Trâu, Rồng Lửa)
    /// và gán trực tiếp vào WaveBannerWidgetView trong Scene & Prefabs.
    /// Menu: Tools > Vong Xuyen > Slice & Integrate Timeline HUD Kit
    /// </summary>
    public static class TimelineHudKitSlicer
    {
        [MenuItem("Tools/Vong Xuyen/Slice & Integrate Timeline HUD Kit", priority = 25)]
        public static void SliceAndIntegrateHudKit()
        {
            string texturePath = "Assets/Art/UI/HUD/Timeline_HUD_Kit.png";
            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;

            if (importer == null)
            {
                // Thử tìm định dạng .jpg nếu người dùng lưu .jpg
                texturePath = "Assets/Art/UI/HUD/Timeline_HUD_Kit.jpg";
                importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            }

            if (importer == null)
            {
                Debug.LogError($"[TimelineHudKitSlicer] Không tìm thấy texture tại Assets/Art/UI/HUD/Timeline_HUD_Kit.png (hoặc .jpg). Vui lòng lưu file ảnh vào thư mục này!");
                EditorUtility.DisplayDialog("Timeline HUD Slicer", "Không tìm thấy file ảnh tại: Assets/Art/UI/HUD/Timeline_HUD_Kit.png\n\nVui lòng lưu ảnh bạn vừa tạo vào đường dẫn trên rồi bấm lại Menu Tool.", "Đã hiểu");
                return;
            }

            // 1. Kiểm tra nạp trực tiếp các sprite đơn lẻ trước nếu có
            Sprite spPlayer = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/Pin_Player_LacBird.png");
            Sprite spDivider = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/Pin_Phase_Divider.png");
            Sprite spSwarm = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/Badge_Swarm_Swords.png");
            Sprite spElite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/Badge_Elite_OxHead.png");
            Sprite spDragon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/HUD/Badge_Boss_Dragon.png");

            // Nếu chưa có, tiến hành slice từ file texture atlas Timeline_HUD_Kit.png
            if (spPlayer == null || spDivider == null || spSwarm == null || spElite == null || spDragon == null)
            {
                // 1. Cấu hình Texture Importer sang Sprite Multiple
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.maxTextureSize = 2048;

                var metaDataList = new List<SpriteMetaData>();

                // 1. Player Position Marker (Pin Ngọc Chim Lạc) - Trái ngoài cùng
                metaDataList.Add(new SpriteMetaData
                {
                    name = "Pin_Player_LacBird",
                    rect = new Rect(41, 105, 192, 412),
                    alignment = (int)SpriteAlignment.Custom,
                    pivot = new Vector2(0.5f, 0.05f) // Điểm nhọn cắm xuống thanh bar
                });

                // 2. Phase Divider Pin (Trụ Đuốc Đồng) - Vị trí 2
                metaDataList.Add(new SpriteMetaData
                {
                    name = "Pin_Phase_Divider",
                    rect = new Rect(268, 105, 142, 420),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                });

                // 3. Swarm Wave Badge (Khung Song Kiếm)
                metaDataList.Add(new SpriteMetaData
                {
                    name = "Badge_Swarm_Swords",
                    rect = new Rect(479, 24, 231, 230),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                });

                // 4. Elite Monster Crest (Đầu Trâu)
                metaDataList.Add(new SpriteMetaData
                {
                    name = "Badge_Elite_OxHead",
                    rect = new Rect(484, 303, 207, 226),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                });

                // 5. Final Boss Crest (Rồng Lửa)
                metaDataList.Add(new SpriteMetaData
                {
                    name = "Badge_Boss_Dragon",
                    rect = new Rect(756, 289, 228, 243),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                });

#pragma warning disable CS0618
                importer.spritesheet = metaDataList.ToArray();
#pragma warning restore CS0618
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
                AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);

                // Load các sub-sprites đã slice
                Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
                Dictionary<string, Sprite> spriteMap = new Dictionary<string, Sprite>();
                foreach (var asset in allAssets)
                {
                    if (asset is Sprite sp)
                    {
                        spriteMap[sp.name] = sp;
                    }
                }

                spriteMap.TryGetValue("Pin_Player_LacBird", out spPlayer);
                spriteMap.TryGetValue("Pin_Phase_Divider", out spDivider);
                spriteMap.TryGetValue("Badge_Swarm_Swords", out spSwarm);
                spriteMap.TryGetValue("Badge_Elite_OxHead", out spElite);
                spriteMap.TryGetValue("Badge_Boss_Dragon", out spDragon);
            }

            // 4. Tìm và gán Sprite vào tất cả WaveBannerWidgetView trong Scene & Prefabs
            var bannerViews = Resources.FindObjectsOfTypeAll<WaveBannerWidgetView>();
            foreach (var bannerView in bannerViews)
            {
                if (bannerView == null) continue;
                SerializedObject so = new SerializedObject(bannerView);

                if (spPlayer != null)
                {
                    var prop = so.FindProperty("_playerIndicatorSprite");
                    if (prop != null) prop.objectReferenceValue = spPlayer;
                }

                if (spDivider != null)
                {
                    var prop = so.FindProperty("_phaseDividerSprite");
                    if (prop != null) prop.objectReferenceValue = spDivider;
                }

                if (spSwarm != null)
                {
                    var prop = so.FindProperty("_swarmBadgeSprite");
                    if (prop != null) prop.objectReferenceValue = spSwarm;
                }

                if (spElite != null)
                {
                    var prop = so.FindProperty("_eliteBadgeSprite");
                    if (prop != null) prop.objectReferenceValue = spElite;
                }

                if (spDragon != null)
                {
                    var prop = so.FindProperty("_finalBossBadgeSprite");
                    if (prop != null) prop.objectReferenceValue = spDragon;
                }

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(bannerView);
                Debug.Log($"<color=#4DEEEA><b>[TimelineHudKitSlicer]</b> ✅ Đã gán thành công 5 Sprite phụ kiện vào {bannerView.name}!</color>");
            }

            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Timeline HUD Slicer", "Đã slice thành công 5 Icon và tích hợp vào WaveBannerWidgetView!\n\n1. Pin_Player_LacBird\n2. Pin_Phase_Divider\n3. Badge_Swarm_Swords\n4. Badge_Elite_OxHead\n5. Badge_Boss_Dragon", "Tuyệt vời");
        }
    }
}
#endif
