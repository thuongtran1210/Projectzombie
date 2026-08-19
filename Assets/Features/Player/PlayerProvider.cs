using System;
using UnityEngine;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// Service trung tâm cung cấp tham chiếu Player tập trung theo chuẩn Event-Driven (Mục 4 AGENTS.md).
    /// Loại bỏ hoàn toàn chi phí quét chuỗi và race condition của GameObject.FindGameObjectWithTag("Player").
    /// </summary>
    public static class PlayerProvider
    {
        public static Transform PlayerTransform { get; private set; }
        public static HealthSystem PlayerHealth { get; private set; }
        public static GameObject PlayerGameObject => PlayerTransform != null ? PlayerTransform.gameObject : null;

        public static bool HasPlayer => PlayerTransform != null && PlayerHealth != null;

        public static event Action<Transform, HealthSystem> OnPlayerSpawned;
        public static event Action OnPlayerDespawned;

        /// <summary>
        /// Đăng ký thực thể người chơi mới sinh ra từ GameplayBootstrapper.
        /// </summary>
        public static void RegisterPlayer(GameObject playerInstance)
        {
            if (playerInstance == null)
            {
                ClearPlayer();
                return;
            }

            PlayerTransform = playerInstance.transform;
            PlayerHealth = playerInstance.GetComponent<HealthSystem>();

            OnPlayerSpawned?.Invoke(PlayerTransform, PlayerHealth);
        }

        /// <summary>
        /// Xóa bỏ tham chiếu khi người chơi chết hoặc đổi màn chơi.
        /// </summary>
        public static void ClearPlayer()
        {
            PlayerTransform = null;
            PlayerHealth = null;
            OnPlayerDespawned?.Invoke();
        }
    }
}
