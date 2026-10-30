using ProjectZombie.Features.MetaProgression;

namespace ProjectZombie.Features.UI
{
    public partial class CardCodexPresenter
    {
        private bool _upgradeInProgress;

        private void UpdateScreenPurpose()
        {
            if (_view == null) return;
            string title = _equipmentMode ? "TRANG BỊ ĐỘI HÌNH" : _currentTab == CodexTabType.HeroCards ? "ANH HÙNG"
                : _currentTab == CodexTabType.RelicFusion ? "THẦN KHÍ" : _currentTab == CodexTabType.Minions ? "THẺ LÍNH" : "THẺ TRỤ";
            _view.SetCollectionMode(!_equipmentMode, title);
        }

        private void ShowUpgradeAvailability(int star, int shards, int required, int cost, bool available, string unavailableReason)
        {
            if (_view == null) return;
            if (_equipmentMode)
            {
                _view.SetUpgradeStatus("Chọn thẻ rồi xác nhận trang bị. Xem và nâng cấp thẻ từ mục tương ứng ở Mainmenu.");
                return;
            }
            if (star >= 5)
            {
                _view.SetUpgradeStatus("<color=#00FF88>Đã đạt tối đa 5 sao.</color>");
                return;
            }
            if (!string.IsNullOrEmpty(unavailableReason))
            {
                _view.SetUpgradeStatus("<color=#FFAA00>" + unavailableReason + "</color>");
                return;
            }
            int balance = MetaCurrencyManager.HasInstance ? MetaCurrencyManager.Instance.TotalCurrency : 0;
            string requirements = $"Mảnh: {shards:N0}/{required:N0} • Cổ Tiền: {balance:N0}/{cost:N0}";
            string missing = string.Empty;
            if (shards < required) missing = $"Thiếu {required - shards:N0} mảnh";
            if (balance < cost) missing += (missing.Length > 0 ? " và " : "Thiếu ") + $"{cost - balance:N0} Cổ Tiền";
            string status = available ? (star == 0 ? "Đủ điều kiện mở khóa." : "Đủ điều kiện tăng sao.")
                : missing.Length > 0 ? missing + "." : "Chưa thể nâng cấp; vui lòng kiểm tra dữ liệu thẻ.";
            _view.SetUpgradeStatus(requirements + "\n<color=" + (available ? "#00FF88" : "#FFAA00") + ">" + status + "</color>");
        }

        private void PerformSelectedUpgrade()
        {
            if (_upgradeInProgress || _view == null || _equipmentMode) return;
            var hero = _selectedHero;
            var relic = _selectedRelic;
            string defenseId = _inspectedDefenseId;
            var tab = _currentTab;
            var position = _view.CaptureListPosition();
            bool success = false;
            string error = "Không thể nâng cấp thẻ. Vui lòng thử lại.";
            int previousStar = 0;
            _upgradeInProgress = true;
            try
            {
                if (tab == CodexTabType.HeroCards)
                {
                    var manager = CharacterProgressionManager.HasInstance ? CharacterProgressionManager.Instance : null;
                    if (manager == null || hero == null) error = "Dữ liệu anh hùng chưa sẵn sàng.";
                    else
                    {
                        previousStar = manager.GetCharacterStarLevel(hero.characterId);
                        if (manager.CanUpgradeCharacter(hero.characterId, out error))
                        {
                            var result = manager.TryUpgradeCharacterStar(hero.characterId);
                            success = result == UpgradeCharacterResult.Success;
                            error = result == UpgradeCharacterResult.SaveFailed
                                ? "Lưu nâng cấp thất bại. Tiền và mảnh đã được giữ nguyên. Vui lòng thử lại."
                                : "Không thể nâng cấp anh hùng; vui lòng kiểm tra tài nguyên và cấu hình.";
                        }
                    }
                }
                else
                {
                    var manager = RelicInventoryManager.HasInstance ? RelicInventoryManager.Instance : null;
                    string id = tab == CodexTabType.RelicFusion && relic != null ? relic.weaponId : defenseId;
                    if (manager == null || string.IsNullOrEmpty(id)) error = "Dữ liệu thẻ chưa sẵn sàng.";
                    else
                    {
                        previousStar = manager.GetRelicStarLevel(id);
                        if (manager.CanFuse(id, out error))
                        {
                            success = manager.TryFuseRelic(id);
                            error = "Không thể lưu nâng cấp. Tiền và mảnh được giữ nguyên; vui lòng thử lại khi dữ liệu sẵn sàng.";
                        }
                    }
                }

                // Model events are suppressed during the transaction; refresh exactly once afterwards.
                RefreshCurrency();
                PopulateGridForTab(tab);
                if (tab == CodexTabType.HeroCards && hero != null) SelectHero(hero);
                else if (tab == CodexTabType.RelicFusion && relic != null) SelectRelic(relic);
                else if (!string.IsNullOrEmpty(defenseId) && _defenseCatalog.TryGetEntry(defenseId, out var entry)) InspectDefenseEntry(entry);
                _view.RestoreListPosition(position);
                if (success)
                {
                    _audioService?.PlayUIConfirm();
                    _view.ShowUpgradeOutcome(previousStar == 0 ? "<color=#00FF88>Mở khóa thẻ thành công.</color>"
                        : $"<color=#00FF88>Tăng sao thành công: {previousStar} → {previousStar + 1} sao.</color>");
                }
                else
                {
                    _audioService?.PlayUIError();
                    _view.ShowUpgradeOutcome("<color=#FFAA00>" + error + "</color>");
                }
            }
            finally
            {
                _upgradeInProgress = false;
            }
        }
    }
}
