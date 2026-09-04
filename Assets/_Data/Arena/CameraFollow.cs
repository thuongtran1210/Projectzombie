using System.Collections;
using UnityEngine;
using Cinemachine;
using ProjectZombie.Core.Juice;

namespace ProjectZombie.Features.Arena
{
    /// <summary>
    /// Bộ điều phối Camera hiện đại chuẩn Cinemachine 2D (HD / Anime URP).
    /// Đảm bảo tương thích 100% với API cũ (SetTarget, ZoomTo, ResetZoom, TriggerShake)
    /// nhưng vận hành trên nền tảng Cinemachine Virtual Camera mượt mà 60/120 FPS.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        public static CameraFollow Instance { get; private set; }

        [Header("Cinemachine References")]
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;
        [SerializeField] private CinemachineImpulseSource _impulseSource;

        [Header("Fallback Settings (nếu không dùng Cinemachine)")]
        [SerializeField] private Transform target;
        [SerializeField] private float smoothSpeed = 6f;
        [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);

        private Vector3 _shakeOffset = Vector3.zero;
        private Coroutine _shakeCoroutine;
        private Coroutine _zoomCoroutine;
        private float _defaultOrthoSize = 5.5f;

        public float DefaultOrthoSize => _defaultOrthoSize;

        private void Awake()
        {
            if (Instance == null) Instance = this;

            EnsureVirtualCamera();

            if (_virtualCamera != null)
            {
                _defaultOrthoSize = _virtualCamera.m_Lens.OrthographicSize;
                if (_impulseSource == null)
                {
                    _impulseSource = _virtualCamera.GetComponent<CinemachineImpulseSource>();
                }
            }
            else
            {
                Camera cam = GetComponent<Camera>() ?? Camera.main;
                if (cam != null && cam.orthographic)
                {
                    _defaultOrthoSize = cam.orthographicSize;
                }
            }
        }

        private void EnsureVirtualCamera()
        {
            if (_virtualCamera == null)
            {
                _virtualCamera = FindObjectOfType<CinemachineVirtualCamera>(true);
            }
        }

        private void OnEnable()
        {
            GameJuiceEvents.OnCameraShakeRequested += TriggerShake;
        }

        private void OnDisable()
        {
            GameJuiceEvents.OnCameraShakeRequested -= TriggerShake;
        }

        private void LateUpdate()
        {
            // Nếu có Virtual Camera thì Cinemachine tự động xử lý follow mượt mà
            if (_virtualCamera != null) return;

            // Fallback khi không có Cinemachine
            if (target == null) return;

            float dt = Time.timeScale > 0.001f ? Time.deltaTime : Time.unscaledDeltaTime;
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * dt);
            transform.position = smoothedPosition + _shakeOffset;
        }

        /// <summary>
        /// Gán mục tiêu cần bám theo (Player).
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            EnsureVirtualCamera();

            if (_virtualCamera != null)
            {
                _virtualCamera.Follow = newTarget;
                _virtualCamera.LookAt = newTarget;
            }
            else if (target != null)
            {
                transform.position = target.position + offset;
            }
        }

        /// <summary>
        /// Kích hoạt hiệu ứng zoom cận cảnh mượt mà vào nhân vật (chạy trên unscaledDeltaTime).
        /// </summary>
        public void ZoomTo(float targetOrthoSize, float duration)
        {
            EnsureVirtualCamera();

            if (_zoomCoroutine != null)
            {
                StopCoroutine(_zoomCoroutine);
            }
            _zoomCoroutine = StartCoroutine(ZoomCoroutine(targetOrthoSize, duration));
        }

        /// <summary>
        /// Khôi phục lại kích thước Camera ban đầu.
        /// </summary>
        public void ResetZoom(float duration = 0.5f)
        {
            ZoomTo(_defaultOrthoSize, duration);
        }

        private IEnumerator ZoomCoroutine(float targetSize, float duration)
        {
            if (duration <= 0f)
            {
                ApplyOrthoSize(targetSize);
                yield break;
            }

            float initialSize = GetCurrentOrthoSize();
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);
                ApplyOrthoSize(Mathf.Lerp(initialSize, targetSize, smoothT));
                yield return null;
            }

            ApplyOrthoSize(targetSize);
            _zoomCoroutine = null;
        }

        private float GetCurrentOrthoSize()
        {
            if (_virtualCamera != null)
            {
                return _virtualCamera.m_Lens.OrthographicSize;
            }
            Camera cam = GetComponent<Camera>() ?? Camera.main;
            return cam != null ? cam.orthographicSize : _defaultOrthoSize;
        }

        private void ApplyOrthoSize(float size)
        {
            if (_virtualCamera != null)
            {
                _virtualCamera.m_Lens.OrthographicSize = size;
            }
            else
            {
                Camera cam = GetComponent<Camera>() ?? Camera.main;
                if (cam != null) cam.orthographicSize = size;
            }
        }

        /// <summary>
        /// Kích hoạt hiệu ứng rung màn hình.
        /// </summary>
        public void TriggerShake(float duration, float magnitude)
        {
            if (_impulseSource != null)
            {
                _impulseSource.GenerateImpulse(magnitude * 0.5f);
                return;
            }

            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
            }
            _shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
        }

        private IEnumerator ShakeCoroutine(float duration, float magnitude)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;
                _shakeOffset = new Vector3(x, y, 0f);
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
            _shakeOffset = Vector3.zero;
            _shakeCoroutine = null;
        }
    }
}

