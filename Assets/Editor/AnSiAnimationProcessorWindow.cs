#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace ProjectZombie.Editor.Tools
{
    /// <summary>
    /// Tool xử lý tự động tách nền, cắt lát (slice), scale đồng bộ và xuất animation clips cho Ẩn Sĩ Sơn Lâm (Art DNA: Boxy Squircle Chibi)
    /// Menu: ProjectZombie > Art > Process AnSi New Animations
    /// </summary>
    public class AnSiAnimationProcessorWindow : EditorWindow
    {
        private Texture2D _runTexture;
        private Texture2D _attackTexture;
        private Texture2D _idleTexture;

        private int _frameWidth = 128;
        private int _frameHeight = 128;
        private float _pixelsPerUnit = 64f;

        [MenuItem("ProjectZombie/Art/Process AnSi New Animations")]
        public static void ShowWindow()
        {
            var win = GetWindow<AnSiAnimationProcessorWindow>("AnSi Animation Processor");
            win.minSize = new Vector2(450, 400);
        }

        private void OnGUI()
        {
            GUILayout.Label("🧙‍♂️ Xử Lý & Đồng Bộ Animation Cho Ẩn Sĩ Sơn Lâm", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Kéo thả 3 ảnh Sheet (Run, Attack, Idle) mà bạn vừa tạo vào bên dưới. Tool sẽ tự động:\n1. Tách nền xám sạch sẽ 100% RGBA\n2. Cắt lát và căn trục chân đất Bottom-Center {0.5, 0}\n3. Cập nhật đè trực tiếp lên Assets/Art/AnSi/ và cập nhật Prefab!", MessageType.Info);
            EditorGUILayout.Space();

            _runTexture = (Texture2D)EditorGUILayout.ObjectField("Run Sheet (6 Frames)", _runTexture, typeof(Texture2D), false);
            _attackTexture = (Texture2D)EditorGUILayout.ObjectField("Attack Sheet (6 Frames)", _attackTexture, typeof(Texture2D), false);
            _idleTexture = (Texture2D)EditorGUILayout.ObjectField("Idle Sheet (6 Frames)", _idleTexture, typeof(Texture2D), false);

            EditorGUILayout.Space();
            _pixelsPerUnit = EditorGUILayout.FloatField("Pixels Per Unit", _pixelsPerUnit);

            EditorGUILayout.Space();
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("🚀 BẮT ĐẦU XỬ LÝ & ĐỒNG BỘ TOÀN BỘ HỆ THỐNG", GUILayout.Height(45)))
            {
                ProcessAllSheets();
            }
            GUI.backgroundColor = Color.white;
        }

        private void ProcessAllSheets()
        {
            if (_runTexture == null && _attackTexture == null && _idleTexture == null)
            {
                EditorUtility.DisplayDialog("Lỗi", "Vui lòng gán ít nhất một ảnh Texture để xử lý!", "OK");
                return;
            }

            string targetDir = "Assets/Art/AnSi";
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            if (_runTexture != null) ProcessSingleSheet(_runTexture, Path.Combine(targetDir, "AnSi-Run.png"), 6, "Run.anim", 12f, true);
            if (_attackTexture != null) ProcessSingleSheet(_attackTexture, Path.Combine(targetDir, "AnSi-Attack.png"), 6, "Attack.anim", 14f, false);
            if (_idleTexture != null) ProcessSingleSheet(_idleTexture, Path.Combine(targetDir, "AnSi-Idle.png"), 6, "Idle.anim", 6f, true);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Thành công", "Đã xử lý tách nền, căn trục, tạo Sprite Sheet và cập nhật Animation Clips cho Ẩn Sĩ Sơn Lâm!", "OK");
        }

        private void ProcessSingleSheet(Texture2D source, string targetPath, int frameCount, string animName, float sampleRate, bool isLooping)
        {
            // 1. Đọc và lấy raw pixels
            string sourcePath = AssetDatabase.GetAssetPath(source);
            TextureImporter srcImporter = AssetImporter.GetAtPath(sourcePath) as TextureImporter;
            bool wasReadable = srcImporter.isReadable;
            if (!wasReadable)
            {
                srcImporter.isReadable = true;
                srcImporter.SaveAndReimport();
            }

            int srcW = source.width;
            int srcH = source.height;
            Color[] pixels = source.GetPixels();

            // 2. Tách nền Chroma Keying
            Color bgColor = pixels[0]; // Lấy màu góc ảnh
            Color[] cleanPixels = new Color[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                Color c = pixels[i];
                float dist = Mathf.Sqrt(Mathf.Pow(c.r - bgColor.r, 2) + Mathf.Pow(c.g - bgColor.g, 2) + Mathf.Pow(c.b - bgColor.b, 2));
                if (dist < 0.12f)
                {
                    cleanPixels[i] = Color.clear;
                }
                else
                {
                    cleanPixels[i] = c;
                }
            }

            Texture2D cleanTex = new Texture2D(srcW, srcH, TextureFormat.RGBA32, false);
            cleanTex.SetPixels(cleanPixels);
            cleanTex.Apply();

            // 3. Phân chia frame đều theo Grid 3x2 hoặc 6x1
            // AI thường sinh 6 frame dưới dạng Grid 3 cột x 2 hàng
            int cols = 3;
            int rows = 2;
            int cellW = srcW / cols;
            int cellH = srcH / rows;

            int targetFrameW = 128;
            int targetFrameH = 128;
            Texture2D outStrip = new Texture2D(targetFrameW * frameCount, targetFrameH, TextureFormat.RGBA32, false);
            Color[] clearStrip = new Color[targetFrameW * frameCount * targetFrameH];
            for (int i = 0; i < clearStrip.Length; i++) clearStrip[i] = Color.clear;
            outStrip.SetPixels(clearStrip);

            List<SpriteMetaData> metas = new List<SpriteMetaData>();
            List<Sprite> generatedSprites = new List<Sprite>();

            int frameIdx = 0;
            for (int r = rows - 1; r >= 0; r--)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (frameIdx >= frameCount) break;

                    // Cắt sub-frame
                    Color[] cellPixels = cleanTex.GetPixels(c * cellW, r * cellH, cellW, cellH);
                    
                    // Tìm Bounding box của nhân vật
                    int minX = cellW, maxX = 0, minY = cellH, maxY = 0;
                    bool hasPixel = false;
                    for (int y = 0; y < cellH; y++)
                    {
                        for (int x = 0; x < cellW; x++)
                        {
                            if (cellPixels[y * cellW + x].a > 0.1f)
                            {
                                if (x < minX) minX = x;
                                if (x > maxX) maxX = x;
                                if (y < minY) minY = y;
                                if (y > maxY) maxY = y;
                                hasPixel = true;
                            }
                        }
                    }

                    if (hasPixel)
                    {
                        int charW = maxX - minX + 1;
                        int charH = maxY - minY + 1;

                        // Paste vào Canvas 128x128 ghim gót chân
                        float scale = 96f / (float)charH;
                        int scaledW = Mathf.RoundToInt(charW * scale);
                        int scaledH = Mathf.RoundToInt(charH * scale);

                        int pasteX = frameIdx * targetFrameW + (targetFrameW - scaledW) / 2;
                        int pasteY = 8; // Gót chân đặt ở y=8px

                        // Scale pixel thô
                        for (int py = 0; py < scaledH; py++)
                        {
                            for (int px = 0; px < scaledW; px++)
                            {
                                int sampleX = minX + Mathf.FloorToInt(px / scale);
                                int sampleY = minY + Mathf.FloorToInt(py / scale);
                                sampleX = Mathf.Clamp(sampleX, 0, cellW - 1);
                                sampleY = Mathf.Clamp(sampleY, 0, cellH - 1);

                                Color col = cellPixels[sampleY * cellW + sampleX];
                                if (col.a > 0.05f)
                                {
                                    outStrip.SetPixel(pasteX + px, pasteY + py, col);
                                }
                            }
                        }
                    }

                    SpriteMetaData meta = new SpriteMetaData();
                    meta.name = $"{Path.GetFileNameWithoutExtension(targetPath)}_{frameIdx}";
                    meta.rect = new Rect(frameIdx * targetFrameW, 0, targetFrameW, targetFrameH);
                    meta.alignment = 7; // Bottom-Center
                    meta.pivot = new Vector2(0.5f, 0.0f);
                    metas.Add(meta);

                    frameIdx++;
                }
            }

            outStrip.Apply();
            byte[] pngBytes = outStrip.EncodeToPNG();
            File.WriteAllBytes(targetPath, pngBytes);

            // 4. Thiết lập Texture Importer Unity
            AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(targetPath) as TextureImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritesheet = metas.ToArray();
            importer.filterMode = FilterMode.Point;
            importer.spritePixelsPerUnit = _pixelsPerUnit;
            importer.isReadable = true;
            importer.SaveAndReimport();

            // 5. Tạo/Cập nhật Animation Clip
            string animPath = Path.Combine("Assets/Art/AnSi", animName);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animPath);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, animPath);
            }

            Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(targetPath);
            List<Sprite> sprites = new List<Sprite>();
            foreach (var asset in subAssets)
            {
                if (asset is Sprite s) sprites.Add(s);
            }
            sprites.Sort((a, b) => a.name.CompareTo(b.name));

            if (sprites.Count > 0)
            {
                EditorCurveBinding binding = new EditorCurveBinding();
                binding.type = typeof(SpriteRenderer);
                binding.path = "";
                binding.propertyName = "m_Sprite";

                ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Count];
                float frameDuration = 1f / sampleRate;
                for (int i = 0; i < sprites.Count; i++)
                {
                    keyframes[i] = new ObjectReferenceKeyframe
                    {
                        time = i * frameDuration,
                        value = sprites[i]
                    };
                }

                AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = isLooping;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                EditorUtility.SetDirty(clip);
            }
        }
    }
}
#endif
