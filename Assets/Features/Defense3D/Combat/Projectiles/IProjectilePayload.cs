using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    public enum DefensePayloadType
    {
        SingleTarget = 0,   // Gây sát thương đơn mục tiêu khi chạm
        AoEExplosion = 1,   // Kích hoạt vụ nổ bán kính khi chạm đích (Pháo cối, Cầu lửa)
        Piercing = 2        // Xuyên qua nhiều mục tiêu trên đường bay
    }

    /// <summary>
    /// Chiến lược xử lý va chạm và gây sát thương khi đạn tiếp đất hoặc trúng đích (Strategy Pattern).
    /// Tuân thủ quy chuẩn 60 FPS Mobile: 0 GC Allocations, sử dụng Physics NonAlloc buffer cố định.
    /// </summary>
    public interface IProjectilePayload
    {
        DefensePayloadType Type { get; }

        bool Execute(
            Vector3 impactPoint,
            IDefenseEnemyTarget3D directTarget,
            float damage,
            Component source,
            int generation,
            object session,
            IDefenseGameplayContext gameplay,
            float aoeRadius = 0f);
    }

    /// <summary>
    /// Gây sát thương tập trung lên 1 mục tiêu chỉ định (Tháp bắn tên, đòn đánh thường).
    /// </summary>
    public sealed class SingleTargetDamagePayload : IProjectilePayload
    {
        public static readonly SingleTargetDamagePayload Instance = new SingleTargetDamagePayload();

        public DefensePayloadType Type => DefensePayloadType.SingleTarget;

        public bool Execute(
            Vector3 impactPoint,
            IDefenseEnemyTarget3D directTarget,
            float damage,
            Component source,
            int generation,
            object session,
            IDefenseGameplayContext gameplay,
            float aoeRadius = 0f)
        {
            if (directTarget == null || directTarget.TargetTransform == null) return false;

            var context = new DefenseDamageContext(
                amount: damage,
                source: source,
                hitPoint: impactPoint,
                runGeneration: generation,
                sessionIdentity: session);

            return directTarget.TakeDamage(context);
        }
    }

    /// <summary>
    /// Gây sát thương nổ diện rộng (AoE) xung quanh điểm va chạm.
    /// Sử dụng Physics.OverlapSphereNonAlloc triệt tiêu 100% rác bộ nhớ (0 GC).
    /// </summary>
    public sealed class AoEExplosionPayload : IProjectilePayload
    {
        public static readonly AoEExplosionPayload Instance = new AoEExplosionPayload();

        private static readonly Collider[] _hitBuffer = new Collider[64];
        private static readonly LayerMask _enemyLayerMask = LayerMask.GetMask("Enemy", "Default");

        public DefensePayloadType Type => DefensePayloadType.AoEExplosion;

        public bool Execute(
            Vector3 impactPoint,
            IDefenseEnemyTarget3D directTarget,
            float damage,
            Component source,
            int generation,
            object session,
            IDefenseGameplayContext gameplay,
            float aoeRadius = 0f)
        {
            float radius = Mathf.Max(0.5f, aoeRadius);
            int hitCount = Physics.OverlapSphereNonAlloc(impactPoint, radius, _hitBuffer, _enemyLayerMask);
            bool anyHit = false;

            var context = new DefenseDamageContext(
                amount: damage,
                source: source,
                hitPoint: impactPoint,
                runGeneration: generation,
                sessionIdentity: session);

            for (int i = 0; i < hitCount; i++)
            {
                var col = _hitBuffer[i];
                if (col == null) continue;

                if (col.TryGetComponent<IDefenseDamageable3D>(out var damageable))
                {
                    if (damageable.TakeDamage(context))
                    {
                        anyHit = true;
                    }
                }
                _hitBuffer[i] = null; // Clear buffer reference
            }

            // Fallback nếu mục tiêu trực tiếp không nằm trong buffer do layer/collider
            if (!anyHit && directTarget != null)
            {
                anyHit = directTarget.TakeDamage(context);
            }

            return anyHit;
        }
    }

    /// <summary>
    /// Gây sát thương xuyên mục tiêu khi bay qua quái vật.
    /// </summary>
    public sealed class PiercingDamagePayload : IProjectilePayload
    {
        public static readonly PiercingDamagePayload Instance = new PiercingDamagePayload();

        public DefensePayloadType Type => DefensePayloadType.Piercing;

        public bool Execute(
            Vector3 impactPoint,
            IDefenseEnemyTarget3D directTarget,
            float damage,
            Component source,
            int generation,
            object session,
            IDefenseGameplayContext gameplay,
            float aoeRadius = 0f)
        {
            if (directTarget == null || directTarget.TargetTransform == null) return false;

            var context = new DefenseDamageContext(
                amount: damage,
                source: source,
                hitPoint: impactPoint,
                runGeneration: generation,
                sessionIdentity: session);

            return directTarget.TakeDamage(context);
        }
    }
}
