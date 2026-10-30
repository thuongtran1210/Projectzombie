using UnityEngine;
using ProjectZombie.Core.Architecture;
using ProjectZombie.Features.Defense3D;
using ProjectZombie.Features.Defense3D.Loadout;
using ProjectZombie.Features.Maps;

namespace ProjectZombie.Features.UI
{
    /// <summary>Scene-local menu navigation adapter. Never retains UI across scene unloads.</summary>
    public sealed class MetaSceneTransitionController : MonoBehaviour
    {
        public static MetaSceneTransitionController Instance { get; private set; }
        [SerializeField] private MetaUIManager _metaUIManager;
        public StageDefinitionSO SelectedStage => AppCompositionRoot.Instance?.Flow.SelectedStage;
        public bool IsLoadoutReviewPending { get; private set; }
        public void CancelLoadoutReview() => IsLoadoutReviewPending = false;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            StageSelect.StageSelectUIPresenter.OnStageSelectedForBattle += HandleStageSelectedForBattle;
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            StageSelect.StageSelectUIPresenter.OnStageSelectedForBattle -= HandleStageSelectedForBattle;
            Instance = null;
        }

        private void HandleStageSelectedForBattle(StageDefinitionSO stage)
        {
            var flow = AppCompositionRoot.Instance?.Flow;
            if (flow == null || stage == null || !stage.CanLaunchDefense) return;
            flow.SelectStage(stage);
            IsLoadoutReviewPending = true;
            _metaUIManager.OpenChildScreen(MetaScreenType.DefensePreparation);
        }

        public async void TransitionToCombat(StageDefinitionSO stage = null)
        {
            var selected = stage != null ? stage : SelectedStage;
            var dto = new DefenseLoadoutRepository().Load();
            var snapshot = new DefenseLoadoutSnapshot(dto.HeroId, dto.RelicId, dto.MinionIds, dto.TowerIds);
            await DefenseMatchNavigation.LaunchDefenseMatchAsync(selected, snapshot);
        }

        public void StartRun()
        {
            if (SelectedStage != null) TransitionToCombat(SelectedStage);
            else _metaUIManager.OpenScreen(MetaScreenType.StageSelect);
        }

        public void ReturnToMetaHub() => _ = DefenseNavigationService.Default.ReturnToHubAsync();
    }
}
