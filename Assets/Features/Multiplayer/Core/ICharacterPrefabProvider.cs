using UnityEngine;
using Fusion;

namespace ProjectZombie.Features.Multiplayer.Core
{
    /// <summary>
    /// Hợp đồng cung cấp Network Prefab của nhân vật dựa trên mã định danh (DIP & OCP).
    /// Tách việc truy xuất dữ liệu CharacterDatabase khỏi logic Spawner.
    /// </summary>
    public interface ICharacterPrefabProvider
    {
        /// <summary>
        /// Lấy NetworkObject Prefab tương ứng với mã nhân vật.
        /// </summary>
        NetworkObject GetNetworkCharacterPrefab(string characterId);

        /// <summary>
        /// Lấy NetworkObject Prefab mặc định làm fallback.
        /// </summary>
        NetworkObject GetDefaultNetworkCharacterPrefab();
    }
}
