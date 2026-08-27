#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Upgrades;

namespace ProjectZombie.Features.Weapons.Editor
{
    /// <summary>
    /// Editor Tool tự động tạo toàn bộ ScriptableObject WeaponData và WeaponUpgradeData cho Hệ Thống Slapstick/Fun Combat (GDD v5.1).
    /// Menu: ProjectZombie > Weapons > Generate Slapstick Weapons & Relics SOs
    /// </summary>
    public static class FunCombatDataGenerator
    {
        private struct FunWeaponDef
        {
            public string id;
            public string name;
            public ElementType element;
            public float damage;
            public float cooldown;
            public string desc;
            public WeaponRole role;

            public FunWeaponDef(string id, string name, ElementType element, float damage, float cooldown, string desc, WeaponRole role)
            {
                this.id = id;
                this.name = name;
                this.element = element;
                this.damage = damage;
                this.cooldown = cooldown;
                this.desc = desc;
                this.role = role;
            }
        }

        private struct FunUpgradeDef
        {
            public string id;
            public string weaponId;
            public string name;
            public string desc;
            public int reqLevel;
            public ElementType element;
            public WeaponStatModifier modifier;

            public FunUpgradeDef(string id, string weaponId, string name, string desc, int reqLevel, ElementType element, WeaponStatModifier modifier)
            {
                this.id = id;
                this.weaponId = weaponId;
                this.name = name;
                this.desc = desc;
                this.reqLevel = reqLevel;
                this.element = element;
                this.modifier = modifier;
            }
        }

        [MenuItem("ProjectZombie/Weapons/Generate Slapstick Weapons & Relics SOs")]
        public static void GenerateFunCombatData()
        {
            string weaponFolder = "Assets/_Data/Weapons";
            string upgradeFolder = "Assets/_Data/Upgrades/Slapstick";

            if (!AssetDatabase.IsValidFolder("Assets/_Data")) AssetDatabase.CreateFolder("Assets", "_Data");
            if (!AssetDatabase.IsValidFolder(weaponFolder)) AssetDatabase.CreateFolder("Assets/_Data", "Weapons");
            if (!AssetDatabase.IsValidFolder("Assets/_Data/Upgrades")) AssetDatabase.CreateFolder("Assets/_Data", "Upgrades");
            if (!AssetDatabase.IsValidFolder(upgradeFolder)) AssetDatabase.CreateFolder("Assets/_Data/Upgrades", "Slapstick");

            // =========================================================================
            // 1. TẠO 5 SCRIPTABLE OBJECT WEAPON DATA (TOÀN BỘ LÀ PHÁP BẢO HỘ THÂN - RELICS)
            // =========================================================================
            FunWeaponDef[] funItems = new FunWeaponDef[]
            {
                new FunWeaponDef("W_SLIPPER", "Dép Tổ Ong Thần Sa", ElementType.Kim, 25f, 1.2f, "Pháp bảo ném Boomerang tự động; Hit 3 quăng lốc dép gây hiệu ứng 'Quê Độ' khiến quái quay sang đấm đồng minh.", WeaponRole.RelicOnHitTrigger),
                new FunWeaponDef("W_POT", "Nồi Cơm Thạch Sanh", ElementType.Tho, 35f, 2.0f, "Pháp bảo cận chiến gom tối đa 3 quái vào nồi và phóng ra như đạn pháo; chạm đất rơi cơm nắm hồi máu.", WeaponRole.RelicOrbitalShield),
                new FunWeaponDef("W_PIPE", "Điếu Cày Cửu U", ElementType.Hoa, 20f, 1.8f, "Pháp bảo phun luồng khói dày đặc gây hiệu ứng 'Say Thuốc Lào' khiến quái đi giật lùi và nổ sát thương ho sặc sụa.", WeaponRole.RelicSupportAura),
                new FunWeaponDef("R007", "Chiếu Trải Hoàng Tuyền", ElementType.Moc, 0f, 8.0f, "Pháp bảo thả chiếu khiến quái ngủ say (nhận x2 Crit DMG khi bị đánh thức); Người chơi bước lên trượt ván ủi văng quái.", WeaponRole.RelicSupportAura),
                new FunWeaponDef("R008", "Chổi Lông Gà Gia Truyền", ElementType.Kim, 45f, 4.0f, "Triệu hồi Chổi Lông Gà khổng lồ giáng từ trời xuống với lực Knockback 12m/s cực đại và găm quái vào tường.", WeaponRole.RelicOnHitTrigger)
            };

            for (int i = 0; i < funItems.Length; i++)
            {
                var item = funItems[i];
                string assetPath = $"{weaponFolder}/Weapon_{item.id}.asset";
                WeaponData so = AssetDatabase.LoadAssetAtPath<WeaponData>(assetPath);

                if (so == null)
                {
                    so = ScriptableObject.CreateInstance<WeaponData>();
                    AssetDatabase.CreateAsset(so, assetPath);
                }

                // Tự động tạo và liên kết Prefab tương ứng trong Assets/_Prefabs/Weapons
                string prefabFolder = "Assets/_Prefabs/Weapons";
                if (!AssetDatabase.IsValidFolder("Assets/_Prefabs")) AssetDatabase.CreateFolder("Assets", "_Prefabs");
                if (!AssetDatabase.IsValidFolder(prefabFolder)) AssetDatabase.CreateFolder("Assets/_Prefabs", "Weapons");

                string prefabPath = $"{prefabFolder}/Weapon_{item.id}.prefab";
                GameObject prefabObj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefabObj == null)
                {
                    GameObject tempGo = new GameObject($"Weapon_{item.id}");
                    System.Type compType = GetWeaponComponentType(item.id);
                    if (compType != null)
                    {
                        var comp = tempGo.AddComponent(compType) as WeaponBase;
                        if (comp != null)
                        {
                            comp.weaponId = item.id;
                            comp.displayName = item.name;
                            comp.description = item.desc;
                        }
                    }
                    prefabObj = PrefabUtility.SaveAsPrefabAsset(tempGo, prefabPath);
                    Object.DestroyImmediate(tempGo);
                }

                so.weaponRole = item.role;
                so.weaponId = item.id;
                so.weaponName = item.name;
                so.description = item.desc;
                so.elementType = item.element;
                so.baseDamage = item.damage;
                so.baseAttackSpeed = item.cooldown;

                if (prefabObj != null)
                {
                    so.weaponPrefab = prefabObj.GetComponent<WeaponBase>();
                }

                EditorUtility.SetDirty(so);
            }

            // =========================================================================
            // 2. TẠO TOÀN BỘ CÁC THẺ UPGRADE TƯƠNG ỨNG (CẤP 2 -> CẤP 6)
            // =========================================================================
            FunUpgradeDef[] upgrades = new FunUpgradeDef[]
            {
                // --- W_SLIPPER: Dép Tổ Ong ---
                new FunUpgradeDef("UP_SLIPPER_02", "W_SLIPPER", "Song Dép Tăng Tốc", "+25% Sát thương, +20% Tốc độ bay của dép.", 1, ElementType.Kim, new WeaponStatModifier { damageBonus = 0.25f, projectileSpeedBonus = 0.20f }),
                new FunUpgradeDef("UP_SLIPPER_03", "W_SLIPPER", "Lốc Dép Gom Quái", "+35% Quy mô lốc xoáy quét quái và tăng lực hút vào tâm.", 2, ElementType.Kim, new WeaponStatModifier { scaleBonus = 0.35f, damageBonus = 0.15f }),
                new FunUpgradeDef("UP_SLIPPER_04", "W_SLIPPER", "Vả Quê Thần Sầu", "Tăng tỉ lệ gây 'Quê Độ' lên 80%, quái đấm bạn đồng minh đau hơn.", 3, ElementType.Kim, new WeaponStatModifier { critChanceBonus = 0.20f, damageBonus = 0.20f }),
                new FunUpgradeDef("UP_SLIPPER_05", "W_SLIPPER", "Tổ Ong Hoàng Kim", "+45% Sát thương đòn đánh và +30% Sát thương chí mạng.", 4, ElementType.Kim, new WeaponStatModifier { damageBonus = 0.45f, critDamageBonus = 0.30f }),
                new FunUpgradeDef("UP_SLIPPER_06", "W_SLIPPER", "Đột Phá: Vạn Dép Quy Tông", "ĐỘT PHÁ TỐI THƯỢNG: Ném 4 chiếc dép cùng lúc, 100% gây Quê Độ và tăng +60% Sát thương!", 5, ElementType.Kim, new WeaponStatModifier { damageBonus = 0.60f, projectileCountBonus = 2, critChanceBonus = 0.25f }),

                // --- W_POT: Nồi Cơm Thạch Sanh ---
                new FunUpgradeDef("UP_POT_02", "W_POT", "Nắp Nồi Gang Nặng", "+30% Sát thương gõ nắp, tăng thời gian Choáng lên 0.5s.", 1, ElementType.Tho, new WeaponStatModifier { damageBonus = 0.30f }),
                new FunUpgradeDef("UP_POT_03", "W_POT", "Lực Hút Chân Không", "Mở rộng bán kính hút quái lên 5.0m, hút tối đa 5 quái thường.", 2, ElementType.Tho, new WeaponStatModifier { scaleBonus = 0.40f, damageBonus = 0.15f }),
                new FunUpgradeDef("UP_POT_04", "W_POT", "Cơm Niêu Thơm Dẻo", "Cơm Nắm rơi ra khi quái chạm đất hồi +10% Max HP.", 3, ElementType.Tho, new WeaponStatModifier { damageBonus = 0.25f }),
                new FunUpgradeDef("UP_POT_05", "W_POT", "Đại Bác Thần Công", "+50% Sát thương đạn pháo quái vật và tăng tầm bay xa.", 4, ElementType.Tho, new WeaponStatModifier { damageBonus = 0.50f, projectileSpeedBonus = 0.30f }),
                new FunUpgradeDef("UP_POT_06", "W_POT", "Đột Phá: Nồi Thần Bất Tử", "ĐỘT PHÁ TỐI THƯỢNG: Quái bị bắn chạm đất nổ sát thương chuỗi diện rộng, hồi 15% HP!", 5, ElementType.Tho, new WeaponStatModifier { damageBonus = 0.80f, scaleBonus = 0.50f }),

                // --- W_PIPE: Điếu Cày Cửu U ---
                new FunUpgradeDef("UP_PIPE_02", "W_PIPE", "Tàn Lửa Cháy Rực", "+35% Sát thương Hỏa thiêu đốt DoT và tăng 1s thời gian cháy.", 1, ElementType.Hoa, new WeaponStatModifier { damageBonus = 0.35f }),
                new FunUpgradeDef("UP_PIPE_03", "W_PIPE", "Bão Khói Lan Rộng", "+40% Diện tích vùng khói và kéo dài thời gian tồn tại.", 2, ElementType.Hoa, new WeaponStatModifier { scaleBonus = 0.40f, damageBonus = 0.20f }),
                new FunUpgradeDef("UP_PIPE_04", "W_PIPE", "Thuốc Lào Tiên Lãng", "Quái dính khói bị say thuốc đi giật lùi nhanh hơn, sau 2s nổ ho lan x2 Sát thương.", 3, ElementType.Hoa, new WeaponStatModifier { damageBonus = 0.30f, critChanceBonus = 0.15f }),
                new FunUpgradeDef("UP_PIPE_05", "W_PIPE", "Cán Điếu Gỗ Mun", "+40% Sát thương đập gõ và đẩy lùi quái 3m.", 4, ElementType.Hoa, new WeaponStatModifier { damageBonus = 0.40f }),
                new FunUpgradeDef("UP_PIPE_06", "W_PIPE", "Đột Phá: Cửu U Long Phun Khói", "ĐỘT PHÁ TỐI THƯỢNG: Đám khói rồng cuộn bao phủ toàn màn hình, 100% quái trúng khói nổ tung!", 5, ElementType.Hoa, new WeaponStatModifier { damageBonus = 0.75f, scaleBonus = 0.60f }),

                // --- R007: Chiếu Trải Hoàng Tuyền ---
                new FunUpgradeDef("UP_R007_02", "R007", "Chiếu Đôi Rộng Lớn", "+40% Kích thước tấm chiếu trải, dễ dàng bẫy cả đàn quái.", 1, ElementType.Moc, new WeaponStatModifier { scaleBonus = 0.40f }),
                new FunUpgradeDef("UP_R007_03", "R007", "Giấc Ngủ Ngàn Thu", "Tăng thời gian ngủ lên 4.5s; Đòn đánh thức gây +150% Sát thương Chí mạng!", 2, ElementType.Moc, new WeaponStatModifier { critDamageBonus = 0.50f }),
                new FunUpgradeDef("UP_R007_04", "R007", "Trượt Bowling Siêu Tốc", "Khi người chơi trượt trên chiếu: Tăng +150% Tốc độ và tông văng quái xa hơn.", 3, ElementType.Moc, new WeaponStatModifier { damageBonus = 0.30f }),
                new FunUpgradeDef("UP_R007_05", "R007", "Chiếu Trải Liên Tục", "Giảm 30% thời gian hồi chiêu trải chiếu (thả chiếu mỗi 5.5s).", 4, ElementType.Moc, new WeaponStatModifier { attackSpeedBonus = 0.30f }),
                new FunUpgradeDef("UP_R007_06", "R007", "Đột Phá: Chiếu Thần Hoàng Kim", "ĐỘT PHÁ TỐI THƯỢNG: Thả 2 chiếu liên tiếp, quái ngủ nhận x3.0 Sát thương bạo kích!", 5, ElementType.Moc, new WeaponStatModifier { damageBonus = 0.60f, scaleBonus = 0.50f, critDamageBonus = 1.0f }),

                // --- R008: Chổi Lông Gà Gia Truyền ---
                new FunUpgradeDef("UP_R008_02", "R008", "Cán Chổi Gỗ Nghiến", "+30% Sát thương đập và tăng Lực đẩy lùi lên 15m/s.", 1, ElementType.Kim, new WeaponStatModifier { damageBonus = 0.30f }),
                new FunUpgradeDef("UP_R008_03", "R008", "Chổi Quét Sạch", "+40% Bán kính va đập quét quái từ trên trời.", 2, ElementType.Kim, new WeaponStatModifier { scaleBonus = 0.40f }),
                new FunUpgradeDef("UP_R008_04", "R008", "Đòn Phạt Tuổi Thơ", "Quái bị đập găm vào tường (Wall Splat) bị Choáng trong 2.0s.", 3, ElementType.Kim, new WeaponStatModifier { damageBonus = 0.35f, critChanceBonus = 0.20f }),
                new FunUpgradeDef("UP_R008_05", "R008", "Chổi Thần Trấn Quỷ", "+50% Sát thương toàn diện và giảm 1.5s hồi chiêu.", 4, ElementType.Kim, new WeaponStatModifier { damageBonus = 0.50f, attackSpeedBonus = 0.25f }),
                new FunUpgradeDef("UP_R008_06", "R008", "Đột Phá: Thiên Binh Chổi Quét", "ĐỘT PHÁ TỐI THƯỢNG: Triệu hồi 3 cây Chổi Khổng Lồ đập đồng thời, san phẳng chiến trường!", 5, ElementType.Kim, new WeaponStatModifier { damageBonus = 0.90f, projectileCountBonus = 2, scaleBonus = 0.50f })
            };

            for (int i = 0; i < upgrades.Length; i++)
            {
                var up = upgrades[i];
                string assetPath = $"{upgradeFolder}/{up.id}.asset";
                WeaponUpgradeData so = AssetDatabase.LoadAssetAtPath<WeaponUpgradeData>(assetPath);

                if (so == null)
                {
                    so = ScriptableObject.CreateInstance<WeaponUpgradeData>();
                    AssetDatabase.CreateAsset(so, assetPath);
                }

                so.id = up.id;
                so.weaponId = up.weaponId;
                so.upgradeName = up.name;
                so.description = up.desc;
                so.requiredCurrentLevel = up.reqLevel;
                so.element = up.element;
                so.upgradeType = up.reqLevel == 5 ? UpgradeType.BreakthroughUltimate : (up.weaponId.StartsWith("R") ? UpgradeType.RelicAwakening : UpgradeType.ComboAugment);
                so.spawnWeight = up.reqLevel == 5 ? 3f : 6f;
                so.statModifier = up.modifier;

                EditorUtility.SetDirty(so);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Nạp tự động vào UpgradeManager trong Scene nếu đang mở
            var upgradeManager = Object.FindAnyObjectByType<UpgradeManager>();
            if (upgradeManager != null)
            {
                upgradeManager.PopulateAllAvailableUpgrades();
                EditorUtility.SetDirty(upgradeManager);
            }

            Debug.Log($"[FunCombatDataGenerator] Đã tạo thành công {funItems.Length} Weapons và {upgrades.Length} Thẻ Nâng Cấp Slapstick!");
        }

        private static System.Type GetWeaponComponentType(string id)
        {
            switch (id.ToUpper())
            {
                case "W_SLIPPER": return typeof(Weapon_Slipper);
                case "W_POT": return typeof(Weapon_Pot);
                case "W_PIPE": return typeof(Weapon_Pipe);
                case "R007": return typeof(Relic_SleepingMat);
                case "R008": return typeof(Relic_ChickenFeatherBroom);
                default: return null;
            }
        }
    }
}
#endif
