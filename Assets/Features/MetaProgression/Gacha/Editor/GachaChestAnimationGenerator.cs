#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using System.IO;

namespace ProjectZombie.Features.MetaProgression.Gacha.Editor
{
    /// <summary>
    /// Tạo Animation Clip và Animator Controller cho Rương Gacha Frame-by-Frame từ các Sprite cắt sẵn.
    /// </summary>
    public static class GachaChestAnimationGenerator
    {
        [MenuItem("ProjectZombie/Gacha/Create Chest Animation & Controller", priority = 202)]
        public static void CreateChestAnimation()
        {
            string animDir = "Assets/Art/UI/Gacha/Animations";
            if (!Directory.Exists(animDir)) Directory.CreateDirectory(animDir);

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            // Nạp các frames (thử nạp Sprite, nếu chưa reimport kịp thì nạp Texture2D và ép kiểu)
            Sprite[] frames = new Sprite[8];
            for (int i = 1; i <= 8; i++)
            {
                string path = $"Assets/Art/UI/Gacha/Chest_Frame_{i:02d}.png";
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null)
                {
                    // Ép TextureImporter import lại nếu cần
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer != null)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.SaveAndReimport();
                        sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    }
                }
                frames[i - 1] = sprite;
            }

            if (frames[0] == null)
            {
                Debug.LogWarning("[GachaChestAnimationGenerator] Chưa nạp được các frame Chest. Hãy kiểm tra các file ảnh tại 'Assets/Art/UI/Gacha/Chest_Frame_01.png'.");
                return;
            }

            // 1. Tạo Idle Clip (Frame 1)
            string idleClipPath = $"{animDir}/Chest_Idle.anim";
            AnimationClip idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(idleClipPath);
            if (idleClip == null)
            {
                idleClip = new AnimationClip();
                AssetDatabase.CreateAsset(idleClip, idleClipPath);
            }
            idleClip.frameRate = 12;

            var idleCurveBinding = new EditorCurveBinding
            {
                type = typeof(UnityEngine.UI.Image),
                path = "",
                propertyName = "m_Sprite"
            };
            ObjectReferenceKeyframe[] idleKeyframes = new ObjectReferenceKeyframe[1];
            idleKeyframes[0] = new ObjectReferenceKeyframe { time = 0f, value = frames[0] };
            AnimationUtility.SetObjectReferenceCurve(idleClip, idleCurveBinding, idleKeyframes);

            // 2. Tạo Open Chest Clip (Frame 1 -> Frame 8)
            string openClipPath = $"{animDir}/Chest_Open.anim";
            AnimationClip openClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(openClipPath);
            if (openClip == null)
            {
                openClip = new AnimationClip();
                AssetDatabase.CreateAsset(openClip, openClipPath);
            }
            openClip.frameRate = 10;

            ObjectReferenceKeyframe[] openKeyframes = new ObjectReferenceKeyframe[8];
            for (int i = 0; i < 8; i++)
            {
                openKeyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i * (1f / 10f),
                    value = frames[i]
                };
            }
            AnimationUtility.SetObjectReferenceCurve(openClip, idleCurveBinding, openKeyframes);

            // 3. Tạo Animator Controller
            string controllerPath = $"{animDir}/Chest_AnimatorController.controller";
            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

            controller.AddParameter("OpenChest", AnimatorControllerParameterType.Trigger);

            var rootStateMachine = controller.layers[0].stateMachine;

            var idleState = rootStateMachine.AddState("Idle");
            idleState.motion = idleClip;

            var openState = rootStateMachine.AddState("Open");
            openState.motion = openClip;

            // Transition: Idle -> Open khi kích hoạt trigger OpenChest
            var toOpenTransition = idleState.AddTransition(openState);
            toOpenTransition.AddCondition(AnimatorConditionMode.If, 0, "OpenChest");
            toOpenTransition.hasExitTime = false;
            toOpenTransition.duration = 0f;

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[GachaChestAnimationGenerator] Đã tạo thành công Animation Clips và Controller tại '{animDir}'.");
        }
    }
}
#endif
