#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Upgrades;

namespace ProjectZombie.Features.Weapons.Editor
{
    /// <summary>
    /// Editor Tool tự động tạo toàn bộ ScriptableObject cho Hệ Thống Slapstick/Fun Combat (GDD v5.1).
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

        [MenuItem("ProjectZombie/Weapons/Generate Slapstick Weapons & Relics SOs")]
        public static void GenerateFunCombatData()
        {
            string weaponFolder = "Assets/_Data/Weapons";
            string upgradeFolder = "Assets/_Data/Upgrades";

            if (!AssetDatabase.IsValidFolder("Assets/_Data")) AssetDatabase.CreateFolder("Assets", "_Data");
            if (!AssetDatabase.IsValidFolder(weaponFolder)) AssetDatabase.CreateFolder("Assets/_Data", "Weapons");
            if (!AssetDatabase.IsValidFolder(upgradeFolder)) AssetDatabase.CreateFolder("Assets/_Data", "Upgrades");

            FunWeaponDef[] funItems = new FunWeaponDef[]
            {
                new FunWeaponDef("W_SLIPPER", "Dép Tổ Ong Thần Sa", ElementType.Kim, 25f, 1.0f, "Vũ khí ném Boomerang hài hước; Hit 3 quăng lốc dép gây hiệu ứng 'Quê Độ' khiến quái quay sang đấm đồng minh.", WeaponRole.PrimaryWeapon),
                new FunWeaponDef("W_POT", "Nồi Cơm Thạch Sanh", ElementType.Tho, 35f, 1.4f, "Vũ khí cận chiến gom tối đa 3 quái vào nồi và phóng ra như đạn pháo; chạm đất rơi cơm nắm hồi máu.", WeaponRole.PrimaryWeapon),
                new FunWeaponDef("W_PIPE", "Điếu Cày Cửu U", ElementType.Hoa, 20f, 1.1f, "Vũ khí phun luồng khói dày đặc gây hiệu ứng 'Say Thuốc Lào' khiến quái đi giật lùi và nổ sát thương ho sặc sụa.", WeaponRole.PrimaryWeapon),
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

                so.weaponId = item.id;
                so.weaponName = item.name;
                so.elementType = item.element;
                so.baseDamage = item.damage;
                so.baseAttackSpeed = item.cooldown;
                so.description = item.desc;
                so.weaponRole = item.role;

                EditorUtility.SetDirty(so);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[FunCombatDataGenerator] Đã tạo thành công {funItems.Length} ScriptableObjects Vũ Khí & Relics Slapstick!");
        }
    }
}
#endif
