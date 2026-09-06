using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Enemies.Visuals;

namespace ProjectZombie.Features.Enemies.Special
{
    /// <summary>
    /// Component liên kết Dây Xích Oán Khí giữa các thực thể Quỷ Nhập Tràng (E_QUYNHAPTRANG).
    /// HOÀN TOÀN TỰ CHỦ (AUTONOMOUS / PLUG-AND-PLAY - NO SINGLETON).
    /// - Tự động quét tìm đồng loại gần nhất trong bán kính linkRadius (8.0m).
    /// - So sánh InstanceID để chỉ tạo duy nhất 1 liên kết giữa 2 con quái.
    /// - Gây sát thương và làm chậm khi người chơi đi cắt ngang qua sợi xích.
    /// - Cho phép người chơi dùng Dash lướt qua an toàn nhờ I-Frames.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class EnemyTetherLink : MonoBehaviour
    {
        [Header("Tether Link Settings")]
        [Tooltip("Bán kính tối đa để kết nối xích với đồng loại")]
        [SerializeField] private float _linkRadius = 8.0f;

        [Tooltip("Khoảng cách tối đa trước khi xích bị đứt")]
        [SerializeField] private float _breakRadius = 10.5f;

        [Tooltip("Tần suất quét tìm đồng loại (giây)")]
        [SerializeField] private float _scanInterval = 0.2f;

        [Header("Damage & Hazard Settings")]
        [Tooltip("Sát thương mỗi giây khi người chơi chạm vào xích")]
        [SerializeField] private float _damagePerSecond = 20f;

        [Tooltip("Tần suất gây sát thương (giây)")]
        [SerializeField] private float _damageTickInterval = 0.2f;

        [Tooltip("Tỉ lệ làm chậm người chơi khi chạm vào xích (0.4 = giảm 40% tốc độ)")]
        [SerializeField] private float _slowAmount = 0.4f;

        [Tooltip("Thời gian duy trì làm chậm sau khi rời khỏi xích")]
        [SerializeField] private float _slowDuration = 0.6f;

        [Tooltip("Bán kính nhận diện va chạm của sợi xích")]
        [SerializeField] private float _hitboxWidth = 0.45f;

        [Header("Layer Settings")]
        [SerializeField] private LayerMask _enemyLayer;

        private Enemy _enemy;
        private TetherBeamVisual _beamVisual;
        private EnemyTetherLink _linkedPartner;

        private float _scanTimer = 0f;
        private float _damageTimer = 0f;
        private static readonly Collider2D[] _scanBuffer = new Collider2D[16];

        public bool IsLinked => _linkedPartner != null;
        public Enemy Enemy => _enemy;

        private void Awake()
        {
            _enemy = GetComponent<Enemy>();

            // Tạo GameObject con chứa TetherBeamVisual nếu chưa có
            Transform visualTrans = transform.Find("TetherBeam_Visual");
            if (visualTrans == null)
            {
                GameObject beamObj = new GameObject("TetherBeam_Visual");
                beamObj.transform.SetParent(transform, false);
                _beamVisual = beamObj.AddComponent<TetherBeamVisual>();
            }
            else
            {
                _beamVisual = visualTrans.GetComponent<TetherBeamVisual>();
                if (_beamVisual == null) _beamVisual = visualTrans.gameObject.AddComponent<TetherBeamVisual>();
            }

            if (_enemyLayer == 0)
            {
                _enemyLayer = LayerMask.GetMask("Enemy");
                if (_enemyLayer == 0) _enemyLayer = LayerMask.NameToLayer("Enemy") != -1 ? (1 << LayerMask.NameToLayer("Enemy")) : ~0;
            }
        }

        private void OnEnable()
        {
            _scanTimer = Random.Range(0f, _scanInterval); // Random offset để rải đều tải CPU
            _damageTimer = 0f;
            BreakTether();

            if (_enemy != null && _enemy.HealthSystem != null)
            {
                _enemy.HealthSystem.OnDied += HandleDeath;
            }
        }

        private void OnDisable()
        {
            BreakTether();

            if (_enemy != null && _enemy.HealthSystem != null)
            {
                _enemy.HealthSystem.OnDied -= HandleDeath;
            }
        }

        private void Update()
        {
            if (_enemy == null || _enemy.HealthSystem == null || !_enemy.HealthSystem.IsAlive)
            {
                BreakTether();
                return;
            }

            // 1. Quản lý trạng thái liên kết hiện tại
            if (_linkedPartner != null)
            {
                // Kiểm tra điều kiện đứt xích
                if (!_linkedPartner.gameObject.activeInHierarchy || 
                    _linkedPartner.Enemy == null || 
                    _linkedPartner.Enemy.HealthSystem == null || 
                    !_linkedPartner.Enemy.HealthSystem.IsAlive ||
                    Vector2.Distance(transform.position, _linkedPartner.transform.position) > _breakRadius)
                {
                    BreakTether();
                }
                else
                {
                    // Kiểm tra và gây sát thương cho người chơi khi chạm vào xích
                    CheckPlayerDamage();
                }
            }
            else
            {
                // 2. Quét tìm đồng loại mới định kỳ
                _scanTimer -= Time.deltaTime;
                if (_scanTimer <= 0f)
                {
                    _scanTimer = _scanInterval;
                    ScanForPartner();
                }
            }
        }

        /// <summary>
        /// Quét tìm một Quỷ Nhập Tràng khác trong bán kính để kết nối xích.
        /// </summary>
        private void ScanForPartner()
        {
            int count = Physics2D.OverlapCircleNonAlloc(transform.position, _linkRadius, _scanBuffer, _enemyLayer);
            int myInstanceId = GetInstanceID();

            for (int i = 0; i < count; i++)
            {
                Collider2D col = _scanBuffer[i];
                if (col == null || col.gameObject == gameObject) continue;

                if (col.TryGetComponent<EnemyTetherLink>(out var candidate))
                {
                    // Quy tắc: Chỉ đối tượng có InstanceID nhỏ hơn mới chủ động kết nối sang đối tượng lớn hơn
                    // Giúp tránh tạo trùng lặp 2 sợi xích giữa cùng 1 cặp quỷ
                    if (candidate.Enemy != null && candidate.Enemy.HealthSystem != null && candidate.Enemy.HealthSystem.IsAlive)
                    {
                        if (myInstanceId < candidate.GetInstanceID() && !candidate.IsLinked)
                        {
                            ConnectWith(candidate);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Thiết lập liên kết xích giữa 2 quái.
        /// </summary>
        public void ConnectWith(EnemyTetherLink partner)
        {
            if (partner == null || partner == this) return;

            _linkedPartner = partner;
            partner._linkedPartner = this;

            if (_beamVisual != null)
            {
                _beamVisual.Connect(transform, partner.transform);
            }
        }

        /// <summary>
        /// Bẻ gãy liên kết xích hiện tại.
        /// </summary>
        public void BreakTether()
        {
            if (_beamVisual != null)
            {
                _beamVisual.Disconnect();
            }

            if (_linkedPartner != null)
            {
                var temp = _linkedPartner;
                _linkedPartner = null;
                if (temp._linkedPartner == this)
                {
                    temp.BreakTether();
                }
            }
        }

        /// <summary>
        /// Kiểm tra va chạm với Player và gây sát thương / làm chậm.
        /// </summary>
        private void CheckPlayerDamage()
        {
            if (_beamVisual == null || !_beamVisual.IsActive) return;

            if (PlayerProvider.HasPlayer && PlayerProvider.PlayerTransform != null)
            {
                Vector2 playerPos = PlayerProvider.PlayerTransform.position;

                if (_beamVisual.IsPointIntersecting(playerPos, _hitboxWidth))
                {
                    // Tận dụng Dash I-Frames nếu Player đang lướt
                    if (PlayerProvider.PlayerGameObject != null && 
                        PlayerProvider.PlayerGameObject.TryGetComponent<PlayerController>(out var ctrl) && 
                        ctrl.IsDashing)
                    {
                        return; // Lướt xuyên qua an toàn!
                    }

                    _damageTimer -= Time.deltaTime;
                    if (_damageTimer <= 0f)
                    {
                        _damageTimer = _damageTickInterval;
                        ApplyTetherDamageAndSlow();
                    }
                }
            }
        }

        private void ApplyTetherDamageAndSlow()
        {
            float tickDamage = _damagePerSecond * _damageTickInterval;

            // Gây sát thương máu cho Player
            if (PlayerProvider.PlayerHealth != null)
            {
                PlayerProvider.PlayerHealth.TakeDamage(tickDamage);
            }

            // Gây hiệu ứng làm chậm (Slow) cho Player
            if (PlayerProvider.PlayerGameObject != null && 
                PlayerProvider.PlayerGameObject.TryGetComponent<PlayerController>(out var ctrl))
            {
                ctrl.ApplySlow(_slowAmount, _slowDuration);
            }
        }

        private void HandleDeath()
        {
            BreakTether();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, _linkRadius);
        }
    }
}
