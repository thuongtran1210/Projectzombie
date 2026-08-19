using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Player.Visuals
{
    /// <summary>
    /// Hiệu ứng hình ảnh toàn diện khi nhân vật Lướt (Dash):
    /// 1. Ghost Afterimage (Tàn ảnh nhân vật mờ dần theo đường lướt - 0 GC Pooling).
    /// 2. Bụi gió đạp chân (Wind Dust Burst) phụt ngược hướng lướt.
    /// </summary>
    public class PlayerDashVisuals : MonoBehaviour
    {
        [Header("Ghost Afterimage Settings")]
        [SerializeField] private Color _ghostColor = new Color(0.3f, 0.9f, 1f, 0.65f); // Xanh ngọc Cyan Anime
        [SerializeField] private float _ghostDuration = 0.25f;
        [SerializeField] private float _ghostSpawnInterval = 0.04f;
        [SerializeField] private int _poolSize = 8;

        [Header("Dust VFX Settings")]
        [SerializeField] private GameObject _dashDustPrefab;

        private PlayerController _playerController;
        private SpriteRenderer _playerSpriteRenderer;
        private readonly Queue<SpriteRenderer> _ghostPool = new Queue<SpriteRenderer>();
        private GameObject _ghostContainer;
        private Coroutine _ghostRoutine;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
            _playerSpriteRenderer = GetComponentInChildren<SpriteRenderer>();

            CreateGhostPool();
        }

        private void OnEnable()
        {
            if (_playerController != null)
            {
                _playerController.OnDashed += HandleDashVisuals;
            }
        }

        private void OnDisable()
        {
            if (_playerController != null)
            {
                _playerController.OnDashed -= HandleDashVisuals;
            }
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
            float dashTime = 0.2f;

            while (elapsed < dashTime)
            {
                SpawnSingleGhost();
                elapsed += _ghostSpawnInterval;
                yield return new WaitForSeconds(_ghostSpawnInterval);
            }
        }

        private void SpawnSingleGhost()
        {
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
            ghost.color = _ghostColor;

            StartCoroutine(FadeOutGhost(ghost));
        }

        private IEnumerator FadeOutGhost(SpriteRenderer ghost)
        {
            float t = 0f;
            Color startCol = _ghostColor;

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
