using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Bộ điều phối Lộ trình di chuyển của quái vật trên bản đồ.
    /// Tuân thủ chuẩn SOLID (Open/Closed & Dependency Inversion):
    /// Cho phép nạp nhà cung cấp lộ trình động (IDefenseRouteProvider / DefenseBridgeRouteProfileSO)
    /// và tự động dự phòng sang DefaultRouteProvider cho bản đồ Sông Quê.
    /// </summary>
    public static class DefenseBridgeRoute3D
    {
        private static IDefenseRouteProvider _provider;

        public static void SetProvider(IDefenseRouteProvider provider)
        {
            _provider = provider;
        }

        public static void ResetProvider()
        {
            _provider = null;
        }

        private static IDefenseRouteProvider ActiveProvider => _provider ?? DefaultRouteProvider.Instance;

        public static Vector3[] Create(int lane, Vector3 origin, Vector3 destination) =>
            ActiveProvider.Create(lane, origin, destination);

        public static Vector3[] Create(int lane) =>
            ActiveProvider.Create(lane);

        public static float LaneZ(int lane) =>
            ActiveProvider.LaneZ(lane);

        public static Vector3 EntryPoint(int lane) =>
            ActiveProvider.EntryPoint(lane);

        private sealed class DefaultRouteProvider : IDefenseRouteProvider
        {
            public static readonly DefaultRouteProvider Instance = new DefaultRouteProvider();

            public float LaneZ(int lane) => lane == 0 ? -10.5f : lane == 2 ? 10.5f : -0.5f;

            public Vector3 EntryPoint(int lane) => new Vector3(8.8f, 0f, LaneZ(lane));

            public Vector3[] Create(int lane)
            {
                float z = LaneZ(lane);
                return new[]
                {
                    EntryPoint(lane),
                    new Vector3(6.25f, 0f, z),
                    new Vector3(3.8f, 0f, z),
                    new Vector3(-1.8f, 0f, z),
                    new Vector3(-7.5f, 0f, z * 0.6f),
                    new Vector3(-12f, 0f, 0f)
                };
            }

            public Vector3[] Create(int lane, Vector3 origin, Vector3 destination)
            {
                if (origin.x <= 4.45f) return new[] { destination };
                var route = Create(lane);
                route[route.Length - 1] = destination;
                return route;
            }
        }
    }
}
