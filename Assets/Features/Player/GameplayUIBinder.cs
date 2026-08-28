using System;
using UnityEngine;
using ProjectZombie.Features.UI;
using ProjectZombie.Features.UI.HUD;
using ProjectZombie.Features.UI.StatsAndSkills;

namespace ProjectZombie.Features.Player
{
    /// <summary>
    /// Chuyên trách phân phối và inject dependencies từ PlayerContext vào tất cả các Presenter UI.
    /// Giúp GameplayBootstrapper tuân thủ nguyên tắc SRP (Single Responsibility Principle).
    /// </summary>
    [Serializable]
    public class GameplayUIBinder
    {
        [Header("UI Managers")]
        [SerializeField] private GameplayUIManager _gameplayUIManager;

        [Header("UI Presenters")]
        [SerializeField] private RunHUDPresenter _runHUDPresenter;
        [SerializeField] private PlayerInfoUIPresenter _playerInfoUIPresenter;
        [SerializeField] private UpgradeUIPresenter _upgradeUIPresenter;
        [SerializeField] private GameOverScreenPresenter _gameOverScreenPresenter;
        [SerializeField] private CharacterGaugeWidgetPresenter _characterGaugeWidgetPresenter;
        [SerializeField] private SignatureSkillPresenter _signatureSkillPresenter;
        [SerializeField] private AttackButtonPresenter _attackButtonPresenter;
        [SerializeField] private DashButtonPresenter _dashButtonPresenter;

        public GameplayUIBinder() { }

        public GameplayUIBinder(
            RunHUDPresenter runHUD,
            PlayerInfoUIPresenter playerInfo,
            UpgradeUIPresenter upgradeUI,
            GameOverScreenPresenter gameOver,
            CharacterGaugeWidgetPresenter gaugeWidget)
        {
            _runHUDPresenter = runHUD;
            _playerInfoUIPresenter = playerInfo;
            _upgradeUIPresenter = upgradeUI;
            _gameOverScreenPresenter = gameOver;
            _characterGaugeWidgetPresenter = gaugeWidget;
        }

        public void SetGameplayUIManager(GameplayUIManager manager)
        {
            _gameplayUIManager = manager;
        }

        private void EnsureReferences()
        {
            if (_runHUDPresenter == null) _runHUDPresenter = UnityEngine.Object.FindObjectOfType<RunHUDPresenter>(true);
            if (_playerInfoUIPresenter == null) _playerInfoUIPresenter = UnityEngine.Object.FindObjectOfType<PlayerInfoUIPresenter>(true);
            if (_upgradeUIPresenter == null) _upgradeUIPresenter = UnityEngine.Object.FindObjectOfType<UpgradeUIPresenter>(true);
            if (_gameOverScreenPresenter == null) _gameOverScreenPresenter = UnityEngine.Object.FindObjectOfType<GameOverScreenPresenter>(true);
            if (_characterGaugeWidgetPresenter == null) _characterGaugeWidgetPresenter = UnityEngine.Object.FindObjectOfType<CharacterGaugeWidgetPresenter>(true);
            if (_signatureSkillPresenter == null) _signatureSkillPresenter = UnityEngine.Object.FindObjectOfType<SignatureSkillPresenter>(true);
            if (_attackButtonPresenter == null) _attackButtonPresenter = UnityEngine.Object.FindObjectOfType<AttackButtonPresenter>(true);
            if (_dashButtonPresenter == null) _dashButtonPresenter = UnityEngine.Object.FindObjectOfType<DashButtonPresenter>(true);
        }

        public void BindAll(PlayerContext context)
        {
            if (context == null || context.GameObject == null)
            {
                Debug.LogWarning("[GameplayUIBinder] Không thể Bind UI vì PlayerContext null.");
                return;
            }

            EnsureReferences();

            BindRunHUD(context);
            BindPlayerInfo(context);
            BindUpgradeUI(context);
            BindGameOverScreen(context);
            BindCharacterGauge(context);
            BindSignatureSkill(context);
            BindAttackButton(context);
            BindDashButton(context);
        }

        private void BindRunHUD(PlayerContext context)
        {
            if (_runHUDPresenter != null)
            {
                _runHUDPresenter.Construct(
                    context.Health,
                    context.Stats,
                    context.Experience,
                    context.WeaponManager,
                    context.Passives);
                Debug.Log("[GameplayUIBinder] Đã inject dependencies vào RunHUDPresenter.");
            }
        }

        private void BindPlayerInfo(PlayerContext context)
        {
            if (_playerInfoUIPresenter != null)
            {
                _playerInfoUIPresenter.Construct(
                    context.Stats,
                    context.Health,
                    context.Experience,
                    context.WeaponManager,
                    context.Passives);
                Debug.Log("[GameplayUIBinder] Đã inject dependencies vào PlayerInfoUIPresenter.");
            }
        }

        private void BindUpgradeUI(PlayerContext context)
        {
            if (_upgradeUIPresenter != null)
            {
                _upgradeUIPresenter.Construct(context.Experience, context.WeaponManager);
                Debug.Log("[GameplayUIBinder] Đã inject dependencies vào UpgradeUIPresenter.");
            }
        }

        private void BindGameOverScreen(PlayerContext context)
        {
            if (_gameOverScreenPresenter != null)
            {
                _gameOverScreenPresenter.Construct(context.Health);
                Debug.Log("[GameplayUIBinder] Đã inject dependencies vào GameOverScreenPresenter.");
            }
        }

        private void BindCharacterGauge(PlayerContext context)
        {
            if (_characterGaugeWidgetPresenter != null)
            {
                if (context.GaugeProvider != null)
                {
                    _characterGaugeWidgetPresenter.Bind(context.GaugeProvider);
                    Debug.Log($"[GameplayUIBinder] CharacterGaugeWidgetPresenter đã Bind provider: {context.GaugeProvider.GetType().Name}");
                }
                else
                {
                    _characterGaugeWidgetPresenter.Unbind();
                }
            }
        }

        private void BindSignatureSkill(PlayerContext context)
        {
            if (_signatureSkillPresenter != null)
            {
                if (context.SignatureSkillManager != null)
                {
                    _signatureSkillPresenter.Bind(context.SignatureSkillManager);
                    Debug.Log("[GameplayUIBinder] SignatureSkillPresenter đã Bind SignatureSkillManager.");
                }
            }
        }

        private void BindAttackButton(PlayerContext context)
        {
            if (_attackButtonPresenter != null)
            {
                if (context.Combat != null) _attackButtonPresenter.Bind(context.Combat);
                if (context.WeaponManager != null) _attackButtonPresenter.Bind(context.WeaponManager);
                Debug.Log("[GameplayUIBinder] AttackButtonPresenter đã Bind Combat & WeaponManager.");
            }
        }

        private void BindDashButton(PlayerContext context)
        {
            if (_dashButtonPresenter != null && context.Controller != null && context.Stats != null)
            {
                _dashButtonPresenter.Bind(context.Controller, context.Stats);
                Debug.Log("[GameplayUIBinder] DashButtonPresenter đã Bind PlayerController & PlayerStats.");
            }
        }
    }
}
