using System.Threading.Tasks;
using UnityEngine;

namespace ProjectZombie.Core.Services.Addressables
{
    /// <summary>
    /// Giao diện tầng dịch vụ quản lý và nạp tài nguyên bất đồng bộ qua Addressable Asset System.
    /// </summary>
    public interface IAssetProvider
    {
        /// <summary>
        /// Tải bất đồng bộ một Asset vào bộ nhớ RAM và tự động cache lại Handle.
        /// </summary>
        /// <typeparam name="T">Loại UnityEngine.Object cần load (GameObject, Sprite, AudioClip, SO...).</typeparam>
        /// <param name="address">Địa chỉ tên gợi nhớ của Asset trong Addressables Groups.</param>
        /// <returns>Đối tượng Asset đã nạp thành công hoặc null nếu thất bại.</returns>
        Task<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object;

        /// <summary>
        /// Khởi tạo trực tiếp một GameObject từ địa chỉ Addressable.
        /// </summary>
        Task<GameObject> InstantiateAsync(string address, Vector3 position, Quaternion rotation, Transform parent = null);

        /// <summary>
        /// Giải phóng một Asset cụ thể khỏi bộ nhớ RAM khi không còn sử dụng.
        /// </summary>
        /// <param name="address">Địa chỉ Asset cần xả khỏi bộ nhớ.</param>
        void ReleaseAsset(string address);

        /// <summary>
        /// Giải phóng toàn bộ Asset đã cache (thường gọi khi đổi Scene hoặc kết thúc trận đấu).
        /// </summary>
        void Cleanup();
    }
}
