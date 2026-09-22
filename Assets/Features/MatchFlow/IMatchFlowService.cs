using System;
using System.Threading.Tasks;
using UnityEngine;
using ProjectZombie.Features.Maps;
using ProjectZombie.Features.Player;

namespace ProjectZombie.Features.MatchFlow
{
    /// <summary>
    /// Contract dịch vụ điều phối luồng chuẩn bị và vòng đời trận đấu (Match Flow).
    /// Hỗ trợ Dependency Injection và Lifecycle quản lý bản đồ rõ ràng (Mục 3.2 & 3.3 AGENTS.md).
    /// </summary>
    public interface IMatchFlowService
    {
        /// <summary>
        /// Instance bản đồ hiện tại đang hoạt động trên Scene.
        /// </summary>
        GameObject CurrentMapInstance { get; }

        /// <summary>
        /// Thực thi tuần tự chuỗi khởi tạo trận đấu bất đồng bộ.
        /// </summary>
        Task ExecuteCombatPreparationAsync(
            StageDefinitionSO stage,
            GameplayBootstrapper gameplayBootstrapper,
            Action<float, string> reportProgress = null);

        /// <summary>
        /// Hủy và giải phóng bản đồ hiện tại.
        /// </summary>
        void DestroyCurrentMapInstance();
    }
}
