#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace ProjectZombie.Editor.Tools
{
    [InitializeOnLoad]
    public static class AnSiAutoMetaConfigurator
    {
        [MenuItem("ProjectZombie/Art/Configure AnSi Meta & Clips Now")]
        public static void ConfigureAll()
        {
            SetupSpriteSheetAndClip("Assets/Art/AnSi/AnSi-Run.png", "Run.anim", 6, 12f, true);
            SetupSpriteSheetAndClip("Assets/Art/AnSi/AnSi-Attack.png", "Attack.anim", 6, 14f, false);
            SetupSpriteSheetAndClip("Assets/Art/AnSi/AnSi-Idle.png", "Idle.anim", 6, 6f, true);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AnSiAutoMetaConfigurator] Đã cấu hình xong Meta, Slices và Animation Clips cho Ẩn Sĩ Sơn Lâm!");
        }

        private static void SetupSpriteSheetAndClip(string texturePath, string animName, int frameCount, float sampleRate, bool isLooping)
        {
            if (!File.Exists(texturePath)) return;

            AssetDatabase.ImportAsset(texturePath, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer == null) return;

            int frameW = 128;
            int frameH = 128;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Point;
            importer.spritePixelsPerUnit = 64;
            importer.isReadable = true;

            List<SpriteMetaData> metas = new List<SpriteMetaData>();
            string baseName = Path.GetFileNameWithoutExtension(texturePath);

            for (int i = 0; i < frameCount; i++)
            {
                SpriteMetaData meta = new SpriteMetaData();
                meta.name = $"{baseName}_{i}";
                meta.rect = new Rect(i * frameW, 0, frameW, frameH);
                meta.alignment = 7; // Bottom-Center
                meta.pivot = new Vector2(0.5f, 0.0f);
                metas.Add(meta);
            }

#pragma warning disable CS0618
            importer.spritesheet = metas.ToArray();
#pragma warning restore CS0618
            importer.SaveAndReimport();

            // Cập nhật Animation Clip
            string animPath = Path.Combine("Assets/Art/AnSi", animName);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(animPath);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, animPath);
            }

            Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            List<Sprite> sprites = new List<Sprite>();
            foreach (var a in subAssets)
            {
                if (a is Sprite s) sprites.Add(s);
            }
            sprites.Sort((a, b) => a.name.CompareTo(b.name));

            if (sprites.Count > 0)
            {
                EditorCurveBinding binding = new EditorCurveBinding();
                binding.type = typeof(SpriteRenderer);
                binding.path = "";
                binding.propertyName = "m_Sprite";

                ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Count];
                float duration = 1f / sampleRate;
                for (int i = 0; i < sprites.Count; i++)
                {
                    keyframes[i] = new ObjectReferenceKeyframe
                    {
                        time = i * duration,
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
