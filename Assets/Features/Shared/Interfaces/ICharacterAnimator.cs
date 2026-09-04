using UnityEngine;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Interface thống nhất cho toàn bộ hệ thống Animator của Nhân vật, Quái vật và Boss.
    /// Tuân thủ nguyên lý Open-Closed Principle (OCP) và Dependency Inversion Principle (DIP).
    /// </summary>
    public interface ICharacterAnimator
    {
        /// <summary>
        /// Component Animator thực tế của Unity.
        /// </summary>
        Animator AnimatorComponent { get; }

        /// <summary>
        /// Chuyển đổi trạng thái di chuyển (Run) và đứng yên (Idle).
        /// </summary>
        void SetRunning(bool isRunning);

        /// <summary>
        /// Kích hoạt hoạt ảnh tấn công (Attack).
        /// </summary>
        void TriggerAttack();

        /// <summary>
        /// Kích hoạt hoạt ảnh tử trận (Dead).
        /// </summary>
        void TriggerDeath();

        /// <summary>
        /// Kích hoạt hoạt ảnh hồi sinh (Revive).
        /// </summary>
        void TriggerRevive();

        /// <summary>
        /// Phát hoạt ảnh tùy biến theo tên State.
        /// </summary>
        void PlayAnimation(string stateName, bool forceReplay = false);

        /// <summary>
        /// Xoay lật nhân vật/quái vật theo hướng di chuyển (Scale X).
        /// </summary>
        void FlipToDirection(float velocityX);

        /// <summary>
        /// Đồng bộ tốc độ phát hoạt ảnh.
        /// </summary>
        void SetAnimationSpeed(float speedMultiplier);

        /// <summary>
        /// Lấy thời lượng thực tế của clip animation theo tên (tính bằng giây).
        /// </summary>
        float GetCurrentClipLength(string stateName, float defaultFallback = 0.5f);
    }
}
