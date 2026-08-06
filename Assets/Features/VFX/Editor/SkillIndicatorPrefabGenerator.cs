#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.Features.VFX.Editor
{
    public static class SkillIndicatorPrefabGenerator
    {
        [MenuItem("Tools/ProjectZombie/VFX/Generate Skill Indicator Prefabs")]
        public static void GeneratePrefabs()
        {
            string folderPath = "Assets/Features/VFX/Prefabs";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/Features/VFX", "Prefabs");
            }

            // 1. Tạo Box Indicator Prefab (32x32 px, PPU = 32 -> Kích thước World = 1x1m)
            GameObject boxObj = new GameObject("Indicator_Box");
            var boxRenderer = boxObj.AddComponent<SpriteRenderer>();

            Texture2D boxTex = new Texture2D(32, 32);
            Color[] boxColors = new Color[32 * 32];
            for (int i = 0; i < boxColors.Length; i++) boxColors[i] = Color.white;
            boxTex.SetPixels(boxColors);
            boxTex.Apply();
            boxRenderer.sprite = Sprite.Create(boxTex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
            boxRenderer.color = new Color(1f, 0f, 0f, 0.4f);
            boxRenderer.sortingLayerName = "Default";
            boxRenderer.sortingOrder = 10;

            var boxScript = boxObj.AddComponent<Indicators.SkillIndicator>();
            var serializedBox = new SerializedObject(boxScript);
            serializedBox.FindProperty("_spriteRenderer").objectReferenceValue = boxRenderer;
            serializedBox.FindProperty("_shape").enumValueIndex = (int)Indicators.IndicatorShape.Box;
            serializedBox.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(boxObj, $"{folderPath}/Indicator_Box.prefab");
            Object.DestroyImmediate(boxObj);

            // 2. Tạo Circle Indicator Prefab (64x64 px, PPU = 64 -> Kích thước World = 1x1m)
            GameObject circleObj = new GameObject("Indicator_Circle");
            var circleRenderer = circleObj.AddComponent<SpriteRenderer>();

            Texture2D circleTex = new Texture2D(64, 64);
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(31.5f, 31.5f));
                    if (dist <= 31.5f)
                        circleTex.SetPixel(x, y, Color.white);
                    else
                        circleTex.SetPixel(x, y, Color.clear);
                }
            }
            circleTex.Apply();
            circleRenderer.sprite = Sprite.Create(circleTex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64f);
            circleRenderer.color = new Color(1f, 0f, 0f, 0.4f);
            circleRenderer.sortingLayerName = "Default";
            circleRenderer.sortingOrder = 10;

            var circleScript = circleObj.AddComponent<Indicators.SkillIndicator>();
            var serializedCircle = new SerializedObject(circleScript);
            serializedCircle.FindProperty("_spriteRenderer").objectReferenceValue = circleRenderer;
            serializedCircle.FindProperty("_shape").enumValueIndex = (int)Indicators.IndicatorShape.Circle;
            serializedCircle.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(circleObj, $"{folderPath}/Indicator_Circle.prefab");
            Object.DestroyImmediate(circleObj);

            AssetDatabase.Refresh();
            Debug.Log($"[SkillIndicatorPrefabGenerator] Đã tạo thành công Prefabs Indicator chuẩn 1x1m tại {folderPath}!");
        }
    }
}
#endif

