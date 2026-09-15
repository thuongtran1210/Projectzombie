#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Upgrades.Runtimes;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Combat;

namespace ProjectZombie.Features.Upgrades.Editor
{
    /// <summary>
    /// Editor Tool tự động hóa 100% việc sinh dữ liệu:
    /// 1. Tự tạo 5 Prefabs Runtime cho 5 Lõi Thần Thoại.
    /// 2. Tự tạo 5 ScriptableObjects Đại Lõi Kim Cương (MythicCoreUpgradeData).
    /// 3. Tự tạo 25 ScriptableObjects Thẻ Nhánh Độc Quyền (SynergyTraitUpgradeData).
    /// 4. Tự động kiểm tra và thêm component PlayerCombatEvents & PlayerMythicManager vào Player Prefab.
    /// </summary>
    public static class MythicCoresDataGenerator
    {
        private const string BASE_DIR = "Assets/_Data/Upgrades/Mythic";
        private const string PREFAB_DIR = "Assets/_Data/Upgrades/Mythic/Prefabs";
        private const string TRAITS_DIR = "Assets/_Data/Upgrades/Mythic/Traits";

        [MenuItem("ProjectZombie/0. 🌟 Mythic Setup & Refresh Pool (1-Click)", priority = -100)]
        [MenuItem("ProjectZombie/Setup/1. Generate 5 Mythic Cores & 25 Traits (1-Click Setup)")]
        public static void GenerateAllMythicData()
        {
            EnsureDirectories();

            // 1. Tạo Prefabs
            var phuDongPrefab = CreateOrGetRuntimePrefab<PhuDongCoreRuntime>("PhuDongCoreRuntime");
            var kimQuyPrefab = CreateOrGetRuntimePrefab<KimQuyCoreRuntime>("KimQuyCoreRuntime");
            var sonThanhPrefab = CreateOrGetRuntimePrefab<SonThanhCoreRuntime>("SonThanhCoreRuntime");
            var thuyBaPrefab = CreateOrGetRuntimePrefab<ThuyBaCoreRuntime>("ThuyBaCoreRuntime");
            var longTienPrefab = CreateOrGetRuntimePrefab<LongTienCoreRuntime>("LongTienCoreRuntime");

            // 2. Tạo 5 Đại Lõi Kim Cương
            CreateCoreAsset("CORE_PHUDONG", "Phù Đổng Thiên Uy", "PHÙ ĐỔNG THẦN TƯỚNG",
                "Diệt 50 quái <color=#FF7700>phóng to 10% kích thước</color> và +50 HP (Max 200%). Lướt cưỡi Ngựa Sắt phun lửa càn quét. Hồi sinh giáng sét dọn map 10m.",
                MythicArchetype.PhuDongThienUy, ElementType.Hoa, phuDongPrefab);

            CreateCoreAsset("CORE_KIMQUY", "Kim Quy Thần Cơ", "NHẤT TIỄN VẠN TIỄN",
                "Tất cả vũ khí <color=#FFD700>bắn đạn nảy phân tách</color> sang 2 mục tiêu. Đứng yên 1s kích hoạt Mai Rùa giảm 50% sát thương. Hồi sinh bất tử 4s.",
                MythicArchetype.KimQuyThanCo, ElementType.Kim, kimQuyPrefab);

            CreateCoreAsset("CORE_TANVIEN", "Tản Viên Sơn Thánh", "BẠT SƠN DỜI LŨY",
                "Sở hữu <color=#D4AF37>Giáp Đá Bất Hoại 100% Max HP</color> (hồi sau 8s). Đứng yên 2.5s triệu hồi 4 Thạch Trụ đè bẹp quái vật xung quanh.",
                MythicArchetype.TanVienSonThanh, ElementType.Tho, sonThanhPrefab);

            CreateCoreAsset("CORE_THUYBA", "Thủy Bá Cuồng Nộ", "HÔ PHONG HOÁN VŨ",
                "Mưa bão toàn map, quái bị <color=#00BFFF>Ẩm Ướt</color> và giảm 30% tốc chạy. Mỗi 6s tạo 1 đợt Sóng Thần gom quái. 30% Đóng Băng & nổ mảnh vụn.",
                MythicArchetype.ThuyBaCuongNo, ElementType.Thuy, thuyBaPrefab);

            CreateCoreAsset("CORE_LONGTIEN", "Long Tiên Huyết Mạch", "THÁI CỰC LONG TIÊN BIẾN",
                "Chuyển đổi 2 dạng Rồng (Dương - <color=#FF00E5>+100% Sát thương</color>) và Tiên (Âm - Hồi 2% HP/s). Sở hữu <color=#FFD700>1 Mạng Miễn Tử</color> chặn chết!",
                MythicArchetype.LongTienHuyetMach, ElementType.None, longTienPrefab);

            // 3. Tạo 25 Thẻ Nhánh Độc Quyền
            GenerateTraitsForPhuDong();
            GenerateTraitsForKimQuy();
            GenerateTraitsForSonThanh();
            GenerateTraitsForThuyBa();
            GenerateTraitsForLongTien();

            // 4. Auto-setup Player Prefabs & UpgradeManager Pool
            SetupPlayerComponentsInSceneOrPrefab();
            PopulateAllUpgradeManagers();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color=#00FF88>===============================================================</color>");
            Debug.Log("<color=#00FF88>[MythicSetup] THÀNH CÔNG: Đã sinh trọn bộ 5 Đại Lõi & 25 Thẻ Nhánh và nạp sạch Data Pool!</color>");
            Debug.Log("<color=#00FF88>===============================================================</color>");
            EditorUtility.DisplayDialog("Mythic Setup Complete", "Đã khởi tạo hoàn tất 5 Đại Lõi Thần Thoại, 25 Thẻ Nhánh và tự động nạp sạch sẽ UpgradeManager Data Pool!", "Tuyệt vời");
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(BASE_DIR)) Directory.CreateDirectory(BASE_DIR);
            if (!Directory.Exists(PREFAB_DIR)) Directory.CreateDirectory(PREFAB_DIR);
            if (!Directory.Exists(TRAITS_DIR)) Directory.CreateDirectory(TRAITS_DIR);
        }

        private static T CreateOrGetRuntimePrefab<T>(string prefabName) where T : MythicCoreRuntime
        {
            string path = $"{PREFAB_DIR}/{prefabName}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null)
            {
                return existing.GetComponent<T>();
            }

            var go = new GameObject(prefabName);
            var comp = go.AddComponent<T>();
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<T>();
        }

        private static void CreateCoreAsset(string id, string cardName, string title, string desc, MythicArchetype archetype, ElementType element, MythicCoreRuntime runtimePrefab)
        {
            string path = $"{BASE_DIR}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<MythicCoreUpgradeData>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<MythicCoreUpgradeData>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.id = id;
            asset.upgradeName = cardName;
            asset.mythicTitle = title;
            asset.description = desc;
            asset.archetype = archetype;
            asset.element = element;
            asset.runtimePrefab = runtimePrefab;
            asset.upgradeType = UpgradeType.MythicCore;
            asset.spawnWeight = 1.0f;
            asset.maxLevel = 1;

            EditorUtility.SetDirty(asset);
        }

        private static void CreateTraitAsset(string id, string cardName, string desc, MythicArchetype archetype, ElementType element, PlayerStatModifier modifier, int maxLevel = 3)
        {
            string path = $"{TRAITS_DIR}/{id}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<SynergyTraitUpgradeData>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<SynergyTraitUpgradeData>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.id = id;
            asset.upgradeName = cardName;
            asset.description = desc;
            asset.requiredArchetype = archetype;
            asset.element = element;
            asset.playerStatModifier = modifier;
            asset.upgradeType = UpgradeType.SynergyTrait;
            asset.spawnWeight = 1.0f;
            asset.maxLevel = maxLevel;

            EditorUtility.SetDirty(asset);
        }

        private static void GenerateTraitsForPhuDong()
        {
            CreateTraitAsset("TRAIT_PD_01", "Hỏa Ký Đạp Lôi", "Khi Lướt (Dash), tăng +25% tốc độ lướt và giảm 0.5s hồi chiêu lướt.",
                MythicArchetype.PhuDongThienUy, ElementType.Hoa, new PlayerStatModifier { dashSpeedBonus = 0.25f, dashCooldownReduction = 0.5f });

            CreateTraitAsset("TRAIT_PD_02", "Nhổ Tre Đánh Giặc", "Tăng +15 sát thương cơ bản và +20% tốc độ xuất chiêu cận chiến.",
                MythicArchetype.PhuDongThienUy, ElementType.Hoa, new PlayerStatModifier { baseDamageBonus = 15f, attackSpeedBonus = 0.20f });

            CreateTraitAsset("TRAIT_PD_03", "Huyết Khí Thần Đồng", "Tăng +100 Máu tối đa và +10% Tốc độ di chuyển.",
                MythicArchetype.PhuDongThienUy, ElementType.Hoa, new PlayerStatModifier { maxHealthBonus = 100f, moveSpeedBonus = 0.10f });

            CreateTraitAsset("TRAIT_PD_04", "Thiết Giáp Bất Phá", "Tăng +150 Máu tối đa.",
                MythicArchetype.PhuDongThienUy, ElementType.Hoa, new PlayerStatModifier { maxHealthBonus = 150f });

            CreateTraitAsset("TRAIT_PD_05", "Phù Đổng Nộ Hống", "Tăng +25% Sát thương cơ bản và +15% Tỉ lệ Bạo Kích.",
                MythicArchetype.PhuDongThienUy, ElementType.Hoa, new PlayerStatModifier { baseDamageBonus = 25f, critChanceBonus = 0.15f });
        }

        private static void GenerateTraitsForKimQuy()
        {
            CreateTraitAsset("TRAIT_KQ_01", "Linh Quy Hộ Quốc Trận", "Tăng +80 Máu tối đa và +1.0m Bán kính nhặt vật phẩm.",
                MythicArchetype.KimQuyThanCo, ElementType.Kim, new PlayerStatModifier { maxHealthBonus = 80f, pickupRangeBonus = 1.0f });

            CreateTraitAsset("TRAIT_KQ_02", "Mũi Tên Đồng Cổ Loa", "Tăng +20% Tỉ lệ Bạo Kích và +10 Sát thương cơ bản.",
                MythicArchetype.KimQuyThanCo, ElementType.Kim, new PlayerStatModifier { critChanceBonus = 0.20f, baseDamageBonus = 10f });

            CreateTraitAsset("TRAIT_KQ_03", "Mắt Thần Xuyên Tâm", "Tăng +25% Tốc độ tấn công và +15% Kinh nghiệm nhận được.",
                MythicArchetype.KimQuyThanCo, ElementType.Kim, new PlayerStatModifier { attackSpeedBonus = 0.25f, expMultiplierBonus = 0.15f });

            CreateTraitAsset("TRAIT_KQ_04", "Kim Quy Trợ Lực", "Tăng +20% Tốc độ di chuyển và giảm 0.3s hồi chiêu lướt.",
                MythicArchetype.KimQuyThanCo, ElementType.Kim, new PlayerStatModifier { moveSpeedBonus = 0.20f, dashCooldownReduction = 0.3f });

            CreateTraitAsset("TRAIT_KQ_05", "Cơ Quan Tốc Xạ", "Tăng +35% Tốc độ tấn công của tất cả vũ khí tầm xa.",
                MythicArchetype.KimQuyThanCo, ElementType.Kim, new PlayerStatModifier { attackSpeedBonus = 0.35f });
        }

        private static void GenerateTraitsForSonThanh()
        {
            CreateTraitAsset("TRAIT_ST_01", "Chấn Địa Nham Thạch", "Tăng +120 Máu tối đa và +15 Sát thương cơ bản.",
                MythicArchetype.TanVienSonThanh, ElementType.Tho, new PlayerStatModifier { maxHealthBonus = 120f, baseDamageBonus = 15f });

            CreateTraitAsset("TRAIT_ST_02", "Thần Thổ Dưỡng Khí", "Tăng +150 Máu tối đa và +1.5m Bán kính nhặt vật phẩm.",
                MythicArchetype.TanVienSonThanh, ElementType.Tho, new PlayerStatModifier { maxHealthBonus = 150f, pickupRangeBonus = 1.5f });

            CreateTraitAsset("TRAIT_ST_03", "Kim Cương Nham Bì", "Tăng +200 Máu tối đa.",
                MythicArchetype.TanVienSonThanh, ElementType.Tho, new PlayerStatModifier { maxHealthBonus = 200f });

            CreateTraitAsset("TRAIT_ST_04", "Địa Chấn Phản Phách", "Tăng +20 Sát thương cơ bản và +10% Tỉ lệ Bạo Kích.",
                MythicArchetype.TanVienSonThanh, ElementType.Tho, new PlayerStatModifier { baseDamageBonus = 20f, critChanceBonus = 0.10f });

            CreateTraitAsset("TRAIT_ST_05", "Sơn Thần Uy Áp", "Tăng +100 Máu tối đa và +15% Tốc độ di chuyển.",
                MythicArchetype.TanVienSonThanh, ElementType.Tho, new PlayerStatModifier { maxHealthBonus = 100f, moveSpeedBonus = 0.15f });
        }

        private static void GenerateTraitsForThuyBa()
        {
            CreateTraitAsset("TRAIT_TB_01", "Băng Phong Vạn Lý", "Tăng +15% Tỉ lệ Bạo Kích và +15 Sát thương cơ bản.",
                MythicArchetype.ThuyBaCuongNo, ElementType.Thuy, new PlayerStatModifier { critChanceBonus = 0.15f, baseDamageBonus = 15f });

            CreateTraitAsset("TRAIT_TB_02", "Thủy Long Cuộn Trào", "Tăng +20% Tốc độ di chuyển và giảm 0.4s hồi chiêu lướt.",
                MythicArchetype.ThuyBaCuongNo, ElementType.Thuy, new PlayerStatModifier { moveSpeedBonus = 0.20f, dashCooldownReduction = 0.4f });

            CreateTraitAsset("TRAIT_TB_03", "Thủy Triều Dâng Cao", "Tăng +25% Kinh nghiệm nhận được và +2.0m Bán kính nhặt đồ.",
                MythicArchetype.ThuyBaCuongNo, ElementType.Thuy, new PlayerStatModifier { expMultiplierBonus = 0.25f, pickupRangeBonus = 2.0f });

            CreateTraitAsset("TRAIT_TB_04", "Hàn Khí Thấu Xương", "Tăng +20% Tốc độ xuất chiêu và +10 Sát thương cơ bản.",
                MythicArchetype.ThuyBaCuongNo, ElementType.Thuy, new PlayerStatModifier { attackSpeedBonus = 0.20f, baseDamageBonus = 10f });

            CreateTraitAsset("TRAIT_TB_05", "Thủy Lưu Hồi Chuyển", "Giảm 0.5s hồi chiêu lướt và tăng +20% Tốc độ lướt.",
                MythicArchetype.ThuyBaCuongNo, ElementType.Thuy, new PlayerStatModifier { dashCooldownReduction = 0.5f, dashSpeedBonus = 0.20f });
        }

        private static void GenerateTraitsForLongTien()
        {
            CreateTraitAsset("TRAIT_LT_01", "Bách Noãn Hộ Thể", "Tăng +100 Máu tối đa và +10 Sát thương cơ bản.",
                MythicArchetype.LongTienHuyetMach, ElementType.None, new PlayerStatModifier { maxHealthBonus = 100f, baseDamageBonus = 10f });

            CreateTraitAsset("TRAIT_LT_02", "Âm Dương Giao Hòa", "Tăng +15% Tốc chạy, +15% Tốc đánh và +15% Kinh nghiệm nhận được.",
                MythicArchetype.LongTienHuyetMach, ElementType.None, new PlayerStatModifier { moveSpeedBonus = 0.15f, attackSpeedBonus = 0.15f, expMultiplierBonus = 0.15f });

            CreateTraitAsset("TRAIT_LT_03", "Hồng Bàng Khí Vận", "Tăng +35% Lượng kinh nghiệm nhận được và +2.0m Bán kính nhặt đồ.",
                MythicArchetype.LongTienHuyetMach, ElementType.None, new PlayerStatModifier { expMultiplierBonus = 0.35f, pickupRangeBonus = 2.0f });

            CreateTraitAsset("TRAIT_LT_04", "Long Uy Phấn Chấn", "Tăng +30% Tốc độ đánh và +20 Sát thương cơ bản.",
                MythicArchetype.LongTienHuyetMach, ElementType.None, new PlayerStatModifier { attackSpeedBonus = 0.30f, baseDamageBonus = 20f });

            CreateTraitAsset("TRAIT_LT_05", "Tiên Âm Dưỡng Hồn", "Tăng +150 Máu tối đa và +10% Tốc độ di chuyển.",
                MythicArchetype.LongTienHuyetMach, ElementType.None, new PlayerStatModifier { maxHealthBonus = 150f, moveSpeedBonus = 0.10f });
        }

        private static void SetupPlayerComponentsInSceneOrPrefab()
        {
            // Tìm tất cả Player Prefabs trong Project
            string[] guids = AssetDatabase.FindAssets("t:Prefab Player");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null && prefab.GetComponent<PlayerStats>() != null)
                {
                    bool isDirty = false;
                    if (prefab.GetComponent<PlayerCombatEvents>() == null)
                    {
                        prefab.AddComponent<PlayerCombatEvents>();
                        isDirty = true;
                    }
                    if (prefab.GetComponent<PlayerMythicManager>() == null)
                    {
                        prefab.AddComponent<PlayerMythicManager>();
                        isDirty = true;
                    }

                    if (isDirty)
                    {
                        EditorUtility.SetDirty(prefab);
                        Debug.Log($"[MythicSetup] Đã tự động gắn PlayerCombatEvents & PlayerMythicManager vào Player Prefab tại: {path}");
                    }
                }
            }
        }

        private static void PopulateAllUpgradeManagers()
        {
            var managers = Object.FindObjectsOfType<UpgradeManager>();
            foreach (var mgr in managers)
            {
                mgr.PopulateAllAvailableUpgrades();
                EditorUtility.SetDirty(mgr);
            }
        }
    }
}
#endif
