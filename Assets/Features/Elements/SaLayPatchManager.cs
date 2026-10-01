using ProjectZombie.Features.Enemies;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Shared.VFX;
using UnityEngine;

namespace ProjectZombie.Features.Elements
{
    /// <summary>
    /// Quản lý các vũng bùn Sa Lầy (Thổ → Thủy) độc lập, không sinh rác bộ nhớ (0 GC Allocations).
    /// Áp dụng làm chậm (Slow 35%) cho mọi kẻ địch đứng trong vùng, tự động giải phóng khi hết 2 giây.
    /// </summary>
    public sealed class SaLayPatchManager : MonoBehaviour
    {
        private static SaLayPatchManager _instance;
        private SaLayReactionSettings _settings;

        private struct MudPatchState
        {
            public bool Active;
            public Vector2 Center;
            public float Radius;
            public float ExpiresAt;
        }

        private MudPatchState[] _patches;
        private Collider2D[] _overlapBuffer;

        public static void CreatePatch(Vector2 position)
        {
            Instance.SpawnMudPatch(position);
        }

        public static SaLayPatchManager Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = FindObjectOfType<SaLayPatchManager>();
                if (_instance == null)
                {
                    var go = new GameObject("[SaLayPatchManager]");
                    _instance = go.AddComponent<SaLayPatchManager>();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            _settings = SaLayReactionSettings.Runtime;
            int capacity = _settings != null ? _settings.patchCapacity : 8;
            int bufferSize = _settings != null ? _settings.colliderBufferSize : 24;

            _patches = new MudPatchState[capacity];
            _overlapBuffer = new Collider2D[bufferSize];
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void SpawnMudPatch(Vector2 position)
        {
            if (_patches == null) return;
            float now = Time.time;
            float duration = _settings != null ? _settings.mudDuration : 2.0f;
            float radius = _settings != null ? _settings.mudRadius : 1.4f;

            int bestSlot = -1;
            float earliestExpiry = float.MaxValue;

            for (int i = 0; i < _patches.Length; i++)
            {
                if (!_patches[i].Active || _patches[i].ExpiresAt <= now)
                {
                    bestSlot = i;
                    break;
                }
                if (_patches[i].ExpiresAt < earliestExpiry)
                {
                    earliestExpiry = _patches[i].ExpiresAt;
                    bestSlot = i;
                }
            }

            if (bestSlot < 0) return;

            _patches[bestSlot] = new MudPatchState
            {
                Active = true,
                Center = position,
                Radius = radius,
                ExpiresAt = now + duration
            };

            global::ProjectZombie.Core.Audio.AudioService.Current?.PlayElementalReaction(position);

            if (_settings != null && _settings.mudVisualPrefab != null && GlobalVFXPoolManager.Instance != null)
            {
                GlobalVFXPoolManager.Instance.PlayEffect(_settings.mudVisualPrefab, position, Quaternion.identity, duration);
            }
        }

        private void Update()
        {
            if (_patches == null || _overlapBuffer == null) return;
            float now = Time.time;
            float slow = _settings != null ? _settings.slowAmount : 0.35f;

            for (int p = 0; p < _patches.Length; p++)
            {
                if (!_patches[p].Active) continue;
                if (_patches[p].ExpiresAt <= now)
                {
                    _patches[p].Active = false;
                    continue;
                }

                int hitCount = Physics2D.OverlapCircleNonAlloc(
                    _patches[p].Center,
                    _patches[p].Radius,
                    _overlapBuffer,
                    TargetingUtility.EnemyLayerMask);

                for (int i = 0; i < hitCount; i++)
                {
                    var col = _overlapBuffer[i];
                    if (col != null && col.TryGetComponent<Enemy>(out var enemy) && enemy.HealthSystem != null && enemy.HealthSystem.IsAlive)
                    {
                        // Refresh 0.4s Slow liên tục khi đứng trong bùn. Khi rời vùng bùn, Slow sẽ tự biến mất sau 0.4s mà không làm ảnh hưởng các nguồn Slow khác
                        enemy.ApplyStatusEffect(StatusEffectType.Slow, 0.4f, slow);
                    }
                    _overlapBuffer[i] = null;
                }
            }
        }
    }
}
