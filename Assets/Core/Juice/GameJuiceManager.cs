using System.Collections;
using UnityEngine;

namespace ProjectZombie.Core.Juice
{
    /// <summary>
    /// Manager quản lý các hiệu ứng khựng hình (Hit Stop) an toàn với Time.timeScale và Game State.
    /// </summary>
    public class GameJuiceManager : MonoBehaviour
    {
        public static GameJuiceManager Instance { get; private set; }

        private Coroutine _hitStopCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            GameJuiceEvents.OnHitStopRequested += HandleHitStop;
        }

        private void OnDisable()
        {
            GameJuiceEvents.OnHitStopRequested -= HandleHitStop;
        }

        private void HandleHitStop(float duration)
        {
            if (_hitStopCoroutine != null)
            {
                StopCoroutine(_hitStopCoroutine);
            }
            _hitStopCoroutine = StartCoroutine(HitStopCoroutine(duration));
        }

        private IEnumerator HitStopCoroutine(float duration)
        {
            float previousTimeScale = Time.timeScale;
            
            // Nếu game đang Pause (timeScale = 0) thì bỏ qua không thực hiện Hit Stop
            if (Mathf.Approximately(previousTimeScale, 0f))
            {
                yield break;
            }

            Time.timeScale = 0.05f;
            yield return new WaitForSecondsRealtime(duration);

            // Khôi phục lại đúng timeScale trước đó (đảm bảo không tự ý Unpause game)
            Time.timeScale = previousTimeScale;
            _hitStopCoroutine = null;
        }
    }
}
