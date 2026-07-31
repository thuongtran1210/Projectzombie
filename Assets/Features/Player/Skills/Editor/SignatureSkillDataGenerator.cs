#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ProjectZombie.Features.Player.Skills.Editor
{
    /// <summary>
    /// Editor Tool giúp tự động khởi tạo 3 ScriptableObject mẫu cho Thư Sinh, Đạo Sĩ và Võ Tăng.
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

            // 1. Thư Sinh - Văn Khí Bảo Hộ (Hồi chiêu: 25s)
            string thuSinhPath = $"{folderPath}/ThuSinhSignatureSkill.asset";
            var thuSinh = AssetDatabase.LoadAssetAtPath<ThuSinhSkillData>(thuSinhPath);
            if (thuSinh == null)
            {
                thuSinh = ScriptableObject.CreateInstance<ThuSinhSkillData>();
                AssetDatabase.CreateAsset(thuSinh, thuSinhPath);
            }
            SerializedObject soThuSinh = new SerializedObject(thuSinh);
            soThuSinh.FindProperty("_skillName").stringValue = "Văn Khí Bảo Hộ";
            soThuSinh.FindProperty("_description").stringValue = "Mở giao diện chọn 1 trong 5 thuộc tính Ngũ Hành (trong 1.5s). Hit tiếp theo khớp vòng Tương Sinh sẽ lập tức giảm 20% Hồi Chiêu cho vũ khí đó (Cooldown: 25s).";
            soThuSinh.FindProperty("_baseCooldown").floatValue = 25f;
            soThuSinh.ApplyModifiedProperties();
            Debug.Log($"[SignatureSkillDataGenerator] Đã cập nhật/tạo: {thuSinhPath}");

            // 2. Đạo Sĩ - Bát Quái Trận Đồ (Hồi chiêu: 30s)
            string daoSiPath = $"{folderPath}/DaoSiSignatureSkill.asset";
            var daoSi = AssetDatabase.LoadAssetAtPath<DaoSiSkillData>(daoSiPath);
            if (daoSi == null)
            {
                daoSi = ScriptableObject.CreateInstance<DaoSiSkillData>();
                AssetDatabase.CreateAsset(daoSi, daoSiPath);
            }
            SerializedObject soDaoSi = new SerializedObject(daoSi);
            soDaoSi.FindProperty("_skillName").stringValue = "Bát Quái Trận Đồ";
            soDaoSi.FindProperty("_description").stringValue = "Tạo vùng Bát Quái bán kính 4.5m trong 4s. Nhốt quái thường đi vòng quanh viền trận và ép Âm Dương về mức cân bằng 50 để mở cửa sổ chọn thẻ Thái Cực (Cooldown: 30s).";
            soDaoSi.FindProperty("_baseCooldown").floatValue = 30f;
            soDaoSi.ApplyModifiedProperties();
            Debug.Log($"[SignatureSkillDataGenerator] Đã cập nhật/tạo: {daoSiPath}");

            // 3. Võ Tăng - Phá Giới Chấn Thế (Hồi chiêu: 20s)
            string voTangPath = $"{folderPath}/VoTangSignatureSkill.asset";
            var voTang = AssetDatabase.LoadAssetAtPath<VoTangSkillData>(voTangPath);
            if (voTang == null)
            {
                voTang = ScriptableObject.CreateInstance<VoTangSkillData>();
                AssetDatabase.CreateAsset(voTang, voTangPath);
            }
            SerializedObject soVoTang = new SerializedObject(voTang);
            soVoTang.FindProperty("_skillName").stringValue = "Phá Giới Chấn Thế";
            soVoTang.FindProperty("_description").stringValue = "Hy sinh 30% HP hiện tại tạo sóng chấn động gây sát thương khủng, đẩy lùi (8m/s) và làm choáng 1.2s quái xung quanh. Tăng trực tiếp +25 Âm Dương về cực Dương (Cooldown: 20s). Yêu cầu HP > 15%.";
            soVoTang.FindProperty("_baseCooldown").floatValue = 20f;
            soVoTang.ApplyModifiedProperties();
            Debug.Log($"[SignatureSkillDataGenerator] Đã cập nhật/tạo: {voTangPath}");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Signature Skill Generator", $"Đã khởi tạo thành công 3 ScriptableObject mẫu trong thư mục {folderPath}", "OK");
        }
    }
}
#endif
