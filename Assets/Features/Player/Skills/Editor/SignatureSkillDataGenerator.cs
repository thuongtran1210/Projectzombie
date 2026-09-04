#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.Features.Player.Skills.Editor
{
    /// <summary>
    /// Editor Tool giúp tự động khởi tạo và gán VFX Prefab cho ScriptableObject Tuyệt Kỹ của Thư Sinh, Thanh Đồng và Ẩn Sĩ.
    /// Lưu vào thư mục chuẩn của dự án: Assets/_Data/Skills/
    /// Truy cập qua Menu: ProjectZombie > Skills > Generate Sample Signature Skill SOs
    /// </summary>
    public static class SignatureSkillDataGenerator
    {
        [MenuItem("ProjectZombie/Skills/Generate Sample Signature Skill SOs")]
        public static void GenerateSampleSkills()
        {
            string folderPath = "Assets/_Data/Skills";

            if (!AssetDatabase.IsValidFolder("Assets/_Data"))
            {
                AssetDatabase.CreateFolder("Assets", "_Data");
            }
            if (!AssetDatabase.IsValidFolder("Assets/_Data/Skills"))
            {
                AssetDatabase.CreateFolder("Assets/_Data", "Skills");
            }

            // 1. Thư Sinh - Phán Quyết Tiền Định / Phán Quyết Âm Ty (Hồi chiêu: 25s)
            string thuSinhPath = $"{folderPath}/ThuSinhSignatureSkill.asset";
            var thuSinh = AssetDatabase.LoadAssetAtPath<ThuSinhSkillData>(thuSinhPath);
            if (thuSinh == null)
            {
                thuSinh = ScriptableObject.CreateInstance<ThuSinhSkillData>();
                AssetDatabase.CreateAsset(thuSinh, thuSinhPath);
            }
            SerializedObject soThuSinh = new SerializedObject(thuSinh);
            soThuSinh.FindProperty("_skillName").stringValue = "Phán Quyết Tiền Định";
            soThuSinh.FindProperty("_description").stringValue = "Thư Sinh vẽ bút lệnh khí thiêng sông núi điểm hóa từ Đức Thánh Trần. Bung trận địa cổ tự, nổ 250% sát thương diện rộng, giảm 20% CD toàn vũ khí và nhận +30% Tốc đánh, +20% Tốc chạy trong 4s (Cooldown: 25s).";
            soThuSinh.FindProperty("_baseCooldown").floatValue = 25f;

            var groundDecal = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/SkillLibrary/Prefabs/VFX_W002_GroundDecal.prefab");
            var inkSlash = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/SkillLibrary/Prefabs/VFX_ThuSinh_InkSlash.prefab");
            var lightning = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/SkillLibrary/Prefabs/VFX_W009_LightningChain.prefab");

            if (groundDecal != null) soThuSinh.FindProperty("_groundDecalPrefab").objectReferenceValue = groundDecal;
            if (inkSlash != null) soThuSinh.FindProperty("_inkSlashPrefab").objectReferenceValue = inkSlash;
            if (lightning != null) soThuSinh.FindProperty("_lightningPrefab").objectReferenceValue = lightning;

            soThuSinh.ApplyModifiedProperties();
            EditorUtility.SetDirty(thuSinh);
            Debug.Log($"[SignatureSkillDataGenerator] Đã cập nhật/tạo: {thuSinhPath}");

            // 2. Thanh Đồng - Giá Đồng Tứ Phủ (Hầu Đồng Tứ Phủ - Hồi chiêu: 30s)
            string thanhDongPath = $"{folderPath}/ThanhDongSignatureSkill.asset";
            var thanhDong = AssetDatabase.LoadAssetAtPath<ThanhDongSkillData>(thanhDongPath);
            if (thanhDong == null)
            {
                thanhDong = ScriptableObject.CreateInstance<ThanhDongSkillData>();
                AssetDatabase.CreateAsset(thanhDong, thanhDongPath);
            }
            SerializedObject soThanhDong = new SerializedObject(thanhDong);
            soThanhDong.FindProperty("_skillName").stringValue = "Giá Đồng Tứ Phủ";
            soThanhDong.FindProperty("_description").stringValue = "Thanh Đồng thỉnh nhập Thánh thần Tứ Phủ (Thiên, Nhạc, Thoải, Địa Phủ). Sóng xung kích gây 180% sát thương, làm Choáng 2.5s quái xung quanh, hút sạch ExpGem và nhận +35% DMG, +35% Tốc chạy trong 5s (Cooldown: 30s).";
            soThanhDong.FindProperty("_baseCooldown").floatValue = 30f;

            var auraVfx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/SkillLibrary/Prefabs/VFX_ThanhDong_TuPhuPossessionAura.prefab");
            var shockwaveVfx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/SkillLibrary/Prefabs/VFX_ThanhDong_OracleShockwave.prefab");

            if (auraVfx != null) soThanhDong.FindProperty("_tuPhuAuraPrefab").objectReferenceValue = auraVfx;
            if (shockwaveVfx != null) soThanhDong.FindProperty("_shockwavePrefab").objectReferenceValue = shockwaveVfx;

            soThanhDong.ApplyModifiedProperties();
            EditorUtility.SetDirty(thanhDong);
            Debug.Log($"[SignatureSkillDataGenerator] Đã cập nhật/tạo: {thanhDongPath}");

            // 3. Ẩn Sĩ Sơn Lâm - Thập Phương Chấn Thế (Hồi chiêu: 20s)
            string voTangPath = $"{folderPath}/VoTangSignatureSkill.asset";
            var voTang = AssetDatabase.LoadAssetAtPath<VoTangSkillData>(voTangPath);
            if (voTang == null)
            {
                voTang = ScriptableObject.CreateInstance<VoTangSkillData>();
                AssetDatabase.CreateAsset(voTang, voTangPath);
            }
            SerializedObject soVoTang = new SerializedObject(voTang);
            soVoTang.FindProperty("_skillName").stringValue = "Thập Phương Chấn Thế";
            soVoTang.FindProperty("_description").stringValue = "Ẩn Sĩ Sơn Lâm dậm chân giải phóng địa khí núi ngàn nứt vỡ đất đá, gây 320% sát thương, hất văng (10m/s) và làm choáng 2.0s quái xung quanh. Nhận Hóa Thân Bàn Thạch (+15% HP, +30% DMG) và tăng +25 điểm về Cực Dương (Cooldown: 20s).";
            soVoTang.FindProperty("_baseCooldown").floatValue = 20f;

            var earthShockwave = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/SkillLibrary/Prefabs/VFX_W005_DongSonShockwave.prefab");
            var earthImpact = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/SkillLibrary/Prefabs/VFX_AnSi_EarthImpactSlash.prefab");

            if (earthShockwave != null) soVoTang.FindProperty("_shockwavePrefab").objectReferenceValue = earthShockwave;
            if (earthImpact != null) soVoTang.FindProperty("_earthImpactPrefab").objectReferenceValue = earthImpact;

            soVoTang.ApplyModifiedProperties();
            EditorUtility.SetDirty(voTang);
            Debug.Log($"[SignatureSkillDataGenerator] Đã cập nhật/tạo: {voTangPath}");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SignatureSkillDataGenerator] Hoàn tất cập nhật 3 ScriptableObject Tuyệt Kỹ!");
        }
    }
}
#endif
