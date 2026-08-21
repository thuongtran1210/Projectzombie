using UnityEngine;
using ProjectZombie.Core.Juice;
using System.Collections;

namespace ProjectZombie.Features.Arena
{
    public class CameraFollow : MonoBehaviour
    {
        public static CameraFollow Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);

        [Header("Camera Zoom Settings")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private UnityEngine.U2D.PixelPerfectCamera _pixelPerfectCamera;

        private Vector3 _shakeOffset = Vector3.zero;
        private Coroutine _shakeCoroutine;
        private Coroutine _zoomCoroutine;
        private float _defaultOrthoSize = 5f;

        public float DefaultOrthoSize => _defaultOrthoSize;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
                if (targetCamera == null) targetCamera = Camera.main;
            }

            if (targetCamera != null)
            {
                if (targetCamera.orthographic)
                {
                    _defaultOrthoSize = targetCamera.orthographicSize;
                }
                _pixelPerfectCamera = targetCamera.GetComponent<UnityEngine.U2D.PixelPerfectCamera>();
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
            if (target == null) return;

            // Dùng unscaledDeltaTime để camera vẫn follow mượt khi slow-motion
            float dt = Time.timeScale > 0.001f ? Time.deltaTime : Time.unscaledDeltaTime;
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * dt);
            transform.position = smoothedPosition + _shakeOffset;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null)
            {
                transform.position = target.position + offset;
            }
        }

        /// <summary>
        /// Kích hoạt hiệu ứng zoom cận cảnh mượt mà vào nhân vật (chạy trên unscaledDeltaTime).
        /// Tự động tạm tắt PixelPerfectCamera để tránh bị override orthographicSize mỗi frame.
        /// </summary>
        public void ZoomTo(float targetOrthoSize, float duration)
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>() ?? Camera.main;
            }

            if (targetCamera == null || !targetCamera.orthographic) return;

            if (_pixelPerfectCamera == null && targetCamera != null)
            {
                _pixelPerfectCamera = targetCamera.GetComponent<UnityEngine.U2D.PixelPerfectCamera>();
            }

            // Vô hiệu hóa PixelPerfectCamera trong quá trình zoom
            if (_pixelPerfectCamera != null)
            {
                _pixelPerfectCamera.enabled = false;
            }

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
            if (_pixelPerfectCamera != null)
            {
                _pixelPerfectCamera.enabled = true;
            }
            ZoomTo(_defaultOrthoSize, duration);
        }

        private IEnumerator ZoomCoroutine(float targetSize, float duration)
        {
            if (duration <= 0f)
            {
                targetCamera.orthographicSize = targetSize;
                yield break;
            }

            float initialSize = targetCamera.orthographicSize;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                // Dùng SmoothStep để zoom có độ chuyển tự nhiên
                float smoothT = Mathf.SmoothStep(0f, 1f, t);
                targetCamera.orthographicSize = Mathf.Lerp(initialSize, targetSize, smoothT);
                yield return null;
            }

            targetCamera.orthographicSize = targetSize;
            _zoomCoroutine = null;
        }

        /// <summary>
        /// Kích hoạt hiệu ứng rung màn hình.
        /// </summary>
        public void TriggerShake(float duration, float magnitude)
        {
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
                elapsed += Time.unscaledDeltaTime; // Sử dụng unscaledDeltaTime để không bị ảnh hưởng bởi Hit Stop
                yield return null;
            }
            _shakeOffset = Vector3.zero;
            _shakeCoroutine = null;
        }
    }
}

