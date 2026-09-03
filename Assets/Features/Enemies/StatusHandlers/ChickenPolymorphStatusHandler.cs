using UnityEngine;

namespace ProjectZombie.Features.Enemies.StatusHandlers
{
    /// <summary>
    /// Strategy xử lý hiệu ứng Hóa Gà (Chicken Polymorph):
    /// - Quái vật bị biến thành Gà con nhỏ nhắn, mất khả năng tấn công và hoảng loạn chạy vòng quanh.
    /// - Quái vật nhận thêm +50% sát thương gia tăng khi đang ở trạng thái Gà.
    /// </summary>
    public class ChickenPolymorphStatusHandler : IStatusEffectHandler
    {
        public StatusEffectType Type => StatusEffectType.ChickenPolymorph;

        public void OnApplied(Enemy enemy, ActiveStatusEffect effectData)
        {
            if (enemy == null) return;

            // Thu nhỏ kích thước dạng Gà con chibi dựa trên InitialLocalScale chuẩn xác
            Vector3 baseScale = enemy.InitialLocalScale.sqrMagnitude > 0.001f ? enemy.InitialLocalScale : Vector3.one;
            enemy.transform.localScale = baseScale * 0.55f;

            // Đổi màu lông vàng gà con
            var sr = enemy.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = new Color(1f, 0.95f, 0.4f, 1f);
            }
        }

        public void OnTick(Enemy enemy, ActiveStatusEffect effectData, float deltaTime)
        {
            if (enemy == null) return;

            // Quái hoảng loạn đi loạng choạng theo hướng ngẫu nhiên (Panic Roam)
            Vector2 panicDir = new Vector2(Mathf.Sin(Time.time * 6f), Mathf.Cos(Time.time * 5f)).normalized;
            enemy.transform.position += (Vector3)(panicDir * (1.8f * deltaTime));
        }

        public void OnExpired(Enemy enemy, ActiveStatusEffect effectData)
        {
            RestoreEnemy(enemy);
        }

        public void OnRemoved(Enemy enemy, ActiveStatusEffect effectData)
        {
            RestoreEnemy(enemy);
        }

        private void RestoreEnemy(Enemy enemy)
        {
            if (enemy == null) return;

            // Khôi phục kích thước ban đầu chuẩn xác 100%
            Vector3 baseScale = enemy.InitialLocalScale.sqrMagnitude > 0.001f ? enemy.InitialLocalScale : Vector3.one;
            enemy.transform.localScale = baseScale;

            var sr = enemy.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = Color.white;
            }
        }
    }
}
