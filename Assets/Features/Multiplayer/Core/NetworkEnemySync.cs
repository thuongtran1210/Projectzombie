using UnityEngine;
using Fusion;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Enemies;

namespace ProjectZombie.Features.Multiplayer.Core
{
    /// <summary>
    /// Cấu trúc lưu trữ thông tin của từng hit trong batch sát thương gửi từ Client lên Host.
    /// Giữ nguyên sát thương thô, hệ nguyên tố, nguồn đòn đánh, crit, attack id và quyền kích hoạt phản ứng.
    /// </summary>
    [System.Serializable]
    public struct NetworkDamageHit : INetworkStruct
    {
        public float RawDamage;
        public byte Element;
        public byte HitSource;
        public NetworkBool IsCritical;
        public NetworkBool CanTriggerReaction;
        public ulong AttackId;

        public NetworkDamageHit(float rawDamage, byte element = 0, byte hitSource = 0, bool isCritical = false, bool canTriggerReaction = true, ulong attackId = 0)
        {
            RawDamage = rawDamage;
            Element = element;
            HitSource = hitSource;
            IsCritical = isCritical;
            CanTriggerReaction = canTriggerReaction;
            AttackId = attackId;
        }
    }

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
        private readonly System.Collections.Generic.List<NetworkDamageHit> _pendingHits = new System.Collections.Generic.List<NetworkDamageHit>();

        private void Awake()
        {
            _healthSystem = GetComponent<HealthSystem>();
            _enemy = GetComponent<Enemy>();
        }

        public override void Spawned()
        {
            _pendingHits.Clear();

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

                // Chuyển hướng mọi đòn đánh cục bộ của Client vào bộ đệm gom danh sách các hit (Damage Hit Batching)
                // để bảo toàn từng hit (hệ, nguồn, crit, attack id, reaction flag) và tránh spam RPC.
                if (_healthSystem != null)
                {
                    _healthSystem.CustomDamageInterceptor = (amount, data) =>
                    {
                        var hit = new NetworkDamageHit(
                            rawDamage: data.RawAmount > 0f ? data.RawAmount : amount,
                            element: (byte)data.Element,
                            hitSource: (byte)data.HitSource,
                            isCritical: data.IsCritical,
                            canTriggerReaction: data.CanTriggerReaction,
                            attackId: data.AttackId
                        );
                        _pendingHits.Add(hit);

                        // Match local cooldown prediction to the owning attacker for responsive feel,
                        // while authoritative damage calculation and monster reactions happen on host.
                        DamageUtility.RegisterSuccessfulHit(data);
                        return true;
                    };
                }
            }
        }

        public override void FixedUpdateNetwork()
        {
            // Trên máy Client: Nếu có các hit tích lũy trong tick này, gửi 1 batch duy nhất lên Host
            if (!Runner.IsServer && _pendingHits.Count > 0 && Object.IsValid)
            {
                RpcApplyDamageBatch(_pendingHits.ToArray());
                _pendingHits.Clear();
            }
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _pendingHits.Clear();
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

        /// <summary>
        /// RPC cho phép Client gửi batch danh sách các hit gây sát thương lên Host (State Authority)
        /// để tính toán Tương Khắc (1.3x) và Phản Ứng Nguyên Tố chuẩn xác theo thứ tự,
        /// bảo toàn đầy đủ thông tin Ngũ Hành, Nguồn đòn đánh, Crit, AttackId và CanTriggerReaction.
        /// Danh tính người gửi được trích xuất an toàn từ RpcInfo.Source (không tin client-provided id).
        /// </summary>
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RpcApplyDamageBatch(NetworkDamageHit[] hits, RpcInfo info = default)
        {
            GameObject attacker = ResolvePlayerObject(info.Source);
            ProcessDamageBatchHost(hits, attacker);
        }

        /// <summary>
        /// Xử lý batch sát thương trên Host theo đúng thứ tự, tính Tương Khắc (1.3x) và Phản Ứng Nguyên Tố.
        /// </summary>
        public void ProcessDamageBatchHost(NetworkDamageHit[] hits, GameObject attacker)
        {
            if (_healthSystem == null || !_healthSystem.IsAlive || hits == null || hits.Length == 0) return;

            for (int i = 0; i < hits.Length; i++)
            {
                if (!_healthSystem.IsAlive) break;

                var hit = hits[i];
                var damageData = new DamageData(hit.RawDamage, hit.IsCritical, (ElementType)hit.Element, false, null, hit.CanTriggerReaction)
                {
                    RawAmount = hit.RawDamage,
                    Owner = attacker,
                    HitSource = (ElementHitSource)hit.HitSource,
                    AttackId = hit.AttackId,
                    ElementMultiplierApplied = false
                };

                _healthSystem.TakeDamage(damageData);
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        public void RpcApplyDamage(float damageAmount, int attackerPlayerId = 0, byte element = 0, byte hitSource = 0, bool isCritical = false, RpcInfo info = default)
        {
            PlayerRef playerRef = info.Source != PlayerRef.None ? info.Source : PlayerRef.FromIndex(attackerPlayerId);
            GameObject attacker = ResolvePlayerObject(playerRef);
            var hit = new NetworkDamageHit(damageAmount, element, hitSource, isCritical, true, 0);
            ProcessDamageBatchHost(new[] { hit }, attacker);
        }

        public GameObject ResolvePlayerObject(PlayerRef playerRef)
        {
            if (Runner == null) return null;
            if (playerRef == PlayerRef.None)
            {
                playerRef = Runner.LocalPlayer;
            }
            if (Runner.TryGetPlayerObject(playerRef, out var netObj) && netObj != null)
                return netObj.gameObject;
            return null;
        }

        public GameObject ResolvePlayerObject(int attackerPlayerId)
        {
            return ResolvePlayerObject(PlayerRef.FromIndex(attackerPlayerId));
        }
    }
}
