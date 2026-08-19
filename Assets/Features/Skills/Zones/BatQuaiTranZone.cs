using System.Collections;
using UnityEngine;
using ProjectZombie.Features.YinYang;
using ProjectZombie.Features.Enemies;

namespace ProjectZombie.Features.Skills.Zones
{
    /// <summary>
    /// Vùng hiệu lực Bát Quái Trận Đồ của Đạo Sĩ (Mục 3.1.2 GDD v4.0).
    /// Bán kính: 4.5m, Thời lượng: 4s.
    /// Nâng cấp đồ họa: Thái Cực Bát Quái Ground Decal xoay tròn mượt mà, hiệu ứng nở trận (Spawn In) và thu trận (Fade Out).
    /// </summary>
    public class BatQuaiTranZone : MonoBehaviour
    {
        [Header("Zone Settings")]
        [SerializeField] private float _radius = 4.5f;
        [SerializeField] private float _duration = 4.0f;

        [Header("Visual Components")]
        [SerializeField] private Transform _decalTransform;
        [SerializeField] private SpriteRenderer _decalSpriteRenderer;
        [SerializeField] private ParticleSystem _talismanParticles;
        [SerializeField] private float _rotationSpeed = 35f;

        private static readonly Collider2D[] _hitBuffer = new Collider2D[60];
        private float _spawnTime;
        private bool _isActive;
        private Vector3 _targetScale;

        public float Radius => _radius;
        public float Duration => _duration;

        private void Awake()
        {
            FetchComponents();
        }

        private void FetchComponents()
        {
            if (_decalTransform == null)
            {
                var visual = transform.Find("DecalVisual");
                if (visual != null)
                {
                    _decalTransform = visual;
                    _decalSpriteRenderer = visual.GetComponent<SpriteRenderer>();
                }
            }
        }

        public void Initialize(Vector3 centerPosition, float radius = 4.5f, float duration = 4.0f)
        {
            FetchComponents();
            transform.position = centerPosition;
            _radius = radius;
            _duration = duration;
            _spawnTime = Time.time;
            _isActive = true;

            // Bán kính 4.5m -> Đường kính 9m. Sprite 1024x1024 (PPU = 128 -> Size 8m) -> Scale ~ 1.125f
            _targetScale = new Vector3(_radius * 2f / 8f, _radius * 2f / 8f, 1f);

            if (_decalTransform != null)
            {
                _decalTransform.localScale = Vector3.zero;
            }

            StartCoroutine(DecalAnimationRoutine());

            // Ép Cán cân Âm Dương về 50 trong 4s
            if (YinYangManager.Instance != null)
            {
                YinYangManager.Instance.SetTemporaryNeutralOverride(_duration, 50f);
            }

            Destroy(gameObject, _duration + 0.3f);
        }

        private IEnumerator DecalAnimationRoutine()
        {
            float elapsed = 0f;
            float spawnDuration = 0.35f;
            float fadeDuration = 0.4f;
            float activeDuration = _duration - spawnDuration - fadeDuration;

            // 1. Giai đoạn Mở Trận (Expand & Spin up)
            while (elapsed < spawnDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / spawnDuration);
                // OutBack ease
                float s = 1.4f;
                float progress = 1f + (s + 1f) * Mathf.Pow(t - 1f, 3f) + s * Mathf.Pow(t - 1f, 2f);
                
                if (_decalTransform != null)
                {
                    _decalTransform.localScale = _targetScale * Mathf.Max(0f, progress);
                    _decalTransform.Rotate(0f, 0f, _rotationSpeed * 2.5f * Time.deltaTime);
                }
                yield return null;
            }

            if (_decalTransform != null)
            {
                _decalTransform.localScale = _targetScale;
            }

            // 2. Giai đoạn Duy trì (Active Rotation)
            elapsed = 0f;
            while (elapsed < activeDuration)
            {
                elapsed += Time.deltaTime;
                if (_decalTransform != null)
                {
                    _decalTransform.Rotate(0f, 0f, _rotationSpeed * Time.deltaTime);
                }
                yield return null;
            }

            // 3. Giai đoạn Thu Trận (Fade Out & Dissolve)
            elapsed = 0f;
            Color initColor = _decalSpriteRenderer != null ? _decalSpriteRenderer.color : Color.white;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                if (_decalSpriteRenderer != null)
                {
                    Color c = initColor;
                    c.a = Mathf.Lerp(1f, 0f, t);
                    _decalSpriteRenderer.color = c;
                }
                if (_decalTransform != null)
                {
                    _decalTransform.Rotate(0f, 0f, _rotationSpeed * 0.5f * Time.deltaTime);
                    _decalTransform.localScale = Vector3.Lerp(_targetScale, _targetScale * 0.7f, t);
                }
                yield return null;
            }
        }

        private void Update()
        {
            if (!_isActive) return;

            if (Time.time - _spawnTime >= _duration)
            {
                _isActive = false;
                return;
            }

            // Quét các enemy trong vùng bounding circle trước để tối ưu hiệu năng
            int mask = Shared.TargetingUtility.EnemyLayerMask;
            int hitCount = Physics2D.OverlapCircleNonAlloc(transform.position, _radius, _hitBuffer, mask);
            
            float currentRotationDeg = _decalTransform != null ? _decalTransform.eulerAngles.z : 0f;

            for (int i = 0; i < hitCount; i++)
            {
                Collider2D col = _hitBuffer[i];
                if (col == null) continue;

                // Boss miễn nhiễm Bát Quái Trận
                if (col.CompareTag("Boss")) continue;

                // Kiểm tra chính xác nằm trong hình Bát Giác 8 Cạnh (IsInsideOctagon)
                if (IsPointInsideOctagon(col.transform.position, transform.position, _radius, currentRotationDeg))
                {
                    if (col.TryGetComponent<Enemy>(out var enemy) && !enemy.IsBoss)
                    {
                        enemy.ApplyTrapCirclingState(transform.position, _radius, 0.5f);
                    }
                }
            }
        }

        /// <summary>
        /// Thuật toán kiểm tra điểm có nằm trong hình Bát Giác 8 Cạnh đều xoay theo trận đồ hay không.
        /// </summary>
        private static bool IsPointInsideOctagon(Vector2 point, Vector2 center, float radius, float rotationDeg)
        {
            Vector2 localPos = point - center;
            float angleRad = -rotationDeg * Mathf.Deg2Rad;
            // Xoay ngược tọa độ về hệ quy chiếu chuẩn
            float cos = Mathf.Cos(angleRad);
            float sin = Mathf.Sin(angleRad);
            float x = localPos.x * cos - localPos.y * sin;
            float y = localPos.x * sin + localPos.y * cos;

            // Bát giác 8 cạnh đều có apothem (khoảng cách từ tâm đến cạnh) = radius * cos(22.5°)
            float apothem = radius * 0.92387953f; // cos(pi/8)

            // Kiểm tra 4 cặp trục đối xứng của bát giác
            if (Mathf.Abs(x) > apothem || Mathf.Abs(y) > apothem) return false;

            float diag1 = (Mathf.Abs(x) + Mathf.Abs(y)) * 0.70710678f; // cos(45°)
            if (diag1 > apothem) return false;

            return true;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            DrawOctagonGizmo(transform.position, _radius, _decalTransform != null ? _decalTransform.eulerAngles.z : 0f);
        }

        private static void DrawOctagonGizmo(Vector3 center, float radius, float rotationDeg)
        {
            Vector3 prevPoint = Vector3.zero;
            for (int i = 0; i <= 8; i++)
            {
                float angle = (i * 45f - 22.5f + rotationDeg) * Mathf.Deg2Rad;
                Vector3 currentPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
                if (i > 0)
                {
                    Gizmos.DrawLine(prevPoint, currentPoint);
                }
                prevPoint = currentPoint;
            }
        }
    }
}
