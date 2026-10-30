using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Maps;
using ProjectZombie.Features.Shared;

namespace ProjectZombie.Core.Architecture
{
    public enum AppPhase { Booting, Menu, Loading, Match, Returning, Error }

    /// <summary>Single navigation owner. Serializes transitions and waits for explicit scene readiness.</summary>
    public sealed class AppFlowController
    {
        private readonly IContentSceneLoader _loader;
        private readonly AssetReference _hub;
        private readonly AppLoadingView _loading;
        private readonly Action<Scene, MatchLaunchRequest> _initializeMatch;
        private readonly Action<Exception> _reportError;
        private readonly IStageRulesLoader _rules;
        public bool IsNavigating { get; private set; }
        public AppPhase Phase { get; private set; } = AppPhase.Booting;
        public StageDefinitionSO SelectedStage { get; private set; }
        public MatchLaunchRequest ActiveMatch { get; private set; }
        public string LastError { get; private set; }

        public AppFlowController(IContentSceneLoader loader, AssetReference hub, AppLoadingView loading,
            Action<Scene, MatchLaunchRequest> initializeMatch = null, Action<Exception> reportError = null,
            IStageRulesLoader rulesLoader = null)
        {
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            _hub = hub;
            _loading = loading;
            _initializeMatch = initializeMatch;
            _reportError = reportError ?? (exception => Debug.LogError("[AppFlow] " + exception));
            _rules = rulesLoader ?? new AddressableStageRulesLoader();
        }

        public void SelectStage(StageDefinitionSO stage)
        {
            if (IsNavigating) throw new InvalidOperationException("Navigation in progress.");
            if (stage == null || !stage.CanLaunchDefense) throw new ArgumentException("Ải chưa được cấu hình để xuất trận.");
            if (!stage.IsUnlocked(ProjectZombie.Core.Save.GameManager.Instance?.SaveData)) throw new InvalidOperationException("Ải chưa được mở khóa.");
            SelectedStage = stage;
        }

        public Task<bool> EnterMatchAsync(MatchLaunchRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (!request.Stage.IsUnlocked(ProjectZombie.Core.Save.GameManager.Instance?.SaveData))
            {
                LastError = "Ải chưa được mở khóa.";
                return Task.FromResult(false);
            }
            return TransitionAsync(request, null);
        }

        public Task<bool> ReturnToHubAsync(Action beforeUnload = null) => TransitionAsync(null, beforeUnload);

        public void AdoptEditorMatch(MatchLaunchRequest request)
        {
            ActiveMatch = request;
            Phase = AppPhase.Match;
        }

        public Task<StageSpawnConfig> LoadEditorRulesAsync(StageDefinitionSO stage) => _rules.LoadAsync(stage);

        /// <summary>A restart retains map resources but creates a new reward/attempt identity.</summary>
        public MatchLaunchRequest BeginNewAttempt()
        {
            if (ActiveMatch == null || IsNavigating) throw new InvalidOperationException("No active match to restart.");
            ActiveMatch = new MatchLaunchRequest(ActiveMatch.Stage, ActiveMatch.Loadout).ResolveRules(ActiveMatch.ResolvedRules);
            return ActiveMatch;
        }

        private async Task<bool> TransitionAsync(MatchLaunchRequest request, Action beforeUnload)
        {
            if (IsNavigating) return false;
            IsNavigating = true;
            Phase = request == null ? AppPhase.Returning : AppPhase.Loading;
            LastError = null;
            _loading?.Show(request == null ? "Đang trở về sảnh..." : "Đang tải " + request.Stage.stageName + "...");
            try
            {
                beforeUnload?.Invoke();
                Time.timeScale = 1f;
                await _loader.UnloadAsync();
                _rules.Release();
                if (request != null) request = request.ResolveRules(await _rules.LoadAsync(request.Stage));
                ActiveMatch = request;
                var scene = await _loader.LoadAsync(request == null ? _hub : request.Stage.Map.Scene);
                if (request != null)
                {
                    if (_initializeMatch != null) _initializeMatch(scene, request);
                    else await InitializeMatch(scene, request);
                    SelectedStage = request.Stage;
                    Phase = AppPhase.Match;
                }
                else
                {
                    SelectedStage = null;
                    Phase = AppPhase.Menu;
                    GameStateManager.Instance?.ChangeState(GameState.MainMenu);
                }
                _loading?.Hide();
                return true;
            }
            catch (Exception exception)
            {
                LastError = exception.Message;
                _reportError(exception);
                ActiveMatch = null;
                try
                {
                    await _loader.UnloadAsync();
                    _rules.Release();
                    await _loader.LoadAsync(_hub);
                    SelectedStage = null;
                    Phase = AppPhase.Menu;
                    GameStateManager.Instance?.ChangeState(GameState.MainMenu);
                    _loading?.ShowError("Không thể tải trận: " + LastError + "\nĐã trở về sảnh. Bạn có thể thử lại.");
                }
                catch (Exception recovery)
                {
                    Phase = AppPhase.Error;
                    _loading?.Show("Không thể tải nội dung. Kiểm tra kết nối và khởi động lại.\n" + recovery.Message);
                    _reportError(recovery);
                }
                return false;
            }
            finally
            {
                IsNavigating = false;
            }
        }

        private static async Task InitializeMatch(Scene scene, MatchLaunchRequest request)
        {
            DefenseSceneCompositionRoot result = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var candidate in root.GetComponentsInChildren<DefenseSceneCompositionRoot>(true))
                {
                    if (result != null) throw new InvalidOperationException("Multiple composition roots in map.");
                    result = candidate;
                }
            }
            if (result == null) throw new InvalidOperationException("Map requires one DefenseSceneCompositionRoot.");
            await result.InitializeMatchAsync(request);
            if (!result.IsReady) throw new InvalidOperationException("Map initialization failed.");
        }
    }
}
