using System.Collections.Generic;
using UnityEngine;

namespace ProjectZombie.Features.Boss
{
    /// <summary>
    /// Component điều khiển Animator cho các loại Boss trong game.
    /// Tuân thủ quy chuẩn Animator State Machine by Script (Rule 10): 
    /// - Không dùng Animator Transitions (mũi tên)
    /// - Cache String-to-Hash tự động linh hoạt (0 GC Allocation)
    /// - Hỗ trợ AnimatorOverrideController khi Boss đổi Phase
    /// </summary>
    public class BossAnimator : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Kéo thả Animator vào đây (nếu để trống sẽ tự tìm trong GameObject con)")]
        [SerializeField] private Animator animator;

        private readonly Dictionary<string, int> _animationHashes = new();
        private string _currentStateName = string.Empty;

        public Animator Animator => animator;
        public string CurrentStateName => _currentStateName;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            // Mặc định cache một số animation name chuẩn cơ bản
            CacheAnimationHash("Idle");
            CacheAnimationHash("Run");
            CacheAnimationHash("Attack");
            CacheAnimationHash("Dash");
            CacheAnimationHash("GroundSlam");
            CacheAnimationHash("Dead");
            CacheAnimationHash("Revive");
        }

        /// <summary>
        /// Thêm trước name hash để tối ưu 0 GC khi phát hoạt ảnh về sau
        /// </summary>
        public int CacheAnimationHash(string stateName)
        {
            if (string.IsNullOrEmpty(stateName)) return 0;

            if (!_animationHashes.TryGetValue(stateName, out int hash))
            {
                hash = Animator.StringToHash(stateName);
                _animationHashes[stateName] = hash;
            }
            return hash;
        }

        /// <summary>
        /// Chuyển animation trực tiếp bằng State Name Hash (0 GC, Bỏ qua Animator Transitions)
        /// </summary>
        public void PlayAnimation(string stateName, bool forceReplay = false)
        {
            if (animator == null || !animator.gameObject.activeInHierarchy || string.IsNullOrEmpty(stateName)) return;

            if (!forceReplay && _currentStateName == stateName) return;

            int hash = CacheAnimationHash(stateName);
            _currentStateName = stateName;
            animator.Play(hash, 0, 0f);
        }

        /// <summary>
        /// Thay đổi tốc độ animation của Boss (ví dụ khi Boss vào trạng thái Cuồng Nộ)
        /// </summary>
        public void SetAnimationSpeed(float speedMultiplier)
        {
            if (animator != null)
            {
                animator.speed = speedMultiplier;
            }
        }

        /// <summary>
        /// Đổi toàn bộ bộ animation của Boss khi chuyển Phase (sử dụng AnimatorOverrideController)
        /// </summary>
        public void ApplyPhaseOverride(AnimatorOverrideController overrideController)
        {
            if (animator != null && overrideController != null)
            {
                animator.runtimeAnimatorController = overrideController;
            }
        }

        /// <summary>
        /// Quay mặt Boss theo hướng di chuyển (Flip Scale X)
        /// </summary>
        public void FlipToDirection(float velocityX)
        {
            if (animator != null && animator.gameObject.activeInHierarchy)
            {
                Transform visualTransform = animator.transform;
                if (velocityX > 0.1f)
                {
                    visualTransform.localScale = new Vector3(1f, 1f, 1f);
                }
                else if (velocityX < -0.1f)
                {
                    visualTransform.localScale = new Vector3(-1f, 1f, 1f);
                }
            }
        }
    }
}
