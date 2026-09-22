using System;
using System.Threading.Tasks;
using UnityEngine;
using ProjectZombie.Features.Maps;
using ProjectZombie.Features.Player;
using ProjectZombie.Core.Architecture;

namespace ProjectZombie.Features.MatchFlow
{
    /// <summary>
    /// Facade Adapter tương thích ngược (Backward Compatible) điều phối luồng chuẩn bị trận đấu.
    /// Toàn bộ logic được ủy quyền cho IMatchFlowService đăng ký trong ServiceContext (Mục 3.2 & 3.3 AGENTS.md).
    /// </summary>
    public static class MatchFlowOrchestrator
    {
        private static IMatchFlowService Service =>
            ServiceContext.Get<IMatchFlowService>() ?? MatchFlowService.Default;

        public static GameObject CurrentMapInstance => Service.CurrentMapInstance;

        public static Task ExecuteCombatPreparationAsync(
            StageDefinitionSO stage,
            GameplayBootstrapper gameplayBootstrapper,
            Action<float, string> reportProgress = null)
        {
            return Service.ExecuteCombatPreparationAsync(stage, gameplayBootstrapper, reportProgress);
        }

        public static void DestroyCurrentMapInstance()
        {
            Service.DestroyCurrentMapInstance();
        }
    }
}
