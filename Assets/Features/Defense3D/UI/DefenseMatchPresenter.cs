using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectZombie.Features.Defense3D.UI
{
    /// <summary>
    /// Presenter điều phối toàn bộ luồng giao tiếp giữa Models và Views trong chế độ Defense 3D.
    /// Tuân thủ quy tắc MVP: View thụ động nhận dữ liệu đã format, Presenter quản lý vòng đời và sự kiện.
    /// </summary>
    public class DefenseMatchPresenter : MonoBehaviour
    {
        [Header("Views")]
        [SerializeField] private DefenseMatchHUDView _hudView;
        [SerializeField] private DefenseUpgradeModalView _upgradeModalView;
        [SerializeField] private DefenseMatchResultModalView _resultModalView;

        [Header("Scene References")]
        [SerializeField] private DefenseRun6003D _run;
        [SerializeField] private DefenseWoodWallet3D _woodWallet;

        private DefenseExperienceCore _experienceCore;
        private DefenseUpgradeDeck _upgradeDeck;
        private DefenseUpgradeCard[] _currentDrawnCards;
        private bool _isLevelUpModalOpen;
        private IDefenseSessionCommands _sessionCommands;

        public DefenseExperienceCore ExperienceCore => _experienceCore;
        public DefenseUpgradeDeck UpgradeDeck => _upgradeDeck;

        /// <summary>Scene composition supplies commands; UI never mutates the gameplay FSM directly.</summary>
        public void BindSession(IDefenseSessionCommands commands, DefenseRunStats modifiers = null)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (_run == null || _woodWallet == null || _hudView == null ||
                _upgradeModalView == null || _resultModalView == null)
                throw new InvalidOperationException("Match Presenter has missing scene/view references.");
            _sessionCommands = commands;
            if (modifiers != null) _upgradeDeck.BindModifiers(modifiers);
        }

        private void Awake()
        {
            if (_hudView == null) _hudView = GetComponentInChildren<DefenseMatchHUDView>(true);
            if (_upgradeModalView == null) _upgradeModalView = GetComponentInChildren<DefenseUpgradeModalView>(true);
            if (_resultModalView == null) _resultModalView = GetComponentInChildren<DefenseMatchResultModalView>(true);

            _experienceCore = new DefenseExperienceCore();
            _upgradeDeck = new DefenseUpgradeDeck();
        }

        private void Start()
        {
            if (_sessionCommands == null || _run == null || _woodWallet == null || _hudView == null ||
                _upgradeModalView == null || _resultModalView == null)
            {
                Debug.LogError("[TREBRAVE Defense3D] Assign all Match Presenter scene and view references in the Inspector.", this);
                enabled = false;
                return;
            }

            if (_upgradeModalView != null)
            {
                _upgradeModalView.OnCardSelected += OnUpgradeCardSelected;
                _upgradeModalView.Hide();
            }

            if (_resultModalView != null)
            {
                _resultModalView.OnRestartClicked += OnRestartClicked;
                _resultModalView.OnMainMenuClicked += OnMainMenuClicked;
                _resultModalView.Hide();
            }

            if (_run != null)
            {
                _run.MatchEnded += OnMatchEnded;
            }

            if (_experienceCore != null)
            {
                _experienceCore.LevelGained += OnHeroLevelGained;
            }
        }

        private void OnDestroy()
        {
            if (_upgradeModalView != null)
                _upgradeModalView.OnCardSelected -= OnUpgradeCardSelected;

            if (_resultModalView != null)
            {
                _resultModalView.OnRestartClicked -= OnRestartClicked;
                _resultModalView.OnMainMenuClicked -= OnMainMenuClicked;
            }

            if (_run != null)
                _run.MatchEnded -= OnMatchEnded;

            if (_experienceCore != null)
                _experienceCore.LevelGained -= OnHeroLevelGained;
        }

        private void Update()
        {
            UpdateHUD();
        }

        private void UpdateHUD()
        {
            if (_hudView == null || _run == null) return;

            // 1. Đồng hồ thời gian
            float elapsed = _run.ElapsedTime;
            int minutes = Mathf.FloorToInt(elapsed / 60f);
            int seconds = Mathf.FloorToInt(elapsed % 60f);
            string timeString = $"{minutes:00}:{seconds:00}";
            if (_run.IsOvertime)
            {
                timeString += " <color=#FFA500>[BÙ GIỜ]</color>";
            }
            _hudView.SetMatchTime(timeString);

            // 2. Tiến trình Event
            _hudView.SetEventStatus($"Đợt: {_run.ProcessedEventsCount}/{_run.TotalEventsCount}");

            // 3. Máu Căn Cứ Nhà Chính
            if (_run.Base != null)
            {
                float baseCur = _run.Base.CurrentHP;
                float baseMax = _run.Base.MaxHP;
                _hudView.SetBaseHealth(baseCur, baseMax, $"<color=#00FF88>Căn Cứ:</color> {Mathf.CeilToInt(baseCur)}/{Mathf.CeilToInt(baseMax)}");
            }

            // 4. Máu Tướng & Đếm ngược Hồi sinh
            if (_run.HeroHealth != null)
            {
                float heroCur = _run.HeroHealth.CurrentHP;
                float heroMax = _run.HeroHealth.MaxHP;
                string heroText = $"<color=#00E5FF>Tướng:</color> {Mathf.CeilToInt(heroCur)}/{Mathf.CeilToInt(heroMax)}";
                if (_run.HeroHealth.IsDown)
                {
                    heroText += $" <color=#FF4444>[HỒI SINH TRONG {Mathf.CeilToInt(_run.HeroHealth.RemainingRespawnTime)}s]</color>";
                }
                _hudView.SetHeroHealth(heroCur, heroMax, heroText);
            }

            // 5. Gỗ
            int wood = _woodWallet != null ? _woodWallet.CurrentWood : 0;
            _hudView.SetWoodCount($"<color=#FFD700>Gỗ:</color> {wood}");

            // 6. EXP & Cấp độ
            if (_experienceCore != null)
            {
                int curExp = _experienceCore.CurrentExp;
                int reqExp = DefenseExperienceCore.GetExpRequiredForLevel(_experienceCore.CurrentLevel + _experienceCore.PendingLevelUps);
                _hudView.SetExperience(curExp, reqExp, $"<color=#FFCC00>Cấp {_experienceCore.CurrentLevel}</color> ({curExp}/{reqExp})");
            }

            // 7. Boss Dạ Xoa
            bool showBoss = _run.BossSpawned && _run.Yaksha != null && !_run.Yaksha.IsDead;
            if (showBoss)
            {
                float bossCur = _run.Yaksha.CurrentHP;
                float bossMax = _run.Yaksha.MaxHP;
                string bossPhase = _run.AssaultStarted ? "TẤN CÔNG NHÀ CHÍNH" : "ĐẤU TRƯỜNG DẠ XOA";
                _hudView.SetBossHealth(true, bossCur, bossMax, $"<color=#FF0055>Dạ Xoa [{bossPhase}]:</color> {Mathf.CeilToInt(bossCur)}/{Mathf.CeilToInt(bossMax)}");
            }
            else
            {
                _hudView.SetBossHealth(false, 0, 1, string.Empty);
            }
        }

        private void OnHeroLevelGained(int newLevel)
        {
            if (_isLevelUpModalOpen) return;
            OpenNextLevelUpChoice();
        }

        private void OpenNextLevelUpChoice()
        {
            if (_upgradeDeck == null || _upgradeModalView == null) return;
            if (!_isLevelUpModalOpen && (_sessionCommands == null || !_sessionCommands.OpenLevelUp())) return;
            _isLevelUpModalOpen = true;

            var choices = _upgradeDeck.DrawThreeChoices();
            _currentDrawnCards = choices.ToArray();
            _upgradeModalView.Show(_currentDrawnCards);
        }

        private void OnUpgradeCardSelected(int cardIndex)
        {
            if (!_isLevelUpModalOpen || _experienceCore.PendingLevelUps <= 0 ||
                _currentDrawnCards == null || cardIndex < 0 || cardIndex >= _currentDrawnCards.Length)
                return;

            var chosenCard = _currentDrawnCards[cardIndex];
            _currentDrawnCards = null;
            if (!_experienceCore.TryConsumePendingLevelUp()) return;
            _upgradeDeck.SelectUpgrade(chosenCard);

            if (_experienceCore.PendingLevelUps > 0)
            {
                // Còn lần thăng cấp tiếp theo trong hàng đợi
                OpenNextLevelUpChoice();
            }
            else
            {
                _isLevelUpModalOpen = false;
                _upgradeModalView.Hide();
                _sessionCommands?.Resume();
            }
        }

        private void OnMatchEnded(bool isWon)
        {
            CancelRunPresentation();
            if (_resultModalView == null || _run == null) return;

            string title = isWon
                ? "<color=#FFD700>CHIẾN THẮNG QUANG VINH</color>"
                : "<color=#FF4444>CĂN CỨ THẤT THỦ</color>";

            string message = isWon
                ? "Đã tiêu diệt Đại Boss Dạ Xoa và bảo vệ vững chắc Sông Quê!"
                : "Căn cứ Nhà Chính Bờ Tây đã bị quỷ sứ tàn phá hoàn toàn.";

            int minutes = Mathf.FloorToInt(_run.ElapsedTime / 60f);
            int seconds = Mathf.FloorToInt(_run.ElapsedTime % 60f);
            int wood = _woodWallet != null ? _woodWallet.CurrentWood : 0;
            string summary = $"Thời gian sống sót: {minutes:00}:{seconds:00} | Cấp độ: {_experienceCore.CurrentLevel} | Gỗ thu thập: {wood}";

            _resultModalView.ShowResult(title, message, summary);
        }

        private void OnRestartClicked()
        {
            _sessionCommands?.Restart();
        }

        /// <summary>Invalidates old selections before session entities are cleaned up.</summary>
        public void CancelRunPresentation()
        {
            if (_upgradeModalView != null) _upgradeModalView.Hide();
            if (_resultModalView != null) _resultModalView.Hide();
            _currentDrawnCards = null;
            _isLevelUpModalOpen = false;
        }

        /// <summary>Transitional model reset owner until progression leaves the Presenter in RF3D-025.</summary>
        public void ResetRunPresentation()
        {
            CancelRunPresentation();
            _experienceCore?.Reset();
        }

        private void OnMainMenuClicked()
        {
            // Trở về Scene 0 (SampleScene - Survival 2D) hoặc Scene Menu
            SceneManager.LoadScene(0, LoadSceneMode.Single);
        }
    }
}
