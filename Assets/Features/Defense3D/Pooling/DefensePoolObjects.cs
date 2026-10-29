using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Hủy object đúng môi trường; hỗ trợ Unit Test Edit Mode mà không chạy game.</summary>
    internal static class DefensePoolObjects
    {
        public static void Destroy(Object instance)
        {
            if (instance == null) return;
            if (Application.isPlaying) Object.Destroy(instance);
            else Object.DestroyImmediate(instance);
        }
    }
}
