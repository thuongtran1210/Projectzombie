#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Weapons;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Weapons.Editor
{
    /// <summary>
    /// Editor Tool tự động cập nhật thuộc tính Ngũ Hành (ElementType) trên 12 Prefabs vũ khí 
    /// trong Assets/_Prefabs/Weapons/ bám sát 100% GDD v4.0.
    /// Menu: ProjectZombie > Weapons > Fix All 12 Weapon Prefabs Elements
    /// </summary>
    public static class WeaponPrefabElementUpdater
    {
        private struct WeaponElementInfo
        {
            public string id;
            public ElementType element;

            public WeaponElementInfo(string id, ElementType element)
            {
                this.id = id;
                this.element = element;
            }
        }

        [MenuItem("ProjectZombie/Weapons/Fix All 12 Weapon Prefabs Elements")]
        public static void FixAllWeaponPrefabsElements()
        {
            WeaponElementInfo[] elements = new WeaponElementInfo[]
            {
                new WeaponElementInfo("W001", ElementType.Kim), // Nỏ Thần (Kim)
                new WeaponElementInfo("W002", ElementType.Kim), // Bút Phán Quan (Kim)
                new WeaponElementInfo("W003", ElementType.Moc), // Bùa Trấn Yêu (Mộc)
                new WeaponElementInfo("W004", ElementType.Hoa), // Cửu Vĩ Hồ Trảo (Hỏa)
                new WeaponElementInfo("W005", ElementType.Tho), // Trống Đồng Đông Sơn (Thổ)
                new WeaponElementInfo("W006", ElementType.Hoa), // Lựu Đạn Thần Sa (Hỏa)
                new WeaponElementInfo("W007", ElementType.Kim), // Cung Thạch Sanh (Kim)
                new WeaponElementInfo("W008", ElementType.Hoa), // Đao Cửu Vĩ (Hỏa)
                new WeaponElementInfo("W009", ElementType.Thuy), // Trượng Long Vương (Thủy)
                new WeaponElementInfo("W010", ElementType.Thuy), // Linh Phù Ma Da (Thủy)
                new WeaponElementInfo("W011", ElementType.Tho), // Nước Thánh Chùa Hương (Thổ)
                new WeaponElementInfo("W012", ElementType.Moc)  // Phi Tiêu Bát Quái (Mộc)
            };

            int successCount = 0;
            foreach (var info in elements)
            {
                string[] guids = AssetDatabase.FindAssets($"Weapon_{info.id}_ t:Prefab", new string[] { "Assets/_Prefabs/Weapons" });
                if (guids.Length > 0)
                {
                    string prefabPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
                    {
                        GameObject root = scope.prefabContentsRoot;
                        WeaponBase weaponComp = root.GetComponent<WeaponBase>();
                        if (weaponComp != null)
                        {
                            weaponComp.weaponId = info.id;
                            weaponComp.element = info.element;
                            successCount++;
                            Debug.Log($"[WeaponPrefabElementUpdater] Đã cập nhật Prefab '{prefabPath}': WeaponId = {info.id}, Element = {info.element}");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[WeaponPrefabElementUpdater] Không tìm thấy Prefab cho ID: {info.id}");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[WeaponPrefabElementUpdater] Đã cập nhật thành công thuộc tính Ngũ Hành cho {successCount}/12 Weapon Prefabs!");
        }
    }
}
#endif
