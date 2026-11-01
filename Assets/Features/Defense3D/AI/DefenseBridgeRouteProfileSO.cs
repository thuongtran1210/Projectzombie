using System;
using UnityEngine;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>
    /// Bản cấu hình Lộ trình đường đi dạng ScriptableObject (Data-Driven Route Profile).
    /// Cho phép Game Designer tùy biến tọa độ các cây cầu, các khúc cua và điểm đón quái vật
    /// của từng bản đồ hoặc từng ải mà không cần viết lại mã nguồn C#.
    /// </summary>
    [CreateAssetMenu(fileName = "RouteProfile_New", menuName = "Defense3D/Route Profile (Cấu Hình Đường Đi)")]
    public sealed class DefenseBridgeRouteProfileSO : ScriptableObject, IDefenseRouteProvider
    {
        [Header("── 1. CẤU HÌNH TỌA ĐỘ LÀN ĐƯỜNG (Lane Z Coordinates) ──")]
        [Tooltip("Tọa độ Z của Làn 0 (Cầu Bắc / Đường trên)")]
        [SerializeField] private float _northLaneZ = -10.5f;

        [Tooltip("Tọa độ Z của Làn 1 (Cầu Giữa / Đường trung tâm)")]
        [SerializeField] private float _middleLaneZ = -0.5f;

        [Tooltip("Tọa độ Z của Làn 2 (Cầu Nam / Đường dưới)")]
        [SerializeField] private float _southLaneZ = 10.5f;

        [Header("── 2. CÁC MỐC TRỤC X TRÊN ĐƯỜNG TIẾN CÔNG ──")]
        [Tooltip("Tọa độ X điểm đón vào cầu (Bờ Đông)")]
        [SerializeField] private float _eastEntryX = 8.8f;

        [Tooltip("Tọa độ X giữa cầu")]
        [SerializeField] private float _bridgeCenterX = 6.25f;

        [Tooltip("Tọa độ X ra khỏi cầu (Bờ Tây)")]
        [SerializeField] private float _westExitX = 3.8f;

        [Tooltip("Tọa độ X khúc cua tiếp cận sân đình")]
        [SerializeField] private float _approachBendX = -1.8f;

        [Tooltip("Tọa độ X khúc cua sân đình")]
        [SerializeField] private float _courtyardBendX = -7.5f;

        [Tooltip("Hệ số co hẹp trục Z tại khúc cua sân đình (mặc định 0.6)")]
        [SerializeField] private float _courtyardBendFactor = 0.6f;

        [Tooltip("Tọa độ X vị trí Căn cứ Bờ Tây (Mục tiêu cuối)")]
        [SerializeField] private float _baseX = -12f;

        [Tooltip("Ngưỡng giới hạn X bờ Tây để xác định quái đã vượt qua cầu (mặc định 4.45)")]
        [SerializeField] private float _westBankThresholdX = 4.45f;

        public float LaneZ(int lane)
        {
            switch (lane)
            {
                case 0: return _northLaneZ;
                case 2: return _southLaneZ;
                default: return _middleLaneZ;
            }
        }

        public Vector3 EntryPoint(int lane)
        {
            return new Vector3(_eastEntryX, 0f, LaneZ(lane));
        }

        public Vector3[] Create(int lane)
        {
            float z = LaneZ(lane);
            return new[]
            {
                EntryPoint(lane),
                new Vector3(_bridgeCenterX, 0f, z),
                new Vector3(_westExitX, 0f, z),
                new Vector3(_approachBendX, 0f, z),
                new Vector3(_courtyardBendX, 0f, z * _courtyardBendFactor),
                new Vector3(_baseX, 0f, 0f)
            };
        }

        public Vector3[] Create(int lane, Vector3 origin, Vector3 destination)
        {
            if (origin.x <= _westBankThresholdX)
            {
                return new[] { destination };
            }

            var route = Create(lane);
            route[route.Length - 1] = destination;
            return route;
        }
    }
}

