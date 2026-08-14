using UnityEngine;
using UnityEditor;
using TMPro;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Editor Tool cấu hình Fallback Font Assets cho TextMeshPro.
    /// Tự động liên kết Dynamic Fallback Font Asset vào TMP Settings và tất cả Font Asset mặc định.
    /// </summary>
    public static class TMPVietnameseFontSetupTool
    {
        private const string FALLBACK_FONT_PATH = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset";

        [MenuItem("Tools/ProjectZombie/Font/Setup Vietnamese TMP Fallbacks", priority = 100)]
        public static void SetupVietnameseFallbacks()
        {
            // 1. Tải Dynamic Fallback Font Asset đã có sẵn trong TMP
            TMP_FontAsset fallbackFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FALLBACK_FONT_PATH);

            if (fallbackFont == null)
            {
                Debug.LogError($"[TMPVietnameseFontSetupTool] Không tìm thấy font asset tại '{FALLBACK_FONT_PATH}'!");
                return;
            }

            // Đảm bảo chế độ Dynamic
            if (fallbackFont.atlasPopulationMode != AtlasPopulationMode.Dynamic)
            {
                fallbackFont.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                EditorUtility.SetDirty(fallbackFont);
            }

            // 2. Gán vào Default Font Asset (LiberationSans SDF) Fallback Table
            TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;
            if (defaultFont != null && defaultFont != fallbackFont)
            {
                if (defaultFont.fallbackFontAssetTable == null)
                {
                    defaultFont.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
                }

                if (!defaultFont.fallbackFontAssetTable.Contains(fallbackFont))
                {
                    defaultFont.fallbackFontAssetTable.Add(fallbackFont);
                    EditorUtility.SetDirty(defaultFont);
                    Debug.Log($"[TMPVietnameseFontSetupTool] Đã thêm Dynamic Fallback vào default font '{defaultFont.name}'.");
                }
            }

            // 3. Gán vào TMP Settings Fallback List
            TMP_Settings settings = TMP_Settings.instance;
            if (settings != null)
            {
                SerializedObject settingsSo = new SerializedObject(settings);
                var fallbackListProp = settingsSo.FindProperty("m_fallbackFontAssets");
                if (fallbackListProp != null)
                {
                    bool alreadyExists = false;
                    for (int i = 0; i < fallbackListProp.arraySize; i++)
                    {
                        var elem = fallbackListProp.GetArrayElementAtIndex(i);
                        if (elem.objectReferenceValue == fallbackFont)
                        {
                            alreadyExists = true;
                            break;
                        }
                    }

                    if (!alreadyExists)
                    {
                        int index = fallbackListProp.arraySize;
                        fallbackListProp.InsertArrayElementAtIndex(index);
                        fallbackListProp.GetArrayElementAtIndex(index).objectReferenceValue = fallbackFont;
                        settingsSo.ApplyModifiedProperties();
                        EditorUtility.SetDirty(settings);
                        Debug.Log("[TMPVietnameseFontSetupTool] Đã thêm Dynamic Fallback vào TMP Settings.");
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=#4DEEEA><b>[TMPVietnameseFontSetupTool] Cấu hình hoàn tất!</b></color> Hệ thống Fallback Font TextMesh Pro đã được thiết lập chuẩn.");
        }
    }
}
