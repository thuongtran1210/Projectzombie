using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Combat;
using ProjectZombie.Features.Combat.Events;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Upgrades.Runtimes.Augments
{
    /// <summary>
    /// Runtime cho Lõi [Liệt Hỏa Bộ Pháp] (AUG_GOLD_FIRE_TRAIL):
    /// Khi Dash để lại vệt lửa thiêu đốt quái vật đi qua.
    /// </summary>
    public class FireTrailAugmentRuntime : AugmentRuntimeBase
    {
        [Header("Settings")]
        [SerializeField] private float _fireDamage = 35f;
        [SerializeField] private float _trailDuration = 4f;
        [SerializeField] private float _damageInterval = 0.5f;
        [SerializeField] private float _hitRadius = 1.5f;
        [Header("VFX Settings")]
        [SerializeField] private GameObject _fireVfxPrefab;

        private static readonly Collider2D[] _hitBuffer = new Collider2D[24];

        private struct FireZone
        {
            public Vector2 Position;
            public float ExpiryTime;
            public float NextDamageTime;
        }

        private readonly List<FireZone> _activeZones = new List<FireZone>(16);

        protected override void SubscribeCombatEvents(PlayerCombatEvents events)
        {
            events.OnDashPerformed += HandleDash;
        }

        protected override void UnsubscribeCombatEvents(PlayerCombatEvents events)
        {
            events.OnDashPerformed -= HandleDash;
        }

        private void HandleDash(in DashEvent e)
        {
            // Tạo 3 điểm lửa dọc theo đường lướt
            Vector2 start = e.StartPosition;
            Vector2 end = e.EndPosition;
            float now = Time.time;

            for (int i = 0; i <= 3; i++)
            {
                float t = i / 3f;
                Vector2 pos = Vector2.Lerp(start, end, t);
                _activeZones.Add(new FireZone
                {
                    Position = pos,
                    ExpiryTime = now + _trailDuration,
                    NextDamageTime = now
                });

                if (_fireVfxPrefab != null && ProjectZombie.Features.Shared.VFX.GlobalVFXPoolManager.Instance != null)
                {
                    ProjectZombie.Features.Shared.VFX.GlobalVFXPoolManager.Instance.PlayEffect(_fireVfxPrefab, pos, Quaternion.identity, _trailDuration);
                }
            }
        }

        private void Update()
        {
            if (_activeZones.Count == 0) return;

            float now = Time.time;
            for (int i = _activeZones.Count - 1; i >= 0; i--)
            {
                var zone = _activeZones[i];
                if (now >= zone.ExpiryTime)
                {
                    _activeZones.RemoveAt(i);
                    continue;
                }

                if (now >= zone.NextDamageTime)
                {
                    zone.NextDamageTime = now + _damageInterval;
                    _activeZones[i] = zone;

                    // Gây sát thương vùng lửa
                    int hitCount = Physics2D.OverlapCircleNonAlloc(zone.Position, _hitRadius, _hitBuffer);
                    for (int h = 0; h < hitCount; h++)
                    {
                        var col = _hitBuffer[h];
                        if (col != null && col.CompareTag("Enemy") && col.TryGetComponent<IDamageable>(out var damageable))
                        {
                            damageable.TakeDamage(new DamageData(_fireDamage, isCritical: false, element: ElementType.Hoa));
                        }
                    }
                }
            }
        }
    }
}
