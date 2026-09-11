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

        private void Awake()
        {
            if (_view == null) _view = GetComponent<StageSelectUIView>();
            EnsureStageDatabaseLoaded();
        }

        private void OnEnable()
        {
            EnsureStageDatabaseLoaded();
            RefreshView();
        }

        private async void EnsureStageDatabaseLoaded()
        {
            WorldStageDatabaseSO db = null;

            // 1. Ưu tiên nạp phiên bản Hot Update mới nhất từ Addressables CDN
            try
            {
                var locHandle = UnityEngine.AddressableAssets.Addressables.LoadResourceLocationsAsync("WorldStageDatabase");
                await locHandle.Task;
                if (locHandle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded && locHandle.Result != null && locHandle.Result.Count > 0)
                {
                    var handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<WorldStageDatabaseSO>("WorldStageDatabase");
                    db = await handle.Task;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[{nameof(StageSelectUIPresenter)}] Không nạp được WorldStageDatabase từ Addressables: {ex.Message}");
            }

            // 2. Fallback nạp từ Resources cục bộ trong APK (khi offline)
            if (db == null)
            {
                db = Resources.Load<WorldStageDatabaseSO>("WorldStageDatabase");
            }

#if UNITY_EDITOR
            if (db == null)
            {
                db = UnityEditor.AssetDatabase.LoadAssetAtPath<WorldStageDatabaseSO>("Assets/_Data/Levels/WorldStageDatabase.asset");
            }
#endif
            if (db != null && db.Stages != null && db.Stages.Count > 0)
            {
                _stageList = new List<StageDefinitionSO>(db.Stages);
                RefreshView();
            }
        }

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
            _patchManager.OnPatchFailed += HandlePatchFailed;

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
                _patchManager.OnPatchFailed -= HandlePatchFailed;
            }
        }

        private async void RefreshView()
        {
            if (_stageList == null || _stageList.Count == 0 || _view == null) return;

            int requestedIndex = _currentStageIndex;
            var currentStage = _stageList[requestedIndex];

            // Kiểm tra xem trong Cache đã có bản đồ mới nhất chưa bằng GetDownloadSizeAsync
            bool isDlcDownloaded = false;
            float actualDownloadSizeMb = 0f;
            if (!string.IsNullOrEmpty(currentStage.mapPrefabAddress))
            {
                var status = await _patchManager.CheckAssetStatusAsync(currentStage.mapPrefabAddress);
                
                // Nếu không cần tải hoặc dung lượng cần tải == 0 byte (đã nằm trong cache máy) -> ĐÃ TẢI!
                if (!status.NeedsDownload || status.DownloadSizeBytes == 0)
                {
                    isDlcDownloaded = true;
                }
                else
                {
                    isDlcDownloaded = false;
                    actualDownloadSizeMb = status.DownloadSizeBytes / 1048576f;
                }
            }

            // Đảm bảo không bị race condition khi người chơi bấm Next/Prev nhanh
            if (requestedIndex != _currentStageIndex) return;

            bool isPrevAvailable = _currentStageIndex > 0;
            bool isNextAvailable = _currentStageIndex < _stageList.Count - 1;

            _view.RenderStageInfo(currentStage, isDlcDownloaded, isPrevAvailable, isNextAvailable, actualDownloadSizeMb);

            // Nếu ải này đang trong quá trình tải ngầm (người chơi chuyển tab hoặc thoát rồi quay lại), khôi phục ngay thanh %
            if (_patchManager != null && _patchManager.IsDownloading)
            {
                var downloadingKey = _patchManager.CurrentDownloadingKey?.ToString();
                if (downloadingKey == currentStage.mapPrefabAddress || downloadingKey == "ALL_PATCH")
                {
                    var p = _patchManager.CurrentProgress;
                    _view.UpdateDownloadProgress(p.Percent, string.IsNullOrEmpty(p.StatusMessage) ? "Đang tiếp tục tải tài nguyên..." : p.StatusMessage);
                }
            }
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
                if (_currentStageIndex >= 0 && _currentStageIndex < _stageList.Count)
                {
                    var currentStage = _stageList[_currentStageIndex];
                    var downloadingKey = _patchManager.CurrentDownloadingKey?.ToString();
                    if (downloadingKey == currentStage.mapPrefabAddress || downloadingKey == "ALL_PATCH")
                    {
                        _view.UpdateDownloadProgress(progress.Percent, progress.FormattedProgress);
                    }
                }
            }
        }

        private void HandlePatchCompleted()
        {
            global::Core.Audio.AudioManager.Instance?.PlayUIConfirm(1.2f);
            RefreshView();
        }

        private void HandlePatchFailed(string error)
        {
            Debug.LogWarning($"[StageSelectUIPresenter] Tải DLC thất bại: {error}");
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
            if (MetaUIManager.Instance != null)
            {
                MetaUIManager.Instance.PopScreen();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
