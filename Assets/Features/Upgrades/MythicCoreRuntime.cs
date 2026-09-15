using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Combat.Events;

namespace ProjectZombie.Features.Upgrades
{
    /// <summary>
    /// Abstract Base Class quản lý toàn bộ vòng đời và logic thực thi chiến đấu của Lõi Thần Thoại.
    /// Đảm bảo dọn dẹp sạch sẽ (Teardown) khi Đổi Lõi, Restart trận đấu, Player chết hoặc Đổi Scene.
    /// </summary>
    public abstract class MythicCoreRuntime : MonoBehaviour
    {
        public abstract MythicArchetype Archetype { get; }

        public bool IsActive { get; private set; }
        protected PlayerContext Context { get; private set; }

        private bool _isDisposed = false;

        /// <summary>
        /// Khởi tạo Core với PlayerContext được tiêm từ Player Root.
        /// </summary>
        public void Initialize(PlayerContext context)
        {
            if (IsActive || _isDisposed) return;

            Context = context;
            _isDisposed = false;
            IsActive = true;

            OnInitialize();
            SubscribeCombatEvents();

            if (Context?.CombatEvents != null)
            {
                Context.CombatEvents.OnPlayerRevived += HandlePlayerRevived;
            }

            Debug.Log($"<color=#00FF88>[{GetType().Name}] Initialized cho Archetype: {Archetype}.</color>");
        }

        /// <summary>
        /// Dọn dẹp triệt để Core Runtime (Idempotent - An toàn khi gọi nhiều lần).
        /// </summary>
        public void Teardown()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            IsActive = false;

            if (Context?.CombatEvents != null)
            {
                Context.CombatEvents.OnPlayerRevived -= HandlePlayerRevived;
            }

            UnsubscribeCombatEvents();
            OnTeardown();

            Context = null;
            Debug.Log($"<color=#FF4444>[{GetType().Name}] Teardown & Cleaned up hoàn tất.</color>");
        }

        private void OnDestroy()
        {
            if (!_isDisposed)
            {
                Teardown();
            }
        }

        private void HandlePlayerRevived(in ReviveEvent e)
        {
            OnRevived(e);
        }

        // --- Template Methods cho các Lõi Con kế thừa ---
        protected virtual void OnInitialize() { }
        protected virtual void OnTeardown() { }
        protected virtual void OnRevived(in ReviveEvent e) { }
        protected abstract void SubscribeCombatEvents();
        protected abstract void UnsubscribeCombatEvents();
    }
}
