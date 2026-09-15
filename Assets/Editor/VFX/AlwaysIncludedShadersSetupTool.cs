#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProjectZombie.Editor.VFX
{
    /// <summary>
    /// Công cụ tự động thêm toàn bộ URP VFX Shaders vào 'Always Included Shaders' trong Graphics Settings.
    /// Giải quyết triệt để lỗi VFX bị biến thành màu hồng (Magenta #FF00FF) khi Build APK lên Android.
    /// </summary>
    public static class AlwaysIncludedShadersSetupTool
    {
        private static readonly string[] RequiredShaderNames = new string[]
        {
            "Universal Render Pipeline/Particles/Unlit",
            "Universal Render Pipeline/2D/Sprite-Unlit-Default",
            "ProjectZombie/VFX/Slash_Additive",
            "ProjectZombie/VFX/SonicWave_Additive",
            "ProjectZombie/VFX/Distortion_Shockwave",
            "ProjectZombie/VFX/GroundDecal_Dissolve",
            "ProjectZombie/VFX/URP_VFX_Ink_AlphaBlend",
            "ProjectZombie/Sprite_HitFlash"
        };

        [MenuItem("ProjectZombie/VFX/🎨 Bổ Sung URP Shaders Vào Always Included Shaders (Fix Pink VFX)", priority = 350)]
        public static void AddRequiredShaders()
        {
            GraphicsSettings graphicsSettings = GraphicsSettings.GetGraphicsSettings() as GraphicsSettings ?? AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
            if (graphicsSettings == null)
            {
                Debug.LogError("[AlwaysIncludedShaders] Không thể lấy GraphicsSettings từ Editor!");
                return;
            }

            SerializedObject serializedObj = new SerializedObject(graphicsSettings);
            SerializedProperty arrayProp = serializedObj.FindProperty("m_AlwaysIncludedShaders");
            if (arrayProp == null || !arrayProp.isArray)
            {
                Debug.LogError("[AlwaysIncludedShaders] Không tìm thấy thuộc tính m_AlwaysIncludedShaders!");
                return;
            }

            int addedCount = 0;
            foreach (string shaderName in RequiredShaderNames)
            {
                Shader shader = Shader.Find(shaderName);
                if (shader == null) continue;

                bool alreadyExists = false;
                for (int i = 0; i < arrayProp.arraySize; i++)
                {
                    var elem = arrayProp.GetArrayElementAtIndex(i);
                    if (elem.objectReferenceValue == shader)
                    {
                        alreadyExists = true;
                        break;
                    }
                }

                if (!alreadyExists)
                {
                    arrayProp.arraySize++;
                    var newElem = arrayProp.GetArrayElementAtIndex(arrayProp.arraySize - 1);
                    newElem.objectReferenceValue = shader;
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                serializedObj.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("Fix Pink VFX Success!", $"Đã tự động thêm thành công {addedCount} URP VFX Shaders vào Always Included Shaders trong Graphics Settings!\n\nLỗi màu hồng khi build Android đã được giải quyết triệt để.", "OK");
                Debug.Log($"<color=#00FF88>[AlwaysIncludedShaders]</color> Đã tự động thêm {addedCount} URP VFX Shaders vào Graphics Settings!");
            }
            else
            {
                EditorUtility.DisplayDialog("Graphics Settings OK", "Tất cả các URP VFX Shaders quan trọng đã có sẵn trong Always Included Shaders.", "OK");
                Debug.Log("<color=#00FF88>[AlwaysIncludedShaders]</color> Tất cả URP VFX Shaders đã có sẵn trong Always Included Shaders.");
            }
        }
    }
}
#endif
