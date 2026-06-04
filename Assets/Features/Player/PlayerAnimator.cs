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

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        /// <summary>
        /// Yêu cầu chuyển sang một trạng thái hoạt ảnh mới.
        /// Sử dụng kỹ thuật animator.Play() trực tiếp bằng Hash để BỎ QUA hệ thống mũi tên rối rắm.
        /// </summary>
        public void ChangeAnimationState(PlayerAnimationState newState)
        {
            if (animator == null) return;

            // Nếu đang ở đúng State này rồi thì bỏ qua, không play lại từ đầu để tránh giật hình
            if (_currentState == newState) return;

            // Dùng tên của Enum để làm tên State trong Animator.
            // Ví dụ Enum là "Run" thì trong Animator phải có hộp tên là "Run".
            animator.Play(newState.ToString());

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
