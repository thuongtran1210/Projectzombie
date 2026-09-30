using UnityEngine;
using Fusion;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Enemies;

namespace ProjectZombie.Features.Multiplayer.Core
{
    /// <summary>
    /// Đồng bộ hóa trạng thái quái vật theo cơ chế Host-Authoritative (Photon Fusion 2.0).
    /// Mọi tính toán AI, di chuyển và trừ máu đều do Host quyết định, Client gửi yêu cầu sát thương qua RPC.
    /// </summary>
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkEnemySync : NetworkBehaviour
    {
        [Networked]
        public float NetworkHealth { get; set; }

        [Networked]
        public NetworkBool NetworkIsDead { get; set; }

        private HealthSystem _healthSystem;
        private Enemy _enemy;
        private float _accumulatedDamageBuffer = 0f;

        private void Awake()
        {
            _healthSystem = GetComponent<HealthSystem>();
            _enemy = GetComponent<Enemy>();
        }

        public override void Spawned()
        {
            _accumulatedDamageBuffer = 0f;

            if (Runner.IsServer)
            {
                // Host khởi tạo máu mạng ban đầu
                if (_healthSystem != null)
                {
                    NetworkHealth = _healthSystem.CurrentHealth;
                    NetworkIsDead = false;
                    _healthSystem.OnHealthChanged += HandleHostHealthChanged;
                    _healthSystem.OnDied += HandleHostDied;
                }
            }
            else
            {
                // Client: Tắt Enemy FSM & Physics để nhường quyền điều phối chuyển động cho NetworkTransform từ Host
                if (_enemy != null) _enemy.enabled = false;
                if (TryGetComponent<Rigidbody2D>(out var rb))
                {
                    rb.isKinematic = true;
                    rb.velocity = Vector2.zero;
                }

                // Chuyển hướng mọi đòn đánh cục bộ của Client vào bộ đệm gom (Damage Batching)
                // để tránh spam hàng trăm gói tin RPC/giây khi 4 người cùng xả AOE
                if (_healthSystem != null)
                {
                    _healthSystem.CustomDamageInterceptor = (amount, data) =>
                    {
                        _accumulatedDamageBuffer += amount;
                        if (data.Element != ElementType.None) _lastElement = (byte)data.Element;
                        if (data.HitSource != ElementHitSource.Unknown) _lastHitSource = (byte)data.HitSource;
                        if (data.IsCritical) _lastIsCrit = true;
                        // Match local cooldown prediction to the owning attacker, even though
                        // health is updated later by the host's existing damage batch RPC.
                        DamageUtility.RegisterSuccessfulHit(data);
                        return true;
                    };
                }
            }
        }

        public override void FixedUpdateNetwork()
        {
            // Trên máy Client: Nếu có sát thương dồn tích lũy trong tick này, gửi 1 gói duy nhất lên Host
            if (!Runner.IsServer && _accumulatedDamageBuffer > 0f && Object.IsValid)
            {
                int attackerId = Runner.LocalPlayer.PlayerId;
                RpcApplyDamage(_accumulatedDamageBuffer, attackerId, _lastElement, _lastHitSource, _lastIsCrit);
                _accumulatedDamageBuffer = 0f;
                _lastElement = 0;
                _lastHitSource = 0;
                _lastIsCrit = false;
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (_healthSystem != null)
            {
                if (Runner.IsServer)
                {
                    _healthSystem.OnHealthChanged -= HandleHostHealthChanged;
                    _healthSystem.OnDied -= HandleHostDied;
                }
                else
                {
                    _healthSystem.CustomDamageInterceptor = null;
                }
            }
        }

        public override void Render()
        {
            // Client cập nhật hiển thị máu cục bộ theo dữ liệu mạng đã nội suy từ Host
            if (!Runner.IsServer && _healthSystem != null)
            {
                if (NetworkIsDead && _healthSystem.IsAlive)
                {
                    _healthSystem.SetCurrentHealth(0f);
                }
                else if (Mathf.Abs(_healthSystem.CurrentHealth - NetworkHealth) > 0.5f && NetworkHealth > 0)
                {
                    _healthSystem.SetCurrentHealth(NetworkHealth);
                }
            }
        }

        private void HandleHostHealthChanged(float current, float max)
        {
            NetworkHealth = current;
        }

        private void HandleHostDied()
        {
            NetworkIsDead = true;
        }

        private byte _lastElement = 0;
        private byte _lastHitSource = 0;
        private bool _lastIsCrit = false;

        /// <summary>
        /// RPC cho phép Client gửi yêu cầu gây sát thương lên Host (State Authority) để tính toán chuẩn xác,
        /// bảo toàn đầy đủ thông tin Ngũ Hành, Nguồn đòn đánh và Chủ sở hữu (Attacker) phục vụ Phản Ứng Nguyên Tố mục tiêu.
        /// </summary>
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RpcApplyDamage(float damageAmount, int attackerPlayerId, byte element = 0, byte hitSource = 0, bool isCritical = false)
        {
            if (_healthSystem != null && _healthSystem.IsAlive)
            {
                GameObject attacker = ResolvePlayerObject(attackerPlayerId);
                var damageData = new DamageData(damageAmount, isCritical, (ElementType)element, false, null)
                {
                    Owner = attacker,
                    HitSource = (ElementHitSource)hitSource
                };
                _healthSystem.TakeDamage(damageData);
            }
        }

        private GameObject ResolvePlayerObject(int attackerPlayerId)
        {
            if (Runner == null) return null;
            var playerRef = PlayerRef.FromIndex(attackerPlayerId);
            if (Runner.TryGetPlayerObject(playerRef, out var netObj) && netObj != null)
                return netObj.gameObject;
            return null;
        }
    }
}
