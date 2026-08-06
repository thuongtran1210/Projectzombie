using UnityEngine;

namespace ProjectZombie.Features.Enemies
{
    public enum EnemyAnimationState 
    { 
        Idle, 
        Run, 
        Attack, 
        Dead,
        Revive
    }

    /// <summary>
    /// Điều khiển Animator cho quái vật (Slime).
    /// Tuân thủ nguyên tắc Animator State Machine by Script (không dùng Transitions).
    /// </summary>
    public class EnemyAnimator : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Kéo thả GameObject con chứa Animator vào đây")]
        [SerializeField] private Animator animator;

        [Header("State Names (Must match Animator Exact Names)")]
        [SerializeField] private string idleStateName = "Idle";
        [SerializeField] private string runStateName = "Run";
        [SerializeField] private string attackStateName = "Attack";
        [SerializeField] private string deadStateName = "Dead";
        [SerializeField] private string reviveStateName = "Revive";

        private int _idleHash;
        private int _runHash;
        private int _attackHash;
        private int _deadHash;
        private int _reviveHash;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            // Cache các hash để gọi animator.Play() hiệu quả hơn
            _idleHash = Animator.StringToHash(idleStateName);
            _runHash = Animator.StringToHash(runStateName);
            _attackHash = Animator.StringToHash(attackStateName);
            _deadHash = Animator.StringToHash(deadStateName);
            _reviveHash = Animator.StringToHash(reviveStateName);
        }

        public void PlayState(EnemyAnimationState state)
        {
            if (animator == null || !animator.gameObject.activeInHierarchy) return;

            switch (state)
            {
                case EnemyAnimationState.Idle:
                    animator.Play(_idleHash);
                    break;
                case EnemyAnimationState.Run:
                    animator.Play(_runHash);
                    break;
                case EnemyAnimationState.Attack:
                    animator.Play(_attackHash);
                    break;
                case EnemyAnimationState.Dead:
                    animator.Play(_deadHash);
                    break;
                case EnemyAnimationState.Revive:
                    animator.Play(_reviveHash);
                    break;
            }
        }

        // --- Helper methods để tương thích với FSM hiện tại ---
        // FSM đã kiểm soát logic chặt chẽ nên ta chỉ cần map sang PlayState.

        public void SetRunning(bool isRunning)
        {
            PlayState(isRunning ? EnemyAnimationState.Run : EnemyAnimationState.Idle);
        }

        public void TriggerAttack()
        {
            PlayState(EnemyAnimationState.Attack);
        }

        public void TriggerDeath()
        {
            PlayState(EnemyAnimationState.Dead);
        }

        public void TriggerRevive()
        {
            PlayState(EnemyAnimationState.Revive);
        }

        /// <summary>
        /// Sự kiện này sẽ được gọi từ Animation Event.
        /// </summary>
        public event System.Action OnAttackEvent;

        /// <summary>
        /// Gắn hàm này vào Animation Event trên frame vũ khí chạm đất/kẻ địch.
        /// </summary>
        public void TriggerAttackEvent()
        {
            OnAttackEvent?.Invoke();
        }

        /// <summary>
        /// Xoay mặt quái vật dựa trên hướng di chuyển (trái/phải)
        /// </summary>
        public void FlipToDirection(float velocityX)
        {
            if (animator != null && animator.gameObject.activeInHierarchy)
            {
                Transform visualTransform = animator.transform;
                if (velocityX > 0.1f)
                {
                    visualTransform.localScale = new Vector3(1f, 1f, 1f); // Quay phải
                }
                else if (velocityX < -0.1f)
                {
                    visualTransform.localScale = new Vector3(-1f, 1f, 1f); // Quay trái
                }
            }
        }
    }
}
