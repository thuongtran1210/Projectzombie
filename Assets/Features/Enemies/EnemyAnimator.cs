using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Shared;

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
    /// Điều khiển Animator cho quái vật (Enemy).
    /// Tuân thủ nguyên tắc Animator State Machine by Script (không dùng Transitions).
    /// Implement ICharacterAnimator để tương thích đa hình toàn hệ thống.
    /// </summary>
    public class EnemyAnimator : MonoBehaviour, ICharacterAnimator
    {
        [Header("References")]
        [Tooltip("Kéo thả GameObject con chứa Animator vào đây")]
        [SerializeField] private Animator animator;

        [Header("State Names (Must match Animator Exact Names)")]
        [SerializeField] private string idleStateName = AnimationConstants.IDLE;
        [SerializeField] private string runStateName = AnimationConstants.RUN;
        [SerializeField] private string attackStateName = AnimationConstants.ATTACK;
        [SerializeField] private string deadStateName = AnimationConstants.DEAD;
        [SerializeField] private string reviveStateName = AnimationConstants.REVIVE;

        private readonly Dictionary<string, int> _stateHashes = new();
        private string _currentStateName = string.Empty;

        public Animator AnimatorComponent => animator;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            CacheAnimationHash(idleStateName);
            CacheAnimationHash(runStateName);
            CacheAnimationHash(attackStateName);
            CacheAnimationHash(deadStateName);
            CacheAnimationHash(reviveStateName);
        }

        public int CacheAnimationHash(string stateName)
        {
            if (string.IsNullOrEmpty(stateName)) return 0;

            if (!_stateHashes.TryGetValue(stateName, out int hash))
            {
                hash = Animator.StringToHash(stateName);
                _stateHashes[stateName] = hash;
            }
            return hash;
        }

        public void PlayState(EnemyAnimationState state)
        {
            switch (state)
            {
                case EnemyAnimationState.Idle:
                    PlayAnimation(idleStateName);
                    break;
                case EnemyAnimationState.Run:
                    PlayAnimation(runStateName);
                    break;
                case EnemyAnimationState.Attack:
                    PlayAnimation(attackStateName, true);
                    break;
                case EnemyAnimationState.Dead:
                    PlayAnimation(deadStateName);
                    break;
                case EnemyAnimationState.Revive:
                    PlayAnimation(reviveStateName);
                    break;
            }
        }

        public void PlayAnimation(string stateName, bool forceReplay = false)
        {
            if (animator == null || !animator.gameObject.activeInHierarchy || string.IsNullOrEmpty(stateName)) return;

            if (!forceReplay && _currentStateName == stateName) return;

            int hash = CacheAnimationHash(stateName);
            _currentStateName = stateName;
            animator.Play(hash, 0, 0f);
        }

        public void SetRunning(bool isRunning)
        {
            PlayAnimation(isRunning ? runStateName : idleStateName);
        }

        public void TriggerAttack()
        {
            PlayAnimation(attackStateName, true);
        }

        public void TriggerDeath()
        {
            PlayAnimation(deadStateName);
        }

        public void TriggerRevive()
        {
            PlayAnimation(reviveStateName);
        }

        public void SetAnimationSpeed(float speedMultiplier)
        {
            if (animator != null)
            {
                animator.speed = Mathf.Clamp(speedMultiplier, 0.2f, 3.0f);
            }
        }

        public void SetAttackAnimationSpeed(float speedMultiplier) => SetAnimationSpeed(speedMultiplier);

        public float GetCurrentClipLength(string stateName, float defaultFallback = 0.5f)
        {
            if (animator == null || animator.runtimeAnimatorController == null) return defaultFallback;

            var clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo != null && clipInfo.Length > 0 && clipInfo[0].clip != null)
            {
                if (string.IsNullOrEmpty(stateName) || clipInfo[0].clip.name.IndexOf(stateName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return clipInfo[0].clip.length;
                }
            }

            var clips = animator.runtimeAnimatorController.animationClips;
            if (clips != null)
            {
                for (int i = 0; i < clips.Length; i++)
                {
                    if (clips[i] != null && clips[i].name.IndexOf(stateName, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return clips[i].length;
                    }
                }
            }

            return defaultFallback;
        }

        public float GetCurrentAttackClipLength(float defaultFallback = 0.5f) => GetCurrentClipLength(attackStateName, defaultFallback);

        /// <summary>
        /// Sự kiện này sẽ được gọi từ Animation Event khi ra đòn.
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
        /// Fallback cho các clip có Event đặt tên là AnimEvent_OnHit
        /// </summary>
        public void AnimEvent_OnHit()
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
