using UnityEngine;

namespace ProjectZombie.Features.Collectibles
{
    /// <summary>
    /// Interface chung cho tất cả các vật phẩm nhặt được trên sàn đấu.
    /// Cho phép PlayerMagnetTrigger và các hệ thống nam châm toàn bản đồ tương tác đa hình.
    /// </summary>
    public interface ICollectible
    {
        /// <summary>
        /// Vật phẩm đang ở trạng thái nhàn rỗi hoặc rơi trên mặt đất, sẵn sàng được hút.
        /// </summary>
        bool IsActiveOnGround { get; }

        /// <summary>
        /// Kích hoạt hiệu ứng bay / hút về phía đối tượng chỉ định (Player).
        /// </summary>
        void StartMagnetEffect(Transform target);

        /// <summary>
        /// Thu thập vật phẩm khi đã bay chạm tới Player.
        /// </summary>
        void Collect();
    }
}
