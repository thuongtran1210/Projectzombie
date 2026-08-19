#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using ProjectZombie.Features.Projectiles.Data;
using System.IO;

namespace ProjectZombie.Features.Projectiles.Editor
{
    /// <summary>
    /// Editor Script giúp tự động tạo 12 ScriptableObjects ProjectileData chuẩn cho 12 vũ khí MVP.
    /// Lưu vào: Assets/_Data/Projectiles/Data/
    /// Menu: ProjectZombie > Projectiles > Generate 12 ProjectileData SOs
    /// </summary>
    public static class ProjectileDataGenerator
    {
        private struct ProjDef
        {
            public string id;
            public string name;
            public float speed;
            public float lifetime;
            public float radius;
            public float damage;

            public ProjDef(string id, string name, float speed, float lifetime, float radius, float damage)
            {
                this.id = id;
                this.name = name;
                this.speed = speed;
                this.lifetime = lifetime;
                this.radius = radius;
                this.damage = damage;
            }
        }

        [MenuItem("ProjectZombie/Projectiles/Generate 12 ProjectileData SOs")]
        public static void GenerateAllProjectiles()
        {
            string folderPath = "Assets/_Data/Projectiles/Data";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                AssetDatabase.Refresh();
            }

            ProjDef[] projectiles = new ProjDef[]
            {
                new ProjDef("Proj_W001", "Nỏ Thần An Dương Vương (Mũi Tên Đồng)", 18f, 2.5f, 0.4f, 25f),
                new ProjDef("Proj_W002", "Bút Phán Quan (Vệt Mực Thư Pháp)", 0f, 0.2f, 1.8f, 30f),
                new ProjDef("Proj_W003", "Bùa Trấn Yêu (Bùa Bay Quanh)", 12f, 10.0f, 0.5f, 15f),
                new ProjDef("Proj_W004", "Cửu Vĩ Hồ Trảo (Huyết Trảo Cào Xé)", 14f, 0.35f, 0.5f, 18f),
                new ProjDef("Proj_W005", "Trống Đồng Đông Sơn (Sóng Âm Tỏa Rộng)", 7.5f, 0.7f, 0.45f, 9f),
                new ProjDef("Proj_W006", "Lựu Đạn Thần Sa (Hạt Thần Sa Bão Lửa)", 9f, 2.5f, 0.25f, 45f),
                new ProjDef("Proj_W007", "Cung Thạch Sanh (Mũi Tên Thần Lực)", 16f, 3.5f, 0.4f, 35f),
                new ProjDef("Proj_W008", "Đao Cửu Vĩ (Luồng Phun Rồng Lửa)", 6.5f, 0.45f, 0.35f, 8f),
                new ProjDef("Proj_W009", "Trượng Long Vương (Sét Nước Thủy Cung)", 15f, 2.0f, 0.6f, 25f),
                new ProjDef("Proj_W010", "Linh Phù Ma Da (Bãi Độc Thủy Cung)", 6f, 4.0f, 2.5f, 10f),
                new ProjDef("Proj_W011", "Nước Thánh Chùa Hương (Giếng Thiêng)", 0f, 5.0f, 3.0f, 14f),
                new ProjDef("Proj_W012", "Phi Tiêu Bát Quái (Phi Tiêu Cửu Cung)", 10f, 3.0f, 0.5f, 22f)
            };

            foreach (var def in projectiles)
            {
                CreateOrUpdateProjectileAsset(folderPath, def);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ProjectileDataGenerator] Đã tạo/cập nhật thành công 12 ProjectileData SOs trong {folderPath}");
        }

        private static void CreateOrUpdateProjectileAsset(string folderPath, ProjDef def)
        {
            string assetPath = $"{folderPath}/{def.id}_{def.name.Split(' ')[0]}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<ProjectileData>(assetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<ProjectileData>();
                AssetDatabase.CreateAsset(asset, assetPath);
            }

            int enemyMask = LayerMask.GetMask("Enemy");
            if (enemyMask == 0) enemyMask = LayerMask.GetMask("Default", "Enemy");

            SerializedObject so = new SerializedObject(asset);
            so.FindProperty("ProjectileID").stringValue = def.id;
            so.FindProperty("Speed").floatValue = def.speed;
            so.FindProperty("Lifetime").floatValue = def.lifetime;
            so.FindProperty("CollisionRadius").floatValue = def.radius;
            so.FindProperty("BaseDamage").floatValue = def.damage;
            so.FindProperty("HitLayer").intValue = enemyMask;
            so.FindProperty("PrewarmCount").intValue = 10;
            so.FindProperty("MaxPoolSize").intValue = 50;
            so.ApplyModifiedProperties();
        }
    }
}
#endif
