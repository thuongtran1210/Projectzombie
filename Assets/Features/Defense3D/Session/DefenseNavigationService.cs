using System;
using System.Threading.Tasks;
using ProjectZombie.Core.Architecture;

namespace ProjectZombie.Features.Defense3D
{
    public interface IDefenseNavigationService
    {
        bool IsNavigating { get; }
        Task<bool> ReturnToHubAsync(Action onBeforeUnload = null);
    }

    /// <summary>Legacy call adapter. Scene ownership and navigation locks live in AppFlow.</summary>
    public sealed class DefenseNavigationService : IDefenseNavigationService
    {
        public const string HUB_SCENE_NAME = "MainMenu";
        public const string LEGACY_HUB_SCENE_NAME = "SampleScene";
        private static DefenseNavigationService _default;
        public static DefenseNavigationService Default => _default ??= new DefenseNavigationService();
        public static Action<string> LogWarningHook { get; set; }
        public static Action<string> LogErrorHook { get; set; }
        public bool IsNavigating => AppCompositionRoot.Instance != null && AppCompositionRoot.Instance.Flow.IsNavigating;
        public Task<bool> ReturnToHubAsync(Action onBeforeUnload = null)
        {
            DefenseSessionContext.Clear();
            var flow = AppCompositionRoot.Instance?.Flow;
            return flow != null ? flow.ReturnToHubAsync(onBeforeUnload) : Task.FromResult(false);
        }
        public void ResetStateForTesting() { }
    }
}
