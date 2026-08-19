using UnityEditor;
using UnityEngine;

namespace ProjectZombie.Features.VFX.Editor
{
    public static class SkillIndicatorPrefabGenerator
    {
        [MenuItem("ProjectZombie/VFX/Rebuild Boss Skill Indicators (Circle & Box)")]
        public static void GeneratePrefabs()
        {
            string folderPath = "Assets/Features/VFX/Prefabs";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/Features/VFX", "Prefabs");
            }

            // 1. Configure Textures Meta
            ConfigureTexture("Assets/Art/VFX/Indicators/TEX_Indicator_Circle.png", 512);
            ConfigureTexture("Assets/Art/VFX/Indicators/TEX_Indicator_Box.png", 256);
            ConfigureTexture("Assets/Art/VFX/Indicators/TEX_Indicator_Fill.png", 256);

            Sprite circleBorder = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/VFX/Indicators/TEX_Indicator_Circle.png");
            Sprite boxBorder = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/VFX/Indicators/TEX_Indicator_Box.png");
            Sprite fillDisc = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/VFX/Indicators/TEX_Indicator_Fill.png");

            // 2. Build Circle Indicator Prefab (Ground Slam AOE)
            GameObject circleObj = new GameObject("Indicator_Circle");
            var circleBorderRenderer = circleObj.AddComponent<SpriteRenderer>();
            circleBorderRenderer.sprite = circleBorder;
            circleBorderRenderer.sortingLayerName = "Shadows";
            circleBorderRenderer.sortingOrder = 7;
            circleBorderRenderer.color = Color.white;

            // Child Fill
            GameObject circleFillObj = new GameObject("FillVisual");
            circleFillObj.transform.SetParent(circleObj.transform);
            circleFillObj.transform.localPosition = Vector3.zero;
            var circleFillRenderer = circleFillObj.AddComponent<SpriteRenderer>();
            circleFillRenderer.sprite = fillDisc;
            circleFillRenderer.sortingLayerName = "Shadows";
            circleFillRenderer.sortingOrder = 8;
            circleFillRenderer.color = new Color(1f, 0.25f, 0.25f, 0.65f);

            var circleScript = circleObj.AddComponent<Indicators.SkillIndicator>();
            circleScript.Construct(circleBorderRenderer, circleFillRenderer, Indicators.IndicatorShape.Circle);

            string circlePrefabPath = $"{folderPath}/Indicator_Circle.prefab";
            PrefabUtility.SaveAsPrefabAsset(circleObj, circlePrefabPath);
            Object.DestroyImmediate(circleObj);

            // 3. Build Box Indicator Prefab (Bull Dash / Slash)
            GameObject boxObj = new GameObject("Indicator_Box");
            var boxBorderRenderer = boxObj.AddComponent<SpriteRenderer>();
            boxBorderRenderer.sprite = boxBorder;
            boxBorderRenderer.sortingLayerName = "Shadows";
            boxBorderRenderer.sortingOrder = 7;
            boxBorderRenderer.color = Color.white;

            // Child Fill
            GameObject boxFillObj = new GameObject("FillVisual");
            boxFillObj.transform.SetParent(boxObj.transform);
            boxFillObj.transform.localPosition = Vector3.zero;
            var boxFillRenderer = boxFillObj.AddComponent<SpriteRenderer>();
            boxFillRenderer.sprite = boxBorder;
            boxFillRenderer.sortingLayerName = "Shadows";
            boxFillRenderer.sortingOrder = 8;
            boxFillRenderer.color = new Color(1f, 0.25f, 0.25f, 0.5f);

            var boxScript = boxObj.AddComponent<Indicators.SkillIndicator>();
            boxScript.Construct(boxBorderRenderer, boxFillRenderer, Indicators.IndicatorShape.Box);

            string boxPrefabPath = $"{folderPath}/Indicator_Box.prefab";
            PrefabUtility.SaveAsPrefabAsset(boxObj, boxPrefabPath);
            Object.DestroyImmediate(boxObj);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SkillIndicatorPrefabGenerator] ĐÃ NÂNG CẤP VÀ XUẤT THÀNH CÔNG PREFABS CHỈ BÁO BOSS TẠI {folderPath}!");
        }

        private static void ConfigureTexture(string path, int ppu)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = ppu;
                importer.filterMode = FilterMode.Bilinear;
                importer.alphaIsTransparency = true;
                importer.sRGBTexture = true;
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
        }
    }
}
