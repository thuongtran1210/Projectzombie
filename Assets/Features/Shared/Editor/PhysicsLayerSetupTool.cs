using UnityEditor;
using UnityEngine;

namespace ProjectZombie.Features.Shared.Editor
{
    /// <summary>
    /// Editor Tool tự động tạo Layers và cấu hình Physics2D Collision Matrix 
    /// chuẩn xác theo thiết lập trong PHYSICS_LAYER_DESIGN_SPEC.md.
    /// </summary>
    public static class PhysicsLayerSetupTool
    {
        private const int LAYER_OBSTACLE = 3;
        private const int LAYER_PLAYER = 6;
        private const int LAYER_ENEMY = 7;
        private const int LAYER_PLAYER_PROJECTILE = 8;
        private const int LAYER_ENEMY_PROJECTILE = 9;
        private const int LAYER_PICKUP = 10;
        private const int LAYER_PLAYER_HITBOX = 11;

        [MenuItem("ProjectZombie/Physics 2D/Setup Layers & Collision Matrix")]
        public static void SetupLayersAndCollisionMatrix()
        {
            SetupTagManagerLayers();
            SetupCollisionMatrix();

            Debug.Log("[PhysicsLayerSetupTool] ✅ Đã khởi tạo và cấu hình thành công Physics 2D Layers & Collision Matrix theo PHYSICS_LAYER_DESIGN_SPEC.md!");
        }

        private static void SetupTagManagerLayers()
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layers = tagManager.FindProperty("layers");

            if (layers == null || !layers.isArray)
            {
                Debug.LogError("[PhysicsLayerSetupTool] Không thể truy cập thuộc tính layers trong TagManager.asset");
                return;
            }

            SetLayerName(layers, LAYER_OBSTACLE, "Obstacle");
            SetLayerName(layers, LAYER_PLAYER, "Player");
            SetLayerName(layers, LAYER_ENEMY, "Enemy");
            SetLayerName(layers, LAYER_PLAYER_PROJECTILE, "PlayerProjectile");
            SetLayerName(layers, LAYER_ENEMY_PROJECTILE, "EnemyProjectile");
            SetLayerName(layers, LAYER_PICKUP, "Pickup");
            SetLayerName(layers, LAYER_PLAYER_HITBOX, "PlayerHitbox");

            tagManager.ApplyModifiedProperties();
        }

        private static void SetLayerName(SerializedProperty layers, int index, string name)
        {
            if (index < layers.arraySize)
            {
                SerializedProperty element = layers.GetArrayElementAtIndex(index);
                if (element != null)
                {
                    element.stringValue = name;
                }
            }
        }

        private static void SetupCollisionMatrix()
        {
            // Reset / Cấu hình ma trận va chạm chuẩn xác (Ignore Layer Collision)

            // 1. Tắt va chạm Quái - Quái (Tối ưu hiệu năng 200+ quái)
            Physics2D.IgnoreLayerCollision(LAYER_ENEMY, LAYER_ENEMY, true);

            // 2. Tắt va chạm Đạn Player - Đạn Player & Đạn Player - Player
            Physics2D.IgnoreLayerCollision(LAYER_PLAYER_PROJECTILE, LAYER_PLAYER_PROJECTILE, true);
            Physics2D.IgnoreLayerCollision(LAYER_PLAYER_PROJECTILE, LAYER_PLAYER, true);
            Physics2D.IgnoreLayerCollision(LAYER_PLAYER_PROJECTILE, LAYER_ENEMY_PROJECTILE, true);
            Physics2D.IgnoreLayerCollision(LAYER_PLAYER_PROJECTILE, LAYER_PICKUP, true);

            // 3. Tắt va chạm Đạn Enemy - Đạn Enemy & Đạn Enemy - Enemy & Đạn Enemy - Pickup
            Physics2D.IgnoreLayerCollision(LAYER_ENEMY_PROJECTILE, LAYER_ENEMY_PROJECTILE, true);
            Physics2D.IgnoreLayerCollision(LAYER_ENEMY_PROJECTILE, LAYER_ENEMY, true);
            Physics2D.IgnoreLayerCollision(LAYER_ENEMY_PROJECTILE, LAYER_PICKUP, true);

            // 4. Tắt va chạm Vật phẩm (Pickup) - Enemy / Obstacle / Pickup
            Physics2D.IgnoreLayerCollision(LAYER_PICKUP, LAYER_ENEMY, true);
            Physics2D.IgnoreLayerCollision(LAYER_PICKUP, LAYER_OBSTACLE, true);
            Physics2D.IgnoreLayerCollision(LAYER_PICKUP, LAYER_PICKUP, true);

            // 5. Bật các va chạm cần thiết (Enable Collisions)
            Physics2D.IgnoreLayerCollision(LAYER_PLAYER, LAYER_ENEMY, false);
            Physics2D.IgnoreLayerCollision(LAYER_PLAYER, LAYER_OBSTACLE, false);
            Physics2D.IgnoreLayerCollision(LAYER_PLAYER, LAYER_ENEMY_PROJECTILE, false);
            Physics2D.IgnoreLayerCollision(LAYER_PLAYER, LAYER_PICKUP, false);

            Physics2D.IgnoreLayerCollision(LAYER_ENEMY, LAYER_OBSTACLE, false);
            Physics2D.IgnoreLayerCollision(LAYER_ENEMY, LAYER_PLAYER_PROJECTILE, false);

            Physics2D.IgnoreLayerCollision(LAYER_PLAYER_PROJECTILE, LAYER_OBSTACLE, false);
            Physics2D.IgnoreLayerCollision(LAYER_ENEMY_PROJECTILE, LAYER_OBSTACLE, false);

            Physics2D.IgnoreLayerCollision(LAYER_PLAYER_HITBOX, LAYER_ENEMY, false);
            Physics2D.IgnoreLayerCollision(LAYER_PLAYER_HITBOX, LAYER_ENEMY_PROJECTILE, false);
        }
    }
}
