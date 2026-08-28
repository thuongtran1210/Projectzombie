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
            public bool isPassive;
            public float activeCd;
            public float activeDur;
            public string skillName;

            public WeaponDef(string id, string name, ElementType element, string rarity, float damage, float cooldown, string evoId, string desc, WeaponRole role, bool isPassive, float activeCd, float activeDur, string skillName)
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
                this.isPassive = isPassive;
                this.activeCd = activeCd;
                this.activeDur = activeDur;
                this.skillName = skillName;
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
                new WeaponDef("W001", "Nỏ Thần", ElementType.Kim, "Common", 12f, 0.6f, "E001", "Kỹ năng chủ động: Bắn chùm 5 linh tiễn thần uy xuyên quái và đẩy lùi mạnh.", WeaponRole.RelicOnHitTrigger, false, 6.0f, 0f, "Vạn Tiễn Phá Trận"),
                new WeaponDef("W002", "Bút Phán Quan", ElementType.Kim, "Common", 20f, 0.8f, "E002", "Pháp bảo bị động: Tự động vung nhát chém phán quyết âm ty khi Hero đánh trúng quái.", WeaponRole.RelicOnHitTrigger, true, 0f, 0f, ""),
                new WeaponDef("W003", "Bùa Trấn Yêu", ElementType.Moc, "Rare", 8f, 0.4f, "E003", "Pháp bảo bị động: Vòng lá bùa thần xoay quanh cản đạn và đẩy lùi yêu ma.", WeaponRole.RelicOrbitalShield, true, 0f, 0f, ""),
                new WeaponDef("W004", "Cửu Vĩ Hồ Trảo", ElementType.Hoa, "Rare", 18f, 1.2f, "E004", "Pháp bảo bị động: Móng vuốt cáo lửa tự cào xé quái và hút sinh khí hồi phục.", WeaponRole.RelicOnHitTrigger, true, 0f, 0f, ""),
                new WeaponDef("W005", "Trống Đồng Đông Sơn", ElementType.Tho, "Common", 40f, 1.5f, "E005", "Kỹ năng chủ động: Dậm sóng âm 360 độ cực đại gây choáng cứng 1.5s và đẩy lùi quái.", WeaponRole.RelicOrbitalShield, false, 10.0f, 0f, "Thần Âm Trảm Linh"),
                new WeaponDef("W006", "Lựu Đạn Thần Sa", ElementType.Hoa, "Epic", 45f, 2.5f, "E006", "Kỹ năng chủ động: Quăng chùm 3 hạt Thần Sa nổ tung bão lửa thiêu rụi vùng rộng.", WeaponRole.RelicOnHitTrigger, false, 8.0f, 0f, "Bão Lửa Thần Sa"),
                new WeaponDef("W007", "Cung Thạch Sanh", ElementType.Kim, "Rare", 35f, 1.0f, "E007", "Pháp bảo bị động: Tự động bắn mũi tên thần lực Thạch Sanh xuyên qua hàng loạt yêu tinh.", WeaponRole.RelicOnHitTrigger, true, 0f, 0f, ""),
                new WeaponDef("W008", "Đao Cửu Vĩ", ElementType.Hoa, "Rare", 8f, 0.25f, "E008", "Kỹ năng chủ động: Bộc phát thần uy trong 5s, liên tục vung trảm hỏa long 8 hướng.", WeaponRole.RelicSupportAura, false, 12.0f, 5.0f, "Hỏa Long Bộc Phát"),
                new WeaponDef("W009", "Trượng Long Vương", ElementType.Thuy, "Epic", 25f, 1.8f, "E009", "Pháp bảo bị động: Tự động giáng sét nước thủy cung lan truyền qua chuỗi 6 yêu quái.", WeaponRole.RelicSupportAura, true, 0f, 0f, ""),
                new WeaponDef("W010", "Linh Phù Ma Da", ElementType.Thuy, "Rare", 10f, 2.0f, "E010", "Pháp bảo bị động: Triệu hồi linh thú Ma Da bơi theo phun độc làm chậm liên tục.", WeaponRole.RelicSupportAura, true, 0f, 0f, ""),
                new WeaponDef("W011", "Nước Thánh Chùa Hương", ElementType.Tho, "Rare", 14f, 3.0f, "E011", "Kỹ năng chủ động: Tạo trận pháp 3 giếng thiêng phong tỏa và hồi 10% Max HP.", WeaponRole.RelicSupportAura, false, 15.0f, 6.0f, "Trận Pháp Giếng Thiêng"),
                new WeaponDef("W012", "Phi Tiêu Bát Quái", ElementType.Moc, "Common", 22f, 1.4f, "E012", "Pháp bảo bị động: Phi tiêu ma thuật tự động xoay tròn quét kẻ địch và quy hồi.", WeaponRole.RelicOnHitTrigger, true, 0f, 0f, "")
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
            so.FindProperty("isPassiveRelic").boolValue = def.isPassive;
            so.FindProperty("activeCooldown").floatValue = def.activeCd;
            so.FindProperty("activeDuration").floatValue = def.activeDur;
            so.FindProperty("skillActionName").stringValue = def.skillName;
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
