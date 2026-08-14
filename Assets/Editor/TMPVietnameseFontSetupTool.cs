using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.TextCore.LowLevel;

namespace ProjectZombie.EditorTools
{
    /// <summary>
    /// Editor Tool cấu hình Fallback Font Assets cho TextMeshPro.
    /// Tự động liên kết Dynamic Fallback Font Asset vào TMP Settings và tất cả Font Asset mặc định.
    /// </summary>
    public static class TMPVietnameseFontSetupTool
    {
        private const string BAKED_FONT_PATH_1 = "Assets/TextMesh Pro/Fonts/GameFont_Vietnamese_SD.asset";
        private const string BAKED_FONT_PATH_2 = "Assets/TextMesh Pro/Resources/Fonts & Materials/GameFont_Vietnamese_SD.asset";

        [MenuItem("Tools/ProjectZombie/Font/Setup Vietnamese TMP Fallbacks", priority = 100)]
        public static void SetupVietnameseFallbacks()
        {
            // 1. Tải Font Asset tĩnh đã Bake full tiếng Việt
            TMP_FontAsset vietnameseFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BAKED_FONT_PATH_1);
            if (vietnameseFont == null)
            {
                vietnameseFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BAKED_FONT_PATH_2);
            }

            if (vietnameseFont == null)
            {
                Debug.LogError($"[TMPVietnameseFontSetupTool] Không tìm thấy font tĩnh '{BAKED_FONT_PATH_1}'. Vui lòng kiểm tra file!");
                return;
            }

            // 2. Dọn dẹp & Gán vào Default Font Asset (LiberationSans SDF) Fallback Table
            TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;
            if (defaultFont != null)
            {
                if (defaultFont.fallbackFontAssetTable == null)
                {
                    defaultFont.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
                }
                else
                {
                    defaultFont.fallbackFontAssetTable.RemoveAll(f => f == null);
                }

                if (defaultFont != vietnameseFont && !defaultFont.fallbackFontAssetTable.Contains(vietnameseFont))
                {
                    defaultFont.fallbackFontAssetTable.Insert(0, vietnameseFont);
                    EditorUtility.SetDirty(defaultFont);
                    Debug.Log($"<color=#4DEEEA>[TMPVietnameseFontSetupTool]</color> Đã thêm '{vietnameseFont.name}' vào Fallback Table của default font '{defaultFont.name}'.");
                }
            }

            // 3. Dọn dẹp & Gán vào TMP Settings Fallback List
            TMP_Settings settings = TMP_Settings.instance;
            if (settings != null)
            {
                SerializedObject settingsSo = new SerializedObject(settings);
                var fallbackListProp = settingsSo.FindProperty("m_fallbackFontAssets");
                if (fallbackListProp != null)
                {
                    // Dọn dẹp các entry null
                    for (int i = fallbackListProp.arraySize - 1; i >= 0; i--)
                    {
                        var elem = fallbackListProp.GetArrayElementAtIndex(i);
                        if (elem.objectReferenceValue == null)
                        {
                            fallbackListProp.DeleteArrayElementAtIndex(i);
                        }
                    }

                    bool alreadyExists = false;
                    for (int i = 0; i < fallbackListProp.arraySize; i++)
                    {
                        var elem = fallbackListProp.GetArrayElementAtIndex(i);
                        if (elem.objectReferenceValue == vietnameseFont)
                        {
                            alreadyExists = true;
                            break;
                        }
                    }

                    if (!alreadyExists)
                    {
                        fallbackListProp.InsertArrayElementAtIndex(0);
                        fallbackListProp.GetArrayElementAtIndex(0).objectReferenceValue = vietnameseFont;
                        Debug.Log($"<color=#4DEEEA>[TMPVietnameseFontSetupTool]</color> Đã thêm '{vietnameseFont.name}' vào đầu danh sách TMP Settings Fallback.");
                    }

                    settingsSo.ApplyModifiedProperties();
                    EditorUtility.SetDirty(settings);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=#4DEEEA><b>[TMPVietnameseFontSetupTool] Cấu hình hoàn tất!</b></color> Đã kích hoạt Font tĩnh Bake sẵn Full tiếng Việt.");
        }
    }
}
