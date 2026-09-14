using System;
using UnityEngine;
using ProjectZombie.Features.Shared;
using ProjectZombie.Features.Player.Core;
using ProjectZombie.Core.Architecture;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// Facade Adapter tương thích ngược (Backward Compatible) cung cấp tham chiếu Player.
    /// Ủy quyền toàn bộ truy vấn sang IPlayerRegistry đăng ký trong ServiceContext (Mục 3.2 & 3.3 AGENTS.md).
    /// </summary>
    public static class PlayerProvider
    {
        private static IPlayerRegistry _cachedRegistry;

        public static IPlayerRegistry Registry
        {
            get
            {
                if (_cachedRegistry == null)
                {
                    _cachedRegistry = ServiceContext.Get<IPlayerRegistry>();
                    if (_cachedRegistry == null)
                    {
                        _cachedRegistry = new SinglePlayerRegistry();
                        ServiceContext.Register<IPlayerRegistry>(_cachedRegistry);
                    }
                }
                return _cachedRegistry;
            }
        }

        public static Transform PlayerTransform => Registry.LocalPlayer?.Transform;
        public static HealthSystem PlayerHealth => Registry.LocalPlayer?.Health;
        public static GameObject PlayerGameObject => Registry.LocalPlayer?.GameObject;

        public static bool HasPlayer => Registry.HasAnyPlayer && Registry.LocalPlayer?.Transform != null;

        public static event Action<Transform, HealthSystem> OnPlayerSpawned;
        public static event Action OnPlayerDespawned;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetStaticState()
        {
            _cachedRegistry = null;
            OnPlayerSpawned = null;
            OnPlayerDespawned = null;
        }

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

            var context = PlayerContext.Create(playerInstance, isLocal: true);
            Registry.Register(context);

            OnPlayerSpawned?.Invoke(context.Transform, context.Health);
        }

        /// <summary>
        /// Lấy component trên GameObject Player an toàn.
        /// </summary>
        public static T GetPlayerComponent<T>() where T : class
        {
            if (PlayerTransform == null) return null;
            return PlayerTransform.GetComponent<T>();
        }

        /// <summary>
        /// Xóa bỏ tham chiếu khi người chơi chết hoặc đổi màn chơi.
        /// </summary>
        public static void ClearPlayer()
        {
            Registry.Clear();
            OnPlayerDespawned?.Invoke();
        }
    }
}
