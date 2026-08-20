using UnityEditor;
using UnityEngine;

namespace Projectzombie.Editor.Rigging
{
    public static class AnSiRigPrefabBuilder
    {
        [MenuItem("Tools/Rigging/Build AnSi Rigged Prefab")]
        public static void BuildPrefab()
        {
            string spriteDir = "Assets/Sprites/Rigging/Characters/AnSi";
            string outPrefabPath = "Assets/_Prefabs/Characters/Players/An Si (Rigged).prefab";
            string controllerPath = "Assets/Art/AnSi/Rigging/AnSi_Rig_Animator.controller";

            // 1. Root GameObject
            GameObject root = new GameObject("An Si (Rigged)");
            root.tag = "Player";
            root.layer = LayerMask.NameToLayer("Characters") != -1 ? LayerMask.NameToLayer("Characters") : 6;

            // Player Components
            var rb = root.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.freezeRotation = true;

            var col = root.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.5f, 0.8f);
            col.offset = new Vector2(0, 0.4f);

            // 2. Visual Root & Animator
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);

            var animator = visual.AddComponent<Animator>();
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
            if (controller != null)
            {
                animator.runtimeAnimatorController = controller;
            }

            // 3. Bone Hierarchy & SpriteRenderers
            GameObject boneRoot = new GameObject("Bone_Root");
            boneRoot.transform.SetParent(visual.transform, false);

            // Hat (Nón lá đeo sau lưng - Sorting 92)
            CreateBone(boneRoot.transform, "Hat", new Vector3(-0.05f, 0.35f, 0), spriteDir + "/char_hat.png", 92);

            // Hips / Torso
            GameObject hips = CreateBone(boneRoot.transform, "Hips", new Vector3(0, 0.4f, 0), spriteDir + "/char_torso.png", 100);

            // Gourd (Hồ lô đeo bên hông phải - Sorting 104)
            CreateBone(hips.transform, "Gourd", new Vector3(0.2f, -0.05f, 0), spriteDir + "/char_gourd.png", 104);

            // Chest
            GameObject chest = CreateBone(hips.transform, "Chest", new Vector3(0, 0.25f, 0), null, 0);

            // Head
            CreateBone(chest.transform, "Head", new Vector3(0, 0.15f, 0), spriteDir + "/char_head.png", 102);

            // Left Arm (Upper -> Lower)
            GameObject armUpperL = CreateBone(chest.transform, "Arm_Upper_L", new Vector3(-0.25f, 0.05f, 0), spriteDir + "/char_arm_upper_l.png", 95);
            CreateBone(armUpperL.transform, "Arm_Lower_L", new Vector3(-0.2f, 0, 0), spriteDir + "/char_arm_lower_l.png", 94);

            // Right Arm (Upper -> Lower -> WeaponSocket & Staff)
            GameObject armUpperR = CreateBone(chest.transform, "Arm_Upper_R", new Vector3(0.25f, 0.05f, 0), spriteDir + "/char_arm_upper_r.png", 105);
            GameObject armLowerR = CreateBone(armUpperR.transform, "Arm_Lower_R", new Vector3(0.2f, 0, 0), spriteDir + "/char_arm_lower_r.png", 106);

            // WeaponSocket with Staff Prop
            GameObject weaponSocket = CreateBone(armLowerR.transform, "WeaponSocket", new Vector3(0.1f, -0.05f, 0), spriteDir + "/char_prop_staff.png", 110);

            // Left Leg (Thigh -> Shin)
            GameObject legThighL = CreateBone(hips.transform, "Leg_Thigh_L", new Vector3(-0.12f, -0.1f, 0), spriteDir + "/char_leg_thigh_l.png", 98);
            CreateBone(legThighL.transform, "Leg_Shin_L", new Vector3(0, -0.2f, 0), spriteDir + "/char_leg_shin_l.png", 97);

            // Right Leg (Thigh -> Shin)
            GameObject legThighR = CreateBone(hips.transform, "Leg_Thigh_R", new Vector3(0.12f, -0.1f, 0), spriteDir + "/char_leg_thigh_r.png", 101);
            CreateBone(legThighR.transform, "Leg_Shin_R", new Vector3(0, -0.2f, 0), spriteDir + "/char_leg_shin_r.png", 102);

            // Sockets
            GameObject firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(boneRoot.transform, false);
            firePoint.transform.localPosition = new Vector3(0.35f, 0.45f, 0);

            GameObject orbitCenter = new GameObject("OrbitCenter");
            orbitCenter.transform.SetParent(boneRoot.transform, false);
            orbitCenter.transform.localPosition = new Vector3(0, 0.4f, 0);

            // 4. Save Prefab
            PrefabUtility.SaveAsPrefabAsset(root, outPrefabPath);
            Object.DestroyImmediate(root);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=#4CAF50><b>[AnSiRigPrefabBuilder]</b> Đã tạo thành công Prefab: {outPrefabPath}</color>");
        }

        private static GameObject CreateBone(Transform parent, string name, Vector3 localPos, string spritePath, int sortingOrder)
        {
            GameObject bone = new GameObject(name);
            bone.transform.SetParent(parent, false);
            bone.transform.localPosition = localPos;

            if (!string.IsNullOrEmpty(spritePath))
            {
                var sr = bone.AddComponent<SpriteRenderer>();
                sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                sr.sortingLayerName = "Characters";
                sr.sortingOrder = sortingOrder;
            }

            return bone;
        }
    }
}
