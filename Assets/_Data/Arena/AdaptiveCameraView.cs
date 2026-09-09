using UnityEngine;
using Cinemachine;

namespace ProjectZombie.Features.Arena
{
    /// <summary>
    /// Bộ điều phối thích ứng tỉ lệ màn hình cho Camera 2D (Adaptive Camera View).
    /// Đảm bảo chiều rộng chiến trường tối thiểu (Target Horizontal Width) luôn hiển thị đầy đủ
    /// trên mọi tỉ lệ màn hình di động (16:9, 18:9, 19.5:9, 4:3, v.v.).
    /// </summary>
    [ExecuteAlways]
    public class AdaptiveCameraView : MonoBehaviour
    {
        [Header("Target World Dimensions")]
        [Tooltip("Chiều rộng thế giới tối thiểu cần hiển thị đầy đủ trên màn hình (đơn vị: Unity Units / Mét)")]
        [SerializeField] private float _targetWorldWidth = 21.33f;

        [Tooltip("Kích thước Ortho mặc định ở tỉ lệ 16:9")]
        [SerializeField] private float _baseOrthoSize = 6.0f;

        [Header("References")]
        [SerializeField] private CinemachineVirtualCamera _virtualCamera;
        [SerializeField] private Camera _targetCamera;

        private float _lastAspect = 0f;

        private void Awake()
        {
            CacheReferences();
            UpdateOrthoSize();
        }

        private void OnEnable()
        {
            CacheReferences();
            UpdateOrthoSize();
        }

        private void Update()
        {
            float currentAspect = GetCurrentAspectRatio();
            if (Mathf.Abs(currentAspect - _lastAspect) > 0.01f)
            {
                UpdateOrthoSize();
            }
        }

        private void CacheReferences()
        {
            if (_virtualCamera == null)
            {
                _virtualCamera = GetComponent<CinemachineVirtualCamera>() ?? FindObjectOfType<CinemachineVirtualCamera>(true);
            }

            if (_targetCamera == null)
            {
                _targetCamera = GetComponent<Camera>() ?? Camera.main;
            }
        }

        /// <summary>
        /// Cân chỉnh Orthographic Size để đảm bảo chiều ngang chiến trường không bị cắt xén.
        /// </summary>
        public void UpdateOrthoSize()
        {
            float aspect = GetCurrentAspectRatio();
            if (aspect <= 0.001f) return;

            _lastAspect = aspect;

            // Target Ortho Size dựa trên chiều ngang thế giới mong muốn: Width = 2 * OrthoSize * Aspect => OrthoSize = Width / (2 * Aspect)
            float requiredOrthoSize = _targetWorldWidth / (2f * aspect);
            float finalOrthoSize = Mathf.Max(_baseOrthoSize, requiredOrthoSize);

            if (_virtualCamera != null)
            {
                _virtualCamera.m_Lens.OrthographicSize = finalOrthoSize;
            }
            else if (_targetCamera != null && _targetCamera.orthographic)
            {
                _targetCamera.orthographicSize = finalOrthoSize;
            }
        }

        private float GetCurrentAspectRatio()
        {
            if (_targetCamera != null && _targetCamera.pixelHeight > 0)
            {
                return (float)_targetCamera.pixelWidth / _targetCamera.pixelHeight;
            }

            return Screen.height > 0 ? (float)Screen.width / Screen.height : 16f / 9f;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            CacheReferences();
            UpdateOrthoSize();
        }
#endif
    }
}
