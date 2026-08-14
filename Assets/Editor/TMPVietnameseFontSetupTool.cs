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
        private const string FALLBACK_FONT_PATH = "Assets/TextMesh Pro/Resources/Fonts & Materials/Arial SDF - Fallback.asset";
        private const string SOURCE_TTF_PATH = "Assets/TextMesh Pro/Fonts/Arial.ttf";

        [MenuItem("Tools/ProjectZombie/Font/Setup Vietnamese TMP Fallbacks", priority = 100)]
        public static void SetupVietnameseFallbacks()
        {
            // 1. Kiểm tra và xóa file hỏng nếu có
            TMP_FontAsset fallbackFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FALLBACK_FONT_PATH);
            if (fallbackFont != null && (fallbackFont.material == null || fallbackFont.atlasTextures == null || fallbackFont.atlasTextures.Length == 0 || fallbackFont.atlasTextures[0] == null))
            {
                AssetDatabase.DeleteAsset(FALLBACK_FONT_PATH);
                fallbackFont = null;
            }

            if (fallbackFont == null)
            {
                Font sourceTtf = AssetDatabase.LoadAssetAtPath<Font>(SOURCE_TTF_PATH);
                if (sourceTtf == null)
                {
                    sourceTtf = Font.CreateDynamicFontFromOSFont("Arial", 16);
                }

                if (sourceTtf != null)
                {
                    fallbackFont = TMP_FontAsset.CreateFontAsset(sourceTtf, 90, 9, GlyphRenderMode.SDFAA, 512, 512, AtlasPopulationMode.Dynamic);
                    fallbackFont.name = "Arial SDF - Fallback";

                    string dir = System.IO.Path.GetDirectoryName(FALLBACK_FONT_PATH);
                    if (!System.IO.Directory.Exists(dir))
                    {
                        System.IO.Directory.CreateDirectory(dir);
                    }

                    // Lưu main asset
                    AssetDatabase.CreateAsset(fallbackFont, FALLBACK_FONT_PATH);

                    // Đính kèm Material và Atlas Textures vào sub-asset để không bị Garbage Collected / Destroyed
                    if (fallbackFont.material != null)
                    {
                        fallbackFont.material.name = fallbackFont.name + " Material";
                        AssetDatabase.AddObjectToAsset(fallbackFont.material, fallbackFont);
                    }

                    if (fallbackFont.atlasTextures != null)
                    {
                        for (int i = 0; i < fallbackFont.atlasTextures.Length; i++)
                        {
                            var tex = fallbackFont.atlasTextures[i];
                            if (tex != null)
                            {
                                tex.name = $"{fallbackFont.name} Atlas {i}";
                                AssetDatabase.AddObjectToAsset(tex, fallbackFont);
                            }
                        }
                    }

                    EditorUtility.SetDirty(fallbackFont);
                    AssetDatabase.SaveAssets();
                    Debug.Log($"<color=#4DEEEA>[TMPVietnameseFontSetupTool]</color> Đã tạo mới chuẩn Dynamic Font Asset từ Arial tại '{FALLBACK_FONT_PATH}'.");
                }
                else
                {
                    Debug.LogError($"[TMPVietnameseFontSetupTool] Không tìm thấy font TTF nguồn tại '{SOURCE_TTF_PATH}'!");
                    return;
                }
            }

            // Đảm bảo chế độ Dynamic
            if (fallbackFont.atlasPopulationMode != AtlasPopulationMode.Dynamic)
            {
                fallbackFont.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                EditorUtility.SetDirty(fallbackFont);
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
                    // Xóa các entry null hoặc trỏ sai
                    defaultFont.fallbackFontAssetTable.RemoveAll(f => f == null);
                }

                if (defaultFont != fallbackFont && !defaultFont.fallbackFontAssetTable.Contains(fallbackFont))
                {
                    defaultFont.fallbackFontAssetTable.Add(fallbackFont);
                    EditorUtility.SetDirty(defaultFont);
                    Debug.Log($"[TMPVietnameseFontSetupTool] Đã thêm Dynamic Fallback vào default font '{defaultFont.name}'.");
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
                    // Xóa các entry null ngược từ cuối về đầu
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
                        Debug.Log("[TMPVietnameseFontSetupTool] Đã thêm Dynamic Fallback vào TMP Settings.");
                    }

                    settingsSo.ApplyModifiedProperties();
                    EditorUtility.SetDirty(settings);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=#4DEEEA><b>[TMPVietnameseFontSetupTool] Cấu hình hoàn tất!</b></color> Hệ thống Fallback Font TextMesh Pro đã được thiết lập chuẩn.");
        }
    }
}
