using System.Collections;
using UnityEngine;
using ProjectZombie.Features.Shared;

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
            if (_hitStopCoroutine != null)
            {
                StopCoroutine(_hitStopCoroutine);
                _hitStopCoroutine = null;
            }
        }

        private void HandleHitStop(float duration)
        {
            // Nếu game đang Pause hoặc không trong trạng thái Playing thì bỏ qua HitStop
            if (GameStateManager.Instance != null && GameStateManager.Instance.CurrentState != GameState.Playing)
            {
                return;
            }

            if (Mathf.Approximately(Time.timeScale, 0f))
            {
                return;
            }

            if (_hitStopCoroutine != null)
            {
                StopCoroutine(_hitStopCoroutine);
            }
            _hitStopCoroutine = StartCoroutine(HitStopCoroutine(duration));
        }

        private IEnumerator HitStopCoroutine(float duration)
        {
            Time.timeScale = 0.05f;
            yield return new WaitForSecondsRealtime(duration);

            // Luôn khôi phục về 1.0f nếu game đang trong trạng thái Playing (hoặc chưa khởi tạo GameStateManager)
            if (GameStateManager.Instance == null || GameStateManager.Instance.CurrentState == GameState.Playing)
            {
                Time.timeScale = 1.0f;
            }
            _hitStopCoroutine = null;
        }
    }
}
