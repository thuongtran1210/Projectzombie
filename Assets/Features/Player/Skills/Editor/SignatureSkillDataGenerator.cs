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

            // 1. Thư Sinh - Phán Quyết Tiền Định (Hồi chiêu: 25s)
            string thuSinhPath = $"{folderPath}/ThuSinhSignatureSkill.asset";
            var thuSinh = AssetDatabase.LoadAssetAtPath<ThuSinhSkillData>(thuSinhPath);
            if (thuSinh == null)
            {
                thuSinh = ScriptableObject.CreateInstance<ThuSinhSkillData>();
                AssetDatabase.CreateAsset(thuSinh, thuSinhPath);
            }
            SerializedObject soThuSinh = new SerializedObject(thuSinh);
            soThuSinh.FindProperty("_skillName").stringValue = "Phán Quyết Tiền Định";
            soThuSinh.FindProperty("_description").stringValue = "Thư Sinh vẽ bút lệnh khí thiêng sông núi điểm hóa từ Đức Thánh Trần. Mở giao diện chọn 1 thuộc tính Ngũ Hành (trong 1.5s). Hit tiếp theo khớp Tương Sinh sẽ kích hoạt ngay 20% giảm hồi chiêu (Cooldown: 25s).";
            soThuSinh.FindProperty("_baseCooldown").floatValue = 25f;
            soThuSinh.ApplyModifiedProperties();
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
            soThanhDong.FindProperty("_description").stringValue = "Thanh Đồng thỉnh nhập Thánh thần Tứ Phủ (Thiên, Nhạc, Thoải, Địa Phủ), nhận hào quang & buff sắc phục 4 cõi trong 5s (Cooldown: 30s).";
            soThanhDong.FindProperty("_baseCooldown").floatValue = 30f;
            soThanhDong.ApplyModifiedProperties();
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
            soVoTang.FindProperty("_description").stringValue = "Ẩn Sĩ Sơn Lâm dậm chân giải phóng địa khí núi ngàn, hy sinh 30% HP hiện tại nứt vỡ đất đá, đẩy lùi (8m/s) và làm choáng 1.2s quái xung quanh. Tăng +25 điểm về Cực Dương (Cooldown: 20s). Yêu cầu HP > 15%.";
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
