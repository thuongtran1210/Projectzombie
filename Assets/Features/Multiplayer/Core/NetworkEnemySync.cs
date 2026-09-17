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

        private void Awake()
        {
            _healthSystem = GetComponent<HealthSystem>();
            _enemy = GetComponent<Enemy>();
        }

        public override void Spawned()
        {
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
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (Runner.IsServer && _healthSystem != null)
            {
                _healthSystem.OnHealthChanged -= HandleHostHealthChanged;
                _healthSystem.OnDied -= HandleHostDied;
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

        /// <summary>
        /// RPC cho phép Client gửi yêu cầu gây sát thương lên Host (State Authority) để tính toán chuẩn xác.
        /// </summary>
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RpcApplyDamage(float damageAmount, int attackerPlayerId)
        {
            if (_healthSystem != null && _healthSystem.IsAlive)
            {
                _healthSystem.TakeDamage(damageAmount);
            }
        }
    }
}
