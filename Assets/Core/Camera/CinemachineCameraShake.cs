using System.Collections;
using UnityEngine;
using Cinemachine;
using ProjectZombie.Core.Juice;

namespace ProjectZombie.Core.Camera
{
    /// <summary>
    /// Handler điều khiển Camera Shake qua Cinemachine Noise, 
    /// lắng nghe event tập trung từ GameJuiceEvents.
    /// </summary>
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public class CinemachineCameraShake : MonoBehaviour
    {
        private CinemachineVirtualCamera _virtualCamera;
        private CinemachineBasicMultiChannelPerlin _multiChannelPerlin;
        private Coroutine _shakeCoroutine;

        private void Awake()
        {
            _virtualCamera = GetComponent<CinemachineVirtualCamera>();
            if (_virtualCamera != null)
            {
                _multiChannelPerlin = _virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            }
        }

        private void OnEnable()
        {
            GameJuiceEvents.OnCameraShakeRequested += HandleCameraShake;
        }

        private void OnDisable()
        {
            GameJuiceEvents.OnCameraShakeRequested -= HandleCameraShake;
        }

        private void HandleCameraShake(float duration, float magnitude)
        {
            if (_multiChannelPerlin == null) return;

            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
            }
            _shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
        }

        private IEnumerator ShakeCoroutine(float duration, float magnitude)
        {
            _multiChannelPerlin.m_AmplitudeGain = magnitude;
            _multiChannelPerlin.m_FrequencyGain = 2f; // Tần số rung phù hợp cho hiệu ứng đánh/nổ

            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.unscaledDeltaTime; // Dùng unscaledDeltaTime để ngay cả khi HitStop (Time.timeScale thấp) camera vẫn rung mượt
                _multiChannelPerlin.m_AmplitudeGain = Mathf.Lerp(magnitude, 0f, elapsedTime / duration);
                yield return null;
            }

            _multiChannelPerlin.m_AmplitudeGain = 0f;
            _shakeCoroutine = null;
        }
    }
}
