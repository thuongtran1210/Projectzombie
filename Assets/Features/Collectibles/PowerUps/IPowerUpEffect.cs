using UnityEngine;

namespace ProjectZombie.Features.Collectibles.PowerUps
{
    /// <summary>
    /// Giao diện Strategy cho hiệu ứng của các vật phẩm bổ trợ (Power-up) xuất hiện trong trận chiến.
    /// Giúp tách rời hoàn toàn logic tác động (Hồi máu, Tăng tốc...) khỏi đối tượng hiển thị (PowerUpItem).
    /// </summary>
    public interface IPowerUpEffect
    {
        /// <summary>
        /// Kích hoạt hiệu ứng lên đối tượng người chơi (Player).
        /// </summary>
        /// <param name="player">GameObject của nhân vật đang chơi</param>
        void Apply(GameObject player);
    }
}
