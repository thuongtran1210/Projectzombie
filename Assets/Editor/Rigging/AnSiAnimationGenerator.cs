using UnityEditor;
using UnityEngine;

namespace Projectzombie.Editor.Rigging
{
    public static class AnSiAnimationGenerator
    {
        private const string AnimationDir = "Assets/Art/AnSi/Rigging";

        [MenuItem("Tools/Rigging/Generate AnSi Rig Animation Clips")]
        public static void GenerateAllClips()
        {
            if (!System.IO.Directory.Exists(AnimationDir))
            {
                System.IO.Directory.CreateDirectory(AnimationDir);
            }

            CreateIdleClip();
            CreateRunClip();
            CreateAttackClip();
            CreateDashClip();
            CreateDeadClip();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=#4CAF50><b>[AnSiAnimationGenerator]</b> Đã tạo thành công toàn bộ 5 Animation Clips (Idle, Run, Attack, Dash, Dead) cho Ẩn Sĩ Sơn Lâm!</color>");
        }

        #region 1. Idle Clip (1.2s, Loop)
        private static void CreateIdleClip()
        {
            var clip = new AnimationClip { name = "Idle_Rig", frameRate = 60 };
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            // Path 1: Hips (Thở nhún bụng/thân)
            clip.SetCurve("Bone_Root/Hips", typeof(Transform), "localPosition.y", 
                CreateSmoothCurve(new (float, float)[] { (0f, 0.4f), (0.6f, 0.43f), (1.2f, 0.4f) }));

            // Path 2: Chest (Thở lồng ngực)
            clip.SetCurve("Bone_Root/Hips/Chest", typeof(Transform), "localPosition.y", 
                CreateSmoothCurve(new (float, float)[] { (0f, 0.25f), (0.6f, 0.27f), (1.2f, 0.25f) }));

            // Path 3: Head (Gật nhẹ)
            clip.SetCurve("Bone_Root/Hips/Chest/Head", typeof(Transform), "localEulerAnglesRaw.z", 
                CreateSmoothCurve(new (float, float)[] { (0f, 0f), (0.6f, -1.8f), (1.2f, 0f) }));

            // Path 4: Arm_R (Tay chống gậy dập dềnh nhẹ)
            clip.SetCurve("Bone_Root/Hips/Chest/Arm_R", typeof(Transform), "localEulerAnglesRaw.z", 
                CreateSmoothCurve(new (float, float)[] { (0f, 0f), (0.6f, 2.5f), (1.2f, 0f) }));

            // Path 5: Arm_L (Tay trái đung đưa)
            clip.SetCurve("Bone_Root/Hips/Chest/Arm_L", typeof(Transform), "localEulerAnglesRaw.z", 
                CreateSmoothCurve(new (float, float)[] { (0f, 0f), (0.6f, -2.0f), (1.2f, 0f) }));

            // Path 6: Gourd (Hồ lô lắc nhẹ)
            clip.SetCurve("Bone_Root/Hips/Gourd", typeof(Transform), "localEulerAnglesRaw.z", 
                CreateSmoothCurve(new (float, float)[] { (0f, 0f), (0.3f, 3f), (0.9f, -3f), (1.2f, 0f) }));

            SaveOrReplaceClip(clip, $"{AnimationDir}/Idle_Rig.anim");
        }
        #endregion

        #region 2. Run Clip (0.6s, Loop)
        private static void CreateRunClip()
        {
            var clip = new AnimationClip { name = "Run_Rig", frameRate = 60 };
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            // Nhún người chạy
            clip.SetCurve("Bone_Root/Hips", typeof(Transform), "localPosition.y", 
                CreateSmoothCurve(new (float, float)[] { (0f, 0.38f), (0.15f, 0.44f), (0.3f, 0.38f), (0.45f, 0.44f), (0.6f, 0.38f) }));
            clip.SetCurve("Bone_Root/Hips", typeof(Transform), "localEulerAnglesRaw.z", 
                CreateSmoothCurve(new (float, float)[] { (0f, 4f), (0.3f, -4f), (0.6f, 4f) }));

            // Đầu chúi nhẹ về trước
            clip.SetCurve("Bone_Root/Hips/Chest/Head", typeof(Transform), "localEulerAnglesRaw.z", 
                CreateSmoothCurve(new (float, float)[] { (0f, 5f), (0.15f, 2f), (0.3f, 5f), (0.45f, 2f), (0.6f, 5f) }));

            // Chân phải (Leg_R)
            clip.SetCurve("Bone_Root/Hips/Leg_R", typeof(Transform), "localEulerAnglesRaw.z", 
                CreateSmoothCurve(new (float, float)[] { (0f, 28f), (0.3f, -32f), (0.6f, 28f) }));

            // Chân trái (Leg_L) - Pha đối xứng lệch 0.3s
            clip.SetCurve("Bone_Root/Hips/Leg_L", typeof(Transform), "localEulerAnglesRaw.z", 
                CreateSmoothCurve(new (float, float)[] { (0f, -32f), (0.3f, 28f), (0.6f, -32f) }));

            // Tay phải vung gậy
            clip.SetCurve("Bone_Root/Hips/Chest/Arm_R", typeof(Transform), "localEulerAnglesRaw.z", 
                CreateSmoothCurve(new (float, float)[] { (0f, -20f), (0.3f, 25f), (0.6f, -20f) }));

            // Tay trái đánh nhịp
            clip.SetCurve("Bone_Root/Hips/Chest/Arm_L", typeof(Transform), "localEulerAnglesRaw.z", 
                CreateSmoothCurve(new (float, float)[] { (0f, 25f), (0.3f, -20f), (0.6f, 25f) }));

            // Hồ lô & Nón lắc mạnh khi chạy
            clip.SetCurve("Bone_Root/Hips/Gourd", typeof(Transform), "localEulerAnglesRaw.z", 
                CreateSmoothCurve(new (float, float)[] { (0f, -12f), (0.15f, 8f), (0.3f, -12f), (0.45f, 8f), (0.6f, -12f) }));
            clip.SetCurve("Bone_Root/Hat", typeof(Transform), "localEulerAnglesRaw.z", 
                CreateSmoothCurve(new (float, float)[] { (0f, -6f), (0.3f, 6f), (0.6f, -6f) }));

            SaveOrReplaceClip(clip, $"{AnimationDir}/Run_Rig.anim");
        }
        #endregion

        #region 3. Attack Clip (0.45s, Single Play)
        private static void CreateAttackClip()
        {
            var clip = new AnimationClip { name = "Attack_Rig", frameRate = 60 };
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = false;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            // Thân rướn
            clip.SetCurve("Bone_Root/Hips", typeof(Transform), "localPosition.x", 
                CreateSmoothCurve(new (float, float)[] { (0f, 0f), (0.12f, -0.06f), (0.18f, 0.12f), (0.45f, 0f) }));
            clip.SetCurve("Bone_Root/Hips", typeof(Transform), "localEulerAnglesRaw.z", 
                CreateSmoothCurve(new (float, float)[] { (0f, 0f), (0.12f, -12f), (0.18f, 20f), (0.45f, 0f) }));

            // Tay phải giương gậy cao qua đầu rồi bổ xuống
            clip.SetCurve("Bone_Root/Hips/Chest/Arm_R", typeof(Transform), "localEulerAnglesRaw.z", 
                CreateSmoothCurve(new (float, float)[] { (0f, 0f), (0.12f, 75f), (0.18f, -45f), (0.28f, -30f), (0.45f, 0f) }));

            // Gậy trúc quét xoay
            clip.SetCurve("Bone_Root/Hips/Chest/Arm_R/WeaponSocket", typeof(Transform), "localEulerAnglesRaw.z", 
                CreateSmoothCurve(new (float, float)[] { (0f, 0f), (0.12f, 40f), (0.18f, -50f), (0.45f, 0f) }));

            // Đầu ngửa nhẹ rồi chúi nhìn theo đòn đánh
            clip.SetCurve("Bone_Root/Hips/Chest/Head", typeof(Transform), "localEulerAnglesRaw.z", 
                CreateSmoothCurve(new (float, float)[] { (0f, 0f), (0.12f, -8f), (0.18f, 15f), (0.45f, 0f) }));

            // Tay trái giương ra sau giữ thăng bằng
            clip.SetCurve("Bone_Root/Hips/Chest/Arm_L", typeof(Transform), "localEulerAnglesRaw.z", 
                CreateSmoothCurve(new (float, float)[] { (0f, 0f), (0.12f, -35f), (0.18f, 30f), (0.45f, 0f) }));

            SaveOrReplaceClip(clip, $"{AnimationDir}/Attack_Rig.anim");
        }
        #endregion

        #region 4. Dash Clip (0.3s, Fast Recovery)
        private static void CreateDashClip()
        {
            var clip = new AnimationClip { name = "Dash_Rig", frameRate = 60 };
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = false;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            // Thân dốc nghiêng lướt gió
            clip.SetCurve("Bone_Root/Hips", typeof(Transform), "localEulerAnglesRaw.z", 
                CreateSmoothCurve(new (float, float)[] { (0f, 0f), (0.08f, 25f), (0.22f, 20f), (0.3f, 0f) }));
            clip.SetCurve("Bone_Root/Hips", typeof(Transform), "localPosition.y", 
                CreateSmoothCurve(new (float, float)[] { (0f, 0.4f), (0.1f, 0.33f), (0.3f, 0.4f) }));

            // Tay cầm gậy chĩa xuôi theo gió
            clip.SetCurve("Bone_Root/Hips/Chest/Arm_R", typeof(Transform), "localEulerAnglesRaw.z", 
                CreateSmoothCurve(new (float, float)[] { (0f, 0f), (0.1f, -40f), (0.3f, 0f) }));
            clip.SetCurve("Bone_Root/Hips/Chest/Arm_L", typeof(Transform), "localEulerAnglesRaw.z", 
                CreateSmoothCurve(new (float, float)[] { (0f, 0f), (0.1f, -45f), (0.3f, 0f) }));

            // Nón lá bay ngược ra sau
            clip.SetCurve("Bone_Root/Hat", typeof(Transform), "localEulerAnglesRaw.z", 
                CreateSmoothCurve(new (float, float)[] { (0f, 0f), (0.1f, -25f), (0.3f, 0f) }));

            SaveOrReplaceClip(clip, $"{AnimationDir}/Dash_Rig.anim");
        }
        #endregion

        #region 5. Dead Clip (0.8s, Dramatic Fall)
        private static void CreateDeadClip()
        {
            var clip = new AnimationClip { name = "Dead_Rig", frameRate = 60 };
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = false;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            // Thân ngửa bật ra sau rồi đổ bệt xuống đất
            clip.SetCurve("Bone_Root/Hips", typeof(Transform), "localEulerAnglesRaw.z", 
                CreateSmoothCurve(new (float, float)[] { (0f, 0f), (0.2f, -30f), (0.5f, -85f), (0.8f, -90f) }));
            clip.SetCurve("Bone_Root/Hips", typeof(Transform), "localPosition.y", 
                CreateSmoothCurve(new (float, float)[] { (0f, 0.4f), (0.2f, 0.5f), (0.5f, 0.15f), (0.8f, 0.1f) }));

            // Buông rơi gậy trúc
            clip.SetCurve("Bone_Root/Hips/Chest/Arm_R", typeof(Transform), "localEulerAnglesRaw.z", 
                CreateSmoothCurve(new (float, float)[] { (0f, 0f), (0.2f, 60f), (0.5f, 110f), (0.8f, 120f) }));

            // Đầu gục
            clip.SetCurve("Bone_Root/Hips/Chest/Head", typeof(Transform), "localEulerAnglesRaw.z", 
                CreateSmoothCurve(new (float, float)[] { (0f, 0f), (0.2f, -20f), (0.8f, -40f) }));

            // Nón lá tuột rơi
            clip.SetCurve("Bone_Root/Hat", typeof(Transform), "localPosition.x", 
                CreateSmoothCurve(new (float, float)[] { (0f, 0f), (0.4f, -0.3f), (0.8f, -0.4f) }));
            clip.SetCurve("Bone_Root/Hat", typeof(Transform), "localPosition.y", 
                CreateSmoothCurve(new (float, float)[] { (0f, 0.35f), (0.4f, 0.2f), (0.8f, 0.05f) }));

            SaveOrReplaceClip(clip, $"{AnimationDir}/Dead_Rig.anim");
        }
        #endregion

        #region Utilities
        private static AnimationCurve CreateSmoothCurve((float time, float val)[] keyframes)
        {
            var keys = new Keyframe[keyframes.Length];
            for (int i = 0; i < keyframes.Length; i++)
            {
                keys[i] = new Keyframe(keyframes[i].time, keyframes[i].val);
            }

            var curve = new AnimationCurve(keys);
            for (int i = 0; i < keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            }
            return curve;
        }

        private static void SaveOrReplaceClip(AnimationClip clip, string path)
        {
            var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(clip, existing);
            }
            else
            {
                AssetDatabase.CreateAsset(clip, path);
            }
        }
        #endregion
    }
}
