using UnityEngine;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.Multiplayer.Core
{
    /// <summary>
    /// Component chuyên trách đồng bộ hoá Hoạt ảnh (Animation State & Facing Flip) qua mạng
    /// cho các Remote Player Proxies.
    /// Tuân thủ nguyên tắc Single Responsibility Principle (SRP), tách rời khỏi NetworkPlayerCharacter.
    /// </summary>
    public class NetworkCharacterAnimatorSync : MonoBehaviour
    {
        private PlayerAnimator _animator;
        private Vector3 _lastRenderPosition;

        private void Awake()
        {
            _animator = GetComponentInChildren<PlayerAnimator>();
            _lastRenderPosition = transform.position;
        }

        /// <summary>
        /// Đồng bộ hoạt ảnh di chuyển và lật mặt (Flip) dựa trên biến thiên vị trí mạng (NetworkTransform).
        /// </summary>
        public void UpdateRemoteAnimation()
        {
            if (_animator == null) return;

            Vector3 currentPos = transform.position;
            Vector3 delta = currentPos - _lastRenderPosition;
            float distSqr = delta.sqrMagnitude;
            _lastRenderPosition = currentPos;

            // Nếu có dịch chuyển đáng kể trong frame (đang chạy), bật Run và Flip hướng
            if (distSqr > 0.0001f)
            {
                _animator.ChangeAnimationState(PlayerAnimationState.Run);
                if (Mathf.Abs(delta.x) > 0.001f)
                {
                    _animator.FlipToDirection(delta.x);
                }
            }
            else
            {
                _animator.ChangeAnimationState(PlayerAnimationState.Idle);
            }
        }

        public void ForceDeadAnimation()
        {
            if (_animator != null && _animator.CurrentState != PlayerAnimationState.Dead)
            {
                _animator.ChangeAnimationState(PlayerAnimationState.Dead);
            }
        }

        public void ResetToIdle()
        {
            if (_animator != null)
            {
                _animator.ChangeAnimationState(PlayerAnimationState.Idle);
            }
        }
    }
}
