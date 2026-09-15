#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Upgrades.Editor.Studio;

namespace ProjectZombie.Features.Upgrades.Editor
{
    /// <summary>
    /// Builder chuyên trách tự động tạo và chuẩn hóa toàn bộ ScriptableObject Data cho Hệ thống Tiến trình Mới:
    /// 1. Kho Lõi Đột Biến (Mutation Augments: Bạc / Vàng / Kim Cương xuất hiện tại Lv.5, 15, 30).
    /// 2. Kho Thẻ Chỉ Số Nền Tảng (Micro-Cards 1 dòng xuất hiện tại Lv.2-4, 6-14, 16-29).
    /// </summary>
    public static class ProgressionUpgradesDatabaseBuilder
    {
        private const string AUGMENTS_DIR = "Assets/_Data/Upgrades/MutationAugments";
        private const string MICRO_STATS_DIR = "Assets/_Data/Upgrades/MicroStats";

        [MenuItem("ProjectZombie/Upgrades/⚡ Sinh Toàn Bộ Data Lõi Đột Biến & Micro-Cards (Logarithmic Progression)")]
        public static void GenerateAllProgressionData()
        {
            EnsureDirectory(AUGMENTS_DIR);
            EnsureDirectory(MICRO_STATS_DIR);

            GeneratePrismaticAugments();
            GenerateGoldAugments();
            GenerateSilverAugments();
            GenerateGenericMicroStats();
            GenerateArchetypeMicroStats();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Đồng bộ sang Resources
            int synced = UpgradeStudioAuditEngine.SyncDataToResources();

            Debug.Log($"<color=#00FF88><b>[ProgressionUpgradesDatabaseBuilder]</b></color> Đã tạo thành công toàn bộ Database Lõi Đột Biến & Micro-Cards và đồng bộ {synced} tệp sang Resources/Upgrades!");
            EditorUtility.DisplayDialog("Thành Công", $"Đã khởi tạo xong toàn bộ Database Lõi Đột Biến (Kim Cương / Vàng / Bạc) và Thẻ Chỉ Số Nền Tảng Micro-Cards!\n\nĐã đồng bộ {synced} tệp sang Resources.", "OK");
        }

        #region 1. Lõi Kim Cương (Prismatic - Bẻ Gãy Quy Tắc)
        private static void GeneratePrismaticAugments()
        {
            CreateOrUpdateAugment(
                "AUG_PRIS_PHUDONG_GIGANTISM",
                "Phù Đổng Cự Thần",
                "Mỗi khi tiêu diệt <b>50</b> quái vật, biến thành <b>Khổng Lồ</b> trong <b>6s</b>: Miễn nhiễm sát thương và bước đi dẫm nát toàn bộ kẻ địch xung quanh.",
                AugmentTier.Prismatic,
                10f,
                new PlayerStatModifier { baseDamageBonus = 20f, areaScaleBonus = 0.50f },
                "PhuDong"
            );

            CreateOrUpdateAugment(
                "AUG_PRIS_KIMQUY_MULTICAST",
                "Nỏ Thần Vạn Tiễn Đa Xạ",
                "Tất cả vũ khí bắn thêm <b>+3 luồng đạn phụ</b> tỏa hình nón. Đạn bắn ra có khả năng <b>xuyên thấu toàn bộ</b> mục tiêu.",
                AugmentTier.Prismatic,
                10f,
                new PlayerStatModifier { attackSpeedBonus = 0.25f, baseDamageBonus = 15f },
                "KimQuy"
            );

            CreateOrUpdateAugment(
                "AUG_PRIS_TANVIEN_IMMORTAL",
                "Tản Viên Bất Hoại",
                "Khi nhận sát thương chí tử, <b>Hồi Sinh</b> ngay lập tức với <b>100% Máu</b> và giải phóng một đợt <b>Địa Chấn</b> hất tung toàn bộ sàn đấu (Hồi chiêu: 180s).",
                AugmentTier.Prismatic,
                10f,
                new PlayerStatModifier { maxHealthBonus = 300f },
                "TanVien"
            );

            CreateOrUpdateAugment(
                "AUG_PRIS_THUYBA_FREEZE_BURST",
                "Băng Hà Tuyệt Kỹ",
                "Kẻ địch bị làm chậm hoặc đóng băng khi tử trận sẽ <b>Phát Nổ Băng Giá</b>, gây sát thương lan và <b>đóng băng toàn bộ</b> kẻ địch trong phạm vi 5m.",
                AugmentTier.Prismatic,
                10f,
                new PlayerStatModifier { baseDamageBonus = 15f, areaScaleBonus = 0.35f },
                "ThuyBa"
            );

            CreateOrUpdateAugment(
                "AUG_PRIS_LONGTIEN_YIN_YANG",
                "Âm Dương Lưỡng Cực",
                "Luân chuyển linh hoạt giữa 2 thái cực: <b>Thái Âm</b> (Hút 20% Máu trên mọi đòn đánh) và <b>Thái Dương</b> (Sát thương tăng gấp đôi <b>+100%</b>).",
                AugmentTier.Prismatic,
                10f,
                new PlayerStatModifier { critChanceBonus = 0.20f, baseDamageBonus = 20f },
                "LongTien"
            );

            CreateOrUpdateAugment(
                "AUG_PRIS_OMNIPOTENT_RESET",
                "Thần Hành Tối Thượng",
                "Mỗi lần thực hiện <b>Lướt (Dash)</b> sẽ hồi chiêu tức thì toàn bộ vũ khí và tăng <b>+60% Tốc Đánh</b> trong 2.5 giây tiếp theo.",
                AugmentTier.Prismatic,
                10f,
                new PlayerStatModifier { moveSpeedBonus = 0.20f, dashCooldownReduction = 0.20f },
                "Generic"
            );
        }
        #endregion

        #region 2. Lõi Vàng (Gold - Cường Hóa Giao Tranh)
        private static void GenerateGoldAugments()
        {
            CreateOrUpdateAugment(
                "AUG_GOLD_FIRE_TRAIL",
                "Liệt Hỏa Bộ Pháp",
                "Mỗi khi lướt để lại một <b>Vệt Lửa</b> thiêu đốt tồn tại trong 5s, quái vật đi qua nhận sát thương Hỏa liên tục.",
                AugmentTier.Gold,
                30f,
                new PlayerStatModifier { fireDamageBonus = 0.25f, moveSpeedBonus = 0.10f },
                "PhuDong"
            );

            CreateOrUpdateAugment(
                "AUG_GOLD_CRIT_EXPLODE",
                "Bạo Kích Bộc Phá",
                "Mọi đòn đánh <b>Chí Mạng</b> sẽ kích hoạt vụ nổ năng lượng gây <b>50%</b> sát thương lan ra bán kính xung quanh mục tiêu.",
                AugmentTier.Gold,
                30f,
                new PlayerStatModifier { critChanceBonus = 0.12f },
                "Generic"
            );

            CreateOrUpdateAugment(
                "AUG_GOLD_EXECUTE",
                "Tuyệt Diệt",
                "Kẻ địch thường có lượng máu dưới <b>18%</b> sẽ bị <b>Kết Liễu ngay lập tức</b> khi chịu bất kỳ nguồn sát thương nào.",
                AugmentTier.Gold,
                30f,
                new PlayerStatModifier { baseDamageBonus = 10f },
                "Generic"
            );

            CreateOrUpdateAugment(
                "AUG_GOLD_SHIELD_BASH",
                "Kim Giáp Phản Kích",
                "Nhận vĩnh viễn một <b>Lớp Lá Chắn</b> bằng <b>35% Máu tối đa</b>. Khi lá chắn bị vỡ sẽ phóng thích sóng xung kích đẩy lùi quái vật.",
                AugmentTier.Gold,
                30f,
                new PlayerStatModifier { maxHealthBonus = 150f },
                "TanVien"
            );

            CreateOrUpdateAugment(
                "AUG_GOLD_CHAIN_LIGHTNING",
                "Lôi Đình Liên Hoàn",
                "Đòn đánh có <b>35% tỉ lệ</b> phóng ra tia sét giật truyền qua <b>4 quái vật lân cận</b>, gây 80% sát thương vũ khí.",
                AugmentTier.Gold,
                30f,
                new PlayerStatModifier { attackSpeedBonus = 0.15f },
                "KimQuy"
            );

            CreateOrUpdateAugment(
                "AUG_GOLD_VAMPIRIC_FRENZY",
                "Huyết Thần Cuồng Bạo",
                "Nhận <b>+30% Tốc Độ Đánh</b>. Mỗi quái vật bị tiêu diệt hồi phục lại <b>1.5% Máu tối đa</b>.",
                AugmentTier.Gold,
                30f,
                new PlayerStatModifier { attackSpeedBonus = 0.30f },
                "LongTien"
            );
        }
        #endregion

        #region 3. Lõi Bạc (Silver - Chỉ Số & Tiện Ích)
        private static void GenerateSilverAugments()
        {
            CreateOrUpdateAugment(
                "AUG_SILV_TITAN_HEALTH",
                "Cương Thể Hộ Thân",
                "Tăng vĩnh viễn <b>+250 Máu Tối Đa</b> và hồi phục <b>+4 Máu mỗi giây</b>.",
                AugmentTier.Silver,
                60f,
                new PlayerStatModifier { maxHealthBonus = 250f },
                "TanVien"
            );

            CreateOrUpdateAugment(
                "AUG_SILV_SWIFT_FEET",
                "Thần Tốc Vô Song",
                "Tăng <b>+22% Tốc Độ Di Chuyển</b> và nhận thêm <b>+1 Lần Lướt Trữ Sẵn</b>.",
                AugmentTier.Silver,
                60f,
                new PlayerStatModifier { moveSpeedBonus = 0.22f, dashSpeedBonus = 0.15f },
                "Generic"
            );

            CreateOrUpdateAugment(
                "AUG_SILV_MAGNET_COIN",
                "Cự Lực Thu Hồn",
                "Tăng <b>+120% Bán Kính Thu Hút</b> ngọc kinh nghiệm và tiền cổ rơi ra trên toàn chiến trường.",
                AugmentTier.Silver,
                60f,
                new PlayerStatModifier { pickupRangeBonus = 3.5f },
                "Generic"
            );

            CreateOrUpdateAugment(
                "AUG_SILV_SWIFT_COOLDOWN",
                "Linh Hoạt Tuyệt Luân",
                "Giảm <b>-18% Thời Gian Hồi Chiêu</b> của toàn bộ kỹ năng kích hoạt và vũ khí.",
                AugmentTier.Silver,
                60f,
                new PlayerStatModifier { dashCooldownReduction = 0.18f },
                "Generic"
            );

            CreateOrUpdateAugment(
                "AUG_SILV_WAR_DRUM",
                "Trống Trận Lạc Hồng",
                "Tăng <b>+18% Sát Thương Cơ Bản</b> cho tất cả các nguồn sát thương.",
                AugmentTier.Silver,
                60f,
                new PlayerStatModifier { baseDamageBonus = 12f },
                "Generic"
            );

            CreateOrUpdateAugment(
                "AUG_SILV_IRON_SKIN",
                "Thiết Bì Kim Cương",
                "Giảm <b>-12% Mọi Nguồn Sát Thương</b> mà nhân vật phải gánh chịu từ kẻ địch.",
                AugmentTier.Silver,
                60f,
                new PlayerStatModifier { maxHealthBonus = 100f },
                "TanVien"
            );
        }
        #endregion

        #region 4. Thẻ Chỉ Số Nền Tảng (Micro-Cards 1 Dòng)
        private static void GenerateGenericMicroStats()
        {
            CreateOrUpdateMicroStat(
                "MICRO_DAMAGE_RAW",
                "Cường Lực Bội Tăng",
                "+8 Sát Thương Cơ Bản",
                "Tăng trực tiếp sát thương đòn đánh căn bản cho mọi vũ khí.",
                MythicArchetype.None,
                new PlayerStatModifier { baseDamageBonus = 8f },
                "Damage"
            );

            CreateOrUpdateMicroStat(
                "MICRO_ATK_SPEED",
                "Thần Tốc Đao Pháp",
                "+14% Tốc Độ Đánh",
                "Tăng tốc độ vung đao và bắn tên của tất cả pháp bảo.",
                MythicArchetype.None,
                new PlayerStatModifier { attackSpeedBonus = 0.14f },
                "AttackSpeed"
            );

            CreateOrUpdateMicroStat(
                "MICRO_MOVE_SPEED",
                "Phi Thần Bộ",
                "+10% Tốc Độ Chạy",
                "Gia tăng độ linh hoạt khi né tránh đàn quái vật áp sát.",
                MythicArchetype.None,
                new PlayerStatModifier { moveSpeedBonus = 0.10f },
                "Mobility"
            );

            CreateOrUpdateMicroStat(
                "MICRO_CRIT_CHANCE",
                "Điểm Huyệt Tuyệt Kỹ",
                "+7% Tỉ Lệ Chí Mạng",
                "Tăng xác suất gây đòn đánh chí mạng nhân đôi sát thương.",
                MythicArchetype.None,
                new PlayerStatModifier { critChanceBonus = 0.07f },
                "Crit"
            );

            CreateOrUpdateMicroStat(
                "MICRO_MAX_HP",
                "Huyết Khí Tinh Hoa",
                "+120 Máu Tối Đa",
                "Mở rộng thanh sinh lực giúp chống chịu tốt hơn trước bầy quái đông đúc.",
                MythicArchetype.None,
                new PlayerStatModifier { maxHealthBonus = 120f },
                "Health"
            );

            CreateOrUpdateMicroStat(
                "MICRO_PICKUP_RANGE",
                "Lực Hút Vạn Vật",
                "+40% Tầm Hút Ngọc",
                "Mở rộng tầm hút ngọc kinh nghiệm và cổ tiền từ xa.",
                MythicArchetype.None,
                new PlayerStatModifier { pickupRangeBonus = 1.8f },
                "Utility"
            );

            CreateOrUpdateMicroStat(
                "MICRO_DASH_CD",
                "Linh Động Lướt",
                "-0.4s Hồi Chiêu Lướt",
                "Rút ngắn thời gian nạp lại kỹ năng lướt né đòn.",
                MythicArchetype.None,
                new PlayerStatModifier { dashCooldownReduction = 0.4f },
                "Mobility"
            );

            CreateOrUpdateMicroStat(
                "MICRO_AOE_SCALE",
                "Khí Hải Uy Áp",
                "+15% Vùng Ảnh Hưởng AoE",
                "Mở rộng tầm ảnh hưởng của các chiêu thức và vũ khí diện rộng.",
                MythicArchetype.None,
                new PlayerStatModifier { areaScaleBonus = 0.15f },
                "AoE"
            );
        }

        private static void GenerateArchetypeMicroStats()
        {
            CreateOrUpdateMicroStat(
                "MICRO_FIRE_BURN_RATE",
                "Hỏa Linh Tụ Khí",
                "+20% Sát Thương Đốt Hỏa",
                "Cường hóa sát thương thiêu đốt hệ Hỏa cho trường phái Phù Đổng.",
                MythicArchetype.PhuDongThienUy,
                new PlayerStatModifier { fireDamageBonus = 0.20f },
                "PhuDong"
            );

            CreateOrUpdateMicroStat(
                "MICRO_PIERCE_BONUS",
                "Xuyên Vân Tiễn Quyết",
                "+1 Số Lần Xuyên Thấu & +5% Tốc Đánh",
                "Tăng khả năng xuyên thủng hàng ngũ địch cho trường phái Kim Quy.",
                MythicArchetype.KimQuyThanCo,
                new PlayerStatModifier { attackSpeedBonus = 0.05f, baseDamageBonus = 5f },
                "KimQuy"
            );

            CreateOrUpdateMicroStat(
                "MICRO_ARMOR_BLOCK",
                "Bàn Thạch Hộ Thể",
                "+80 Máu & +10% Kháng Sát Thương",
                "Gia cố sức bền vững chãi cho trường phái Tản Viên Sơn Thánh.",
                MythicArchetype.TanVienSonThanh,
                new PlayerStatModifier { maxHealthBonus = 80f },
                "TanVien"
            );

            CreateOrUpdateMicroStat(
                "MICRO_SLOW_DURATION",
                "Hàn Băng Thấu Xương",
                "+25% Hiệu Quả Làm Chậm & Đóng Băng",
                "Tăng cường khả năng khống chế diện rộng cho trường phái Thủy Bá.",
                MythicArchetype.ThuyBaCuongNo,
                new PlayerStatModifier { areaScaleBonus = 0.10f, baseDamageBonus = 5f },
                "ThuyBa"
            );

            CreateOrUpdateMicroStat(
                "MICRO_YIN_YANG_POWER",
                "Vô Cực Kình Lực",
                "+10% Sát Thương & +5% Chí Mạng",
                "Tăng cường uy lực khí công cho trường phái Long Tiên Huyết Mạch.",
                MythicArchetype.LongTienHuyetMach,
                new PlayerStatModifier { baseDamageBonus = 6f, critChanceBonus = 0.05f },
                "LongTien"
            );
        }
        #endregion

        #region Helpers
        private static void CreateOrUpdateAugment(
            string id,
            string name,
            string desc,
            AugmentTier tier,
            float weight,
            PlayerStatModifier statMod,
            string synergyTag)
        {
            string path = $"{AUGMENTS_DIR}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<MutationAugmentUpgradeData>(path);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<MutationAugmentUpgradeData>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.id = id;
            asset.upgradeName = name;
            asset.description = desc;
            asset.tier = tier;
            asset.upgradeType = UpgradeType.BreakthroughUltimate;
            asset.spawnWeight = weight;
            asset.maxLevel = 1;
            asset.statModifier = statMod;
            asset.synergyTag = synergyTag;

            EditorUtility.SetDirty(asset);
        }

        private static void CreateOrUpdateMicroStat(
            string id,
            string name,
            string oneLineSummary,
            string desc,
            MythicArchetype preferredArchetype,
            PlayerStatModifier statMod,
            string synergyTag)
        {
            string path = $"{MICRO_STATS_DIR}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<StatMicroUpgradeData>(path);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<StatMicroUpgradeData>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.id = id;
            asset.upgradeName = name;
            asset.oneLineSummary = oneLineSummary;
            asset.description = desc;
            asset.preferredArchetype = preferredArchetype;
            asset.upgradeType = UpgradeType.CommonUpgrade;
            asset.spawnWeight = 60f;
            asset.maxLevel = 5;
            asset.statModifier = statMod;
            asset.synergyTag = synergyTag;

            EditorUtility.SetDirty(asset);
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        #endregion
    }
}
#endif
