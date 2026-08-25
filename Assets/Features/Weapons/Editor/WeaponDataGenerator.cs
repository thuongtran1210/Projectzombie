#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons.Editor
{
    /// <summary>
    /// Editor Tool tự động tạo và cập nhật 12 ScriptableObject Pháp Bảo gốc + 12 Evolution theo GDD v4.0.
    /// Lưu trữ tại: Assets/_Data/Weapons/
    /// Menu: ProjectZombie > Weapons > Generate 12 MVP Weapon SOs
    /// </summary>
    public static class WeaponDataGenerator
    {
        private struct WeaponDef
        {
            public string id;
            public string name;
            public ElementType element;
            public string rarity;
            public float damage;
            public float cooldown;
            public string evoId;
            public string desc;
            public WeaponRole role;

            public WeaponDef(string id, string name, ElementType element, string rarity, float damage, float cooldown, string evoId, string desc, WeaponRole role = WeaponRole.PrimaryWeapon)
            {
                this.id = id;
                this.name = name;
                this.element = element;
                this.rarity = rarity;
                this.damage = damage;
                this.cooldown = cooldown;
                this.evoId = evoId;
                this.desc = desc;
                this.role = role;
            }
        }

        [MenuItem("ProjectZombie/Weapons/Generate 12 MVP Weapon SOs")]
        public static void GenerateWeapons()
        {
            string folderPath = "Assets/_Data/Weapons";

            if (!AssetDatabase.IsValidFolder("Assets/_Data"))
            {
                AssetDatabase.CreateFolder("Assets", "_Data");
            }
            if (!AssetDatabase.IsValidFolder("Assets/_Data/Weapons"))
            {
                AssetDatabase.CreateFolder("Assets/_Data", "Weapons");
            }

            WeaponDef[] weapons = new WeaponDef[]
            {
                new WeaponDef("W001", "Nỏ Thần", ElementType.Kim, "Common", 12f, 0.6f, "E001", "Mũi tên thần An Dương Vương bắn thẳng xuyên táo 2 kẻ địch.", WeaponRole.PrimaryWeapon),
                new WeaponDef("W002", "Bút Phán Quan", ElementType.Kim, "Common", 20f, 0.8f, "E002", "Nhát chém mang uy lực phán quyết âm ty gây sát thương chí mạng 2 bên.", WeaponRole.PrimaryWeapon),
                new WeaponDef("W003", "Bùa Trấn Yêu", ElementType.Moc, "Rare", 8f, 0.4f, "E003", "Vòng lá bùa thần xoay quanh bảo vệ và đẩy lùi yêu ma.", WeaponRole.RelicOrbitalShield),
                new WeaponDef("W004", "Cửu Vĩ Hồ Trảo", ElementType.Hoa, "Rare", 18f, 1.2f, "E004", "Móng vuốt cáo lửa tự tìm diệt quái và hút sinh khí.", WeaponRole.RelicOnHitTrigger),
                new WeaponDef("W005", "Trống Đồng Đông Sơn", ElementType.Tho, "Common", 40f, 1.5f, "E005", "Sóng âm trảm linh tỏa rộng 5 hướng gây choáng diện rộng.", WeaponRole.RelicOrbitalShield),
                new WeaponDef("W006", "Lựu Đạn Thần Sa", ElementType.Hoa, "Epic", 45f, 2.5f, "E006", "Hạt thần sa phát nổ tạo bão lửa thiêu rụi vùng rộng (Knockback mạnh).", WeaponRole.RelicOnHitTrigger),
                new WeaponDef("W007", "Cung Thạch Sanh", ElementType.Kim, "Rare", 35f, 1.0f, "E007", "Mũi tên thần lực bối cảnh Thạch Sanh xuyên qua hàng loạt yêu tinh.", WeaponRole.PrimaryWeapon),
                new WeaponDef("W008", "Đao Cửu Vĩ", ElementType.Hoa, "Rare", 8f, 0.25f, "E008", "Luồng rồng lửa thiêu đốt liên tục theo đường thẳng.", WeaponRole.PrimaryWeapon),
                new WeaponDef("W009", "Trượng Long Vương", ElementType.Thuy, "Epic", 25f, 1.8f, "E009", "Sét nước thủy cung lan truyền qua chuỗi 6 yêu quái (Choáng 0.5s).", WeaponRole.RelicSupportAura),
                new WeaponDef("W010", "Linh Phù Ma Da", ElementType.Thuy, "Rare", 10f, 2.0f, "E010", "Triệu hồi linh thú Ma Da phun độc sát thương liên tục.", WeaponRole.RelicSupportAura),
                new WeaponDef("W011", "Nước Thánh Chùa Hương", ElementType.Tho, "Rare", 14f, 3.0f, "E011", "Tạo bãi giếng thiêng trên mặt đất làm chậm và gây sát thương liên tục.", WeaponRole.RelicSupportAura),
                new WeaponDef("W012", "Phi Tiêu Bát Quái", ElementType.Moc, "Common", 22f, 1.4f, "E012", "Phi tiêu ma thuật xoay tròn và quay lại vị trí người chơi.", WeaponRole.PrimaryWeapon)
            };

            foreach (var def in weapons)
            {
                CreateOrUpdateWeaponAsset(folderPath, def);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[WeaponDataGenerator] Đã tạo/cập nhật thành công 12 Pháp Bảo MVP trong {folderPath}");
        }

        private static void CreateOrUpdateWeaponAsset(string folderPath, WeaponDef def)
        {
            string assetPath = $"{folderPath}/{def.id}_{def.name.Replace(" ", "")}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<WeaponData>(assetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<WeaponData>();
                AssetDatabase.CreateAsset(asset, assetPath);
            }

            // Tự động tìm Prefab tương ứng trong Assets/_Prefabs/Weapons/ theo ID (tránh lệch dấu tiếng Việt)
            WeaponBase weaponPrefab = null;
            string[] guids = AssetDatabase.FindAssets($"Weapon_{def.id}_ t:Prefab", new string[] { "Assets/_Prefabs/Weapons" });
            if (guids.Length > 0)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                var prefabObj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefabObj != null)
                {
                    weaponPrefab = prefabObj.GetComponent<WeaponBase>();
                }
            }

            SerializedObject so = new SerializedObject(asset);
            so.FindProperty("weaponRole").enumValueIndex = (int)def.role;
            so.FindProperty("weaponId").stringValue = def.id;
            so.FindProperty("weaponName").stringValue = def.name;
            so.FindProperty("description").stringValue = def.desc;
            so.FindProperty("rarity").stringValue = def.rarity;
            so.FindProperty("evolutionWeaponId").stringValue = def.evoId;
            so.FindProperty("elementType").enumValueIndex = (int)def.element;
            so.FindProperty("baseDamage").floatValue = def.damage;
            so.FindProperty("baseAttackSpeed").floatValue = def.cooldown;
            if (weaponPrefab != null)
            {
                so.FindProperty("weaponPrefab").objectReferenceValue = weaponPrefab;
                Debug.Log($"[WeaponDataGenerator] Đã tự động gán Prefab '{weaponPrefab.name}' vào WeaponData '{def.id}_{def.name}'");
            }
            else
            {
                Debug.LogWarning($"[WeaponDataGenerator] Không tìm thấy Prefab khớp với ID 'Weapon_{def.id}_' trong Assets/_Prefabs/Weapons/");
            }
            so.ApplyModifiedProperties();
        }
    }
}
#endif
