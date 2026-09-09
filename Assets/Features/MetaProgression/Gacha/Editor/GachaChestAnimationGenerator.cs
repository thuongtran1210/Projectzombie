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

            // Nạp các frames
            Sprite[] frames = new Sprite[8];
            for (int i = 1; i <= 8; i++)
            {
                string path = $"Assets/Art/UI/Gacha/Chest_Frame_{i:02d}.png";
                
                // 1. Kiểm tra và đảm bảo TextureImporter đã import dạng Sprite
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null && (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single))
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.alphaIsTransparency = true;
                    importer.SaveAndReimport();
                }

                // 2. Load Sprite từ AssetDatabase
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                // 3. Fallback: duyệt qua all sub-assets
                if (sprite == null)
                {
                    var all = AssetDatabase.LoadAllAssetsAtPath(path);
                    foreach (var a in all)
                    {
                        if (a is Sprite s)
                        {
                            sprite = s;
                            break;
                        }
                    }
                }

                frames[i - 1] = sprite;
            }

            if (frames[0] == null)
            {
                // Fallback từ Spritesheet nếu frame rời không tìm thấy
                string sheetPath = "Assets/Art/UI/Gacha/UI_Gacha_Chest_Spritesheet.png";
                Texture2D sheetTex = AssetDatabase.LoadAssetAtPath<Texture2D>(sheetPath);
                if (sheetTex != null)
                {
                    var fbSprite = Sprite.Create(sheetTex, new Rect(0, 0, sheetTex.width, sheetTex.height), new Vector2(0.5f, 0.5f), 100f);
                    for (int i = 0; i < 8; i++) frames[i] = fbSprite;
                }
            }

            if (frames[0] == null)
            {
                Debug.LogWarning("[GachaChestAnimationGenerator] Chưa nạp được các frame Chest. Hãy kiểm tra các file ảnh tại 'Assets/Art/UI/Gacha/Chest_Frame_01.png'.");
                return;
            }

            // 1. Tạo Idle Clip (Frame 1) trực tiếp bằng YAML chuẩn xác với GUID
            string idleClipPath = $"{animDir}/Chest_Idle.anim";
            string idleYaml = @"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!74 &7400000
AnimationClip:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: Chest_Idle
  serializedVersion: 7
  m_Legacy: 0
  m_Compressed: 0
  m_UseHighQualityCurve: 1
  m_RotationCurves: []
  m_CompressedRotationCurves: []
  m_EulerCurves: []
  m_PositionCurves: []
  m_ScaleCurves: []
  m_FloatCurves: []
  m_PPtrCurves:
  - serializedVersion: 2
    curve:
    - time: 0
      value: {fileID: 21300000, guid: 9d0a106ac41e5124e94c4ce9b5469ea3, type: 3}
    attribute: m_Sprite
    path: 
    classID: 114
    script: {fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}
    flags: 2
  m_SampleRate: 12
  m_WrapMode: 0
  m_Bounds:
    m_Center: {x: 0, y: 0, z: 0}
    m_Extent: {x: 0, y: 0, z: 0}
  m_ClipBindingConstant:
    genericBindings:
    - serializedVersion: 2
      path: 0
      attribute: 2015549526
      script: {fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}
      typeID: 114
      customType: 0
      isPPtrCurve: 1
      isIntCurve: 0
      isSerializeReferenceCurve: 0
    pptrCurveMapping:
    - {fileID: 21300000, guid: 9d0a106ac41e5124e94c4ce9b5469ea3, type: 3}
  m_AnimationClipSettings:
    serializedVersion: 2
    m_AdditiveReferencePoseClip: {fileID: 0}
    m_AdditiveReferencePoseTime: 0
    m_StartTime: 0
    m_StopTime: 0.083333336
    m_OrientationOffsetY: 0
    m_Level: 0
    m_CycleOffset: 0
    m_HasAdditiveReferencePose: 0
    m_LoopTime: 1
    m_LoopBlend: 0
    m_LoopBlendOrientation: 0
    m_LoopBlendPositionY: 0
    m_LoopBlendPositionXZ: 0
    m_KeepOriginalOrientation: 0
    m_KeepOriginalPositionY: 1
    m_KeepOriginalPositionXZ: 0
    m_HeightFromFeet: 0
    m_Mirror: 0
  m_EditorCurves: []
  m_EulerEditorCurves: []
  m_HasGenericRootTransform: 0
  m_HasMotionFloatCurves: 0
  m_Events: []";
            File.WriteAllText(idleClipPath, idleYaml);

            // 2. Tạo Open Chest Clip (Frame 1 -> Frame 8) trực tiếp bằng YAML chuẩn xác
            string openClipPath = $"{animDir}/Chest_Open.anim";
            string openYaml = @"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!74 &7400000
AnimationClip:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: Chest_Open
  serializedVersion: 7
  m_Legacy: 0
  m_Compressed: 0
  m_UseHighQualityCurve: 1
  m_RotationCurves: []
  m_CompressedRotationCurves: []
  m_EulerCurves: []
  m_PositionCurves: []
  m_ScaleCurves: []
  m_FloatCurves: []
  m_PPtrCurves:
  - serializedVersion: 2
    curve:
    - time: 0
      value: {fileID: 21300000, guid: 9d0a106ac41e5124e94c4ce9b5469ea3, type: 3}
    - time: 0.1
      value: {fileID: 21300000, guid: fc9be697ac17e9b40a432144a3407fbd, type: 3}
    - time: 0.2
      value: {fileID: 21300000, guid: 51b21e91d0af7b64f9b38ad352e2287b, type: 3}
    - time: 0.3
      value: {fileID: 21300000, guid: c553b1730eb94c949a93736f8cd2ffcc, type: 3}
    - time: 0.4
      value: {fileID: 21300000, guid: 08f368f22ad436243b944d496d4ae241, type: 3}
    - time: 0.5
      value: {fileID: 21300000, guid: 4cb4dce49c569234dbcdef43c59bcb00, type: 3}
    - time: 0.6
      value: {fileID: 21300000, guid: f398b245999a96e44a73c1e305d657c7, type: 3}
    - time: 0.7
      value: {fileID: 21300000, guid: 5149e9994bfdfcf43b71621eb63eb835, type: 3}
    attribute: m_Sprite
    path: 
    classID: 114
    script: {fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}
    flags: 2
  m_SampleRate: 10
  m_WrapMode: 0
  m_Bounds:
    m_Center: {x: 0, y: 0, z: 0}
    m_Extent: {x: 0, y: 0, z: 0}
  m_ClipBindingConstant:
    genericBindings:
    - serializedVersion: 2
      path: 0
      attribute: 2015549526
      script: {fileID: 11500000, guid: fe87c0e1cc204ed48ad3b37840f39efc, type: 3}
      typeID: 114
      customType: 0
      isPPtrCurve: 1
      isIntCurve: 0
      isSerializeReferenceCurve: 0
    pptrCurveMapping:
    - {fileID: 21300000, guid: 9d0a106ac41e5124e94c4ce9b5469ea3, type: 3}
    - {fileID: 21300000, guid: fc9be697ac17e9b40a432144a3407fbd, type: 3}
    - {fileID: 21300000, guid: 51b21e91d0af7b64f9b38ad352e2287b, type: 3}
    - {fileID: 21300000, guid: c553b1730eb94c949a93736f8cd2ffcc, type: 3}
    - {fileID: 21300000, guid: 08f368f22ad436243b944d496d4ae241, type: 3}
    - {fileID: 21300000, guid: 4cb4dce49c569234dbcdef43c59bcb00, type: 3}
    - {fileID: 21300000, guid: f398b245999a96e44a73c1e305d657c7, type: 3}
    - {fileID: 21300000, guid: 5149e9994bfdfcf43b71621eb63eb835, type: 3}
  m_AnimationClipSettings:
    serializedVersion: 2
    m_AdditiveReferencePoseClip: {fileID: 0}
    m_AdditiveReferencePoseTime: 0
    m_StartTime: 0
    m_StopTime: 0.8
    m_OrientationOffsetY: 0
    m_Level: 0
    m_CycleOffset: 0
    m_HasAdditiveReferencePose: 0
    m_LoopTime: 0
    m_LoopBlend: 0
    m_LoopBlendOrientation: 0
    m_LoopBlendPositionY: 0
    m_LoopBlendPositionXZ: 0
    m_KeepOriginalOrientation: 0
    m_KeepOriginalPositionY: 1
    m_KeepOriginalPositionXZ: 0
    m_HeightFromFeet: 0
    m_Mirror: 0
  m_EditorCurves: []
  m_EulerEditorCurves: []
  m_HasGenericRootTransform: 0
  m_HasMotionFloatCurves: 0
  m_Events: []";
            File.WriteAllText(openClipPath, openYaml);

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            AnimationClip idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(idleClipPath);
            AnimationClip openClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(openClipPath);

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
