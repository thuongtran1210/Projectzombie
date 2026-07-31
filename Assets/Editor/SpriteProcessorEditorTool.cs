using UnityEngine;
using UnityEditor;
using System.IO;

namespace ProjectZombie.Editor.Tools
{
    public enum SpriteSizePreset
    {
        Size8x8,
        Size16x16,
        Size32x32,
        Size64x64,
        Custom
    }

    public class SpriteProcessorEditorTool : EditorWindow
    {
        private Texture2D _sourceTexture;
        private SpriteSizePreset _sizePreset = SpriteSizePreset.Size16x16;
        private int _targetWidth = 16;
        private int _targetHeight = 16;
        
        private bool _removeBackground = true;
        private Color _backgroundColor = Color.white;
        private float _colorThreshold = 0.15f;

        private DefaultAsset _outputFolderAsset;
        private string _customFolderRelativePath = "Assets/_ART";

        private Texture2D _previewTextureSingle;
        private Texture2D _previewTexture8Way;
        private Vector2 _scrollPos;

        [MenuItem("Tools/ProjectZombie/Pixel Sprite Processor (Resize & 8-Way Rotation)")]
        public static void ShowWindow()
        {
            var win = GetWindow<SpriteProcessorEditorTool>("Pixel Sprite Processor");
            win.minSize = new Vector2(400, 550);
        }

        private void OnEnable()
        {
            // Auto find default folder
            if (_outputFolderAsset == null && Directory.Exists("Assets/_ART"))
            {
                _outputFolderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>("Assets/_ART");
            }
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            GUILayout.Label("🎨 Pixel Sprite Processor (v2.0)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Tool hỗ trợ xem trước (Preview), chọn vị trí lưu, đổi kích thước (8x8, 16x16, 32x32...) và xoay lật đạn 8 hướng.", MessageType.Info);
            EditorGUILayout.Space();

            // 1. Source Texture Selection
            EditorGUI.BeginChangeCheck();
            _sourceTexture = (Texture2D)EditorGUILayout.ObjectField("Source Texture", _sourceTexture, typeof(Texture2D), false);

            // 2. Preset & Size Selection
            _sizePreset = (SpriteSizePreset)EditorGUILayout.EnumPopup("Kích Thước (Preset)", _sizePreset);
            switch (_sizePreset)
            {
                case SpriteSizePreset.Size8x8:
                    _targetWidth = 8; _targetHeight = 8;
                    break;
                case SpriteSizePreset.Size16x16:
                    _targetWidth = 16; _targetHeight = 16;
                    break;
                case SpriteSizePreset.Size32x32:
                    _targetWidth = 32; _targetHeight = 32;
                    break;
                case SpriteSizePreset.Size64x64:
                    _targetWidth = 64; _targetHeight = 64;
                    break;
                case SpriteSizePreset.Custom:
                    _targetWidth = EditorGUILayout.IntField("Custom Width (px)", _targetWidth);
                    _targetHeight = EditorGUILayout.IntField("Custom Height (px)", _targetHeight);
                    break;
            }

            // 3. Background Removal Settings
            _removeBackground = EditorGUILayout.Toggle("Remove Background", _removeBackground);
            if (_removeBackground)
            {
                EditorGUI.indentLevel++;
                _backgroundColor = EditorGUILayout.ColorField("Background Color", _backgroundColor);
                _colorThreshold = EditorGUILayout.Slider("Color Threshold", _colorThreshold, 0.01f, 0.5f);
                EditorGUI.indentLevel--;
            }

            // 4. Output Path Selector
            EditorGUILayout.Space();
            GUILayout.Label("📁 Vị Trí Lưu File (Save Location)", EditorStyles.boldLabel);
            _outputFolderAsset = (DefaultAsset)EditorGUILayout.ObjectField("Folder Output", _outputFolderAsset, typeof(DefaultAsset), false);

            if (_outputFolderAsset != null)
            {
                string folderPath = AssetDatabase.GetAssetPath(_outputFolderAsset);
                if (Directory.Exists(folderPath))
                {
                    _customFolderRelativePath = folderPath;
                }
            }
            EditorGUILayout.LabelField("Lưu vào đường dẫn:", _customFolderRelativePath, EditorStyles.miniBoldLabel);

            if (EditorGUI.EndChangeCheck())
            {
                UpdatePreview();
            }

            EditorGUILayout.Space();

            // 5. Live Preview Area
            GUILayout.Label("🖼️ Xem Trước Ảnh (Realtime Preview)", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            if (_sourceTexture == null)
            {
                EditorGUILayout.HelpBox("Kéo gán Source Texture để xem trước kết quả xử lý.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                
                if (_previewTextureSingle != null)
                {
                    EditorGUILayout.BeginVertical(GUILayout.Width(120));
                    GUILayout.Label("16x16 Resized:", EditorStyles.miniLabel);
                    Rect r1 = GUILayoutGetRect(96, 96);
                    EditorGUI.DrawTextureTransparent(r1, _previewTextureSingle, ScaleMode.ScaleToFit);
                    EditorGUILayout.EndVertical();
                }

                if (_previewTexture8Way != null)
                {
                    EditorGUILayout.BeginVertical();
                    GUILayout.Label("8-Way Sprite Sheet:", EditorStyles.miniLabel);
                    Rect r2 = GUILayoutGetRect(220, 50);
                    EditorGUI.DrawTextureTransparent(r2, _previewTexture8Way, ScaleMode.ScaleToFit);
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // 6. Action Buttons
            GUI.enabled = _sourceTexture != null;
            if (GUILayout.Button("💾 1. Xuất Ảnh Single Sprite Resized", GUILayout.Height(32)))
            {
                SaveResizedSprite();
            }

            if (GUILayout.Button("🔄 2. Xuất Ảnh 8-Way Sprite Sheet (Bảng Gộp 8 Hướng)", GUILayout.Height(36)))
            {
                Save8WaySpriteSheet();
            }

            EditorGUILayout.Space();
            GUILayout.Label("🧭 Xuất Lẻ Từng Hướng Bay (Separate Directional PNGs)", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);

            string[] dirNames = new string[] { "0° (East - Phải)", "45° (NorthEast - Phải Trên)", "90° (North - Trên)", "135° (NorthWest - Trái Trên)", "180° (West - Trái)", "225° (SouthWest - Trái Dưới)", "270° (South - Dưới)", "315° (SouthEast - Phải Dưới)" };
            float[] dirAngles = new float[] { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };
            string[] dirShortCodes = new string[] { "East", "NE", "North", "NW", "West", "SW", "South", "SE" };

            for (int i = 0; i < 8; i += 2)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button($"➡️ {dirNames[i]}", GUILayout.Height(26)))
                {
                    SaveSingleDirectionalSprite(dirAngles[i], dirShortCodes[i]);
                }
                if (GUILayout.Button($"➡️ {dirNames[i + 1]}", GUILayout.Height(26)))
                {
                    SaveSingleDirectionalSprite(dirAngles[i + 1], dirShortCodes[i + 1]);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(4);
            if (GUILayout.Button("📦 XUẤT TRỌN BỘ 8 FILE LẺ (Batch Export All 8 PNGs)", GUILayout.Height(32)))
            {
                SaveAll8DirectionalSprites(dirAngles, dirShortCodes);
            }

            EditorGUILayout.EndVertical();
            GUI.enabled = true;

            EditorGUILayout.EndScrollView();
        }

        private void SaveSingleDirectionalSprite(float angle, string dirName)
        {
            if (_previewTextureSingle == null) UpdatePreview();

            Texture2D rotated = RotateTexture(_previewTextureSingle, angle);
            byte[] bytes = rotated.EncodeToPNG();
            string filename = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(_sourceTexture));
            string outPath = Path.Combine(_customFolderRelativePath, $"{filename}_{dirName}_{_targetWidth}x{_targetHeight}.png");

            File.WriteAllBytes(outPath, bytes);
            AssetDatabase.Refresh();
            ConfigureAsSprite(outPath);
            EditorUtility.DisplayDialog("Thành công", $"Đã xuất file Hướng [{dirName}] thành công:\n{outPath}", "OK");
        }

        private void SaveAll8DirectionalSprites(float[] angles, string[] dirCodes)
        {
            if (_previewTextureSingle == null) UpdatePreview();

            string filename = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(_sourceTexture));
            int count = 0;

            for (int i = 0; i < angles.Length; i++)
            {
                Texture2D rotated = RotateTexture(_previewTextureSingle, angles[i]);
                byte[] bytes = rotated.EncodeToPNG();
                string outPath = Path.Combine(_customFolderRelativePath, $"{filename}_{dirCodes[i]}_{_targetWidth}x{_targetHeight}.png");

                File.WriteAllBytes(outPath, bytes);
                ConfigureAsSprite(outPath);
                count++;
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Thành công", $"Đã xuất thành công trọn bộ {count} file PNG lẻ theo 8 hướng vào thư mục:\n{_customFolderRelativePath}", "OK");
        }

        private Rect GUILayoutGetRect(float width, float height)
        {
            return GUILayoutUtility.GetRect(width, height, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
        }

        private void UpdatePreview()
        {
            if (_sourceTexture == null)
            {
                _previewTextureSingle = null;
                _previewTexture8Way = null;
                return;
            }

            MakeTextureReadable(_sourceTexture);

            // Single Preview
            _previewTextureSingle = ResizePixelArt(_sourceTexture, _targetWidth, _targetHeight);
            if (_removeBackground)
            {
                RemoveBg(_previewTextureSingle, _backgroundColor, _colorThreshold);
            }

            // 8-Way Preview
            _previewTexture8Way = Build8WayTexture(_previewTextureSingle, _targetWidth, _targetHeight);
        }

        private void SaveResizedSprite()
        {
            if (_previewTextureSingle == null) UpdatePreview();

            byte[] bytes = _previewTextureSingle.EncodeToPNG();
            string filename = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(_sourceTexture));
            string outPath = Path.Combine(_customFolderRelativePath, $"{filename}_{_targetWidth}x{_targetHeight}.png");

            File.WriteAllBytes(outPath, bytes);
            AssetDatabase.Refresh();
            ConfigureAsSprite(outPath);
            EditorUtility.DisplayDialog("Thành công", $"Đã xuất file Single Sprite thành công:\n{outPath}", "OK");
        }

        private void Save8WaySpriteSheet()
        {
            if (_previewTexture8Way == null) UpdatePreview();

            byte[] bytes = _previewTexture8Way.EncodeToPNG();
            string filename = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(_sourceTexture));
            string outPath = Path.Combine(_customFolderRelativePath, $"{filename}_8Way_{_targetWidth}x{_targetHeight}.png");

            File.WriteAllBytes(outPath, bytes);
            AssetDatabase.Refresh();
            ConfigureAsSprite(outPath);
            EditorUtility.DisplayDialog("Thành công", $"Đã xuất file 8-Way Sprite Sheet thành công:\n{outPath}", "OK");
        }

        private Texture2D Build8WayTexture(Texture2D baseSprite, int cellW, int cellH)
        {
            Texture2D spriteSheet = new Texture2D(cellW * 8, cellH, TextureFormat.RGBA32, false);
            spriteSheet.filterMode = FilterMode.Point;

            Color[] clearColors = new Color[spriteSheet.width * spriteSheet.height];
            for (int i = 0; i < clearColors.Length; i++) clearColors[i] = Color.clear;
            spriteSheet.SetPixels(clearColors);

            float[] angles = new float[] { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };

            for (int col = 0; col < 8; col++)
            {
                Texture2D rotated = RotateTexture(baseSprite, angles[col]);
                Color[] rotatedPixels = rotated.GetPixels();
                spriteSheet.SetPixels(col * cellW, 0, cellW, cellH, rotatedPixels);
            }

            spriteSheet.Apply();
            return spriteSheet;
        }

        private Texture2D ResizePixelArt(Texture2D source, int targetW, int targetH)
        {
            Texture2D result = new Texture2D(targetW, targetH, TextureFormat.RGBA32, false);
            result.filterMode = FilterMode.Point;

            float ratioX = (float)source.width / targetW;
            float ratioY = (float)source.height / targetH;

            for (int y = 0; y < targetH; y++)
            {
                for (int x = 0; x < targetW; x++)
                {
                    int srcX = Mathf.FloorToInt(x * ratioX);
                    int srcY = Mathf.FloorToInt(y * ratioY);
                    result.SetPixel(x, y, source.GetPixel(srcX, srcY));
                }
            }
            result.Apply();
            return result;
        }

        private Texture2D RotateTexture(Texture2D source, float angle)
        {
            int w = source.width;
            int h = source.height;
            Texture2D result = new Texture2D(w, h, TextureFormat.RGBA32, false);
            result.filterMode = FilterMode.Point;

            float rad = angle * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);

            Vector2 center = new Vector2(w / 2f, h / 2f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Vector2 pos = new Vector2(x - center.x, y - center.y);
                    int srcX = Mathf.RoundToInt(pos.x * cos + pos.y * sin + center.x);
                    int srcY = Mathf.RoundToInt(-pos.x * sin + pos.y * cos + center.y);

                    if (srcX >= 0 && srcX < w && srcY >= 0 && srcY < h)
                    {
                        result.SetPixel(x, y, source.GetPixel(srcX, srcY));
                    }
                    else
                    {
                        result.SetPixel(x, y, Color.clear);
                    }
                }
            }
            result.Apply();
            return result;
        }

        private void RemoveBg(Texture2D tex, Color bgCol, float threshold)
        {
            Color[] pixels = tex.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                float diff = Mathf.Abs(pixels[i].r - bgCol.r) + Mathf.Abs(pixels[i].g - bgCol.g) + Mathf.Abs(pixels[i].b - bgCol.b);
                if (diff < threshold)
                {
                    pixels[i] = Color.clear;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();
        }

        private void MakeTextureReadable(Texture2D tex)
        {
            string path = AssetDatabase.GetAssetPath(tex);
            if (string.IsNullOrEmpty(path)) return;

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && !importer.isReadable)
            {
                importer.isReadable = true;
                importer.SaveAndReimport();
            }
        }

        private void ConfigureAsSprite(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }
    }
}

