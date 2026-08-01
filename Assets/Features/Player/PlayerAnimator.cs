using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Player
{
    // Danh sách các trạng thái có thể có của Player.
    // Tên Enum nên tương ứng với tên của các Clip/State bên trong Animator.
    public enum PlayerAnimationState
    {
        Idle,
        Run,
        Dash,
        Attack,
        Dead
    }

    /// <summary>
    /// Kịch bản quản lý Animation cho Player một cách sạch sẽ, không dùng Mũi tên (Transitions).
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Kéo thả GameObject chứa Animator (hình ảnh nhân vật) vào đây")]
        [SerializeField] private Animator animator;

        private PlayerAnimationState _currentState;
        private readonly Dictionary<PlayerAnimationState, int> _stateHashes = new Dictionary<PlayerAnimationState, int>();

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            // Cache các hash của Enum State để triệt tiêu GC Allocation do ToString() khi chuyển animation state
            _stateHashes[PlayerAnimationState.Idle] = Animator.StringToHash(nameof(PlayerAnimationState.Idle));
            _stateHashes[PlayerAnimationState.Run] = Animator.StringToHash(nameof(PlayerAnimationState.Run));
            _stateHashes[PlayerAnimationState.Dash] = Animator.StringToHash(nameof(PlayerAnimationState.Dash));
            _stateHashes[PlayerAnimationState.Attack] = Animator.StringToHash(nameof(PlayerAnimationState.Attack));
            _stateHashes[PlayerAnimationState.Dead] = Animator.StringToHash(nameof(PlayerAnimationState.Dead));
        }

        /// <summary>
        /// Yêu cầu chuyển sang một trạng thái hoạt ảnh mới.
        /// Sử dụng kỹ thuật animator.Play() trực tiếp bằng Hash để BỎ QUA hệ thống mũi tên rối rắm và tối ưu 0 GC.
        /// </summary>
        public void ChangeAnimationState(PlayerAnimationState newState)
        {
            if (animator == null) return;

            // Nếu đang ở đúng State này rồi thì bỏ qua, không play lại từ đầu để tránh giật hình
            if (_currentState == newState) return;

            if (_stateHashes.TryGetValue(newState, out int stateHash))
            {
                animator.Play(stateHash);
            }
            else
            {
                animator.Play(newState.ToString());
            }

            _currentState = newState;
        }

        /// <summary>
        /// Xoay mặt nhân vật trái/phải dựa vào hướng Input (trục X)
        /// </summary>
        public void FlipToDirection(float inputX)
        {
            if (animator == null) return;

            Transform visualTransform = animator.transform;
            if (inputX > 0.01f)
            {
                visualTransform.localScale = new Vector3(1f, 1f, 1f); // Xoay phải
            }
            else if (inputX < -0.01f)
            {
                visualTransform.localScale = new Vector3(-1f, 1f, 1f); // Xoay trái
            }
        }

        public PlayerAnimationState CurrentState => _currentState;
    }
}
