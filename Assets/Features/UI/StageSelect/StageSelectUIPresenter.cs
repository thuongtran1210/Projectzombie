using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectZombie.Features.Maps;
using ProjectZombie.Core.Services.Addressables;

namespace ProjectZombie.Features.UI.StageSelect
{
    /// <summary>
    /// Presenter điều phối dữ liệu Chọn Ải & Tải DLC (MVP Pattern).
    /// </summary>
    public class StageSelectUIPresenter : MonoBehaviour
    {
        [Header("View Reference")]
        [SerializeField] private StageSelectUIView _view;

        [Header("Stage Database")]
        [SerializeField] private List<StageDefinitionSO> _stageList = new List<StageDefinitionSO>();

        private int _currentStageIndex = 0;
        private AddressablePatchManager _patchManager => AddressablePatchManager.Instance;

        public static event Action<StageDefinitionSO> OnStageSelectedForBattle;

        private void Start()
        {
            if (_view != null)
            {
                _view.OnStartBattleClicked += HandleStartBattle;
                _view.OnDownloadDlcClicked += HandleDownloadDlc;
                _view.OnPrevStageClicked += HandlePrevStage;
                _view.OnNextStageClicked += HandleNextStage;
                _view.OnBackClicked += HandleBack;
            }

            _patchManager.OnPatchProgressChanged += HandlePatchProgress;
            _patchManager.OnPatchCompleted += HandlePatchCompleted;

            RefreshView();
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.OnStartBattleClicked -= HandleStartBattle;
                _view.OnDownloadDlcClicked -= HandleDownloadDlc;
                _view.OnPrevStageClicked -= HandlePrevStage;
                _view.OnNextStageClicked -= HandleNextStage;
                _view.OnBackClicked -= HandleBack;
            }

            if (_patchManager != null)
            {
                _patchManager.OnPatchProgressChanged -= HandlePatchProgress;
                _patchManager.OnPatchCompleted -= HandlePatchCompleted;
            }
        }

        private void RefreshView()
        {
            if (_stageList == null || _stageList.Count == 0 || _view == null) return;

            var currentStage = _stageList[_currentStageIndex];
            
            // Kiểm tra xem Stage này đã tải Asset về máy chưa (hoặc là màn 1 đã có sẵn trong APK)
            bool isDlcDownloaded = _currentStageIndex == 0 || AddressableAssetManager.Instance.IsAssetLoaded(currentStage.mapPrefabAddress);

            bool isPrevAvailable = _currentStageIndex > 0;
            bool isNextAvailable = _currentStageIndex < _stageList.Count - 1;

            _view.RenderStageInfo(currentStage, isDlcDownloaded, isPrevAvailable, isNextAvailable);
        }

        private void HandlePrevStage()
        {
            if (_currentStageIndex > 0)
            {
                _currentStageIndex--;
                global::Core.Audio.AudioManager.Instance?.PlayUIClick();
                RefreshView();
            }
        }

        private void HandleNextStage()
        {
            if (_currentStageIndex < _stageList.Count - 1)
            {
                _currentStageIndex++;
                global::Core.Audio.AudioManager.Instance?.PlayUIClick();
                RefreshView();
            }
        }

        private async void HandleDownloadDlc()
        {
            if (_currentStageIndex >= _stageList.Count) return;
            var stage = _stageList[_currentStageIndex];

            global::Core.Audio.AudioManager.Instance?.PlayUIConfirm();
            _view.UpdateDownloadProgress(0f, "Đang kết nối máy chủ CDN...");

            // Tải AssetBundle chứa Map và Timeline của Ải này
            await _patchManager.DownloadPatchAsync(new object[] { stage.mapPrefabAddress });
        }

        private void HandlePatchProgress(PatchProgress progress)
        {
            if (_view != null && progress.State == PatchState.Downloading)
            {
                _view.UpdateDownloadProgress(progress.Percent, progress.FormattedProgress);
            }
        }

        private void HandlePatchCompleted()
        {
            global::Core.Audio.AudioManager.Instance?.PlayUIConfirm(1.2f);
            RefreshView();
        }

        private void HandleStartBattle()
        {
            if (_currentStageIndex >= _stageList.Count) return;
            var stage = _stageList[_currentStageIndex];

            global::Core.Audio.AudioManager.Instance?.PlayUIConfirm();
            OnStageSelectedForBattle?.Invoke(stage);
        }

        private void HandleBack()
        {
            global::Core.Audio.AudioManager.Instance?.PlayUIClick();
            gameObject.SetActive(false);
        }
    }
}
