using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Map-specific bridge waypoints, separate from the navigation implementation.</summary>
    public static class DefenseBridgeRoute3D
    {
        public static Vector3[] Create(int lane,Vector3 origin,Vector3 destination)
        {
            if (origin.x<=4.45f) return new[]{destination};
            var route=Create(lane);
            route[route.Length-1]=destination;
            return route;
        }
        public static Vector3[] Create(int lane)
        {
            float z = LaneZ(lane);
            return new[] {
                EntryPoint(lane), new Vector3(6.25f, 0f, z),
                new Vector3(3.8f, 0f, z), new Vector3(-1.8f, 0f, z),
                new Vector3(-7.5f, 0f, z * 0.6f), new Vector3(-12f, 0f, 0f)
            };
        }
        public static float LaneZ(int lane) => lane == 0 ? -10.5f : lane == 2 ? 10.5f : -0.5f;
        public static Vector3 EntryPoint(int lane) => new Vector3(8.8f, 0f, LaneZ(lane));
    }
}
