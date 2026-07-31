using UnityEngine;
using ProjectZombie.Features.YinYang;
using ProjectZombie.Features.Enemies;

namespace ProjectZombie.Features.Skills.Zones
{
    /// <summary>
    /// Vùng hiệu lực Bát Quái Trận Đồ của Đạo Sĩ (Mục 3.1.2 GDD v4.0).
    /// Bán kính: 4.5m, Thời lượng: 4s.
    /// Khóa pathing quái trong vùng (TrapCircling) và ép cán cân Âm Dương về 50.
    /// </summary>
    public class BatQuaiTranZone : MonoBehaviour
    {
        [Header("Zone Settings")]
        [SerializeField] private float _radius = 4.5f;
        [SerializeField] private float _duration = 4.0f;

        [Header("Visual Settings")]
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private int _octagonSegments = 8;

        private static readonly Collider2D[] _hitBuffer = new Collider2D[60];
        private float _spawnTime;
        private bool _isActive;

        public float Radius => _radius;
        public float Duration => _duration;

        private void Awake()
        {
            if (_lineRenderer == null)
            {
                _lineRenderer = GetComponent<LineRenderer>();
            }
        }

        public void Initialize(Vector3 centerPosition, float radius = 4.5f, float duration = 4.0f)
        {
            transform.position = centerPosition;
            _radius = radius;
            _duration = duration;
            _spawnTime = Time.time;
            _isActive = true;

            DrawOctagonShape();

            // Ép Cán cân Âm Dương về 50 trong 4s
            if (YinYangManager.Instance != null)
            {
                YinYangManager.Instance.SetTemporaryNeutralOverride(_duration, 50f);
            }

            Destroy(gameObject, _duration);
        }

        private void Update()
        {
            if (!_isActive) return;

            if (Time.time - _spawnTime >= _duration)
            {
                _isActive = false;
                return;
            }

            // Quét các enemy trong vùng 4.5m liên tục per-frame / intervals
            int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, _radius, _hitBuffer);
            for (int i = 0; i < hitCount; i++)
            {
                Collider2D col = _hitBuffer[i];
                if (col == null) continue;

                // Boss miễn nhiễm Bát Quái Trận (GDD balance rule)
                if (col.CompareTag("Boss")) continue;

                var enemyFSM = col.GetComponent<EnemyStateMachine>();
                if (enemyFSM != null && !enemyFSM.IsBoss)
                {
                    enemyFSM.ApplyTrapCirclingState(transform.position, _radius, 0.5f);
                }
            }
        }

        private void DrawOctagonShape()
        {
            if (_lineRenderer == null) return;

            _lineRenderer.positionCount = _octagonSegments + 1;
            _lineRenderer.useWorldSpace = true;
            _lineRenderer.loop = true;

            float angleStep = 360f / _octagonSegments;
            for (int i = 0; i <= _octagonSegments; i++)
            {
                float angleRad = Mathf.Deg2Rad * (i * angleStep);
                Vector3 pos = transform.position + new Vector3(Mathf.Cos(angleRad) * _radius, Mathf.Sin(angleRad) * _radius, 0f);
                _lineRenderer.SetPosition(i, pos);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}
