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

            // 1. Tạo Box Indicator Prefab
            GameObject boxObj = new GameObject("Indicator_Box");
            var boxRenderer = boxObj.AddComponent<SpriteRenderer>();
            boxRenderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            boxRenderer.color = new Color(1f, 0f, 0f, 0.4f);
            boxRenderer.sortingLayerName = "Default";
            boxRenderer.sortingOrder = -1;

            var boxScript = boxObj.AddComponent<Indicators.SkillIndicator>();
            // Gán private field via SerializedObject
            var serializedBox = new SerializedObject(boxScript);
            serializedBox.FindProperty("_spriteRenderer").objectReferenceValue = boxRenderer;
            serializedBox.FindProperty("_shape").enumValueIndex = (int)Indicators.IndicatorShape.Box;
            serializedBox.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(boxObj, $"{folderPath}/Indicator_Box.prefab");
            Object.DestroyImmediate(boxObj);

            // 2. Tạo Circle Indicator Prefab
            GameObject circleObj = new GameObject("Indicator_Circle");
            var circleRenderer = circleObj.AddComponent<SpriteRenderer>();
            circleRenderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            circleRenderer.color = new Color(1f, 0f, 0f, 0.4f);
            circleRenderer.sortingLayerName = "Default";
            circleRenderer.sortingOrder = -1;

            var circleScript = circleObj.AddComponent<Indicators.SkillIndicator>();
            var serializedCircle = new SerializedObject(circleScript);
            serializedCircle.FindProperty("_spriteRenderer").objectReferenceValue = circleRenderer;
            serializedCircle.FindProperty("_shape").enumValueIndex = (int)Indicators.IndicatorShape.Circle;
            serializedCircle.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(circleObj, $"{folderPath}/Indicator_Circle.prefab");
            Object.DestroyImmediate(circleObj);

            AssetDatabase.Refresh();
            Debug.Log($"[SkillIndicatorPrefabGenerator] Đã tạo thành công Prefabs Indicator tại {folderPath}!");
        }
    }
}
#endif
