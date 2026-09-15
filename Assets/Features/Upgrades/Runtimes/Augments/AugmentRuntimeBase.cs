using System;
using UnityEngine;
using ProjectZombie.Features.Player;
using ProjectZombie.Features.Combat;
using ProjectZombie.Features.Combat.Events;

namespace ProjectZombie.Features.Upgrades.Runtimes.Augments
{
    /// <summary>
    /// Base class cho các Runtime Component của Lõi Đột Biến (Mutation Augment).
    /// Tự động lấy PlayerContext từ parent GameObject, quản lý đăng ký/hủy sự kiện chiến đấu và dọn dẹp sạch sẽ.
    /// </summary>
    public abstract class AugmentRuntimeBase : MonoBehaviour
    {
        public bool IsActive { get; private set; }
        protected PlayerContext Context { get; private set; }
        private bool _isDisposed = false;

        protected virtual void Awake()
        {
            InitializeContext();
        }

        protected virtual void Start()
        {
            if (!IsActive && !_isDisposed)
            {
                InitializeContext();
            }
        }

        public void InitializeContext()
        {
            if (IsActive || _isDisposed) return;

            var rootObj = transform.root != null ? transform.root.gameObject : gameObject;
            Context = PlayerContext.Create(rootObj);

            IsActive = true;
            SubscribeEvents();
            OnAugmentInitialized();
        }

        public void Teardown()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            IsActive = false;

            UnsubscribeEvents();
            OnAugmentTeardown();
            Context = null;
        }

        protected virtual void OnDestroy()
        {
            if (!_isDisposed)
            {
                Teardown();
            }
        }

        protected virtual void SubscribeEvents()
        {
            if (Context?.CombatEvents != null)
            {
                SubscribeCombatEvents(Context.CombatEvents);
            }
        }

        protected virtual void UnsubscribeEvents()
        {
            if (Context?.CombatEvents != null)
            {
                UnsubscribeCombatEvents(Context.CombatEvents);
            }
        }

        protected virtual void OnAugmentInitialized() { }
        protected virtual void OnAugmentTeardown() { }
        protected virtual void SubscribeCombatEvents(PlayerCombatEvents events) { }
        protected virtual void UnsubscribeCombatEvents(PlayerCombatEvents events) { }
    }
}
