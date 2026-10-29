using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// [VÍ TÀI NGUYÊN GỖ - KINH TẾ TRẬN ĐẤU]
    /// Quản lý số lượng Gỗ (Cổ Mộc) trong suốt phiên phòng thủ 3D:
    /// - Quái bị hạ gục rơi ra khối Gỗ vật lý -> Tướng nhặt thì được cộng vào ví qua `CollectWood()`.
    /// - Khi xây mới hoặc nâng cấp tháp canh tại các Socket -> Trừ tiền qua `TrySpend()`.
    /// </summary>
    public sealed class DefenseWoodWallet3D : MonoBehaviour
    {
        [Header("=== THÔNG SỐ KHO TÀI NGUYÊN GỖ ===")]
        [Tooltip("Số Gỗ khởi điểm người chơi nhận được ngay khi trận đấu bắt đầu (Mặc định: 100 Gỗ để xây tháp đầu tiên)")]
        [SerializeField, Min(0)] private int startingWood = 100;

        [Header("── EVENT CHANNELS (Kênh Sự Kiện Decoupling) ──")]
        [Tooltip("Kênh sự kiện phát ra số lượng Gỗ hiện tại để HUD hiển thị")]
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
