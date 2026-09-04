using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Player.Visuals
{
    /// <summary>
    /// Hiệu ứng hình ảnh toàn diện khi nhân vật Lướt (Dash) và Tăng Tốc Độ (Speed Buff):
    /// 1. Ghost Afterimage (Tàn ảnh nhân vật mờ dần theo đường lướt hoặc khi buff tốc độ - 0 GC Pooling).
    /// 2. Bụi gió đạp chân (Wind Dust Burst) phụt ngược hướng lướt.
    /// 3. Continuous Ghost Trail khi nhân vật đang trong trạng thái Speed Buff.
    /// </summary>
    public class PlayerDashVisuals : MonoBehaviour
    {
        [Header("Ghost Afterimage Settings (Dash)")]
        [SerializeField] private Color _ghostColor = new Color(0.3f, 0.9f, 1f, 0.65f); // Xanh ngọc Cyan Anime
        [SerializeField] private float _ghostDuration = 0.25f;
        [SerializeField] private float _ghostSpawnInterval = 0.04f;
        [SerializeField] private int _poolSize = 12;

        [Header("Speed Buff Ghost Settings")]
        [SerializeField] private Color _speedBuffGhostColor = new Color(1f, 0.85f, 0.2f, 0.5f); // Hoàng Kim Tốc Biến
        [SerializeField] private float _speedBuffInterval = 0.08f;

        [Header("Dust VFX Settings")]
        [SerializeField] private GameObject _dashDustPrefab;

        private PlayerController _playerController;
        private SpriteRenderer _playerSpriteRenderer;
        private readonly Queue<SpriteRenderer> _ghostPool = new Queue<SpriteRenderer>();
        private GameObject _ghostContainer;
        private Coroutine _ghostRoutine;
        private Coroutine _speedBuffRoutine;
        private bool _isSpeedBuffActive;

        public bool IsSpeedBuffActive => _isSpeedBuffActive;

        private PlayerStats _playerStats;
        private float _baselineMoveSpeed = 5.0f;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
            _playerStats = GetComponent<PlayerStats>();
            _playerSpriteRenderer = GetComponentInChildren<SpriteRenderer>();

            CreateGhostPool();
        }

        private void Start()
        {
            if (_playerStats != null)
            {
                _baselineMoveSpeed = _playerStats.MoveSpeed;
                CheckSpeedBuffStatus();
            }
        }

        private void OnEnable()
        {
            if (_playerController != null)
            {
                _playerController.OnDashed += HandleDashVisuals;
            }
            if (_playerStats != null)
            {
                _playerStats.OnStatsUpdated += HandleStatsUpdated;
            }
        }

        private void OnDisable()
        {
            if (_playerController != null)
            {
                _playerController.OnDashed -= HandleDashVisuals;
            }
            if (_playerStats != null)
            {
                _playerStats.OnStatsUpdated -= HandleStatsUpdated;
            }
            StopSpeedBuffVisual();
        }

        private void HandleStatsUpdated()
        {
            CheckSpeedBuffStatus();
        }

        private void CheckSpeedBuffStatus()
        {
            if (_playerStats == null) return;

            // Nếu tốc chạy hiện tại cao hơn 15% so với tốc chạy cơ bản -> Tự động bật tàn ảnh tốc độ
            bool isBoosted = _playerStats.MoveSpeed >= (_baselineMoveSpeed * 1.15f);

            if (isBoosted && !_isSpeedBuffActive)
            {
                StartContinuousSpeedBuffVisual();
            }
            else if (!isBoosted && _isSpeedBuffActive && _speedBuffRoutine != null)
            {
                StopSpeedBuffVisual();
            }
        }

        /// <summary>
        /// Kích hoạt chuỗi tàn ảnh liên tục khi nhân vật đang duy trì trạng thái buff tốc độ.
        /// </summary>
        public void StartContinuousSpeedBuffVisual(Color? customColor = null)
        {
            if (_speedBuffRoutine != null)
            {
                StopCoroutine(_speedBuffRoutine);
            }
            _speedBuffRoutine = StartCoroutine(ContinuousSpeedBuffTrailRoutine(customColor ?? _speedBuffGhostColor));
        }

        private IEnumerator ContinuousSpeedBuffTrailRoutine(Color ghostCol)
        {
            _isSpeedBuffActive = true;

            while (_isSpeedBuffActive)
            {
                // Chỉ sinh tàn ảnh khi nhân vật đang thực sự di chuyển
                if (_playerController != null && _playerController.MovementInput.sqrMagnitude > 0.01f)
                {
                    SpawnSingleGhost(ghostCol);
                }

                yield return new WaitForSeconds(_speedBuffInterval);
            }

            _speedBuffRoutine = null;
        }

        private void CreateGhostPool()
        {
            _ghostContainer = new GameObject("GhostTrail_Pool");
            _ghostContainer.transform.SetParent(null);

            for (int i = 0; i < _poolSize; i++)
            {
                GameObject ghostObj = new GameObject($"Ghost_{i}");
                ghostObj.transform.SetParent(_ghostContainer.transform);
                var sr = ghostObj.AddComponent<SpriteRenderer>();
                sr.sortingLayerName = "Entities";
                sr.sortingOrder = -1; // Vẽ ngay phía sau thân nhân vật
                ghostObj.SetActive(false);
                _ghostPool.Enqueue(sr);
            }
        }

        private void HandleDashVisuals()
        {
            if (_ghostRoutine != null)
            {
                StopCoroutine(_ghostRoutine);
            }
            _ghostRoutine = StartCoroutine(GhostTrailRoutine());

            // Sinh Bụi Gió lướt nếu có Prefab
            if (_dashDustPrefab != null && _playerController != null)
            {
                Vector2 dir = _playerController.MovementInput;
                if (dir == Vector2.zero) dir = Vector2.right;

                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                Quaternion rot = Quaternion.Euler(0f, 0f, angle + 180f); // Phụt ngược hướng lướt

                Instantiate(_dashDustPrefab, transform.position, rot);
            }
        }

        private IEnumerator GhostTrailRoutine()
        {
            float elapsed = 0f;
            float dashTime = _playerController != null ? _playerController.DashDuration : 0.2f;

            while (elapsed < dashTime)
            {
                SpawnSingleGhost(_ghostColor);
                elapsed += _ghostSpawnInterval;
                yield return new WaitForSeconds(_ghostSpawnInterval);
            }
        }

        /// <summary>
        /// Kích hoạt chuỗi tàn ảnh liên tục khi nhân vật nhận hiệu ứng tăng tốc độ di chuyển trong duration giây.
        /// </summary>
        public void StartSpeedBuffVisual(float duration, Color? customColor = null)
        {
            if (_speedBuffRoutine != null)
            {
                StopCoroutine(_speedBuffRoutine);
            }
            _speedBuffRoutine = StartCoroutine(SpeedBuffTrailRoutine(duration, customColor ?? _speedBuffGhostColor));
        }

        public void StopSpeedBuffVisual()
        {
            _isSpeedBuffActive = false;
            if (_speedBuffRoutine != null)
            {
                StopCoroutine(_speedBuffRoutine);
                _speedBuffRoutine = null;
            }
        }

        private IEnumerator SpeedBuffTrailRoutine(float duration, Color ghostCol)
        {
            _isSpeedBuffActive = true;
            float timer = duration;

            while (timer > 0f)
            {
                timer -= _speedBuffInterval;

                // Chỉ sinh tàn ảnh khi nhân vật đang thực sự di chuyển
                if (_playerController != null && _playerController.MovementInput.sqrMagnitude > 0.01f)
                {
                    SpawnSingleGhost(ghostCol);
                }

                yield return new WaitForSeconds(_speedBuffInterval);
            }

            _isSpeedBuffActive = false;
            _speedBuffRoutine = null;
        }

        private void SpawnSingleGhost(Color color)
        {
            if (_playerSpriteRenderer == null)
            {
                _playerSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
            if (_playerSpriteRenderer == null || _playerSpriteRenderer.sprite == null) return;
            if (_ghostPool.Count == 0) return;

            SpriteRenderer ghost = _ghostPool.Dequeue();
            ghost.gameObject.SetActive(true);
            ghost.transform.position = _playerSpriteRenderer.transform.position;
            ghost.transform.rotation = _playerSpriteRenderer.transform.rotation;
            ghost.transform.localScale = _playerSpriteRenderer.transform.lossyScale;
            ghost.sprite = _playerSpriteRenderer.sprite;
            ghost.flipX = _playerSpriteRenderer.flipX;
            ghost.flipY = _playerSpriteRenderer.flipY;
            ghost.sortingLayerID = _playerSpriteRenderer.sortingLayerID;
            ghost.sortingOrder = _playerSpriteRenderer.sortingOrder - 1; // Vẽ ngay sau lưng nhân vật
            ghost.sharedMaterial = _playerSpriteRenderer.sharedMaterial; // Kế thừa cùng Shader URP Sprite Unlit
            ghost.color = color;

            StartCoroutine(FadeOutGhost(ghost, color));
        }

        private IEnumerator FadeOutGhost(SpriteRenderer ghost, Color startCol)
        {
            float t = 0f;

            while (t < _ghostDuration)
            {
                t += Time.deltaTime;
                float alpha = Mathf.Lerp(startCol.a, 0f, t / _ghostDuration);
                ghost.color = new Color(startCol.r, startCol.g, startCol.b, alpha);
                yield return null;
            }

            ghost.gameObject.SetActive(false);
            _ghostPool.Enqueue(ghost);
        }

        private void OnDestroy()
        {
            if (_ghostContainer != null)
            {
                Destroy(_ghostContainer);
            }
        }
    }
}
