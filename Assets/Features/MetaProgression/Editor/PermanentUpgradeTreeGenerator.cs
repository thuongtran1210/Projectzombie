using UnityEngine;
using UnityEditor;
using System.IO;
using ProjectZombie.Features.MetaProgression;
using ProjectZombie.Features.Upgrades;

namespace ProjectZombie.Editor.MetaProgression
{
    public static class PermanentUpgradeTreeGenerator
    {
        [MenuItem("ProjectZombie/Meta/Generate Permanent Upgrade Tree Data")]
        public static PermanentUpgradeTreeData GenerateTreeData()
        {
            string folderPath = "Assets/_Data/Meta";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string assetPath = $"{folderPath}/PermanentUpgradeTree.asset";
            var tree = AssetDatabase.LoadAssetAtPath<PermanentUpgradeTreeData>(assetPath);
            if (tree == null)
            {
                tree = ScriptableObject.CreateInstance<PermanentUpgradeTreeData>();
                AssetDatabase.CreateAsset(tree, assetPath);
            }

            Sprite iconDmg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/UpgradeIcons/Icon_P001_Damage.png");
            Sprite iconCrit = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/UpgradeIcons/Icon_W012_PhiTieuBatQuai.png");
            Sprite iconAtkSpd = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/UpgradeIcons/Icon_W001_NoThan.png");

            Sprite iconHp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/UpgradeIcons/Icon_P003_Health.png");
            Sprite iconDash = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Skills/Btn_Dash_PhiVan.png");
            Sprite iconSpeed = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/UpgradeIcons/Icon_P006_Speed.png");

            Sprite iconMagnet = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/UpgradeIcons/Icon_P010_Magnet.png");
            Sprite iconExp = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/UpgradeIcons/Icon_W005_TrongDong.png");
            Sprite iconLuck = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/UpgradeIcons/Icon_P012_Luck.png");

            tree.nodes = new PermanentUpgradeNode[]
            {
                // ==================== NHÁNH 1: TẢN VIÊN SƠN THÁNH (CÔNG) ====================
                new PermanentUpgradeNode
                {
                    branch = SanctuaryBranch.TanVienSonThanh,
                    nodeId = "atk_power",
                    displayName = "Ngoại Công Bất Diệt",
                    description = "Gia tăng thần lực bản mệnh, tăng thêm +5% sát thương cơ bản cho mọi chiêu thức.",
                    icon = iconDmg,
                    maxLevel = 10,
                    costPerLevel = new int[] { 100, 200, 350, 550, 800, 1100, 1500, 2000, 2600, 3500 },
                    statBonusPerLevel = new PlayerStatModifier { baseDamageBonus = 0.5f }
                },
                new PermanentUpgradeNode
                {
                    branch = SanctuaryBranch.TanVienSonThanh,
                    nodeId = "atk_crit",
                    displayName = "Khai Sơn Lực",
                    description = "Khai mở nhãn lực, tăng +3% tỉ lệ đánh chí mạng (Crit Chance).",
                    icon = iconCrit,
                    maxLevel = 5,
                    costPerLevel = new int[] { 250, 500, 900, 1500, 2500 },
                    statBonusPerLevel = new PlayerStatModifier { critChanceBonus = 0.03f }
                },
                new PermanentUpgradeNode
                {
                    branch = SanctuaryBranch.TanVienSonThanh,
                    nodeId = "atk_speed",
                    displayName = "Phong Lôi Trảm",
                    description = "Động tác xuất chiêu sắc bén, tăng +4% tốc độ đánh cho nhân vật.",
                    icon = iconAtkSpd,
                    maxLevel = 5,
                    costPerLevel = new int[] { 200, 400, 750, 1200, 2000 },
                    statBonusPerLevel = new PlayerStatModifier { attackSpeedBonus = 0.04f }
                },

                // ==================== NHÁNH 2: PHÙ ĐỔNG THIÊN VƯƠNG (THỦ) ====================
                new PermanentUpgradeNode
                {
                    branch = SanctuaryBranch.PhuDongThienVuong,
                    nodeId = "def_hp",
                    displayName = "Kim Cang Thần Thể",
                    description = "Hấp thụ linh khí trời đất, gia tăng +15 Máu Tối Đa vĩnh viễn.",
                    icon = iconHp,
                    maxLevel = 10,
                    costPerLevel = new int[] { 100, 200, 350, 550, 800, 1100, 1500, 2000, 2600, 3500 },
                    statBonusPerLevel = new PlayerStatModifier { maxHealthBonus = 15f }
                },
                new PermanentUpgradeNode
                {
                    branch = SanctuaryBranch.PhuDongThienVuong,
                    nodeId = "def_dash",
                    displayName = "Tật Phong Thiết Mã",
                    description = "Lướt gió đạp mây, giảm 0.1s thời gian hồi chiêu Lướt (Dash).",
                    icon = iconDash,
                    maxLevel = 5,
                    costPerLevel = new int[] { 150, 300, 600, 1000, 1800 },
                    statBonusPerLevel = new PlayerStatModifier { dashCooldownReduction = 0.1f }
                },
                new PermanentUpgradeNode
                {
                    branch = SanctuaryBranch.PhuDongThienVuong,
                    nodeId = "def_speed",
                    displayName = "Thần Hành Bộ Pháp",
                    description = "Thân thủ nhẹ như cánh hồng, tăng +0.3m/s tốc độ di chuyển.",
                    icon = iconSpeed,
                    maxLevel = 5,
                    costPerLevel = new int[] { 200, 400, 750, 1200, 2000 },
                    statBonusPerLevel = new PlayerStatModifier { moveSpeedBonus = 0.3f }
                },

                // ==================== NHÁNH 3: LIỄU HẠNH & CHỬ ĐỒNG TỬ (BỔ TRỢ) ====================
                new PermanentUpgradeNode
                {
                    branch = SanctuaryBranch.LieuHanhChuDongTu,
                    nodeId = "util_magnet",
                    displayName = "Càn Khôn Nạp Tài",
                    description = "Tạo trường lực từ trường, tăng +0.5m bán kính hút Cổ Tiền và Ngọc EXP.",
                    icon = iconMagnet,
                    maxLevel = 10,
                    costPerLevel = new int[] { 100, 150, 250, 400, 600, 850, 1200, 1600, 2200, 3000 },
                    statBonusPerLevel = new PlayerStatModifier { pickupRangeBonus = 0.5f }
                },
                new PermanentUpgradeNode
                {
                    branch = SanctuaryBranch.LieuHanhChuDongTu,
                    nodeId = "util_exp",
                    displayName = "Khai Tâm Điểm Đạo",
                    description = "Ngộ tính xuất chúng, tăng +5% tốc độ tích lũy kinh nghiệm trong trận.",
                    icon = iconExp,
                    maxLevel = 5,
                    costPerLevel = new int[] { 200, 400, 800, 1400, 2200 },
                    statBonusPerLevel = new PlayerStatModifier { expMultiplierBonus = 0.05f }
                },
                new PermanentUpgradeNode
                {
                    branch = SanctuaryBranch.LieuHanhChuDongTu,
                    nodeId = "util_reroll",
                    displayName = "Thiên Cơ Trùng Toán",
                    description = "Thần cơ diệu toán, tăng thêm may mắn và tỉ lệ nhận ngọc quý sau mỗi run.",
                    icon = iconLuck,
                    maxLevel = 3,
                    costPerLevel = new int[] { 500, 1200, 2500 },
                    statBonusPerLevel = new PlayerStatModifier { critChanceBonus = 0.02f, expMultiplierBonus = 0.05f }
                }
            };

            EditorUtility.SetDirty(tree);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"<color=#00FF88>[PermanentUpgradeTreeGenerator]</color> Đã tạo thành công Cây Nâng Cấp Vĩnh Viễn 3 Nhánh tại: {assetPath}");
            return tree;
        }
    }
}
