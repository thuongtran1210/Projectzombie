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
        [SerializeField] private DefenseTowerBuildPrototype3D _towerBuilder;

        private DefenseExperienceCore _experienceCore;
        private DefenseUpgradeDeck _upgradeDeck;
        private DefenseUpgradeCard[] _currentDrawnCards;
        private bool _isLevelUpModalOpen;
        private IDefenseSessionCommands _sessionCommands;
        private readonly DefenseMatchResultService _resultService = new DefenseMatchResultService();

        // Dirty-checking cache to eliminate string allocations every frame
        private int _lastSeconds = -1;
        private bool _lastOvertime = false;
        private int _lastProcessedEvents = -1;
        private int _lastTotalEvents = -1;
        private int _lastBaseCurHp = -1;
        private int _lastBaseMaxHp = -1;
        private int _lastHeroCurHp = -1;
        private int _lastHeroMaxHp = -1;
        private bool _lastHeroIsDown = false;
        private int _lastHeroRespawnSeconds = -1;
        private int _lastWood = -1;
        private int _lastExp = -1;
        private int _lastReqExp = -1;
        private int _lastLevel = -1;
        private bool _lastShowBoss = false;
        private int _lastBossCurHp = -1;
        private int _lastBossMaxHp = -1;
        private string _lastBossPhase = null;

        public DefenseExperienceCore ExperienceCore => _experienceCore;
        public DefenseUpgradeDeck UpgradeDeck => _upgradeDeck;
        public DefenseMatchResultService ResultService => _resultService;

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

        private int _lastBuildSocket = -2, _lastBuildLevel = -2, _lastBuildCost = -2, _lastBuildMax = -2;
        private System.Action _requestBuild, _requestUpgrade;
        public void BindTowerBuilder(DefenseTowerBuildPrototype3D builder) => _towerBuilder = builder;
        private void RequestTowerMenu() => _towerBuilder?.RequestBuildNearest();
        private void UpgradeTower() => _towerBuilder?.TryUpgradeNearest();

        private void Awake()
        {
            _requestBuild = RequestTowerMenu;
            _requestUpgrade = UpgradeTower;
            if (_hudView == null) _hudView = GetComponentInChildren<DefenseMatchHUDView>(true);
            if (_upgradeModalView == null) _upgradeModalView = GetComponentInChildren<DefenseUpgradeModalView>(true);
            if (_resultModalView == null) _resultModalView = GetComponentInChildren<DefenseMatchResultModalView>(true);

            if (_run == null) _run = FindObjectOfType<DefenseRun6003D>();
            if (_woodWallet == null) _woodWallet = FindObjectOfType<DefenseWoodWallet3D>();

            _experienceCore = new DefenseExperienceCore();
            _upgradeDeck = new DefenseUpgradeDeck();
        }

        private bool _presentationInitialized;

        private void Start()
        {
            if (_sessionCommands != null) InitializePresentation();
        }

        /// <summary>Explicit readiness step; independent of asynchronous scene activation timing.</summary>
        public void InitializePresentation()
        {
            if (_presentationInitialized) return;
            if (_run == null) _run = FindObjectOfType<DefenseRun6003D>();
            if (_woodWallet == null) _woodWallet = FindObjectOfType<DefenseWoodWallet3D>();
            if (_hudView == null) _hudView = GetComponentInChildren<DefenseMatchHUDView>(true);
            if (_upgradeModalView == null) _upgradeModalView = GetComponentInChildren<DefenseUpgradeModalView>(true);
            if (_resultModalView == null) _resultModalView = GetComponentInChildren<DefenseMatchResultModalView>(true);

            if (_sessionCommands == null)
            {
                Debug.LogWarning("[TREBRAVE Defense3D] Match Presenter chưa nhận được Session Commands (SceneCompositionRoot có thể chưa hoàn thành Start hoặc bị lỗi).", this);
                enabled = false;
                return;
            }

            if (_run == null || _woodWallet == null || _hudView == null ||
                _upgradeModalView == null || _resultModalView == null)
            {
                Debug.LogError("[TREBRAVE Defense3D] Assign all Match Presenter scene and view references in the Inspector.", this);
                enabled = false;
                return;
            }

            _presentationInitialized = true;
            enabled = true;
            if (_upgradeModalView != null)
            {
                _upgradeModalView.OnCardSelected += OnUpgradeCardSelected;
                _upgradeModalView.OnRerollRequested += OnUpgradeRerollRequested;
                _upgradeModalView.OnSkipRequested += OnUpgradeSkipRequested;
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
            {
                _upgradeModalView.OnCardSelected -= OnUpgradeCardSelected;
                _upgradeModalView.OnRerollRequested -= OnUpgradeRerollRequested;
                _upgradeModalView.OnSkipRequested -= OnUpgradeSkipRequested;
            }

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

            // 1. Đồng hồ thời gian - Chỉ cập nhật khi số giây thực sự thay đổi
            float elapsed = _run.ElapsedTime;
            int totalSeconds = Mathf.FloorToInt(elapsed);
            bool isOvertime = _run.IsOvertime;
            if (totalSeconds != _lastSeconds || isOvertime != _lastOvertime)
            {
                _lastSeconds = totalSeconds;
                _lastOvertime = isOvertime;
                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;
                string timeString = $"{minutes:00}:{seconds:00}";
                if (isOvertime)
                {
                    timeString += " <color=#FFA500>[BÙ GIỜ]</color>";
                }
                _hudView.SetMatchTime(timeString);
            }

            // 2. Tiến trình Event - Chỉ cập nhật khi số đợt quái thay đổi
            int processedEvents = _run.ProcessedEventsCount;
            int totalEvents = _run.TotalEventsCount;
            if (processedEvents != _lastProcessedEvents || totalEvents != _lastTotalEvents)
            {
                _lastProcessedEvents = processedEvents;
                _lastTotalEvents = totalEvents;
                _hudView.SetEventStatus($"Đợt: {processedEvents}/{totalEvents}");
            }

            // 3. Máu Căn Cứ Nhà Chính - Chỉ cập nhật khi máu Base thay đổi
            if (_run.Base != null)
            {
                float baseCur = _run.Base.CurrentHP;
                float baseMax = _run.Base.MaxHP;
                int baseCurInt = Mathf.CeilToInt(baseCur);
                int baseMaxInt = Mathf.CeilToInt(baseMax);
                if (baseCurInt != _lastBaseCurHp || baseMaxInt != _lastBaseMaxHp)
                {
                    _lastBaseCurHp = baseCurInt;
                    _lastBaseMaxHp = baseMaxInt;
                    _hudView.SetBaseHealth(baseCur, baseMax, $"<color=#00FF88>Căn Cứ:</color> {baseCurInt}/{baseMaxInt}");
                }
            }

            // 4. Máu Tướng & Đếm ngược Hồi sinh - Chỉ cập nhật khi máu hoặc giây hồi sinh đổi
            if (_run.HeroHealth != null)
            {
                float heroCur = _run.HeroHealth.CurrentHP;
                float heroMax = _run.HeroHealth.MaxHP;
                int heroCurInt = Mathf.CeilToInt(heroCur);
                int heroMaxInt = Mathf.CeilToInt(heroMax);
                bool isDown = _run.HeroHealth.IsDown;
                int respawnSeconds = isDown ? Mathf.CeilToInt(_run.HeroHealth.RemainingRespawnTime) : 0;
                if (heroCurInt != _lastHeroCurHp || heroMaxInt != _lastHeroMaxHp || isDown != _lastHeroIsDown || respawnSeconds != _lastHeroRespawnSeconds)
                {
                    _lastHeroCurHp = heroCurInt;
                    _lastHeroMaxHp = heroMaxInt;
                    _lastHeroIsDown = isDown;
                    _lastHeroRespawnSeconds = respawnSeconds;

                    string heroText = $"<color=#00E5FF>Tướng:</color> {heroCurInt}/{heroMaxInt}";
                    if (isDown)
                    {
                        heroText += $" <color=#FF4444>[HỒI SINH TRONG {respawnSeconds}s]</color>";
                    }
                    _hudView.SetHeroHealth(heroCur, heroMax, heroText);
                }
            }

            // 5. Gỗ - Chỉ cập nhật khi số lượng gỗ trong ví thay đổi
            int wood = _woodWallet != null ? _woodWallet.CurrentWood : 0;
            if (wood != _lastWood)
            {
                _lastWood = wood;
                _hudView.SetWoodCount($"<color=#FFD700>Gỗ:</color> {wood}");
            }

            // 6. EXP & Cấp độ - Chỉ cập nhật khi EXP hoặc Level thay đổi
            if (_experienceCore != null)
            {
                int curExp = _experienceCore.CurrentExp;
                int level = _experienceCore.CurrentLevel;
                int reqExp = DefenseExperienceCore.GetExpRequiredForLevel(level + _experienceCore.PendingLevelUps);
                if (curExp != _lastExp || reqExp != _lastReqExp || level != _lastLevel)
                {
                    _lastExp = curExp;
                    _lastReqExp = reqExp;
                    _lastLevel = level;
                    _hudView.SetExperience(curExp, reqExp, $"<color=#FFCC00>Cấp {level}</color> ({curExp}/{reqExp})");
                }
            }

            // 7. Boss Dạ Xoa - Chỉ cập nhật khi trạng thái hoặc máu Boss thay đổi
            bool showBoss = _run.BossSpawned && _run.Yaksha != null && !_run.Yaksha.IsDead;
            if (showBoss)
            {
                float bossCur = _run.Yaksha.CurrentHP;
                float bossMax = _run.Yaksha.MaxHP;
                int bossCurInt = Mathf.CeilToInt(bossCur);
                int bossMaxInt = Mathf.CeilToInt(bossMax);
                string bossPhase = _run.AssaultStarted ? "TẤN CÔNG NHÀ CHÍNH" : "ĐẤU TRƯỜNG DẠ XOA";
                if (!showBoss.Equals(_lastShowBoss) || bossCurInt != _lastBossCurHp || bossMaxInt != _lastBossMaxHp || bossPhase != _lastBossPhase)
                {
                    _lastShowBoss = true;
                    _lastBossCurHp = bossCurInt;
                    _lastBossMaxHp = bossMaxInt;
                    _lastBossPhase = bossPhase;
                    _hudView.SetBossHealth(true, bossCur, bossMax, $"<color=#FF0055>Dạ Xoa [{bossPhase}]:</color> {bossCurInt}/{bossMaxInt}");
                }
            }
            else if (_lastShowBoss)
            {
                _lastShowBoss = false;
                _lastBossCurHp = -1;
                _lastBossMaxHp = -1;
                _lastBossPhase = null;
                _hudView.SetBossHealth(false, 0, 1, string.Empty);
            }

            if (_towerBuilder != null)
            {
                int socket = _towerBuilder.NearestSocket;
                var tower = _towerBuilder.GetTower(socket);
                int level = tower != null && tower.gameObject.activeSelf ? tower.Level : 0;
                int cost = level > 0 ? tower.UpgradeCost : 0;
                int maximum = tower != null ? tower.MaxLevel : 0;
                if (socket != _lastBuildSocket || level != _lastBuildLevel || cost != _lastBuildCost || maximum != _lastBuildMax)
                {
                    _lastBuildSocket = socket; _lastBuildLevel = level; _lastBuildCost = cost; _lastBuildMax = maximum;
                    if (socket < 0) _hudView.SetBuildPrompt(false);
                    else if (level == 0) _hudView.SetBuildPrompt(true, "CHỌN TRỤ", _requestBuild);
                    else if (level < maximum) _hudView.SetBuildPrompt(true, "NÂNG CẤP LV." + (level + 1) + " (" + cost + " GỖ)", _requestUpgrade);
                    else _hudView.SetBuildPrompt(true, "TRỤ ĐÃ ĐẠT CẤP TỐI ĐA", null);
                }
            }
        }

        private void OnHeroLevelGained(int newLevel)
        {
            if (_isLevelUpModalOpen) return;
            OpenNextLevelUpChoice();
        }

        private int _availableRerolls = 2;
        private int _bonusRerolls = 0;

        public void SetBonusRerolls(int bonusRerolls)
        {
            _bonusRerolls = Mathf.Max(0, bonusRerolls);
            _availableRerolls = 2 + _bonusRerolls;
        }

        private void OpenNextLevelUpChoice()
        {
            if (_upgradeDeck == null || _upgradeModalView == null) return;
            if (!_isLevelUpModalOpen && (_sessionCommands == null || !_sessionCommands.OpenLevelUp())) return;
            _isLevelUpModalOpen = true;

            var choices = _upgradeDeck.DrawThreeChoices(UnityEngine.Random.Range(1, 100000));
            _currentDrawnCards = choices.ToArray();
            _upgradeModalView.Show(_currentDrawnCards, _availableRerolls);
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

        private void OnUpgradeRerollRequested()
        {
            if (!_isLevelUpModalOpen || _upgradeDeck == null || _upgradeModalView == null || _availableRerolls <= 0) return;
            _availableRerolls--;
            var choices = _upgradeDeck.DrawThreeChoices(UnityEngine.Random.Range(1, 100000));
            _currentDrawnCards = choices.ToArray();
            _upgradeModalView.Show(_currentDrawnCards, _availableRerolls);
        }

        private void OnUpgradeSkipRequested()
        {
            if (!_isLevelUpModalOpen || _experienceCore.PendingLevelUps <= 0) return;
            _currentDrawnCards = null;
            if (!_experienceCore.TryConsumePendingLevelUp()) return;

            if (_experienceCore.PendingLevelUps > 0)
            {
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

            int wood = _woodWallet != null ? _woodWallet.CurrentWood : 0;
            int defeated = _run != null ? _run.EnemiesDefeatedCount : 0;
            var resultData = new DefenseMatchResultData(
                isWon: isWon,
                elapsedSeconds: _run.ElapsedTime,
                woodCollected: wood,
                enemiesDefeated: defeated,
                stageId: ProjectZombie.Core.Architecture.AppCompositionRoot.Instance?.Flow.ActiveMatch?.Stage.stageId,
                runId: ProjectZombie.Core.Architecture.AppCompositionRoot.Instance?.Flow.ActiveMatch?.RunId,
                victoryReward: ProjectZombie.Core.Architecture.AppCompositionRoot.Instance?.Flow.ActiveMatch?.Stage.baseRewardCoins ?? DefenseMatchResultService.REWARD_VICTORY_CURRENCY);

            var processed = _resultService.ProcessMatchResult(resultData);
            _resultModalView.ShowResult(processed.Title, processed.Message, processed.Summary);
        }

        private void OnRestartClicked()
        {
            _resultService.Reset();
            _sessionCommands?.Restart();
        }

        /// <summary>Invalidates old selections before session entities are cleaned up.</summary>
        public void CancelRunPresentation()
        {
            _lastBuildSocket = -2;
            if (_upgradeModalView != null) _upgradeModalView.Hide();
            if (_resultModalView != null) _resultModalView.Hide();
            _currentDrawnCards = null;
            _isLevelUpModalOpen = false;
            _availableRerolls = 2 + _bonusRerolls;
        }

        /// <summary>Transitional model reset owner until progression leaves the Presenter in RF3D-025.</summary>
        public void ResetRunPresentation()
        {
            CancelRunPresentation();
            _experienceCore?.Reset();
            _resultService.Reset();
        }

        private void OnMainMenuClicked()
        {
            // Trở về Sảnh bằng hợp đồng điều hướng tường minh thay vì SceneManager.LoadScene(0)
            _ = DefenseNavigationService.Default.ReturnToHubAsync(() => CancelRunPresentation());
        }
    }
}
