using UnityEngine;

namespace ProjectZombie.Features.Shared
{
    /// <summary>
    /// Chứa toàn bộ các hằng số tên và Hash của Animation States trong game.
    /// Giúp loại bỏ hoàn toàn Magic Strings và tối ưu hóa 0 GC Allocation.
    /// </summary>
    public static class AnimationConstants
    {
        // String State Names
        public const string IDLE = "Idle";
        public const string RUN = "Run";
        public const string ATTACK = "Attack";
        public const string DASH = "Dash";
        public const string GROUND_SLAM = "GroundSlam";
        public const string DEAD = "Dead";
        public const string REVIVE = "Revive";

        // Precalculated Hash IDs
        public static readonly int IdleHash = Animator.StringToHash(IDLE);
        public static readonly int RunHash = Animator.StringToHash(RUN);
        public static readonly int AttackHash = Animator.StringToHash(ATTACK);
        public static readonly int DashHash = Animator.StringToHash(DASH);
        public static readonly int GroundSlamHash = Animator.StringToHash(GROUND_SLAM);
        public static readonly int DeadHash = Animator.StringToHash(DEAD);
        public static readonly int ReviveHash = Animator.StringToHash(REVIVE);
    }
}
