using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Giao diện trừu tượng hóa cho hệ thống sinh lộ trình đường đi của quái vật (SOLID: Dependency Inversion & Open/Closed).
    /// Cho phép các bản đồ hoặc các màn chơi (Stage) khác nhau tự định nghĩa các lộ trình Waypoints tùy ý.
    /// </summary>
    public interface IDefenseRouteProvider
    {
        /// <summary>Sinh danh sách các điểm mốc từ điểm xuất phát đến mục tiêu.</summary>
        Vector3[] Create(int lane, Vector3 origin, Vector3 destination);

        /// <summary>Sinh danh sách các điểm mốc tiêu chuẩn của làn đường.</summary>
        Vector3[] Create(int lane);

        /// <summary>Tọa độ trục Z tương ứng với làn đường.</summary>
        float LaneZ(int lane);

        /// <summary>Tọa độ điểm đón vào cầu / vào làn đường.</summary>
        Vector3 EntryPoint(int lane);
    }
}

