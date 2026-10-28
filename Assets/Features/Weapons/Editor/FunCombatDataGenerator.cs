#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Shared;

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
            public bool isPassive;
            public float activeCd;
            public float activeDur;
            public string skillName;

            public FunWeaponDef(string id, string name, ElementType element, float damage, float cooldown, string desc, WeaponRole role, bool isPassive, float activeCd, float activeDur, string skillName)
            {
                this.id = id;
                this.name = name;
                this.element = element;
                this.damage = damage;
                this.cooldown = cooldown;
                this.desc = desc;
                this.role = role;
                this.isPassive = isPassive;
                this.activeCd = activeCd;
                this.activeDur = activeDur;
                this.skillName = skillName;
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
                new FunWeaponDef("W_SLIPPER", "Dép Tổ Ong Thần Sa", ElementType.Kim, 25f, 1.2f, "Kỹ năng chủ động: Quăng Boomerang Dép khổng lồ + Lốc Dép Vạn Năng gây Quê Độ 100%.", WeaponRole.RelicOnHitTrigger, false, 7.0f, 0f, "Tổ Ong Lượn Cánh"),
                new FunWeaponDef("W_POT", "Nồi Cơm Thạch Sanh", ElementType.Tho, 35f, 2.0f, "Kỹ năng chủ động: Gom quái diện rộng 6m vào tâm nồi, hất văng và hồi 15% Max HP.", WeaponRole.RelicOrbitalShield, false, 14.0f, 0f, "Hút Chân Không & Tiên Cơm"),
                new FunWeaponDef("W_PIPE", "Điếu Cày Cửu U", ElementType.Hoa, 20f, 1.8f, "Kỹ năng chủ động: Rít hơi dài nhả bão khói diện rộng làm quái đi giật lùi và ho nổ sát thương.", WeaponRole.RelicSupportAura, false, 9.0f, 0f, "Bão Khói Thuốc Lào"),
                new FunWeaponDef("R007", "Chiếu Trải Hoàng Tuyền", ElementType.Moc, 0f, 8.0f, "Kỹ năng chủ động: Chọn vị trí đặt chiếu bẫy khiến quái ngủ say và nhận thêm sát thương chí mạng; người chơi bước lên chiếu sẽ trượt xuyên qua bầy quái.", WeaponRole.RelicSupportAura, false, 7.5f, 4f, "Đặt Bẫy Chiếu"),
                new FunWeaponDef("R008", "Chổi Lông Gà Gia Truyền", ElementType.Kim, 45f, 4.0f, "Kỹ năng chủ động: Ngắm đường bay để phóng lốc chổi quét và đẩy lùi quái; trúng địch có thể triệu hồi gà con hỗ trợ.", WeaponRole.RelicOnHitTrigger, false, 9f, 0f, "Lốc Xoáy Quét Rác")
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
                so.isPassiveRelic = item.isPassive;
                so.activeCooldown = item.activeCd;
                so.activeDuration = item.activeDur;
                so.skillActionName = item.skillName;

                if (prefabObj != null)
                {
                    so.weaponPrefab = prefabObj.GetComponent<WeaponBase>();
                }

                // Tự động tìm và gán Sprite Icon trong Assets/Art/Weapons
                string iconPath = $"Assets/Art/Weapons/Icon_{item.id}.png";
                if (item.id == "R007") iconPath = "Assets/Art/Weapons/Icon_R007_ChieuTrai.png";
                if (item.id == "R008") iconPath = "Assets/Art/Weapons/Icon_R008_ChoiLongGa.png";
                Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                if (iconSprite != null)
                {
                    so.icon = iconSprite;
                    if (prefabObj != null)
                    {
                        var comp = prefabObj.GetComponent<WeaponBase>();
                        if (comp != null) comp.icon = iconSprite;
                    }
                }

                EditorUtility.SetDirty(so);
            }

            // =========================================================================
            // 2. TẠO TOÀN BỘ CÁC THẺ UPGRADE TƯƠNG ỨNG (CẤP 2 -> CẤP 6)
            // =========================================================================
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

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
