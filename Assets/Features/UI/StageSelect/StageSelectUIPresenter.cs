using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using ProjectZombie.Features.Maps;
using ProjectZombie.Core.Architecture;

namespace ProjectZombie.Features.UI.StageSelect
{
    /// <summary>Stage selection reads the app catalog; availability queries use the actual scene key.</summary>
    public sealed class StageSelectUIPresenter : MonoBehaviour, ProjectZombie.Core.Audio.IAudioServiceConsumer
    {
        [SerializeField] private StageSelectUIView _view;
        private IReadOnlyList<StageDefinitionSO> _stages;
        private ProjectZombie.Core.Audio.IAudioService _audio;
        private int _index;
        private int _revision;
        private bool _destroyed;
        private bool _downloaded;
        private bool _downloading;
        public static event Action<StageDefinitionSO> OnStageSelectedForBattle;
        public void InjectAudioService(ProjectZombie.Core.Audio.IAudioService audioService) => _audio = audioService;

        private void Awake()
        {
            if (_view == null) _view = GetComponent<StageSelectUIView>();
        }
        private void Start()
        {
            _view.OnStartBattleClicked += HandleStart;
            _view.OnDownloadDlcClicked += HandleDownload;
            _view.OnPrevStageClicked += Previous;
            _view.OnNextStageClicked += Next;
            _view.OnBackClicked += Back;
            Refresh();
        }
        private void OnEnable() => Refresh();
        private void OnDisable() => _revision++;
        private void OnDestroy()
        {
            _destroyed = true;
            _revision++;
            if (_view == null) return;
            _view.OnStartBattleClicked -= HandleStart;
            _view.OnDownloadDlcClicked -= HandleDownload;
            _view.OnPrevStageClicked -= Previous;
            _view.OnNextStageClicked -= Next;
            _view.OnBackClicked -= Back;
        }
        private async void Refresh()
        {
            var database = AppCompositionRoot.Instance?.Stages;
            if (database == null || database.Stages.Count == 0 || _view == null) return;
            _stages = database.Stages;
            _index = Mathf.Clamp(_index, 0, _stages.Count - 1);
            var stage = _stages[_index];
            int revision = ++_revision;
            _downloaded = false;
            _view.RenderStageInfo(stage, false, _index > 0, _index < _stages.Count - 1);
            if (!stage.CanLaunchDefense) return;
            var handle = Addressables.GetDownloadSizeAsync(DownloadKeys(stage));
            try
            {
                long bytes = await handle.Task;
                if (_destroyed || revision != _revision || !isActiveAndEnabled) return;
                if (handle.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                    throw new InvalidOperationException("Không thể kiểm tra dữ liệu ải.");
                _downloaded = bytes == 0;
                _view.RenderStageInfo(stage, _downloaded, _index > 0, _index < _stages.Count - 1, bytes / 1048576f);
                bool unlocked = stage.IsUnlocked(ProjectZombie.Core.Save.GameManager.Instance?.SaveData);
                _view.SetStartAvailability(_downloaded && unlocked, unlocked ? null : "Hoàn thành các ải yêu cầu để mở khóa.");
            }
            catch (Exception exception)
            {
                if (!_destroyed && revision == _revision) _view.ShowDownloadError(exception.Message, Refresh);
            }
            finally
            {
                if (handle.IsValid()) Addressables.Release(handle);
            }
        }
        private void Previous() { if (!_downloading && _index > 0) { _index--; Refresh(); } }
        private void Next() { if (!_downloading && _stages != null && _index + 1 < _stages.Count) { _index++; Refresh(); } }
        private void Back() { if (!_downloading) MetaUIManager.Instance?.PopScreen(); }
        private void HandleStart()
        {
            if (!_downloaded || _downloading || _stages == null) return;
            if (!_stages[_index].IsUnlocked(ProjectZombie.Core.Save.GameManager.Instance?.SaveData)) return;
            _audio?.PlayUIConfirm();
            OnStageSelectedForBattle?.Invoke(_stages[_index]);
        }
        private async void HandleDownload()
        {
            if (_downloading || _stages == null || !_stages[_index].CanLaunchDefense) return;
            _downloading = true;
            _view.UpdateDownloadProgress(0f, "Đang tải dữ liệu ải...");
            var handle = Addressables.DownloadDependenciesAsync(DownloadKeys(_stages[_index]), Addressables.MergeMode.Union, false);
            try
            {
                await handle.Task;
                if (!_destroyed && handle.Status != UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                    _view.ShowDownloadError("Tải dữ liệu thất bại. Vui lòng thử lại.", HandleDownload);
            }
            catch (Exception exception)
            {
                if (!_destroyed) _view.ShowDownloadError(exception.Message, HandleDownload);
            }
            finally
            {
                if (handle.IsValid()) Addressables.Release(handle);
                _downloading = false;
                if (!_destroyed && isActiveAndEnabled) Refresh();
            }
        }

        private static IEnumerable<object> DownloadKeys(StageDefinitionSO stage)
        {
            return stage.SpawnRules != null && stage.SpawnRules.RuntimeKeyIsValid()
                ? new[] { stage.Map.Scene.RuntimeKey, stage.SpawnRules.RuntimeKey }
                : new[] { stage.Map.Scene.RuntimeKey };
        }
    }
}
