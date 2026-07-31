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

            // 1. Thư Sinh
            string thuSinhPath = $"{folderPath}/ThuSinhSignatureSkill.asset";
            if (AssetDatabase.LoadAssetAtPath<ThuSinhSkillData>(thuSinhPath) == null)
            {
                var thuSinh = ScriptableObject.CreateInstance<ThuSinhSkillData>();
                AssetDatabase.CreateAsset(thuSinh, thuSinhPath);
                Debug.Log($"[SignatureSkillDataGenerator] Đã tạo: {thuSinhPath}");
            }

            // 2. Đạo Sĩ
            string daoSiPath = $"{folderPath}/DaoSiSignatureSkill.asset";
            if (AssetDatabase.LoadAssetAtPath<DaoSiSkillData>(daoSiPath) == null)
            {
                var daoSi = ScriptableObject.CreateInstance<DaoSiSkillData>();
                AssetDatabase.CreateAsset(daoSi, daoSiPath);
                Debug.Log($"[SignatureSkillDataGenerator] Đã tạo: {daoSiPath}");
            }

            // 3. Võ Tăng
            string voTangPath = $"{folderPath}/VoTangSignatureSkill.asset";
            if (AssetDatabase.LoadAssetAtPath<VoTangSkillData>(voTangPath) == null)
            {
                var voTang = ScriptableObject.CreateInstance<VoTangSkillData>();
                AssetDatabase.CreateAsset(voTang, voTangPath);
                Debug.Log($"[SignatureSkillDataGenerator] Đã tạo: {voTangPath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Signature Skill Generator", $"Đã khởi tạo thành công 3 ScriptableObject mẫu trong thư mục {folderPath}", "OK");
        }
    }
}
#endif
