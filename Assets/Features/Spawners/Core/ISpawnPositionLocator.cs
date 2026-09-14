using UnityEngine;

namespace ProjectZombie.Features.Spawners.Core
{
    /// <summary>
    /// Giao diện chuẩn tìm kiếm tọa độ Spawn quái vật hợp lệ.
    /// </summary>
    public interface ISpawnPositionLocator
    {
        /// <summary>
        /// Tìm một vị trí spawn hợp lệ (ngoài camera, nằm trên sàn đi được, không vướng tường).
        /// </summary>
        Vector3 GetSpawnPosition(Transform centerTarget, float minRadius, float maxRadius);
    }
}
