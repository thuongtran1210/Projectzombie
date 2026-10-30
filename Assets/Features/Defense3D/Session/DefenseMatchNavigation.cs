using System;
using System.Threading.Tasks;
using ProjectZombie.Core.Architecture;
using ProjectZombie.Features.Maps;
using ProjectZombie.Features.Defense3D.Loadout;

namespace ProjectZombie.Features.Defense3D
{
    /// <summary>Compatibility facade; all transitions belong to the application flow.</summary>
    public static class DefenseMatchNavigation
    {
        public static bool IsNavigating => AppCompositionRoot.Instance != null && AppCompositionRoot.Instance.Flow.IsNavigating;

        public static Task<bool> LaunchDefenseMatchAsync(DefenseLoadoutSnapshot snapshot, Action<float, string> reportProgress = null)
        {
            var flow = AppCompositionRoot.Instance?.Flow;
            return LaunchDefenseMatchAsync(flow?.SelectedStage, snapshot, reportProgress);
        }

        public static async Task<bool> LaunchDefenseMatchAsync(StageDefinitionSO stage, DefenseLoadoutSnapshot snapshot, Action<float, string> reportProgress = null)
        {
            var flow = AppCompositionRoot.Instance?.Flow;
            if (flow == null || stage == null || !stage.CanLaunchDefense) return false;
            var request = new MatchLaunchRequest(stage, snapshot);
            reportProgress?.Invoke(0f, "Đang chuẩn bị xuất trận...");
            // The requesting scene may be destroyed during this await. Application loading owns the UI.
            return await flow.EnterMatchAsync(request);
        }

        public static void ResetStateForTesting() => DefenseSessionContext.Clear();
    }
}
