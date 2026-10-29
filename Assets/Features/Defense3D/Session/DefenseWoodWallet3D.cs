using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>One-run Wood wallet. Only physical WoodDrop pickups can grant Wood.</summary>
    public sealed class DefenseWoodWallet3D : MonoBehaviour
    {
        [SerializeField, Min(0)] private int startingWood = 0;

        [Header("── EVENT CHANNELS (Sổ tay Kiến trúc 3D - Tùy chọn) ──")]
        [Tooltip("Kênh sự kiện phát ra số lượng Cổ Mộc (Gỗ) hiện tại")]
        [SerializeField] private SO_IntEvent _onWoodChangedEvent;

        public int Wood { get; private set; }
        public int CurrentWood => Wood;
        public event System.Action Changed;

        private void Awake()
        {
            ResetWallet();
        }

        /// <summary>Restores the authored initial balance for a new run.</summary>
        public void ResetWallet()
        {
            Wood = startingWood;
            Changed?.Invoke();
            _onWoodChangedEvent?.Raise(Wood);
        }

        public void CollectWood(int amount)
        {
            if (amount <= 0) return;
            Wood = amount > int.MaxValue - Wood ? int.MaxValue : Wood + amount;
            Changed?.Invoke();
            _onWoodChangedEvent?.Raise(Wood);
            Debug.Log("[TREBRAVE Defense3D] Hero collected " + amount + " Wood. Wallet: " + Wood);
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0 || Wood < amount) return false;
            Wood -= amount;
            Changed?.Invoke();
            _onWoodChangedEvent?.Raise(Wood);
            return true;
        }
    }
}
