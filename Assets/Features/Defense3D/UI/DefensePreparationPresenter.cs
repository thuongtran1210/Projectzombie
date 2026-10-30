using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using ProjectZombie.Features.Defense3D.Loadout;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>
    /// Presenter điều phối dữ liệu chuẩn bị trận đấu Defense 3D (Màn hình chọn Tướng, Pháp Bảo, Lính và Trụ ngoài Sảnh).
    /// Tuân thủ quy chuẩn MVP và kiến trúc G4:
    /// 1. Tương tác với LoadoutSelection thuần C# và ILoadoutRepository.
    /// 2. Nút Xuất Trận chỉ cho phép kích hoạt khi LoadoutValidator.Validate thành công.
    /// 3. Lắng nghe và cập nhật trạng thái realtime khi người chơi thay đổi lựa chọn.
    /// 4. Hỗ trợ hủy / thử lại an toàn khi quá trình nạp gặp sự cố.
    /// </summary>
    public sealed class DefensePreparationPresenter : IDisposable
    {
        private readonly ILoadoutCatalog _catalog;
        private readonly ILoadoutOwnership _ownership;
        private readonly ILoadoutRepository _repository;
        private readonly LoadoutRules _rules;
        private readonly ProjectZombie.Features.Maps.StageDefinitionSO _stage;

        private LoadoutSelection _selection;
        private bool _canStartBattle;
        private string _lastValidationMessage = string.Empty;

        /// <summary>
        /// Phát ra khi trạng thái hợp lệ của đội hình thay đổi (cho phép bật/tắt nút Xuất Trận trên UI).
        /// </summary>
        public event Action<bool, string> OnValidationStateChanged;

        /// <summary>
        /// Phát ra khi đội hình thay đổi bất kỳ vị trí nào.
        /// </summary>
        public event Action<LoadoutSelection> OnSelectionChanged;

        public LoadoutSelection Selection => _selection;
        public ILoadoutCatalog Catalog => _catalog;
        public ILoadoutOwnership Ownership => _ownership;
        public bool CanStartBattle => _canStartBattle;
        public bool HasStageSelected => _stage != null && _stage.CanLaunchDefense;
        public string LastValidationMessage => _lastValidationMessage;

        public DefensePreparationPresenter(
            ILoadoutCatalog catalog = null,
            ILoadoutOwnership ownership = null,
            ILoadoutRepository repository = null,
            LoadoutRules rules = null,
            ProjectZombie.Features.Maps.StageDefinitionSO stage = null)
        {
            _stage = stage;
            _catalog = catalog ?? Defense3DCatalog.CreateDefault();
            _ownership = ownership ?? DefenseMetaOwnershipAdapter.CreateDefault();
            _repository = repository ?? new DefenseLoadoutRepository();
            _rules = rules ?? LoadoutRules.Default;

            InitializeFromRepository();
        }

        private void InitializeFromRepository()
        {
            var savedDto = _repository.Load();
            if (savedDto == null)
            {
                savedDto = DefenseLoadoutRepository.CreateDefaultDto();
            }

            _selection = new LoadoutSelection(
                savedDto.HeroId,
                savedDto.RelicId,
                savedDto.MinionIds,
                savedDto.TowerIds);

            _selection.OnChanged += HandleSelectionChanged;
            if (ValidateCurrentSelection()) SaveSelection();
        }

        private void HandleSelectionChanged()
        {
            if (ValidateCurrentSelection()) SaveSelection();
            OnSelectionChanged?.Invoke(_selection);
        }

        /// <summary>
        /// Thẩm định tính hợp lệ của đội hình hiện tại.
        /// </summary>
        public bool ValidateCurrentSelection()
        {
            var result = LoadoutValidator.Validate(_selection, _rules, _catalog, _ownership);
            _canStartBattle = result.IsValid;
            _lastValidationMessage = result.ErrorMessage;

            OnValidationStateChanged?.Invoke(_canStartBattle, _lastValidationMessage);
            return _canStartBattle;
        }

        public void SaveSelection()
        {
            if (_selection == null || _repository == null) return;
            var dto = new LoadoutSaveDto(_selection.HeroId, _selection.RelicId, _selection.MinionIds, _selection.TowerIds, DefenseLoadoutRepository.CURRENT_VERSION);
            _repository.Save(dto);
        }

        public bool SelectHero(string heroId)
        {
            if (string.IsNullOrWhiteSpace(heroId)) return false;
            return _selection.SetHero(heroId.Trim());
        }

        public bool SelectRelic(string relicId)
        {
            if (string.IsNullOrWhiteSpace(relicId)) return false;
            return _selection.SetRelic(relicId.Trim());
        }

        public bool ToggleMinion(string minionId)
        {
            if (string.IsNullOrWhiteSpace(minionId)) return false;
            minionId = minionId.Trim();

            if (_selection.MinionIds.Contains(minionId))
            {
                return _selection.RemoveMinion(minionId);
            }
            else
            {
                return _selection.AddMinion(minionId);
            }
        }

        public bool ToggleTower(string towerId)
        {
            if (string.IsNullOrWhiteSpace(towerId)) return false;
            towerId = towerId.Trim();

            if (_selection.TowerIds.Contains(towerId))
            {
                return _selection.RemoveTower(towerId);
            }
            else
            {
                return _selection.AddTower(towerId);
            }
        }

        /// <summary>
        /// Kích hoạt xuất trận: Lưu cấu hình hiện tại và bắt đầu nạp Scene Defense 3D.
        /// </summary>
        public async Task<bool> StartBattleAsync(Action<float, string> reportProgress = null)
        {
            if (_stage == null || !_stage.CanLaunchDefense)
            {
                OnValidationStateChanged?.Invoke(false, "Hãy chọn một ải hợp lệ trước khi xuất trận.");
                return false;
            }
            if (!ValidateCurrentSelection())
            {
                Debug.LogWarning($"[DefensePreparationPresenter] Không thể xuất trận: {_lastValidationMessage}");
                return false;
            }

            // Lưu lựa chọn mới vào repository
            var dto = new LoadoutSaveDto(
                _selection.HeroId,
                _selection.RelicId,
                _selection.MinionIds,
                _selection.TowerIds,
                DefenseLoadoutRepository.CURRENT_VERSION);
            _repository.Save(dto);

            // Nạp trận đấu Defense 3D
            var snapshot = _selection.ToSnapshot();
            return await DefenseMatchNavigation.LaunchDefenseMatchAsync(_stage, snapshot, reportProgress);
        }

        public void Dispose()
        {
            if (_selection != null)
            {
                _selection.OnChanged -= HandleSelectionChanged;
            }
            OnValidationStateChanged = null;
            OnSelectionChanged = null;
        }
    }
}
