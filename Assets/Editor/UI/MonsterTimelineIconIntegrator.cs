#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using ProjectZombie.Features.Spawners;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Tool tu dong slice Sprite Sheet quai vat va tich hop truc tiep vao Level1_Timeline.asset
    /// </summary>
    public static class MonsterTimelineIconIntegrator
    {
        [MenuItem("Tools/Vong Xuyen/Integrate Monster Icons into Level 1 Timeline", priority = 20)]
        public static void IntegrateMonsterIcons()
        {
            string texturePath = "Assets/Art/UI/Icons/Monsters/Level1_MonsterIcons_Sheet.png";
            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;

            if (importer == null)
            {
                Debug.LogError($"[MonsterTimelineIconIntegrator] Khong tim thay texture tai: {texturePath}");
                return;
            }

            // 1. Cau hinh Texture Importer sang Sprite Multiple
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 2048;

            // 2. Dinh nghia 8 Sprite Rects (Luoi 3 cot x 3 hang cua anh 1024x1024)
            // Hang 1 (Top): Y tu 683 -> 1024 (Height 341)
            // Hang 2 (Mid): Y tu 341 -> 683  (Height 342)
            // Hang 3 (Bot): Y tu 0   -> 341  (Height 341)
            // Cot: X0: 0->341, X1: 341->683, X2: 683->1024
            var metaDataList = new List<SpriteMetaData>();

            // R0C0: Ma Giap (Top-Left)
            metaDataList.Add(new SpriteMetaData
            {
                name = "Icon_Monster_MaGiap",
                rect = new Rect(0, 683, 341, 341),
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f)
            });

            // R0C1: Ma Da (Top-Mid)
            metaDataList.Add(new SpriteMetaData
            {
                name = "Icon_Monster_MaDa",
                rect = new Rect(341, 683, 342, 341),
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f)
            });

            // R0C2: Ma Troi (Top-Right)
            metaDataList.Add(new SpriteMetaData
            {
                name = "Icon_Monster_MaTroi",
                rect = new Rect(683, 683, 341, 341),
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f)
            });

            // R1C0: Ho Ly Tinh (Mid-Left)
            metaDataList.Add(new SpriteMetaData
            {
                name = "Icon_Monster_HoLyTinh",
                rect = new Rect(0, 341, 341, 342),
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f)
            });

            // R1C1: Quy Nhap Trang (Mid-Mid)
            metaDataList.Add(new SpriteMetaData
            {
                name = "Icon_Monster_QuyNhapTrang",
                rect = new Rect(341, 341, 342, 342),
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f)
            });

            // R1C2: Ma Doi No (Mid-Right)
            metaDataList.Add(new SpriteMetaData
            {
                name = "Icon_Monster_MaDoiNo",
                rect = new Rect(683, 341, 341, 342),
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f)
            });

            // R2C0: Ma Doi No - Bien the trai (Bot-Left)
            metaDataList.Add(new SpriteMetaData
            {
                name = "Icon_Monster_MaDoiNo_Left",
                rect = new Rect(0, 0, 341, 341),
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f)
            });

            // R2C1: Nguu Dau Ma Dien (Bot-Mid)
            metaDataList.Add(new SpriteMetaData
            {
                name = "Icon_Monster_NguuDauMaDien",
                rect = new Rect(341, 0, 342, 341),
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f)
            });

            // R2C2: Diem Vuong (Bot-Right)
            metaDataList.Add(new SpriteMetaData
            {
                name = "Icon_Monster_DiemVuong",
                rect = new Rect(683, 0, 341, 341),
                alignment = (int)SpriteAlignment.Center,
                pivot = new Vector2(0.5f, 0.5f)
            });

#pragma warning disable CS0618
            importer.spritesheet = metaDataList.ToArray();
#pragma warning restore CS0618
            EditorUtility.SetDirty(importer);
            importer.SaveAndReimport();
            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);

            // 3. Load cac sub-sprites da duoc slice
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            Dictionary<string, Sprite> spriteMap = new Dictionary<string, Sprite>();
            foreach (var asset in allAssets)
            {
                if (asset is Sprite sp)
                {
                    spriteMap[sp.name] = sp;
                }
            }

            // 4. Load Level1_Timeline ScriptableObject va gan Icon vao tung Event
            string timelinePath = "Assets/_Data/Levels/Level1_Timeline.asset";
            LevelTimelineConfig timeline = AssetDatabase.LoadAssetAtPath<LevelTimelineConfig>(timelinePath);

            if (timeline == null)
            {
                Debug.LogError($"[MonsterTimelineIconIntegrator] Khong tim thay Timeline tai: {timelinePath}");
                return;
            }

            Undo.RecordObject(timeline, "Integrate Monster Icons");

            for (int i = 0; i < timeline.events.Count; i++)
            {
                var evt = timeline.events[i];
                if (evt == null) continue;

                switch (i)
                {
                    case 0: // Phut 00:00 (0s) Ma Giap
                        if (spriteMap.TryGetValue("Icon_Monster_MaGiap", out var sp0)) evt.eventIcon = sp0;
                        break;
                    case 1: // Phut 01:00 (60s) Ma Da
                        if (spriteMap.TryGetValue("Icon_Monster_MaDa", out var sp1)) evt.eventIcon = sp1;
                        break;
                    case 2: // Phut 02:00 (120s) Ma Troi
                        if (spriteMap.TryGetValue("Icon_Monster_MaTroi", out var sp2)) evt.eventIcon = sp2;
                        break;
                    case 3: // Phut 03:00 (180s) Ma Da Burst
                        if (spriteMap.TryGetValue("Icon_Monster_MaDa", out var sp3)) evt.eventIcon = sp3;
                        break;
                    case 4: // Phut 04:30 (270s) Elite Quy Nhap Trang
                        if (spriteMap.TryGetValue("Icon_Monster_QuyNhapTrang", out var sp4)) evt.eventIcon = sp4;
                        break;
                    case 5: // Phut 06:00 (360s) Ho Ly Tinh
                        if (spriteMap.TryGetValue("Icon_Monster_HoLyTinh", out var sp5)) evt.eventIcon = sp5;
                        break;
                    case 6: // Phut 06:45 (405s) Ma Doi No
                        if (spriteMap.TryGetValue("Icon_Monster_MaDoiNo", out var sp6)) evt.eventIcon = sp6;
                        break;
                    case 7: // Phut 07:30 (450s) Mid-Boss Nguu Dau Ma Dien
                        if (spriteMap.TryGetValue("Icon_Monster_NguuDauMaDien", out var sp7)) evt.eventIcon = sp7;
                        break;
                    case 8: // Phut 09:30 (570s) Ho Ly Tinh Burst
                        if (spriteMap.TryGetValue("Icon_Monster_HoLyTinh", out var sp8)) evt.eventIcon = sp8;
                        break;
                    case 9: // Phut 11:30 (690s) Quy Nhap Trang
                        if (spriteMap.TryGetValue("Icon_Monster_QuyNhapTrang", out var sp9)) evt.eventIcon = sp9;
                        break;
                    case 10: // Phut 13:30 (810s) Ma Giap Burst Pre-Boss
                        if (spriteMap.TryGetValue("Icon_Monster_MaGiap", out var sp10)) evt.eventIcon = sp10;
                        break;
                    case 11: // Phut 15:00 (900s) Diem Vuong Final Boss
                        if (spriteMap.TryGetValue("Icon_Monster_DiemVuong", out var sp11)) evt.eventIcon = sp11;
                        break;
                }
            }

            EditorUtility.SetDirty(timeline);

            // Dong bo sang Resources theo chuan Kien truc Rule 13.3
            string resourcesTimelinePath = "Assets/Resources/Levels/Level1_Timeline.asset";
            LevelTimelineConfig resTimeline = AssetDatabase.LoadAssetAtPath<LevelTimelineConfig>(resourcesTimelinePath);
            if (resTimeline != null)
            {
                Undo.RecordObject(resTimeline, "Integrate Monster Icons to Resources");
                for (int i = 0; i < timeline.events.Count && i < resTimeline.events.Count; i++)
                {
                    if (resTimeline.events[i] != null && timeline.events[i] != null)
                    {
                        resTimeline.events[i].eventIcon = timeline.events[i].eventIcon;
                    }
                }
                EditorUtility.SetDirty(resTimeline);
            }

            AssetDatabase.SaveAssets();
            Debug.Log("<color=#4DEEEA><b>[MonsterTimelineIconIntegrator]</b> Đã slice 9 Sprite Icon và gắn thành công vào Level1_Timeline.asset (_Data & Resources)!</color>");
        }
    }
}
#endif
