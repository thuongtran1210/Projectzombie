using System;
using UnityEngine;

namespace ProjectZombie.Features.Boss
{
    /// <summary>
    /// Component hứng Unity Animation Events từ các Clips hoạt ảnh của Boss.
    /// Tách biệt hoàn toàn visual với game logic (Damage, Slow, VFX Spawns).
    /// Gắn script này vào cùng GameObject chứa Animator Component.
    /// </summary>
    public class BossAnimationEventHandler : MonoBehaviour
    {
        /// <summary>
        /// Sự kiện phát ra tại frame vũ khí/chân Boss chạm đất hoặc chạm mục tiêu
        /// </summary>
        public event Action OnHitFrame;

        /// <summary>
        /// Sự kiện phát ra tại frame khởi tạo hiệu ứng kỹ năng / projectile / AOE vệt đỏ
        /// </summary>
        public event Action OnVFXSpawnFrame;

        /// <summary>
        /// Sự kiện phát ra tại frame bắt đầu lướt
        /// </summary>
        public event Action OnDashStartFrame;

        /// <summary>
        /// Sự kiện phát ra tại frame kết thúc lướt
        /// </summary>
        public event Action OnDashEndFrame;

        /// <summary>
        /// Sự kiện phát ra khi animation hoàn tất dòng đời
        /// </summary>
        public event Action OnAnimationFinished;

        // --- Hàm public được gọi trực tiếp từ Unity Animation Event Window ---

        public void AnimEvent_OnHit()
        {
            OnHitFrame?.Invoke();
        }

        public void AnimEvent_OnVFXSpawn()
        {
            OnVFXSpawnFrame?.Invoke();
        }

        public void AnimEvent_OnDashStart()
        {
            OnDashStartFrame?.Invoke();
        }

        public void AnimEvent_OnDashEnd()
        {
            OnDashEndFrame?.Invoke();
        }

        public void AnimEvent_OnFinished()
        {
            OnAnimationFinished?.Invoke();
        }
    }
}
