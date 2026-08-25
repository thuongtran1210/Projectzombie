#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectZombie.Core.ScriptableObjects;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Enemies.Editor
{
    /// <summary>
    /// Editor Tool tự động tạo và cập nhật 5 ScriptableObject Yêu Ma MVP theo GDD v4.0.
    /// Lưu trữ tại: Assets/_Data/Enemies/
    /// Menu: ProjectZombie > Enemies > Generate 5 MVP Enemy SOs
    /// </summary>
    public static class EnemyDataGenerator
    {
        private struct EnemyDef
        {
            public string id;
            public string name;
            public ElementType element;
            public float hp;
            public float speed;
            public float damage;
            public int exp;
            public EnemyTier tier;
            public bool isHeavyArmor;

            public EnemyDef(string id, string name, ElementType element, float hp, float speed, float damage, int exp, EnemyTier tier, bool isHeavyArmor = false)
            {
                this.id = id;
                this.name = name;
                this.element = element;
                this.hp = hp;
                this.speed = speed;
                this.damage = damage;
                this.exp = exp;
                this.tier = tier;
                this.isHeavyArmor = isHeavyArmor;
            }
        }

        [MenuItem("ProjectZombie/Enemies/Generate 5 MVP Enemy SOs")]
        public static void GenerateEnemies()
        {
            string folderPath = "Assets/_Data/Enemies";

            if (!AssetDatabase.IsValidFolder("Assets/_Data"))
            {
                AssetDatabase.CreateFolder("Assets", "_Data");
            }
            if (!AssetDatabase.IsValidFolder("Assets/_Data/Enemies"))
            {
                AssetDatabase.CreateFolder("Assets/_Data", "Enemies");
            }

            EnemyDef[] enemies = new EnemyDef[]
            {
                new EnemyDef("E_MAGIAP", "Ma Giáp", ElementType.Kim, 100f, 2.5f, 10f, 1, EnemyTier.Common),
                new EnemyDef("E_MATROI", "Ma Trơi", ElementType.Hoa, 65f, 4.0f, 8f, 2, EnemyTier.Common),
                new EnemyDef("E_QUYNHAPTRANG", "Quỷ Nhập Tràng", ElementType.Tho, 350f, 1.5f, 20f, 5, EnemyTier.Elite, isHeavyArmor: true),
                new EnemyDef("E_MADA", "Ma Da", ElementType.Thuy, 90f, 2.0f, 12f, 3, EnemyTier.Common),
                new EnemyDef("E_HOALYTINH", "Hồ Ly Tinh Nhỏ", ElementType.Hoa, 75f, 3.5f, 50f, 4, EnemyTier.Common),
                new EnemyDef("E_NGUUDAUMADIEN", "Ngưu Đầu Mã Diện", ElementType.Tho, 5000f, 2.2f, 35f, 100, EnemyTier.Boss),
                new EnemyDef("E_DIEMVUONG", "Diêm Vương", ElementType.Kim, 15000f, 1.8f, 50f, 500, EnemyTier.Boss)
            };

            foreach (var def in enemies)
            {
                CreateOrUpdateEnemyAsset(folderPath, def);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[EnemyDataGenerator] Đã tạo/cập nhật thành công 5 Yêu Ma MVP trong {folderPath}");
        }

        private static void CreateOrUpdateEnemyAsset(string folderPath, EnemyDef def)
        {
            string assetPath = $"{folderPath}/{def.id}_{def.name.Replace(" ", "")}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<EnemyConfig>(assetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<EnemyConfig>();
                AssetDatabase.CreateAsset(asset, assetPath);
            }

            SerializedObject so = new SerializedObject(asset);
            so.FindProperty("moveSpeed").floatValue = def.speed;
            so.FindProperty("maxHealth").floatValue = def.hp;
            so.FindProperty("damageToPlayer").floatValue = def.damage;
            so.FindProperty("expReward").intValue = def.exp;
            so.FindProperty("tier").enumValueIndex = (int)def.tier;
            so.FindProperty("elementType").enumValueIndex = (int)def.element;
            
            var heavyProp = so.FindProperty("isHeavyArmor");
            if (heavyProp != null) heavyProp.boolValue = def.isHeavyArmor;

            so.ApplyModifiedProperties();
        }
    }
}
#endif
