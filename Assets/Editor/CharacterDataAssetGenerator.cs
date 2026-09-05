#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Editor
{
    /// <summary>
    /// Generator tự động tạo các file ScriptableObject CharacterDataSO riêng lẻ
    /// và tổng hợp vào CharacterDatabase.asset chuẩn hóa mô hình Kéo - Thả (Drag & Drop).
    /// </summary>
    public static class CharacterDataAssetGenerator
    {
        [MenuItem("Tools/ProjectZombie/Characters/Generate Individual Character Assets", priority = 20)]
        public static void GenerateAllCharacterAssets()
        {
            GenerateCharacterAssets();
        }

        public static void GenerateCharacterAssets()
        {
            string charactersFolder = "Assets/_Data/Characters";
            if (!AssetDatabase.IsValidFolder("Assets/_Data"))
            {
                AssetDatabase.CreateFolder("Assets", "_Data");
            }
            if (!AssetDatabase.IsValidFolder(charactersFolder))
            {
                AssetDatabase.CreateFolder("Assets/_Data", "Characters");
            }

            // 1. Nạp các Prefab, VFX, Icon từ Project
            var pThuSinh = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Characters/Players/Thu Sinh.prefab");
            var pDaoSi = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Characters/Players/Dao Si.prefab");
            var pThanhDong = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Characters/Players/Thanh Dong.prefab");
            var pAnSi = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Characters/Players/An Si.prefab");

            var vfxThuSinh = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/SkillLibrary/Prefabs/VFX_ThuSinh_InkSlash.prefab");
            var vfxDaoSi = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/SkillLibrary/Prefabs/VFX_DaoSi_SwordSlash.prefab");
            var vfxThanhDong = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/SkillLibrary/Prefabs/Projectile_ThanhDong_AirWave.prefab");
            var vfxAnSi = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/VFX/SkillLibrary/Prefabs/VFX_AnSi_EarthImpactSlash.prefab");

            var iconAtkThuSinh = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Skills/Icon_Atk_ThuSinh_Brush.png");
            var iconAtkDaoSi = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Skills/Icon_Atk_DaoSi_Sword.png");
            var iconAtkThanhDong = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Skills/Icon_Atk_ThanhDong_Torch.png");
            var iconAtkAnSi = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Skills/Icon_Atk_AnSi_Fist.png");

            var relicThuSinh = AssetDatabase.LoadAssetAtPath<ProjectZombie.Features.Weapons.WeaponData>("Assets/_Data/Weapons/Relic_ButPhanQuan.asset");
            if (relicThuSinh == null) relicThuSinh = AssetDatabase.LoadAssetAtPath<ProjectZombie.Features.Weapons.WeaponData>("Assets/_Data/Weapons/W002_ButPhanQuan.asset");

            // 2. Tạo hoặc cập nhật 4 file CharacterDataSO độc lập
            var listSO = new List<CharacterDataSO>();

            // --- TƯỚNG 1: THƯ SINH ---
            var soThuSinh = GetOrCreateSO<CharacterDataSO>($"{charactersFolder}/Hero_ThuSinh.asset");
            soThuSinh.characterId = "C001_ThuSinh";
            soThuSinh.characterName = "Thư Sinh";
            soThuSinh.element = ElementType.Kim;
            soThuSinh.elementHexColor = "#FFD700";
            soThuSinh.description = "Được anh linh liệt tổ & Đức Thánh Trần điểm hóa. Tay cầm bút lệnh khí thiêng sông núi phán định tà ma.";
            soThuSinh.baseMaxHealth = 100f;
            soThuSinh.baseMoveSpeed = 5.2f;
            soThuSinh.baseDamage = 12f;
            soThuSinh.baseCritChance = 0.08f;
            soThuSinh.baseDashCooldown = 1.8f;
            soThuSinh.uiAtkRatio = 0.85f;
            soThuSinh.uiSpdRatio = 0.75f;
            soThuSinh.uiDefRatio = 0.60f;
            soThuSinh.signatureSkillName = "Phán Quyết Tiền Định";
            soThuSinh.signatureSkillDesc = "Chèn 1 hit ảo Ngũ Hành vào Queue Tương Sinh, kích hoạt giảm 20% Cooldown cho vũ khí khớp lệnh.";
            soThuSinh.passiveTraitName = "Văn Khí Hộ Thể";
            soThuSinh.passiveTraitDesc = "Khi kích hoạt Tương Sinh Ngũ Hành, tăng 15% Tốc độ di chuyển và hồi 5% HP tối đa.";
            soThuSinh.playerPrefab = pThuSinh;
            soThuSinh.basicAttackConfig = new CharacterAttackConfig
            {
                attackType = CharacterAttackType.MeleeSlash,
                attackIcon = iconAtkThuSinh,
                slashVfxPrefab = vfxThuSinh,
                attackName = "Vung Bút Phán Quan",
                meleeAreaSize = new Vector2(3.5f, 2.6f),
                meleeOffset = 1.3f,
                baseAttackSpeed = 1.8f
            };
            soThuSinh.defaultRelic = relicThuSinh;
            EditorUtility.SetDirty(soThuSinh);
            listSO.Add(soThuSinh);

            // --- TƯỚNG 2: ĐẠO SĨ ---
            var soDaoSi = GetOrCreateSO<CharacterDataSO>($"{charactersFolder}/Hero_DaoSi.asset");
            soDaoSi.characterId = "C002_DaoSi";
            soDaoSi.characterName = "Đạo Sĩ";
            soDaoSi.element = ElementType.Moc;
            soDaoSi.elementHexColor = "#9B51E0";
            soDaoSi.description = "Đạo nhân tinh thông Tiên Đạo Bát Quái. Vận hành Cán Cân Âm Dương (Âm Thịnh / Dương Thịnh / Thái Cực).";
            soDaoSi.baseMaxHealth = 110f;
            soDaoSi.baseMoveSpeed = 4.8f;
            soDaoSi.baseDamage = 11f;
            soDaoSi.baseCritChance = 0.05f;
            soDaoSi.baseDashCooldown = 2.0f;
            soDaoSi.uiAtkRatio = 0.80f;
            soDaoSi.uiSpdRatio = 0.65f;
            soDaoSi.uiDefRatio = 0.80f;
            soDaoSi.signatureSkillName = "Bát Quái Trận Đồ";
            soDaoSi.signatureSkillDesc = "Dậm chân tạo vùng Bát Quái làm chậm và gây sát thương yêu ma, ép Cán Cân Âm Dương về 50 (Thái Cực) trong 4s.";
            soDaoSi.passiveTraitName = "Cán Cân Âm Dương";
            soDaoSi.passiveTraitDesc = "Trạng thái Thái Cực (Cân bằng) tăng 25% Sát thương toàn thể và giảm 20% Sát thương nhận vào.";
            soDaoSi.playerPrefab = pDaoSi;
            soDaoSi.basicAttackConfig = new CharacterAttackConfig
            {
                attackType = CharacterAttackType.MeleeSlash,
                attackIcon = iconAtkDaoSi,
                slashVfxPrefab = vfxDaoSi,
                attackName = "Trảm Yêu Trừ Ma Kiếm",
                meleeAreaSize = new Vector2(3.6f, 2.5f),
                meleeOffset = 1.35f,
                baseAttackSpeed = 2.0f
            };
            EditorUtility.SetDirty(soDaoSi);
            listSO.Add(soDaoSi);

            // --- TƯỚNG 3: THANH ĐỒNG ---
            var soThanhDong = GetOrCreateSO<CharacterDataSO>($"{charactersFolder}/Hero_ThanhDong.asset");
            soThanhDong.characterId = "C003_ThanhDong";
            soThanhDong.characterName = "Thanh Đồng";
            soThanhDong.element = ElementType.Moc;
            soThanhDong.elementHexColor = "#4C7A3D";
            soThanhDong.description = "Cô Đồng / Thầy Pháp Đạo Mẫu Tứ Phủ (Thiên, Nhạc, Thoải, Địa). Tay mang Chuỗi Linh Phù Tứ Phủ hộ thân trừ tà.";
            soThanhDong.baseMaxHealth = 95f;
            soThanhDong.baseMoveSpeed = 5.6f;
            soThanhDong.baseDamage = 10f;
            soThanhDong.baseCritChance = 0.10f;
            soThanhDong.baseDashCooldown = 1.6f;
            soThanhDong.uiAtkRatio = 0.78f;
            soThanhDong.uiSpdRatio = 0.90f;
            soThanhDong.uiDefRatio = 0.65f;
            soThanhDong.signatureSkillName = "Giá Đồng Tứ Phủ";
            soThanhDong.signatureSkillDesc = "Thỉnh nhập Thánh thần Tứ Phủ ban hào quang 4 cõi (Tăng công / Tăng tốc / Giảm hồi chiêu / Giáp hộ thân) trong 5s.";
            soThanhDong.passiveTraitName = "Linh Lực Tứ Phủ";
            soThanhDong.passiveTraitDesc = "Thu thập Linh Khí tích lũy thanh Linh Lực Tứ Phủ. Khi kích hoạt Giá Đồng, nhận đồng thời hiệu ứng hộ trì của cả 4 cõi thần linh.";
            soThanhDong.playerPrefab = pThanhDong;
            soThanhDong.basicAttackConfig = new CharacterAttackConfig
            {
                attackType = CharacterAttackType.RangedProjectile,
                attackIcon = iconAtkThanhDong,
                projectilePrefab = vfxThanhDong,
                attackName = "Khí Ba Đạo Mẫu",
                baseAttackSpeed = 2.2f,
                projectileSpeed = 9.0f
            };
            EditorUtility.SetDirty(soThanhDong);
            listSO.Add(soThanhDong);

            // --- TƯỚNG 4: ẨN SĨ SƠN LÂM ---
            var soAnSi = GetOrCreateSO<CharacterDataSO>($"{charactersFolder}/Hero_AnSi.asset");
            soAnSi.characterId = "C004_AnSi";
            soAnSi.characterName = "Ẩn Sĩ Sơn Lâm";
            soAnSi.element = ElementType.Tho;
            soAnSi.elementHexColor = "#8A6A3E";
            soAnSi.description = "Kỳ nhân tự tu nội lực chốn thâm sơn, hòa hợp làm một với núi rừng bản địa. Dồn lực bộc phát địa khí.";
            soAnSi.baseMaxHealth = 150f;
            soAnSi.baseMoveSpeed = 4.2f;
            soAnSi.baseDamage = 15f;
            soAnSi.baseCritChance = 0.04f;
            soAnSi.baseDashCooldown = 2.4f;
            soAnSi.uiAtkRatio = 0.92f;
            soAnSi.uiSpdRatio = 0.50f;
            soAnSi.uiDefRatio = 0.95f;
            soAnSi.signatureSkillName = "Thập Phương Chấn Thế";
            soAnSi.signatureSkillDesc = "Trừ 30% HP hiện tại bộc phát địa khí chấn nứt đất đá, gây sát thương + Choáng 1.2s và đẩy lùi 8m/s.";
            soAnSi.passiveTraitName = "Bàn Thạch Chi Khu";
            soAnSi.passiveTraitDesc = "Máu càng thấp thủ càng cao. Khi HP dưới 50%, nhận thêm 30% Kháng sát thương và miễn nhiễm Đẩy lùi.";
            soAnSi.playerPrefab = pAnSi;
            soAnSi.basicAttackConfig = new CharacterAttackConfig
            {
                attackType = CharacterAttackType.MeleeSlash,
                attackIcon = iconAtkAnSi,
                slashVfxPrefab = vfxAnSi,
                attackName = "Thạch Quyền Phá Địa",
                meleeAreaSize = new Vector2(3.6f, 2.7f),
                meleeOffset = 1.35f,
                baseAttackSpeed = 1.6f
            };
            EditorUtility.SetDirty(soAnSi);
            listSO.Add(soAnSi);

            // 3. Tạo hoặc cập nhật file CharacterDatabase.asset tập trung
            string dbPath = "Assets/_Data/CharacterDatabase.asset";
            var database = GetOrCreateSO<CharacterDatabaseSO>(dbPath);
            database.SetCharacters(listSO);
            EditorUtility.SetDirty(database);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=#00FF88>[CharacterDataAssetGenerator]</color> Đã tạo thành công 4 file CharacterDataSO tại '{charactersFolder}' và liên kết vào '{dbPath}'!");
            EditorUtility.DisplayDialog("Character Database Generator", "Đã tạo thành công 4 file ScriptableObject độc lập cho từng tướng trong thư mục 'Assets/_Data/Characters/'!\n\nBạn có thể mở từng file để tinh chỉnh chỉ số hoặc kéo thả thêm tướng mới vào 'Assets/_Data/CharacterDatabase.asset'.", "Đã hiểu!");
        }

        private static T GetOrCreateSO<T>(string assetPath) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, assetPath);
            }
            return asset;
        }
    }
}
#endif
